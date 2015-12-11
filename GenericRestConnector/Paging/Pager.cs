using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRestConnector
{
    public class Pager
    {
        dynamic page;
        dynamic pagingOptions;
        public Boolean CanPage { get; set; }
        public Pager(String pagingMethod, dynamic options)
        {
            pagingOptions = options;
            switch (pagingMethod)
            {
                case "Pages":
                    CanPage = true;
                    break;
                default:
                    CanPage = false;
                    break;
            }
        }

        public String PrepUrl(String baseUrl, dynamic baseEndpoint, String table)
        {
            String url;
            if (baseEndpoint!=null)
            {
                url = String.Format("{0}/{1}/{2}", baseUrl, baseEndpoint.ToString,  table);
            }
            else
            {
                url = String.Format("{0}/{1}", baseUrl, table);
            }
            
            return url;
        }
    }
}
