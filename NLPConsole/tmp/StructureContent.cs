using System;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;

namespace NLP
{
    public class StructureContent
    {
        public static void CreateTable()
        {
            // 记录日志
            LogTool.LogMessage("StructureContent", "CreateTable", "创建数据表！");

            // 指令字符串
            string cmdString =
                // 删除之前的索引
                "IF OBJECT_ID('StructureSIDIndex') IS NOT NULL " +
                "DROP INDEX dbo.StructureSIDIndex; " +
                // 删除之前的表
                "IF OBJECT_ID('StructureContent') IS NOT NULL " +
                "DROP TABLE dbo.StructureContent; " +
                // 创建数据表
                "CREATE TABLE dbo.StructureContent " +
                "( " +
                // 编号
                "[sid]                  INT                     IDENTITY(1, 1)              NOT NULL, " +
                // 计数器
                "[count]                INT                     NOT NULL                    DEFAULT 0, " +
                // 相关系数
                "[gamma]                FLOAT                   NOT NULL                    DEFAULT -1, " +
                // 内容长度
                "[length]               INT                     NOT NULL                    DEFAULT 0, " +
                // 结构
                "[structure]            NVARCHAR(64)            PRIMARY KEY                 NOT NULL, " +
                // 操作标志
                "[operation]            INT                     NOT NULL                    DEFAULT 0, " +
                // 结果状态
                "[consequence]          INT                     NOT NULL                    DEFAULT 0 " +
                "); " +
                // 创建简单索引
                "CREATE INDEX StructureContentSIDIndex ON dbo.StructureContent ([sid]); ";

            // 执行指令
            DBTool.ExecuteNonQuery(cmdString);

            // 记录日志
            LogTool.LogMessage("StructureContent", "CreateTable", "数据表已创建！");
        }

        public static void AddContent(FunctionSegment segment)
        {
#if DEBUG
            Debug.Assert(segment != null);
#endif
            // 生成批量处理语句
            string cmdString =
                "UPDATE [dbo].[StructureContent] " +
                    "SET [count] = @SqlCount " +
                    "WHERE [structure] = @SqlStructure; " +
                "IF @@ROWCOUNT <= 0 " +
                    "INSERT INTO [dbo].[StructureContent] " +
                    "   ([structure], [count], [length]) " +
                    "VALUES (@SqlStructure, @SqlCount, LEN(@SqlStructure)); ";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("StructureContent", "AddContent", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                //LogTool.LogMessage("StructureContent", "AddContent", "T-SQL指令已创建！");

                // 设置参数
                sqlCommand.Parameters.AddWithValue("SqlCount", segment.Count);
                sqlCommand.Parameters.AddWithValue("SqlStructure", segment.Value);
                // 记录日志
                //LogTool.LogMessage("StructureContent", "AddContent", "T-SQL参数已设定！");

                // 执行指令
                sqlCommand.ExecuteNonQuery();
                // 记录日志
                //LogTool.LogMessage("StructureContent", "AddContent", "T-SQL指令已执行！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("StructureContent", "AddContent", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("StructureContent", "AddContent", "数据库链接已关闭！");
            }
            // 记录日志
            //LogTool.LogMessage("StructureContent", "AddContent", "数据记录已保存！");
        }

#if _USE_CLR
        public static string GetContent(int sid)
#elif _USE_CONSOLE
        public static string? GetContent(int sid)
#endif
        {
            // 记录日志
            //LogTool.LogMessage("StructureContent", "GetContent", "查询原始数据！");

#if _USE_CLR
            // 内容
            string structure = null;
#elif _USE_CONSOLE
            // 内容
            string? structure = null;
#endif
            //指令字符串
            string cmdString =
                  "SELECT [content] FROM [dbo].[StructureContent] WHERE [sid] = @SqlSid;";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("StructureContent", "GetContent", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                //LogTool.LogMessage("StructureContent", "GetContent", "T-SQL指令已创建！");

                // 扩展参数
                sqlCommand.Parameters.AddWithValue("SqlSid", sid);
                // 记录日志
                //LogTool.LogMessage("StructureContent", "GetContent", "T-SQL参数已设定！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                //LogTool.LogMessage("StructureContent", "GetContent", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 获得内容
                    structure = reader.GetString(0);
                    // 检查内容
                    if (structure != null && structure.Length > 0) break;
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                //LogTool.LogMessage("StructureContent", "GetContent", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("StructureContent", "GetContent", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("StructureContent", "GetContent", "数据库链接已关闭！");
            }

            // 记录日志
            //LogTool.LogMessage("StructureContent", "GetContent", "数据查询完毕！");
            // 返回结果
            return structure;
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备创建StructureContent表，原表及其数据将被删除！");
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
                StructureContent.CreateTable();
                // 关闭计时器
                watch.Stop();
                // 打印结果
                Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
            }
        }
#endif
    }
}
