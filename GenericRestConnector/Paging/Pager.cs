using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace GenericRestConnector
{
    public class Pager
    {
        dynamic Page;
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
                case "Offset/Limit":
                    CanPage = true;
                    Page = new OffsetLimitPaging();
                    break;
                default:
                    CanPage = false;
                    break;
            }
        }

        public String PrepUrl(String baseUrl, dynamic baseEndpoint, String table, PageInfo pageInfo)
        {
            //we'll do some generic checks to see if we should be loading data
            //is the current record >= loadlimit
            if (pageInfo.CurrentRecord >= pageInfo.LoadLimit)
            {
                return null;
            }
            //is the current page size/length zero
            if (pageInfo.CurrentPageSize == 0)
            {
                return null;
            }
            String url;
            if (baseEndpoint!=null)
            {
                url = String.Format("{0}/{1}/{2}", baseUrl, baseEndpoint.ToString(),  table);
            }
            else
            {
                url = String.Format("{0}/{1}", baseUrl, table);
            }
            url = Page.PrepUrl(url, pagingOptions, pageInfo);
            return url;
        }
    }
}
