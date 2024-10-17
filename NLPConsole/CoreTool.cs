using System.Data;
using System.Diagnostics;
using System.Collections;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NLP
{
    public class CoreTool
    {
        private static void Clear()
        {
            string cmdString;

            // 删除Count值异常的内容
            cmdString = "DELETE FROM [dbo].[CoreContent] " +
                "   WHERE [count] <= 0;";
            // 执行指令
            DBTool.ExecuteNonQuery(cmdString);

            // 删除Gamma值异常的内容
            cmdString = "DELETE FROM [dbo].[CoreContent] " +
                "   WHERE [gamma] < 0 OR [gamma] > 1;";
            // 执行指令
            DBTool.ExecuteNonQuery(cmdString);
        }

        private static int DoItAgain()
        {
            string cmdString =
                "SELECT [operation] " +
                "   FROM [dbo].[OperationLog] ORDER BY [lid];";

            // 计数
            int total = 0;
            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("CoreTool", "DoItAgain", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                //LogTool.LogMessage("CoreTool", "DoItAgain", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                //LogTool.LogMessage("CoreTool", "DoItAgain", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    if (reader.IsDBNull(0)) continue;
                    // 获得操作
                    string operation = reader.GetString(0);
                    // 检查结果
                    if (string.IsNullOrEmpty(operation)) continue;

                    // 记录日志
                    LogTool.LogMessage("CoreTool", "DoItAgain", operation);
                    // 执行命令行
                    if(!ConsoleTool.Execute(operation))
                    {
                        // 记录日志
                        LogTool.LogMessage("CoreTool", "DoItAgain",
                            string.Format("operation(\"{0}\") failed ！", operation));
                    }
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                //LogTool.LogMessage("CoreTool", "DoItAgain", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("CoreTool", "DoItAgain", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("CoreTool", "DoItAgain", "数据库链接已关闭！");
            }

            // 返回结果
            return total;
        }

        private static void CopyInternal()
        {
            foreach (string unit in
                Quantity.GetUnits())
            {
                {
                    // 创建对象
                    MarkAttribute mark = new MarkAttribute();
                    // 设置参数
                    mark.Index = 0;
                    mark.Value = WordType.QUANTIFIER;
                    mark.Remark = "计量单位";
                    // 加入属性
                    MarkCache.AddMark(unit, mark);
                }

                if (unit.Length > 1)
                {
                    // 创建对象
                    CoreSegment segment = new CoreSegment();
                    // 设置参数
                    segment.Flag = 0;
                    segment.Index = 0;
                    segment.Value = unit;
                    segment.Gamma = -1.0f;
                    segment.Length = unit.Length;
                    segment.Count = CounterTool.GetCount(unit, true);
                    // 加入内容
                    CoreCache.AddSegment(segment);
                }
            }

            foreach (string quantifier in
                Quantity.GetQuantifiers())
            {
                {
                    // 创建对象
                    MarkAttribute mark = new MarkAttribute();
                    // 设置参数
                    mark.Index = 0;
                    mark.Value = WordType.QUANTIFIER;
                    // 加入属性
                    MarkCache.AddMark(quantifier, mark);
                }

                if (quantifier.Length > 1)
                {
                    // 创建对象
                    CoreSegment segment = new CoreSegment();
                    // 设置参数
                    segment.Flag = 0;
                    segment.Index = 0;
                    segment.Value = quantifier;
                    segment.Gamma = -1.0f;
                    segment.Length = quantifier.Length;
                    segment.Count = CounterTool.GetCount(quantifier, true);
                    // 加入内容
                    CoreCache.AddSegment(segment);
                }
            }

            foreach (string currency in
                Quantity.GetCurrencies())
            {
                {
                    // 创建对象
                    MarkAttribute mark = new MarkAttribute();
                    // 设置参数
                    mark.Index = 0;
                    mark.Value = WordType.QUANTIFIER;
                    mark.Remark = "货币";
                    // 加入属性
                    MarkCache.AddMark(currency, mark);
                }

                if (currency.Length > 1)
                {
                    // 创建对象
                    CoreSegment segment = new CoreSegment();
                    // 设置参数
                    segment.Flag = 0;
                    segment.Index = 0;
                    segment.Value = currency;
                    segment.Gamma = -1.0f;
                    segment.Length = currency.Length;
                    segment.Count = CounterTool.GetCount(currency, true);
                    // 加入内容
                    CoreCache.AddSegment(segment);
                }
            }

            foreach (string surname in
                PersonName.GetSurnames())
            {
                {
                    // 创建对象
                    MarkAttribute mark = new MarkAttribute();
                    // 设置参数
                    mark.Index = 0;
                    mark.Value = WordType.NOUN;
                    mark.Remark = PersonName.SURNAME;
                    // 加入属性
                    MarkCache.AddMark(surname, mark);
                }

                if (surname.Length > 1)
                {
                    // 创建对象
                    CoreSegment segment = new CoreSegment();
                    // 设置参数
                    segment.Flag = 0;
                    segment.Index = 0;
                    segment.Value = surname;
                    segment.Gamma = -1.0f;
                    segment.Length = surname.Length;
                    segment.Count = CounterTool.GetCount(surname, true);
                    // 加入内容
                    CoreCache.AddSegment(segment);
                }
            }

            foreach (string gis in
                GISName.GetGises())
            {
                {
                    // 创建对象
                    MarkAttribute mark = new MarkAttribute();
                    // 设置参数
                    mark.Index = 0;
                    mark.Value = WordType.NOUN;
                    mark.Remark = "地名";
                    // 加入属性
                    MarkCache.AddMark(gis, mark);
                }

                if (gis.Length > 1)
                {
                    // 创建对象
                    CoreSegment segment = new CoreSegment();
                    // 设置参数
                    segment.Flag = 0;
                    segment.Index = 0;
                    segment.Value = gis;
                    segment.Gamma = -1.0f;
                    segment.Length = gis.Length;
                    segment.Count = CounterTool.GetCount(gis, true);
                    // 加入内容
                    CoreCache.AddSegment(segment);
                }
            }

            foreach (string region in
                RegionName.GetRegions())
            {
                {
                    // 创建对象
                    MarkAttribute mark = new MarkAttribute();
                    // 设置参数
                    mark.Index = 0;
                    mark.Value = WordType.NOUN;
                    mark.Remark = "地名";
                    // 加入属性
                    MarkCache.AddMark(region, mark);
                }

                if (region.Length > 1)
                {
                    // 创建对象
                    CoreSegment segment = new CoreSegment();
                    // 设置参数
                    segment.Flag = 0;
                    segment.Index = 0;
                    segment.Value = region;
                    segment.Gamma = -1.0f;
                    segment.Length = region.Length;
                    segment.Count = CounterTool.GetCount(region, true);
                    // 加入内容
                    CoreCache.AddSegment(segment);
                }
            }

            foreach (string location in
                LocationName.GetLocations())
            {
                {
                    // 创建对象
                    MarkAttribute mark = new MarkAttribute();
                    // 设置参数
                    mark.Index = 0;
                    mark.Value = WordType.NOUN;
                    mark.Remark = "地名";
                    // 加入属性
                    MarkCache.AddMark(location, mark);
                }

                if (location.Length > 1)
                {
                    // 创建对象
                    CoreSegment segment = new CoreSegment();
                    // 设置参数
                    segment.Flag = 0;
                    segment.Index = 0;
                    segment.Value = location;
                    segment.Gamma = -1.0f;
                    segment.Length = location.Length;
                    segment.Count = CounterTool.GetCount(location, true);
                    // 加入内容
                    CoreCache.AddSegment(segment);
                }
            }

            foreach (string verb in
                VerbWord.GetVerbs())
            {
                {
                    // 创建对象
                    MarkAttribute mark = new MarkAttribute();
                    // 设置参数
                    mark.Index = 0;
                    mark.Value = WordType.VERB;
                    // 加入属性
                    MarkCache.AddMark(verb, mark);
                }

                if (verb.Length > 1)
                {
                    // 创建对象
                    CoreSegment segment = new CoreSegment();
                    // 设置参数
                    segment.Flag = 0;
                    segment.Index = 0;
                    segment.Value = verb;
                    segment.Gamma = -1.0f;
                    segment.Length = verb.Length;
                    segment.Count = CounterTool.GetCount(verb, true);
                    // 加入内容
                    CoreCache.AddSegment(segment);
                }
            }

            foreach (string[] function in
                FunctionWord.GetAllFunctions())
            {
                {
                    // 创建对象
                    MarkAttribute mark = new MarkAttribute();
                    // 设置参数
                    mark.Index = 0;
                    mark.Value = function[1];
                    // 加入属性
                    MarkCache.AddMark(function[0], mark);
                }

                if (function[0].Length > 1)
                {
                    // 创建对象
                    CoreSegment segment = new CoreSegment();
                    // 设置参数
                    segment.Flag = 0;
                    segment.Index = 0;
                    segment.Value = function[0];
                    segment.Gamma = -1.0f;
                    segment.Length = function[0].Length;
                    segment.Count = CounterTool.GetCount(function[0], true);
                    // 加入内容
                    CoreCache.AddSegment(segment);
                }
            }

            // 循环处理
            foreach (string[] item in SingleWord.GetAllWords())
            {
#if DEBUG
                Debug.Assert(item != null && item.Length >= 2);
#endif
                {
                    // 创建对象
                    MarkAttribute mark = new MarkAttribute();
                    // 设置参数
                    mark.Index = 0;
                    mark.Value = item[1];
                    mark.Remark = item[2];
                    // 加入属性
                    MarkCache.AddMark(item[0], mark);
                }

                if (item[0].Length > 1)
                {
                    // 创建对象
                    CoreSegment segment = new CoreSegment();
                    // 设置参数
                    segment.Flag = 0;
                    segment.Index = 0;
                    segment.Value = item[0];
                    segment.Gamma = -1.0f;
                    segment.Length = item[0].Length;
                    segment.Count = CounterTool.GetCount(item[0], true);
                    // 加入内容
                    CoreCache.AddSegment(segment);
                }
            }
        }

#if _USE_CLR
        private static void CopyDictionary(string cmdString,
                    string mainClass, string subClass)
#elif _USE_CONSOLE
        private static void CopyDictionary(string cmdString,
                    string? mainClass = null, string? subClass = null)
#endif
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(cmdString));
#endif
            // 记录日志
            LogTool.LogMessage("CoreTool", "CopyDictionary", "开始拷贝数据！");

            // 计数器
            int total = 0;
            // 任务数组
            List<Task> tasks = new List<Task>();
            // 生成任务控制器
            TaskFactory factory = new TaskFactory(
                new LimitedConcurrencyLevelTaskScheduler(DBTool.MAX_THREADS));

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("CoreTool", "CopyDictionary", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("CoreTool", "CopyDictionary", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("CoreTool", "CopyDictionary", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 增加计数
                    total++;
                    // 检查结果
                    if (total % 10000 == 0)
                    {
                        // 记录日志
                        LogTool.LogMessage("CoreTool", "CopyDictionary",
                            string.Format("{0} items processed !", total));
                    }

                    // 检查参数
                    if (reader.IsDBNull(0)) continue;
#if _USE_CLR
                    // 获得内容
                    string content = reader.GetString(0);
#elif _USE_CONSOLE
                    // 获得内容
                    string? content = reader.GetString(0);
#endif
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) continue;

                    // 获得计数
                    int count = reader.GetInt32(1);
                    // 启动进程
                    tasks.Add(factory.StartNew
                    (() =>
                    {
                        // 清理内容
                        content = content.Trim();
                        // 检查结果
                        if (string.IsNullOrEmpty(content)) return;
                        // 检查结果
                        if (!ChineseTool.IsChinese(content)) return;
#if _USE_CLR
                        CoreSegment segment
#elif _USE_CONSOLE
                        CoreSegment? segment
#endif
                            // 检查是否重复
                            = CoreCache.LoadSegment(content);
                        // 检查结果
                        if (segment == null)
                        {
                            // 创建对象
                            segment = new CoreSegment();
                            // 设置参数
                            segment.Index = 0;
                            segment.Count = count;
                            segment.Gamma = -1.0f;
                            segment.Value = content;
                            segment.Length = content.Length;
                            // 加入数据
                            CoreCache.AddSegment(segment);

                            // 检查主要属性
                            if(!string.IsNullOrEmpty(mainClass))
                            {
                                // 生成对象
                                MarkAttribute mark
                                    = new MarkAttribute();
                                // 设置参数
                                mark.Index = 0;
                                mark.Value = mainClass;
                                mark.Remark = subClass;
                                // 增加属性
                                MarkCache.AddMark(content, mark);
                            }
                        }
                    }));

                    // 检查任务数量
                    if (tasks.Count >= 10000) { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("CoreTool", "CopyDictionary",
                    string.Format("{0} items processed !", total));
                // 记录日志
                LogTool.LogMessage("CoreTool", "CopyDictionary", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("CoreTool", "CopyDictionary", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("CoreTool", "CopyDictionary", "任务全部结束！");

                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("CoreTool", "CopyDictionary", "数据库链接已关闭！");
            }

            // 打印记录
            LogTool.LogMessage("\ttotal = " + total);
            // 记录日志
            LogTool.LogMessage("CoreTool", "CopyDictionary", "数据统计完毕！");
        }

        public static void GenerateCore()
        {
            // SQL指令
            string cmdString;

            // 输出日志操作
            OperationLog.ExportFile("operation.dat");

            // 创建数据表
            CoreContent.CreateTable();
            // 创建数据表
            MarkContent.CreateTable();

            /*
            // 拷贝基础内容
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("开始拷贝外文名！");
            Console.WriteLine("----------------------------------------");
            // 指令字符串
            cmdString =
                "SELECT [content], [count] FROM [dbo].[DictionaryContent] " +
                "   WHERE [source] IN ('英文名') " +
                "   AND [count] > 10 AND [length] >= 2 AND [length] <= 7;";
            CopyDictionary(cmdString, WordType.NOUN, "英文名");

            // 指令字符串
            cmdString =
                "SELECT [content], [count] FROM [dbo].[DictionaryContent] " +
                "   WHERE [source] IN ('日文名') " +
                "   AND [count] > 10 AND [length] >= 2 AND [length] <= 6;";
            CopyDictionary(cmdString, WordType.NOUN, "日文名");
            */

            /*
            // 拷贝基础内容
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("开始拷贝中文名！");
            Console.WriteLine("----------------------------------------");
            // 指令字符串
            cmdString =
                "SELECT [content], [count] FROM [dbo].[DictionaryContent] " +
                "   WHERE [source] IN ('姓名', '名人') AND [count] > 10 AND [length] > 1 AND [length] <= 4 AND " +
                "   ( " +
                "       SUBSTRING([content], 1, 1) IN " +
                "       (SELECT [content] FROM [dbo].[DictionaryContent] WHERE [source] = '姓氏') " +
                "       OR " +
                "       SUBSTRING([content], 1, 2) IN " +
                "       (SELECT [content] FROM [dbo].[DictionaryContent] WHERE [source] = '姓氏' AND [length] >= 2) " +
                "   );";
            CopyDictionary(cmdString, WordType.NOUN, "中文名");
            */

            // 拷贝基础内容
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("开始拷贝新华字典！");
            Console.WriteLine("----------------------------------------");
            // 指令字符串
            cmdString =
                "SELECT [content], [count] FROM [dbo].[DictionaryContent] " +
                "   WHERE [source] = '新华字典' AND [count] > 10 AND [length] >= 3 AND [length] <= 4;";
            CopyDictionary(cmdString);

            // 拷贝基础内容
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("开始拷贝成语词典！");
            Console.WriteLine("----------------------------------------");
            // 指令字符串
            cmdString =
                "SELECT [content], [count] FROM [dbo].[DictionaryContent] " +
                "   WHERE [source] = '成语' AND [count] > 10 AND [length] = 4;";
            CopyDictionary(cmdString, WordType.IDIOM, "成语");

            // 拷贝基础内容
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("开始拷贝现代汉语词典！");
            Console.WriteLine("----------------------------------------");
            // 指令字符串
            cmdString =
                "SELECT [content], [count] FROM [dbo].[DictionaryContent] " +
                "   WHERE [source] = '现代汉语词典' AND [count] > 10 AND [length] > 1;";
            CopyDictionary(cmdString);

            // 拷贝基础内容
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("开始拷贝内置数据！");
            Console.WriteLine("----------------------------------------");
            CopyInternal();

            // 重新计算Gamma数值
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("开始计算Gamma数值！");
            Console.WriteLine("----------------------------------------");
            // 开始统计
            GammaTool.MakeStatistic();

            // 删除异常数值
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("开始删除异常数值！");
            Console.WriteLine("----------------------------------------");
            Clear();

            // 划分词汇隔离带
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("开始已经录制的操作！");
            Console.WriteLine("----------------------------------------");
            DoItAgain();

            // 保存数据
            TokenCache.SaveTokens();
            ShortCache.SaveShorts();
            CoreCache.SaveSegments();
            DictionaryCache.SaveEntries();
            MarkCache.SaveMarks();

            // 删除重复属性
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("开始去除重复的属性内容！");
            Console.WriteLine("----------------------------------------");
            MarkContent.DeleteDuplicate();

            // 拷贝基础内容
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("已经按下录制键！");
            Console.WriteLine("----------------------------------------");
            // 检查数据表
            OperationLog.CreateTable();
            // 重新输入记录
            OperationLog.ImportFile("operation.dat");

            // 拷贝基础内容
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("需要重新运行Gamma计算工具！");
            Console.WriteLine("----------------------------------------");
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备生成CoreContent，原表及其数据将被更改！");
            Console.WriteLine("确认是否执行（Yes/No）？");
            // 检查输入行
            if (!ConsoleTool.Confirm()) return;

            // 检查数据表
            if (!DBTool.TableExists("DictionaryContent"))
            {
                // 记录日志
                Console.WriteLine("数据表DictionaryContent不存在！"); return;
            }

            // 开启日志
            LogTool.SetLog(true);
            // 创建计时器
            Stopwatch watch = new Stopwatch();
            // 开启计时器
            watch.Start();
            // 生成数据表
            GenerateCore();
            // 关闭计时器
            watch.Stop();
            // 打印结果
            Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
        }
#endif
    }
}
