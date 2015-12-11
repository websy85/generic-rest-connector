using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
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
        private dynamic ActiveTable { get; set; }   //Json definition of the table currently being loaded         
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
            client = new WebClient();

            String UserName;
            String Password;
            String dictId;

            //IsMore should start as false and will be re evaluated in the GetJson routine
            IsMore = false;

            //Use MParameters to set extra parameters
            MParameters.TryGetValue("username", out UserName);
            MParameters.TryGetValue("password", out Password);
            MParameters.TryGetValue("url", out UrlBase);
            MParameters.TryGetValue("dictionary", out dictId);

            authInfo.User = UserName;
            authInfo.Password = Password;
            authInfo.Token = Password;
            authInfo.APIKey = Password;

            pageInfo.CurrentRecord = 0;
            pageInfo.CurrentPage = 1;
            pageInfo.LoadLimit = 1000000;
            
            String dictionaryUrl = ConfigurationManager.AppSettings["PublicDictionaryHost"];
            if (String.IsNullOrEmpty(dictionaryUrl))
            {
                Dictionary = null;
            }
            dictionaryUrl += "/api/public/dictionary/";
            dictionaryUrl += dictId;
            String dictionary = client.DownloadString(dictionaryUrl);
            if (!String.IsNullOrEmpty(dictionary))
            {
                Dictionary = JsonConvert.DeserializeObject(dictionary);
                authentication = new Authentication(Dictionary.auth_method.ToString(), authInfo, Dictionary.auth_options);
                pager = new Pager(Dictionary.paging_method.ToString(), pageInfo);
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

        public dynamic GetJson()
        {
            //add the headers to the web client
            client = AddHeaders(client);
            //prep the WebClient with any authentication steps
            client = authentication.PrepClient(client);
            //build the initial url
            ActiveUrl = pager.PrepUrl(UrlBase, Dictionary.base_endpoint, ActiveTable.endpoint.ToString());
            //add any authentication url stuff
            ActiveUrl = authentication.PrepUrl(ActiveUrl);
            try
            {
                ActiveResults = JsonConvert.DeserializeObject(client.DownloadString(ActiveUrl));
                //PageActiveUrl();
                return ActiveResults;
            }
            catch (WebException ex)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "Downloading Web Response - " + ex.Status + ": " + ex.Message);
            }
            return null;
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
            wc.Headers[HttpRequestHeader.Accept] = "application/json";
            wc.Headers[HttpRequestHeader.UserAgent] = "generic-rest-connector";
            return wc;
        }
    }
}
