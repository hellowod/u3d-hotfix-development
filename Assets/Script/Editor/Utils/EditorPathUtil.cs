using Framework;
using UnityEditor;

public class EditorPathUtil
{
    private static string RootPath = string.Format("{0}/{1}/{2}", "Bin/export/version", VersionConfig.APP_VERSION, EditorBaseUtil.GetBuildTarget().ToString().ToLower());

    /// <summary>
    /// 获取项目路径
    /// </summary>
    /// <returns></returns>
    public static string GetProjectPath()
    {
        return "Bin/export/";
    }

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
    /// 获取导出工作空间路径
    /// </summary>
    /// <returns></returns>
    public static string GetExportWorkingPath()
    {
        return string.Format("{0}/working", GetProjectPath());
    }

    /// <summary>
    /// 获取AssetBundle路径
    /// </summary>
    /// <returns></returns>
    public static string GetExportAssetBundlePath()
    {
        return string.Format("{0}/bundle/{1}", RootPath, VersionConfig.RES_VERSION);
    }

    /// <summary>
    /// 获取项目导出生成包版本路径
    /// </summary>
    /// <returns></returns>
    public static string GetExportVersionFullFilePath()
    {
        return string.Format("{0}/config/VersionFullFile.txt", RootPath);
    }

    /// <summary>
    /// 获取最新的版本文件路径
    /// </summary>
    /// <returns></returns>
    public static string GetExportVersionDiffFilePath()
    {
        return string.Format("{0}/config/{1}", RootPath, VersionConfig.VERSION_DIFF_FILE);
    }

    /// <summary>
    /// 获取导出补丁的路径
    /// </summary>
    /// <returns></returns>
    public static string GetExportPatchPath()
    {
        return string.Format("{0}/patch/{1}_{2}", RootPath, VersionConfig.RES_VERSION, (VersionConfig.RES_VERSION - 1));
    }
}
