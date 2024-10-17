using System.Data;
using System.Diagnostics;
using System.Collections;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NLP
{
    public class DictionaryCache : DictionaryContent
    {
        // 创建数据字典
        private static Dictionary<string, int>
            entries = new Dictionary<string, int>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ClickCount(string value, int count = 1)
        {
#if DEBUG
            Debug.Assert(count > 0);
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 增加计数
            // 假设所有数据都加入到内存
            if (entries.ContainsKey(value)) entries[value] += count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int GetCount(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 返回结果
            return entries.ContainsKey(value) ?
                entries[value] : LoadEntry(value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ClearEntries()
        {
            // 记录日志
            LogTool.LogMessage("DictionaryCache", "ClearEntries", "开始执行！");
            // 恢复初始值
            entries.Clear();
            // 记录日志
            LogTool.LogMessage("DictionaryCache", "ClearEntries", "执行完毕！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
#if _USE_CLR
        public static void AddEntry(string value, int count, string[] inputs)
#elif _USE_CONSOLE
        public static void AddEntry(string value, int count, string[]? inputs)
#endif
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
            Debug.Assert(inputs == null || inputs.Length <= 2);
#endif
            // 添加数据值
            AddContent(value, count, inputs);
            // 同步数据
            if (entries.ContainsKey(value))
            {
                // 设置新数值
                entries[value] = count;
            }
            else
            {
                // 增加记录
                entries.Add(value, count < 0 ? 0 : count);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int LoadEntry(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 记录日志
            //LogTool.LogMessage("DictionaryCache", "LoadEntry", "加载数据记录！");

            // 计数器
            int count = -1;
            // 指令字符串
            string cmdString =
                "SELECT TOP 1 [count] " +
                "FROM [dbo].[DictionaryContent] WHERE [content] = @SqlEntry;";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("DictionaryCache", "LoadEntry", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                //LogTool.LogMessage("DictionaryCache", "LoadEntry", "T-SQL指令已创建！");

                // 设置参数
                sqlCommand.Parameters.AddWithValue("SqlEntry", value);
                // 记录日志
                //LogTool.LogMessage("DictionaryCache", "LoadEntry", "T-SQL参数已设定！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                //LogTool.LogMessage("DictionaryCache", "LoadEntry", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 设置参数
                    count = reader.GetInt32(0);
                    // 检查数据
                    if (entries.ContainsKey(value))
                    {
                        // 设置新数值
                        entries[value] = count;
                    }
                    else
                    {
                        // 增加记录
                        entries.Add(value, count < 0 ? 0 : count);
                    }
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                //LogTool.LogMessage("DictionaryCache", "LoadEntry", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("DictionaryCache", "LoadEntry", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("DictionaryCache", "LoadEntry", "数据库链接已关闭！");
            }

            // 记录日志
            //LogTool.LogMessage("DictionaryCache", "LoadEntry", "数据记录已加载！");
            // 返回结果
            return count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void LoadEntries()
        {
            // 清理数据记录
            entries.Clear();
            // 记录日志
            LogTool.LogMessage("DictionaryCache", "LoadEntries", "加载数据记录！");

            // 指令字符串
            string cmdString =
                "SELECT [content], [count] FROM [dbo].[DictionaryContent];";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("DictionaryCache", "LoadEntries", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("DictionaryCache", "LoadEntries", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("DictionaryCache", "LoadEntries", "T-SQL指令已执行！");

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
                        // 打印记录
                        LogTool.LogMessage("DictionaryCache", "LoadEntries", 
                            string.Format("{0} entries loaded !", total));
                    }

                    // 获得内容
                    string value = reader.GetString(0);
                    // 检查结果
                    if (string.IsNullOrEmpty(value)) continue;

                    // 获得计数
                    int count = reader.GetInt32(1);
                    // 检查数据
                    if (entries.ContainsKey(value))
                    {
                        // 更新记录
                        entries[value] = count;
                    }
                    else
                    {
                        // 增加记录
                        entries.Add(value, count < 0 ? 0 : count);
                    }
                }
                // 关闭数据阅读器
                reader.Close();
                // 打印记录
                LogTool.LogMessage("DictionaryCache", "LoadEntries",
                    string.Format("{0} entries loaded !", total));
                // 记录日志
                LogTool.LogMessage("DictionaryCache", "LoadEntries", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("DictionaryCache", "LoadEntries", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("DictionaryCache", "LoadEntries", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\tentries.count = " + entries.Count);
            // 记录日志
            LogTool.LogMessage("DictionaryCache", "LoadEntries", "数据记录已加载！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SaveEntries()
        {
            // 记录日志
            LogTool.LogMessage("DictionaryCache", "SaveEntries", "保存数据记录！");
            // 记录日志
            LogTool.LogMessage(string.Format("\tentries.count = {0}", entries.Count));

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
                foreach (KeyValuePair<string, int> kvp in entries)
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("DictionaryCache", "SaveEntries", 
                            string.Format("{0} entries saved !", total));
                    }

                    // 启动进程
                    tasks.Add(factory.StartNew(() =>
                        AddContent(kvp.Key, kvp.Value, null)));
                    // 检查任务数量
                    if (tasks.Count >= 10000)
                        { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 打印记录
                LogTool.LogMessage("DictionaryCache", "SaveEntries", 
                    string.Format("{0} entries saved !", total));
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("DictionaryCache", "SaveEntries", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                    { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("DictionaryCache", "SaveEntries", "任务全部结束！");
            }

            // 打印记录
            LogTool.LogMessage("\tentries.count = " + entries.Count);
            // 记录日志
            LogTool.LogMessage("DictionaryCache", "SaveEntries", "数据记录已更新！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void MakeStatistic()
        {
            // 记录日志
            LogTool.LogMessage("DictionaryCache", "MakeStatistic", "开始统计数据！");

            // 获得最大长度
            int length = GetMaxLength();
            // 检查结果
            if (length <= 0) length = 64;
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
            LogTool.LogMessage(string.Format("\tfiltered.count = {0}", contents.Length));

            try
            {
                // 循环处理
                foreach(string content in contents)
                {
                    // 增加计数
                    total++;
                    // 检查结果
                    if (total % 10000 == 0)
                    {
                        // 记录日志
                        LogTool.LogMessage("DictionaryCache", "MakeStatistic",
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

                                // 增加计数器
                                lock (entries)
                                {
                                    // 增加数值
                                    if(entries.ContainsKey(value)) entries[value]++;
                                }
                            }
                        }
                    }));

                    // 检查任务数量
                    if (tasks.Count >= 10000) { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 记录日志
                LogTool.LogMessage("DictionaryCache", "MakeStatistic",
                    string.Format("{0} items processed !", total));
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("DictionaryCache", "MakeStatistic", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                    { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("DictionaryCache", "MakeStatistic", "任务全部结束！");
            }

            // 打印记录
            LogTool.LogMessage("\tentries.count = " + entries.Count);
            // 记录日志
            LogTool.LogMessage("DictionaryCache", "MakeStatistic", "数据统计完毕！");
        }

#if _USE_CONSOLE
        public static new void Main(string[] args)
        {
            Console.WriteLine("准备统计Dictionary频次，原表及其数据将被更改！");
            Console.WriteLine("确认是否执行（Yes/No）？");
            // 检查确认
            if (!ConsoleTool.Confirm()) return;

            // 检查数据表
            if (!DBTool.TableExists("DictionaryContent"))
            {
                Console.WriteLine("数据表DictionaryContent不存在！"); return;
            }

            // 开启日志
            LogTool.SetLog(true);
            // 创建计时器
            Stopwatch watch = new Stopwatch();
            // 开启计时器
            watch.Start();
            // 打印输出
            Console.WriteLine("恢复DictionaryContent.Count数值为0……");
            // 恢复初始值
            ResetCounter();
            // 加载数据
            FilteredCache.LoadContents();
            // 加载计数
            LoadEntries();
            // 开始统计
            MakeStatistic();
            // 更新计数
            SaveEntries();
            // 关闭计时器
            watch.Stop();
            // 打印结果
            Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
        }
#endif
    }
}
