using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace GenericRestConnector
{
    public class Authentication
    {
        AuthInfo authInfo;
        dynamic authOptions;
        dynamic Auth;
        public Authentication(String authMethod, AuthInfo info, dynamic options)
        {
            authInfo = info;
            authOptions = options;
            switch (authMethod)
            {
                case "Basic":
                    Auth = new BasicAuth();
                    break;
                case "API Key":
                    Auth = new APIKeyAuth();
                    break;
                case "OAuth":
                    Auth = new OAuthAuth();
                    break;
                default:
                    Auth = new NoAuth();
                    break;
            }
        }

        public WebClient PrepClient(WebClient client)
        {
            return Auth.PrepClient(client, authInfo, authOptions);
        }
        public String PrepUrl(String url)
        {
            return Auth.PrepUrl(url, authInfo, authOptions);
        }
    }
}
