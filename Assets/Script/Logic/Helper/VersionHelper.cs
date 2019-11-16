using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Framework
{
    public class VersionHelper
    {
        /// <summary>
        /// 解析版本信息
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="versionModel"></param>
        public static void ParseVersion(string txt, ref VersionSvrModel versionModel)
        {
            string[] content = txt.Split('\n');
            if (content != null) {
                if (versionModel == null) {
                    versionModel = new VersionSvrModel();
                }
                for (int i = 0; i < content.Length; i++) {
                    string line = content[i];
                    if (string.IsNullOrEmpty(line)) {
                        continue;
                    }
                    if (line.StartsWith(";")) {
                        continue;
                    }
                    string[] lineArr = line.Split('=');
                    if (lineArr == null) {
                        continue;
                    }
                    string key = lineArr[0].Trim();
                    string val = lineArr[1].Trim();
                    if (key.Equals("Version")) {
                        versionModel.AppVersion = val.Substring(0, val.LastIndexOf('.'));
                        versionModel.ResVersion = val.Substring(val.LastIndexOf('.') + 1);
                    } else if (key.Equals("ResUpdateURL")) {
                        versionModel.DownloadBaseUrl = val;
                    }
                }
            }
        }

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
                        versionFile.FilesDic[kvp[0]] = new VersionFileInfo(kvp[1], kvp[2]);
                    }
                }
                versionFile.ResVersion = content[content.Length - 1];
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
            foreach (KeyValuePair<string, VersionFileInfo> kvp in versionFile.FilesDic) {
                string content = string.Format("{0},{1},{2}\n", kvp.Key, kvp.Value.md5, kvp.Value.version);
                sb.Append(content);
            }
            sb.Append(versionFile.ResVersion);
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