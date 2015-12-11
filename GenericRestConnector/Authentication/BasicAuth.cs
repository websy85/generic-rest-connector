using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace GenericRestConnector
{
    public class BasicAuth: AuthBase
    {
        public override WebClient PrepClient(System.Net.WebClient client, AuthInfo info, dynamic options)
        {
            String credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(info.User + ":" + info.Password));
            client.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);
            return client;
        }

        public override string PrepUrl(string url, AuthInfo info, dynamic options)
        {
            return url;
        }
    }
}
