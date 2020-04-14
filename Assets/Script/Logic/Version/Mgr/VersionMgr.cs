using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

namespace Framework
{
    public class VersionMgr
    {
        private const string VERSION_URL = "http://10.12.21.75/Version.txt";

        private string m_appVersion = "";
        private string m_resVersion = "";

        // 版本服务器信息
        private VersionSvrModel m_serverResult = null;

        // 客户端端版本信息
        private VersionFileModel m_versionFileOnClient = null;
        // 服务器端版本信息
        private VersionFileModel m_versionFileOnServer = null;
        // 下载索引
        private int m_downloadIndex = 0;

        private List<string> m_needUpdateFileList = new List<string>();

        private int m_versionFlagIndex = 0;

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
            GetServerVersion();
        }

        public void Update()
        {
            if (m_versionFlagIndex > 1) {
                m_versionFlagIndex = 0;
                if (m_serverResult.VersionType == VersionType.App) {
                    Debug.Log("新版本，需要下载");
                    return;
                } else if (m_serverResult.VersionType == VersionType.Res) {
                    Debug.Log("新资源，需要下载");
                    DownloadVersionFile();
                } else if (m_serverResult.VersionType == VersionType.None) {
                    Debug.Log("无更新，直接进入");
                    LevelLoader.LoadLevelAsync("CreatePlayer");
                }
            }
        }

        /// <summary>
        /// 获取服务器版本
        /// </summary>
        private void GetServerVersion()
        {
            DownloadMgr.Instance.Download(VERSION_URL, OnVersionDownloadFinish);
        }

        /// <summary>
        /// 服务器版本下载完成
        /// </summary>
        /// <param name="www"></param>
        private void OnVersionDownloadFinish(WWW www)
        {
            if (www == null) {
                GetServerVersion();
                return;
            }
            FileHelper.ParseVersion(www.text, ref m_serverResult);
            if (m_serverResult.AppVersion != m_appVersion) {
                m_serverResult.VersionType = VersionType.App;
            } else if (m_serverResult.ResVersion != m_resVersion) {
                m_serverResult.VersionType = VersionType.Res;
            } else {
                m_serverResult.VersionType = VersionType.None;
            }
            m_serverResult.DownloadBaseUrl = PathHelper.GetDownloadUrlWithPlatform(m_serverResult.DownloadBaseUrl, m_appVersion);
            m_versionFlagIndex++;
            Debug.LogFormat("Server Version: {0}.{1}", m_serverResult.AppVersion, m_serverResult.ResVersion);
        }

        /// <summary>
        /// 获取客户端版本
        /// </summary>
        private void GetClientVersion()
        {
            m_appVersion = VersionConfig.APP_VERSION;
            if (m_versionFileOnClient != null) {
                m_resVersion = m_versionFileOnClient.ResVersion;
                return;
            }
            string verionFilePath = string.Format("{0}/{1}", DownloadConfig.DownLoadPath, VersionConfig.VERSION_DIFF_FILE);
            // 获得上一次客户端更新到的版本
            string content = SimpleLoader.LoadText(verionFilePath);
            if (string.IsNullOrEmpty(content)) {
                m_resVersion = VersionConfig.RES_VERSION;
            } else {
                FileHelper.ParseVersionFile(content, ref m_versionFileOnClient);
                m_resVersion = m_versionFileOnClient.ResVersion;
            }
            m_versionFlagIndex++;
            Debug.LogFormat("Client Version: {0}.{1}", m_appVersion, m_resVersion);
        }

        /// <summary>
        /// 下载版本文件
        /// </summary>
        /// <param name="delay"></param>
        private void DownloadVersionFile(float delay = -1)
        {
            //首先下载updateFile文件，然后从updateFile文件里面下载需要更新的资源
            Debug.Log("Begin download version files");
            string versionFileDownUrl = string.Format("{0}/{1}/{2}", m_serverResult.DownloadBaseUrl, m_serverResult.ResVersion, VersionConfig.VERSION_DIFF_FILE);
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
            FileHelper.ParseVersionFile(content, ref m_versionFileOnServer);
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
                allFileDic = m_versionFileOnServer.FilesDic;
            } else {
                //通过比较2次的updateFile生成需要更新的文件
                foreach (KeyValuePair<string, VersionFileInfo> kvp in m_versionFileOnServer.FilesDic) {
                    VersionFileInfo lastFileInfo = null;
                    m_versionFileOnClient.FilesDic.TryGetValue(kvp.Key, out lastFileInfo);
                    if (lastFileInfo == null || lastFileInfo.md5 != kvp.Value.md5) {
                        allFileDic[kvp.Key] = kvp.Value;
                    }
                }
            }
            foreach (KeyValuePair<string, VersionFileInfo> kvp in allFileDic) {
                string filesDownloadPath = string.Format("{0}/{1}", DownloadConfig.DownLoadPath, kvp.Key);
                if (File.Exists(filesDownloadPath)) {
                    string md5 = FileHelper.GetMd5Value(filesDownloadPath);
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
            string version = m_versionFileOnServer.FilesDic[m_needUpdateFileList[index]].version;
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
            string writePath = string.Format("{0}/{1}", DownloadConfig.DownLoadPath, m_needUpdateFileList[m_downloadIndex]);
            Debug.LogError(writePath);
            if (WriteUpdateFile(writePath.Replace('\\', '/'), content)) {
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
                Debug.Log("write download file exception " + e.ToString());
            }
            return false;
        }

        /// <summary>
        /// 下版本文件
        /// </summary>
        private void WriteVersionFile()
        {
            string str = FileHelper.ConvertVersionFileToString(m_versionFileOnServer);
            string versionFilePath = string.Format("{0}/{1}", DownloadConfig.DownLoadPath, VersionConfig.VERSION_DIFF_FILE);
            File.WriteAllText(versionFilePath, str, System.Text.Encoding.UTF8);
        }

        public bool CheckFileIsInVersionFile(string path)
        {
            if (m_versionFileOnClient != null && m_versionFileOnClient.FilesDic.ContainsKey(path)) {
                return true;
            }
            return false;
        }
    }
}