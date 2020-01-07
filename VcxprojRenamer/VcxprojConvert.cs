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
        /// <summary>
        /// 文字コードを判別する
        /// </summary>
        /// <remarks>
        /// Jcode.pmのgetcodeメソッドを移植したものです。
        /// Jcode.pm(http://openlab.ring.gr.jp/Jcode/index-j.html)
        /// Jcode.pmの著作権情報
        /// Copyright 1999-2005 Dan Kogai <dankogai@dan.co.jp>
        /// This library is free software; you can redistribute it and/or modify it
        ///  under the same terms as Perl itself.
        /// </remarks>
        /// <param name="bytes">文字コードを調べるデータ</param>
        /// <returns>適当と思われるEncodingオブジェクト。
        /// 判断できなかった時はnull。</returns>
        public Encoding GetCode(byte[] bytes)
        {
            const byte bEscape = 0x1B;
            const byte bAt = 0x40;
            const byte bDollar = 0x24;
            const byte bAnd = 0x26;
            const byte bOpen = 0x28;    //'('
            const byte bB = 0x42;
            const byte bD = 0x44;
            const byte bJ = 0x4A;
            const byte bI = 0x49;

            int len = bytes.Length;
            byte b1, b2, b3, b4;

            //Encode::is_utf8 は無視

            bool isBinary = false;
            for (int i = 0; i < len; i++)
            {
                b1 = bytes[i];
                if (b1 <= 0x06 || b1 == 0x7F || b1 == 0xFF)
                {
                    //'binary'
                    isBinary = true;
                    if (b1 == 0x00 && i < len - 1 && bytes[i + 1] <= 0x7F)
                    {
                        //smells like raw unicode
                        return System.Text.Encoding.Unicode;
                    }
                }
            }
            if (isBinary)
            {
                return null;
            }

            //not Japanese
            bool notJapanese = true;
            for (int i = 0; i < len; i++)
            {
                b1 = bytes[i];
                if (b1 == bEscape || 0x80 <= b1)
                {
                    notJapanese = false;
                    break;
                }
            }
            if (notJapanese)
            {
                return System.Text.Encoding.ASCII;
            }

            for (int i = 0; i < len - 2; i++)
            {
                b1 = bytes[i];
                b2 = bytes[i + 1];
                b3 = bytes[i + 2];

                if (b1 == bEscape)
                {
                    if (b2 == bDollar && b3 == bAt)
                    {
                        //JIS_0208 1978
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    }
                    else if (b2 == bDollar && b3 == bB)
                    {
                        //JIS_0208 1983
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    }
                    else if (b2 == bOpen && (b3 == bB || b3 == bJ))
                    {
                        //JIS_ASC
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    }
                    else if (b2 == bOpen && b3 == bI)
                    {
                        //JIS_KANA
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    }
                    if (i < len - 3)
                    {
                        b4 = bytes[i + 3];
                        if (b2 == bDollar && b3 == bOpen && b4 == bD)
                        {
                            //JIS_0212
                            //JIS
                            return System.Text.Encoding.GetEncoding(50220);
                        }
                        if (i < len - 5 &&
                            b2 == bAnd && b3 == bAt && b4 == bEscape &&
                            bytes[i + 4] == bDollar && bytes[i + 5] == bB)
                        {
                            //JIS_0208 1990
                            //JIS
                            return System.Text.Encoding.GetEncoding(50220);
                        }
                    }
                }
            }

            //should be euc|sjis|utf8
            //use of (?:) by Hiroki Ohzaki <ohzaki@iod.ricoh.co.jp>
            int sjis = 0;
            int euc = 0;
            int utf8 = 0;
            for (int i = 0; i < len - 1; i++)
            {
                b1 = bytes[i];
                b2 = bytes[i + 1];
                if (((0x81 <= b1 && b1 <= 0x9F) || (0xE0 <= b1 && b1 <= 0xFC)) &&
                    ((0x40 <= b2 && b2 <= 0x7E) || (0x80 <= b2 && b2 <= 0xFC)))
                {
                    //SJIS_C
                    sjis += 2;
                    i++;
                }
            }
            for (int i = 0; i < len - 1; i++)
            {
                b1 = bytes[i];
                b2 = bytes[i + 1];
                if (((0xA1 <= b1 && b1 <= 0xFE) && (0xA1 <= b2 && b2 <= 0xFE)) ||
                    (b1 == 0x8E && (0xA1 <= b2 && b2 <= 0xDF)))
                {
                    //EUC_C
                    //EUC_KANA
                    euc += 2;
                    i++;
                }
                else if (i < len - 2)
                {
                    b3 = bytes[i + 2];
                    if (b1 == 0x8F && (0xA1 <= b2 && b2 <= 0xFE) &&
                        (0xA1 <= b3 && b3 <= 0xFE))
                    {
                        //EUC_0212
                        euc += 3;
                        i += 2;
                    }
                }
            }
            for (int i = 0; i < len - 1; i++)
            {
                b1 = bytes[i];
                b2 = bytes[i + 1];
                if ((0xC0 <= b1 && b1 <= 0xDF) && (0x80 <= b2 && b2 <= 0xBF))
                {
                    //UTF8
                    utf8 += 2;
                    i++;
                }
                else if (i < len - 2)
                {
                    b3 = bytes[i + 2];
                    if ((0xE0 <= b1 && b1 <= 0xEF) && (0x80 <= b2 && b2 <= 0xBF) &&
                        (0x80 <= b3 && b3 <= 0xBF))
                    {
                        //UTF8
                        utf8 += 3;
                        i += 2;
                    }
                }
            }
            //M. Takahashi's suggestion
            //utf8 += utf8 / 2;

            System.Diagnostics.Debug.WriteLine(
                string.Format("sjis = {0}, euc = {1}, utf8 = {2}", sjis, euc, utf8));
            if (euc > sjis && euc > utf8)
            {
                //EUC
                return System.Text.Encoding.GetEncoding(51932);
            }
            else if (sjis > euc && sjis > utf8)
            {
                //SJIS
                return System.Text.Encoding.GetEncoding(932);
            }
            else if (utf8 > euc && utf8 > sjis)
            {
                //UTF8
                return System.Text.Encoding.UTF8;
            }

            return null;
        }

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
        private bool LoadTextAtBin(string p,out string str, out Encoding enc)
        {
            bool ret = false;
            str = "";
            enc = Encoding.UTF8;

            if (File.Exists(p) == false) return ret;

            byte[] bs = new byte[0];
            FileStream fs = new System.IO.FileStream(p,System.IO.FileMode.Open,System.IO.FileAccess.Read);
            try
            {
                bs = new byte[fs.Length];
                int len = fs.Read(bs, 0, bs.Length);
                if (len != bs.Length) return ret;
            }
            catch
            {
                ret = false;
                return ret;
            } finally{
                fs.Close();
            }

            try
            {
                enc = GetCode(bs);
                str = enc.GetString(bs);
                ret = true;
            }
            catch
            {
                ret = false;
            }
            return ret;
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
                if(LoadTextAtBin(p,out str, out enc)==true)
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
    }
}
