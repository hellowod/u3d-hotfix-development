using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework
{
    public class VersionFileInfo
    {
        private string m_md5;
        private string m_version;

        public string md5
        {
            get {
                return m_md5;
            }
        }

        public string version
        {
            get {
                return m_version;
            }
        }

        public VersionFileInfo(string md5Val, string versionVal)
        {
            m_md5 = md5Val;
            m_version = versionVal;
        }
    }

    public class VersionFileModel
    {
        public Dictionary<string, VersionFileInfo> files = new Dictionary<string, VersionFileInfo>();
        public string resVersion = "0.0";
    }
}
