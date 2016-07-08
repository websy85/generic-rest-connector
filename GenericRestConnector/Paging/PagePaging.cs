using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRestConnector
{
    public class PagePaging:PagingBase
    {
        public override string PrepUrl(String url, dynamic options, PageInfo pageInfo)
        {
            if (options.page_url_parameter != null && options.page_url_parameter != "")
            {
                if (url.IndexOf("?") == -1)
                {
                    url += "?";
                }
                url += String.Format("{0}={1}", options.page_url_parameter.ToString(), pageInfo.CurrentPage);
            }
            
            return url;
        }
    }
}
