using System;
using System.Text;
using System.Data;
using System.Linq;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NLP
{
    public class MarkCache : MarkContent
    {
        // 创建数据字典
        private static Dictionary<string, MarkAttribute[]>
            marks = new Dictionary<string, MarkAttribute[]>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ClearMarks()
        {
            // 记录日志
            LogTool.LogMessage("MarkCache", "ClearMarks", "开始执行！");
            // 恢复初始值
            marks.Clear();
            // 记录日志
            LogTool.LogMessage("MarkCache", "ClearMarks", "执行完毕！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void DeleteMarks(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 从数据表中删除
            DeleteContents(value);
            // 从缓冲中删除
            if (marks.ContainsKey(value)) marks.Remove(value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void DeleteMark(string value, MarkAttribute attribute)
        {
#if DEBUG
            Debug.Assert(attribute != null);
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 从数据表中删除
            DeleteContent(value,
                new MarkAttribute[] { attribute });
            // 从缓冲中删除
            if (marks.ContainsKey(value)) marks.Remove(value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void AddMark(string value, MarkAttribute attribute)
        {
#if DEBUG
            Debug.Assert(attribute != null);
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif

#if _USE_CLR
            // 获得属性
            MarkAttribute[]
#elif _USE_CONSOLE
            // 获得属性
            MarkAttribute[]?
#endif
            // 加载结果
            attributes = LoadMark(value);
            // 检查结果
            if (attributes == null ||
                !attributes.Any(s => s.Equals(attribute)))
            {
                // 增加数据库对象
                AddContent(value, new MarkAttribute[] { attribute });

                // 检查缓冲
                if (marks.ContainsKey(value))
                {
                    // 删除缓冲
                    marks.Remove(value);
                    // 重新加载
                    attributes = LoadMark(value);
#if DEBUG
                    Debug.Assert(attributes != null && attributes.Length > 0);
#endif
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
#if _USE_CLR
        public static MarkAttribute GetMark(string value, string major, string minor = null)
#elif _USE_CONSOLE
        public static MarkAttribute? GetMark(string value, string major, string? minor = null)
#endif
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
            Debug.Assert(!string.IsNullOrEmpty(major));
#endif

#if _USE_CLR
            MarkAttribute[]
#elif _USE_CONSOLE
            MarkAttribute[]?
#endif
            // 获得属性
            marks = GetMarks(value);
            // 检查结果
            if (marks == null) return null;
            // 检查结果
            if (string.IsNullOrEmpty(minor))
                return marks.First(s => s.Equals(major));
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(minor));
#endif
            // 返回结果
            return marks.First(s => major.Equals(s.Value) && minor.Equals(s.Remark));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
#if _USE_CLR
        public static MarkAttribute[] GetMarks(string value)
#elif _USE_CONSOLE
        public static MarkAttribute[]? GetMarks(string? value)
#endif
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 返回结果
            return marks.ContainsKey(value) ?
                marks[value] : LoadMark(value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
#if _USE_CLR
        public static MarkAttribute[] LoadMark(string value)
#elif _USE_CONSOLE
        public static MarkAttribute[]? LoadMark(string value)
#endif
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 记录日志
            //LogTool.LogMessage("MarkCache", "LoadMark", "加载数据记录！");

            // 指令字符串
            string cmdString =
                "SELECT " +
                "[aid], [class], [subclass] " +
                "FROM [dbo].[MarkContent] WHERE [content] = @SqlContent;";

            // 创建对象
            List<MarkAttribute> attributes = new List<MarkAttribute>();
            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("MarkCache", "LoadMark", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 设置参数
                sqlCommand.Parameters.AddWithValue("SqlContent", value);
                // 记录日志
                //LogTool.LogMessage("MarkCache", "LoadMark", "参数已设定！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                //LogTool.LogMessage("MarkCache", "LoadMark", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 检查结果
                    if (reader.IsDBNull(1)) continue;

                    // 创建对象
                    MarkAttribute attribute =
                            new MarkAttribute();
                    // 设置参数
                    attribute.Index = reader.GetInt32(0);
                    attribute.Value = reader.GetString(1);
                    if (!reader.IsDBNull(2))
                        attribute.Remark = reader.GetString(2);
                    // 增加到链表
                    attributes.Add(attribute);
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                //LogTool.LogMessage("MarkCache", "LoadMark", "数据阅读器已关闭！");

                // 检查结果
                if(attributes.Count > 0)
                {
                    // 检查数据
                    if (marks.ContainsKey(value))
                    {
                        // 设置新数值
                        marks[value] = attributes.ToArray();
                    }
                    // 增加对象
                    else marks.Add(value, attributes.ToArray());
                    // 记录日志
                    //LogTool.LogMessage("MarkCache", "LoadMark", "缓冲数据已更新！");
                }
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("MarkCache", "LoadMark", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("MarkCache", "LoadMark", "数据库链接已关闭！");
            }

            // 记录日志
            //LogTool.LogMessage("MarkCache", "LoadMark", "数据记录已加载！");
            // 返回结果
            return attributes.Count > 0 ? attributes.ToArray() : null;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void LoadMarks()
        {
            // 清理数据记录
            marks.Clear();
            // 记录日志
            LogTool.LogMessage("MarkCache", "LoadMarks", "加载数据记录！");

            // 指令字符串
            string cmdString =
                "SELECT DISTINCT([content]) FROM [dbo].[MarkContent];";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("MarkCache", "LoadMarks", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("MarkCache", "LoadMarks", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("MarkCache", "LoadMarks", "T-SQL指令已执行！");

                // 计数器
                int total = 0;
                // 循环处理
                while (reader.Read())
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 记录日志
                        LogTool.LogMessage("MarkCache", "LoadMarks",
                            string.Format("{0} items loaded !", total));
                    }

                    // 检查结果
                    if (reader.IsDBNull(0)) continue;
                    // 读取content
                    string value = reader.GetString(0);
#if _USE_CLR
                    // 获得属性
                    MarkAttribute[]
#elif _USE_CONSOLE
                    // 获得属性
                    MarkAttribute[]?
#endif
                    // 加载属性
                    attributes = LoadMark(value);
#if DEBUG
                    Debug.Assert(attributes != null);
#endif
                    // 检查数据
                    if (marks.ContainsKey(value))
                    {
                        // 设置新数值
                        marks[value] = attributes;
                    }
                    // 增加对象
                    else marks.Add(value, attributes);
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("MarkCache", "LoadMarks",
                    string.Format("{0} items loaded !", total));
                // 记录日志
                LogTool.LogMessage("MarkCache", "LoadMarks", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("MarkCache", "LoadMarks", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("MarkCache", "LoadMarks", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\tmarks.count = " + marks.Count);
            // 记录日志
            LogTool.LogMessage("MarkCache", "LoadMarks", "数据记录已加载！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SaveMarks()
        {
            // 记录日志
            LogTool.LogMessage("MarkCache", "SaveMarks", "保存数据记录！");
            LogTool.LogMessage(string.Format("\tmarks.count = {0}", marks.Count));

            // 计数器
            int total = 0;
            // 任务数组
            List<Task> tasks = new List<Task>();
            // 生成任务控制器
            TaskFactory factory = new TaskFactory(
                new LimitedConcurrencyLevelTaskScheduler(DBTool.MAX_THREADS));

            try
            {
                // 遍历参数
                foreach (KeyValuePair<string, MarkAttribute[]> kvp in marks)
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("MarkCache", "SaveMarks",
                                string.Format("{0} items saved !", total));
                    }

                    // 启动进程
                    tasks.Add(factory.StartNew(() =>
                        AddContent(kvp.Key, kvp.Value)));
                    // 检查任务数量
                    if (tasks.Count >= 10000)
                        { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 打印记录
                LogTool.LogMessage("MarkCache", "SaveMarks",
                        string.Format("{0} items saved !", total));
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("MarkCache", "SaveMarks", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("MarkCache", "SaveMarks", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\tmarks.count = " + marks.Count);
            // 记录日志
            LogTool.LogMessage("MarkCache", "SaveMarks", "数据记录已保存！");
        }
    }
}
