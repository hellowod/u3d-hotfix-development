﻿using UnityEngine;
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
        public static string s_assetBundlesPath = EditorPathUtil.GetExportAssetBundlePath();

        private static string s_versionFullFilePath = EditorPathUtil.GetExportVersionFullFilePath();
        private static string s_versionDiffFilePath = EditorPathUtil.GetExportVersionDiffFilePath();

        private static string s_versionPatchPath = EditorPathUtil.GetExportPatchPath();

        private static Dictionary<string, string> s_allFilesMd5DiffVersion = new Dictionary<string, string>();
        private static Dictionary<string, string> s_allFilesMd5LastVersion = new Dictionary<string, string>();

        private static VersionFileModel s_versionFile = new VersionFileModel();

        [MenuItem("Tool/Version/CleanBuildApp")]
        private static void BuildApp()
        {
            CleanBuildAllBundle();
            BuildText();
            CleanAndWriteNewVersion();
            GenerateProject();
        }

        [MenuItem("Tool/Version/FastBuildApp")]
        private static void FastBuildApp()
        {
            BuildAllBundle();
            BuildText();
            CleanAndWriteNewVersion();
            GenerateProject();
        }

        /// <summary>
        /// 清理然后写新的版本文件
        /// </summary>
        private static void CleanAndWriteNewVersion()
        {
            CleanFullVersionFile();
            if (GenerateFullVersionFile()) {
                WriteFullVersionFile();
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 生成目标工程
        /// </summary>
        private static void GenerateProject()
        {
            BuildPlayerOptions options = new BuildPlayerOptions();
            options.target = EditorBaseUtil.GetBuildTarget();
            string exportPath = EditorPathUtil.GetExportProjectPath();
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
                GenerateUpdateFile();
            }
        }

        [MenuItem("Tool/Version/FastBuildPatch")]
        private static void FastBuildAssetbundle()
        {
            if (CheckCanBuildPatch()) {
                BuildAllBundle();
                BuildText();
                GenerateUpdateFile();
            }
        }

        /// <summary>
        /// 检查是否可以构建补丁包
        /// </summary>
        /// <returns></returns>
        private static bool CheckCanBuildPatch()
        {
            if (!File.Exists(s_versionFullFilePath)) {
                Debug.LogError("Can not find last version,you may execute \"CleanBuildApp\" first if you want to update assetbundles based on last version.");
                return false;
            }
            if (LoadDiffVersionFile() == null) {
                Debug.LogError("Load last version update file failed!");
                return false;
            }
            if (s_versionFile.ResVersion == VersionConfig.RES_VERSION) {
                Debug.LogError("You must change your res_verion in version config if you want to publish a patch.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 更新版本文件
        /// </summary>
        private static void GenerateUpdateFile()
        {
            if (!LoadFullVersionFile()) {
                Debug.LogError("Load last full version md5 file failed!");
                return;
            }
            if (!GenerateFullVersionFile()) {
                Debug.LogError("Generate new full version md5 file failed!");
                return;
            }

            List<string> needUpdateFileList = GenerateDiffFilesList();
            List<string> needDeleteFileList = GenerateDeleteFilesList();
            if (needUpdateFileList.Count == 0 && needDeleteFileList.Count == 0) {
                Debug.LogError("nothing need update");
                return;
            }
            // 设置更新文件
            int newVersion = VersionConfig.RES_VERSION;
            s_versionFile.ResVersion = newVersion;
            for (int i = 0; i < needUpdateFileList.Count; i++) {
                s_versionFile.FilesDic[needUpdateFileList[i]] = new VersionFileInfo(s_allFilesMd5DiffVersion[needUpdateFileList[i]], newVersion);
            }
            // 删除未使用文件
            for (int i = 0; i < needDeleteFileList.Count; i++) {
                if (s_versionFile.FilesDic.ContainsKey(needDeleteFileList[i])) {
                    s_versionFile.FilesDic.Remove(needDeleteFileList[i]);
                }
            }
            // 更新版本文件
            WriteDiffVersionFile();
            WriteFullVersionFile();

            // 导出更新文件
            ExportUpdateFiles(needUpdateFileList);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 清除版本文件
        /// </summary>
        private static void CleanFullVersionFile()
        {
            if (Directory.Exists(s_versionFullFilePath)) {
                Directory.Delete(s_versionFullFilePath, true);
            }
        }

        /// <summary>
        /// 加载资源版本文件
        /// </summary>
        /// <returns></returns>
        private static bool LoadFullVersionFile()
        {
            try {
                s_allFilesMd5LastVersion.Clear();
                string[] content = File.ReadAllLines(s_versionFullFilePath);
                if (content != null) {
                    for (int i = 0; i < content.Length; i++) {
                        if (!string.IsNullOrEmpty(content[i])) {
                            string[] kvp = content[i].Split(',');
                            if (kvp == null || kvp.Length < 2) {
                                Debug.LogError("Can not parse full md5 file with content " + content[i]);
                                return false;
                            }
                            s_allFilesMd5LastVersion[kvp[0]] = kvp[1];
                        }
                    }
                }
            } catch (Exception ex) {
                throw new Exception("Load full version file error:" + ex.Message);
            }
            return true;
        }

        /// <summary>
        /// 写新版本文件
        /// </summary>
        private static void WriteFullVersionFile()
        {
            string versionFullFolder = Path.GetDirectoryName(s_versionFullFilePath);
            if (!Directory.Exists(versionFullFolder)) {
                Directory.CreateDirectory(versionFullFolder);
            }
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in s_allFilesMd5DiffVersion) {
                string content = pair.Key + "," + pair.Value + "\n";
                sb.Append(content);
            }
            File.WriteAllText(s_versionFullFilePath, sb.ToString());
        }

        /// <summary>
        /// 生成版本MD5文件
        /// </summary>
        /// <returns></returns>
        private static bool GenerateFullVersionFile()
        {
            s_allFilesMd5DiffVersion.Clear();
            if (!Directory.Exists(s_assetBundlesPath)) {
                Debug.LogError(string.Format("AssetsPath path {0} not exist", s_assetBundlesPath));
                return false;
            }
            DirectoryInfo dir = new DirectoryInfo(s_assetBundlesPath);
            FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; ++i) {
                try {
                    if (files[i].Name.EndsWith(".meta") || files[i].Name.EndsWith(".manifest")) {
                        continue;
                    }
                    string fileMD5 = FileHelper.GetMd5Value(files[i].FullName);
                    string fileRelativePath = files[i].FullName.Replace(Path.GetFullPath(s_assetBundlesPath), "");
                    s_allFilesMd5DiffVersion[fileRelativePath] = fileMD5;
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
            foreach (KeyValuePair<string, string> pair in s_allFilesMd5DiffVersion) {
                if (s_allFilesMd5LastVersion.ContainsKey(pair.Key)) {
                    if (s_allFilesMd5LastVersion[pair.Key] == pair.Value) {
                        continue;
                    } else {
                        needUpdateFileList.Add(pair.Key);
                    }
                } else {
                    needUpdateFileList.Add(pair.Key);
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
                if (s_allFilesMd5DiffVersion.ContainsKey(kvp.Key)) {
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
        private static VersionFileModel LoadDiffVersionFile()
        {
            s_versionFile = new VersionFileModel();
            if (File.Exists(s_versionDiffFilePath)) {
                try {
                    string content = File.ReadAllText(s_versionDiffFilePath);
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
                    string assetbundleSrcPath = Path.Combine(EditorPathUtil.GetExportAssetBundlePath(), updatedFiles[i]);
                    string assetbundleDstPath = Path.Combine(EditorPathUtil.GetExportPatchPath(), updatedFiles[i]);
                    string destDir = Path.GetDirectoryName(assetbundleDstPath);
                    if (!Directory.Exists(destDir)) {
                        Directory.CreateDirectory(destDir);
                    }
                    File.Copy(assetbundleSrcPath, assetbundleDstPath, true);
                }
            }
            string versionFileSrcPath = s_versionDiffFilePath;
            string versionFileDstPath = Path.Combine(s_versionPatchPath, Path.GetFileName(versionFileSrcPath));
            string updateFileDestDir = Path.GetDirectoryName(versionFileDstPath);
            if (!Directory.Exists(updateFileDestDir)) {
                Directory.CreateDirectory(updateFileDestDir);
            }
            File.Copy(versionFileSrcPath, versionFileDstPath, true);
        }

        private static void WriteDiffVersionFile()
        {
            string str = FileHelper.ConvertVersionFileToString(s_versionFile);
            File.WriteAllText(s_versionDiffFilePath, str);
        }

        /// <summary>
        /// 清除然后构建AssetBundle
        /// </summary>
        private static void CleanBuildAllBundle()
        {
            if (Directory.Exists(s_assetBundlesPath)) {
                Directory.Delete(s_assetBundlesPath, true);
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
            //AssetTextBuilder.BuildText();
        }
    }
}