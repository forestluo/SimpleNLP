using System.Text;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NLP
{
    public class CoreTool
    {
        public static void MergeContent()
        {
            // 记录日志
            LogTool.LogMessage("CoreTool", "MergeContent", "合并数据记录！");

            // 指令字符串
            string cmdString =
                "SELECT [content], [count] FROM [dbo].[ShortContent];";

            // 计数器
            int total = 0;
            // 任务数组
            List<Task> tasks = new List<Task>();
            // 生成任务控制器
            TaskFactory factory = new TaskFactory(
                new LimitedConcurrencyLevelTaskScheduler(DBTool.MAX_THREADS));

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("CoreTool", "MergeContent", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("CoreTool", "MergeContent", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("CoreTool", "MergeContent", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("CoreTool", "MergeContent", 
                            string.Format("{0} items processed !", total));
                    }

                    // 检查参数
                    if (reader.IsDBNull(0)) continue;
                    // 获得内容
                    string value = reader.GetString(0);
                    // 检查结果
                    // 检查长度从2起的数据
                    if (value == null || value.Length <= 1) continue;

                    // 获得计数
                    int count = reader.GetInt32(1);

                    // 启动进程
                    tasks.Add(factory.StartNew
                    (() =>
                    {
                        // 获得计数
                        int newCount = DictionaryCache.GetCount(value);
                        // 检查结果
                        // 结果为-1表明字典中不存在
                        if (newCount > 0)
                        {
                            // 检查计数结果
                            if (count != newCount)
                            {
                                // 打印记录
                                LogTool.LogMessage(
                                    string.Format("内容({0})计数({1})与字典计数({2})不相等 !", value, count, newCount));
                                // 选择较大者
                                count = count > newCount ? count : newCount;
                            }
                            // 增加记录
                            CoreCache.AddSegment(value, count);
                        }
                    }));

                    // 检查任务数量
                    if (tasks.Count >= 10000) { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 关闭数据阅读器
                reader.Close();
                // 打印记录
                LogTool.LogMessage("CoreTool", "MergeContent",
                    string.Format("{0} items processed !", total));
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("CoreTool", "MergeContent", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count >= 10000)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("CoreTool", "MergeContent", "任务全部结束！");

                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("CoreTool", "MergeContent", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\titems.count = " + total);
            // 记录日志
            LogTool.LogMessage("CoreTool", "MergeContent", "数据记录已合并！");
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备生成CoreContent，原表及其数据将被更改！");
            Console.WriteLine("确认是否执行（Yes/No）？");
            // 检查确认
            if (!MiscTool.Confirm()) return;
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
                Console.WriteLine("数据表ShortContent不存在！"); return;

            }
            // 检查数据表
            if (!DBTool.TableExists("DictionaryContent"))
            {
                // 记录日志
                Console.WriteLine("数据表DictionaryContent不存在！"); return;
            }
            // 创建数据表
            CoreContent.CreateTable();
            // 装栽Dictionay
            DictionaryCache.LoadEntries();
            // 合并内容
            MergeContent();
            // 保存Dictionay
            DictionaryCache.SaveEntries();
            // 关闭计时器
            watch.Stop();
            // 打印结果
            Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
        }
#endif
    }
}
