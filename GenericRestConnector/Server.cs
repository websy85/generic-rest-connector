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
        String session;
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

        public QvDataContractResponse getDictionaryDef(String id, QvxConnection connection)
        {
            //Debugger.Launch();
            String dictionary = getDictionary(id, connection);
            
            if (!String.IsNullOrEmpty(dictionary))
            {
                dynamic dic = JsonConvert.DeserializeObject(dictionary);
                String factory_oauth_authorize_url;
                if (dic.auth_options.auth_version!=null && dic.auth_options.auth_version.ToString() == "1.0")
                {
                    factory_oauth_authorize_url = ConfigurationManager.AppSettings["FactoryOAuth1AuthorizeUrl"];
                }
                else
                {
                    factory_oauth_authorize_url = ConfigurationManager.AppSettings["FactoryOAuthAuthorizeUrl"];
                }
                dic.factory_oauth_authorize_url = factory_oauth_authorize_url;
                return new Info
                {
                    qMessage = JsonConvert.SerializeObject(dic)
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

        public QvDataContractResponse getDatabases(String id, QvxConnection connection)
        {
            currentDictionary = getDictionary(id, connection);
            String response = "{\"qDatabases\":[{\"qName\": \"Not Applicable\"}]";
            response += ", \"dictionary\":" + currentDictionary + "}";
            return new Info
            {
                qMessage = response
            };
            
        }

        public string getDictionary(string id, QvxConnection connection)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "Getting dictionary");
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, DateTime.Now.ToLongTimeString());
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
                //    connection.MParameters["fulldictionary"] = dictionary;
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "Got dictionary");
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, DateTime.Now.ToLongTimeString());
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
            
            string connector_auth_url = String.Format("{0}/{1}",ConfigurationManager.AppSettings["PublicDictionaryHost"], "auth/oauth2_authorize");
            string authorize_url = String.Format("{0}?client_id={1}&redirect_uri={2}", authorizeUrl , key, redirectUrl);
            string response = String.Format("{0}\"connector_auth_url\":\"{1}\",\"authorize_url\":\"{2}\"{3}", "{",connector_auth_url, authorize_url,"}");
            return new Info
            {
                qMessage = response
            };
        }

        public override string HandleJsonRequest(string method, string[] userParameters, QvxConnection connection)
        {
            connection = (Connection)connection;
            //Debugger.Launch();
            QvDataContractResponse response;

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "VariableStore Session is - " + VariableStore.getSession());
            string provider, url, username, password, dictionary, tempdic;
            connection.MParameters.TryGetValue("provider", out provider);
            connection.MParameters.TryGetValue("userid", out username);
            connection.MParameters.TryGetValue("password", out password);
            connection.MParameters.TryGetValue("url", out url);
            connection.MParameters.TryGetValue("dictionary", out dictionary);

            switch (method)
            {
                case "getOnlineDictionaries":
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "getting dictionaries");
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, DateTime.Now.ToLongTimeString());
                    response = getOnlineDictionaries();
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "got dictionaries");
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, DateTime.Now.ToLongTimeString());
                    break;
                case "getDictionaryDef":
                    response = getDictionaryDef(userParameters[0], connection);
                    break;
                case "getDatabases":
                    response = getDatabases(dictionary, connection);
                    break;
                case "getTables":
                    currentDictionary = JsonConvert.DeserializeObject(userParameters[0]);
                    response = getTables();
                    break;
                case "getFields":
                    currentDictionary = JsonConvert.DeserializeObject(userParameters[1]);
                    response = getFields(userParameters[0]);
                    break;
                case "getPreview":
                    currentDictionary = JsonConvert.DeserializeObject(userParameters[1]);
                    response = getPreview(userParameters[0]);
                    break;
                case "getOAuthAuthorizationUrl":
                    currentDictionary = JsonConvert.DeserializeObject(userParameters[2]);
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
