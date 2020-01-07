using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VcxprojRenamer
{
    public class VcxprojConvert
    {
        // **********************************************************************
        private List<string> m_TargetFiles = null;
        public List<string> TargetFiles
        {
            get { return m_TargetFiles; }
            set { m_TargetFiles = value; }
        }
        // **********************************************************************
        private string m_SrcWord = "";
        public string SrcWord
        {
            get { return m_SrcWord; }
            set
            {
                m_SrcWord = value.Trim();
            }
        }
        private string m_DstWord = "";
        public string DstWord
        {
            get { return m_DstWord; }
            set
            {
                m_DstWord = value.Trim();
            }
        }
        // **********************************************************************
        public bool CanExec
        {
            get
            {
                return ((m_SrcWord != "") && (m_SrcWord != m_DstWord));
            }
        }

        // **********************************************************************
        public VcxprojConvert()
        {
        }
        // **********************************************************************
        private string RepWord(string str)
        {
            string ret = str;
            if (CanExec == false) return ret;
            ret = str.Replace(m_SrcWord, m_DstWord);
            return ret;
        }
        // **********************************************************************
        public string NewPath(string p)
        {
            string d = Path.GetDirectoryName(p);
            string n = Path.GetFileNameWithoutExtension(p);
            string e = Path.GetExtension(p);

            n = n.Replace(m_SrcWord, m_DstWord);

            string p2 = Path.Combine(d, n + e);
            return p2;
        }
        // **********************************************************************
        private bool Export(string p, string str, Encoding enc)
        {
            bool ret = false;
            if (CanExec == false) return ret;
            if (p == "") return ret;
            try
            {
                File.WriteAllText(p, str, enc);

                string p2 = NewPath(p);
                if (p!=p2)
                {
                    if (File.Exists(p2)) File.Delete(p2);
                    File.Move(p, p2);
                }
                ret = true;
            }
            catch
            {
                ret = false;
            }
            return ret;

        }
        // **********************************************************************
        public string ConvertExec(out string stat)
        {
            string ret = "";
            stat = "Target Error!";
            if (m_TargetFiles == null) return ret;
            if (m_TargetFiles.Count <= 0) return ret;
            stat = "";



            string p1 = Path.GetDirectoryName(m_TargetFiles[0]);
            string p2 = Path.GetDirectoryName(p1);
            string n2 = Path.GetFileName(p1);
            n2 = n2.Replace(m_SrcWord, m_DstWord);
            p2 = Path.Combine(p2, n2);


            foreach (string p in m_TargetFiles)
            {
                string str = "";
                Encoding enc = Encoding.UTF8;
                str = ReadTextFile(p, out enc);
                if ((str!=null)&&(enc!=null))
                {
                    str = RepWord(str);
                    if( Export(p,str,enc))
                    {
                    }
                }
            }
            ret = p2;
            if (p1!=p2)
            {
                try
                {
                    if (Directory.Exists(p2)) Directory.Delete(p2, true);
                    Directory.Move(p1, p2);
                }
                catch
                {
                    ret = p1;
                    stat = "Parent Folder Rename Error!";
                }
            }
            return ret;
        }
        // **********************************************************************
        /// <summary>
        /// 自動的にエンコード方式を判定してテキストファイルを読み込みます。
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <param name="enc">エンコード方式</param>
        /// <returns>読み込んだ文字列</returns>
        private string ReadTextFile(string path, out Encoding enc)
        {
            var replacement = new DecoderReplacementFallback("�[FALLBACK]");
            Func<int, Encoding> CodePageをEncodingに = (cp) => {
                var encoding = (Encoding)Encoding.GetEncoding(cp).Clone();
                encoding.DecoderFallback = replacement;
                return encoding;
            };
            int[] aryCP = { 65001, 932, 1200, 1201, 51932 };
            int minLength = int.MaxValue;
            string result = null;
            enc = null;
            byte[] bytes = File.ReadAllBytes(path);
            foreach (var codepage in aryCP)
            {
                var encoding = CodePageをEncodingに(codepage);
                string s = encoding.GetString(bytes);
                int length = Encoding.UTF8.GetByteCount(s);
                if (length < minLength)
                {
                    minLength = length;
                    result = s;
                    enc = encoding;
                }
            }
            return result;
        }
    }
}
