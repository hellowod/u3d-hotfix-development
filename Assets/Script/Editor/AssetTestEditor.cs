using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AssetTestEditor : Editor
{
    [MenuItem("Assets/Test")]
    private static void GetDependend()
    {
        Object selectedObj = Selection.activeObject;
        Debug.LogError(selectedObj.name);
        string[] depends = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(selectedObj));
        for (int i = 0; i < depends.Length; i++) {
            Debug.LogError(depends[i]);
        }
    }
}
