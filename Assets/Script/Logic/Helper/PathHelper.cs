namespace Framework
{
    public class PathHelper
    {
        /// <summary>
        /// 获取版本文件路径
        /// </summary>
        /// <returns></returns>
        public static string GetPlatformName()
        {
            string platformName = string.Empty;
#if UNITY_IOS
            platformName = "iOS/";
#elif UNITY_ANDROID
            platformName = "Android/";
#elif UNITY_STANDALONE_WIN
            platformName = "Win/";
#elif UNITY_STANDALONE_OSX
            platformName = "OSX/";
#endif
            return platformName;
        }

        /// <summary>
        /// 获取下载地址
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetDownloadUrlWithPlatform(string url, string appVersion)
        {
            return string.Format("{0}/{1}/{2}", url, GetPlatformName(), appVersion);
        }
    }
}
