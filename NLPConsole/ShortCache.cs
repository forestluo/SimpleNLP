using System.Data;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NLP
{
    public class ShortCache : ShortContent
    {
        // 创建数据字典
        private static Dictionary<string, int>
            shorts = new Dictionary<string, int>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static string[] GetArray(int length = -1)
        {
            // 生成对象
            List<string> list = new List<string>();
            // 检查参数
            if (length > 0)
            {
                foreach (KeyValuePair<string, int> kvp in shorts)
                {
                    if (kvp.Key.Length >= length) list.Add(kvp.Key);
                }
            }
            else
            {
                foreach (KeyValuePair<string, int> kvp in shorts) list.Add(kvp.Key);
            }
            // 返回结果
            return list.ToArray();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ClickCount(string value, int count = 1)
        {
#if DEBUG
            Debug.Assert(count > 0);
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 增加计数
            // 假设所有数据都加入到内存
            if (shorts.ContainsKey(value)) shorts[value] += count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ClearShorts()
        {
            // 记录日志
            LogTool.LogMessage("ShortCache", "ClearShorts", "开始执行！");
            // 恢复初始值
            shorts.Clear();
            // 记录日志
            LogTool.LogMessage("ShortCache", "ClearShorts", "执行完毕！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int GetCount(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 返回结果
            return shorts.ContainsKey(value) ?
                shorts[value] : LoadShort(value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void AddShort(string value, int count)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 更改数据表
            AddContent(value, count);
            // 执行数据同步
            if (shorts.ContainsKey(value))
            {
                shorts[value] = count;
            }
            else shorts.Add(value, count);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int LoadShort(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 记录日志
            //LogTool.LogMessage("ShortCache", "LoadShort", "加载数据记录！");

            // 计数器
            int count = -1;
            // 指令字符串
            string cmdString =
                "SELECT TOP 1 [count] " +
                "FROM [dbo].[ShortContent] WHERE [content] = @SqlContent;";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("ShortCache", "LoadShort", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                //LogTool.LogMessage("ShortCache", "LoadShort", "T-SQL指令已创建！");

                // 设置参数
                sqlCommand.Parameters.AddWithValue("SqlContent", value);
                // 记录日志
                //LogTool.LogMessage("ShortCache", "LoadShort", "T-SQL参数已设定！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                //LogTool.LogMessage("ShortCache", "LoadShort", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 设置参数
                    count = reader.GetInt32(0);
                    // 检查数据
                    if (shorts.ContainsKey(value))
                    {
                        // 设置新值
                        shorts[value] = count;
                    }
                    else
                    {
                        // 增加记录
                        shorts.Add(value, count < 0 ? 0 : count);
                    }
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                //LogTool.LogMessage("ShortCache", "LoadShort", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("ShortCache", "LoadShort", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("ShortCache", "LoadShort", "数据库链接已关闭！");
            }

            // 记录日志
            //LogTool.LogMessage("LogTool", "LoadShort", "数据记录已加载！");
            // 返回结果
            return count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void LoadShorts()
        {
            // 清理数据记录
            shorts.Clear();
            // 记录日志
            LogTool.LogMessage("ShortCache", "LoadShorts", "加载数据记录！");

            // 指令字符串
            string cmdString =
                "SELECT [content], [count] FROM [dbo].[ShortContent];";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("ShortCache", "LoadShorts", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("ShortCache", "LoadShorts", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("ShortCache", "LoadShorts", "T-SQL指令已执行！");

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
                        LogTool.LogMessage("ShortCache", "LoadShorts", 
                            string.Format("{0} shorts loaded !", total));
                    }

                    // 获得内容
                    string value = reader.GetString(0);
                    // 检查结果
                    if (string.IsNullOrEmpty(value)) continue;

                    // 获得计数
                    int count = reader.GetInt32(1);
                    // 检查数据
                    if (shorts.ContainsKey(value))
                    {
                        // 更新记录
                        shorts[value] = count;
                    }
                    else
                    {
                        // 增加记录
                        shorts.Add(value, count < 0 ? 0 : count);
                    }
                }
                // 关闭数据阅读器
                reader.Close();
                // 打印记录
                LogTool.LogMessage("ShortCache", "LoadShorts",
                    string.Format("{0} shorts loaded !", total));
                // 记录日志
                LogTool.LogMessage("ShortCache", "LoadShorts", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("ShortCache", "LoadShorts", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("ShortCache", "LoadShorts", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\tshorts.count = " + shorts.Count);
            // 记录日志
            LogTool.LogMessage("ShortCache", "LoadShorts", "数据记录已加载！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SaveShorts()
        {
            // 记录日志
            LogTool.LogMessage("ShortCache", "SaveShorts", "保存数据记录！");
            LogTool.LogMessage(string.Format("\tshorts.count = {0}", shorts.Count));

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
                foreach (KeyValuePair<string, int> kvp in shorts)
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("ShortCache", "SaveShorts", 
                            string.Format("{0} shorts saved !", total));
                    }
                    // 启动进程
                    tasks.Add(factory.StartNew(() =>
                        AddContent(kvp.Key, kvp.Value)));
                    // 检查任务数量
                    if (tasks.Count >= 10000)
                        { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 打印记录
                LogTool.LogMessage("ShortCache", "SaveShorts",
                    string.Format("{0} shorts saved !", total));
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("ShortCache", "SaveShorts", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                    { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("ShortCache", "MakeStatistic", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\tshorts.count = " + shorts.Count);
            // 记录日志
            LogTool.LogMessage("ShortCache", "SaveShorts", "数据记录已更新！");
        }

        public static void ExtractShorts()
        {
            // 记录日志
            LogTool.LogMessage("ShortCache", "ExtractShorts", "提取短语数据！");

            // 获得数组
            string[] contents = FilteredCache.GetArray();
#if DEBUG
            Debug.Assert(contents != null && contents.Length > 0);
#endif
            // 记录日志
            LogTool.LogMessage(string.Format("\tcount = {0}", contents.Length));

            // 计数器
            int total = 0;
            // 任务数组
            List<Task> tasks = new List<Task>();
            // 生成任务控制器
            TaskFactory factory = new TaskFactory(
                new LimitedConcurrencyLevelTaskScheduler(DBTool.MAX_THREADS));

            try
            {
                // 循环处理
                foreach (string content in contents)
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("ShortCache", "ExtractShorts",
                            string.Format("{0} items processed !", total));
                    }

                    // 检查内容
                    if (string.IsNullOrEmpty(content)) continue;

                    // 启动进程
                    tasks.Add(factory.StartNew
                    (() =>
                    {
                        // 检查结果
                        StringBuilder sb = new StringBuilder();
                        // 循环处理
                        for (int i = 0; i < content.Length; i++)
                        {
                            // 将非中文字符替换成空格
                            sb.Append(ChineseTool.
                                IsChinese(content[i]) ? content[i] : ' ');
                        }
                        // 分解结果
                        string[] values = sb.ToString().Split(' ');
                        // 循环处理
                        foreach (string value in values)
                        {
                            // 检查参数
                            if (string.IsNullOrEmpty(value)) continue;
                            // 检查参数
                            if (value.Length <= 1 || MiscTool.IsTooLong(value)) continue;

                            // 同步锁定
                            lock (shorts)
                            {
                                // 检查关键字
                                if (shorts.
                                    ContainsKey(value))
                                {
                                    shorts[value]++;
                                }
                                // 增加数据记录
                                else shorts.Add(value, 1);
                            }
                        }
                    }));

                    // 检查任务数量
                    if (tasks.Count >= 10000) { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 打印记录
                LogTool.LogMessage("ShortCache", "ExtractShorts",
                    string.Format("{0} items processed !", total));
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("ShortCache", "ExtractShorts", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 记录日志
                LogTool.LogMessage(string.Format("\tshorts.count = {0}", total));

                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("ShortCache", "ExtractShorts", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("ShortCache", "ExtractShorts", "短语提取完毕！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void MakeStatistic()
        {
            // 记录日志
            LogTool.LogMessage("ShortCache", "MakeStatistic", "开始统计数据！");

            // 获得最大长度
            int length = DictionaryContent.GetMaxLength();
            // 检查结果
            if (length <= 0) length = 64;
            // 记录日志
            LogTool.LogMessage(string.Format("\tmax length = {0}", length));

            // 获得数组
            string[] contents = FilteredCache.GetArray();
#if DEBUG
            Debug.Assert(contents != null && contents.Length > 0);
#endif
            // 记录日志                 
            LogTool.LogMessage(string.Format("\tlength({0}).count = {1}", length, contents.Length));

            // 计数器
            int total = 0;
            // 任务数组
            List<Task> tasks = new List<Task>();
            // 生成任务控制器
            TaskFactory factory = new TaskFactory(
                new LimitedConcurrencyLevelTaskScheduler(DBTool.MAX_THREADS));

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
                        LogTool.LogMessage("ShortCache", "MakeStatistic",
                            string.Format("{0} items processed !", total));
                    }

                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;

                    // 启动进程
                    tasks.Add(factory.StartNew
                    (() =>
                    {
                        // 执行循环
                        for (int j = 2; j <= length && j <= content.Length; j++)
                        {
                            // 检查长度
                            for (int i = 0; i <= content.Length - j; i++)
                            {
                                // 获得子字符串
                                string value = content.Substring(i, j);
#if DEBUG
                                Debug.Assert(value != null && value.Length == j);
#endif
                                // 检查是否为中文
                                if (!ChineseTool.IsChinese(value)) continue;
                                // 增加计数器
                                lock (shorts)
                                {
                                    if (shorts.ContainsKey(value)) shorts[value]++;
                                }
                            }
                        }
                    }));

                    // 检查任务数量
                    if (tasks.Count >= 10000) { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 记录日志
                LogTool.LogMessage("ShortCache", "MakeStatistic",
                    string.Format("{0} items processed !", total));
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("ShortCache", "MakeStatistic", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                    { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("ShortCache", "MakeStatistic", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\tshorts.count = " + shorts.Count);
            // 记录日志
            LogTool.LogMessage("ShortCache", "MakeStatistic", "数据统计完毕！");
        }

#if _USE_CONSOLE
        public static new void Main(string[] args)
        {
            Console.WriteLine("准备统计ShortContent频次，原表及其数据将被更改！");
            Console.WriteLine("确认是否执行（Yes/No）？");
            // 检查确认
            if (!ConsoleTool.Confirm()) return;
            // 开启日志
            LogTool.SetLog(true);
            // 创建计时器
            Stopwatch watch = new Stopwatch();
            // 开启计时器
            watch.Start();
            // 加载数据
            FilteredCache.LoadContents();
            // 创建数据表
            CreateTable();
            // 开始统计
            MakeStatistic();
            // 保存结果
            SaveShorts();
            // 关闭计时器
            watch.Stop();
            // 打印结果
            Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
        }
#endif
    }
}
