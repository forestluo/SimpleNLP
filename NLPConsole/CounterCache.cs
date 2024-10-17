using System;
using System.Text;
using System.Data;
using System.Linq;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NLP
{
    public class CounterCache : CounterContent
    {
        // 最大长度
        public static int MAX_LENGTH = 6;
        // 创建数据字典
        private static Dictionary<string, int>
            counters = new Dictionary<string, int>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ClickCount(string value, int count = 1)
        {
#if DEBUG
            Debug.Assert(count > 0);
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 增加计数
            // 假设所有数据都加入到内存
            if (counters.ContainsKey(value)) counters[value] += count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ClearCounters()
        {
            // 记录日志
            LogTool.LogMessage("CounterCache", "ClearCounters", "开始执行！");
            // 恢复初始值
            counters.Clear();
            // 记录日志
            LogTool.LogMessage("CounterCache", "ClearCounters", "执行完毕！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int GetCount(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 返回结果
            return counters.ContainsKey(value) ?
                counters[value] : LoadCounter(value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void AddCounter(string value, int count)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 在数据表中增加数据
            AddContent(value, count);
            // 保持数据同步
            if (counters.ContainsKey(value)) counters[value] = count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int LoadCounter(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 记录日志
            //LogTool.LogMessage("CounterCache", "LoadCounter", "加载数据记录！");

            // 计数
            int count = -1;
            // 指令字符串
            string cmdString =
                "SELECT TOP 1 [count] " +
                "FROM [dbo].[CounterContent] WHERE [content] = @SqlContent;";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("CounterCache", "LoadCounter", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 设置参数
                sqlCommand.Parameters.AddWithValue("SqlContent", value);
                // 记录日志
                //LogTool.LogMessage("CounterCache", "LoadCounter", "参数已设定！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                //LogTool.LogMessage("CounterCache", "LoadCounter", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 读取count
                    count = reader.GetInt32(0);
                    // 检查数据
                    if (!counters.ContainsKey(value)) counters.Add(value, count);
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                //LogTool.LogMessage("CounterCache", "LoadCounter", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("CounterCache", "LoadCounter", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("CounterCache", "LoadCounter", "数据库链接已关闭！");
            }

            // 记录日志
            //LogTool.LogMessage("CounterCache", "LoadCounter", "数据记录已加载！");
            // 返回结果
            return count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int LoadCounters()
        {
            // 清理数据记录
            counters.Clear();
            // 记录日志
            LogTool.LogMessage("CounterCache", "LoadCounters", "加载数据记录！");

            // 指令字符串
            string cmdString = "SELECT [count], [content] FROM [dbo].[CounterContent];";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("CounterCache", "LoadCounters", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("CounterCache", "LoadCounters", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("CounterCache", "LoadCounters", "T-SQL指令已执行！");

                // 计数器
                int total = 0;
                // 循环处理
                while (reader.Read())
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 记录日志
                        LogTool.LogMessage("CounterCache", "LoadCounters",
                            string.Format("{0} items loaded !", total));
                    }

                    // 检查参数
                    if (reader.IsDBNull(1)) continue;
                    // 获得content
                    string content = reader.GetString(1);
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;

                    // 获得count
                    int count = reader.GetInt32(0);
                    // 检查数据
                    if (counters.ContainsKey(content))
                    {
                        // 设置新数值
                        counters[content] = count;
                    }
                    // 增加对象
                    else counters.Add(content, count);
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("CounterCache", "LoadCounters",
                    string.Format("{0} items loaded !", total));
                // 记录日志
                LogTool.LogMessage("CounterCache", "LoadCounters", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("CounterCache", "LoadCounters", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("CounterCache", "LoadCounters", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\tcounters.count = " + counters.Count);
            // 记录日志
            LogTool.LogMessage("CounterCache", "LoadCounters", "数据记录已加载！");
            // 返回结果
            return counters.Count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SaveCounters()
        {
            // 记录日志
            LogTool.LogMessage("CounterCache", "SaveCounters", "保存数据记录！");
            LogTool.LogMessage(string.Format("\tcounters.count = {0}", counters.Count));

            // 计数器
            int total = 0;
            // 任务数组
            List<Task> tasks = new List<Task>();
            // 生成任务控制器
            TaskFactory factory = new TaskFactory(
                new LimitedConcurrencyLevelTaskScheduler(DBTool.MAX_THREADS));

            try
            {
                // 遍历参数
                foreach (KeyValuePair<string, int> kvp in counters)
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("CounterCache", "SaveCounters",
                                string.Format("{0} items saved !", total));
                    }
                    // 检查数值
                    // 对于不超过1的数值完全没必要保留
                    if (kvp.Value <= 1) continue;
                    // 目前数据库中存在的数值，也不保存
                    if (DictionaryCache.GetCount(kvp.Key) > 0) continue;
                    // 启动进程
                    tasks.Add(factory.StartNew(()
                            => AddContent(kvp.Key, kvp.Value)));
                    // 检查任务数量
                    if (tasks.Count >= 10000)
                    { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 打印记录
                LogTool.LogMessage("CounterCache", "SaveCounters",
                        string.Format("{0} items saved !", total));
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("CounterCache", "SaveCounters", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("CounterCache", "SaveCounters", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\tcounters.count = " + counters.Count);
            // 记录日志
            LogTool.LogMessage("CounterCache", "SaveCounters", "数据记录已保存！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void MakeStatistic()
        {
            // 开始执行循环
            for (int i = 2; i <= MAX_LENGTH; i++)
            {
                // 开始统计
                MakeStatistic(i); SaveCounters(); ClearCounters();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void MakeStatistic(int length)
        {
            // 记录日志
            LogTool.LogMessage("CounterCache", "MakeStatistic", "开始统计数据！");
            // 检查参数
            if (length <= 1) length = 2;
            // 检查参数
            if (length > MAX_LENGTH) length = MAX_LENGTH;
            // 记录日志
            LogTool.LogMessage(string.Format("\tlength = {0}", length));

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
            LogTool.LogMessage(string.Format("\tfiltered.count = {0}", contents.Length));

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
                        LogTool.LogMessage("CounterCache", "MakeStatistic",
                            string.Format("{0} items processed !", total));
                    }

                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;
                    // 检查结果
                    if (content.Length < length) continue;

                    // 启动进程
                    tasks.Add(factory.StartNew
                    (() =>
                    {
                        // 检查长度
                        for (int i = 0; i <= content.Length - length; i++)
                        {
                            // 获得子字符串
                            string value = content.Substring(i, length);
#if DEBUG
                            Debug.Assert(value != null && value.Length == length);
#endif
                            // 检查是否为中文
                            if (!ChineseTool.IsChinese(value)) continue;
                            // 增加计数器
                            lock (counters)
                            {
                                // 查询
                                if (counters.
                                    ContainsKey(value))
                                {
                                    // 计数器
                                    counters[value]++;
                                }
                                // 增加新数据
                                else counters.Add(value, 1);
                            }
                        }
                    }));

                    // 检查任务数量
                    if (tasks.Count >= 10000) { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 记录日志
                LogTool.LogMessage("CounterCache", "MakeStatistic",
                    string.Format("{0} items processed !", total));
                // 记录日志
                LogTool.LogMessage("CounterCache", "MakeStatistic", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("CounterCache", "MakeStatistic", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("CounterCache", "MakeStatistic", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\tcounters.Count = " + counters.Count);
            // 记录日志
            LogTool.LogMessage("CounterCache", "MakeStatistic", "数据统计完毕！");
        }

#if _USE_CONSOLE
        public static new void Main(string[] args)
        {
            Console.WriteLine("准备统计字符串频次，过程将会十分耗时！");
            Console.WriteLine("确认是否执行（Yes/No）？");
            // 是否确认
            if (!ConsoleTool.Confirm()) return;

            // 检查数据表
            if (!DBTool.TableExists("CounterContent"))
            {
                // 记录日志
                Console.WriteLine("数据表CounterContent不存在！");
            }

            // 开启日志
            LogTool.SetLog(true);
            // 创建计时器
            Stopwatch watch = new Stopwatch();
            // 开启计时器
            watch.Start();
            // 创建表
            CreateTable();
            // 加载数据
            FilteredCache.LoadContents();
            // 加载数据
            DictionaryCache.LoadEntries();
            // 开始执行循环
            MakeStatistic();
            // 清理数据
            FilteredCache.ClearContents();
            // 清理数据
            DictionaryCache.ClearEntries();
            // 记录日志
            Console.WriteLine("数据统计已完成！");
            // 关闭计时器
            watch.Stop();
            // 打印结果
            Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
        }
#endif
    }
}
