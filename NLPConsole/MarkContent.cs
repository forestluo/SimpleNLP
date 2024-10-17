using System;
using System.IO;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NLP
{
    public class MarkContent
    {
        public static void CreateTable()
        {
            // 记录日志
            LogTool.LogMessage("MarkContent", "CreateTable", "创建数据表！");

            // 指令字符串
            string cmdString =
                // 删除之前的索引
                "IF OBJECT_ID('MarkContentCCSIndex') IS NOT NULL " +
                "DROP INDEX dbo.MarkContentCCSIndex; " +
                "IF OBJECT_ID('MarkContentCCIndex') IS NOT NULL " +
                "DROP INDEX dbo.MarkContentCCIndex; " +
                "IF OBJECT_ID('MarkContentCIndex') IS NOT NULL " +
                "DROP INDEX dbo.MarkContentCIndex; " +
                "IF OBJECT_ID('MarkContentAidIndex') IS NOT NULL " +
                "DROP INDEX dbo.MarkContentAidIndex; " +
                // 删除之前的表
                "IF OBJECT_ID('MarkContent') IS NOT NULL " +
                "DROP TABLE dbo.MarkContent; " +
                // 创建数据表
                "CREATE TABLE dbo.MarkContent " +
                "( " +
                // 短语编号
                "[aid]                  INT                     IDENTITY(1, 1)              NOT NULL, " +
                // 内容
                "[content]              NVARCHAR(32)            NOT NULL, " +
                // 主要分类
                "[class]                NVARCHAR(8)             NOT NULL, " +
                // 次要分类
                "[subclass]             NVARCHAR(8)             NULL, " +
                // 操作标志
                "[operation]            INT                     NOT NULL                    DEFAULT 0, " +
                // 结果状态
                "[consequence]          INT                     NOT NULL                    DEFAULT 0 " +
                "); " +
                // 创建简单索引
                "CREATE INDEX MarkContentAidIndex ON dbo.MarkContent ([aid]); " +
                "CREATE INDEX MarkContentCIndex ON dbo.MarkContent ([content]); " +
                "CREATE INDEX MarkContentCCIndex ON dbo.MarkContent ([content], [class]); " +
                "CREATE INDEX MarkContentCCSIndex ON dbo.MarkContent ([content], [class], [subclass]); ";

            // 执行指令
            DBTool.ExecuteNonQuery(cmdString);

            // 记录日志
            LogTool.LogMessage("MarkContent", "CreateTable", "数据表已创建！");
        }

        public static void DeleteDuplicate()
        {
            {
                // 指令字符串
                string cmdString =
                    "DELETE [dbo].[MarkContent] " +
                    "   FROM [dbo].[MarkContent] A, " +
                    "   (SELECT [content], [class] " +
                    "       FROM [dbo].[MarkContent] " +
                    "       WHERE [subclass] IS NULL GROUP BY [content], [class] HAVING COUNT(*) > 1) AS B " +
                    "   WHERE A.[content] = B.[content] AND A.[class] = B.[class]" +
                    "   AND A.[aid] NOT IN " +
                    "   (SELECT MIN([aid]) AS [id] " +
                    "       FROM [dbo].[MarkContent] WHERE [subclass] IS NULL GROUP BY [content], [class] HAVING COUNT(*) > 1);";
                // 执行指令
                DBTool.ExecuteNonQuery(cmdString);
            }

            {
                // 指令字符串
                string cmdString =
                    "DELETE [dbo].[MarkContent] " +
                    "   FROM [dbo].[MarkContent] A, " +
                    "   (SELECT [content], [class], [subclass] " +
                    "       FROM [dbo].[MarkContent] " +
                    "       GROUP BY [content], [class], [subclass] HAVING COUNT(*) > 1) AS B " +
                    "   WHERE A.[content] = B.[content] AND A.[class] = B.[class] AND A.[subclass] = B.[subclass] " +
                    "   AND A.[aid] NOT IN " +
                    "   (SELECT MIN([aid]) AS [id] FROM [dbo].[MarkContent] GROUP BY [content], [class], [subclass] HAVING COUNT(*) > 1);";
                // 执行指令
                DBTool.ExecuteNonQuery(cmdString);
            }
        }

        protected static void DeleteContents(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 指令字符串
            string cmdString = "DELETE FROM [dbo].[MarkContent] " +
                        "WHERE [content] = @SqlContent ";
            // 参数字典
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            // 加入参数
            parameters.Add("SqlContent", value);
            // 执行指令
            DBTool.ExecuteNonQuery(cmdString, parameters);
        }

        protected static void DeleteContent(string value, MarkAttribute[] attributes)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
            Debug.Assert(attributes != null && attributes.Length > 0);
#endif
            // 循环处理
            foreach (MarkAttribute attribute in attributes)
            {
                // 指令字符串
                string cmdString = "DELETE FROM [dbo].[MarkContent] " +
                            "WHERE [content] = @SqlContent ";
                // 检查参数
                if (!string.IsNullOrEmpty(attribute.Value))
                    cmdString += " AND [class] = @SqlClass ";
                // 检查参数
                if (!string.IsNullOrEmpty(attribute.Remark))
                    cmdString += " AND [subclass] = @SqlSubclass ";
                // 参数字典
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                // 加入参数
                parameters.Add("SqlContent", value);
                // 检查参数
                if (!string.IsNullOrEmpty(attribute.Value))
                    parameters.Add("SqlClass", attribute.Value);
                // 检查参数
                if (!string.IsNullOrEmpty(attribute.Remark))
                    parameters.Add("SqlSubclass", attribute.Remark);
                // 执行指令
                DBTool.ExecuteNonQuery(cmdString, parameters);
            }
        }

        protected static void AddContent(string value, MarkAttribute[] attributes)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
            Debug.Assert(attributes != null && attributes.Length > 0);
#endif
            // 指令字符串
            string cmdString =
                "IF NOT EXISTS " +
                "   (SELECT TOP 1 * FROM [dbo].[MarkContent] " +
                "       WHERE [content] = @SqlContent AND " +
                "       [class] = @SqlClass AND [subclass] = @SqlSubclass) " +
                "   INSERT INTO [dbo].[MarkContent] " +
                "   ([content], [class], [subclass]) " +
                "   VALUES(@SqlContent, @SqlClass, @SqlSubclass); ";

            // 循环处理
            foreach(MarkAttribute attribute in attributes)
            {
                // 参数字典
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                // 加入参数
                parameters.Add("SqlContent", value);
#if _USE_CLR
                parameters.Add("SqlClass", attribute.Value);
                parameters.Add("SqlSubclass", attribute.Remark);
#elif _USE_CONSOLE
                parameters.Add("SqlClass",
                    string.IsNullOrEmpty(attribute.Value) ? string.Empty : attribute.Value);
                parameters.Add("SqlSubclass",
                    string.IsNullOrEmpty(attribute.Remark) ? string.Empty : attribute.Remark);
#endif
                // 执行指令
                DBTool.ExecuteNonQuery(cmdString, parameters);
            }
        }

        public static void ImportFile(string fileName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(fileName));
