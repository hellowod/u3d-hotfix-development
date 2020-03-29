using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class EditorUtils
{
    /// <summary>
    /// 获取目标平台
    /// </summary>
    /// <returns></returns>
    public static BuildTarget GetBuildTarget()
    {
        BuildTarget buildTarget = BuildTarget.NoTarget;
#if UNITY_IOS
        buildTarget = BuildTarget.iOS;
#elif UNITY_ANDROID
        buildTarget = BuildTarget.Android;
#elif UNITY_STANDALONE_WIN
        buildTarget = BuildTarget.StandaloneWindows;
#elif UNITY_STANDALONE_OSX
        buildTarget = BuildTarget.StandaloneOSX;
#endif
        return buildTarget;
    }

    /// <summary>
    /// 获取导出项目路径
    /// </summary>
    /// <returns></returns>
    public static string GetExportProjectPath(string type)
    {
        BuildTarget target = GetBuildTarget();
        if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64) {
            return string.Format("Bin/export/{0}/{1}/{2}.exe", target.ToString(), type, PlayerSettings.productName);
        } else if (target == BuildTarget.StandaloneOSX) {
            return string.Format("Bin/export/{0}/{1}/{2}.app", target.ToString(), type, PlayerSettings.productName);
        } else {
            return string.Format("Bin/export/{0}/{1}/{2}/", target.ToString(), type, PlayerSettings.productName);
        }
    }

    /// <summary>
    /// 获取导出补丁的路径
    /// </summary>
    /// <returns></returns>
    public static string GetExportPatchPath(string appVersion, string resVersion)
    {
        return string.Format("{0}/{1}/{2}/{3}/{4}/{5}", 
            Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/")), 
            "Bin/export", 
            PathHelper.GetPlatformName().Replace("/", ""),
            "patch",
            appVersion,
            resVersion);
    }
}
