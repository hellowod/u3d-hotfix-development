using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Framework
{
    public class LoaderTest : MonoBehaviour
    {
        private string m_assetbundlePath = "";

        private string m_sceneName = "scene1";
        private string m_prefabName = "Book_01";
        private string m_textName = "Test.txt";
        
        private void OnGUI()
        {
            m_sceneName = GUI.TextField(new Rect(0, 0, Screen.width, 100), m_sceneName);
            if (GUI.Button(new Rect(0, 100, Screen.width, 200), "LoadScene")) {
                string sceneName = m_sceneName;
                Debug.LogFormat("Load Scene Name= {0}", sceneName);
                SceneManager.LoadScene(sceneName);
            }

            m_prefabName = GUI.TextField(new Rect(0, 300 + 10, Screen.width, 100), m_prefabName);
            if (GUI.Button(new Rect(0, 400, Screen.width, 200), "LoadPrefab")) {
                if (!string.IsNullOrEmpty(m_prefabName)) {
                    string prefabName = string.Format("Prefab/{0}", m_prefabName);
                    Debug.LogFormat("Load Prefab Name= {0}", prefabName);
                    SimpleLoader.LoadPrefab(prefabName);
                }
            }

            m_textName = GUI.TextField(new Rect(0, 600 + 10, Screen.width, 100), m_textName);
            if (GUI.Button(new Rect(0, 700, Screen.width, 200), "LoadText")) {
                string textName = string.Format("Config/{0}", m_textName);
                Debug.LogFormat("Load Text Name= {0}", textName);
                string content = SimpleLoader.LoadText(textName);
            }
        }
    }
}