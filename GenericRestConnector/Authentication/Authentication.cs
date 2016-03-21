using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using QlikView.Qvx.QvxLibrary;

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
                case "OAuth1.0":
                    Auth = new OAuth1Auth();
                    break;
                default:
                    Auth = new NoAuth();
                    break;
            }
        }

        public WebClient PrepClient(WebClient client, AuthInfo info)
        {
            if (info!=null) 
            {
                authInfo = info;
            }
            return Auth.PrepClient(client, authInfo, authOptions);
        }
        public String PrepUrl(String url)
        {
            try
            {
                if (url == null)
                {
                    return null;
                }
                return Auth.PrepUrl(url, authInfo, authOptions);
            }
            catch (Exception ex)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, ex.Message);
                return null;
            }
        }
    }
}
