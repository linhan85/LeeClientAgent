using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LeeClientAgent
{
    class TranslateMapItem
    {
        public int lineNo;
        public string lineContent;
        public string origin;
        public string target;
    }

    class ReferItem
    {
        public string origin;
        public int referCount;
    }

    class TransMsgstringtable
    {
        private string AppDirectory = string.Empty;
        private char m_SplitChar = '#';
        private Dictionary<string, string> m_dictTranslate = new Dictionary<string, string>();
        private Dictionary<string, string> m_dictSingleTranslate = new Dictionary<string, string>();
        private Dictionary<string, ReferItem> m_dictReferMap = new Dictionary<string, ReferItem>();

        public TransMsgstringtable(char splitChar = '#')
        {
            // 获得程序的启动目录
            AppDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\";
            m_SplitChar = splitChar;
        }

        public void Execute()
        {
            Console.WriteLine(@"翻译对照表为: TranslateData\msgstringtable\maps.txt");
            Console.WriteLine("");

            LoadTranslateDict(AppDirectory + @"TranslateData\msgstringtable\maps.txt");

            AgentClass agent = new AgentClass();
            agent.ClientListDirectory = AppDirectory + "RagexeClient" + "\\";
            ArrayList ClientList = agent.GetClientList();

            for (int i = 0; i < ClientList.Count; i++)
            {
                List<string> MsgstringtableData = new List<string>();
                string srcFilename = agent.ClientListDirectory + ClientList[i] + "\\Basic\\data\\msgstringtable.txt";

                Console.Write(string.Format("正在汉化 RagexeClient\\{0} ... ", ClientList[i] + "\\Basic\\data\\msgstringtable.txt"));

                LoadMsgStringTable(ref MsgstringtableData, srcFilename);
                DoTranslate(ref MsgstringtableData);
                SaveMsgStringTable(MsgstringtableData, srcFilename);

                Console.WriteLine("完毕");
            }
            return;
        }

        private bool LoadTranslateDict(string strMapFilename)
        {
            // TODO: 参数有效性校验

            try
            {
                // 载入每一行数据到数组
                List<TranslateMapItem> listTranslateMap = new List<TranslateMapItem>();

                StreamReader mapfile = new StreamReader(strMapFilename, Encoding.Default);
                if (mapfile == null) return false;

                string strLine = string.Empty;
                int intLineNo = 1;
                while ((strLine = mapfile.ReadLine()) != null)
                {
                    string[] val = strLine.Split(m_SplitChar);
                    if (val.Count() >= 2 && (val[0].StartsWith("//") == false))
                    {
                        TranslateMapItem tm_item = new TranslateMapItem();
                        tm_item.lineNo = intLineNo;
                        tm_item.lineContent = strLine;
                        tm_item.origin = val[0];
                        tm_item.target = val[1];

                        listTranslateMap.Add(tm_item);
                    }
                    intLineNo++;
                }

                mapfile.Close();

                // 重建字典 - 上下原文字典
                m_dictTranslate.Clear();
                for (int i = 0; i < listTranslateMap.Count; i++)
                {
                    string key = string.Empty;
                    if ((i - 1) >= 0) key += listTranslateMap[i - 1].origin + "|";
                    key += listTranslateMap[i].origin + "|";
                    if (i + 1 < listTranslateMap.Count) key += listTranslateMap[i + 1].origin + "|";

                    if (!m_dictTranslate.ContainsKey(key))
                    {
                        m_dictTranslate.Add(key, listTranslateMap[i].target);
                    }

                    // ---------------

                    key = string.Empty;
                    if ((i - 1) >= 0) key += listTranslateMap[i - 1].target + "|";
                    key += listTranslateMap[i].target + "|";
                    if (i + 1 < listTranslateMap.Count) key += listTranslateMap[i + 1].target + "|";

                    if (!m_dictTranslate.ContainsKey(key))
                    {
                        m_dictTranslate.Add(key, listTranslateMap[i].target);
                    }
                }

                // 创建引用次数表
                m_dictReferMap.Clear();
                for (int i = 0; i < listTranslateMap.Count; i++)
                {
                    string key = listTranslateMap[i].origin;

                    if (m_dictReferMap.ContainsKey(key))
                    {
                        m_dictReferMap[key].referCount++;
                        continue;
                    }

                    // --------

                    ReferItem ritem = new ReferItem();
                    ritem.origin = listTranslateMap[i].origin;
                    ritem.referCount = 1;

                    m_dictReferMap.Add(key, ritem);
                }

                // 根据引用次数表, 创建独立映射表
                m_dictSingleTranslate.Clear();
                for (int i = 0; i < listTranslateMap.Count; i++)
                {
                    string key = listTranslateMap[i].origin;

                    if (m_dictReferMap.ContainsKey(key) && m_dictReferMap[key].referCount == 1)
                    {
                        m_dictSingleTranslate.Add(key, listTranslateMap[i].target);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GetSplitText(string str, char split, int index)
        {
            string[] val = str.Split(split);
            if (index > (val.Count() - 1)) return string.Empty;
            return val[index];
        }

        private bool LoadMsgStringTable(ref List<string> mSourceData, string szSrcFilename)
        {
            // TODO: 参数有效性校验

            try
            {
                mSourceData.Clear();

                StreamReader srcfile = new StreamReader(szSrcFilename, Encoding.Default);
                if (srcfile == null) return false;

                string szLine = string.Empty;
                while ((szLine = srcfile.ReadLine()) != null)
                {
                    string[] val = szLine.Split(m_SplitChar);
                    if (val.Count() == 2 && (!val[0].StartsWith("//")))
                    {
                        mSourceData.Add(val[0]);
                    }
                }

                srcfile.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool DoTranslate(ref List<string> sourceData)
        {
            // 根据上下文进行翻译
            List<string> backup = new List<string>(sourceData);
            for (int i = 0; i < backup.Count; i++)
            {
                string key = string.Empty;
                if ((i - 1) >= 0) key += GetSplitText(backup[i - 1], m_SplitChar, 0) + "|";
                key += GetSplitText(backup[i], m_SplitChar, 0) + "|";
                if (i + 1 < backup.Count) key += GetSplitText(backup[i + 1], m_SplitChar, 0) + "|";

                if (m_dictTranslate.ContainsKey(key) && m_dictTranslate[key] != string.Empty)
                {
                    // 有翻译数据，替换一下原始数据
                    sourceData[i] = m_dictTranslate[key];
                }
            }

            // 根据独立行进行翻译
            for (int i = 0; i < sourceData.Count; i++)
            {
                string key = sourceData[i];

                if (m_dictSingleTranslate.ContainsKey(key) && m_dictSingleTranslate[key] != string.Empty)
                {
                    // 有翻译数据，替换一下原始数据
                    sourceData[i] = m_dictSingleTranslate[key];
                }
            }

            return true;
        }

        private bool SaveMsgStringTable(List<string> sourceData, string szSaveFilename)
        {
            // TODO: 参数有效性校验

            try
            {
                StreamWriter sr = new StreamWriter(szSaveFilename, false, Encoding.Default);

                for (int i = 0; i < sourceData.Count; i++)
                {
                    sr.WriteLine(sourceData[i] + m_SplitChar);
                }

                sr.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
