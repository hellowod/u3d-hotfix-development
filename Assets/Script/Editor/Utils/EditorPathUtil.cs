using Framework;
using UnityEditor;

public class EditorPathUtil
{
    private static string RootPath = string.Format("{0}/{1}/{2}", "Bin/export", VersionConfig.APP_VERSION, EditorBaseUtil.GetBuildTarget().ToString().ToLower());

    /// <summary>
    /// 获取导出项目路径
    /// </summary>
    /// <returns></returns>
    public static string GetExportProjectPath()
    {
        BuildTarget target = EditorBaseUtil.GetBuildTarget();
        if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64) {
            return string.Format("{0}/app/{1}.exe", RootPath, PlayerSettings.productName);
        } else if (target == BuildTarget.StandaloneOSX) {
            return string.Format("{0}/app/{1}.app", RootPath, PlayerSettings.productName);
        } else {
            return string.Format("{0}/app/{1}", RootPath, PlayerSettings.productName);
        }
    }

    /// <summary>
    /// 获取assetbundle导出路径
    /// </summary>
    /// <returns></returns>
    public static string GetExportAssetBundlePath()
    {
        return string.Format("{0}/bundle/", RootPath);
    }

    /// <summary>
    /// 获取导出项目路径
    /// </summary>
    /// <returns></returns>
    public static string GetExportVersionMD5Path()
    {
        return string.Format("{0}/version/VersionMD5File.txt", RootPath);
    }

    /// <summary>
    /// 获取导出项目路径
    /// </summary>
    /// <returns></returns>
    public static string GetExportVersionPath()
    {
        return string.Format("{0}/version/{1}", RootPath, VersionConfig.VERSION_FILE_NAME);
    }

    /// <summary>
    /// 获取导出补丁的路径
    /// </summary>
    /// <returns></returns>
    public static string GetExportPatchPath()
    {
        return string.Format("{0}/patch/{1}", RootPath, VersionConfig.RES_VERSION);
    }
}
