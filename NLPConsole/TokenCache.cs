using System.Data;
using System.Text;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NLP
{
    public class TokenCache : TokenContent
    {
        // 创建数据字典
        private static Dictionary<char, int>
            tokens = new Dictionary<char, int>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ClickCount(char token, int count = 1)
        {
#if DEBUG
            Debug.Assert(count > 0);
#endif
            // 检查结果
            // 假设所有数据都加入到内存
            if (tokens.ContainsKey(token)) tokens[token] += count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ClearTokens()
        {
            // 记录日志
            LogTool.LogMessage("TokenCache", "ClearTokens", "开始执行！");
            // 恢复初始值
            tokens.Clear();
            // 记录日志
            LogTool.LogMessage("TokenCache", "ClearTokens", "执行完毕！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int GetCount(char token)
        {
            // 返回结果
            return tokens.ContainsKey(token) ?
                tokens[token] : LoadToken(token);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void AddToken(char token, int count)
        {
            // 增加数值
            AddContent(token, count);
            // 检查结果
            if(tokens.ContainsKey(token))
            {
                // 设置数值
                tokens[token] = count;
            }
            // 增加数值
            else tokens.Add(token, count);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int LoadToken(char token)
        {
            // 记录日志
            //LogTool.LogMessage("TokenCache", "LoadToken", "加载数据记录！");

            // 计数器
            int count = -1;
            // 指令字符串
            string cmdString =
                "SELECT TOP 1 [count] " +
                "FROM [dbo].[TokenContent] WHERE [content] = @SqlToken;";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("TokenCache", "LoadToken", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                //LogTool.LogMessage("TokenCache", "LoadToken", "T-SQL指令已创建！");

                // 设置参数
                sqlCommand.Parameters.AddWithValue("SqlToken", token);
                // 记录日志
                //LogTool.LogMessage("TokenCache", "LoadToken", "T-SQL参数已设定！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                //LogTool.LogMessage("TokenCache", "LoadToken", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 设置参数
                    count = reader.GetInt32(0);
                    // 检查数据
                    if (!tokens.ContainsKey(token))
                    {
                        // 增加记录
                        tokens.Add(token, count < 0 ? 0 : count);
                    }
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                //LogTool.LogMessage("TokenCache", "LoadToken", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("TokenCache", "LoadToken", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("TokenCache", "LoadToken", "数据库链接已关闭！");
            }

            // 记录日志
            //LogTool.LogMessage("TokenTool", "LoadToken", "数据记录已加载！");
            // 返回结果
            return count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void LoadTokens()
        {
            // 清理
            tokens.Clear();
            // 记录日志
            LogTool.LogMessage("TokenCache", "LoadTokens", "加载数据记录！");

            // 指令字符串
            string cmdString =
                "SELECT [content], [count] FROM [dbo].[TokenContent];";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("TokenCache", "LoadTokens", "数据链接已建立！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("TokenCache", "LoadTokens", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("TokenCache", "LoadTokens", "T-SQL指令已执行！");

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
                        LogTool.LogMessage("TokenCache", "LoadTokens", 
                            string.Format("{0} tokens loaded !", total));
                    }

                    // 获得内容
                    string value = reader.GetString(0);
                    // 检查结果
                    if (string.IsNullOrEmpty(value)) continue;

                    // 字符数值
                    char charValue = value[0];
                    // 获得count
                    int count = reader.GetInt32(1);
                    // 检查记录
                    if (tokens.ContainsKey(charValue))
                    {
                        // 更新计数
                        tokens[charValue] = count;
                    }
                    else
                    {
                        // 增加记录
                        tokens.Add(charValue, count < 0 ? 0 : count);
                    }
                }
                // 打印记录
                LogTool.LogMessage(string.Format("{0} tokens loaded !", total));

                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("TokenCache", "LoadTokens", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("TokenCache", "LoadTokens", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("TokenCache", "LoadTokens", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\ttokens.count = " + tokens.Count);
            // 记录日志
            LogTool.LogMessage("TokenCache", "LoadTokens", "数据记录已加载！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SaveTokens()
        {
            // 记录日志
            LogTool.LogMessage("TokenCache", "SaveTokens", "保存数据记录！");
            LogTool.LogMessage(string.Format("\ttokens.count = {0}", tokens.Count));

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
                foreach (KeyValuePair<char, int> kvp in tokens)
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("TokenCache", "SaveTokens", 
                            string.Format("{0} tokens saved !", total));
                    }
                    // 启动进程
                    tasks.Add(factory.StartNew(() =>
                        AddContent(kvp.Key, kvp.Value)));
                    // 检查任务数量
                    if (tasks.Count >= 10000)
                        { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 打印记录
                LogTool.LogMessage("TokenCache", "SaveTokens", 
                    string.Format("{0} tokens saved !", total));
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("TokenCache", "SaveTokens", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                    { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("TokenCache", "MakeStatistic", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\ttokens.count = " + tokens.Count);
            // 记录日志
            LogTool.LogMessage("TokenCache", "SaveTokens", "数据记录已保存！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void MakeStatistic()
        {
            // 记录日志
            LogTool.LogMessage("TokenCache", "MakeStatistic", "开始统计数据！");

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
                foreach(string content in contents)
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("TokenCache", "MakeStatistic", 
                            string.Format("{0} items processed !", total));
                    }

                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;

                    // 启动进程
                    tasks.Add(factory.StartNew(() =>
                    {
                        //遍历子符串
                        foreach (char value in content)
                        {
                            lock(tokens)
                            {
                                // 检查关键字
                                if (tokens.
                                    ContainsKey(value))
                                {
                                    // 增加计数
                                    tokens[value]++;
                                }
                                // 增加数据
                                else tokens.Add(value, 1);
                            }
                        }
                    }));

                    // 检查任务数量
                    if (tasks.Count >= 10000) { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 打印记录
                LogTool.LogMessage("TokenCache", "MakeStatistic",
                    string.Format("{0} items processed !", total));
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("TokenCache", "MakeStatistic", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                    { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("TokenCache", "MakeStatistic", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\ttokens.count = " + tokens.Count);
            // 记录日志
            LogTool.LogMessage("TokenCache", "MakeStatistic", "数据统计完毕！");
        }

#if _USE_CONSOLE
        public static new void Main(string[] args)
        {
            Console.WriteLine("准备统计TokenContent频次，原表及其数据将被更改！");
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
            // 开始统计Token
            MakeStatistic();
            // 更新Token计数
            SaveTokens();
            // 卸载数据
            ClearTokens();
            // 卸载数据
            FilteredCache.ClearContents();
            // 关闭计时器
            watch.Stop();
            // 打印结果
            Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
        }
#endif
    }
}
