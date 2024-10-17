using System;
using System.IO;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;

namespace NLP
{
    public class CoreContent
    {
        public static void CreateTable()
        {
            // 记录日志
            LogTool.LogMessage("CoreContent", "CreateTable", "创建数据表！");

            // 指令字符串
            string cmdString =
                // 删除之前的表
                "IF OBJECT_ID('CoreContent') IS NOT NULL " +
                "DROP TABLE dbo.CoreContent; " +
                // 创建数据表
                "CREATE TABLE dbo.CoreContent " +
                "( " +
                // 编号
                "[cid]                  INT                     IDENTITY(1, 1)              NOT NULL, " +
                // 标志位
                "[flag]                 INT                     NOT NULL                    DEFAULT 0, " +
                // 计数器
                "[count]                INT                     NOT NULL                    DEFAULT 0, " +
                // 相关系数
                "[gamma]                FLOAT                   NOT NULL                    DEFAULT -1, " +
                // 内容长度
                "[length]               INT                     NOT NULL                    DEFAULT 0, " +
                // 内容
                "[content]              NVARCHAR(64)            PRIMARY KEY                 NOT NULL, " +
                // 分割描述
                "[segment]              NVARCHAR(MAX)           NULL, " +
                // 操作标志
                "[operation]            INT                     NOT NULL                    DEFAULT 0, " +
                // 结果状态
                "[consequence]          INT                     NOT NULL                    DEFAULT 0 " +
                "); ";

            // 执行指令
            DBTool.ExecuteNonQuery(cmdString);

            // 记录日志
            LogTool.LogMessage("CoreContent", "CreateTable", "数据表已创建！");
        }

        public static int GetMaxLength()
        {
            return DBTool.GetMaxLength("CoreContent");
        }

        public static void ResetCounter()
        {
            // 记录日志
            LogTool.LogMessage("CoreContent", "ResetCounter", "开始执行！");
            // 恢复初始值
            // 强制指定
            DBTool.ExecuteNonQuery("UPDATE [dbo].[CoreContent] " +
                    "   SET [length] = LEN([content])," +
                    "       [count] = 0, [gamma] = -1 " +
                    "   WHERE ([flag] & 0x800) <> 0;");
            // 恢复初始值
            // 非强制指定
            DBTool.ExecuteNonQuery("UPDATE [dbo].[CoreContent] " +
                    "   SET [length] = LEN([content])," +
                    "       [count] = 0, [gamma] = -1, [segment] = NULL " +
                    "   WHERE ([flag] & 0x800) = 0;");
            // 记录日志
            LogTool.LogMessage("CoreContent", "ResetCounter", "执行完毕！");
        }

        protected static void SetPath(string content, string path)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(path));
            Debug.Assert(!string.IsNullOrEmpty(content));
#endif
            // 指令字符串
            string cmdString =
                "UPDATE [dbo].[CoreContent] " +
                "   SET [flag] = [flag] | 0x800, " + 
                "   [segment] = @SqlPath WHERE [content] = @SqlContent;";

            // 参数字典
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            // 加入参数
            parameters.Add("SqlPath", path);
            parameters.Add("SqlContent", content);
            // 执行指令
            DBTool.ExecuteNonQuery(cmdString, parameters);
        }

        protected static void DeleteContent(string content)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(content));
#endif

            // 指令字符串
            string cmdString =
                "DELETE FROM [dbo].[CoreContent] WHERE [content] = @SqlContent;";

            // 参数字典
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            // 加入参数
            parameters.Add("SqlContent", content);
            // 执行指令
            DBTool.ExecuteNonQuery(cmdString, parameters);
        }

        protected static void AddContent(string content, int count)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(content));
#endif

            // 指令字符串
            string cmdString = string.Format(
                "UPDATE [dbo].[CoreContent] " +
                    "SET [count] = {0}, [length] = LEN(@SqlContent) " +
                    "WHERE [content] = @SqlContent; " +
                "IF @@ROWCOUNT <= 0 " +
                "   INSERT INTO [dbo].[CoreContent] ([count], [content], [length]) " +
                "   VALUES ({0}, @SqlContent, LEN(@SqlContent)); ", count);

            // 参数字典
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            // 加入参数
            parameters.Add("SqlContent", content);
            // 执行指令
            DBTool.ExecuteNonQuery(cmdString, parameters);
        }

        protected static void AddContent(CoreSegment segment)
        {
#if DEBUG
            Debug.Assert(segment != null);
#endif

            // 生成批量处理语句
            string cmdString =
                "UPDATE [dbo].[CoreContent] " +
                    "SET [flag] = @SqlFlag, [count] = @SqlCount, [length] = LEN(@SqlContent), " +
                    "[gamma] = @SqlGamma, [segment] = @SqlSegment WHERE [content] = @SqlContent; " +
                "IF @@ROWCOUNT <= 0 " +
                    "INSERT INTO [dbo].[CoreContent] " +
                    "([content], [flag], [count], [length], [gamma], [segment]) " +
                    "VALUES (@SqlContent, @SqlFlag, @SqlCount, LEN(@SqlContent), @SqlGamma, @SqlSegment); ";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("CoreContent", "AddContent", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                //LogTool.LogMessage("CoreContent", "AddContent", "T-SQL指令已创建！");

                // 设置参数
                sqlCommand.Parameters.AddWithValue("SqlFlag", segment.Flag);
                sqlCommand.Parameters.AddWithValue("SqlCount", segment.Count);
                sqlCommand.Parameters.AddWithValue("SqlGamma", segment.Gamma);
                sqlCommand.Parameters.AddWithValue("SqlContent", segment.Value);
#if _USE_CLR
                // 获得路径
                string path = segment.GetPath();
                // 设置参数
                sqlCommand.Parameters.AddWithValue("SqlSegment",
                        string.IsNullOrEmpty(path) ? null : path);
#else
                // 获得路径
                string? path = segment.GetPath();
                // 设置参数
                sqlCommand.Parameters.AddWithValue("SqlSegment",
                        string.IsNullOrEmpty(path) ? DBNull.Value : path);
#endif
                // 记录日志
                //LogTool.LogMessage("CoreContent", "AddContent", "T-SQL参数已设定！");

                // 执行指令
                sqlCommand.ExecuteNonQuery();
                // 记录日志
                //LogTool.LogMessage("CoreContent", "AddContent", "T-SQL指令已执行！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("CoreContent", "AddContent", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("CoreContent", "AddContent", "数据库链接已关闭！");
            }
            // 记录日志
            //LogTool.LogMessage("CoreContent", "AddContent", "数据记录已保存！");
        }

        public static void ImportFile(string fileName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(fileName));
