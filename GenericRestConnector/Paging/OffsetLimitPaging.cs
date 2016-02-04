using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRestConnector
{
    public class OffsetLimitPaging:PagingBase
    {
        public override string PrepUrl(String url, dynamic options, PageInfo pageInfo)
        {
            if (options.offset_url_parameter == null || options.offset_url_parameter == "")
            {
                url += String.Format("/{0}/{1}", pageInfo.CurrentRecord, options.batch_size);
            }
            else
            {
                if (url.Contains("?"))
                {
                    url += "&";
                }
                else
                {
                    url += "?";
                }
                url += String.Format("{0}={1}&{2}={3}", options.offset_url_parameter, pageInfo.CurrentRecord, options.limit_url_parameter, options.batch_size);
            }
            return url;
        }
    }
}
