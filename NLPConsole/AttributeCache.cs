using System;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NLP
{
    public class AttributeCache : AttributeContent
    {
        // 创建数据字典
        private static Dictionary<string, GrammarAttribute>
            attributes = new Dictionary<string, GrammarAttribute>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ClearAttributes()
        {
            // 记录日志
            LogTool.LogMessage("AttributeCache", "ClearAttributes", "开始执行！");
            // 恢复初始值
            attributes.Clear();
            // 记录日志
            LogTool.LogMessage("AttributeCache", "ClearAttributes", "执行完毕！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
#if _USE_CLR
        public static double[] GetVector(string value)
#elif _USE_CONSOLE
        public static double[]? GetVector(string value)
#endif
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
#if _USE_CLR
            // 获得属性
            GrammarAttribute
#elif _USE_CONSOLE
            // 获得属性
            GrammarAttribute?
#endif
            // 获得属性
            attribute = GetAttribute(value);
            // 检查结果
            if (attribute == null) return null;
            // 创建数组
            double[] outputs = new double[3];
            // 设置名词
            outputs[0] = attribute.GetPosibility(GrammarType.名词);
            // 检查结果
            if (outputs[0] < 0) outputs[01] = 0;
            // 设置动词
            outputs[1] = attribute.GetPosibility(GrammarType.动词);
            // 检查结果
            if (outputs[1] < 0) outputs[1] = 0;
            // 设置形容词
            outputs[2] = attribute.GetPosibility(GrammarType.形容词);
            // 检查结果
            if (outputs[2] < 0) outputs[2] = 0;
            // 返回结果
            return outputs;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
#if _USE_CLR
        public static GrammarAttribute GetAttribute(string value)
#elif _USE_CONSOLE
        public static GrammarAttribute? GetAttribute(string value)
#endif
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 返回结果
            return attributes.ContainsKey(value) ?
                attributes[value] : LoadAttribute(value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void AddAttribute(GrammarAttribute attribute)
        {
#if DEBUG
            Debug.Assert(attribute != null);
            Debug.Assert(!attribute.IsNullOrEmpty());
#endif
            // 在数据表中增加数据
            AddContent(attribute);

#if _USE_CLR
            // 获得参数
            string value = attribute.Value;
#elif _USE_CONSOLE
            // 获得参数
            string? value = attribute.Value;
#endif
#if DEBUG
            Debug.Assert(value != null);
#endif
            // 保持数据同步
            if (attributes.ContainsKey(value))
            {
                // 设置新数值
                attributes[value] = attribute;
            }
            // 增加对象
            else attributes.Add(value, attribute);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
#if _USE_CLR
        public static GrammarAttribute LoadAttribute(string value)
#elif _USE_CONSOLE
        public static GrammarAttribute? LoadAttribute(string value)
#endif
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 记录日志
            //LogTool.LogMessage("AttributeCache", "LoadAttribute", "加载数据记录！");

            // 指令字符串
            string cmdString =
                "SELECT " +
                "[gid], [noun], [verb], [pronoun], [numeral], " +
                "[adjective], [quantifier], [adverb], [auxiliary], " +
                "[preposition], [conjunction], [exclamation], [onomatopoeia], [others] " +
                "FROM [dbo].[AttributeContent] WHERE [content] = @SqlContent;";

#if _USE_CLR
            // 创建对象
            GrammarAttribute attribute = null;
#elif _USE_CONSOLE
            GrammarAttribute? attribute = null;
#endif
            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("AttributeCache", "LoadAttribute", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 设置参数
                sqlCommand.Parameters.AddWithValue("SqlContent", value);
                // 记录日志
                //LogTool.LogMessage("AttributeCache", "LoadAttribute", "参数已设定！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                //LogTool.LogMessage("AttributeCache", "LoadAttribute", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 创建对象
                    attribute = new GrammarAttribute();
                    // 设置参数
                    attribute.Value = value;
                    attribute.Index = reader.GetInt32(0);
                    // 创建数组
                    attribute.Posibilities = new double[13];
                    // 循环处理
                    for(int i = 0;i < 13;i ++)
                    {
                        attribute.Posibilities[i] = reader.GetDouble(i + 1);
                    }
                    // 检查结果
                    if (attributes.ContainsKey(value))
                    {
                        // 设置新数值
                        attributes[value] = attribute;
                    }
                    // 增加对象
                    else attributes.Add(value, attribute);
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                //LogTool.LogMessage("AttributeCache", "LoadAttribute", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("AttributeCache", "LoadAttribute", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("AttributeCache", "LoadAttribute", "数据库链接已关闭！");
            }

            // 记录日志
            //LogTool.LogMessage("AttributeCache", "LoadAttribute", "数据记录已加载！");
            // 返回结果
            return attribute;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int LoadAttributes()
        {
            // 清理数据记录
            attributes.Clear();
            // 记录日志
            LogTool.LogMessage("AttributeCache", "LoadAttributes", "加载数据记录！");

            // 指令字符串
            string cmdString =
                "SELECT " +
                "[gid], [content], [noun], [verb], [pronoun], [numeral] " +
                "[adjective], [quantifier], [adverb], [auxiliary], " +
                "[preposition], [conjunction], [exclamation], [onomatopoeia], [others] " +
                "FROM [dbo].[AttributeContent];";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("AttributeCache", "LoadAttributes", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("AttributeCache", "LoadAttributes", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("AttributeCache", "LoadAttributes", "T-SQL指令已执行！");

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
                        LogTool.LogMessage("AttributeCache", "LoadAttributes",
                            string.Format("{0} items loaded !", total));
                    }

                    // 检查参数
                    if (reader.IsDBNull(1)) continue;
                    // 获得内容
                    string value = reader.GetString(1);
                    // 检查结果
                    if (string.IsNullOrEmpty(value)) continue;

                    // 创建对象
                    GrammarAttribute attribute
                        = new GrammarAttribute();
                    // 设置参数
                    attribute.Value = value;
                    attribute.Index = reader.GetInt32(0);
                    // 创建数组
                    attribute.Posibilities = new double[13];
                    // 循环处理
                    for (int i = 0; i < 13; i++)
                    {
                        attribute.Posibilities[i] = reader.GetFloat(i + 2);
                    }
                    // 检查结果
                    if (attributes.ContainsKey(value))
                    {
                        // 设置新数值
                        attributes[value] = attribute;
                    }
                    // 增加对象
                    else attributes.Add(value, attribute);
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("AttributeCache", "LoadAttributes",
                    string.Format("{0} items loaded !", total));
                // 记录日志
                LogTool.LogMessage("AttributeCache", "LoadAttributes", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("AttributeCache", "LoadAttributes", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("AttributeCache", "LoadAttributes", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("\tattributes.count = " + attributes.Count);
            // 记录日志
            LogTool.LogMessage("AttributeCache", "LoadAttributes", "数据记录已加载！");
            // 返回结果
            return attributes.Count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SaveAttributes()
        {
            // 记录日志
            LogTool.LogMessage("AttributeCache", "SaveAttributes", "保存数据记录！");
            LogTool.LogMessage(string.Format("\tattributes.count = {0}", attributes.Count));

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
                foreach (KeyValuePair<string, GrammarAttribute> kvp in attributes)
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("AttributeCache", "SaveAttributes",
                                string.Format("{0} items saved !", total));
                    }

                    // 启动进程
                    tasks.Add(factory.StartNew(()
                            => AddContent(kvp.Value)));
                    // 检查任务数量
                    if (tasks.Count >= 10000)
                    { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 打印记录
                LogTool.LogMessage("AttributeCache", "SaveAttributes",
                        string.Format("{0} items saved !", total));
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("AttributeCache", "SaveAttributes", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("AttributeCache", "SaveAttributes", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\tattributes.count = " + attributes.Count);
            // 记录日志
            LogTool.LogMessage("AttributeCache", "SaveAttributes", "数据记录已保存！");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void MakeStatistic()
        {
            // 记录日志
            LogTool.LogMessage("AttributeCache", "MakeStatistic", "开始统计数据！");

            // 计数器
            int total = 0;

            // 指令字符串
            string cmdString =
                "SELECT [content], [pos], [count] FROM [dbo].[LTPAttributeContent];";
            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("AttributeCache", "MakeStatistic", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("AttributeCache", "MakeStatistic", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("AttributeCache", "MakeStatistic", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 增加计数
                    total++;
                    // 检查结果
                    if (total % 10000 == 0)
                    {
                        // 记录日志
                        LogTool.LogMessage("AttributeCache", "MakeStatistic",
                            string.Format("{0} items processed !", total));
                    }

                    // 检查参数
                    if (reader.IsDBNull(0)) continue;
                    // 获得数值
                    string value = reader.GetString(0);
                    // 检查参数
                    if (string.IsNullOrEmpty(value)) continue;
                    // 检查该词汇是否在识别范围内
                    if (value.Length > 1
                        && CoreCache.GetCount(value) <= 0) continue;
                    else if (value.Length == 1
                        && TokenCache.GetCount(value[0]) <= 0) continue;

                    // 检查参数
                    if (reader.IsDBNull(1)) continue;
                    // 获得数值
                    string pos = reader.GetString(1);
                    // 检查参数
                    if (string.IsNullOrEmpty(pos)) continue;

                    // 获得数值
                    int count = reader.GetInt32(2);
                    // 检查参数
                    if (count <= 0) continue;

#if _USE_CLR
                    GrammarAttribute
#elif _USE_CONSOLE
                    GrammarAttribute?
#endif
                    // 获得对象
                    attribute = GetAttribute(value);
                    // 检查结果
                    if (attribute == null)
                    {
                        // 创建对象
                        attribute =
                            new GrammarAttribute();
                        // 设置参数
                        attribute.Value = value;
                    }
#if DEBUG
                    Debug.Assert(attribute != null);
#endif
                    // 根据属性设置参数
                    if (pos.StartsWith(WordType.NOUN))
                    {
                        // 获得计数
                        double sum = attribute.GetPosibility(GrammarType.名词);
                        // 设置参数
                        if (sum < 0) sum = 0;
                        // 设置参数
                        attribute.SetPosibility(GrammarType.名词, count + sum);
                    }
                    else if (pos.StartsWith(WordType.VERB))
                    {
                        // 获得计数
                        double sum = attribute.GetPosibility(GrammarType.动词);
                        // 设置参数
                        if (sum < 0) sum = 0;
                        // 设置参数
                        attribute.SetPosibility(GrammarType.动词, count + sum);
                    }
                    else if (pos.StartsWith(WordType.PRONOUN))
                    {
                        // 获得计数
                        double sum = attribute.GetPosibility(GrammarType.代词);
                        // 设置参数
                        if (sum < 0) sum = 0;
                        // 设置参数
                        attribute.SetPosibility(GrammarType.代词, count + sum);
                    }
                    else if (pos.StartsWith(WordType.NUMERAL))
                    {
                        // 获得计数
                        double sum = attribute.GetPosibility(GrammarType.数词);
                        // 设置参数
                        if (sum < 0) sum = 0;
                        // 设置参数
                        attribute.SetPosibility(GrammarType.数词, count + sum);
                    }
                    else if (pos.StartsWith(WordType.ADJECTIVE))
                    {
                        // 获得计数
                        double sum = attribute.GetPosibility(GrammarType.形容词);
                        // 设置参数
                        if (sum < 0) sum = 0;
                        // 设置参数
                        attribute.SetPosibility(GrammarType.形容词, count + sum);
                    }
                    else if (pos.StartsWith(WordType.QUANTIFIER))
                    {
                        // 获得计数
                        double sum = attribute.GetPosibility(GrammarType.量词);
                        // 设置参数
                        if (sum < 0) sum = 0;
                        // 设置参数
                        attribute.SetPosibility(GrammarType.量词, count + sum);
                    }
                    else if (pos.StartsWith(WordType.ADVERB))
                    {
                        // 获得计数
                        double sum = attribute.GetPosibility(GrammarType.副词);
                        // 设置参数
                        if (sum < 0) sum = 0;
                        // 设置参数
                        attribute.SetPosibility(GrammarType.副词, count + sum);
                    }
                    else if (pos.StartsWith(WordType.AUXILIARY))
                    {
                        // 获得计数
                        double sum = attribute.GetPosibility(GrammarType.助词);
                        // 设置参数
                        if (sum < 0) sum = 0;
                        // 设置参数
                        attribute.SetPosibility(GrammarType.助词, count + sum);
                    }
                    else if (pos.StartsWith(WordType.PREPOSITION))
                    {
                        // 获得计数
                        double sum = attribute.GetPosibility(GrammarType.介词);
                        // 设置参数
                        if (sum < 0) sum = 0;
                        // 设置参数
                        attribute.SetPosibility(GrammarType.介词, count + sum);
                    }
                    else if (pos.StartsWith(WordType.CONJUNCTION))
                    {
                        // 获得计数
                        double sum = attribute.GetPosibility(GrammarType.连词);
                        // 设置参数
                        if (sum < 0) sum = 0;
                        // 设置参数
                        attribute.SetPosibility(GrammarType.连词, count + sum);
                    }
                    else if (pos.StartsWith(WordType.EXCLAMATION))
                    {
                        // 获得计数
                        double sum = attribute.GetPosibility(GrammarType.感叹词);
                        // 设置参数
                        if (sum < 0) sum = 0;
                        // 设置参数
                        attribute.SetPosibility(GrammarType.感叹词, count + sum);
                    }
                    else if (pos.StartsWith(WordType.ONOMATOPOEIA))
                    {
                        // 获得计数
                        double sum = attribute.GetPosibility(GrammarType.拟声词);
                        // 设置参数
                        if (sum < 0) sum = 0;
                        // 设置参数
                        attribute.SetPosibility(GrammarType.拟声词, count + sum);
                    }
                    // 其他类别
                    else
                    {
                        // 获得计数
                        double sum = attribute.GetPosibility(GrammarType.衬词);
                        // 设置参数
                        if (sum < 0) sum = 0;
                        // 设置参数
                        attribute.SetPosibility(GrammarType.衬词, count + sum);
                    }
                    // 检查结果
                    if (!attributes.ContainsKey(value)) attributes.Add(value, attribute);
                }

                // 记录日志
                LogTool.LogMessage("AttributeCache", "MakeStatistic",
                string.Format("{0} items processed !", total));
                // 记录日志
                LogTool.LogMessage("AttributeCache", "MakeStatistic", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("AttributeCache", "MakeStatistic", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }

            // 记录日志
            LogTool.LogMessage("\tattributes.Count = " + attributes.Count);
            // 记录日志
            LogTool.LogMessage("AttributeCache", "MakeStatistic", "数据统计完毕！");

            // 记录日志
            LogTool.LogMessage("AttributeCache", "MakeStatistic", "重新计算数据！");
            // 循环处理
            foreach(KeyValuePair<string, GrammarAttribute> attribute in attributes)
            {
#if DEBUG
                Debug.Assert(attribute.Value != null);
                Debug.Assert(attribute.Value.Posibilities != null);
#endif
                double sum = 0;
                // 循环处理
                foreach(double value in attribute.Value.Posibilities) sum += value;
#if DEBUG
                Debug.Assert(sum > 0);
                Debug.Assert(attribute.Value.Posibilities.Length == 13);
#endif
                // 循环处理（归一化数值）
                for (int i = 0;i < 13; i ++) attribute.Value.Posibilities[i] /= sum;
            }
            // 记录日志
            LogTool.LogMessage("AttributeCache", "MakeStatistic", "数据计算完毕！");
        }

#if _USE_CONSOLE
        public static new void Main(string[] args)
        {
            Console.WriteLine("准备创建AttributeContent表，原表及其数据将被删除！");
            Console.WriteLine("确认是否执行（Yes/No）？");
            // 检查确认
            if (!ConsoleTool.Confirm()) return;

            // 检查数据表
            if(!DBTool.TableExists("LTPAttributeContent"))
            {
                Console.WriteLine("数据表LTPAttributeContent不存在！"); return;
            }

            // 开启日志
            LogTool.SetLog(true);
            // 创建计时器
            Stopwatch watch = new Stopwatch();
            // 开启计时器
            watch.Start();
            // 创建数据表
            CreateTable();
            // 开始统计数据
            MakeStatistic();
            // 开始保存数据
            SaveAttributes();
            // 清理数据
            ClearAttributes();
            // 关闭计时器
            watch.Stop();
            // 打印结果
            Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
        }
#endif
    }
}