#endif
            // 记录日志
            LogTool.LogMessage("CoreContent", "ImportFile", "开始输入数据！");

            // 计数器
            int total = 0;
            // 创建字典
            HashSet<string> segments = new HashSet<string>();

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
                LogTool.LogMessage("CoreContent", "ImportFile", "文件流已开启！");

                // 循环处理
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("CoreContent", "ImportFile",
                            string.Format("{0} items processed !", total));
                    }

                    // 创建对象
                    CoreSegment segment = new CoreSegment();

                    // 读取cid
                    segment.Index = br.ReadInt32();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取flag
                    segment.Flag = br.ReadInt32();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取count
                    segment.Count = br.ReadInt32();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取gamma
                    segment.Gamma = br.ReadDouble();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取content
                    segment.Value = br.ReadString();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取segment
                    string path = br.ReadString();
                    // 检查结果
                    if (path != null) path = path.Trim();

                    // 检查结果
                    if (segment.Count <= 0)
                        segment.Count = 1;
                    // 检查结果
                    if (segment.Value != null)
                        segment.Value = segment.Value.Trim();
                    // 设置路径
                    if (!string.IsNullOrEmpty(path)) segment.SetPath(path);

                    // 调整内容
                    if (segment.Value != null && segment.Value.Length > 0)
                    {
                        // 启动进程
                        tasks.Add(factory.StartNew(() =>
                        {

                            bool newFlag = false;
                            // 同步锁定
                            lock (segments)
                            {
                                // 检查关键字
                                if (!segments.Contains(segment.Value))
                                {
                                    // 增加内容
                                    segments.Add(segment.Value); newFlag = true;
                                }
                            }
                            // 加入内容
                            if (newFlag) CoreCache.AddSegment(segment);
                        }));
                    }

                    // 检查任务数量
                    if (tasks.Count >= 10000) { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 打印记录
                LogTool.LogMessage("CoreContent", "ImportFile",
                    string.Format("{0} items processed !", total));

                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("CoreContent", "ImportFile", "等待任务结束！");

                // 关闭文件流
                br.Close(); fs.Close();
                // 记录日志
                LogTool.LogMessage("CoreContent", "ImportFile", "文件流已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("CoreContent", "ImportFile", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("CoreContent", "ImportFile", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\tsegments.count = " + segments.Count);
            // 记录日志
            LogTool.LogMessage("CoreContent", "ImportFile", "数据输入完毕！");
        }

        public static void ExportFile(string fileName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(fileName));
#endif
            // 记录日志
            LogTool.LogMessage("CoreContent", "ExportFile", "开始输出数据！");

            // 指令字符串
            string cmdString =
                "SELECT [cid], [flag], [count], [gamma], [content], [segment] FROM [dbo].[CoreContent];";

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
                LogTool.LogMessage("CoreContent", "ExportFile", "文件流已开启！");

                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("CoreContent", "ExportFile", "数据库链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("CoreContent", "ExportFile", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("CoreContent", "ExportFile", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("CoreContent", "ExportFile",
                            string.Format("{0} items processed !", total));
                    }

                    // 检查参数
                    if (reader.IsDBNull(4)) continue;
                    // 获得content
                    string content = reader.GetString(4);
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;

                    // 写入cid
                    bw.Write(reader.GetInt32(0));
                    // 写入sid
                    bw.Write(reader.GetInt32(1));
                    // 写入count
                    bw.Write(reader.GetInt32(2));
                    // 写入gamma
                    bw.Write(reader.GetDouble(3));
                    // 写入content
                    bw.Write(content != null ? content : string.Empty);
                    // 写入segment
                    if (reader.IsDBNull(5))
                        bw.Write(string.Empty);
                    else bw.Write(reader.GetString(5));
                }
                // 打印记录
                LogTool.LogMessage("CoreContent", "ExportFile",
                    string.Format("{0} items processed !", total));

                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("CoreContent", "ExportFile", "数据阅读器已关闭！");

                // 刷新打印流
                bw.Flush();
                // 关闭打印流
                bw.Close();
                // 关闭文件流
                fs.Close();
                // 记录日志
                LogTool.LogMessage("CoreContent", "ExportFile", "文件流已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("CoreContent", "ExportFile", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("CoreContent", "ExportFile", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\ttotal = " + total);
            // 记录日志
            LogTool.LogMessage("CoreContent", "ExportFile", "数据输出完毕！");
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备创建CoreContent表，原表及其数据将被删除！");
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
