using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using QlikView.Qvx.QvxLibrary;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Dynamic;
using System.Net;
using System.Configuration;

namespace GenericRestConnector
{
    class Server : QvxServer
    {
        dynamic currentDictionary;
        ErrorHelper errorHelper = new ErrorHelper();
        public override QvxConnection CreateConnection()
        {
            return new Connection();
        }

        public override string CreateConnectionString()
        {
            return "";
        }

        public QvDataContractResponse getOnlineDictionaries()
        {
            //Debugger.Launch();
            WebClient client = new WebClient();
            client.Headers.Add("Accept", "application/json");

            String publicDictionariesUrl = ConfigurationManager.AppSettings["PublicDictionaryHost"];
            publicDictionariesUrl += "/api/public";
            if (String.IsNullOrEmpty(publicDictionariesUrl))
            {
                return new Info
                {
                    qMessage = errorHelper.NoConfigUrl
                };
            }
            String configs = client.DownloadString(publicDictionariesUrl);
            if (!String.IsNullOrEmpty(configs))
            {
                return new Info
                {
                    qMessage = configs
                };
            }
            else
            {
                return new Info
                {
                    qMessage = errorHelper.ErrorDownloadingData
                };
            }
            
        }

        public QvDataContractResponse getDictionaryAuth(String id, QvxConnection connection)
        {
            //Debugger.Launch();

            String dictionary = getDictionary(id, connection);
            if (!String.IsNullOrEmpty(dictionary))
            {
                dynamic dic = JsonConvert.DeserializeObject(dictionary);
                return new Info
                {
                    qMessage = dic.auth_method
                };
            }
            else
            {
                return new Info
                {
                    qMessage = errorHelper.ErrorDownloadingData
                };
            }
        }

        public QvDataContractDatabaseListResponse getDatabases()
        {
            return new QvDataContractDatabaseListResponse
            {
                qDatabases = new Database[] 
                { 
                    new Database {qName = "Not Applicable"}
                }
            };
        }

        public string getDictionary(string id, QvxConnection connection)
        {
            var d = "";
            connection.MParameters.TryGetValue("fulldictionary", out d);
            if (String.IsNullOrEmpty(d))
            {


                WebClient client = new WebClient();
                client.Headers.Add("Accept", "application/json");

                String dictionaryUrl = ConfigurationManager.AppSettings["PublicDictionaryHost"];
                if (String.IsNullOrEmpty(dictionaryUrl))
                {
                    return null;
                }
                dictionaryUrl += "/api/public/dictionary/";
                dictionaryUrl += id;
                String dictionary = client.DownloadString(dictionaryUrl);
                if (!String.IsNullOrEmpty(dictionary))
                {
                    connection.MParameters["fulldictionary"] = dictionary;
                    return dictionary;
                }
            }
            return d;           
        }

        public QvDataContractResponse getTables()
        {
            return new Info
            {
                qMessage = JsonConvert.SerializeObject(currentDictionary.tables)
            };
        }

        public QvDataContractResponse getFields(String tableName)
        {
            return new Info
            {
                qMessage = JsonConvert.SerializeObject(getFieldsForTable(tableName))
            };
        }

        public QvDataContractResponse getPreview(String tableName)
        {
            //need to modify this to return an actual preview
            string[] fields = getFieldNamesForTable(tableName);
            List<dynamic> preview = new List<dynamic>();
            dynamic values = new System.Dynamic.ExpandoObject();
            values.qValues = fields;
            preview.Add(values);
            return new Info
            {
                qMessage = JsonConvert.SerializeObject(preview.ToArray())
            };
        }

        public string[] getFieldNamesForTable(String tableName)
        {
            List<String> fields = new List<string>();
            foreach (dynamic t in currentDictionary.tables)
            {
                if (t.qName == tableName)
                {
                    foreach (dynamic f in t.fields)
                    {
                        fields.Add(f.qName.ToString());
                    }
                }
            }
            return fields.ToArray();
        }

        public dynamic getFieldsForTable(String tableName)
        {
            foreach (dynamic t in currentDictionary.tables)
            {
                if (t.qName == tableName)
                {
                    return t.fields;
                }
            }
            return null;
        }

        public QvDataContractResponse getOAuthAuthorizationUrl(String key)
        {
            String redirectUrl = ConfigurationManager.AppSettings["OAuthRedirectUri"];
            String authorizeUrl = currentDictionary.auth_options.oauth_authorize_url.ToString();
            return new Info
            {
                qMessage = String.Format("{0}?client_id={1}&redirect_uri={2}", authorizeUrl , key, redirectUrl)
            };
        }

        public override string HandleJsonRequest(string method, string[] userParameters, QvxConnection connection)
        {
            //Debugger.Launch();
            QvDataContractResponse response;
            string provider, url, username, password, dictionary, tempdic;
            connection.MParameters.TryGetValue("provider", out provider);
            connection.MParameters.TryGetValue("userid", out username);
            connection.MParameters.TryGetValue("password", out password);
            connection.MParameters.TryGetValue("url", out url);
            connection.MParameters.TryGetValue("dictionary", out dictionary);
            connection.MParameters.TryGetValue("fulldictionary", out tempdic);

            if (currentDictionary == null && String.IsNullOrEmpty(tempdic) && dictionary != null)
            {
                currentDictionary = JsonConvert.DeserializeObject(getDictionary(dictionary, connection));
            }
            else if(currentDictionary == null && !String.IsNullOrEmpty(tempdic))
            {
                currentDictionary = JsonConvert.DeserializeObject(tempdic);
            }

            switch (method)
            {
                case "getOnlineDictionaries":
                    response = getOnlineDictionaries();
                    break;
                case "getDictionaryAuth":
                    response = getDictionaryAuth(userParameters[0], connection);
                    break;
                case "getDatabases":
                    response = getDatabases();
                    break;
                case "getTables":
                    response = getTables();
                    break;
                case "getFields":
                    response = getFields(userParameters[0]);
                    break;
                case "getPreview":
                    response = getPreview(userParameters[0]);
                    break;
                case "getOAuthAuthorizationUrl":
                    //we wont have a cached dictionary at this poiny so we need to set it
                    currentDictionary = JsonConvert.DeserializeObject(getDictionary(userParameters[1], connection));
                    response = getOAuthAuthorizationUrl(userParameters[0]);
                    break;
                default:
                    response = new Info { qMessage = "Unknown command" };
                    break;
            }
            return ToJson(response);    // serializes response into JSON string
        }
    }
}
