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

    private static string RootPath = "Bin/export";

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
    public static string GetExportProjectPath()
    {
        BuildTarget target = GetBuildTarget();
        if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64) {
            return string.Format("{0}/{1}/{2}/app/{3}.exe", RootPath, VersionConfig.APP_VERSION, target.ToString().ToLower(), PlayerSettings.productName);
        } else if (target == BuildTarget.StandaloneOSX) {
            return string.Format("{0}/{1}/{2}/app/{3}.app", RootPath, VersionConfig.APP_VERSION, target.ToString().ToLower(), PlayerSettings.productName);
        } else {
            return string.Format("{0}/{1}/{2}/app/{3}", RootPath, VersionConfig.APP_VERSION, target.ToString().ToLower(), PlayerSettings.productName);
        }
    }

    /// <summary>
    /// 获取导出项目路径
    /// </summary>
    /// <returns></returns>
    public static string GetExportBundlePath()
    {
        BuildTarget target = GetBuildTarget();
        return string.Format("{0}/{1}/{2}/bundle/", RootPath, VersionConfig.APP_VERSION, target.ToString().ToLower());
    }

    /// <summary>
    /// 获取导出项目路径
    /// </summary>
    /// <returns></returns>
    public static string GetExportVersionMD5Path()
    {
        BuildTarget target = GetBuildTarget();
        return string.Format("{0}/{1}/{2}/version/VersionMD5File.txt", RootPath, VersionConfig.APP_VERSION, target.ToString().ToLower());
    }

    /// <summary>
    /// 获取导出项目路径
    /// </summary>
    /// <returns></returns>
    public static string GetExportVersionPath()
    {
        BuildTarget target = GetBuildTarget();
        return string.Format("{0}/{1}/{2}/version/{3}", RootPath, VersionConfig.APP_VERSION, target.ToString().ToLower(), VersionConfig.VERSION_FILE_NAME);
    }

    /// <summary>
    /// 获取导出补丁的路径
    /// </summary>
    /// <returns></returns>
    public static string GetExportPatchPath()
    {
        BuildTarget target = GetBuildTarget();
        return string.Format("{0}/{1}/{2}/patch/{3}", RootPath, VersionConfig.APP_VERSION, target.ToString().ToLower(), VersionConfig.RES_VERSION);
    }
}
