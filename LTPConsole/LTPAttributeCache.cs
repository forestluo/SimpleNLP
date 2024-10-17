using NLP;
using System;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace LTP
{
    public class LTPAttributeCache : LTPAttributeContent
    {
        // 创建数据字典
        private static Dictionary<string, int>
            attributes = new Dictionary<string, int>();

        public static string FormatKey(string content, string pos, string ne)
        {
            // 返回结果
            return string.Format("{0}({1}:{2})", content, pos, ne);
        }

#if _USE_CLR
        public static string[] ParseKey(string value)
#elif _USE_CONSOLE
        public static string[]? ParseKey(string value)
#endif
        {
            // 正则匹配
            MatchCollection matches = Regex.Matches(value,
                "([\u4E00-\u9FA5]+)\\(([a-z][a-z]?):(O|[A-Z]-N[a-z])\\)");
            // 检查结果
            if (matches.Count != 1) return null;

            // 获得第一个
            GroupCollection groups = matches.First().Groups;
            // 检查结果
            if (groups.Count != 4) return null;

            // 创建字符串
            string[] output = new string[3];
            // 提取匹配项内的分组信息
            for (int i = 0; i < groups.Count && i < 3; i++) output[i] = groups[i + 1].Value;
            // 返回结果
            return output;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int GetCount(string value)
        {
            // 返回结果
            // 组合描述"content([pos]:[ne])"
            return attributes.ContainsKey(value) ?
                attributes[value] : LoadAttribute(value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void AddAttribute(string[] inputs, int count)
        {
#if DEBUG
            Debug.Assert(inputs != null && inputs.Length == 3);
#endif
            // 加入到数据库
            AddContent(inputs, count);
            // 获得参数
            string value =
                FormatKey(inputs[0], inputs[1], inputs[2]);
            // 检查数据
            if (attributes.ContainsKey(value))
            {
                // 增加计数
                attributes[value] ++;
            }
            // 增加数据
            else attributes.Add(value, count);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int LoadAttribute(string value)
        {
#if _USE_CLR
            // 获得参数
            string[] parameters = 
#elif _USE_CONSOLE
            // 获得参数
            string[]? parameters =
#endif
                ParseKey(value);
            // 检查结果
            if (parameters == null || parameters.Length != 3)
            {
                // 记录日志
                LogTool.LogMessage("LTPAttributeCache", "LoadAttribute",
                    string.Format("无效参数\"{0}\"！", value)); return -1;
            }

            // 记录日志
            //LogTool.LogMessage("LTPAttributeCache", "LoadAttribute", "加载数据记录！");

            // 指令字符串
            string cmdString =
                "SELECT TOP 1 " +
                "[count] " +
                "FROM [dbo].[AttributeContent] " +
                "WHERE [content] = @SqlContent AND [pos] = @SqlPos AND [ne] = @SqlNe;";

            // 计数
            int count = -1;
            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("LTPAttributeCache", "LoadAttribute", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 设置参数
                sqlCommand.Parameters.AddWithValue("SqlContent", value);
                // 记录日志
                //LogTool.LogMessage("LTPAttributeCache", "LoadAttribute", "参数已设定！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                //LogTool.LogMessage("LTPAttributeCache", "LoadAttribute", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 读取count
                    count = reader.GetInt32(1);
                    // 检查数据
                    if (!attributes.ContainsKey(value)) attributes.Add(value, count);
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                //LogTool.LogMessage("LTPAttributeCache", "LoadAttribute", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("LTPAttributeCache", "LoadAttribute", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("LTPAttributeCache", "LoadAttribute", "数据库链接已关闭！");
            }

            // 记录日志
            //LogTool.LogMessage("LTPAttributeCache", "LoadAttribute", "数据记录已加载！");
            // 返回结果
            return count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void LoadAttributes()
        {
            // 清理数据
            attributes.Clear();
            // 记录日志
            LogTool.LogMessage("LTPAttributeCache", "LoadAttributes", "加载数据记录！");

            // 指令字符串
            string cmdString =
                "SELECT [content], [pos], [ne], [count] FROM [dbo].[AttributeContent];";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("LTPAttributeCache", "LoadAttributes", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("LTPAttributeCache", "LoadAttributes", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("LTPAttributeCache", "LoadAttributes", "T-SQL指令已执行！");

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
                        LogTool.LogMessage("LTPAttributeCache", "LoadAttributes",
                            string.Format("{0} items loaded !", total));
                    }

                    // 检查参数
                    if (reader.IsDBNull(0)) continue;
                    // 获得content
                    string content = reader.GetString(0);
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;

                    // 检查参数
                    if (reader.IsDBNull(1)) continue;
                    // 获得pos
                    string pos = reader.GetString(1);
                    // 检查结果
                    if (string.IsNullOrEmpty(pos)) continue;

                    // 检查参数
                    if (reader.IsDBNull(2)) continue;
                    // 获得ne
                    string ne = reader.GetString(2);
                    // 检查结果
                    if (string.IsNullOrEmpty(ne)) continue;

                    // 获得count
                    int count = reader.GetInt32(3);
                    // 获得参数
                    string value = FormatKey(content, pos, ne);
                    // 检查数据
                    if (attributes.ContainsKey(value))
                    {
                        // 设置新数值
                        attributes[value] = count;
                    }
                    // 增加对象
                    else attributes.Add(value, count);
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("LTPAttributeCache", "LoadAttributes",
                    string.Format("{0} items loaded !", total));
                // 记录日志
                LogTool.LogMessage("LTPAttributeCache", "LoadAttributes", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("LTPAttributeCache", "LoadAttributes", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("LTPAttributeCache", "LoadAttributes", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\tattributes.count = " + attributes.Count);
            // 记录日志
            LogTool.LogMessage("LTPAttributeCache", "LoadAttributes", "数据记录已加载！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SaveAttributes()
        {
            // 记录日志
            LogTool.LogMessage("LTPAttributeCache", "SaveAttributes", "保存数据记录！");
            LogTool.LogMessage(string.Format("\tattributes.count = {0}", attributes.Count));

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
                foreach (KeyValuePair<string, int> kvp in attributes)
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("LTPAttributeCache", "SaveAttributes",
                                string.Format("{0} items saved !", total));
                    }

                    // 启动进程
                    tasks.Add(factory.StartNew(() =>
                    {
                        {
#if _USE_CLR
                            // 获得参数
                            string[] inputs = 
#elif _USE_CONSOLE
                            // 获得参数
                            string[]? inputs =
#endif
                                ParseKey(kvp.Key);
                            // 检查结果
                            if (inputs != null &&
                                    inputs.Length == 3)
                            {
                                // 加入到数据库
                                AddContent(inputs, kvp.Value);
                                // 获得参数
                                string value =
                                    FormatKey(inputs[0],
                                        inputs[1], inputs[2]);
                                // 同步锁
                                lock(attributes)
                                {
                                    // 检查数据
                                    if (attributes.ContainsKey(value))
                                    {
                                        // 增加计数
                                        attributes[value]++;
                                    }
                                    // 增加数据
                                    else attributes.Add(value, kvp.Value);
                                }
                            }
                            else
                            {
                                // 记录日志
                                LogTool.LogMessage("LTPAttributeCache", "SaveAttributes",
                                    string.Format("无效参数\"{0}\"！", kvp.Key));
                            }
                        }
                    }));

                    // 检查任务数量
                    if (tasks.Count >= 10000)
                    { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 打印记录
                LogTool.LogMessage("LTPAttributeCache", "SaveAttributes",
                        string.Format("{0} items saved !", total));
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("LTPAttributeCache", "SaveAttributes", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("LTPAttributeCache", "SaveAttributes", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\tattributes.count = " + attributes.Count);
            // 记录日志
            LogTool.LogMessage("LTPAttributeCache", "SaveAttributes", "数据记录已保存！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void MakeStatistic()
        {
            // 记录日志
            LogTool.LogMessage("LTPAttributeCache", "MakeStatistic", "开始统计数据！");

            // 计数器
            int total = 0;
            // 任务数组
            List<Task> tasks = new List<Task>();
            // 生成任务控制器
            TaskFactory factory = new TaskFactory(
                new LimitedConcurrencyLevelTaskScheduler(DBTool.MAX_THREADS));

            // 指令字符串
            string cmdString =
                "SELECT [content], [pos], [ne] FROM [dbo].[LTPContent];";
            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("LTPAttributeCache", "MakeStatistic", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("LTPAttributeCache", "MakeStatistic", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("LTPAttributeCache", "MakeStatistic", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 增加计数
                    total++;
                    // 检查结果
                    if (total % 10000 == 0)
                    {
                        // 记录日志
                        LogTool.LogMessage("LTPAttributeCache", "MakeStatistic",
                            string.Format("{0} items processed !", total));
                    }

                    // 检查参数
                    if (reader.IsDBNull(0)) continue;
                    // 获得content
                    string content = reader.GetString(0);
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;

                    // 检查参数
                    if (reader.IsDBNull(1)) continue;
                    // 获得pos
                    string pos = reader.GetString(1);
                    // 检查结果
                    if (string.IsNullOrEmpty(pos)) continue;

                    // 检查参数
                    if (reader.IsDBNull(2)) continue;
                    // 获得ne
                    string ne = reader.GetString(2);
                    // 检查结果
                    if (string.IsNullOrEmpty(ne)) continue;

                    // 启动进程
                    tasks.Add(factory.StartNew
                    (() =>
                    {
                        // 获得参数
                        string value = FormatKey(content, pos, ne);
                        // 同步处理
                        lock(attributes)
                        {
                            // 检查结果
                            if (attributes.ContainsKey(value))
                            {
                                attributes[value]++;
                            }
                            // 增加内容
                            else attributes.Add(value, 1);
                        }
                    }));

                    // 检查任务数量
                    if (tasks.Count >= 10000) { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("LTPAttributeCache", "MakeStatistic",
                    string.Format("{0} items processed !", total));
                // 记录日志
                LogTool.LogMessage("LTPAttributeCache", "MakeStatistic", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("LTPAttributeCache", "MakeStatistic", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("LTPAttributeCache", "MakeStatistic", "任务全部结束！");

                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("LTPAttributeCache", "MakeStatistic", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\tattributes.Count = " + attributes.Count);
            // 记录日志
            LogTool.LogMessage("LTPAttributeCache", "MakeStatistic", "数据统计完毕！");
        }

#if _USE_CONSOLE
        public static new void Main(string[] args)
        {
            Console.WriteLine("准备统计属性频次，该统计过程将十分耗时！");
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
                if (!DBTool.TableExists("LTPContent"))
                {
                    // 记录日志
                    Console.WriteLine("数据表LTPContent不存在！");
                }
                else
                {
                    // 创建数据表
                    CreateTable();
                    // 加载所有对象
                    LoadAttributes();
                    // 开始统计
                    MakeStatistic();
                    // 保存所有对象
                    SaveAttributes();
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
