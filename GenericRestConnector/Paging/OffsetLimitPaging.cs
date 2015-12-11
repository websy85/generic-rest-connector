using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRestConnector
{
    public class OffsetLimitPaging:PagingBase
    {
        public override string PrepUrl(String url, dynamic options, PageInfo info)
        {
            if (options.offsetParameter == null || options.offsetParameter == "")
            {
                url += "/0/" + options.batch_size;
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
                url += String.Format("{0}={1}&{2}={3}",options.offsetParameter, info.CurrentRecord, options.limitParameter, options.batch_size);
            }
            return url;
        }
    }
}
