using UnityEngine;
using System.Collections;

namespace Framework
{
    public class Launcher : MonoBehaviour
    {
        private void Start()
        {
            VersionMgr versionMgr = new VersionMgr();
            versionMgr.StartCheckVersion();
        }

        private void Update()
        {

        }
    }
}