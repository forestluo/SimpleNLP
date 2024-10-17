using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLP
{
    public class ConsoleTool
    {
#if _USE_CONSOLE
        public static bool Confirm()
        {
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
                        Console.WriteLine("开始执行操作！"); break;
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
            return line.Equals("Yes");
        }
#endif

        public static bool Execute(string line)
        {
            bool resultFlag = false;
            // 拆分成参数
            string[] args = line.Split(' ');
            // 检查参数
            if (args == null)
            {
#if _USE_CONSOLE
                Console.WriteLine("请输入正确的参数！");
#endif
            }
            else if (args.Length == 1)
            {
                if (args[0].Equals("reload"))
                {
                    resultFlag = true;
                    // 重新加载
                    CoreCache.LoadSegments();
#if _USE_CONSOLE
                    // 打印信息
                    Console.WriteLine("----------------------------------------");
                    Console.WriteLine("内容已重新加载！");
#endif
                }
            }
            else if (args.Length == 2)
            {
                if (args[0].Equals("del") && args[1].Length > 1)
                {
                    resultFlag = true;
                    // 删除属性
                    MarkCache.DeleteMarks(args[1]);
                    // 删除内容
                    CoreCache.DeleteSegment(args[1]);

                    // 更新数据
                    GammaTool.MakeStatistic(args[1]);
#if _USE_CONSOLE
                    // 打印信息
                    Console.WriteLine(string.Format("内容({0})已删除！", args[1]));
#endif
                }
                else if (args[0].Equals("add") && args[1].Length > 1)
                {
                    resultFlag = true;
                    // 获得计数
                    int count = CounterTool.GetCount(args[1], true);
                    // 增加内容
                    CoreCache.AddSegment(args[1], count < 0 ? 1 : count);
                    // 更新数据
                    GammaTool.MakeStatistic(args[1]);
#if _USE_CONSOLE
                    // 打印信息
                    Console.WriteLine(string.Format("内容({0})[{1}]已增加！", args[1], count));
#endif
                }
                else if (args[0].Equals("fsp") && args[1].Length > 1)
                {
                    resultFlag = true;
                    // 获得字符串
                    string[] words = args[1].Split('|');
                    // 检查字符
                    if (!words.Any(w => CounterTool.GetCount(w, false) <= 0))
                    {
                        // 删除分隔符即可
                        string value = args[1].Replace("|", "");
                        // 检查长度
                        if (value.Length > 2)
                        {
                            // 增加内容
                            // 获得计数
                            int count = CounterTool.GetCount(value, true);
                            // 增加内容
                            CoreCache.AddSegment(value, count < 0 ? 1 : count);
                            // 检查结果
                            if (value.Length < args[1].Length)
                            {
                                // 设置强制路径
                                CoreCache.SetPath(value, args[1]);
                            }
                            // 更新数据
                            GammaTool.MakeStatistic(value);
#if _USE_CONSOLE
                            // 打印信息
                            Console.WriteLine(string.Format("路径({0})[{1}]已设定！", args[1], count));
#endif
                        }
                        else
                        {
#if _USE_CONSOLE
                            // 打印信息
                            Console.WriteLine(string.Format("内容太短！"));
                            Console.WriteLine(string.Format("无法设定路径({0})！", args[1]));
#endif
                        }
                    }
                    else
                    {
#if _USE_CONSOLE
                        // 打印信息
                        Console.WriteLine(string.Format("内容不存在！"));
                        Console.WriteLine(string.Format("无法设定路径({0})！", args[1]));
#endif
                    }
                }
            }
            else if (args.Length == 4 || args.Length == 5)
            {
                if (args[0].Equals("attr") && WordType.IsMajor(args[3]))
                {
                    // 创建对象
                    MarkAttribute mark = new MarkAttribute();
                    // 设置参数
                    mark.Index = 0;
                    mark.Value = args[3];
                    // 检查长度
                    if (args.Length == 5)
                    {
                        if (SingleWord.IsMinor(args[4]))
                        {
                            mark.Remark = args[4];
                        }
                        else
                        {
#if _USE_CONSOLE
                            Console.WriteLine("Exec : invalid arguments !");
#endif
                        }
                    }
                    // 检查参数
                    if (args[1].Equals("-add"))
                    {
                        resultFlag = true;
                        // 增加内容
                        MarkCache.AddMark(args[2], mark);
                    }
                    else if (args[1].Equals("-del"))
                    {
                        resultFlag = true;
                        // 删除内容
                        MarkCache.DeleteMark(args[2], mark);
                    }
                    else
                    {
#if _USE_CONSOLE
                        Console.WriteLine("Exec : invalid operation !");
#endif
                    }
                }
            }
            // 返回结果
            return resultFlag;
        }

#if _USE_CONSOLE
        public static void CreateTable(string[] args)
        {
            // 检查参数
            if (args.Length != 2)
            {
                Console.WriteLine("Usage : NLPConsole -create [arguments]"); return;
            }
            // 检查参数
            if (!args[0].Equals("-create"))
            {
                Console.WriteLine("Main : invalid options !"); return;
            }

            Console.WriteLine("准备生成数据表，原表及其数据将被删除！");
            Console.WriteLine("确认是否执行（Yes/No）？");
            // 检查确认
            if (!Confirm()) return;

            // 开启日志
            LogTool.SetLog(true);
            // 创建计时器
            Stopwatch watch = new Stopwatch();
            // 开启计时器
            watch.Start();
            // 检查参数
            if (args[1].Equals("exception")) ExceptionLog.Main(args);
            else if (args[1].Equals("operation")) OperationLog.Main(args);
            else if (args[1].Equals("raw")) RawContent.Main(args);
            else if (args[1].Equals("long")) LongContent.Main(args);
            else if (args[1].Equals("core")) CoreContent.Main(args);
            else if (args[1].Equals("mark")) MarkContent.Main(args);
            else if (args[1].Equals("token")) TokenContent.Main(args);
            else if (args[1].Equals("short")) ShortContent.Main(args);
            else if (args[1].Equals("counter")) CounterContent.Main(args);
            else if (args[1].Equals("filtered")) FilteredContent.Main(args);
            else if (args[1].Equals("attribute")) AttributeContent.Main(args);
            else if (args[1].Equals("dictionary")) DictionaryContent.Main(args);
            else
            {
                Console.WriteLine("Main : invalid arguments !");
            }
            // 关闭计时器
            watch.Stop();
            // 打印结果
            Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
        }
#endif

#if _USE_CONSOLE
        public static void GenerateTable(string[] args)
        {
            // 检查参数
            if (args.Length != 2)
            {
                Console.WriteLine("Usage : NLPConsole -generate [arguments]"); return;
            }
            // 检查参数
            if (!args[0].Equals("-generate"))
            {
                Console.WriteLine("Main : invalid options !"); return;
            }

            Console.WriteLine("准备生成数据表，原表及其数据将被删除！");
            Console.WriteLine("确认是否执行（Yes/No）？");
            // 检查确认
            if (!Confirm()) return;

            // 开启日志
            LogTool.SetLog(true);
            // 创建计时器
            Stopwatch watch = new Stopwatch();
            // 开启计时器
            watch.Start();
            // 检查参数
            if (args[1].Equals("all"))
            {
                // 创建数据表
                FilteredContent.CreateTable();
                // 过滤数据
                FilteredCache.FilterContents();
                // 卸载数据
                FilteredCache.ClearContents();

                // 加载数据
                FilteredCache.LoadContents();
                // 加载数据
                DictionaryCache.LoadEntries();

                // 创建数据表
                TokenContent.CreateTable();
                // 开始统计Token
                TokenCache.MakeStatistic();
                // 更新Token计数
                TokenCache.SaveTokens();
                // 卸载数据
                TokenCache.ClearTokens();

                // 创建数据表
                LongContent.CreateTable();
                // 提取内容
                LongContent.ExtractLongs();

                // 创建数据表
                ShortContent.CreateTable();
                // 提取内容
                ShortCache.ExtractShorts();
                // 保存数据
                ShortCache.SaveShorts();
                // 卸载数据
                ShortCache.ClearShorts();

                // 创建表
                CounterContent.CreateTable();
                // 开始执行循环
                CounterCache.MakeStatistic();

                // 清理数据
                FilteredCache.ClearContents();
                // 清理数据
                DictionaryCache.ClearEntries();
            }
            else if (args[1].Equals("core"))
            {
                // 生成核心数据
                CoreTool.GenerateCore();
            }
            else if (args[1].Equals("long"))
            {
                // 加载数据
                FilteredCache.LoadContents();

                // 创建数据表
                LongContent.CreateTable();
                // 提取内容
                LongContent.ExtractLongs();

                // 卸载数据
                FilteredCache.ClearContents();
            }
            else if (args[1].Equals("token"))
            {
                // 加载数据
                FilteredCache.LoadContents();

                // 创建数据表
                TokenContent.CreateTable();
                // 开始统计Token
                TokenCache.MakeStatistic();
                // 更新Token计数
                TokenCache.SaveTokens();
                // 卸载数据
                TokenCache.ClearTokens();

                // 卸载数据
                FilteredCache.ClearContents();
            }
            else if (args[1].Equals("short"))
            {
                // 加载数据
                FilteredCache.LoadContents();

                // 创建数据表
                ShortContent.CreateTable();
                // 提取内容
                ShortCache.ExtractShorts();
                // 保存数据
                ShortCache.SaveShorts();
                // 卸载数据
                ShortCache.ClearShorts();

                // 卸载数据
                FilteredCache.ClearContents();
            }
            else if (args[1].Equals("counter"))
            {
                // 加载数据
                FilteredCache.LoadContents();
                // 加载数据
                DictionaryCache.LoadEntries();

                // 创建表
                CounterContent.CreateTable();
                // 开始执行循环
                CounterCache.MakeStatistic();
                
                // 清理数据
                FilteredCache.ClearContents();
                // 清理数据
                DictionaryCache.ClearEntries();
            }
            else if (args[1].Equals("filtered"))
            {
                // 创建数据表
                FilteredContent.CreateTable();
                // 过滤数据
                FilteredCache.FilterContents();
                // 卸载数据
                FilteredCache.ClearContents();
            }
            else if (args[1].Equals("attribute")) AttributeCache.Main(args);
            else
            {
                Console.WriteLine("Main : invalid arguments !");
            }
            // 关闭计时器
            watch.Stop();
            // 打印结果
            Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
        }
#endif

#if _USE_CONSOLE
        public static void ImportFile(string[] args)
        {
            // 检查参数
            if (args.Length != 2)
            {
                Console.WriteLine("Usage : NLPConsole -import [arguments]"); return;
            }
            // 检查参数
            if (!args[0].Equals("-import"))
            {
                Console.WriteLine("Main : invalid options !"); return;
            }

            Console.WriteLine("准备导入数据，原表及其数据将被删除！");
            Console.WriteLine("确认是否执行（Yes/No）？");
            // 检查确认
            if (!Confirm()) return;

            // 开启日志
            LogTool.SetLog(true);
            // 创建计时器
            Stopwatch watch = new Stopwatch();
            // 开启计时器
            watch.Start();
            // 检查参数
            if (args[1].Equals("raw"))
            {
                // 创建数据表
                RawContent.CreateTable();
                // 导入数据
                RawContent.ImportFile("raw.dat");
            }
            else if (args[1].Equals("core"))
            {
                // 创建数据表
                CoreContent.CreateTable();
                // 导入数据
                CoreContent.ImportFile("core.dat");
            }
            else if (args[1].Equals("mark"))
            {
                // 创建数据表
                MarkContent.CreateTable();
                // 导入数据
                MarkContent.ImportFile("mark.dat");
            }
            else if (args[1].Equals("token"))
            {
                // 创建数据表
                TokenContent.CreateTable();
                // 导入数据
                TokenContent.ImportFile("token.dat");
            }
            else if (args[1].Equals("long"))
            {
                // 创建数据表
                LongContent.CreateTable();
                // 导入数据
                LongContent.ImportFile("long.dat");
            }
            else if (args[1].Equals("short"))
            {
                // 创建数据表
                ShortContent.CreateTable();
                // 导入数据
                ShortContent.ImportFile("short.dat");
            }
            else if (args[1].Equals("counter"))
            {
                // 创建数据表
                CounterContent.CreateTable();
                // 导入数据
                CounterContent.ImportFile("counter.dat");
            }
            else if (args[1].Equals("opeartion"))
            {
                // 创建数据表
                OperationLog.CreateTable();
                // 导入数据
                OperationLog.ImportFile("operation.dat");
            }
            else if (args[1].Equals("filtered"))
            {
                // 创建数据表
                FilteredContent.CreateTable();
                // 导入数据
                FilteredContent.ImportFile("filtered.dat");
            }
            else if (args[1].Equals("dictionary"))
            {
                // 创建数据表
                DictionaryContent.CreateTable();
                // 导入数据
                DictionaryContent.ImportFile("dictionary.dat");
            }
            else if (args[1].Equals("all"))
            {
                // 创建数据表
                RawContent.CreateTable();
                // 导入数据
                RawContent.ImportFile("raw.dat");

                // 创建数据表
                CoreContent.CreateTable();
                // 导入数据
                CoreContent.ImportFile("core.dat");

                // 创建数据表
                MarkContent.CreateTable();
                // 导入数据
                MarkContent.ImportFile("mark.dat");

                // 创建数据表
                TokenContent.CreateTable();
                // 导入数据
                TokenContent.ImportFile("token.dat");

                // 创建数据表
                ShortContent.CreateTable();
                // 导入数据
                ShortContent.ImportFile("short.dat");

                // 创建数据表
                LongContent.CreateTable();
                // 导入数据
                LongContent.ImportFile("long.dat");

                // 创建数据表
                CounterContent.CreateTable();
                // 导入数据
                CounterContent.ImportFile("counter.dat");

                // 创建数据表
                FilteredContent.CreateTable();
                // 导入数据
                FilteredContent.ImportFile("filtered.dat");

                // 创建数据表
                DictionaryContent.CreateTable();
                // 导入数据
                DictionaryContent.ImportFile("dictionary.dat");
            }
            else
            {
                Console.WriteLine("Main : invalid arguments !");
            }
            // 关闭计时器
            watch.Stop();
            // 打印结果
            Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
        }
#endif

#if _USE_CONSOLE
        public static void ExportFile(string[] args)
        {
            // 检查参数
            if (args.Length != 2)
            {
                Console.WriteLine("Usage : NLPConsole -export [arguments]"); return;
            }
            // 检查参数
            if (!args[0].Equals("-export"))
            {
                Console.WriteLine("Main : invalid options !"); return;
            }

            Console.WriteLine("准备导出数据，原数据文件将被删除！");
            Console.WriteLine("确认是否执行（Yes/No）？");
            // 检查确认
            if (!Confirm()) return;

            // 开启日志
            LogTool.SetLog(true);
            // 创建计时器
            Stopwatch watch = new Stopwatch();
            // 开启计时器
            watch.Start();
            // 检查参数
            if (args[1].Equals("raw"))
            {
                if (DBTool.TableExists("RawContent"))
                {
                    RawContent.ExportFile("raw.dat");
                }
                else
                {
                    Console.WriteLine("数据表RawContent不存在！");
                }
            }
            else if (args[1].Equals("core"))
            {
                if (DBTool.TableExists("CoreContent"))
                {
                    CoreContent.ExportFile("core.dat");
                }
                else
                {
                    Console.WriteLine("数据表CoreContent不存在！");
                }
            }
            else if (args[1].Equals("mark"))
            {
                if (DBTool.TableExists("MarkContent"))
                {
                    MarkContent.ExportFile("mark.dat");
                }
                else
                {
                    Console.WriteLine("数据表MarkContent不存在！");
                }
            }
            else if (args[1].Equals("token"))
            {
                if (DBTool.TableExists("TokenContent"))
                {
                    TokenContent.ExportFile("token.dat");
                }
                else
                {
                    Console.WriteLine("数据表TokenContent不存在！");
                }
            }
            else if (args[1].Equals("long"))
            {
                if (DBTool.TableExists("LongContent"))
                {
                    LongContent.ExportFile("long.dat");
                }
                else
                {
                    Console.WriteLine("数据表LongContent不存在！");
                }
            }
            else if (args[1].Equals("short"))
            {
                if (DBTool.TableExists("ShortContent"))
                {
                    ShortContent.ExportFile("short.dat");
                }
                else
                {
                    Console.WriteLine("数据表ShortContent不存在！");
                }
            }
            else if (args[1].Equals("counter"))
            {
                if (DBTool.TableExists("CounterContent"))
                {
                    CounterContent.ExportFile("counter.dat");
                }
                else
                {
                    Console.WriteLine("数据表CounterContent不存在！");
                }
            }
            else if (args[1].Equals("filtered"))
            {
                if (DBTool.TableExists("FilteredContent"))
                {
                    FilteredContent.ExportFile("filtered.dat");
                }
                else
                {
                    Console.WriteLine("数据表FilteredContent不存在！");
                }
            }
            else if (args[1].Equals("operation"))
            {
                if (DBTool.TableExists("OperationLog"))
                {
                    OperationLog.ExportFile("operation.dat");
                }
                else
                {
                    Console.WriteLine("数据表OperationLog不存在！");
                }
            }
            else if (args[1].Equals("dictionary"))
            {
                if (DBTool.TableExists("DictionaryContent"))
                {
                    OperationLog.ExportFile("dictionary.dat");
                }
                else
                {
                    Console.WriteLine("数据表DictionaryContent不存在！");
                }
            }
            else if (args[1].Equals("all"))
            {
                if (DBTool.TableExists("RawContent"))
                {
                    RawContent.ExportFile("raw.dat");
                }
                else
                {
                    Console.WriteLine("数据表RawContent不存在！");
                }

                if (DBTool.TableExists("CoreContent"))
                {
                    CoreContent.ExportFile("core.dat");
                }
                else
                {
                    Console.WriteLine("数据表CoreContent不存在！");
                }

                if (DBTool.TableExists("MarkContent"))
                {
                    MarkContent.ExportFile("mark.dat");
                }
                else
                {
                    Console.WriteLine("数据表MarkContent不存在！");
                }

                if (DBTool.TableExists("LongContent"))
                {
                    LongContent.ExportFile("long.dat");
                }
                else
                {
                    Console.WriteLine("数据表LongContent不存在！");
                }

                if (DBTool.TableExists("TokenContent"))
                {
                    TokenContent.ExportFile("token.dat");
                }
                else
                {
                    Console.WriteLine("数据表TokenContent不存在！");
                }

                if (DBTool.TableExists("ShortContent"))
                {
                    ShortContent.ExportFile("short.dat");
                }
                else
                {
                    Console.WriteLine("数据表ShortContent不存在！");
                }

                if (DBTool.TableExists("CounterContent"))
                {
                    CounterContent.ExportFile("counter.dat");
                }
                else
                {
                    Console.WriteLine("数据表CounterContent不存在！");
                }

                if (DBTool.TableExists("FilteredContent"))
                {
                    FilteredContent.ExportFile("filtered.dat");
                }
                else
                {
                    Console.WriteLine("数据表FilteredContent不存在！");
                }

                if (DBTool.TableExists("DictionaryContent"))
                {
                    DictionaryContent.ExportFile("dictionary.dat");
                }
                else
                {
                    Console.WriteLine("数据表DictionaryContent不存在！");
                }
            }
            else
            {
                Console.WriteLine("Main : invalid arguments !");
            }
            // 关闭计时器
            watch.Stop();
            // 打印结果
            Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
        }
#endif
    }
}
