using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenericRestConnector
{
    public class ErrorHelper
    {
        public string NoConfigUrl = "{\"err\":\"Could not get public dictionaries. No Url specified in config file.\"}";
        public string ErrorDownloadingData = "{\"err\":\"Could not download data.\"}";
    }
}
