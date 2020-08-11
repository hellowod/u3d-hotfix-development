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
        private int m_version;

        public string md5
        {
            get {
                return m_md5;
            }
        }

        public int version
        {
            get {
                return m_version;
            }
        }

        public VersionFileInfo(string md5Val, int versionVal)
        {
            m_md5 = md5Val;
            m_version = versionVal;
        }
    }

    public class VersionFileModel
    {
        public int ResVersion = 0;
        public Dictionary<string, VersionFileInfo> FilesDic;

        public VersionFileModel()
        {
            FilesDic = new Dictionary<string, VersionFileInfo>();
        }
    }
}
