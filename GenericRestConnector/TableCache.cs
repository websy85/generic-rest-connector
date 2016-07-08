using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRestConnector
{
    public class TableCache
    {
        public Int32 page {get; set;}
        public dynamic data { get; set; }

        public TableCache(Int32 pageNum, dynamic pageData)
        {
            page = pageNum;
            data = pageData;
        }
    }
}
