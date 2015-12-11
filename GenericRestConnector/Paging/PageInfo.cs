using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRestConnector
{
    public class PageInfo
    {
        public Int64 CurrentRecord { get; set; }
        public Int32 CurrentPage { get; set; }
        public Int64 LoadLimit { get; set; }
    }
}
