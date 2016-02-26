using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace GenericRestConnector
{
    public class OAuthAuth: AuthBase
    {
        public override System.Net.WebClient PrepClient(System.Net.WebClient client, AuthInfo info, dynamic options)
        {
            client.Headers[HttpRequestHeader.Authorization] = string.Format("Bearer {0}", info.oauth2Token);
            return client;
        }

        public override string PrepUrl(string url, AuthInfo info, dynamic options)
        {
            return url;
        }
    }
}
