using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

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
            return string.Format("Bin/Export/{0}/{1}.exe", target.ToString(), target.ToString());
        } else if (target == BuildTarget.StandaloneOSX) {
            return string.Format("Bin/Export/{0}/{1}.app", target.ToString(), target.ToString());
        } else {
            return string.Format("Bin/Export/{0}/", target.ToString());
        }
    }
}
