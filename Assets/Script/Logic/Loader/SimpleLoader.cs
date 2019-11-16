using UnityEngine;
using System.Collections;
using System.IO;

namespace Framework
{
    public class SimpleLoader
    {
        private static string RES_ROOT_PATH = Application.dataPath + "/Art/";

        /// <summary>
        /// 通用加载接口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T Load<T>(string path) where T : Object
        {
#if UNITY_EDITOR && !LOAD_ASSETBUNDLE_INEDITOR
            path = string.Format("{0}/{1}", "Assets/Art", path);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
#else
		return AssetbundleLoader.LoadRes<T>(path);
#endif
        }

        /// <summary>
        /// 加载Prefab
        /// </summary>
        /// <param name="path"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static GameObject LoadPrefab(string path, string suffix = ".prefab")
        {
            GameObject go = Load<GameObject>(path + suffix);
            if (go != null) {
                return GameObject.Instantiate(go);
            }
            return null;
        }

        /// <summary>
        /// 加载文本对象
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string LoadText(string path)
        {
#if UNITY_EDITOR && !LOAD_ASSETBUNDLE_INEDITOR
            path = string.Format("{0}/{1}", RES_ROOT_PATH, path);
#else
		if (VersionMgr.Instance.CheckFileIsInVersionFile(path)) {
			path = string.Format("{0}/{1}", AssetbundleLoader.Download_Path, path) ;
		} else {
			path = string.Format("{0}/{1}", AssetbundleLoader.ROOT_PATH, path);
		}
#endif
            if (File.Exists(path)) {
                return File.ReadAllText(path);
            } else {
                Debug.Log(string.Format("{0} not exist", path));
                return null;
            }
        }
    }
}