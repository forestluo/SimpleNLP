using System.IO;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NLP
{
    public class ShortContent
    {
        public static void CreateTable()
        {
            // 记录日志
            LogTool.LogMessage("ShortContent", "CreateTable", "创建数据表！");

            // 指令字符串
            string cmdString =
                // 删除之前的索引
                "IF OBJECT_ID('ShortContentSIDIndex') IS NOT NULL " +
                "DROP INDEX dbo.ShortContentSIDIndex; " +
                // 删除之前的表
                "IF OBJECT_ID('ShortContent') IS NOT NULL " +
                "DROP TABLE dbo.ShortContent; " +
                // 创建数据表
                "CREATE TABLE dbo.ShortContent " +
                "( " +
                // 编号
                "[sid]                  INT                     IDENTITY(1, 1)              NOT NULL, " +
                // 计数器
                "[count]                INT                     NOT NULL                    DEFAULT 0, " +
                // 内容长度
                "[length]               INT                     NOT NULL                    DEFAULT 0, " +
                // 内容
                "[content]              NVARCHAR(450)           PRIMARY KEY                 NOT NULL, " +
                // 操作标志
                "[operation]            INT                     NOT NULL                    DEFAULT 0, " +
                // 结果状态
                "[consequence]          INT                     NOT NULL                    DEFAULT 0 " +
                "); " +
                // 创建简单索引
                "CREATE INDEX ShortContentSIDIndex ON dbo.ShortContent ([sid]); ";

            // 执行指令
            DBTool.ExecuteNonQuery(cmdString);

            // 记录日志
            LogTool.LogMessage("ShortContent", "CreateTable", "数据表已创建！");
        }

        public static int GetMaxID()
        {
            return DBTool.GetMaxID("ShortContent", "sid");
        }

        public static void ResetCounter()
        {
            // 记录日志
            LogTool.LogMessage("ShortContent", "ResetCounter", "开始执行！");
            // 恢复初始值
            DBTool.ExecuteNonQuery("UPDATE [dbo].[ShortContent] " +
                    "SET [length] = LEN([content]), [count] = 0;", 0);
            // 记录日志
            LogTool.LogMessage("ShortContent", "ResetCounter", "执行完毕！");
        }

        protected static void AddContent(string content, int count)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(content));
#endif
            // 指令字符串
            string cmdString =
                "UPDATE [dbo].[ShortContent] " +
                "   SET [count] = @SqlCount " + 
                "   WHERE [content] = @SqlContent; " +
                "IF @@ROWCOUNT <= 0 " +
                "   INSERT INTO [dbo].[ShortContent] " +
                "       ([count], [content], [length]) " +
                "   VALUES(@SqlCount, @SqlContent, LEN(@SqlContent)); ";

            // 参数字典
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            // 加入参数
            parameters.Add("SqlContent", content);
            parameters.Add("SqlCount", count.ToString());
            // 执行指令
            DBTool.ExecuteNonQuery(cmdString, parameters);
        }

#if _USE_CLR
        public static string GetContent(int sid)
#elif _USE_CONSOLE
        public static string? GetContent(int sid)
#endif
        {
            // 记录日志
            //LogTool.LogMessage("ShortContent", "GetContent", "加载数据记录！");

            // 指令字符串
            string cmdString =
                "SELECT TOP 1 [content] " +
                "FROM [dbo].[ShortContent] WHERE [sid] = @SqlSid;";

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
                //LogTool.LogMessage("ShortContent", "GetContent", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 设置参数
                sqlCommand.Parameters.AddWithValue("SqlSid", sid);
                // 记录日志
                //LogTool.LogMessage("ShortContent", "GetContent", "参数已设定！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                //LogTool.LogMessage("ShortContent", "GetContent", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 检查数据
                    if (reader.IsDBNull(0)) continue;
                    // 获得内容
                    content = reader.GetString(0);
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                //LogTool.LogMessage("ShortContent", "GetContent", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("ShortContent", "GetContent", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("ShortContent", "GetContent", "数据库链接已关闭！");
            }

            // 记录日志
            //LogTool.LogMessage("ShortContent", "GetContent", "数据记录已加载！");
            // 返回结果
            return content;
        }

        public static void ImportFile(string fileName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(fileName));
