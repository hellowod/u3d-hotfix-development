using UnityEngine;
using System.Collections;

namespace Framework
{
    public class Launcher : MonoBehaviour
    {
        private VersionMgr versionMgr;

        private void Start()
        {
            versionMgr = new VersionMgr();
            versionMgr.StartCheckVersion();
        }

        private void Update()
        {
            if (versionMgr != null) {
                VersionMgr.Instance.Update();
            }
        }
    }
}