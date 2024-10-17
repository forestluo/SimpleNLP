using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NLP
{
    public class LogTool
    {
        // 是否记录
        private static bool DEBUG = true;
        // 最大日志量
        private readonly static int MAX_LOGS = 2048;
        // 日志记录
        private static List<string[]> logs = new List<string[]>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SetLog(bool bValue)
        {
            // 设置数值
            DEBUG = bValue;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ClearLogs()
        {
            // 清理日志
            logs.Clear();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static string[] GetLogs()
        {
            // 计数器
            int count = 0;

            // 数组
            List<string> items = new List<string>();
            // 检查数量
            foreach (string[] item in logs)
            {
                // 修改计数器
                count++;
                // 检查参数
                if (item.Length == 1)
                {
                    // 发送消息
                    items.Add(item[0]);
                }
                else if (item.Length == 2)
                {
                    // 发送消息
                    items.Add(item[0] + " > " + item[1]);
                }
                else if (item.Length == 3)
                {
                    // 发送消息
                    items.Add(item[0] + " " + item[1] + " > " + item[2]);
                }
                else if (item.Length == 4)
                {
                    // 发送消息
                    items.Add(item[0] + " " + item[1] + "." + item[2] + " > " + item[3]);
                }
            }
            // 删除内容
            if (count > 0) logs.RemoveRange(0, count);
            // 返回结果
            return items.ToArray();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void LogMessage(string strMessage)
        {
            // 检查参数
            if (DEBUG)
            {
                // 检查日志记录
                if (logs.Count >= MAX_LOGS) logs.RemoveAt(0);
                // 加入日志记录
                logs.Add(new string[] { DateTime.Now.ToString("HH:mm:ss"), strMessage });
#if _USE_CONSOLE
                // 打印输出
                Console.WriteLine(string.Format("{0} > {1}", DateTime.Now.ToString("HH:mm:ss"), strMessage));
#endif
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void LogMessage(string strModule, string strFunction, string strMessage)
        {
            // 检查参数
            if (DEBUG)
            {
                // 检查日志记录
                if (logs.Count >= MAX_LOGS) logs.RemoveAt(0);
                // 加入日志记录
                logs.Add(new string[] { DateTime.Now.ToString("HH:mm:ss"), strModule, strFunction, strMessage });
#if _USE_CONSOLE
                // 打印输出
                Console.WriteLine(string.Format("{0} > {1}.{2} : {3}", DateTime.Now.ToString("HH:mm:ss"), strModule, strFunction, strMessage));
#endif
            }
        }
    }
}
