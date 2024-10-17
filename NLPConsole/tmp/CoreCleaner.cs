using System;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;

namespace NLP
{
    public class CoreCleaner
    {
        private static void Mark(string value)
        {
            // 指令字符串
            string cmdString =
                "UPDATE [dbo].[CoreContent] " +
                "   SET [operation] = 1 " +
                "   WHERE [content] = @SqlContent;";
            // 参数字典
            Dictionary<string, string> parameters =
                        new Dictionary<string, string>();
            // 加入参数
            parameters.Add("SqlContent", value);
            // 执行指令
            DBTool.ExecuteNonQuery(cmdString, parameters);
        }

        private static string[] GetSamples(string value)
        {
            // 记录日志
            //LogTool.LogMessage("CoreCleaner", "GetSamples", "加载数据记录！");

            // 指令字符串
            string cmdString =
                "SELECT TOP 3 [content] " +
                "   FROM [dbo].[ShortContent] " +
                "   WHERE [content] LIKE '%@SqlContent%';";

            // 创建对象
            List<string> samples = new List<string>();
            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 设定参数
                sqlCommand.Parameters.AddWithValue("SqlContent", value);
                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 循环处理
                while (reader.Read())
                {
                    // 设置参数
                    if (reader.IsDBNull(0)) continue;
                    // 获得内容
                    string content = reader.GetString(0);
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;
                    // 加入样本
                    samples.Add(content);
                }
                // 关闭数据阅读器
                reader.Close();
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("CoreCleaner", "GetSamples", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
            }

            // 记录日志
            //LogTool.LogMessage("CoreCleaner", "GetSamples", "数据记录已加载！");
            // 返回结果
            return samples.ToArray();
        }

#if _USE_CLR
        private static string GetRandom(string value)
#elif _USE_CONSOLE
        private static string? GetRandom(string? value)
#endif
        {
            // 记录日志
            //LogTool.LogMessage("CoreCleaner", "GetRandom", "加载数据记录！");

#if _USE_CLR
            // 计数器
            string content = null;
#elif _USE_CONSOLE
            // 计数器
            string? content = null;
#endif
            // 指令字符串
            string cmdString =
                string.IsNullOrEmpty(value) ?
                "SELECT TOP 1 [content] " +
                "   FROM [dbo].[CoreContent] " +
                "   WHERE operation = 0 AND [length] > 4 ORDER BY [count] DESC;" :
                string.Format(
                "SELECT TOP 1 [content] " +
                "   FROM [dbo].[CoreContent] " +
                "   WHERE operation = 0 AND [length] > 4 AND content LIKE '%{0}%' ORDER BY [count] DESC;", value);
            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
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
                LogTool.LogMessage("CoreCleaner", "GetRandom", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
            }

            // 记录日志
            //LogTool.LogMessage("CoreCleaner", "GetRandom", "数据记录已加载！");
            // 返回结果
            return content;
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            // 命令行
            string line;
            // 关键字
            string? key = null;
            // 循环处理
            do
            {
                // 获得随机数据
                string? value = GetRandom(key);
                // 检查结果
                if (string.IsNullOrEmpty(value))
                {
                    // 清理关键字
                    key = null;
                    // 重新选择
                    value = GetRandom(key);
                    // 检查结果
                    if(string.IsNullOrEmpty(value))
                    {
                        Console.WriteLine("没有需要清理的数据？"); break;
                    }
                }
                /*
                // 获得样本
                string[] samples = GetSamples(value);
                */
                // 获得数据
                CoreSegment? segment
                    = CoreCache.GetSegment(value);
                // 检查结果
                if (segment == null) continue;

                // 获得属性描述
                segment.Marks = MarkCache.GetMarks(value);

                // 获得描述
                string? description = segment.GetDescription();
                if (string.
                    IsNullOrEmpty(description))
                    description = string.Empty;
                else description = " |" + description;
                // 记录日志
                LogTool.LogMessage(
                    string.Format("\t(\"{0}\" : {1}{2}) = {3:0.00000000}",
                        segment.Value, segment.Count, description, segment.Gamma));
                // 打印切分结果
                for (int i = 0; segment.Subsegs != null && i < segment.Subsegs.Length; i++)
                {
                    // 检查参数
                    if(segment.Subsegs[i].Value != null)
                    {
                        // 获得属性
                        segment.Subsegs[i].Marks =
                            MarkCache.GetMarks(segment.Subsegs[i].Value);
                        segment.Subsegs[i].Count =
                            CounterTool.GetCount(segment.Subsegs[i].Value, false);
                    }

                    description = segment.Subsegs[i].GetDescription();
                    if (string.
                        IsNullOrEmpty(description))
                        description = string.Empty;
                    else description = " |" + description;
                    // 记录日志
                    LogTool.LogMessage(
                        string.Format("\t[{0}](\"{1}\" : {2}{3}) = {4:0.00000000}",
                                    i, segment.Subsegs[i].Value, segment.Subsegs[i].Count, description, segment.Subsegs[i].Gamma));
                }

                /*
                // 打印例子
                for(int i = 0;i < samples.Length;i ++)
                {
                    // 记录日志
                    LogTool.LogMessage(string.Format("\tsample[{0}] = ", i, samples[i]));
                }
                */

                // 读取一行
                line = Console.ReadLine()!;
                // 检查结果
                if (line == null || line.Length <= 0)
                {
                    // 标记数据
                    Mark(value);
                }
                else if (line.Equals("quit") || line.Equals("exit")) break;
                else if (line.Equals("del"))
                {
                    // 删除属性
                    MarkCache.DeleteMarks(value);
                    // 删除内容
                    CoreCache.DeleteSegment(value);

                    // 更新数据
                    GammaTool.MakeStatistic(value);

                    // 记录操作
                    OperationLog.Log("CoreSegment.Main", line + " " + value);
                    // 打印信息
                    Console.WriteLine(string.Format("内容({0})已删除！", value));
                }
                else
                {
                    // 执行命令行
                    if (MiscTool.Execute(line))
                    {
                        // 记录操作
                        OperationLog.Log("CoreSegment.Main", line);
                    }
                    else
                    {
                        Console.WriteLine("请输入正确的参数！");
                        Console.WriteLine("Usage : reload");
                        Console.WriteLine("Usage : (add | del) [string]");
                        Console.WriteLine("Usage : attr -(add| del) [string] [major]");
                        Console.WriteLine("\tmajor : 名词|动词|形容词|副词|代词|介词|连词|数词|量词|助词|感叹词|拟声词|数量词|习惯用语");
                    }
                }
            } while (true);
        }
#endif
    }
}
