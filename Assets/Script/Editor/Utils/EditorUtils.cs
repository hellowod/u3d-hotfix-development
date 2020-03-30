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
    public static string GetExportProjectPath()
    {
        BuildTarget target = GetBuildTarget();
        if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64) {
            return string.Format("Bin/export/{0}/application/{1}.exe", target.ToString(), PlayerSettings.productName);
        } else if (target == BuildTarget.StandaloneOSX) {
            return string.Format("Bin/export/{0}/application/{1}.app", target.ToString(), PlayerSettings.productName);
        } else {
            return string.Format("Bin/export/{0}/application/{1}/", target.ToString(), PlayerSettings.productName);
        }
    }

    /// <summary>
    /// 获取导出项目路径
    /// </summary>
    /// <returns></returns>
    public static string GetExportBundlePath()
    {
        BuildTarget target = GetBuildTarget();
        return string.Format("Bin/export/{0}/assetbundle/", target.ToString());
    }

    /// <summary>
    /// 获取导出项目路径
    /// </summary>
    /// <returns></returns>
    public static string GetExportVersionMD5Path()
    {
        BuildTarget target = GetBuildTarget();
        return string.Format("Bin/export/{0}/versionfile/{1}", target.ToString(), "VersionMD5File.txt");
    }

    /// <summary>
    /// 获取导出项目路径
    /// </summary>
    /// <returns></returns>
    public static string GetExportVersionPath()
    {
        BuildTarget target = GetBuildTarget();
        return string.Format("Bin/export/{0}/versionfile/{1}", target.ToString(), VersionConfig.s_versionFileName);
    }

    /// <summary>
    /// 获取导出补丁的路径
    /// </summary>
    /// <returns></returns>
    public static string GetExportPatchPath()
    {
        BuildTarget target = GetBuildTarget();
        return string.Format("Bin/export/{0}/patch/{1}/{2}", target.ToString(), VersionConfig.s_appVersion, VersionConfig.s_resVersion);
    }
}
