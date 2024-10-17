using NLP;
using System;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;

namespace LTP
{
    public class LTPContent
    {
        public static void CreateTable()
        {
            // 记录日志
            LogTool.LogMessage("LTPContent", "CreateTable", "创建数据表！");

            // 指令字符串
            string cmdString =
                // 删除之前的索引
                "IF OBJECT_ID('LTPContentSIDIndex') IS NOT NULL " +
                "DROP INDEX dbo.LTPContentSIDIndex; " +
                "IF OBJECT_ID('LTPContentSIIDIndex') IS NOT NULL " +
                "DROP INDEX dbo.LTPContentSIIDIndex; " +
                "IF OBJECT_ID('LTPContentContentIndex') IS NOT NULL " +
                "DROP INDEX dbo.LTPContentContentIndex; " +
                // 删除之前的表
                "IF OBJECT_ID('LTPContent') IS NOT NULL " +
                "DROP TABLE dbo.LTPContent; " +
                // 创建数据表
                "CREATE TABLE dbo.LTPContent " +
                "( " +
                // 短语编号
                "[sid]                  INT                     NOT NULL                    DEFAULT 0, " +
                // 内部编号
                "[iid]                  INT                     NOT NULL                    DEFAULT 0, " +
                // 内容
                "[content]              NVARCHAR(16)            NOT NULL, " +
                // 词性标注
                "[pos]                  NVARCHAR(8)             NULL, " +
                // 实体标注
                "[ne]                   NVARCHAR(8)             NULL, " +
                // 父对象
                "[parent]               INT                     NOT NULL                    DEFAULT -1, " +
                // 相互关系
                "[relate]               NVARCHAR(8)             NULL, " +
                // 额外参数
                "[arg]                  NVARCHAR(128)           NULL, " +
                // 操作标志
                "[operation]            INT                     NOT NULL                    DEFAULT 0, " +
                // 结果状态
                "[consequence]          INT                     NOT NULL                    DEFAULT 0 " +
                "); " +
                // 创建简单索引
                "CREATE INDEX LTPContentSIDIndex ON dbo.LTPContent ([sid]); " +
                "CREATE INDEX LTPContentSIIDIndex ON dbo.LTPContent ([sid], [iid]); " +
                "CREATE INDEX LTPContentContentIndex ON dbo.LTPContent ([content]); ";

            // 执行指令
            DBTool.ExecuteNonQuery(cmdString);

            // 记录日志
            LogTool.LogMessage("LTPContent", "CreateTable", "数据表已创建！");
        }

        public static void AddContent(int sid, string[] attributes)
        {
            // 指令字符串
            string cmdString =
                "UPDATE [dbo].[LTPContent] " +
                    "SET [content] = @SqlContent, [pos] = @SqlPos, " +
                    "[ne] = @SqlNe, [parent] = @SqlParent, [relate] = @SqlRelate," +
                    "   [arg] = @SqlArg WHERE [sid] = @SqlSid AND [iid] = @SqlIid; " +
                "IF @@ROWCOUNT <= 0 " +
                "   INSERT INTO [dbo].[LTPContent] " +
                "   ([sid], [iid], [content], [pos], [ne], [parent], [relate], [arg]) " +
                "   VALUES(@SqlSid, @SqlIid, @SqlContent, @SqlPos, @SqlNe, @SqlParent, @SqlRelate, @SqlArg); ";

            // 参数字典
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            // 加入参数
            parameters.Add("SqlSid", sid.ToString());
            parameters.Add("SqlIid", attributes[0]);
            parameters.Add("SqlContent", attributes[1]);
            parameters.Add("SqlPos", attributes[2]);
            parameters.Add("SqlNe", attributes[3]);
            parameters.Add("SqlParent", attributes[4]);
            parameters.Add("SqlRelate", attributes[5]);
            parameters.Add("SqlArg", attributes[6]);
            // 执行指令
            DBTool.ExecuteNonQuery(cmdString, parameters);
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备创建LTPContent表，原表及其数据将被删除！");
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
                LTPContent.CreateTable();
                // 关闭计时器
                watch.Stop();
                // 打印结果
                Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
            }
        }
#endif
    }
}
