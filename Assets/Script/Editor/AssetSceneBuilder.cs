using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace Framework
{
    public static class AssetSceneBuilder
    {
        public static string s_assetBundlePath = EditorPathUtil.GetExportAssetBundlePath();

        [MenuItem("Tool/Builder/Build Scene")]
        public static void BuildScenesAssetBundles()
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
    }
}
