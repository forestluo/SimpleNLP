using System.Diagnostics;
using System.Collections.Generic;

namespace NLP
{
    public class ExceptionLog
    {
        public static void CreateTable()
        {
            // 记录日志
            LogTool.LogMessage("ExceptionLog", "CreateTable", "创建数据表！");

            // 指令字符串
            string cmdString =
                // 删除索引
                "IF OBJECT_ID('ExceptionLogEIDIndex') IS NOT NULL " +
                "DROP INDEX dbo.ExceptionLogEIDIndex; " +
                // 删除之前的表
                "IF OBJECT_ID('ExceptionLog') IS NOT NULL " +
                "DROP TABLE dbo.ExceptionLog; " +
                // 创建数据表
                "CREATE TABLE dbo.ExceptionLog " +
                "( " +
                // 编号
                "[eid]                  INT                     IDENTITY(1, 1)              NOT NULL, " +
                // 时间
                "[time]                 DATETIME                NOT NULL                    DEFAULT GETDATE(), " +
                // 错误号
                "[number]               INT                     NOT NULL                    DEFAULT 0, " +
                // 严重性
                "[serverity]            INT                     NOT NULL                    DEFAULT 0, " +
                // 错误状态号
                "[state]                INT                     NOT NULL                    DEFAULT 0, " +
                // 出现错误的存储过程或触发器的名称
                "[prodcedure]           NVARCHAR(256), " +
                // 导致错误的例程中的行号
                "[line]                 INT                     NOT NULL                    DEFAULT 0, " +
                // 错误消息的完整文本
                "[message]              NVARCHAR(MAX), " +
                // 操作标志
                "[operation]            INT                     NOT NULL                    DEFAULT 0, " +
                // 结果状态
                "[consequence]          INT                     NOT NULL                    DEFAULT 0 " +
                "); " +
                // 创建简单索引
                "CREATE INDEX ExceptionLogEIDIndex ON dbo.ExceptionLog (eid); ";

            // 执行指令
            DBTool.ExecuteNonQuery(cmdString);

            // 记录日志
            LogTool.LogMessage("ExceptionLog", "CreateTable", "数据表已创建！");
        }

        public static void CatchException(string tips)
        {
            // 记录日志
            LogTool.LogMessage("ExceptionLog", "CatchException", "捕捉异常！");

            // 指令字符串
            string cmdString =
                "INSERT INTO dbo.ExceptionLog " +
                "([number], [serverity], [state], [prodcedure], [line], [message]) " +
                "VALUES " +
                "(ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ISNULL(@SqlTips, '') + '> ' + ERROR_MESSAGE()); ";
            // 设置参数
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            // 增加参数
            parameters.Add("SqlTips", tips);
            // 执行指令
            DBTool.ExecuteNonQuery(cmdString, parameters);

            // 记录日志
            LogTool.LogMessage("ExceptionLog", "CatchException", "异常已记录！");
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备创建ExceptionLog表，原表及其数据将被删除！");
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
