using System.IO;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace NLP
{
    public class OperationLog
    {
        public static void CreateTable()
        {
            // 记录日志
            LogTool.LogMessage("OperationLog", "CreateTable", "创建数据表！");

            // 指令字符串
            string cmdString =
                // 删除索引
                "IF OBJECT_ID('OperationLogLIDIndex') IS NOT NULL " +
                "DROP INDEX dbo.OperationLogLIDIndex; " +
                // 删除之前的表
                "IF OBJECT_ID('OperationLog') IS NOT NULL " +
                "DROP TABLE dbo.OperationLog; " +
                // 创建数据表
                "CREATE TABLE dbo.OperationLog " +
                "( " +
                // 编号
                "[lid]                  INT                     IDENTITY(1, 1)              NOT NULL, " +
                // 时间
                "[time]                 DATETIME                NOT NULL                    DEFAULT GETDATE(), " +
                // 操作来源
                "[source]               NVARCHAR(256)           NOT NULL," +
                // 操作信息
                "[operation]            NVARCHAR(MAX)           NOT NULL," +
                // 结果状态
                "[consequence]          INT                     NOT NULL                    DEFAULT 0 " +
                "); " +
                // 创建简单索引
                "CREATE INDEX OperationLogLIDIndex ON dbo.OperationLog (lid); ";

            // 执行指令
            DBTool.ExecuteNonQuery(cmdString);

            // 记录日志
            LogTool.LogMessage("OperationLog", "CreateTable", "数据表已创建！");
        }

        public static void Log(string source, string operation)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(source));
            Debug.Assert(!string.IsNullOrEmpty(operation));
#endif
            // 记录日志
            //LogTool.LogMessage("OperationLog", "Log", "记录操作！");

            // 指令字符串
            string cmdString =
                "INSERT INTO [dbo].[OperationLog] " +
                "   ([source], [operation]) VALUES (@SqlSource, @SqlOperation);";
            // 设置参数
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            // 增加参数
            parameters.Add("SqlSource", source);
            parameters.Add("SqlOperation", operation);
            // 执行指令
            DBTool.ExecuteNonQuery(cmdString, parameters);

            // 记录日志
            //LogTool.LogMessage("OperationLog", "Log", "记录操作！");
        }

        public static void ImportFile(string fileName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(fileName));
#endif
            // 记录日志
            LogTool.LogMessage("OperationLog", "ImportFile", "开始输入数据！");

            // 计数器
            int total = 0;

            try
            {
                // 保存至本地磁盘
                // 创建文件流
                FileStream fs = new FileStream(fileName, FileMode.Open);
                // 创建输出流
                BinaryReader br = new BinaryReader(fs);
                // 记录日志
                LogTool.LogMessage("OperationLog", "ImportFile", "文件流已开启！");

                // 循环处理
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("OperationLog", "ImportFile",
                            string.Format("{0} items processed !", total));
                    }

                    // 读取lid
                    int lid = br.ReadInt32();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取source
                    string source = br.ReadString();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取operation
                    string operation = br.ReadString();
                    // 调整内容
                    if (operation != null)
                        operation = operation.Trim();
                    // 检查内容
                    if (string.IsNullOrEmpty(operation))
                    {
                        // 记录日志
                        LogTool.LogMessage("OperationLog", "ImportFile", "无效内容！");
                        LogTool.LogMessage(string.Format("\tlid = {0}", lid)); continue;
                    }

                    // 记录操作
                    OperationLog.Log(source, operation);
                }
                // 打印记录
                LogTool.LogMessage("OperationLog", "ImportFile",
                    string.Format("{0} items processed !", total));

                // 关闭文件流
                br.Close(); fs.Close();
                // 记录日志
                LogTool.LogMessage("OperationLog", "ImportFile", "文件流已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("OperationLog", "ImportFile", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }

            // 记录日志
            LogTool.LogMessage("\ttotal = " + total);
            // 记录日志
            LogTool.LogMessage("OperationLog", "ImportFile", "数据输入完毕！");
        }

        public static void ExportFile(string fileName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(fileName));
#endif
            // 记录日志
            LogTool.LogMessage("OperationLog", "ExportFile", "开始输出数据！");

            // 指令字符串
            string cmdString =
                "SELECT [lid], [source], [operation] " +
                "   FROM [dbo].[OperationLog] ORDER BY [lid];";

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
                LogTool.LogMessage("OperationLog", "ExportFile", "文件流已开启！");

                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("OperationLog", "ExportFile", "数据库链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("OperationLog", "ExportFile", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("OperationLog", "ExportFile", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("OperationLog", "ExportFile",
                            string.Format("{0} items processed !", total));
                    }

                    // 检查参数
                    if (reader.IsDBNull(2)) continue;
                    // 获得operation
                    string operation = reader.GetString(2);
                    // 检查结果
                    if (string.IsNullOrEmpty(operation)) continue;

                    // 写入lid
                    bw.Write(reader.GetInt32(0));
                    // 写入source
                    if (reader.IsDBNull(1))
                        bw.Write(string.Empty);
                    else bw.Write(reader.GetString(1));
                    // 写入operation
                    bw.Write(operation != null ? operation : string.Empty);
                }
                // 打印记录
                LogTool.LogMessage("OperationLog", "ExportFile",
                    string.Format("{0} items processed !", total));

                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("OperationLog", "ExportFile", "数据阅读器已关闭！");

                // 刷新打印流
                bw.Flush();
                // 关闭打印流
                bw.Close();
                // 关闭文件流
                fs.Close();
                // 记录日志
                LogTool.LogMessage("OperationLog", "ExportFile", "文件流已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("OperationLog", "ExportFile", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("OperationLog", "ExportFile", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\ttotal = " + total);
            // 记录日志
            LogTool.LogMessage("OperationLog", "ExportFile", "数据输出完毕！");
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备创建OperationLog表，原表及其数据将被删除！");
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
