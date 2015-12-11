using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRestConnector
{
    public class NoAuth: AuthBase
    {
        public override System.Net.WebClient PrepClient(System.Net.WebClient client, AuthInfo info, dynamic options)
        {
            return client;
        }

        public override string PrepUrl(string url, AuthInfo info, dynamic options)
        {
            return url;
        }
    }
}
