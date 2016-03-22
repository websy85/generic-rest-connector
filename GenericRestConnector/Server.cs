using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
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
            try
            {
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
            catch (Exception ex)
            {
                return new Info
                {
                    qMessage = "\"configs\":[]"
                };
            }
            
        }

        public QvDataContractResponse getLocalDictionaries()
        {
            Catalog.Open();
            return new Info
            {
                qMessage = JsonConvert.SerializeObject(Catalog._catalog)
            };
        }

        public QvDataContractResponse updateLocalCatalog()
        {
            Catalog.Update();
            return getLocalDictionaries();
        }

        public QvDataContractResponse getDictionaryDef(String id, String source, QvxConnection connection)
        {
            String dictionary = getDictionary(id, source, connection);
            
            if (!String.IsNullOrEmpty(dictionary))
            {
                dynamic dic = JsonConvert.DeserializeObject(dictionary);
                String factory_oauth_authorize_url;
                if (dic.auth_options.auth_version!=null && dic.auth_options.auth_version.ToString() == "1.0")
                {
                    factory_oauth_authorize_url = String.Concat(ConfigurationManager.AppSettings["PublicDictionaryHost"], "/api/oauth1_authorize");
                }
                else
                {
                    factory_oauth_authorize_url = String.Concat(ConfigurationManager.AppSettings["PublicDictionaryHost"], "/api/oauth2_authorize");
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

        public QvDataContractResponse getDatabases(String id, String source, QvxConnection connection)
        {
            currentDictionary = getDictionary(id, source, connection);
            String response = "{\"qDatabases\":[{\"qName\": \"Not Applicable\"}]";
            response += ", \"dictionary\":" + currentDictionary + "}";
            return new Info
            {
                qMessage = response
            };
            
        }

        public string getDictionary(string id, String source, QvxConnection connection)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "Getting dictionary");
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, DateTime.Now.ToLongTimeString());
            string d = "";
            if(source=="online"){

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
            else
            {
                string configDirectory = @"C:\Program Files\Common Files\Qlik\Custom Data\GenericRestConnector\configs\";
                return new StreamReader(configDirectory + id + "\\dictionary.json").ReadToEnd();
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

        public QvDataContractResponse copyDictionaryToLocal(String owner, String name, String displayName)
        {
            //Debugger.Launch();
            Catalog.Open();
            string dictionaryHost = ConfigurationManager.AppSettings["PublicDictionaryHost"];
            string repo = String.Format("http://github.com/{0}/{1}", owner, name);
            string zipurl = String.Format("{0}/archive/master.zip", repo);
            string configDirectory = @"C:\Program Files\Common Files\Qlik\Custom Data\GenericRestConnector\configs\";
            WebRequest wr = WebRequest.Create(zipurl) as HttpWebRequest;
            wr.Method = "GET";
            String tempPath = Path.GetTempPath();
            String tempZip = String.Format("{0}{1}.zip", tempPath, name);
            try
            {
                using (var responseStream = wr.GetResponse().GetResponseStream())
                {                   
                    using (FileStream fs = new FileStream(tempZip, FileMode.Create, FileAccess.Write))
                    {
                        responseStream.CopyTo(fs);                       
                    }
                }
                if (!Directory.Exists(configDirectory + name + "-master"))
                {
                    ZipFile.ExtractToDirectory(tempZip, configDirectory);
                }
                Catalog.AddEntry(displayName, name, repo);    
               
                return new Info
                {
                    qMessage = "\"status\":1"
                };

                //
            }

            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return new Info
                {
                    qMessage = "\"err\":\""+ex.Message+"\""
                };
            }
            

            
            
        }

        public override string HandleJsonRequest(string method, string[] userParameters, QvxConnection connection)
        {
            connection = (Connection)connection;
            //Debugger.Launch();
            QvDataContractResponse response;
            string provider, url, username, password, dictionary, source;
            connection.MParameters.TryGetValue("provider", out provider);
            connection.MParameters.TryGetValue("userid", out username);
            connection.MParameters.TryGetValue("password", out password);
            connection.MParameters.TryGetValue("url", out url);
            connection.MParameters.TryGetValue("dictionary", out dictionary);
            connection.MParameters.TryGetValue("source", out source);

            switch (method)
            {
                case "getLocalDictionaries":
                    response = getLocalDictionaries();
                    break;
                case "updateLocalCatalog":
                    response = updateLocalCatalog();
                    break;
                case "getOnlineDictionaries":
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "getting dictionaries");
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, DateTime.Now.ToLongTimeString());
                    response = getOnlineDictionaries();
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "got dictionaries");
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, DateTime.Now.ToLongTimeString());
                    break;
                case "getDictionaryDef":
                    response = getDictionaryDef(userParameters[0], userParameters[1], connection);
                    break;
                case "getDatabases":
                    response = getDatabases(dictionary, source, connection);
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
                case "copyDictionaryToLocal":
                    response = copyDictionaryToLocal(userParameters[0], userParameters[1], userParameters[2]);
                    break;
                default:
                    response = new Info { qMessage = "Unknown command" };
                    break;
            }
            return ToJson(response);    // serializes response into JSON string
        }
    }
}
