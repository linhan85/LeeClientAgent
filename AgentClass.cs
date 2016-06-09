using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LeeClientAgent
{
    class AgentClass
    {
        #region 属性访问控制器
        protected string m_ClientListDirectory = string.Empty;
        protected string m_LeeClientDirectory = string.Empty;

        public string ClientListDirectory
        {
            get { return m_ClientListDirectory; }
            set { m_ClientListDirectory = value; }
        }
        public string LeeClientDirectory
        {
            get { return m_LeeClientDirectory; }
            set { m_LeeClientDirectory = value; }
        }
        #endregion

        /// <summary>
        /// 返回可用的客户端版本列表，用于显示到主界面的 ListBox 中
        /// </summary>
        /// <returns></returns>
        public ArrayList GetClientList()
        {
            ArrayList agent = new ArrayList();

            if (!Directory.Exists(m_ClientListDirectory))
            {
                MessageBox.Show(
                    null, 
                    string.Format("目录 '{0}' 不存在, 无法确定客户端版本列表.", m_ClientListDirectory), 
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning
                );
                return null;
            }

            string[] directories = Directory.GetDirectories(
                m_ClientListDirectory,
                "????-??-??"
            );

            foreach(string directory in directories)
            {
                // 这里利用 GetFileName 的特性，将最后的目录名字拿出来
                agent.Add(Path.GetFileName(directory));
            }

            return agent;
        }

        /// <summary>
        /// 使用指定的客户端版本来对 LeeClient 进行初始化操作
        /// </summary>
        /// <param name="szClientVersion"></param>
        /// <returns></returns>
        public bool DoInitial(string szClientVersion)
        {
            if (szClientVersion.Trim().Length == 0)
                return false;

            if (!Directory.Exists(m_ClientListDirectory + szClientVersion))
                return false;

            if (!Directory.Exists(m_LeeClientDirectory))
                return false;

            try
            {
                FileHelper.CopyDirectory(m_ClientListDirectory + szClientVersion + "\\Basic", m_LeeClientDirectory);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 对 LeeClient 文件夹进行清理操作
        /// </summary>
        /// <returns></returns>
        public bool DoReset()
        {
            try
            {
                DeleteDirectory(m_LeeClientDirectory + "_tmpEmblem");
                DeleteDirectory(m_LeeClientDirectory + "memo");
                DeleteDirectory(m_LeeClientDirectory + "Replay");
                DeleteDirectory(m_LeeClientDirectory + "savedata");
                DeleteDirectory(m_LeeClientDirectory + "data\\luafiles514");
                DeleteFile(m_LeeClientDirectory + "data\\msgstringtable.txt");
                DeleteFile(m_LeeClientDirectory + "data\\clientinfo.xml");

                string[] ExcludeList = { "setup.exe" };
                DeleteFile(m_LeeClientDirectory + "*.exe", ExcludeList);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 删除指定的目录，包括里面的子目录和文件
        /// </summary>
        /// <param name="szDirectory">要删除的目录路径</param>
        private void DeleteDirectory(string szDirectory)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(szDirectory);
                di.Delete(true);
            }
            catch
            {

            }
        }

        /// <summary>
        /// 删除指定路径的文件
        /// </summary>
        /// <param name="szFile">要删除的文件路径，文件名支持通配符</param>
        /// <param name="szExcludeList">要排除的文件名，若要删的文件名和该数组中任何一个匹配，则跳过不删它</param>
        private void DeleteFile(string szFile, string[] szExcludeList = null)
        {
            string szDirectory = Path.GetDirectoryName(szFile);
            string szFileName = Path.GetFileName(szFile);

            string[] fileList = Directory.GetFiles(szDirectory, szFileName);
            foreach (string file in fileList)
            {
                if (szExcludeList != null)
                {
                    string filename = Path.GetFileName(file).ToLower();
                    bool is_exclude = false;
                    foreach (string exclude in szExcludeList)
                    {
                        is_exclude = (filename == exclude.ToLower());
                    }
                    if (is_exclude) continue;
                }

                File.Delete(file);
            }
        }
    }
}
