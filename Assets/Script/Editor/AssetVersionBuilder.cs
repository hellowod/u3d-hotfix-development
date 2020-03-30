using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;

namespace Framework
{
    public class AssetVersionBuilder : Editor
    {
        public static string s_bundlesPath = EditorUtils.GetExportBundlePath();

        private static string s_verionFilePath = EditorUtils.GetExportVersionPath();
        private static string s_versionMD5FilePath = EditorUtils.GetExportVersionMD5Path();
        
        
        private static Dictionary<string, string> s_allFilesMd5NowVersion = new Dictionary<string, string>();
        private static Dictionary<string, string> s_allFilesMd5LastVersion = new Dictionary<string, string>();

        private static string s_versionPatchPath = EditorUtils.GetExportPatchPath(VersionConfig.s_appVersion, VersionConfig.s_resVersion);

        private static VersionFileModel s_versionFile = new VersionFileModel();

        [MenuItem("Tool/Version/CleanBuildApp")]
        private static void BuildApp()
        {
            CleanBuildAllBundle();
            BuildText();
            CleanAndWriteNewVersion();
            ExportProject();
        }

        [MenuItem("Tool/Version/FastBuildApp")]
        private static void FastBuildApp()
        {
            BuildAllBundle();
            BuildText();
            CleanAndWriteNewVersion();
            ExportProject();
        }

