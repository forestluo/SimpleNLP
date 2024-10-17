using System.Text;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NLP
{
    public class GammaTool
    {
        public static double GetGamma(string content)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(content));
#endif
            // 检查参数
            if (content.Length == 1) return +1.0f;
            // 累计值
            double result = 0.0f;
            // 最小计数
            int minCount = int.MaxValue;
            // 循环处理
            foreach (char item in content)
            {
                // 获得数值
                int count =
                    TokenCache.GetCount(item);
                // 检查结果
                if (count <= 0) count = 1;
                // 寻找最小值
                if (count < minCount) minCount = count;
                // 累加结果
                result += 1.0f / (double)count;
            }
            // 获得数值
            int total = CounterTool.GetCount(content, true);
            // 检查结果
            if (total <= 0)
            {
                // 让计数等于最小值
                total = minCount > 1 ? minCount : 1;
            }
            // 返回结果
            return result * (double)total / (double)content.Length;
        }

        // 使用全局计数
        public static double GetGamma(string[] contents)
        {
            // 检查参数
            if (contents == null ||
                contents.Length <= 0) return -1.0f;
            // 检查长度
            if (contents.Length == 1)
            {
                // 返回结果
                return GetGamma(contents[0]);
            }

            // 对象
            StringBuilder sb = new StringBuilder();
            // 累计值
            double result = 0.0f;
            // 循环处理
            foreach (string content in contents)
            {
                // 增加内容
                sb.Append(content);

                // 获得数值
                int count =
                    CounterTool.
                    GetCount(content, true);
                // 检查结果
                if (count <= 0) return -1.0f;

                // 累加结果
                result += 1.0f / (double)count;
            }

            // 获得数值
            int total = CounterTool.GetCount(sb.ToString(), true);
            // 返回结果
            return total < 0 ? -1.0f : result * (double)total / (double)contents.Length;
        }

        // 使用全局计数
        public static double GetGamma(bool prefix, string single, string[] groups)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(single));
            Debug.Assert(groups != null && groups.Length > 0);
#endif
            // 频次参数
            double fSingle = CounterTool.GetCount(single, true);
            // 检查结果
            if (fSingle <= 0) return -1.0f;

            // 频次参数
            double fCombination = 0; double fGroup = 0;
            // 增加数据
            foreach (string group in groups)
            {
                // 获得计数
                int fg = CounterTool.GetCount(group, true); if (fg > 0) fGroup += fg;
                // 获得新值
                string newValue = prefix ? (single + group) : (group + single);
                // 获得计数
                int count = CounterTool.GetCount(newValue, true); if (count > 0) fCombination += count;
            }
            // 检查结果
            if (fGroup <= 0.0f || fSingle <= 0.0f) return -1.0f;
            //返回结果
            return fCombination * 0.5f * (1.0f / fGroup + 1.0f / fSingle);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
#if _USE_CLR
        public static void MakeStatistic(string value = null)
#elif _USE_CONSOLE
        public static void MakeStatistic(string? value = null)
#endif
        {
            // 记录日志
            LogTool.LogMessage("GammaTool", "MakeStatistic", "开始统计数据！");

            // 计数器
            int total = 0;

            // 必须严格按照顺序执行。
            // 相互之间不允许出现可能的交叉干扰。
            // 指令字符串
            string cmdString;
            // 检查结果
            if(string.IsNullOrEmpty(value))
            {
                cmdString = "SELECT [flag], [content] " +
                    "   FROM [dbo].[CoreContent] ORDER BY [length];";
            }
            else
            {
                cmdString = string.Format(
                    "SELECT [flag], [content] FROM [dbo].[CoreContent] " +
                    "   WHERE [content] LIKE '%{0}%' ORDER BY [length]; ", value);
            }

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("GammaTool", "MakeStatistic", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("GammaTool", "MakeStatistic", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("GammaTool", "MakeStatistic", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 增加计数
                    total++;
                    // 检查结果
                    if (total % 10000 == 0)
                    {
                        // 记录日志
                        LogTool.LogMessage("GammaTool", "MakeStatistic",
                            string.Format("{0} items processed !", total));
                    }

                    // 获得内容
                    string content = reader.GetString(1);
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;

                    // 获得标志位
                    int flag = reader.GetInt32(0);
                    // 检查标志位
                    if((flag & 0x800) != 0)
                    {
#if _USE_CLR
                        CoreSegment
#elif _USE_CONSOLE
                        CoreSegment?
#endif
                        segment =
                            // 加载数据
                            CoreCache.GetSegment(content);
#if DEBUG
                        Debug.Assert(segment != null);
#endif
                        // 检查结果
                        if(!segment.IsLeaf())
                        {
                            // 计算Gamma数值
                            segment.Gamma =
                                segment.GetGamma();
                            // 更新数据
                            CoreCache.AddSegment(segment); continue;
                        }
                    }
                    // 按照常规分割处理
                    {
                        // 分割内容
                        // 只需要使用最大分割法？
                        CoreSegment segment = CoreSegment.Split(content);
                        // 检查结果
                        if (segment == null || segment.IsLeaf()) continue;
                        // 检查结果
                        if (segment.Gamma >= 0)
                        {
#if DEBUG
                            Debug.Assert(segment.Subsegs != null);
#endif
                            // 获得最合适路径
                            segment =
#if _USE_CLR
                                segment.SelectPath();
#elif _USE_CONSOLE
                                segment.SelectPath()!;
#endif
#if DEBUG
                            Debug.Assert(segment != null);
                            Debug.Assert(!segment.IsLeaf());
#endif
                            // 更新数据
                            CoreCache.AddSegment(segment);
                        }
                    }
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("GammaTool", "MakeStatistic",
                    string.Format("{0} items processed !", total));
                // 记录日志
                LogTool.LogMessage("GammaTool", "MakeStatistic", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("GammaTool", "MakeStatistic", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("GammaTool", "MakeStatistic", "数据库链接已关闭！");
            }
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备计算Gamma数据，原表及其数据将被更改！");
            Console.WriteLine("确认是否执行（Yes/No）？");
            // 检查确认
            if(!ConsoleTool.Confirm()) return;

            // 开启日志
            LogTool.SetLog(true);
            // 创建计时器
            Stopwatch watch = new Stopwatch();
            // 开启计时器
            watch.Start();
            // 检查数据表
            if (!DBTool.TableExists("CoreContent"))
            {
                // 记录日志
                Console.WriteLine("数据表CoreContent不存在！"); return;
            }
            // 恢复初始值
            CoreContent.ResetCounter();
            // 加载数据
            CoreCache.LoadSegments();
            // 开始统计
            MakeStatistic();
            // 保存数据
            CoreCache.SaveSegments();
            // 关闭计时器
            watch.Stop();
            // 打印结果
            Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
        }
#endif
    }
}
