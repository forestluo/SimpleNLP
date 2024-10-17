using NLP;
using System;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LTP
{
    public class LTPAttributeContent
    {
        public static void CreateTable()
        {
            // 记录日志
            LogTool.LogMessage("LTPAttributeContent", "CreateTable", "创建数据表！");

            // 指令字符串
            string cmdString =
                // 删除之前的索引
                "IF OBJECT_ID('LTPAttributeContentContentPosNeIndex') IS NOT NULL " +
                "DROP INDEX dbo.LTPAttributeContentContentPosNeIndex; " +
                "IF OBJECT_ID('LTPAttributeContentContentPosIndex') IS NOT NULL " +
                "DROP INDEX dbo.LTPAttributeContentContentPosIndex; " +
                "IF OBJECT_ID('LTPAttributeContentContentIndex') IS NOT NULL " +
                "DROP INDEX dbo.LTPAttributeContentContentIndex; " +
                // 删除之前的表
                "IF OBJECT_ID('LTPAttributeContent') IS NOT NULL " +
                "DROP TABLE dbo.LTPAttributeContent; " +
                // 创建数据表
                "CREATE TABLE dbo.LTPAttributeContent " +
                "( " +
                // 短语编号
                "[aid]                  INT                     IDENTITY(1, 1)              NOT NULL, " +
                // 内容
                "[content]              NVARCHAR(16)            NOT NULL, " +
                // 词性标注
                "[pos]                  NVARCHAR(8)             NOT NULL, " +
                // 实体标注
                "[ne]                   NVARCHAR(8)             NOT NULL, " +
                // 计数器
                "[count]                INT                     NOT NULL                    DEFAULT 1, " +
                // 操作标志
                "[operation]            INT                     NOT NULL                    DEFAULT 0, " +
                // 结果状态
                "[consequence]          INT                     NOT NULL                    DEFAULT 0 " +
                "); " +
                // 创建简单索引
                "CREATE INDEX LTPAttributeContentContentIndex ON dbo.LTPAttributeContent ([content]); " +
                "CREATE INDEX LTPAttributeContentContentPosIndex ON dbo.LTPAttributeContent ([content], [pos]); " +
                "CREATE INDEX LTPAttributeContentContentPosNeIndex ON dbo.LTPAttributeContent ([content], [pos], [ne]); ";

            // 执行指令
            DBTool.ExecuteNonQuery(cmdString);

            // 记录日志
            LogTool.LogMessage("LTPAttributeContent", "CreateTable", "数据表已创建！");
        }

        protected static void AddContent(string[] inputs, int count)
        {
#if DEBUG
            Debug.Assert(inputs != null && inputs.Length == 3);
#endif
            // 指令字符串
            string cmdString =
                "UPDATE [dbo].[LTPAttributeContent] " +
                    "SET [count] = @SqlCount " +
                    "WHERE [content] = @SqlContent AND [pos] = @SqlPos AND [ne] = @SqlNe; " +
                "IF @@ROWCOUNT <= 0 " +
                "   INSERT INTO [dbo].[LTPAttributeContent] " +
                "   ([content], [pos], [ne], [count]) " +
                "   VALUES(@SqlContent, @SqlPos, @SqlNe, @SqlCount); ";

            // 参数字典
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            // 加入参数
            parameters.Add("SqlContent", inputs[0]);
            parameters.Add("SqlPos", inputs[1]);
            parameters.Add("SqlNe", inputs[2]);
            parameters.Add("SqlCount", count.ToString());
            // 执行指令
            DBTool.ExecuteNonQuery(cmdString, parameters);
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备创建LTPAttributeContent表，原表及其数据将被删除！");
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
                CreateTable();
                // 关闭计时器
                watch.Stop();
                // 打印结果
                Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
            }
        }
#endif
    }
}
