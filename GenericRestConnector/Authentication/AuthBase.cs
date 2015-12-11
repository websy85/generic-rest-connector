using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace GenericRestConnector
{
    public abstract class AuthBase
    {
        public abstract WebClient PrepClient(WebClient client, AuthInfo info, dynamic options);
        public abstract String PrepUrl(String url, AuthInfo info, dynamic options);
    }
}
