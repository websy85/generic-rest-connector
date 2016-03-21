using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Configuration;
using Newtonsoft.Json;
using QlikView.Qvx.QvxLibrary;

namespace GenericRestConnector
{
    public class RESTHelper
    {
        WebClient client;
        private dynamic ActiveResults;      //Dynamic object to store the active result set in so we don't need to pass it between voids
        public dynamic Dictionary { get; set; } //Json config definition
        public dynamic ActiveTable { get; set; }   //Json definition of the table currently being loaded         
        public Dictionary<String, dynamic> ActiveFields { get; set; }   //Reorganised Json definition of the fields for the active table       
        public Boolean IsMore { get; set; }     //Boolean that identifies if there is more data to be loaded
        public String DataElement { get; set; }     //Element that identifies where the data is accessed
        public String ActiveUrl { get; set; }   //Current Url used for loading Json
        public String UrlBase;

        public AuthInfo authInfo = new AuthInfo();
        public Authentication authentication;

        public PageInfo pageInfo = new PageInfo();
        public Pager pager;
        
        public RESTHelper(Dictionary<String, String> MParameters)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "Setting up REST Helper");
            client = new WebClient();

            String UserName;
            String Password;
            String dictionaryUrl;
            String Token;
            String consumerSecret;
            String source;
            String dictionaryId;

            //IsMore should start as false and will be re evaluated in the GetJson routine
            IsMore = true;

            //Use MParameters to set extra parameters
            MParameters.TryGetValue("username", out UserName);
            MParameters.TryGetValue("password", out Password);
            MParameters.TryGetValue("token", out Token);
            MParameters.TryGetValue("consumer_secret", out consumerSecret);
            MParameters.TryGetValue("url", out UrlBase);
            MParameters.TryGetValue("dictionaryurl", out dictionaryUrl);
            MParameters.TryGetValue("source", out source);
            MParameters.TryGetValue("dictionary", out dictionaryId);

            authInfo.User = UserName;
            authInfo.Password = Password;
            authInfo.oauth2Token = Password;
            authInfo.APIKey = Password;
            authInfo.ConsumerSecret = consumerSecret;
            authInfo.oauth1Token = Token;
            authInfo.oauth1Secret = Password;

            pageInfo.CurrentRecord = 0;
            pageInfo.CurrentPage = 1;
            pageInfo.LoadLimit = 1000000;
            
