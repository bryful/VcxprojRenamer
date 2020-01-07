using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VcxprojRenamer
{
    

    public class CTargetExt
    {
        public string ItemsText
        {
            get
            {
                return string.Join(";", m_Items);
            }
            set
            {
                if (value.Length <= 0) return;
                string[] p = value.Split(';');
                m_Items.Clear();
                foreach(string s in p)
                {
                    AddExt(s.Trim());
                }
            }
        }
        public string[] Exts
        {
            get { return m_Items.ToArray(); }
            set
            {
                if (value.Length <= 0) return;
                m_Items.Clear();
                foreach (string s in value)
                {
                    m_Items.Add(s);
                }
            }
        }
        private List<string> m_Items = new List<string>();
        public CTargetExt()
        {
            Init();
        }
        public int IndexOfExt(string e)
        {
            int ret = -1;
            if (m_Items.Count <= 0) return ret;
            string e2 = e.ToLower();
            for (int i=0; i<m_Items.Count; i++)
            {
                if(m_Items[i] == e2)
                {
                    ret = i;
                    break;
                }
            }
            return ret;
        }
        public void AddExt(string e)
        {
            if (e == "") return; 
            int idx = IndexOfExt(e);
            if (idx >= 0) return;
            m_Items.Add(e.ToLower());
        }
        public void Clear() { m_Items.Clear(); }
        public void Init()
        {
            Clear();
            AddExt(".c");
            AddExt(".cpp");
            AddExt(".cpp");
            AddExt(".h");
            AddExt(".r");
            AddExt(".vcxproj");
            AddExt(".filters");
            AddExt(".plist");
            AddExt(".pbxproj");
            AddExt(".mode1v3");
            AddExt(".pbxuser");
            AddExt(".xcworkspacedata");
            AddExt(".xcuserstate");
            AddExt(".xcsettings");
            AddExt(".xcscheme");
        }

    }
}
