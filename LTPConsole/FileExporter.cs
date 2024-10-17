using NLP;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace LTP
{
    public class FileExporter
    {
        public static void ExportLTP(string fileName)
        {
            // 记录日志
            LogTool.LogMessage("FileExporter", "ExportLTP", "开始输出数据！");

            // 指令字符串
            string cmdString =
                "SELECT [sid], [iid], [content], [pos], [ne], [parent], [relate], [arg] FROM [dbo].[LTPContent];";

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
                LogTool.LogMessage("FileExporter", "ExportLTP", "文件流已开启！");

                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("FileExporter", "ExportLTP", "数据库链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("FileExporter", "ExportLTP", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("FileExporter", "ExportLTP", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("FileExporter", "ExportLTP",
                            string.Format("{0} items processed !", total));
                    }

                    // 检查参数
                    if (reader.IsDBNull(2)) continue;
                    // 获得content
                    string content = reader.GetString(2);
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;

                    // 获得sid
                    int sid = reader.GetInt32(0);
#if _USE_CLR
                    // 获得内容
                    string value =
#elif _USE_CONSOLE
                    // 获得内容
                    string? value =
#endif
                        ShortContent.GetContent(sid);
                    // 检查结果
                    if (string.IsNullOrEmpty(value)) continue;
                    // 写入value.
                    bw.Write(value);
                    // 写入iid.
                    bw.Write(reader.GetInt32(1));
                    // 写入content
                    bw.Write(content);
                    // 写入pos
                    if (reader.IsDBNull(3))
                        bw.Write(string.Empty);
                    else
                        bw.Write(reader.GetString(3));
                    // 写入ne
                    if (reader.IsDBNull(4))
                        bw.Write(string.Empty);
                    else
                        bw.Write(reader.GetString(4));
                    // 写入parent
                    bw.Write(reader.GetInt32(5));
                    // 写入relate
                    if (reader.IsDBNull(6))
                        bw.Write(string.Empty);
                    else
                        bw.Write(reader.GetString(6));
                    // 写入arg
                    if (reader.IsDBNull(7))
                        bw.Write(string.Empty);
                    else
                        bw.Write(reader.GetString(7));
                }
                // 打印记录
                LogTool.LogMessage("FileExporter", "ExportLTP",
                    string.Format("{0} items processed !", total));

                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("FileExporter", "ExportLTP", "数据阅读器已关闭！");

                // 刷新打印流
                bw.Flush();
                // 关闭打印流
                bw.Close();
                // 关闭文件流
                fs.Close();
                // 记录日志
                LogTool.LogMessage("FileExporter", "ExportLTP", "文件流已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("FileExporter", "ExportLTP", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("FileExporter", "ExportLTP", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\ttotal = " + total);
            // 记录日志
            LogTool.LogMessage("FileExporter", "ExportLTP", "数据输出完毕！");
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            // 获得文件名
            string fileName = args[1];
            // 检查参数
            if (string.IsNullOrEmpty(fileName))
            {
                Console.WriteLine("无效的参数！"); return;
            }
            // 打印文件名
            Console.WriteLine(string.Format("准备开启{0}！", fileName));

            if (DBTool.TableExists("LTPContent") &&
                DBTool.TableExists("ShortContent"))
            {
                ExportLTP(fileName);
            }
            else
            {
                Console.WriteLine("数据表LTPContent或者ShortContent不存在！");
            }
        }
#endif
    }
}