            if (String.IsNullOrEmpty(dictionaryUrl))
            {
                Dictionary = null;
            }
            Debugger.Launch();
            String dictionary;
            if (source == "local")
            {
                String configDirectory = @"C:\Program Files\Common Files\Qlik\Custom Data\GenericRestConnector\configs\";
                dictionary = new StreamReader(configDirectory + dictionaryId + "\\dictionary.json").ReadToEnd();
            }
            else
            {
                //During an online reload the Dictionary JSON is loaded from GitHub. This helps to reduce traffic to the Heroku Dyno 
                WebClient gitClient = new WebClient();
                gitClient = AddHeaders(gitClient);
                dictionary = gitClient.DownloadString(dictionaryUrl);
            }
            
            
            if (!String.IsNullOrEmpty(dictionary))
            {
                
                Dictionary = JsonConvert.DeserializeObject(dictionary);
                if (source == "online")
                {
                    Dictionary = Dictionary.content.ToString();
                    Dictionary = Convert.FromBase64String(Dictionary);
                    Dictionary = Encoding.UTF8.GetString(Dictionary);
                    Dictionary = JsonConvert.DeserializeObject(Dictionary);
                }
                String authMethod = Dictionary.auth_method.ToString();
                if (authMethod == "OAuth" && Dictionary.auth_options.auth_version.ToString() == "1.0")
                {
                    authMethod = "OAuth1.0";
                }
                authentication = new Authentication(authMethod, authInfo, Dictionary.auth_options);
                pager = new Pager(Dictionary.paging_method.ToString(), Dictionary.paging_options);
            }
        }

        public void SetActiveTable(String tableName)
        {
            foreach (dynamic t in Dictionary.tables)
            {
                if (t.qName == tableName)
                {
                    ActiveTable = t;
                    ActiveUrl = String.Concat(UrlBase, "/", Dictionary.base_endpoint, "/", t.endpoint);
                    //Get the name of the data element or use the override if specified
                    DataElement = Dictionary.data_element;
                    if (ActiveTable.data_element_override != null && !String.IsNullOrEmpty(ActiveTable.data_element_override.ToString()))
                    {
                        DataElement = ActiveTable.data_element_override;
                    }
                    //Check to see if we need to set a load limit
                    if (ActiveTable.load_limit != null && !String.IsNullOrEmpty(ActiveTable.load_limit.ToString()))
                    {
                        pageInfo.LoadLimit = Convert.ToInt64(ActiveTable.load_limit);
                    }
                    //create a new object to store the fields in a way that makes them easier to access

                    ActiveFields = new Dictionary<string, dynamic>();
                    foreach (dynamic field in ActiveTable.fields)
                    {
                        ActiveFields[field.qName.ToString()] = field;
                    }
                    break;
                }
            }
        }

        public void Prep()
        {
            IsMore = true;
            pageInfo.CurrentRecord = 0;
            pageInfo.CurrentPageSize = null;
            //add the headers to the web client
            client = AddHeaders(client);
            //build the initial url
            ActiveUrl = pager.PrepUrl(UrlBase, Dictionary.base_endpoint.ToString(), ActiveTable.endpoint.ToString(), pageInfo);
            //add any authentication url stuff
            ActiveUrl = authentication.PrepUrl(ActiveUrl);
            //prep the WebClient with any authentication steps. we perform this last in case we're authenticating with oAuth 1.0a
            authInfo.Url = ActiveUrl;
            client = authentication.PrepClient(client, authInfo);
        }

        public dynamic GetJSON()
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "Getting JSON");
            IsMore = false;
            String json = "";
            try
            {
                json = client.DownloadString(ActiveUrl);
                ActiveResults = JsonConvert.DeserializeObject(json);
                //PageActiveUrl();
                return ActiveResults;
            }           
            catch (WebException ex)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "Downloading Web Response - " + ex.Status + ": " + ex.Message);
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "Url was - " + ActiveUrl);
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "Json was - " + json);
            }
            catch (Exception ex)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "Converting Web Response - " +  ex.Message);
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "Url was - " + ActiveUrl);
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "Json was - " + json);
            }
            return null;
        }

        public void Page()
        {
            //at present supplied url paging doesn't fit into the 'generic' paging model
            if (Dictionary.paging_method.ToString() == "Supplied URL")
            {
                ActiveUrl = "";
                if (Dictionary.paging_options.supplied_url_element!=null)
                {
                    dynamic temp = ActiveResults;
                    String suppliedUrlPath = Dictionary.paging_options.supplied_url_element.ToString();
                    if (!String.IsNullOrEmpty(suppliedUrlPath))
                    {
                        List<String> pagingElemPath = suppliedUrlPath.Split(new String[] { "." }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        foreach (String elem in pagingElemPath)
                        {
                            temp = temp[elem];
                        }
                        ActiveUrl = temp;
                    }
                }
            }
            else
            {
                ActiveUrl = pager.PrepUrl(UrlBase, Dictionary.base_endpoint.ToString(), ActiveTable.endpoint.ToString(), pageInfo);
                //add any authentication url stuff
                ActiveUrl = authentication.PrepUrl(ActiveUrl);
            }
            if(String.IsNullOrEmpty(ActiveUrl))
            {
                IsMore = false;
            }
            else
            {
                IsMore = true;
            }
        }

        public QvxField[] createFieldList(String tableName, String fields)
        {
            List<QvxField> fList = new List<QvxField>();
            dynamic table = getTableByName(tableName);
            foreach (dynamic f in table.fields)
            {
                if (fields.IndexOf(f.qName.ToString()) != -1)
                {
                    //Need to add functionality for converting types
                    fList.Add(new QvxField(f.qName.ToString(), getFieldType(f.type.ToString()), QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, getFieldAttr(f.type.ToString())));
                }
            }
            return fList.ToArray<QvxField>();
        }

        //Method for finding a table by Name
        private dynamic getTableByName(String name)
        {
            foreach (dynamic t in Dictionary.tables)
            {
                if (t.qName == name)
                {
                    return t;
                }
            }
            return null;
        }

        //Method for getting the field type
        private QvxFieldType getFieldType(String fieldType)
        {
            switch (fieldType)
            {
                case "String":
                case "Boolean":
                    return QvxFieldType.QVX_TEXT;
                case "Integer":
                    return QvxFieldType.QVX_SIGNED_INTEGER;
                case "Real":
                    return QvxFieldType.QVX_IEEE_REAL;
                default:
                    return QvxFieldType.QVX_TEXT;
            }
        }

        //Method for getting the field type
        private FieldAttrType getFieldAttr(String fieldType)
        {
            switch (fieldType)
            {
                case "String":
                    return FieldAttrType.ASCII;
                case "Boolean":
                case "Integer":
                    return FieldAttrType.INTEGER;
                case "Real":
                    return FieldAttrType.REAL;
                default:
                    return FieldAttrType.ASCII;
            }
        }

        private WebClient AddHeaders(WebClient wc)
        {
            //wc.Headers[HttpRequestHeader.ContentType] = "application/json";
            wc.Headers[HttpRequestHeader.Accept] = "application/json";
            wc.Headers[HttpRequestHeader.UserAgent] = "generic-rest-connector";
            return wc;
        }
    }
}
