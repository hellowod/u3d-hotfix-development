using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

namespace Framework
{
    public class AssetTextBuilder : Editor
    {
        //存放文本文件的文件夹
        public static List<string> s_textFolder = new List<string>()
        {
            "Art/Config",
        };

        [MenuItem("Tool/Builder/BuildText")]
        public static void BuildText()
        {
            for (int i = 0; i < s_textFolder.Count; i++) {
                string path = Application.dataPath + "/" + s_textFolder[i];
                if (!Directory.Exists(path)) {
                    Debug.LogError(string.Format("textFolder {0} not exist", s_textFolder[i]));
                } else {
                    DirectoryInfo dir = new DirectoryInfo(path);
                    FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories);
                    for (int j = 0; j < files.Length; j++) {
                        if (CheckFileSuffixNeedIgnore(files[j].Name)) {
                            continue;
                        }
                        string relativePath = GetRelativePathToAssets(files[j].FullName).Replace('\\', '/');
                        string exportPath = string.Format("{0}/{1}", Application.streamingAssetsPath, relativePath).Replace('\\', '/');
                        string exportPathDir = exportPath.Substring(0, exportPath.LastIndexOf("/"));
                        if (!Directory.Exists(exportPathDir)) {
                            Directory.CreateDirectory(exportPathDir);
                        }
                        files[j].CopyTo(exportPath, true);
                    }
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static string GetRelativePathToAssets(string path)
        {
            return path.Substring(Application.dataPath.Length);
        }

        private static bool CheckFileSuffixNeedIgnore(string fileName)
        {
            if (fileName.EndsWith(".meta") || fileName.EndsWith(".DS_Store")) {
                return true;
            }
            return false;
        }
    }
}