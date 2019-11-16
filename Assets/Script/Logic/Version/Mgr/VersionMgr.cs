using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

namespace Framework
{
    public class VersionMgr
    {
        private string m_appVersion = "1.0";
        private string m_resVersion = "0.0";

        // 版本服务器信息
        private VersionSvrModel m_serverResult = null;

        // 客户端端版本信息（最后）
        private VersionFileModel m_versionFileOnClient = null;
        // 服务器端版本信息
        private VersionFileModel m_versionFileOnServer = null;
        // 下载索引
        private int m_downloadIndex = 0;

        private List<string> m_needUpdateFileList = new List<string>();

        public static VersionMgr Instance = null;

        public VersionMgr()
        {
            Instance = this;
        }

        /// <summary>
        /// 开始检查版本
        /// </summary>
        public void StartCheckVersion()
        {
            GetClientVersion();
            m_serverResult = CheckVersionFromServer(m_appVersion, m_resVersion);
            if (m_serverResult.VersionType == VersionType.App) {
                //提示下载新的APP，新的下载地址result.downloadUrl
                return;
            } else if (m_serverResult.VersionType == VersionType.Res) {
                DownloadVersionFile();
            } else if (m_serverResult.VersionType == VersionType.None) {
                //验证通过，直接进入游戏
            }
        }

        /// <summary>
        /// 获取客户端版本
        /// </summary>
        private void GetClientVersion()
        {
            m_appVersion = VersionConfig.s_appVersion;
            if (m_versionFileOnClient != null) {
                m_resVersion = m_versionFileOnClient.resVersion;
                return;
            }
            string verionFilePath = string.Format("{0}/{1}", DownloadConfig.s_downLoadPath, VersionConfig.s_versionFileName);
            // 获得上一次客户端更新到的版本
            string content = SimpleLoader.LoadText(verionFilePath);
            if (string.IsNullOrEmpty(content)) {
                m_resVersion = VersionConfig.s_resVersion;
            } else {
                VersionHelper.ParseVersionFile(content, ref m_versionFileOnClient);
                m_resVersion = m_versionFileOnClient.resVersion;
            }
        }

        /// <summary>
        /// 服务器获取版本信息
        /// </summary>
        /// <param name="appVersion"></param>
        /// <param name="resVersion"></param>
        /// <returns></returns>
        private VersionSvrModel CheckVersionFromServer(string appVersion, string resVersion)
        {
            VersionSvrModel result = new VersionSvrModel();
            result.DownloadBaseUrl = "http://10.12.21.75";
            // 测试的时候改为VersionType.Res,代表资源更新
            result.VersionType = VersionType.Res;
            result.ServerResVersion = "0";
            // 下载地址
            result.DownloadBaseUrl = GetDownloadUrlWithPlatform(result.DownloadBaseUrl);
            return result;
        }

        /// <summary>
        /// 获取下载地址
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string GetDownloadUrlWithPlatform(string url)
        {
#if UNITY_IOS
            return string.Format("{0}/{1}/{2}", url, "iOS", m_appVersion);
#elif UNITY_ANDROID
		    return string.Format("{0}/{1}/{2}", url, "Android", m_appVersion);
#elif UNITY_STANDALONE_OSX
            return string.Format("{0}/{1}/{2}", url, "OSX", m_appVersion);
#elif UNITY_STANDALONE_WIN
            return string.Format("{0}/{1}/{2}", url, "Win", m_appVersion);
#endif
        }

        /// <summary>
        /// 下载版本文件
        /// </summary>
        /// <param name="delay"></param>
        private void DownloadVersionFile(float delay = -1)
        {
            //首先下载updateFile文件，然后从updateFile文件里面下载需要更新的资源
            Debug.Log("Begin download version files");
            string versionFileDownUrl = string.Format("{0}/{1}/{2}", m_serverResult.DownloadBaseUrl, m_serverResult.ServerResVersion, VersionConfig.s_versionFileName);
            Debug.Log(versionFileDownUrl);
            DownloadMgr.Instance.Download(versionFileDownUrl, OnVersionFileDownloadOnFinsh, delay);
        }

        /// <summary>
        /// 下载完成回调
        /// </summary>
        /// <param name="www"></param>
        private void OnVersionFileDownloadOnFinsh(WWW www)
        {
            if (www == null) {
                // 如果下载失败，等待1s后重新下载，可以是个逐渐增长的等待时间
                DownloadVersionFile(1.0f);
                return;
            }
            string content = www.text;
            m_downloadIndex = 0;
            VersionHelper.ParseVersionFile(content, ref m_versionFileOnServer);
            GenerateUpdateFilesList();
            DownloadUpdateFiles(m_downloadIndex);
        }
        
