using System;
using System.IO;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;

namespace NLP
{
    public class CounterContent
    {
        public static void CreateTable()
        {
            // 记录日志
            LogTool.LogMessage("CounterContent", "CreateTable", "创建数据表！");

            // 指令字符串
            string cmdString =
                // 删除之前的表
                "IF OBJECT_ID('CounterContent') IS NOT NULL " +
                "DROP TABLE dbo.CounterContent; " +
                // 创建数据表
                "CREATE TABLE dbo.CounterContent " +
                "( " +
                // 编号
                "[cid]                  INT                     IDENTITY(1, 1)              NOT NULL, " +
                // 计数器
                "[count]                INT                     NOT NULL                    DEFAULT 0, " +
                // 内容
                "[content]              NVARCHAR(8)             PRIMARY KEY                 NOT NULL, " +
                // 操作标志
                "[operation]            INT                     NOT NULL                    DEFAULT 0, " +
                // 结果状态
                "[consequence]          INT                     NOT NULL                    DEFAULT 0 " +
                "); ";

            // 执行指令
            DBTool.ExecuteNonQuery(cmdString);

            // 记录日志
            LogTool.LogMessage("CounterContent", "CreateTable", "数据表已创建！");
        }

        protected static void AddContent(string content, int count)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(content));
#endif

            // 指令字符串
            string cmdString = string.Format(
                "UPDATE [dbo].[CounterContent] " +
                    "SET [count] = {0} " +
                    "WHERE [content] = @SqlContent; " +
                "IF @@ROWCOUNT <= 0 " +
                "   INSERT INTO [dbo].[CounterContent] ([count], [content]) " +
                "   VALUES ({0}, @SqlContent); ", count);

            // 参数字典
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            // 加入参数
            parameters.Add("SqlContent", content);
            // 执行指令
            DBTool.ExecuteNonQuery(cmdString, parameters);
        }

        public static void ImportFile(string fileName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(fileName));
#endif
            // 记录日志
            LogTool.LogMessage("CounterContent", "ImportFile", "开始输入数据！");

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
                LogTool.LogMessage("CounterContent", "ImportFile", "文件流已开启！");

                // 循环处理
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("CounterContent", "ImportFile",
                            string.Format("{0} items processed !", total));
                    }

                    // 读取cid
                    int cid = br.ReadInt32();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取content
                    string content = br.ReadString();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取count
                    int count = br.ReadInt32();

                    // 增加内容
                    CounterCache.AddCounter(content, count);
                }
                // 打印记录
                LogTool.LogMessage("CounterContent", "ImportFile",
                    string.Format("{0} items processed !", total));

                // 关闭文件流
                br.Close(); fs.Close();
                // 记录日志
                LogTool.LogMessage("CounterContent", "ImportFile", "文件流已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("CounterContent", "ImportFile", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }

            // 记录日志
            LogTool.LogMessage("\ttotal = " + total);
            // 记录日志
            LogTool.LogMessage("CounterContent", "ImportFile", "数据输入完毕！");
        }

        public static void ExportFile(string fileName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(fileName));
#endif
            // 记录日志
            LogTool.LogMessage("CounterContent", "ExportFile", "开始输出数据！");

            // 指令字符串
            string cmdString =
                "SELECT [cid], [content], [count] FROM [dbo].[CounterContent]";

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
                LogTool.LogMessage("CounterContent", "ExportFile", "文件流已开启！");

                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("CounterContent", "ExportFile", "数据库链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("CounterContent", "ExportFile", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("CounterContent", "ExportFile", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("CounterContent", "ExportFile",
                            string.Format("{0} items processed !", total));
                    }

                    // 检查参数
                    if (reader.IsDBNull(1)) continue;
                    // 获得content
                    string content = reader.GetString(1);
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;

                    // 写入cid
                    bw.Write(reader.GetInt32(0));
                    // 写入content
                    bw.Write(content);
                    // 写入count
                    bw.Write(reader.GetInt32(2));
                }
                // 打印记录
                LogTool.LogMessage("CounterContent", "ExportFile",
                    string.Format("{0} items processed !", total));

                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("CounterContent", "ExportFile", "数据阅读器已关闭！");

                // 刷新打印流
                bw.Flush();
                // 关闭打印流
                bw.Close();
                // 关闭文件流
                fs.Close();
                // 记录日志
                LogTool.LogMessage("CounterContent", "ExportFile", "文件流已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("CounterContent", "ExportFile", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("CounterContent", "ExportFile", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\ttotal = " + total);
            // 记录日志
            LogTool.LogMessage("CounterContent", "ExportFile", "数据输出完毕！");
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备创建CounterContent表，原表及其数据将被删除！");
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
