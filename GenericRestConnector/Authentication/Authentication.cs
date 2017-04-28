using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using QlikView.Qvx.QvxLibrary;
using System.IO;
using Newtonsoft.Json;

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
        public String GetAccessTokenFromRefreshToken(String tokenEndPoint, AuthInfo info)
        {
            //string url = string.Format(@"{0}?client_id={1}&client_secret={2}&refresh_token={3}&grant_type=refresh_token", tokenEndPoint, info.User, info.oauth2Secret, info.oauth2Token);
            WebRequest client = System.Net.WebRequest.Create(tokenEndPoint) as HttpWebRequest;
            client.Method = "POST";
            client.ContentType = "application/x-www-form-urlencoded";
            client.Headers[HttpRequestHeader.Authorization] = "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(String.Format("{0}:{1}", info.User, info.oauth2Secret)));
            StreamWriter requestWriter = new StreamWriter(client.GetRequestStream());
            try
            {
                requestWriter.Write(String.Format("refresh_token={0}&grant_type=refresh_token", info.oauth2Token));
            }
            catch
            {
                throw;
            }
            finally
            {
                requestWriter.Close();
            }
            StreamReader responseReader = new StreamReader(client.GetResponse().GetResponseStream());
            TokenResponse Tokens = new TokenResponse();
            Tokens = JsonConvert.DeserializeObject<TokenResponse>(responseReader.ReadToEnd());
            return Tokens.access_token;
        }
    }
}