        /// <summary>
        /// 通用更新文件列表
        /// </summary>
        private void GenerateUpdateFilesList()
        {
            m_needUpdateFileList.Clear();
            if (m_versionFileOnServer == null) {
                return;
            }
            Dictionary<string, VersionFileInfo> allFileDic = new Dictionary<string, VersionFileInfo>();
            if (m_versionFileOnClient == null) {
                allFileDic = m_versionFileOnServer.files;
            } else {
                //通过比较2次的updateFile生成需要更新的文件
                foreach (KeyValuePair<string, VersionFileInfo> kvp in m_versionFileOnServer.files) {
                    VersionFileInfo lastFileInfo = null;
                    m_versionFileOnClient.files.TryGetValue(kvp.Key, out lastFileInfo);
                    if (lastFileInfo == null || lastFileInfo.md5 != kvp.Value.md5) {
                        allFileDic[kvp.Key] = kvp.Value;
                    }
                }
            }
            foreach (KeyValuePair<string, VersionFileInfo> kvp in allFileDic) {
                string filesDownloadPath = string.Format("{0}/{1}", DownloadConfig.s_downLoadPath, kvp.Key);
                if (File.Exists(filesDownloadPath)) {
                    string md5 = VersionHelper.GetMd5Val(filesDownloadPath);
                    if (!string.Equals(md5, kvp.Value.md5)) {
                        m_needUpdateFileList.Add(kvp.Key);
                    }
                } else {
                    m_needUpdateFileList.Add(kvp.Key);
                }
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="index"></param>
        /// <param name="delay"></param>
        private void DownloadUpdateFiles(int index, float delay = -1)
        {
            if (index >= m_needUpdateFileList.Count) {
                //下载完所有更新文件之后，写入新的updateFile
                m_versionFileOnClient = m_versionFileOnServer;
                WriteVersionFile();
                return;
            }
            Debug.Log("Begin download update files " + m_needUpdateFileList[index]);
            string version = m_versionFileOnServer.files[m_needUpdateFileList[index]].version;
            string downloadUrl = string.Format("{0}/{1}/{2}", m_serverResult.DownloadBaseUrl, version, m_needUpdateFileList[index]);
            DownloadMgr.Instance.Download(downloadUrl, OnUpdateFileDownloadFinised);
        }

        /// <summary>
        /// 文件更新完成
        /// </summary>
        /// <param name="www"></param>
        private void OnUpdateFileDownloadFinised(WWW www)
        {
            if (www == null) {
                //如果下载失败，等待1s后重新下载，可以是个逐渐增长的等待时间
                DownloadUpdateFiles(m_downloadIndex, 1.0f);
                return;
            }
            byte[] content = www.bytes;
            string writePath = string.Format("{0}/{1}", DownloadConfig.s_downLoadPath, m_needUpdateFileList[m_downloadIndex]);
            if (WriteUpdateFile(writePath, content)) {
                m_downloadIndex += 1;
                DownloadUpdateFiles(m_downloadIndex);
            }
        }

        /// <summary>
        /// 写更新文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private bool WriteUpdateFile(string path, byte[] content)
        {
            try {
                string dir = path.Substring(0, path.LastIndexOf("/"));
                if (!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }
                FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
                fs.Write(content, 0, content.Length);
                fs.Flush();
                fs.Close();
                fs.Dispose();
                return true;
            } catch (Exception e) {
                Debug.Log("write download file to disk exception " + e.ToString());
            }
            return false;
        }

        /// <summary>
        /// 下版本文件
        /// </summary>
        private void WriteVersionFile()
        {
            string str = VersionHelper.ConvertVersionFileToString(m_versionFileOnServer);
            string versionFilePath = string.Format("{0}/{1}", DownloadConfig.s_downLoadPath, VersionConfig.s_versionFileName);
            File.WriteAllText(versionFilePath, str, System.Text.Encoding.UTF8);
        }

        public bool CheckFileIsInVersionFile(string path)
        {
            if (m_versionFileOnClient != null && m_versionFileOnClient.files.ContainsKey(path)) {
                return true;
            }
            return false;
        }
    }
}