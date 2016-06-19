using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LeeClientAgent
{
    public partial class MainForm : Form
    {
        AgentClass agent = new AgentClass();
        string AppDirectory = string.Empty;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            AppDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\";
            agent.ClientListDirectory = AppDirectory + "RagexeClient" + "\\";
            agent.LeeClientDirectory = AppDirectory + "..\\";

            ArrayList ClientList = agent.GetClientList();
            ClientListBox.Items.Clear();

            if (ClientList != null)
            {
                for (int i = 0; i < ClientList.Count; i++)
                {
                    ClientListBox.Items.Add(ClientList[i]);
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x0201:                    // 鼠标左键按下的消息 
                    m.Msg = 0x00A1;             // 更改消息为非客户区按下鼠标 
                    m.LParam = IntPtr.Zero;     // 默认值 
                    m.WParam = new IntPtr(2);   // 鼠标放在标题栏内 
                    base.WndProc(ref m);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void ClientListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnInit.Enabled = (ClientListBox.SelectedIndex >= 0);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "确定要进行重置吗（这将撤销 LeeClient 在初始化时做出的改动）？", "询问", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            if (agent.DoReset())
            {
                MessageBox.Show(this, "重置成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(this, "很抱歉，重置失败（请确保游戏已经全部退出后再试一次）！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnInit_Click(object sender, EventArgs e)
        {
            string szClientVersion = ClientListBox.SelectedItem.ToString();

            if (MessageBox.Show(this, string.Format("确定要以 {0} 版本为基础，对 LeeClient 进行初始化吗？", szClientVersion), "询问", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            if (agent.DoInitial(szClientVersion))
            {
                MessageBox.Show(this, string.Format("成功以 {0} 版本为基础，对 LeeClient 进行初始化！", szClientVersion), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(this, string.Format("很抱歉，对 LeeClient 进行初始化失败了！", szClientVersion), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
