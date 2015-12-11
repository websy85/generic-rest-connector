using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRestConnector
{
    public abstract class PagingBase
    {
        public abstract string PrepUrl(String url, dynamic options, PageInfo info);
    }
}
