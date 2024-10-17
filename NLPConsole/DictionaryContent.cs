using System.IO;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NLP
{
    public class DictionaryContent
    {
        public static void CreateTable()
        {
            // 记录日志
            LogTool.LogMessage("DictionaryContent", "CreateTable", "创建数据表！");

            // 指令字符串
            string cmdString =
                // 删除索引
                "IF OBJECT_ID('DictionaryContentCIndex') IS NOT NULL " +
                "DROP INDEX dbo.DictionaryContentCIndex; " +
                // 删除索引
                "IF OBJECT_ID('DictionaryContent') IS NOT NULL " +
                "DROP TABLE dbo.DictionaryContent; " +
                // 创建字典表
                "CREATE TABLE dbo.DictionaryContent " +
                "( " +
                // 编号
                "[did]                  INT                     IDENTITY(1, 1)               NOT NULL, " +
                // 分类描述
                "[source]               NVARCHAR(64)            NULL, " +
                // 计数器
                "[count]                INT                     NOT NULL                     DEFAULT 0, " +
                // 内容长度
                "[length]               INT                     NOT NULL                     DEFAULT 0, " +
                // 内容描述
                "[content]              NVARCHAR(450)           NOT NULL, " +
                // 备注
                "[remark]               NVARCHAR(MAX)           NULL, " +
                // 操作标志
                "[operation]            INT                     NOT NULL                     DEFAULT 0, " +
                // 结果状态
                "[consequence]          INT                     NOT NULL                     DEFAULT 0 " +
                "); " +
                // 创建简单索引
                "CREATE INDEX DictionaryContentCIndex ON dbo.DictionaryContent([content]); ";

            // 执行指令
            DBTool.ExecuteNonQuery(cmdString);

            // 记录日志
            LogTool.LogMessage("DictionaryContent", "CreateTable", "数据表已创建！");
        }

        public static int GetMaxLength()
        {
            return DBTool.GetMaxLength("DictionaryContent");
        }

        public static void ResetCounter()
        {
            // 记录日志
            LogTool.LogMessage("DictionaryContent", "ResetCounter", "开始执行！");
            // 恢复初始值
            DBTool.ExecuteNonQuery("UPDATE [dbo].[DictionaryContent] " +
                    "SET [length] = LEN([content]), [count] = 0;", 0);
            // 记录日志
            LogTool.LogMessage("DictionaryContent", "ResetCounter", "执行完毕！");
        }

#if _USE_CLR
        protected static void AddContent(string value, int count, string[] inputs)
#elif _USE_CONSOLE
        protected static void AddContent(string value, int count, string[]? inputs)
#endif
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
            Debug.Assert(inputs == null || inputs.Length <= 2);
#endif
#if _USE_CLR
            // 指令参数
            string cmdString = null;
#elif _USE_CONSOLE
            // 指令参数
            string? cmdString = null;
#endif
            // 参数字典
            Dictionary<string, string> parameters =
                        new Dictionary<string, string>();
            // 检查输入参数
            if (inputs == null || inputs.Length == 0)
            {
                // 生成批量处理语句
                cmdString =
                    "UPDATE [dbo].[DictionaryContent] " +
                        "SET [count] = @SqlCount WHERE [content] = @SqlContent; " +
                    "IF @@ROWCOUNT <= 0 " +
                        "INSERT INTO [dbo].[DictionaryContent] " +
                        "([content], [count]) VALUES (@SqlContent, @SqlCount); ";
            }
            else if (inputs.Length == 1)
            {
                parameters.Add("SqlSource", inputs[0]);
                // 指令字符串
                cmdString =
                    "UPDATE [dbo].[DictionaryContent] " +
                        "SET [count] = @SqlCount, [source] = @SqlSource " +
                        "WHERE [content] = @SqlContent; " +
                    "IF @@ROWCOUNT <= 0 " +
                    "   INSERT INTO [dbo].[DictionaryContent] " +
                    "       ([count], [source], [content], [length]) " +
                    "   VALUES(@SqlCount, @SqlSource, @SqlContent, LEN(@SqlContent)); ";
            }
            else if (inputs.Length == 2)
            {
                parameters.Add("SqlSource", inputs[0]);
                parameters.Add("SqlRemark", inputs[1]);
                // 指令字符串
                cmdString =
                    "UPDATE [dbo].[DictionaryContent] " +
                        "SET [count] = @SqlCount, [source] = @SqlSource, " +
                        "[remark] = @SqlRemark WHERE [content] = @SqlContent; " +
                    "IF @@ROWCOUNT <= 0 " +
                    "   INSERT INTO [dbo].[DictionaryContent] " +
                    "       ([count], [source], [content], [remark], [length]) " +
                    "   VALUES(@SqlCount, @SqlSource, @SqlContent, @SqlRemark, LEN(@SqlContent)); ";
            }
            // 加入参数
            parameters.Add("SqlContent", value);
            parameters.Add("SqlCount", count.ToString());
#if DEBUG
            Debug.Assert(cmdString != null);
#endif
            // 执行指令
            DBTool.ExecuteNonQuery(cmdString, parameters);
        }

        public static void ImportFile(string fileName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(fileName));
#endif
            // 记录日志
            LogTool.LogMessage("DictionaryContent", "ImportFile", "开始输入数据！");

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
                LogTool.LogMessage("DictionaryContent", "ImportFile", "文件流已开启！");

                // 循环处理
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("DictionaryContent", "ImportFile",
                            string.Format("{0} items processed !", total));
                    }

                    // 读取did
                    int did = br.ReadInt32();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取count
                    int count = br.ReadInt32();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取source
                    string source = br.ReadString();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取content
                    string content = br.ReadString();
                    // 调整内容
                    if (content != null) content = content.Trim();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取remark
                    string remark = br.ReadString();
                    // 检查内容
                    if (string.IsNullOrEmpty(content))
                    {
                        // 记录日志
                        LogTool.LogMessage("DictionaryContent", "ImportFile", "无效内容！");
                        LogTool.LogMessage(string.Format("\tdid = {0}", did)); continue;
                    }

                    // 启动进程
                    tasks.Add(factory.StartNew(() =>
                        DictionaryCache.AddEntry(content, count,
                                    new string[] { source, remark })));
                    // 检查任务数量
                    if (tasks.Count >= 10000) { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 打印记录
                LogTool.LogMessage("DictionaryContent", "ImportFile",
                    string.Format("{0} items processed !", total));

                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("DictionaryContent", "ImportFile", "等待任务结束！");

                // 关闭文件流
                br.Close(); fs.Close();
                // 记录日志
                LogTool.LogMessage("DictionaryContent", "ImportFile", "文件流已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("DictionaryContent", "ImportFile", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("DictionaryContent", "ImportFile", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\ttotal = " + total);
            // 记录日志
            LogTool.LogMessage("DictionaryContent", "ImportFile", "数据输入完毕！");
        }

        public static void ExportFile(string fileName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(fileName));