#endif
            // 记录日志
            LogTool.LogMessage("ShortContent", "ImportFile", "开始输入数据！");

            // 计数器
            int total = 0;
            // 创建字典
            HashSet<string> shorts = new HashSet<string>();

            // 任务数组
            List<Task> tasks = new List<Task>();
            // 生成任务控制器
            TaskFactory factory = new TaskFactory(
                new LimitedConcurrencyLevelTaskScheduler(DBTool.MAX_THREADS));

            try
            {
                // 保存至本地磁盘
                // 创建文件流
                FileStream fs = new FileStream(fileName, FileMode.Open);
                // 创建输出流
                BinaryReader br = new BinaryReader(fs);
                // 记录日志
                LogTool.LogMessage("ShortContent", "ImportFile", "文件流已开启！");

                // 循环处理
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("ShortContent", "ImportFile",
                            string.Format("{0} items processed !", total));
                    }

                    // 读取sid
                    int sid = br.ReadInt32();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取count
                    int count = br.ReadInt32();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 检查内容
                    if (count <= 0) count = 1;
                    // 读取content
                    string content = br.ReadString();
                    // 检查结果
                    if (content != null)
                        content = content.Trim();
                    // 调整内容
                    if (content != null && content.Length > 0)
                    {
                        // 启动进程
                        tasks.Add(factory.StartNew(() =>
                        {
                            bool newFlag = false;
                            // 同步锁定
                            lock (shorts)
                            {
                                // 检查关键字
                                if (!shorts.Contains(content))
                                {
                                    // 增加内容
                                    shorts.Add(content); newFlag = true;
                                }
                            }
                            // 加入内容
                            if (newFlag) ShortCache.AddShort(content, count);
                        }));
                    }

                    // 检查任务数量
                    if (tasks.Count >= 10000) { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 打印记录
                LogTool.LogMessage("ShortContent", "ImportFile",
                    string.Format("{0} items processed !", total));

                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("ShortContent", "ImportFile", "等待任务结束！");

                // 关闭文件流
                br.Close(); fs.Close();
                // 记录日志
                LogTool.LogMessage("ShortContent", "ImportFile", "文件流已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("ShortContent", "ImportFile", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("ShortContent", "ImportFile", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\tshorts.count = " + shorts.Count);
            // 记录日志
            LogTool.LogMessage("ShortContent", "ImportFile", "数据输入完毕！");
        }

        public static void ExportFile(string fileName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(fileName));
#endif
            // 记录日志
            LogTool.LogMessage("ShortContent", "ExportFile", "开始输出数据！");

            // 指令字符串
            string cmdString =
                "SELECT [sid], [count], [content] FROM [dbo].[ShortContent];";

            // 计数器
            int total = 0;
            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 保存至本地磁盘
                // 创建文件流
                FileStream fs = new FileStream(fileName, FileMode.Create);
                // 创建输出流
                BinaryWriter bw = new BinaryWriter(fs);
                // 记录日志
                LogTool.LogMessage("ShortContent", "ExportFile", "文件流已开启！");

                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("ShortContent", "ExportFile", "数据库链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("ShortContent", "ExportFile", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("ShortContent", "ExportFile", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("ShortContent", "ExportFile",
                            string.Format("{0} items processed !", total));
                    }

                    // 检查参数
                    if (reader.IsDBNull(2)) continue;
                    // 获得content
                    string content = reader.GetString(2);
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;

                    // 写入sid
                    bw.Write(reader.GetInt32(0));
                    // 写入count
                    bw.Write(reader.GetInt32(1));
                    // 写入content
                    bw.Write(content != null ? content : string.Empty);
                }
                // 打印记录
                LogTool.LogMessage("ShortContent", "ExportFile",
                    string.Format("{0} items processed !", total));

                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("ShortContent", "ExportFile", "数据阅读器已关闭！");

                // 刷新打印流
                bw.Flush();
                // 关闭打印流
                bw.Close();
                // 关闭文件流
                fs.Close();
                // 记录日志
                LogTool.LogMessage("ShortContent", "ExportFile", "文件流已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("ShortContent", "ExportFile", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("ShortContent", "ExportFile", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\ttotal = " + total);
            // 记录日志
            LogTool.LogMessage("ShortContent", "ExportFile", "数据输出完毕！");
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备创建ShortContent表，原表及其数据将被删除！");
            Console.WriteLine("确认是否执行（Yes/No）？");
            // 检查确认
            if (!ConsoleTool.Confirm()) return;

            // 开启日志
            LogTool.SetLog(true);
            // 创建计时器
            Stopwatch watch = new Stopwatch();
            // 开启计时器
            watch.Start();
            // 创建数据表
            CreateTable();
            // 关闭计时器
            watch.Stop();
            // 打印结果
            Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
        }
#endif
    }
}
