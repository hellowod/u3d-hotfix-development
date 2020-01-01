using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AssetTestEditor : Editor
{
    [MenuItem("Assets/Test")]
    private static void GetDependend()
    {
        Debug.LogError(EditorUtils.GetExportPatchPath("1.0", "1"));
    }
}
