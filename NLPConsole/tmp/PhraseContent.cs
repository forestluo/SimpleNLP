using System;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;

namespace NLP
{
    public class PhraseContent
    {
        public static void CreateTable()
        {
            // 记录日志
            LogTool.LogMessage("PhraseContent", "CreateTable", "创建数据表！");

            // 指令字符串
            string cmdString =
                // 删除之前的表
                "IF OBJECT_ID('PhraseContent') IS NOT NULL " +
                "DROP TABLE dbo.PhraseContent; " +
                // 创建数据表
                "CREATE TABLE dbo.PhraseContent " +
                "( " +
                // 编号
                "[pid]                  INT                     IDENTITY(1, 1)              NOT NULL, " +
                // 结构编号
                "[sid]                  INT                     NOT NULL                    DEFAULT 0, " +
                // 计数器
                "[count]                INT                     NOT NULL                    DEFAULT 0, " +
                // 相关系数
                "[gamma]                FLOAT                   NOT NULL                    DEFAULT -1, " +
                // 内容长度
                "[length]               INT                     NOT NULL                    DEFAULT 0, " +
                // 内容
                "[content]              NVARCHAR(450)           PRIMARY KEY                 NOT NULL, " +
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
            LogTool.LogMessage("PhraseContent", "CreateTable", "数据表已创建！");
        }

        public static int GetMaxLength()
        {
            return DBTool.GetMaxLength("PhraseContent");
        }

        public static void AddContent(CoreSegment segment)
        {
#if DEBUG
            Debug.Assert(segment != null);
#endif
            // 生成批量处理语句
            string cmdString =
                "UPDATE [dbo].[PhraseContent] " +
                    "SET [sid] = @SqlSid, [count] = @SqlCount, " +
                    "[gamma] = @SqlGamma, [segment] = @SqlSegment WHERE [content] = @SqlContent; " +
                "IF @@ROWCOUNT <= 0 " +
                    "INSERT INTO [dbo].[PhraseContent] " +
                    "([content], [sid], [count], [length], [gamma], [segment]) " +
                    "VALUES (@SqlContent, @SqlSid, @SqlCount, LEN(@SqlContent), @SqlGamma, @SqlSegment); ";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("PhraseContent", "AddContent", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                //LogTool.LogMessage("PhraseContent", "AddContent", "T-SQL指令已创建！");

                // 设置参数
                sqlCommand.Parameters.AddWithValue("SqlSid", segment.Sid);
                sqlCommand.Parameters.AddWithValue("SqlCount", segment.Count);
                sqlCommand.Parameters.AddWithValue("SqlGamma", segment.Gamma);
                sqlCommand.Parameters.AddWithValue("SqlContent", segment.Value);
#if _USE_CLR
                // 获得路径
                string path = segment.GetPath();
#elif _USE_CONSOLE
                // 获得路径
                string? path = segment.GetPath();
#endif
                // 设置参数
                sqlCommand.Parameters.AddWithValue("SqlSegment",
                        (path != null && path.Length > 0) ? path : DBNull.Value);
                // 记录日志
                //LogTool.LogMessage("PhraseContent", "AddContent", "T-SQL参数已设定！");

                // 执行指令
                sqlCommand.ExecuteNonQuery();
                // 记录日志
                //LogTool.LogMessage("PhraseContent", "AddContent", "T-SQL指令已执行！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("PhraseContent", "AddContent", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("PhraseContent", "AddContent", "数据库链接已关闭！");
            }
            // 记录日志
            //LogTool.LogMessage("PhraseContent", "AddContent", "数据记录已保存！");
        }

#if _USE_CLR
        public static string GetContent(int pid)
#elif _USE_CONSOLE
        public static string? GetContent(int pid)
#endif
        {
            // 记录日志
            //LogTool.LogMessage("PhraseContent", "GetContent", "加载数据记录！");

#if _USE_CLR
            // 计数器
            string content = null;
#elif _USE_CONSOLE
            // 计数器
            string? content = null;
#endif      
            // 指令字符串
            string cmdString =
                "SELECT TOP 1 [content] " +
                "FROM [dbo].[PhraseContent] WHERE [pid] = @SqlPid;";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 设置参数
                sqlCommand.Parameters.AddWithValue("SqlPid", pid);
                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 循环处理
                while (reader.Read())
                {
                    // 设置参数
                    if (!reader.IsDBNull(0)) content = reader.GetString(0);
                }
                // 关闭数据阅读器
                reader.Close();
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("PhraseContent", "GetContent", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
            }

            // 记录日志
            //LogTool.LogMessage("PhraseContent", "GetContent", "数据记录已加载！");
            // 返回结果
            return content;
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备创建PhraseContent表，原表及其数据将被删除！");
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
                        Console.WriteLine("执行当前操作！"); break;
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
                // 创建数据表
                PhraseContent.CreateTable();
                // 关闭计时器
                watch.Stop();
                // 打印结果
                Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
            }
        }
#endif
    }
}
