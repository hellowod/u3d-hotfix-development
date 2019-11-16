using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

namespace Framework
{
    public enum VersionType
    {
        App,
        Res,
        None,
    }

    public class VersionMgr
    {
        private string m_appVersion = "1.0";
        private string m_resVersion = "0.0";

        class VersionResultFromServer
        {
            public string downloadBaseUrl = null;
            public string serverResVersion;
            public VersionType versionType = VersionType.None;
        }

        private VersionResultFromServer m_serverResult = null;
        private VersionFileModel m_lastVersionFile = null;
        private VersionFileModel m_versionFileOnServer = null;
        private int m_downloadIndex = 0;

        private List<string> m_needUpdateFileList = new List<string>();

        public static VersionMgr instance = null;

        public VersionMgr()
        {
            instance = this;
        }

        /// <summary>
        /// 获取客户端版本
        /// </summary>
        private void GetClientVersion()
        {
            m_appVersion = VersionConfig.s_appVersion;
            if (m_lastVersionFile != null) {
                m_resVersion = m_lastVersionFile.resVersion;
                return;
            }
            string verionFilePath = string.Format("{0}/{1}", DownloadConfig.downLoadPath, VersionConfig.s_versionFileName);
            // 获得上一次客户端更新到的版本
            Debug.LogError(verionFilePath);
            string content = SimpleLoader.LoadText(verionFilePath);
            Debug.LogError(content);
            if (string.IsNullOrEmpty(content)) {
                m_resVersion = VersionConfig.s_resVersion;
            } else {
                VersionHelper.ParseVersionFile(content, ref m_lastVersionFile);
                m_resVersion = m_lastVersionFile.resVersion;
            }
        }

        /// <summary>
        /// 开始检查版本
        /// </summary>
        public void StartCheckVersion()
        {
            GetClientVersion();
            m_serverResult = CheckVersionFromServer(m_appVersion, m_resVersion);
            if (m_serverResult.versionType == VersionType.App) {
                //提示下载新的APP，新的下载地址result.downloadUrl
                return;
            } else if (m_serverResult.versionType == VersionType.Res) {
                DownloadVersionFile();
            } else if (m_serverResult.versionType == VersionType.None) {
                //验证通过，直接进入游戏
            }
        }

        /// <summary>
        /// 服务器获取版本信息
        /// </summary>
        /// <param name="appVersion"></param>
        /// <param name="resVersion"></param>
        /// <returns></returns>
        private VersionResultFromServer CheckVersionFromServer(string appVersion, string resVersion)
        {
            VersionResultFromServer result = new VersionResultFromServer();
            //result.downloadBaseUrl = "file:///Users/yr/GamePatch";
            result.downloadBaseUrl = "http://192.168.10.180:9001";
            //测试的时候改为VersionType.Res,代表资源更新
            result.versionType = VersionType.None;
            result.serverResVersion = "170423.2";
            //服务器传回来的下载地址假设是xx.xx.xxx.xx,我们需要根据平台重新生成一下，最终为xx.xx.xxx.xx/iOS 或者xx.xx.xxx.xx/Android
            result.downloadBaseUrl = ConnectDownloadUrlWithPlatform(result.downloadBaseUrl);
            return result;
        }

        private string ConnectDownloadUrlWithPlatform(string url)
        {
#if UNITY_IOS
            return string.Format("{0}/{1}/{2}", url, "iOS", _appVersion);
#elif UNITY_ANDROID
		    return string.Format("{0}/{1}/{2}", url, "Android", _appVersion);
#elif UNITY_STANDALONE_OSX
            return string.Format("{0}/{1}/{2}", url, "OSX", _appVersion);
#elif UNITY_STANDALONE_WIN
            return string.Format("{0}/{1}/{2}", url, "Win", m_appVersion);
#endif
        }

