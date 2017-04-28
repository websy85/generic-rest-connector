using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace GenericRestConnector
{
    public class OAuthAuth: AuthBase
    {
        public override System.Net.WebClient PrepClient(System.Net.WebClient client, AuthInfo info, dynamic options)
        {
            if (options.oauth_params_in_query!=null && options.oauth_params_in_query == true)
            {

            }
            else
            {
                client.Headers[HttpRequestHeader.Authorization] = string.Format("Bearer {0}", info.oauth2Token);
            }
            return client;
        }

        public override string PrepUrl(string url, AuthInfo info, dynamic options)
        {
            if (options.oauth_params_in_query!=null && options.oauth_params_in_query == true)
            {
                if (url.IndexOf("?") == -1)
                {
                    url += "?";
                }
                else
                {
                    url += "&";
                }
                url += "access_token=";
                url += info.oauth2Token;
            }
            return url;
        }
    }
}
