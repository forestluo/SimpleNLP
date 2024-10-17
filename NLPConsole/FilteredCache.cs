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
    public class FilteredCache : FilteredContent
    {
        // 创建链表
        private static HashSet<string>
            contents = new HashSet<string>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static string[] GetArray(int length = -1)
        {
            // 返回结果
            return length < 0 ? contents.ToArray() :
                contents.Where(s => s.Length >= length).ToArray();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static new void AddContent(string content)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(content));
#endif
            // 加入数据
            FilteredContent.AddContent(content);
            // 检查缓冲
            if (!contents.Contains(content)) contents.Add(content);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
#if _USE_CLR
        public static string LoadContent(int fid)
#elif _USE_CONSOLE
        public static string? LoadContent(int fid)
#endif
        {
            // 记录日志
            //LogTool.LogMessage("FilteredCache", "LoadContent", "加载数据记录！");

            // 指令字符串
            string cmdString =
                "SELECT TOP 1 [content] " +
                "FROM [dbo].[FilteredContent] WHERE [fid] = @SqlFid;";

            // 内容
#if _USE_CLR
            string content = null;
#elif _USE_CONSOLE
            string? content = null;
#endif
            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("FilteredCache", "LoadContent", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 设置参数
                sqlCommand.Parameters.AddWithValue("SqlFid", fid);
                // 记录日志
                //LogTool.LogMessage("FilteredCache", "LoadContent", "参数已设定！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                //LogTool.LogMessage("FilteredCache", "LoadContent", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 检查数据
                    if (reader.IsDBNull(0)) continue;
                    // 获得内容
                    content = reader.GetString(0);
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;
                    // 检查数据
                    if (!contents.Contains(content)) contents.Add(content);
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                //LogTool.LogMessage("FilteredCache", "LoadContent", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("FilteredCache", "LoadContent", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("FilteredCache", "LoadContent", "数据库链接已关闭！");
            }

            // 记录日志
            //LogTool.LogMessage("FilteredCache", "LoadContent", "数据记录已加载！");
            // 返回结果
            return content;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ClearContents()
        {
            // 记录日志
            LogTool.LogMessage("FilteredCache", "ClearContents", "开始执行！");
            // 恢复初始值
            contents.Clear();
            // 记录日志
            LogTool.LogMessage("FilteredCache", "ClearContents", "执行完毕！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int LoadContents()
        {
            // 清理数据记录
            contents.Clear();
            // 记录日志
            LogTool.LogMessage("FilteredCache", "LoadContents", "加载数据记录！");

            // 指令字符串
            string cmdString = "SELECT [content] FROM [dbo].[FilteredContent];";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("FilteredCache", "LoadContents", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("FilteredCache", "LoadContents", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("FilteredCache", "LoadContents", "T-SQL指令已执行！");

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
                        LogTool.LogMessage("FilteredCache", "LoadContents",
                            string.Format("{0} items loaded !", total));
                    }

                    // 检查参数
                    if (reader.IsDBNull(0)) continue;
#if _USE_CLR
                    string
#elif _USE_CONSOLE
                    string?
#endif
                    // 获得内容
                    content = reader.GetString(0);
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;
                    // 检查数据
                    if (!contents.Contains(content)) contents.Add(content);
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("FilteredCache", "LoadContents",
                    string.Format("{0} items loaded !", total));
                // 记录日志
                LogTool.LogMessage("FilteredCache", "LoadContents", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("FilteredCache", "LoadContents", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("FilteredCache", "LoadContents", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\tcontents.count = " + contents.Count);
            // 记录日志
            LogTool.LogMessage("FilteredCache", "LoadContents", "数据记录已加载！");
            // 返回结果
            return contents.Count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void FilterContents()
        {
            // 记录日志
            LogTool.LogMessage("FilteredCache", "FilterContents", "开始过滤数据！");

            // 计数器
            int total = 0;
            // 任务数组
            List<Task> tasks = new List<Task>();
            // 生成任务控制器
            TaskFactory factory = new TaskFactory(
                new LimitedConcurrencyLevelTaskScheduler(DBTool.MAX_THREADS));

            // 指令字符串
            string cmdString = "SELECT [content] FROM [dbo].[RawContent];";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("FilteredCache", "FilterContents", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("FilteredCache", "FilterContents", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("FilteredCache", "FilterContents", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 增加计数
                    total++;
                    // 检查结果
                    if (total % 10000 == 0)
                    {
                        // 记录日志
                        LogTool.LogMessage("FilteredCache", "FilterContents",
                            string.Format("{0} items processed !", total));
                    }

                    // 检查数据
                    if (reader.IsDBNull(0)) continue;
#if _USE_CLR
                    // 获得内容
                    string
#elif _USE_CONSOLE
                    string?
#endif
                    // 获得内容
                    content = reader.GetString(0);
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;

                    // 启动进程
                    tasks.Add(factory.StartNew
                    (() =>
                    {
                        // 清理内容
                        content = MiscTool.ClearContent(content);
                        // 检查结果
                        if (string.IsNullOrEmpty(content)) return;

                        lock (contents)
                        {
                            // 加入数据
                            FilteredContent.AddContent(content);
                            // 检查缓冲
                            if (!contents.Contains(content)) contents.Add(content);
                        }
                    }));

                    // 检查任务数量
                    if (tasks.Count >= 10000) { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("CoreCache", "FilterContents",
                    string.Format("{0} items processed !", total));
                // 记录日志
                LogTool.LogMessage("FilteredCache", "FilterContents", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("FilteredCache", "FilterContents", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("FilteredCache", "FilterContents", "任务全部结束！");

                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("FilteredCache", "MakeStatistic", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\tcontents.Count = " + contents.Count);
            // 记录日志
            LogTool.LogMessage("FilteredCache", "FilterContents", "数据过滤完毕！");
        }

#if _USE_CONSOLE
        public static new void Main(string[] args)
        {
            Console.WriteLine("准备过滤原始数据，过程将会十分耗时！");
            Console.WriteLine("确认是否执行（Yes/No）？");
            // 检查确认
            if (!ConsoleTool.Confirm()) return;

            // 检查数据表
            if (!DBTool.TableExists("RawContent"))
            {
                // 记录日志
                Console.WriteLine("数据表RawContent不存在！"); return;
            }

            // 开启日志
            LogTool.SetLog(true);
            // 创建计时器
            Stopwatch watch = new Stopwatch();
            // 开启计时器
            watch.Start();
            // 创建数据表
            CreateTable();
            // 过滤数据
            FilterContents();
            // 卸载数据
            ClearContents();
            // 记录日志
            Console.WriteLine("数据过滤已完成！");
            // 关闭计时器
            watch.Stop();
            // 打印结果
            Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
        }
#endif
    }
}
