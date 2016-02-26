using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;
using QlikView.Qvx.QvxLibrary;

namespace GenericRestConnector
{
    public static class VariableStore
    {
        private static MemoryCache _cache = MemoryCache.Default;
    
        private static string session { get; set; }

        public static string getSession()
        {
            object sessionCache = _cache.Get("grcsession");
            if (sessionCache == null)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "Actual is - " + session);
                session = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
                _cache.Set("grcsession", session, new CacheItemPolicy());
            }
            else
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "From Cache is - " + sessionCache.ToString());
                session = sessionCache.ToString();
            }
            return session;
        }

    }
}