        private void DownloadVersionFile(float delay = -1)
        {
            //首先下载updateFile文件，然后从updateFile文件里面下载需要更新的资源
            Debug.Log("Begin download version files");
            string versionFileDownUrl = string.Format("{0}/{1}/{2}", m_serverResult.downloadBaseUrl, m_serverResult.serverResVersion, VersionConfig.s_versionFileName);
            DownloadMgr.instance.Download(versionFileDownUrl, OnVersionFileDownload, delay);
        }

        private void OnVersionFileDownload(WWW www)
        {
            if (www == null) {
                //如果下载失败，等待1s后重新下载，可以是个逐渐增长的等待时间
                DownloadVersionFile(1.0f);
                return;
            }
            string content = www.text;
            m_downloadIndex = 0;
            VersionHelper.ParseVersionFile(content, ref m_versionFileOnServer);
            GenerateUpdateFilesList();
            DownloadUpdateFiles(m_downloadIndex);
        }
        
        private void GenerateUpdateFilesList()
        {
            m_needUpdateFileList.Clear();
            if (m_versionFileOnServer == null) {
                return;
            }
            Dictionary<string, VersionFileInfo> _allFileDic = new Dictionary<string, VersionFileInfo>();
            if (m_lastVersionFile == null) {
                _allFileDic = m_versionFileOnServer.files;
            } else {
                //通过比较2次的updateFile生成需要更新的文件
                foreach (KeyValuePair<string, VersionFileInfo> kvp in m_versionFileOnServer.files) {
                    VersionFileInfo lastFileInfo = null;
                    m_lastVersionFile.files.TryGetValue(kvp.Key, out lastFileInfo);
                    if (lastFileInfo == null || lastFileInfo.md5 != kvp.Value.md5) {
                        _allFileDic[kvp.Key] = kvp.Value;
                    }
                }
            }
            foreach (KeyValuePair<string, VersionFileInfo> kvp in _allFileDic) {
                string filesDownloadPath = string.Format("{0}/{1}", DownloadConfig.downLoadPath, kvp.Key);
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

        private void DownloadUpdateFiles(int index, float delay = -1)
        {
            if (index >= m_needUpdateFileList.Count) {
                //下载完所有更新文件之后，写入新的updateFile
                m_lastVersionFile = m_versionFileOnServer;
                WriteVersionFile();
                return;
            }
            Debug.Log("Begin download update files " + m_needUpdateFileList[index]);
            string version = m_versionFileOnServer.files[m_needUpdateFileList[index]].version;
            string downloadUrl = string.Format("{0}/{1}/{2}", m_serverResult.downloadBaseUrl, version, m_needUpdateFileList[index]);
            DownloadMgr.instance.Download(downloadUrl, OnUpdateFileDownloadFinised);
        }

        private void OnUpdateFileDownloadFinised(WWW www)
        {
            if (www == null) {
                //如果下载失败，等待1s后重新下载，可以是个逐渐增长的等待时间
                DownloadUpdateFiles(m_downloadIndex, 1.0f);
                return;
            }
            byte[] content = www.bytes;
            string writePath = string.Format("{0}/{1}", DownloadConfig.downLoadPath, m_needUpdateFileList[m_downloadIndex]);
            if (WriteUpdateFile(writePath, content)) {
                m_downloadIndex += 1;
                DownloadUpdateFiles(m_downloadIndex);
            }
        }

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

        private void WriteVersionFile()
        {
            string str = VersionHelper.ConvertVersionFileToString(m_versionFileOnServer);
            string versionFilePath = string.Format("{0}/{1}", DownloadConfig.downLoadPath, VersionConfig.s_versionFileName);
            File.WriteAllText(versionFilePath, str, System.Text.Encoding.UTF8);
        }

        public bool CheckFileIsInVersionFile(string path)
        {
            if (m_lastVersionFile != null && m_lastVersionFile.files.ContainsKey(path)) {
                return true;
            }
            return false;
        }
    }
}