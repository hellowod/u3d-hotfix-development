using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

namespace Framework
{
    /// <summary>
    /// 资源节点
    /// </summary>
    public class AssetNode
    {
        public List<AssetNode> parents = new List<AssetNode>();
        public string path;
        public int depth = 0;
    }

    public class AssetBundleBuilder : Editor
    {
        //需要打包的资源路径（相对于Assets目录），通常是prefab,lua,及其他数据。（贴图，动画，模型，材质等可以通过依赖自己关联上，不需要添加在该路径里，除非是特殊需要）
        //注意这里是目录，单独零散的文件，可以新建一个目录，都放在里面打包
        public static List<string> s_abResourcePath = new List<string>()
        {
            "Art/Prefab",
        };

        public static string s_assetBundlePath = EditorUtils.GetExportProjectPath("assetbundle");

        private static List<AssetNode> s_leafAssetNodes = new List<AssetNode>();
        private static Dictionary<string, AssetNode> s_allAssetNodes = new Dictionary<string, AssetNode>();
        private static List<string> s_buildMap = new List<string>();

        [MenuItem("Tool/Builder/BuildScene")]
        private static void BuildScenesAssetBundles()
        {
            string outPath = s_assetBundlePath + "/Scenes/";
            if (Directory.Exists(outPath)) {
            } else {
                DirectoryInfo newDirect = Directory.CreateDirectory(outPath);
            }
            //获取buildsetting里面enable的场景
            foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes) {
                if (e == null) {
                    continue;
                }
                if (e.enabled) {
                    string levelName = e.path.Substring(e.path.LastIndexOf("/") + 1);
                    BuildPipeline.BuildPlayer(new string[1] { e.path }, outPath + levelName, EditorUserBuildSettings.activeBuildTarget, BuildOptions.BuildAdditionalStreamedScenes);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Tool/Builder/BuildAssetbundle")]
        public static void BuildAssetBundle()
        {
            Init();
            CollectDependcy();
            BuildResourceBuildMap();
            BuildAssetBundleWithBuildMap();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void Init()
        {
            s_buildMap.Clear();
            s_leafAssetNodes.Clear();
            s_allAssetNodes.Clear();
        }

        /// <summary>
        /// 收集资源依赖
        /// </summary>
        private static void CollectDependcy()
        {
            for (int i = 0; i < s_abResourcePath.Count; i++) {
                string path = Application.dataPath + "/" + s_abResourcePath[i];
                if (!Directory.Exists(path)) {
                    Debug.LogError(string.Format("abResourcePath {0} not exist", s_abResourcePath[i]));
                } else {
                    DirectoryInfo dir = new DirectoryInfo(path);
                    FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories);
                    for (int j = 0; j < files.Length; j++) {
                        FileInfo fileInfo = files[j];
                        if (CheckFileSuffixNeedIgnore(fileInfo.Name)) {
                            continue;
                        }
                        string fileRelativePath = GetReleativeToAssets(fileInfo.FullName);
                        AssetNode root = null;
                        // 文件可能在上一个文件的依赖关系中被处理了
                        if (!s_allAssetNodes.TryGetValue(fileRelativePath, out root)) {
                            root = new AssetNode();
                            root.path = fileRelativePath;
                            s_allAssetNodes[root.path] = root;
                            GetDependcyRecursive(fileRelativePath, root);
                        }
                    }
                }
            }
            //PrintDependcy();
        }

        /// <summary>
        /// 递归查找所有资源包依赖文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parentNode"></param>
        private static void GetDependcyRecursive(string path, AssetNode parentNode)
        {
            string[] dependcy = AssetDatabase.GetDependencies(path, false);
            for (int i = 0; i < dependcy.Length; i++) {
                AssetNode node = null;
                if (!s_allAssetNodes.TryGetValue(dependcy[i], out node)) {
                    node = new AssetNode();
                    node.path = dependcy[i];
                    node.depth = parentNode.depth + 1;
                    node.parents.Add(parentNode);
                    s_allAssetNodes[node.path] = node;
                    GetDependcyRecursive(dependcy[i], node);
                } else {
                    if (!node.parents.Contains(parentNode)) {
                        node.parents.Add(parentNode);
                    }
                    if (node.depth < parentNode.depth + 1) {
                        node.depth = parentNode.depth + 1;
                        GetDependcyRecursive(dependcy[i], node);
                    }
                }
            }
            if (dependcy.Length == 0) {
                if (!s_leafAssetNodes.Contains(parentNode)) {
                    s_leafAssetNodes.Add(parentNode);
                }
            }
        }

        /// <summary>
        /// 按照依赖关系的深度，从最底层往上遍历打包，如果一个叶子节点有多个父节点，则该叶子节点被多个资源依赖，该叶子节点需要打包，如果一个节点没有
        /// 父节点，则该叶子节点是最顶层的文件（prefab,lua...），需要打包。
        /// </summary>
        private static void BuildResourceBuildMap()
        {
            int maxDepth = GetMaxDepthOfLeafNodes();
            while (s_leafAssetNodes.Count > 0) {
                List<AssetNode> curDepthNodesList = new List<AssetNode>();
                for (int i = 0; i < s_leafAssetNodes.Count; i++) {
                    if (s_leafAssetNodes[i].depth == maxDepth) {
                        //如果叶子节点有多个父节点或者没有父节点,打包该叶子节点
                        if (s_leafAssetNodes[i].parents.Count != 1) {
                            if (!ShouldIgnoreFile(s_leafAssetNodes[i].path)) {
                                s_buildMap.Add(s_leafAssetNodes[i].path);
                            }
                        }
                        curDepthNodesList.Add(s_leafAssetNodes[i]);
                    }
                }
                //删除已经遍历过的叶子节点，并把这些叶子节点的父节点添加到新一轮的叶子节点中
                for (int i = 0; i < curDepthNodesList.Count; i++) {
                    s_leafAssetNodes.Remove(curDepthNodesList[i]);
                    foreach (AssetNode node in curDepthNodesList[i].parents) {
                        if (!s_leafAssetNodes.Contains(node)) {
                            s_leafAssetNodes.Add(node);
                        }
                    }
                }
                maxDepth -= 1;
            }
        }

        private static bool ShouldIgnoreFile(string path)
        {
            if (path.EndsWith(".cs")) {
                return true;
            }
            return false;
        }

        private static int GetMaxDepthOfLeafNodes()
        {
            if (s_leafAssetNodes.Count == 0) {
                return 0;
            }
            s_leafAssetNodes.Sort((x, y) => {
                return y.depth - x.depth;
            });
            return s_leafAssetNodes[0].depth;
        }

        private static void BuildAssetBundleWithBuildMap()
        {
            string prefix = "Assets";
            AssetBundleBuild[] buildMapArray = new AssetBundleBuild[s_buildMap.Count];
            for (int i = 0; i < s_buildMap.Count; i++) {
                buildMapArray[i].assetBundleName = s_buildMap[i].Substring(prefix.Length + 1);
                buildMapArray[i].assetNames = new string[] { s_buildMap[i] };
                Debug.Log(s_buildMap[i]);
            }
            if (!Directory.Exists(s_assetBundlePath)) {
                Directory.CreateDirectory(s_assetBundlePath);
            }
            BuildPipeline.BuildAssetBundles(s_assetBundlePath, buildMapArray, BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle, EditorUserBuildSettings.activeBuildTarget);
        }

        /// <summary>
        /// 获取文件相对Assets路径
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        private static string GetReleativeToAssets(string fullName)
        {
            //获得在文件在Assets下的目录，类似于Assets/path/of/yourfile
            string fileRelativePath = fullName.Substring(Application.dataPath.Length - 6);
            //如果在windows平台下运行，需要替换路径中的\为/;
            fileRelativePath = fileRelativePath.Replace("\\", "/");
            return fileRelativePath;
        }

        /// <summary>
        /// 建筑忽略文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static bool CheckFileSuffixNeedIgnore(string fileName)
        {
            if (fileName.EndsWith(".meta") ||
                fileName.EndsWith(".DS_Store") ||
                fileName.EndsWith(".cs")) {
                return true;
            }
            return false;
        }
    }
}