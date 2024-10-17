using System.IO;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NLP
{
    public class TokenContent
    {
        public static void CreateTable()
        {
            // 记录日志
            LogTool.LogMessage("TokenContent", "CreateTable", "创建数据表！");

            // 指令字符串
            string cmdString =
                // 删除之前的表
                "IF OBJECT_ID('TokenContent') IS NOT NULL " +
                "DROP TABLE dbo.TokenContent; " +
                // 创建数据表
                "CREATE TABLE dbo.TokenContent " +
                "( " +
                // 编号
                "[tid]                  INT                     IDENTITY(1, 1)              NOT NULL, " +
                // 计数器
                "[count]                INT                     NOT NULL                    DEFAULT 0, " +
                // 内容
                "[content]              NVARCHAR(1)             PRIMARY KEY                 NOT NULL, " +
                // Unicode编码值
                "[unicode]              INT                     NOT NULL                    DEFAULT 0, " +
                // 备注
                "[remark]               NVARCHAR(32)            NULL, " +
                // 操作标志
                "[operation]            INT                     NOT NULL                    DEFAULT 0, " +
                // 结果状态
                "[consequence]          INT                     NOT NULL                    DEFAULT 0 " +
                "); ";

            // 执行指令
            DBTool.ExecuteNonQuery(cmdString);

            // 记录日志
            LogTool.LogMessage("TokenContent", "CreateTable", "数据表已创建！");
        }

        public static void ResetCounter()
        {
            // 记录日志
            LogTool.LogMessage("TokenContent", "ResetCounter", "开始执行！");
            // 恢复初始值
            DBTool.ExecuteNonQuery("UPDATE [dbo].[TokenContent] " +
                    "SET [count] = 0;");
            // 记录日志
            LogTool.LogMessage("TokenContent", "ResetCounter", "执行完毕！");
        }

        protected static void AddContent(char token, int count)
        {
            // 记录日志
            //LogTool.LogMessage("TokenContent", "AddContent", "开始执行！");

            // 指令字符串
            string cmdString =
                "UPDATE [dbo].[TokenContent] " +
                "   SET [count] = @SqlCount " +
                "   WHERE [content] = @SqlToken; " +
                "IF @@ROWCOUNT <= 0 " +
                "   INSERT INTO [dbo].[TokenContent] " +
                "       ([count], [content], [unicode], [remark]) " +
                "   VALUES (@SqlCount, @SqlToken, UNICODE(@SqlToken), @SqlRemark); ";

            // 设置参数
            Dictionary<string, string> parameters
                = new Dictionary<string, string>();
            // 增加参数
            parameters.Add("SqlCount", count.ToString());
            parameters.Add("SqlToken", token.ToString());
#if _USE_CLR
            // 获得结果
            string description = Token.GetDescription(token);
#elif _USE_CONSOLE
            // 获得结果
            string? description = Token.GetDescription(token);
#endif
            // 设置参数
            parameters.Add("SqlRemark",
                description != null ? description : string.Empty);
            // 执行指令
            DBTool.ExecuteNonQuery(cmdString, parameters);

            // 记录日志
            //LogTool.LogMessage("TokenContent", "AddContent", "执行完毕！");
        }

        public static void ImportFile(string fileName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(fileName));
#endif
            // 记录日志
            LogTool.LogMessage("TokenContent", "ImportFile", "开始输入数据！");

            // 计数器
            int total = 0;
            // 创建字典
            HashSet<char> tokens = new HashSet<char>();

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
                LogTool.LogMessage("TokenContent", "ImportFile", "文件流已开启！");

                // 循环处理
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("TokenContent", "ImportFile",
                            string.Format("{0} items processed !", total));
                    }

                    // 读取tid
                    int tid = br.ReadInt32();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取count
                    int count = br.ReadInt32();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取content
                    char content = br.ReadChar();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取unicode
                    int unicode = br.ReadInt32();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取remark
                    string remark = br.ReadString();
                    // 调整内容
                    if (remark != null) remark = remark.Trim();

                    // 启动进程
                    tasks.Add(factory.StartNew(() =>
                    {
                        bool newFlag = false;
                        // 同步锁定
                        lock (tokens)
                        {
                            // 检查关键字
                            if (!tokens.Contains(content))
                            {
                                // 增加内容
                                tokens.Add(content); newFlag = true;
                            }
                        }
                        // 加入内容
                        if (newFlag) TokenCache.AddToken(content, count);
                    }));
                    // 检查任务数量
                    if (tasks.Count >= 10000) { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 打印记录
                LogTool.LogMessage("TokenContent", "ImportFile",
                    string.Format("{0} items processed !", total));

                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("TokenContent", "ImportFile", "等待任务结束！");

                // 关闭文件流
                br.Close(); fs.Close();
                // 记录日志
                LogTool.LogMessage("TokenContent", "ImportFile", "文件流已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("TokenContent", "ImportFile", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("TokenContent", "ImportFile", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\ttokens.count = " + tokens.Count);
            // 记录日志
            LogTool.LogMessage("TokenContent", "ImportFile", "数据输入完毕！");
        }

        public static void ExportFile(string fileName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(fileName));
#endif
            // 记录日志
            LogTool.LogMessage("TokenContent", "ExportFile", "开始输出数据！");

            // 指令字符串
            string cmdString =
                "SELECT [tid], [count], [content], [unicode], [remark] FROM [dbo].[TokenContent];";

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
                LogTool.LogMessage("TokenContent", "ExportFile", "文件流已开启！");

                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("TokenContent", "ExportFile", "数据库链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("TokenContent", "ExportFile", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("TokenContent", "ExportFile", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("TokenContent", "ExportFile",
                            string.Format("{0} items processed !", total));
                    }

                    // 检查参数
                    if (reader.IsDBNull(2)) continue;

                    // 写入tid
                    bw.Write(reader.GetInt32(0));
                    // 写入count
                    bw.Write(reader.GetInt32(1));
                    // 获得content
                    char[] buffer = new char[1];
                    reader.GetChars(2, 0, buffer, 0, 1);
                    // 写入content
                    bw.Write(buffer[0]);
                    // 写入unicode
                    bw.Write(reader.GetInt32(3));
                    // 写入remark
                    if (reader.IsDBNull(4))
                        bw.Write(string.Empty);
                    else bw.Write(reader.GetString(4));
                }
                // 打印记录
                LogTool.LogMessage("TokenContent", "ExportFile",
                    string.Format("{0} items processed !", total));

                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("TokenContent", "ExportFile", "数据阅读器已关闭！");

                // 刷新打印流
                bw.Flush();
                // 关闭打印流
                bw.Close();
                // 关闭文件流
                fs.Close();
                // 记录日志
                LogTool.LogMessage("TokenContent", "ExportFile", "文件流已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("TokenContent", "ExportFile", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("TokenContent", "ExportFile", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\ttotal = " + total);
            // 记录日志
            LogTool.LogMessage("TokenContent", "ExportFile", "数据输出完毕！");
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备创建TokenContent表，原表及其数据将被删除！");
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
