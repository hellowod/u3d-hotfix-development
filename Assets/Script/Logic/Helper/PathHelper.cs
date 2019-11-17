using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework
{
    public class PathHelper
    {
        /// <summary>
        /// 获取下载地址
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetDownloadUrlWithPlatform(string url, string appVersion)
        {
#if UNITY_IOS
            return string.Format("{0}/{1}/{2}", url, "iOS", appVersion);
#elif UNITY_ANDROID
            return string.Format("{0}/{1}/{2}", url, "Android", appVersion);
#elif UNITY_STANDALONE_OSX
            return string.Format("{0}/{1}/{2}", url, "OSX", appVersion);
#elif UNITY_STANDALONE_WIN
            return string.Format("{0}/{1}/{2}", url, "Win", appVersion);
#endif
        }
    }
}
