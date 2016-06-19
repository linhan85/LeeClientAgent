using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LeeClientAgent
{
    static class Program
    {
        // 引入必要的API，为接下来的工作做准备
        // http://stackoverflow.com/questions/7198639/c-sharp-application-both-gui-and-commandline

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        /// <summary>
        /// 程序的主入口点
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // 重定向终端输出到父进程
            // 必须在 Console.WriteLine() 之前调用
            AttachConsole(ATTACH_PARENT_PROCESS);

            if (args.Length > 0)
            {
                // 如果启动的时候有参数，那么以命令行方式运行此程序
                Console.WriteLine("");

                CmdProcessor cmd = new CmdProcessor();
                cmd.Exec(args);

                // 模拟发送一个回车按键, 这样看起来才能和一个命令行工具一样（执行完毕自动结束）
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                Application.Exit();
            }
            else
            {
                // 如果没有参数，那么以 Winform 方式运行此程序
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }
    }
}
