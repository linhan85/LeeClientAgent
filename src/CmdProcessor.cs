using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeeClientAgent
{
    class CmdProcessor
    {
        public void Exec(string[] args)
        {
            //Console.WriteLine("Welcome use LeeClient Agent.");

            // 处理开关参数
            CmdArgs cmdArgs = CmdLine.Parse(args);
            List<string> aloneParams = cmdArgs.Params;

            for (int i = 0; i < aloneParams.Count; i++)
            {
                string commandArgString = cmdArgs.Params[i];
                //Console.WriteLine("Stand-alone: " + commandArgString);

                switch(commandArgString)
                {
                    case "/trans-msgstringtable":
                        Console.WriteLine("正在请求对 msgstringtable.txt 进行汉化.");

                        TransMsgstringtable trans = new TransMsgstringtable();
                        trans.Execute();
                        break;
                }
            }

            // 处理带值参数
            Dictionary<string, string> argPairs = cmdArgs.ArgPairs;
            List<string> keys = argPairs.Keys.ToList();

            for (int i = 0; i < keys.Count; i++)
            {
                string strKey = keys[i];
                string strValue = argPairs[strKey];

                //System.Console.WriteLine("Key/Value: " + strKey + "/" + strValue);
            }
        }
    }

    /// <summary>
    /// 命令行参数解析类
    /// http://blog.csdn.net/jackxinxu2100/article/details/6642694
    /// </summary>
    class CmdArgs
    {
        Dictionary<string, string> mArgPairs = new Dictionary<string, string>();
        public Dictionary<string, string> ArgPairs
        {
            get { return mArgPairs; }
        }

        List<string> mParams = new List<string>();
        public List<string> Params
        {
            get { return mParams; }
        }
    }

    class CmdLine
    {
        public static CmdArgs Parse(string[] args)
        {
            char[] kEqual = new char[] { '=' };
            char[] kArgStart = new char[] { '-', '\\' };
            CmdArgs ca = new CmdArgs();
            int ii = -1;
            string token = NextToken(args, ref ii);

            while (token != null)
            {
                if (IsArg(token))
                {
                    string arg = token.TrimStart(kArgStart).TrimEnd(kEqual);
                    string value = null;

                    if (arg.Contains("="))
                    {
                        // arg was specified with an '=' sign, so we need
                        // to split the string into the arg and value, but only
                        // if there is no space between the '=' and the arg and value.
                        string[] r = arg.Split(kEqual, 2);

                        if (r.Length == 2 && r[1] != string.Empty)
                        {
                            arg = r[0];
                            value = r[1];
                        }
                    }

                    while (value == null)
                    {
                        string next = NextToken(args, ref ii);

                        if (next != null)
                        {
                            if (IsArg(next))
                            {
                                // push the token back onto the stack so
                                // it gets picked up on next pass as an Arg
                                ii--;

                                value = "true";
                            }
                            else if (next != "=")
                            {
                                // save the value (trimming any '=' from the start)
                                value = next.TrimStart(kEqual);
                            }
                        }
                    }

                    // save the pair
                    ca.ArgPairs.Add(arg, value);
                }
                else if (token != string.Empty)
                {
                    // this is a stand-alone parameter.
                    ca.Params.Add(token);
                }

                token = NextToken(args, ref ii);
            }
            return ca;
        }

        static bool IsArg(string arg)
        {
            return (arg.StartsWith("-") || arg.StartsWith("\\"));
        }

        static string NextToken(string[] args, ref int ii)
        {
            ii++; // move to next token
            while (ii < args.Length)
            {
                string cur = args[ii].Trim();

                if (cur != string.Empty)
                {
                    // found valid token
                    return cur;
                }
                ii++;
            }

            // failed to get another token
            return null;
        }
    }
}