#endif
            // 记录日志
            LogTool.LogMessage("DictionaryContent", "ExportFile", "开始输出数据！");

            // 指令字符串
            string cmdString =
                "SELECT [did], [count], [source], [content], [remark] FROM [dbo].[DictionaryContent];";

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
                LogTool.LogMessage("DictionaryContent", "ExportFile", "文件流已开启！");

                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("DictionaryContent", "ExportFile", "数据库链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("DictionaryContent", "ExportFile", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("DictionaryContent", "ExportFile", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("DictionaryContent", "ExportFile",
                            string.Format("{0} items processed !", total));
                    }

                    // 检查字段
                    if (reader.IsDBNull(3)) continue;
                    // 获得内容
                    string content = reader.GetString(3);
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;

                    // 清理内容
                    content = Blankspace.ClearInvisible(content);
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;

                    // Trim字符串
                    content = content.Trim();
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;

                    // 写入did
                    bw.Write(reader.GetInt32(0));
                    // 写入count
                    bw.Write(reader.GetInt32(1));
                    // 写入source
                    if (reader.IsDBNull(2))
                        bw.Write(string.Empty);
                    else bw.Write(reader.GetString(2));
                    // 写入content
                    bw.Write(content != null ? content : string.Empty);
                    // 写入remark
                    if (reader.IsDBNull(4))
                        bw.Write(string.Empty);
                    else bw.Write(reader.GetString(4));
                }
                // 打印记录
                LogTool.LogMessage("DictionaryContent", "ExportFile",
                    string.Format("{0} items processed !", total));

                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("DictionaryContent", "ExportFile", "数据阅读器已关闭！");

                // 刷新打印流
                bw.Flush();
                // 关闭文件流
                bw.Close();
                // 关闭文件流
                fs.Close();
                // 记录日志
                LogTool.LogMessage("DictionaryContent", "ExportFile", "文件流已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("DictionaryContent", "ExportFile", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("DictionaryContent", "ExportFile", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\ttotal = " + total);
            // 记录日志
            LogTool.LogMessage("DictionaryContent", "ExportFile", "数据输出完毕！");
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备创建DictionaryContent表，原表及其数据将被删除！");
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
