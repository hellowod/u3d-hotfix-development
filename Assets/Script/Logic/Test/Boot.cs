using UnityEngine;
using System.Collections;

namespace Framework
{
    public class Boot : MonoBehaviour
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