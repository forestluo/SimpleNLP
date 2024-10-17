using NLP;
using System.IO;
using System.Xml;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LTP
{
    public class FileImporter
    {
        public static void ImportLTP(string fileName)
        {
            // 记录日志
            LogTool.LogMessage("FileImporter", "ImportLTP", "开始输入数据！");

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
                LogTool.LogMessage("FileImporter", "ImportLTP", "文件流已开启！");

                // 循环处理
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    // 增加计数
                    total++;
                    // 检查计数
                    if (total % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("FileImporter", "ImportLTP",
                            string.Format("{0} items processed !", total));
                    }

                    // 读取value
                    string value = br.ReadString();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 创建属性
                    string[] attributes = new string[7];

                    // 读取iid
                    attributes[0] = br.ReadInt32().ToString();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取content
                    attributes[1] = br.ReadString();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取pos
                    attributes[2] = br.ReadString();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取ne
                    attributes[3] = br.ReadString();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取parent
                    attributes[4] = br.ReadInt32().ToString();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取relate
                    attributes[5] = br.ReadString();
                    // 检查结果
                    if (br.BaseStream.Position >= br.BaseStream.Length) continue;

                    // 读取arg
                    attributes[6] = br.ReadString();

                    // 检查结果
                    if (string.IsNullOrEmpty(value)) continue;
                    // 检查结果
                    if (string.IsNullOrEmpty(attributes[1])) continue;
                    // 获得sid
                    int sid = ShortContent.GetID(value);
                    // 检查结果
                    if (sid > 0)
                    {
                        // 启动进程
                        tasks.Add(factory.StartNew(() =>
                            LTPContent.AddContent(sid, attributes)));
                    }

                    // 检查任务数量
                    if (tasks.Count >= 10000) { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                }
                // 打印记录
                LogTool.LogMessage("FileImporter", "ImportLTP",
                    string.Format("{0} items processed !", total));

                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("FileImporter", "ImportLTP", "等待任务结束！");

                // 关闭文件流
                br.Close(); fs.Close();
                // 记录日志
                LogTool.LogMessage("FileImporter", "ImportLTP", "文件流已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("FileImporter", "ImportLTP", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("FileImporter", "ImportLTP", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\ttotal = " + total);
            // 记录日志
            LogTool.LogMessage("FileImporter", "ImportLTP", "数据输入完毕！");
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备导入数据，原表及其数据将被删除！");
            Console.WriteLine("确认是否执行（Yes/No）？");

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
            if (line.Equals("Yes"))
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

                // 开启日志
                LogTool.SetLog(true);
                // 创建计时器
                Stopwatch watch = new Stopwatch();
                // 开启计时器
                watch.Start();
                // 检查数据表
                if (!DBTool.TableExists("ShortContent"))
                {
                    Console.WriteLine("数据表ShortContent不存在！");
                }
                else
                {
                    // 创建数据表
                    LTPContent.CreateTable();
                    // 打印日志
                    Console.WriteLine("数据表LTPContent已创建！");
                    // 导入数据
                    ImportLTP(fileName);
                }
                // 关闭计时器
                watch.Stop();
                // 打印结果
                Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
            }
        }
#endif
    }
}
