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
    public class CoreCache : CoreContent
    {
        // 创建数据字典
        private static Dictionary<string, CoreSegment>
            segments = new Dictionary<string, CoreSegment>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ClickCount(string value, int count = 1)
        {
#if DEBUG
            Debug.Assert(count > 0);
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 增加计数
            // 假设所有数据都加入到内存
            if (segments.ContainsKey(value)) segments[value].Count += count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int GetCount(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif

#if _USE_CLR
            CoreSegment segment = GetSegment(value);
#elif _USE_CONSOLE
            CoreSegment? segment = GetSegment(value);
#endif
            // 返回结果
            return segment != null ? segment.Count : 0;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static double GetGamma(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif

#if _USE_CLR
            CoreSegment segment = GetSegment(value);
#elif _USE_CONSOLE
            CoreSegment? segment = GetSegment(value);
#endif
            // 返回结果
            return segment != null ? segment.Gamma : -1.0f;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ClearSegments()
        {
            // 记录日志
            LogTool.LogMessage("CoreCache", "ClearSegments", "开始执行！");
            // 恢复初始值
            segments.Clear();
            // 记录日志
            LogTool.LogMessage("CoreCache", "ClearSegments", "执行完毕！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static new void SetPath(string value, string path)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(path));
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 设置强制路径
            CoreContent.SetPath(value, path);
            // 删除缓冲（保持数据同步）
            if (segments.ContainsKey(value)) segments.Remove(value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void AddSegment(CoreSegment segment)
        {
#if DEBUG
            Debug.Assert(segment != null);
            Debug.Assert(!segment.IsNullOrEmpty());
#endif
            // 在数据表中增加数据
            AddContent(segment);

#if _USE_CLR
            // 获得参数
            string value = segment.Value;
#elif _USE_CONSOLE
            // 获得参数
            string? value = segment.Value;
#endif
#if DEBUG
            Debug.Assert(value != null);
#endif
            // 删除缓冲（保持数据同步）
            if (segments.ContainsKey(value)) segments.Remove(value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void AddSegment(string value, int count)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 在数据表中增加数据
            AddContent(value, count);
            // 保持数据同步
            if(segments.ContainsKey(value)) segments[value].Count = count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
#if _USE_CLR
        public static CoreSegment GetSegment(string value)
#elif _USE_CONSOLE
        public static CoreSegment? GetSegment(string value)
#endif
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 返回结果
            return segments.ContainsKey(value) ?
                segments[value] : LoadSegment(value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void DeleteSegment(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 从数据表中删除
            DeleteContent(value);
            // 从缓冲中删除
            if (segments.ContainsKey(value)) segments.Remove(value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
#if _USE_CLR
        public static CoreSegment LoadSegment(string value)
#elif _USE_CONSOLE
        public static CoreSegment? LoadSegment(string value)
#endif
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 记录日志
            //LogTool.LogMessage("CoreCache", "LoadSegment", "加载数据记录！");

            // 指令字符串
            string cmdString =
                "SELECT TOP 1 [cid], [flag], [count], [gamma], [segment] " +
                "FROM [dbo].[CoreContent] WHERE [content] = @SqlContent;";

#if _USE_CLR
            // 对象
            CoreSegment segment = null;
#elif _USE_CONSOLE
            // 对象
            CoreSegment? segment = null;
#endif
            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("CoreCache", "LoadSegment", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 设置参数
                sqlCommand.Parameters.AddWithValue("SqlContent", value);
                // 记录日志
                //LogTool.LogMessage("CoreCache", "LoadSegment", "参数已设定！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                //LogTool.LogMessage("CoreCache", "LoadSegment", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 生成对象
                    segment = new CoreSegment();
                    // 设置参数值
                    segment.Value = value;
                    segment.Length = value.Length;
                    // 设置参数值
                    segment.Index = reader.GetInt32(0);
                    segment.Flag = reader.GetInt32(1);
                    segment.Count = reader.GetInt32(2);
                    segment.Gamma = reader.GetDouble(3);
                    // 设置路径
                    if (!reader.IsDBNull(4)) segment.SetPath(reader.GetString(4));
                    // 检查数据
                    if (!segments.ContainsKey(value)) segments.Add(value, segment);
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                //LogTool.LogMessage("CoreCache", "LoadSegment", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("CoreCache", "LoadSegment", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("CoreCache", "LoadSegment", "数据库链接已关闭！");
            }

            // 记录日志
            //LogTool.LogMessage("CoreCache", "LoadSegment", "数据记录已加载！");
            // 返回结果
            return segment;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int LoadSegments(int length = -1)
        {
            // 清理数据记录
            segments.Clear();
            // 记录日志
            LogTool.LogMessage("CoreCache", "LoadSegments", "加载数据记录！");
            LogTool.LogMessage(string.Format("\tlength = {0}", length));

            // 指令字符串
            string cmdString;
            // 检查长度
            if(length <= 0)
            {
                cmdString = "SELECT [cid], [flag], [count], " +
                    "[content], [gamma], [segment] FROM [dbo].[CoreContent];";
            }
            else
            {
                cmdString = string.Format(
                    "SELECT [cid], [flag], [count], " +
                    "[content], [gamma], [segment] " +
                    "FROM [dbo].[CoreContent] WHERE [length] = {0};", length);
            }

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("CoreCache", "LoadSegments", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("CoreCache", "LoadSegments", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("CoreCache", "LoadSegments", "T-SQL指令已执行！");

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
                        LogTool.LogMessage("CoreCache", "LoadSegments",
                            string.Format("{0} items loaded !", total));
                    }

                    // 检查参数
                    if (reader.IsDBNull(3)) continue;
                    // 获得内容
                    string value = reader.GetString(3);
                    // 检查结果
                    if (string.IsNullOrEmpty(value)) continue;

                    // 生成对象
                    CoreSegment segment = new CoreSegment();
                    // 设置参数值
                    segment.Value = value;
                    segment.Length = value.Length;
                    // 设置参数值
                    segment.Index = reader.GetInt32(0);
                    segment.Flag = reader.GetInt32(1);
                    segment.Count = reader.GetInt32(2);
                    segment.Gamma = reader.GetDouble(4);
                    // 设置路径
                    if(!reader.IsDBNull(5))
                        segment.SetPath(reader.GetString(5));
                    // 检查数据
                    if (segments.ContainsKey(value))
                    {
                        // 设置新数值
                        segments[value] = segment;
                    }
                    // 增加对象
                    else segments.Add(value, segment);
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("CoreCache", "LoadSegments",
                    string.Format("{0} items loaded !", total));
                // 记录日志
                LogTool.LogMessage("CoreCache", "LoadSegments", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("CoreCache", "LoadSegments", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("CoreCache", "LoadSegments", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\tsegments.count = " + segments.Count);
            // 记录日志
            LogTool.LogMessage("CoreCache", "LoadSegments", "数据记录已加载！");
            // 返回结果
            return segments.Count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SaveSegments()
        {
            // 记录日志
            LogTool.LogMessage("CoreCache", "SaveSegments", "保存数据记录！");
            LogTool.LogMessage(string.Format("\tsegments.count = {0}", segments.Count));

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
                foreach (KeyValuePair<string, CoreSegment> kvp in segments)
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("CoreCache", "SaveSegments",
                                string.Format("{0} items saved !", total));
                    }

                    // 启动进程
                    tasks.Add(factory.StartNew(()
                            => AddContent(kvp.Value)));
                    // 检查任务数量
                    if (tasks.Count >= 10000)
                        { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 打印记录
                LogTool.LogMessage("CoreCache", "SaveSegments",
                        string.Format("{0} items saved !", total));
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("CoreCache", "SaveSegments", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                    { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("CoreCache", "SaveSegments", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\tsegments.count = " + segments.Count);
            // 记录日志
            LogTool.LogMessage("CoreCache", "SaveSegments", "数据记录已保存！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void MakeStatistic(int length = -1)
        {
            // 记录日志
            LogTool.LogMessage("CoreCache", "MakeStatistic", "开始统计数据！");

            // 获得最大长度
            int maxLength = GetMaxLength();
            // 检查参数
            if (length > maxLength) length = maxLength;
            // 记录日志
            LogTool.LogMessage(string.Format("\tlength = {0}", length));
            LogTool.LogMessage(string.Format("\tmax length = {0}", maxLength));

            // 计数器
            int total = 0;
            // 任务数组
            List<Task> tasks = new List<Task>();
            // 生成任务控制器
            TaskFactory factory = new TaskFactory(
                new LimitedConcurrencyLevelTaskScheduler(DBTool.MAX_THREADS));

            // 获得数组
            string[] contents = FilteredCache.GetArray(length);
#if DEBUG
            Debug.Assert(contents != null && contents.Length > 0);
#endif
            // 记录日志
            LogTool.LogMessage(string.Format("\tlength{0}.count = {1}", length, contents.Length));

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
                        LogTool.LogMessage("CoreCache", "MakeStatistic",
                            string.Format("{0} items processed !", total));
                    }

                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;

                    // 启动进程
                    tasks.Add(factory.StartNew
                    (() =>
                    {
                        // 检查长度
                        if (length > 0)
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
                                int index = ChineseTool.NonChinese(value);
                                // 检查结果
                                if (index >= 0) { i += index; continue; }
                                // 增加计数器
                                lock (segments)
                                {
                                    // 增加数值
                                    if (segments.ContainsKey(value)) segments[value].Count++;
                                }
                            }
                        }
                        else
                        {
                            // 执行循环
                            for (int j = 2; j <= maxLength && j <= content.Length; j++)
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
                                    int index = ChineseTool.NonChinese(value);
                                    // 检查结果
                                    if (index >= 0) { i += index; continue; }
                                    // 增加计数器
                                    lock (segments)
                                    {
                                        // 查询
                                        if (segments.ContainsKey(value)) segments[value].Count++;
                                    }
                                }
                            }
                        }
                    }));

                    // 检查任务数量
                    if (tasks.Count >= 10000) { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 记录日志
                LogTool.LogMessage("CoreCache", "MakeStatistic",
                    string.Format("{0} items processed !", total));
                // 记录日志
                LogTool.LogMessage("CoreCache", "MakeStatistic", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("CoreCache", "MakeStatistic", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("CoreCache", "MakeStatistic", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\tsegments.Count = " + segments.Count);
            // 记录日志
            LogTool.LogMessage("CoreCache", "MakeStatistic", "数据统计完毕！");
        }

#if _USE_CONSOLE
        public static new void Main(string[] args)
        {
            Console.WriteLine("准备统计字符串频次，过程将会十分耗时！");
            Console.WriteLine("确认是否执行（Yes/No）？");
            // 是否确认
            if (!ConsoleTool.Confirm()) return;

            // 缺省起始长度
            int length = 2;
            // 获得最大长度
            int maxLength =
                CoreContent.GetMaxLength();
            // 请输入起始长度
            Console.WriteLine("请输入起始长度！");
            // 读取一行
            string line = Console.ReadLine()!;
            // 检查结果
            if (line == null || line.Length <= 0)
            {
                Console.WriteLine("将使用缺省长度！");
            }
            else if (!int.TryParse(line, out length))
            {
                Console.WriteLine("将使用缺省长度！");
            }
            else if (length > maxLength)
            {
                Console.WriteLine("将使用缺省长度！");
            }
            else
            {
                Console.WriteLine(string.Format("使用起始长度：{0}！", length));
            }

            // 开启日志
            LogTool.SetLog(true);
            // 创建计时器
            Stopwatch watch = new Stopwatch();
            // 开启计时器
            watch.Start();
            // 检查数据表
            if (!DBTool.TableExists("CoreContent"))
            {
                // 记录日志
                Console.WriteLine("数据表CoreContent不存在！"); return;
            }
            // 检查数据表
            if (!DBTool.TableExists("FilteredContent"))
            {
                // 记录日志
                Console.WriteLine("数据表FilteredContent不存在！"); return;
            }
            // 加载数据
            FilteredCache.LoadContents();
            // 循环处理
            for(int i = length; i < maxLength; i++)
            {
                // 恢复初始值
                DBTool.ExecuteNonQuery(string.Format(
                    "UPDATE [dbo].[CoreContent] " +
                    "SET [count] = 1 WHERE [length] = {0};", i));
                // 加载所有对象
                // 开始统计和保存数据
                if (LoadSegments(i) > 0) { MakeStatistic(i); SaveSegments(); }
            }
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
