using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRestConnector
{
    public class AuthInfo
    {
        public String Url { get; set; }
        public String User { get; set; }
        public String Password { get; set; }
        public String oauth2Token { get; set; }
        public String oauth1Token { get; set; }
        public String oauth1Secret { get; set; }
        public String ConsumerSecret { get; set; }
        public String APIKey { get; set; }
    }
}
