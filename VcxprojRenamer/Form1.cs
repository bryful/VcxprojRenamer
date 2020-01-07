using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using System.Design;

using BRY;

using Codeplex.Data;
/// <summary>
/// 基本となるアプリのスケルトン
/// </summary>
namespace VcxprojRenamer
{
    public partial class Form1 : Form
    {
        private string m_path = "";
        private List<string> m_TargetFiles = new List<string>();
        //-------------------------------------------------------------
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }
        /// <summary>
        /// コントロールの初期化はこっちでやる
        /// </summary>
        protected override void InitLayout()
        {
            base.InitLayout();
            lbTarget.Items.Clear();
        }
        //-------------------------------------------------------------
        /// <summary>
        /// フォーム作成時に呼ばれる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            //設定ファイルの読み込み
            JsonPref pref = new JsonPref();
            if (pref.Load())
            {
                bool ok = false;
                Size sz = pref.GetSize("Size", out ok);
                if (ok) this.Size = sz;
                Point p = pref.GetPoint("Point", out ok);
                if (ok) this.Location = p;
                string s = pref.GetString("Path",out ok);
                if (ok) m_path = s;
            }
            this.Text = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
        }
        //-------------------------------------------------------------
        /// <summary>
        /// フォームが閉じられた時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //設定ファイルの保存
            JsonPref pref = new JsonPref();
            pref.SetSize("Size", this.Size);
            pref.SetPoint("Point", this.Location);
            pref.SetString("Path", m_path);
            pref.Save();

        }
        //-------------------------------------------------------------
        /// <summary>
        /// ドラッグ＆ドロップの準備
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        /// <summary>
        /// ドラッグ＆ドロップの本体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            //ここでは単純にファイルをリストアップするだけ
            GetCommand(files);
        }
        //-------------------------------------------------------------
        /// <summary>
        /// ダミー関数
        /// </summary>
        /// <param name="cmd"></param>
        public void GetCommand(string[] cmd)
        {
            m_TargetFiles.Clear();
            lbTarget.Items.Clear();
            tbPath.Text = "";
            tbOrg.Text = "";
            tbNew.Text = "";
            btnExec.Enabled = false;

            if (cmd.Length > 0)
            {
                foreach (string s in cmd)
                {
                    if (Directory.Exists(s) == false) continue;
                    if (GetFilesFromFolder(s)==true)
                    {
                        break;
                    }
                }
            }
            if(m_TargetFiles.Count>0)
            {
                m_TargetFiles.Sort();
                foreach (string s in m_TargetFiles)
                {
                    lbTarget.Items.Add(s);
                }
                tbPath.Text = Path.GetDirectoryName(m_TargetFiles[0]);
                tbOrg.Text = tbNew.Text = Path.GetFileNameWithoutExtension(m_TargetFiles[0]);


            }
        }
        /// <summary>
        /// メニューの終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //-------------------------------------------------------------
        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppInfoDialog.ShowAppInfoDialog();
        }
        // **************************************************************************
        private bool IsTargetFile(string p)
        {
            bool ret = false;

            string e = Path.GetExtension(p);

            ret = ((e == ".c") || (e == ".cpp") || (e == ".h") || (e == ".r")
                || (e == ".vcxproj") || (e == ".filters")
                || (e == ".plist") || (e == ".pbxproj") || (e == ".mode1v3") || (e == ".pbxuser")
                || (e == ".xcworkspacedata") || (e == ".xcuserstate") || (e == ".xcsettings") || (e == "xcscheme"));

            return ret;
        }
        // **************************************************************************
        private int FindFile(string p)
        {
            int ret = -1;
            int cnt = m_TargetFiles.Count;
            if (cnt <= 0) return ret;
            for ( int i=0; i<cnt;i++)
            {
                if( m_TargetFiles[i]==p)
                {
                    ret = i;
                    break;
                }
            }
            return ret;
        }
        // **************************************************************************
        private void AddFile(string p)
        {
            if (IsTargetFile(p) == false) return;
            int idx = FindFile(p);
            if(idx<0)
            {
                m_TargetFiles.Add(p);
            }
        }
        // **************************************************************************
        private bool GetFilesFromFolder(string p)
        {
            bool ret = false;
            if (Directory.Exists(p) == false) return ret;

            string[] flist = Directory.GetFiles(p);
            if(flist.Length>0)
            {
                for ( int i=0; i<flist.Length;i++)
                {
                    AddFile(flist[i]);
                }
            }

            string[] dlist = Directory.GetDirectories(p);
            if(dlist.Length>0)
            {
                for (int i = 0; i < dlist.Length; i++)
                {
                    string p2 = dlist[i];
                    if ((p2 == ".") || (p2 == "..")) continue;
                    GetFilesFromFolder(p2);
                }
            }
            ret = true;
            return ret;
        }
        // **************************************************************************
        private void TbNew_TextChanged(object sender, EventArgs e)
        {
            string s = tbOrg.Text;
            string d = tbNew.Text;
            btnExec.Enabled = ((s != "")&& (d != "") && (s != d));
        }
        // **************************************************************************
        private void BtnExec_Click(object sender, EventArgs e)
        {
            VcxprojConvert vc = new VcxprojConvert
            {
                SrcWord = tbOrg.Text,
                DstWord = tbNew.Text
            };
            if ( vc.CanExec)
            vc.TargetFiles = m_TargetFiles;
            string[] p = new string[1];
            string err = "";
            p[0] = vc.ConvertExec(out err);
            m_path = p[0];
            GetCommand(p);
            if (err != "")
            {
                MessageBox.Show(err);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //FolderBrowserDialogクラスのインスタンスを作成
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            fbd.Description = "フォルダを指定してください。";
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            fbd.SelectedPath = m_path;

            fbd.ShowNewFolderButton = false;

            //ダイアログを表示する
            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                string[] p = new string[1];
                p[0] = fbd.SelectedPath;
                GetCommand(p);
            }
        }
    }
}