        /// <summary>
        /// 清理然后写新的版本文件
        /// </summary>
        private static void CleanAndWriteNewVersion()
        {
            CleanVersionFiles();
            if (GenerateAllFilesMD5()) {
                WriteVersionMd5Files();
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 导出目标工程
        /// </summary>
        private static void ExportProject()
        {
            BuildPlayerOptions options = new BuildPlayerOptions();
            options.target = EditorUtils.GetBuildTarget();
            string exportPath = EditorUtils.GetExportProjectPath("application");
            string folderPath = Path.GetDirectoryName(exportPath);
            if (Directory.Exists(folderPath)) {
                FileUtil.DeleteFileOrDirectory(folderPath);
            }
            options.locationPathName = exportPath;
            options.options = BuildOptions.AcceptExternalModificationsToPlayer;
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded) {
                Debug.LogFormat("Export project path={0} success!", exportPath);
            } else {
                Debug.LogFormat("Export project path={0} Failure!", exportPath);
            }
        }

        [MenuItem("Tool/Version/CleanBuildPatch")]
        private static void BuildAssetbundle()
        {
            if (CheckCanBuildPatch()) {
                CleanBuildAllBundle();
                BuildText();
                GenerateUpdateFiles();
            }
        }

        [MenuItem("Tool/Version/FastBuildPatch")]
        private static void FastBuildAssetbundle()
        {
            if (CheckCanBuildPatch()) {
                BuildAllBundle();
                BuildText();
                GenerateUpdateFiles();
            }
        }

        /// <summary>
        /// 检查是否可以构建补丁包
        /// </summary>
        /// <returns></returns>
        private static bool CheckCanBuildPatch()
        {
            if (!File.Exists(s_versionMD5FilePath)) {
                Debug.LogError("Can not find last version,you may execute \"CleanBuildApp\" first if you want to update assetbundles based on last version");
                return false;
            }
            if (LoadVersionFile() == null) {
                Debug.LogError("Load last update file failed!");
                return false;
            }
            if (s_versionFile.ResVersion == VersionConfig.s_resVersion) {
                Debug.LogError("You must change your res_verion in version config if you want to publish a patch");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 生成更新版本文件
        /// </summary>
        private static void GenerateUpdateFiles()
        {
            if (!LoadMd5VersionFile()) {
                Debug.LogError("Load last version md5 file failed!");
                return;
            }
            if (!GenerateAllFilesMD5()) {
                Debug.LogError("Generate new version md5 file failed!");
                return;
            }

            List<string> needUpdateFileList = GenerateDiffFilesList();
            List<string> needDeleteFileList = GenerateDeleteFilesList();
            if (needUpdateFileList.Count == 0 && needDeleteFileList.Count == 0) {
                Debug.LogError("nothing need update");
                return;
            }
            // set update files version
            string newVersion = VersionConfig.s_resVersion;
            s_versionFile.ResVersion = newVersion;
            for (int i = 0; i < needUpdateFileList.Count; i++) {
                s_versionFile.FilesDic[needUpdateFileList[i]] = new VersionFileInfo(s_allFilesMd5NowVersion[needUpdateFileList[i]], newVersion);
            }
            // delete unused assetbundles
            for (int i = 0; i < needDeleteFileList.Count; i++) {
                if (s_versionFile.FilesDic.ContainsKey(needDeleteFileList[i])) {
                    s_versionFile.FilesDic.Remove(needDeleteFileList[i]);
                }
            }
            WriteVersionFiles();
            WriteVersionMd5Files();

            //export update assetbundles
            ExportUpdateFiles(needUpdateFileList);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 清除版本文件
        /// </summary>
        private static void CleanVersionFiles()
        {
            if (Directory.Exists(s_versionMD5FilePath)) {
                Directory.Delete(s_versionMD5FilePath, true);
            }
        }

        /// <summary>
        /// 写新版本文件
        /// </summary>
        private static void WriteVersionMd5Files()
        {
            string versionMD5Folder = Path.GetDirectoryName(s_versionMD5FilePath);
            if (!Directory.Exists(versionMD5Folder)) {
                Directory.CreateDirectory(versionMD5Folder);
            }
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> kvp in s_allFilesMd5NowVersion) {
                string content = kvp.Key + "," + kvp.Value + "\n";
                sb.Append(content);
            }
            File.WriteAllText(s_versionMD5FilePath, sb.ToString());
        }

        /// <summary>
        /// 加载资源版本文件
        /// </summary>
        /// <returns></returns>
        private static bool LoadMd5VersionFile()
        {
            try {
                s_allFilesMd5LastVersion.Clear();
                string[] content = File.ReadAllLines(s_versionMD5FilePath);
                if (content != null) {
                    for (int i = 0; i < content.Length; i++) {
                        if (!string.IsNullOrEmpty(content[i])) {
                            string[] kvp = content[i].Split(',');
                            if (kvp == null || kvp.Length < 2) {
                                Debug.LogError("Can not parse last md5 file with content " + content[i]);
                                return false;
                            }
                            s_allFilesMd5LastVersion[kvp[0]] = kvp[1];
                        }
                    }
                }
            } catch (Exception ex) {
                throw new Exception("LoadLastVersionMd5 Error:" + ex.Message);
            }
            return true;
        }

        /// <summary>
        /// 生成版本MD5文件
        /// </summary>
        /// <returns></returns>
        private static bool GenerateAllFilesMD5()
        {
            s_allFilesMd5NowVersion.Clear();
            if (!Directory.Exists(s_bundlesPath)) {
                Debug.LogError(string.Format("AssetsPath path {0} not exist", s_bundlesPath));
                return false;
            }
            DirectoryInfo dir = new DirectoryInfo(s_bundlesPath);
            FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; ++i) {
                try {
                    if (files[i].Name.EndsWith(".meta") || files[i].Name.EndsWith(".manifest")) {
                        continue;
                    }
                    string fileMD5 = FileHelper.GetMd5Val(files[i].FullName);
                    string fileRelativePath = files[i].FullName.Substring(s_bundlesPath.Length + 1);
                    s_allFilesMd5NowVersion[fileRelativePath] = fileMD5;
                } catch (Exception ex) {
                    throw new Exception("GetMD5HashFromFile fail,error:" + ex.Message);
                }
            }
            return true;
        }

        /// <summary>
        /// 生成插件文件列表
        /// </summary>
        /// <returns></returns>
        private static List<string> GenerateDiffFilesList()
        {
            List<string> needUpdateFileList = new List<string>();
            foreach (KeyValuePair<string, string> kvp in s_allFilesMd5NowVersion) {
                if (s_allFilesMd5LastVersion.ContainsKey(kvp.Key)) {
                    if (s_allFilesMd5LastVersion[kvp.Key] == kvp.Value) {
                        continue;
                    } else {
                        needUpdateFileList.Add(kvp.Key);
                    }
                } else {
                    needUpdateFileList.Add(kvp.Key);
                }
            }
            return needUpdateFileList;
        }

        /// <summary>
        /// 生成待删除的文件列表
        /// </summary>
        /// <returns></returns>
        private static List<string> GenerateDeleteFilesList()
        {
            List<string> needDeleteFileList = new List<string>();
            foreach (KeyValuePair<string, string> kvp in s_allFilesMd5LastVersion) {
                if (s_allFilesMd5NowVersion.ContainsKey(kvp.Key)) {
                    continue;
                } else {
                    needDeleteFileList.Add(kvp.Key);
                }
            }
            return needDeleteFileList;
        }

        /// <summary>
        /// 加载版本文件
        /// </summary>
        /// <returns></returns>
        private static VersionFileModel LoadVersionFile()
        {
            s_versionFile = new VersionFileModel();
            if (File.Exists(s_verionFilePath)) {
                try {
                    string content = File.ReadAllText(s_verionFilePath);
                    FileHelper.ParseVersionFile(content, ref s_versionFile);
                } catch (Exception ex) {
                    throw new Exception("Load UpdateFile Error:" + ex.Message);
                }
            }
            return s_versionFile;
        }

        /// <summary>
        /// 导出资源
        /// </summary>
        /// <param name="updatedFiles"></param>
        private static void ExportUpdateFiles(List<string> updatedFiles)
        {
            if (updatedFiles.Count > 0) {
                for (int i = 0; i < updatedFiles.Count; i++) {
                    string assetbundleDestPath = updatedFiles[i];
                    string assetbundleSrcPath = updatedFiles[i];
                    string destDir = Path.GetDirectoryName(assetbundleDestPath);
                    if (!Directory.Exists(destDir)) {
                        Directory.CreateDirectory(destDir);
                    }
                    File.Copy(assetbundleSrcPath, assetbundleDestPath, true);
                }
            }
            string versionFileSrcPath = s_verionFilePath;
            string versionFileDestPath = s_versionPatchPath + "/" + Path.GetFileName(versionFileSrcPath);
            string updateFileDestDir = Path.GetDirectoryName(versionFileDestPath);
            if (!Directory.Exists(updateFileDestDir)) {
                Directory.CreateDirectory(updateFileDestDir);
            }
            File.Copy(versionFileSrcPath, versionFileDestPath, true);
        }

        private static void WriteVersionFiles()
        {
            string str = FileHelper.ConvertVersionFileToString(s_versionFile);
            File.WriteAllText(s_verionFilePath, str);
        }

        /// <summary>
        /// 清除然后构建AssetBundle
        /// </summary>
        private static void CleanBuildAllBundle()
        {
            if (Directory.Exists(s_bundlesPath)) {
                Directory.Delete(s_bundlesPath, true);
            }
            BuildAllBundle();
        }

        /// <summary>
        /// 构建AssetBundle
        /// </summary>
        private static void BuildAllBundle()
        {
            AssetBundleBuilder.BuildAssetBundle();
        }

        /// <summary>
        /// 构建的文本资源
        /// </summary>
        private static void BuildText()
        {
            AssetTextBuilder.BuildText();
        }
    }
}