#endif
            // 记录日志
            LogTool.LogMessage("MarkContent", "ImportFile", "开始输入数据！");

            // 计数器
            int total = 0;
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
                LogTool.LogMessage("MarkContent", "ImportFile", "文件流已开启！");

                // 循环处理
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("MarkContent", "ImportFile",
                            string.Format("{0} items processed !", total));
                    }

                    // 读取aid
                    int aid = br.ReadInt32();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取content.
                    string content = br.ReadString();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取class
                    string mainClass = br.ReadString();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取subclass
                    string subClass = br.ReadString();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;
                    // 启动进程
                    tasks.Add(factory.StartNew(() =>
                    {
                        // 创建对象
                        MarkAttribute attribute =
                            new MarkAttribute();
                        // 设置参数
                        attribute.Index = aid;
                        attribute.Value = mainClass;
                        attribute.Remark = subClass;
                        // 加入属性
                        MarkCache.AddMark(content, attribute);
                    }));
                    // 检查任务数量
                    if (tasks.Count >= 10000) { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 打印记录
                LogTool.LogMessage("MarkContent", "ImportFile",
                    string.Format("{0} items processed !", total));

                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("MarkContent", "ImportFile", "等待任务结束！");

                // 关闭文件流
                br.Close(); fs.Close();
                // 记录日志
                LogTool.LogMessage("MarkContent", "ImportFile", "文件流已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("MarkContent", "ImportFile", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("MarkContent", "ImportFile", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\ttotal = " + total);
            // 记录日志
            LogTool.LogMessage("MarkContent", "ImportFile", "数据输入完毕！");
        }

        public static void ExportFile(string fileName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(fileName));
#endif
            // 记录日志
            LogTool.LogMessage("MarkContent", "ExportFile", "开始输出数据！");

            // 指令字符串
            string cmdString =
                "SELECT [aid], [content], [class], [subclass] FROM [dbo].[MarkContent];";

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
                LogTool.LogMessage("MarkContent", "ExportFile", "文件流已开启！");

                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("MarkContent", "ExportFile", "数据库链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("MarkContent", "ExportFile", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("MarkContent", "ExportFile", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("MarkContent", "ExportFile",
                            string.Format("{0} items processed !", total));
                    }

                    // 检查参数
                    if (reader.IsDBNull(1)) continue;
                    // 获得content
                    string content = reader.GetString(1);
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;

                    // 写入aid
                    bw.Write(reader.GetInt32(0));
                    // 写入content
                    bw.Write(content != null ? content : string.Empty);
                    // 写入class
                    if (reader.IsDBNull(2))
                        bw.Write(string.Empty);
                    else bw.Write(reader.GetString(2));
                    // 写入subclass
                    if (reader.IsDBNull(3))
                        bw.Write(string.Empty);
                    else bw.Write(reader.GetString(3));
                }
                // 打印记录
                LogTool.LogMessage("MarkContent", "ExportFile",
                    string.Format("{0} items processed !", total));

                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("MarkContent", "ExportFile", "数据阅读器已关闭！");

                // 刷新打印流
                bw.Flush();
                // 关闭打印流
                bw.Close();
                // 关闭文件流
                fs.Close();
                // 记录日志
                LogTool.LogMessage("MarkContent", "ExportFile", "文件流已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("MarkContent", "ExportFile", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("MarkContent", "ExportFile", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\ttotal = " + total);
            // 记录日志
            LogTool.LogMessage("MarkContent", "ExportFile", "数据输出完毕！");
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备创建MarkContent表，原表及其数据将被删除！");
            Console.WriteLine("确认是否执行（Yes/No）？");
            // 检查输入行
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
