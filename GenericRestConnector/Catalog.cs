using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;

namespace GenericRestConnector
{
    public static class Catalog
    {
        public static dynamic _catalog;
        static string _configDirectory = @"C:\Program Files\Common Files\Qlik\Custom Data\GenericRestConnector\configs\";

        public static void Open()
        {
            try
            {
                using (StreamReader sr = new StreamReader(_configDirectory + "catalog.json"))
                {
                    _catalog = JsonConvert.DeserializeObject(sr.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = new StreamWriter(_configDirectory + "catalog.json"))
                {
                    sw.WriteLine("{\"configs\":{}}");
                    _catalog = JsonConvert.DeserializeObject("{\"configs\":{}}");
                }
            }
        }

        public static void Update()
        {
            _catalog = JsonConvert.DeserializeObject("{\"configs\":{}}");
            try
            {
                String[] dirs = Directory.GetDirectories(_configDirectory);
                foreach (String dir in dirs)
                {
                    String dirName = dir.Split(new string[] {"\\"}, StringSplitOptions.RemoveEmptyEntries).Last<String>();
                    dynamic dictionary = JsonConvert.DeserializeObject(new StreamReader(dir+"\\dictionary.json").ReadToEnd());
                    if(_catalog.configs[dictionary.name.ToString()]==null){
                        AddEntry(dictionary.display_name.ToString(), dirName, dictionary.name.ToString());
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void AddEntry(String displayName, String name, String id)
        {
            _catalog.configs[name] = "{\"displayName\":\""+displayName+"\",\"folder\":\"" + name + "\", \"id\":\"" + id + "\"}";
            using (StreamWriter sw = new StreamWriter(_configDirectory + "catalog.json"))
            {
                using (JsonTextWriter jw = new JsonTextWriter(sw))
                {
                    jw.WriteRawValue(JsonConvert.SerializeObject(_catalog));
                }
            }
        }
    }
}
