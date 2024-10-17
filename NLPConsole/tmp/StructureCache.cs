using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace NLP
{
    public class StructureCache
    {
        // 创建数据字典
        private static Dictionary<int, FunctionSegment>
            structures = new Dictionary<int, FunctionSegment>();

        [MethodImpl(MethodImplOptions.Synchronized)]
#if _USE_CLR
        public static FunctionSegment GetStructure(int sid)
#elif _USE_CONSOLE
        public static FunctionSegment? GetStructure(int sid)
#endif
        {
            // 返回结果
            return structures.ContainsKey(sid) ?
                structures[sid] : LoadStructure(sid);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ClearStructures()
        {
            // 记录日志
            LogTool.LogMessage("StructureCache", "ClearStructures", "清理数据记录！");
            // 清理数据
            structures.Clear();
            // 记录日志
            LogTool.LogMessage("StructureCache", "ClearStructures", "数据清理完毕！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
#if _USE_CLR
        public static FunctionSegment LoadStructure(int sid)
#elif _USE_CONSOLE
        public static FunctionSegment? LoadStructure(int sid)
#endif
        {
            // 记录日志
            //LogTool.LogMessage("StructureCache", "LoadStructure", "加载数据记录！");

#if _USE_CLR
            // 计数器
            FunctionSegment segment = null;
#elif _USE_CONSOLE
            // 计数器
            FunctionSegment? segment = null;
#endif
            // 指令字符串
            string cmdString =
                "SELECT TOP 1 [count], [length], [structure] " +
                "FROM [dbo].[StructureContent] WHERE [sid] = @SqlSid;";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("StructureCache", "LoadStructure", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                //LogTool.LogMessage("StructureCache", "LoadStructure", "T-SQL指令已创建！");

                // 设置参数
                sqlCommand.Parameters.AddWithValue("SqlSid", sid);
                // 记录日志
                //LogTool.LogMessage("StructureCache", "LoadStructure", "T-SQL参数已设定！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                //LogTool.LogMessage("StructureCache", "LoadStructure", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 检查字段
                    if (reader.IsDBNull(2)) continue;
                    // 读取内容
                    segment = new FunctionSegment();
                    // 设置参数
                    segment.Index = sid;
                    segment.Count = reader.GetInt32(0);
                    segment.Length = reader.GetInt32(1);
                    segment.Value = reader.GetString(2);
                    // 检查结果
                    if(segment.Value != null && segment.Value.Length > 0)
                    {
                        // 增加记录
                        if (!structures.ContainsKey(sid)) structures.Add(sid, segment);
                    }
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                //LogTool.LogMessage("StructureCache", "LoadStructure", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("StructureCache", "LoadStructure", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("StructureCache", "LoadStructure", "数据库链接已关闭！");
            }

            // 记录日志
            //LogTool.LogMessage("TokenTool", "LoadStructure", "数据记录已加载！");
            // 返回结果
            return segment;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void LoadStructures()
        {
            // 记录日志
            LogTool.LogMessage("StructureCache", "LoadStructures", "加载数据记录！");

            // 指令字符串
            string cmdString =
                "SELECT [sid], [count], [length], [structure] FROM [dbo].[StructureContent];";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("StructureCache", "LoadStructures", "数据链接已建立！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("StructureCache", "LoadStructures", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("StructureCache", "LoadStructures", "T-SQL指令已执行！");

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
                        LogTool.LogMessage("StructureCache", "LoadStructures",
                            string.Format("{0} tokens loaded !", total));
                    }

                    // 检查字段
                    if (reader.IsDBNull(3)) continue;
                    // 读取内容
                    FunctionSegment segment = new FunctionSegment();
                    // 设置参数
                    segment.Index = reader.GetInt32(0);
                    segment.Count = reader.GetInt32(1);
                    segment.Length = reader.GetInt32(2);
                    segment.Value = reader.GetString(3);
                    // 检查结果
                    if (segment.Value != null &&
                            segment.Value.Length > 0)
                    {
                        // 检查结果
                        if (structures.ContainsKey(segment.Index))
                        {
                            // 更新数据
                            structures[segment.Index] = segment;
                        }
                        else
                        {
                            // 增加记录
                            structures.Add(segment.Index, segment);
                        }
                    }
                }
                // 打印记录
                LogTool.LogMessage(string.Format("{0} structures loaded !", total));

                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("StructureCache", "LoadStructures", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("StructureCache", "LoadStructures", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("StructureCache", "LoadStructures", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\tstructures.count = " + structures.Count);
            // 记录日志
            LogTool.LogMessage("StructureCache", "LoadStructures", "数据记录已加载！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SaveStructures()
        {
            // 记录日志
            LogTool.LogMessage("StructureCache", "SaveStructures", "保存数据记录！");
            LogTool.LogMessage(string.Format("\tstructures.count = {0}", structures.Count));

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
                foreach (KeyValuePair<int, FunctionSegment> kvp in structures)
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("StructureCache", "SaveStructures",
                            string.Format("{0} tokens saved !", total));
                    }
                    // 启动进程
                    tasks.Add(factory.StartNew(() =>
                        StructureContent.AddContent(kvp.Value)));
                    // 检查任务数量
                    if (tasks.Count >= 10000)
                    { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 打印记录
                LogTool.LogMessage("StructureCache", "SaveStructures",
                    string.Format("{0} tokens saved !", total));
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("StructureCache", "SaveStructures", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("StructureCache", "MakeStatistic", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\tstructures.count = " + structures.Count);
            // 记录日志
            LogTool.LogMessage("StructureCache", "SaveStructures", "数据记录已保存！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void MakeStatistic()
        {
            // 记录日志
            LogTool.LogMessage("StructureCache", "MakeStatistic", "开始统计数据！");

            // 指令字符串
            string cmdString =
                "SELECT [content] FROM [dbo].[ShortContent];";

            // 计数器
            int total = 0;
            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("StructureCache", "MakeStatistic", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("StructureCache", "MakeStatistic", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("StructureCache", "MakeStatistic", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("StructureCache", "MakeStatistic",
                            string.Format("{0} items processed !", total));
                    }

                    // 获得内容
                    string content = reader.GetString(0);
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;
                    // 检查长度
                    if (content.Length <= 1 || content.Length > 64) continue;

                    // 循环处理
                    foreach (KeyValuePair<int, FunctionSegment> kvp in structures)
                    {
                        // 获得对象
                        FunctionSegment segment = kvp.Value;
#if DEBUG
                        Debug.Assert(segment != null && segment.Value != null);
#endif
                        // 获得规则
                        string rule = segment.Value;
                        // 检查结果
                        if (rule == null || rule.Length <= 0) continue;
                        // 替换标识
                        rule = rule.Replace("$z",
                            Quantity.GetRuleString("$z"));
                        // 检查结果
                        if (rule == null || rule.Length <= 0) continue;

                        // 只处理中文
                        Match match = Regex.Match(content, rule);
                        // 检查结果
                        while (match.Success)
                        {
#if _USE_CLR
                            // 查找对象
                            CoreSegment coreSegment =
                                PhraseCache.GetPhrase(match.Value);
#elif _USE_CONSOLE
                            // 查找对象
                            CoreSegment? coreSegment =
                                PhraseCache.GetPhrase(match.Value);
#endif
                            /*
                            // 打印记录
                            LogTool.LogMessage("StructureCache", "MakeStatistic",
                                string.Format("match[{0}] = \"{1}\" ({2})",
                                    kvp.Key, match.Value, segment.Value));
                            */
                            // 检查结果
                            if (coreSegment == null)
                            {
                                // 生成对象
                                coreSegment = new CoreSegment();
                                // 设置参数
                                coreSegment.Count = 1;
                                coreSegment.Sid = kvp.Key;
                                coreSegment.Gamma = 0.0f;
                                coreSegment.Value = match.Value;
                                coreSegment.Length = match.Value.Length;
                                // 增加内容
                                PhraseContent.AddContent(coreSegment);
                                // 从数据表加载数据
                                if (PhraseCache.GetCount(match.Value) != 1)
                                {
                                    // 打印记录
                                    LogTool.LogMessage("StructureCache", "MakeStatistic",
                                        string.Format("未能加载数据(\"{0}\")！", match.Value));
                                }
                            }
                            else
                            {
                                // 检查编号
                                if (coreSegment.Sid <= 0 ||
                                    coreSegment.Sid == segment.Index)
                                {
                                    // 增加计数
                                    segment.Count++;
                                    // 增加计数
                                    coreSegment.Count++;
                                    // 设置数值
                                    coreSegment.Sid = segment.Index;
                                }
                                else
                                {
#if _USE_CLR
                                    // 比较匹配字符串
                                    FunctionSegment structure = null;
#elif _USE_CONSOLE
                                    // 比较匹配字符串
                                    FunctionSegment? structure = null;
#endif
                                    // 检查数据
                                    if (structures.ContainsKey(coreSegment.Sid))
                                        structure = structures[coreSegment.Sid];
                                    // 检查结果
                                    if (structure != null)
                                    {
                                        // 检查长度
                                        if (structure.Length > segment.Length)
                                        {
                                            // 增加计数
                                            segment.Count++;
                                        }
                                        else if (structure.Length < segment.Length)
                                        {
                                            // 增加计数
                                            segment.Count++;
                                            // 增加计数
                                            coreSegment.Count++;
                                            // 设置数值
                                            coreSegment.Sid = segment.Index;
                                        }
                                        else
                                        {
                                            // 打印记录
                                            LogTool.LogMessage("StructureCache", "MakeStatistic",
                                                string.Format("不应该出现的情况(\"{0}\")！", match.Value));
                                        }
                                    }
                                }
                            }
                            //下一个匹配项目
                            match = match.NextMatch();
                        }
                    }
                }
                // 关闭数据阅读器
                reader.Close();
                // 打印记录
                LogTool.LogMessage("StructureCache", "MakeStatistic",
                    string.Format("{0} items processed !", total));
                // 记录日志
                LogTool.LogMessage("StructureCache", "MakeStatistic", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("StructureCache", "MakeStatistic", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("StructureCache", "MakeStatistic", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\tstructures.count = " + structures.Count);
            // 记录日志
            LogTool.LogMessage("StructureCache", "MakeStatistic", "数据统计完毕！");
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备统计结构频次，过程将会十分耗时！");
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
                if (!DBTool.TableExists("StructureContent"))
                {
                    // 记录日志
                    Console.WriteLine("数据表StructureContent不存在！");
                }
                else
                {
                    // 加载所有对象
                    PhraseCache.LoadPhrases();
                    // 加载所有对象
                    LoadStructures();
                    // 开始统计
                    MakeStatistic();
                    // 保存所有对象
                    SaveStructures();
                    // 保存所有对象
                    PhraseCache.SavePhrases();
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
