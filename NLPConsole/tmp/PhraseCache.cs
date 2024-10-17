using System;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NLP
{
    public class PhraseCache
    {
        // 创建数据字典
        private static Dictionary<string, CoreSegment>
            phrases = new Dictionary<string, CoreSegment>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int GetCount(string value)
        {
#if _USE_CLR
            CoreSegment segment = GetPhrase(value);
#elif _USE_CONSOLE
            CoreSegment? segment = GetPhrase(value);
#endif
            // 返回结果
            return segment != null ? segment.Count : 0;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ClearPhrases()
        {
            // 记录日志
            LogTool.LogMessage("PhraseCache", "ClearPhrases", "清理数据记录！");
            // 清理数据
            phrases.Clear();
            // 记录日志
            LogTool.LogMessage("PhraseCache", "ClearPhrases", "数据清理完毕！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
#if _USE_CLR
        public static CoreSegment GetPhrase(string value)
#elif _USE_CONSOLE
        public static CoreSegment? GetPhrase(string value)
#endif
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 返回结果
            return phrases.ContainsKey(value) ?
                phrases[value] : LoadPhrase(value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
#if _USE_CLR
        public static CoreSegment LoadPhrase(string value)
#elif _USE_CONSOLE
        public static CoreSegment? LoadPhrase(string value)
#endif
        {
            // 记录日志
            //LogTool.LogMessage("PhraseCache", "LoadPhrase", "加载数据记录！");

            // 指令字符串
            string cmdString =
                "SELECT TOP 1 " +
                "[pid], [sid], [count], [gamma], [segment] " +
                "FROM [dbo].[PhraseContent] WHERE [content] = @SqlContent;";

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
                //LogTool.LogMessage("PhraseCache", "LoadPhrase", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 设置参数
                sqlCommand.Parameters.AddWithValue("SqlContent", value);
                // 记录日志
                //LogTool.LogMessage("PhraseCache", "LoadPhrase", "参数已设定！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                //LogTool.LogMessage("PhraseCache", "LoadPhrase", "T-SQL指令已执行！");

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
                    segment.Sid = reader.GetInt32(1);
                    segment.Count = reader.GetInt32(2);
                    segment.Gamma = reader.GetDouble(3);
                    // 设置路径
                    if (!reader.IsDBNull(4)) segment.SetPath(reader.GetString(4));
                    // 检查数据
                    if (!phrases.ContainsKey(value)) phrases.Add(value, segment);
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                //LogTool.LogMessage("PhraseCache", "LoadPhrase", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("PhraseCache", "LoadPhrase", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("PhraseCache", "LoadPhrase", "数据库链接已关闭！");
            }

            // 记录日志
            //LogTool.LogMessage("PhraseCache", "LoadPhrase", "数据记录已加载！");
            // 返回结果
            return segment;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void LoadPhrases()
        {
            // 记录日志
            LogTool.LogMessage("PhraseCache", "LoadPhrases", "加载数据记录！");

            // 指令字符串
            string cmdString =
                "SELECT [pid], [sid], [count], [content], [gamma], [segment] FROM [dbo].[PhraseContent];";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("PhraseCache", "LoadPhrases", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("PhraseCache", "LoadPhrases", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("PhraseCache", "LoadPhrases", "T-SQL指令已执行！");

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
                        LogTool.LogMessage("SegmentTool", "LoadPhrases",
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
                    segment.Sid = reader.GetInt32(1);
                    segment.Count = reader.GetInt32(2);
                    segment.Gamma = reader.GetDouble(4);
                    // 设置路径
                    if (!reader.IsDBNull(5))
                        segment.SetPath(reader.GetString(5));
                    // 检查数据
                    if (phrases.ContainsKey(value))
                    {
                        // 设置新数值
                        phrases[value] = segment;
                    }
                    // 增加对象
                    else phrases.Add(value, segment);
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("SegmentTool", "LoadPhrases",
                    string.Format("{0} items loaded !", total));
                // 记录日志
                LogTool.LogMessage("PhraseCache", "LoadPhrases", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("PhraseCache", "LoadPhrases", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("PhraseCache", "LoadPhrases", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\tphrases.count = " + phrases.Count);
            // 记录日志
            LogTool.LogMessage("PhraseCache", "LoadPhrases", "数据记录已加载！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SavePhrases()
        {
            // 记录日志
            LogTool.LogMessage("PhraseCache", "SavePhrases", "保存数据记录！");
            LogTool.LogMessage(string.Format("\titems.count = {0}", phrases.Count));

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
                foreach (KeyValuePair<string, CoreSegment> kvp in phrases)
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("PhraseCache", "SavePhrases",
                                string.Format("{0} items saved !", total));
                    }

                    // 启动进程
                    tasks.Add(factory.StartNew(() =>
                        PhraseContent.AddContent(kvp.Value)));
                    // 检查任务数量
                    if (tasks.Count >= 10000)
                    { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 打印记录
                LogTool.LogMessage("PhraseCache", "SavePhrases",
                        string.Format("{0} items saved !", total));
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("PhraseCache", "SavePhrases", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("PhraseCache", "SavePhrases", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\tphrases.count = " + phrases.Count);
            // 记录日志
            LogTool.LogMessage("PhraseCache", "SavePhrases", "数据记录已保存！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void MakeStatistic()
        {
            // 记录日志
            LogTool.LogMessage("PhraseCache", "MakeStatistic", "开始统计数据！");

            // 循环处理
            foreach (KeyValuePair<string, CoreSegment>
                        kvp in phrases) kvp.Value.Count = 0;
            // 记录日志
            LogTool.LogMessage("PhraseCache", "MakeStatistic", "所有数据清零！");

            // 计数器
            int total = 0;
            // 任务数组
            List<Task> tasks = new List<Task>();
            // 生成任务控制器
            TaskFactory factory = new TaskFactory(
                new LimitedConcurrencyLevelTaskScheduler(DBTool.MAX_THREADS));

            // 指令字符串
            string cmdString =
                "SELECT [content] FROM [dbo].[ShortContent];";
            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 获得最大长度
                int length = PhraseContent.GetMaxLength();
                // 检查结果
                if (length <= 0) length = 64;
                // 记录日志
                LogTool.LogMessage(string.Format("\tmax length = {0}", length));

                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("PhraseCache", "MakeStatistic", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("PhraseCache", "MakeStatistic", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("PhraseCache", "MakeStatistic", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 增加计数
                    total++;
                    // 检查结果
                    if (total % 10000 == 0)
                    {
                        // 记录日志
                        LogTool.LogMessage("PhraseCache", "MakeStatistic",
                            string.Format("{0} items processed !", total));
                    }

                    // 获得内容
                    string content = reader.GetString(0);
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;

                    // 启动进程
                    tasks.Add(factory.StartNew
                    (() =>
                    {
                        // 执行循环
                        for (int j = 2; j <= length; j++)
                        {
                            // 检查长度
                            for (int i = 0; i <= content.Length - j; i++)
                            {
                                // 获得子字符串
                                string value = content.Substring(i, j);
                                // 检查结果
                                if (value == null || value.Length != j) continue;

                                // 检查结果
                                if (phrases.ContainsKey(value))
                                {
                                    // 增加计数器
                                    lock (phrases[value]) phrases[value].Count++;
                                }
                            }
                        }
                    }));

                    // 检查任务数量
                    if (tasks.Count >= 10000) { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("PhraseCache", "MakeStatistic",
                    string.Format("{0} items processed !", total));
                // 记录日志
                LogTool.LogMessage("PhraseCache", "MakeStatistic", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("PhraseCache", "MakeStatistic", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("PhraseCache", "MakeStatistic", "任务全部结束！");

                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("PhraseCache", "MakeStatistic", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\tphrases.Count = " + phrases.Count);
            // 记录日志
            LogTool.LogMessage("PhraseCache", "MakeStatistic", "数据统计完毕！");
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备统计Prase频次，过程将会十分耗时！");
            Console.WriteLine("确认是否执行（Yes/No）？");

            // 命令行
            string line;
            // 循环处理
            do
            {
                // 读取一行
                line = Console.ReadLine()!;
                // 检查结果
                if (line == null || line.Length <= 0)
                {
                    Console.WriteLine("请输入正确的操作符！");
                    Console.WriteLine("确认是否执行（Yes/No）？");
                }
                else
                {
                    if (line.Equals("Yes"))
                    {
                        Console.WriteLine("开始执行操作！"); break;
                    }
                    else if (line.Equals("No"))
                    {
                        Console.WriteLine("放弃当前操作！"); break;
                    }
                    else
                    {
                        Console.WriteLine("无效的操作符！");
                        Console.WriteLine("请确认是否执行（Yes/No）？");
                    }
                }
            } while (true);

            // 检查输入行
            if (line.Equals("Yes"))
            {
                // 开启日志
                LogTool.SetLog(true);
                // 创建计时器
                Stopwatch watch = new Stopwatch();
                // 开启计时器
                watch.Start();
                // 检查数据表
                if (!DBTool.TableExists("ShortContent"))
                {
                    // 记录日志
                    Console.WriteLine("数据表ShortContent不存在！");
                }
                else if (!DBTool.TableExists("PhraseContent"))
                {
                    // 记录日志
                    Console.WriteLine("数据表PhraseContent不存在！");
                }
                else
                {
                    // 加载所有对象
                    LoadPhrases();
                    // 开始统计
                    MakeStatistic();
                    // 保存所有对象
                    SavePhrases();
                    // 记录日志
                    Console.WriteLine("数据统计已完成！");
                }
                // 关闭计时器
                watch.Stop();
                // 打印结果
                Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
            }
        }
#endif
    }
}
