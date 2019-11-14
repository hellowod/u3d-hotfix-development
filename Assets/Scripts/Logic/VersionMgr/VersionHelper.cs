using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Framework
{
    public class VersionConfig
    {
        public static string app_version = "1.0";
        public static string res_version = "1";
        public static string VersionFileName = "VersionFile.txt";
    }

    public class VersionFileInfo
    {
        private string m_md5;
        private string m_version;

        public string md5
        {
            get { return m_md5; }
        }
        
        public string version
        {
            get { return m_version; }
        }

        public VersionFileInfo(string md5Val, string versionVal)
        {
            m_md5 = md5Val;
            m_version = versionVal;
        }
    }

    public class VersionFileModel
    {
        public Dictionary<string, VersionFileInfo> files = new Dictionary<string, VersionFileInfo>();
        public string resVersion = "0.0";
    }

    public class VersionHelper
    {
        /// <summary>
        /// 解析版本文件
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="versionFile"></param>
        public static void ParseVersionFile(string txt, ref VersionFileModel versionFile)
        {
            string[] content = txt.Split('\n');
            if (content != null) {
                if (versionFile == null)
                    versionFile = new VersionFileModel();
                for (int i = 0; i < content.Length - 1; i++) {
                    if (!string.IsNullOrEmpty(content[i])) {
                        string[] kvp = content[i].Split(',');
                        if (kvp == null || kvp.Length < 3) {
                            Debug.LogError("Can not parse last update file with content " + content[i]);
                            return;
                        }
                        versionFile.files[kvp[0]] = new VersionFileInfo(kvp[1], kvp[2]);
                    }
                }
                versionFile.resVersion = content[content.Length - 1];
            }
        }

        /// <summary>
        /// 转换版本文件为String
        /// </summary>
        /// <param name="versionFile"></param>
        /// <returns></returns>
        public static string ConvertVersionFileToString(VersionFileModel versionFile)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, VersionFileInfo> kvp in versionFile.files) {
                string content = string.Format("{0},{1},{2}\n", kvp.Key, kvp.Value.md5, kvp.Value.version);
                sb.Append(content);
            }
            sb.Append(versionFile.resVersion);
            return sb.ToString();
        }

        /// <summary>
        /// 获取文件MD5值
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetMd5Val(string path)
        {
            FileStream file = new FileStream(path, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();
            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < retVal.Length; j++) {
                sb.Append(retVal[j].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}