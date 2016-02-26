using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Security.Cryptography;

namespace GenericRestConnector
{
    public class OAuth1Auth: AuthBase
    {
        public override System.Net.WebClient PrepClient(System.Net.WebClient client, AuthInfo info, dynamic options)
        {
            String nonce = getNonce();
            String timestamp = getTimestamp();
            
            Dictionary<string, string> oAuthParams = new Dictionary<string, string> { 
                { "oauth_consumer_key", info.User },
                { "oauth_consumer_secret", info.ConsumerSecret },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", timestamp },
                { "oauth_nonce", nonce },
                { "oauth_version", "1.0"},
                { "oauth_token", info.oauth1Token},
                { "oauth_token_secret", info.Password}

            };
            Uri uri = new Uri(info.Url);
            String signingKey = String.Concat(Uri.EscapeDataString(info.ConsumerSecret), "&", Uri.EscapeDataString(info.Password));
            String signature = generateSignature("GET", uri, oAuthParams, signingKey);
            oAuthParams.Add("oauth_signature", signature);
            //String oauthHeader = String.Format(
            //    "OAuth oauth_consumer_key=\"{0}\"," +
            //    "oauth_nonce=\"{1}\"," +
            //    "oauth_signature_method=\"HMAC-SHA1\"," +
            //    "oauth_timestamp=\"{2}\"," + 
            //    "oauth_token=\"{3}\"," + 
            //    "oauth_version=\"1.0\"," + 
            //    "oauth_signature=\"{4}\"",
            //     Uri.EscapeDataString(info.User),
            //     Uri.EscapeDataString(nonce),
            //     Uri.EscapeDataString(timestamp),
            //     Uri.EscapeDataString(info.oauth1Token),
            //     Uri.EscapeDataString(signature));
            String oauthHeader = "OAuth ";
            foreach (KeyValuePair<string, string> d in oAuthParams)
            {
                oauthHeader += String.Format("{0}=\"{1}\",",  Uri.EscapeDataString(d.Key),  Uri.EscapeDataString(d.Value));
            }
            oauthHeader = oauthHeader.Substring(0, oauthHeader.Length - 1);
            client.Headers.Add(HttpRequestHeader.Authorization, oauthHeader);
            return client;
        }

        public override string PrepUrl(string url, AuthInfo info, dynamic options)
        {
            return url;
        }

        private static string generateSignature(
          string httpMethod,
          Uri url,
          IDictionary<string, string> oauthParams,
          string secret
        )
        {
            // Ensure the HTTP Method is upper-cased
            httpMethod = httpMethod.ToUpper();

            // Construct the URL-encoded OAuth parameter portion of the signature base string
            string encodedParams = normalizeParams(httpMethod, url, oauthParams);

            // URL-encode the relative URL
            string encodedUri = Uri.EscapeDataString(url.OriginalString.Replace(url.Query,""));

            // Build the signature base string to be signed with the Consumer Secret
            string baseString = String.Format("{0}&{1}&{2}", httpMethod, encodedUri, encodedParams);

            return generateHmac(secret, baseString);
        }

        private static string getNonce()
        {
            string rtn = Path.GetRandomFileName() + Path.GetRandomFileName() + Path.GetRandomFileName();
            rtn = rtn.Replace(".", "");
            if (rtn.Length > 32)
                return rtn.Substring(0, 32);
            else
                return rtn;
        }

        private static string getTimestamp()
        {
            return ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
        }

        private static string normalizeParams(
          string httpMethod,
          Uri url,
          IEnumerable<KeyValuePair<string, string>> oauthParams
        )
        {
            IEnumerable<KeyValuePair<string, string>> kvpParams = oauthParams;

            // Place any Query String parameters into a key value pair using equals ("=") to mark
            // the key/value relationship and join each paramter with an ampersand ("&")
            if (!String.IsNullOrWhiteSpace(url.Query))
            {
                IEnumerable<KeyValuePair<string, string>> queryParams =
                  from p in url.Query.Substring(1).Split('&').AsEnumerable()
                  let key = Uri.EscapeDataString(p.Substring(0, p.IndexOf("=")))
                  let value = Uri.EscapeDataString(p.Substring(p.IndexOf("=") + 1))
                  select new KeyValuePair<string, string>(key, value);

                kvpParams = kvpParams.Union(queryParams);
            }

            

            // Sort the parameters in lexicographical order, 1st by Key then by Value; separate with ("=")
            IEnumerable<string> sortedParams =
              from p in kvpParams
              orderby p.Key ascending, p.Value ascending
              select p.Key + "=" + p.Value;

            // Add the ampersand delimiter and then URL-encode the equals ("%3D") and ampersand ("%26")
            string stringParams = String.Join("&", sortedParams);
            string encodedParams = Uri.EscapeDataString(stringParams);

            return encodedParams;
        }

        private static string generateHmac(string key, string msg)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] msgBytes = Encoding.UTF8.GetBytes(msg);

            HMACSHA1 hmac = new HMACSHA1(keyBytes);
            
            return Convert.ToBase64String(hmac.ComputeHash(msgBytes));
        }
    }
}
