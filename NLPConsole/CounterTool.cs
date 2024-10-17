using System.Data;
using System.Linq;
using System.Diagnostics;
using System.Collections;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NLP
{
    public class CounterTool
    {
#if _USE_CLR
        public static int GetCount(string value, bool extend)
#elif _USE_CONSOLE
        public static int GetCount(string? value, bool extend)
#endif
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 检查长度
            if (value.Length == 1)
            {
                // 查询字符表
                return TokenCache.GetCount(value[0]);
            }
            // 先查询Count
            int count = CoreCache.GetCount(value);
            // 检查结果
            if (!extend || count > 0) return count;
            // 去查字典
            count = DictionaryCache.GetCount(value);
            // 检查结果
            if (count > 0) return count;
            // 去查短语表
            count = ShortCache.GetCount(value);
            // 检查结果
            if (count > 0) return count;
            // 检查结果
            count = CounterCache.GetCount(value);
            // 返回结果
            return count;
        }

        // 使用全局计数
        public static int GetCount(string[] groups)
        {
#if DEBUG
            Debug.Assert(groups != null && groups.Length > 0);
#endif
            // 频次参数
            int groupCount = 0;
            // 增加数据
            foreach (string group in groups)
            {
                // 获得计数
                int fg = GetCount(group, true); if (fg > 0) groupCount += fg;
/*
#if DEBUG
                Console.WriteLine(string.Format("\tvalue(\"{0}\").count = {1}", group, fg));
#endif
*/
            }
            // 返回结果
            return groupCount <= 0 ? -1 : groupCount;
        }

        // 使用全局计数
        public static int GetCount(bool prefix, string single, string[] groups)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(single));
            Debug.Assert(groups != null && groups.Length > 0);
#endif
            // 频次参数
            int combinationCount = 0;
            // 增加数据
            foreach (string group in groups)
            {
                // 获得新值
                string value = prefix ? (single + group) : (group + single);
                // 获得计数
                int count = GetCount(value, true); if (count > 0) combinationCount += count;
/*
#if DEBUG
                Console.WriteLine(string.Format("\tvalue(\"{0}\").count = {1}", value, count));
#endif
*/
            }
            // 返回结果
            return combinationCount <= 0 ? -1 : combinationCount;
        }

        public static void MakeStatistic()
        {
            // 记录日志
            LogTool.LogMessage("CounterTool", "MakeStatistic", "开始统计数据！");

            // 获得最大长度
            int length = DictionaryContent.GetMaxLength();
            // 检查结果
            if (length <= 0) length = 50;
            // 记录日志
            LogTool.LogMessage(string.Format("\tmax length = {0}", length));

            // 计数器
            int total = 0;
            // 任务数组
            List<Task> tasks = new List<Task>();
            // 生成任务控制器
            TaskFactory factory = new TaskFactory(
                new LimitedConcurrencyLevelTaskScheduler(DBTool.MAX_THREADS));

            // 获得数组
            string[] contents = FilteredCache.GetArray();
#if DEBUG
            Debug.Assert(contents != null && contents.Length > 0);
#endif
            // 记录日志
            LogTool.LogMessage(string.Format("\tcount = {0}", contents.Length));

            try
            {
                // 循环处理
                foreach (string content in contents)
                {
                    // 增加计数
                    total++;
                    // 检查结果
                    if (total % 10000 == 0)
                    {
                        // 记录日志
                        LogTool.LogMessage("CounterTool", "MakeStatistic",
                            string.Format("{0} items processed !", total));
                    }

                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;

                    // 启动进程
                    tasks.Add(factory.StartNew
                    (() =>
                    {
                        // 执行循环
                        for (int j = 1; j <= length && j <= content.Length; j++)
                        {
                            // 检查长度
                            for (int i = 0; i <= content.Length - j; i++)
                            {
                                // 获得子字符串
                                string value = content.Substring(i, j);
                                // 检查结果
                                if (value == null || value.Length != j) continue;

                                // 点击计数
                                CoreCache.ClickCount(value);
                                ShortCache.ClickCount(value);
                                DictionaryCache.ClickCount(value);
                                if (j == 1) TokenCache.ClickCount(value[0]);
                            }
                        }
                    }));

                    // 检查任务数量
                    if (tasks.Count >= 10000) { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 记录日志
                LogTool.LogMessage("CounterTool", "MakeStatistic",
                    string.Format("{0} items processed !", total));
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("CounterTool", "MakeStatistic", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("CounterTool", "MakeStatistic", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\ttotal = " + total);
            // 记录日志
            LogTool.LogMessage("CounterTool", "MakeStatistic", "数据统计完毕！");
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备统计所有频次，原表及其数据将被更改！");
            Console.WriteLine("确认是否执行（Yes/No）？");
            // 是否确认
            if (!ConsoleTool.Confirm()) return;

            // 打印信息
            Console.WriteLine("恢复TokenContent.Count初始值为0……");
            // 开始处理
            TokenContent.ResetCounter();

            // 打印信息
            Console.WriteLine("恢复ShortContent.Count初始值为0……");
            Console.WriteLine("数据量较大，请耐心等待。");
            // 开始处理
            ShortContent.ResetCounter();

            // 打印信息
            Console.WriteLine("恢复DictionaryContent.Count初始值为0……");
            Console.WriteLine("数据量较大，请耐心等待。");
            // 开始处理
            DictionaryContent.ResetCounter();

            // 打印信息
            Console.WriteLine("恢复CoreContent.Count初始值为0……");
            // 开始处理
            CoreContent.ResetCounter();

            // 开启日志
            LogTool.SetLog(true);
            // 创建计时器
            Stopwatch watch = new Stopwatch();
            // 开启计时器
            watch.Start();
            // 加载缓冲
            FilteredCache.LoadContents();
            // 加载缓冲
            TokenCache.LoadTokens();
            ShortCache.LoadShorts();
            CoreCache.LoadSegments();
            DictionaryCache.LoadEntries();
            // 开始统计Count数值
            MakeStatistic();
            // 更新计数
            TokenCache.SaveTokens();
            ShortCache.SaveShorts();
            CoreCache.SaveSegments();
            DictionaryCache.SaveEntries();
            // 关闭计时器
            watch.Stop();
            // 打印结果
            Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
        }
#endif
    }
}
