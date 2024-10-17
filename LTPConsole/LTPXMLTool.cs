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
    public class LTPXMLTool
    {
        /*
        <?xml version="1.0" encoding="utf-8" ?>
        <xml4nlp>
            <note sent="y" word="y" pos="y" ne="y" parser="y" wsd="n" srl="y" />
            <doc>
                <para id="0">
                    <sent id="0" cont="北京市精神疾病会诊中心于">
                        <word id="0" cont="北京市" pos="ns" ne="B-Ni" parent="4" relate="ATT" />
                        <word id="1" cont="精神" pos="n" ne="I-Ni" parent="2" relate="ATT" />
                        <word id="2" cont="疾病" pos="n" ne="I-Ni" parent="3" relate="ATT" />
                        <word id="3" cont="会诊" pos="v" ne="I-Ni" parent="4" relate="ATT">
                            <arg id="0" type="" beg="1" end="2" />
                        </word>
                        <word id="4" cont="中心" pos="n" ne="E-Ni" parent="-1" relate="HED" />
                        <word id="5" cont="于" pos="p" ne="O" parent="4" relate="CMP" />
                    </sent>
                </para>
            </doc>
        </xml4nlp>
        */
        public static void ImportXML(string fileName)
        {
            // 记录日志
            LogTool.LogMessage("LTPXMLTool", "ImportXML", "开始输入数据！");

            // 计数器
            int total = 0;
            // 任务数组
            List<Task> tasks = new List<Task>();
            // 生成任务控制器
            TaskFactory factory = new TaskFactory(
                new LimitedConcurrencyLevelTaskScheduler(DBTool.MAX_THREADS));

            try
            {
                // 开启文件
                StreamReader sr = new StreamReader(fileName);
                // 记录日志
                LogTool.LogMessage("LTPXMLTool", "ImportXML", "文件流已经开启！");

                string line;
                // 循环处理
                while ((line = sr.ReadLine()!) != null)
                {
                    // 检查开头
                    if (line.StartsWith("<?xml"))
                    {
                        // 创建对象
                        StringBuilder sb = new StringBuilder();
                        // 循环处理
                        while ((line = sr.ReadLine()!) != null)
                        {
                            sb.Append(line);
                            // 检查结束
                            if (line.StartsWith("</xml4nlp>"))
                            {
                                // 创建XML文档
                                XmlDocument document = new XmlDocument();
                                // 加载内容
                                document.LoadXml(sb.ToString()); sb.Clear();
                                // 选择节点
                                XmlElement element = (XmlElement)document.
                                    SelectSingleNode("xml4nlp/doc/para[@id=0]/sent[@id=0]")!;
                                // 检查结果
                                if (element == null) continue;

                                // 获得内容
                                string value = element.GetAttribute("cont");
                                // 检查结果
                                if (string.IsNullOrEmpty(value)) continue;

                                // 获得sid
                                int sid = ShortContent.GetID(value);
                                // 检查结果
                                if (sid <= 0)
                                {
                                    // 打印记录
                                    LogTool.LogMessage("LTPXMLTool", "ImportXML",
                                        string.Format("无效内容({0})", value));
                                    continue;
                                }
                                // 记录日志
                                //LogTool.LogMessage("FileImporter", "ImportXML", value);

                                // 增加计数
                                total++;
                                // 检查计数
                                if (total % 10000 == 0)
                                {
                                    // 打印记录
                                    LogTool.LogMessage("LTPXMLTool", "ImportXML",
                                        string.Format("{0} items processed !", total));
                                }

                                // 循环处理所有子节点
                                foreach (XmlElement subnode
                                    in element.GetElementsByTagName("word"))
                                {
                                    // 创建对象
                                    string[] attributes = new string[7];
                                    // 获得id
                                    attributes[0] = subnode.GetAttribute("id");
                                    // 检查结果
                                    if (string.IsNullOrEmpty(attributes[0])) continue;
                                    // 获得content
                                    attributes[1] = subnode.GetAttribute("cont");
                                    // 检查结果
                                    if (string.IsNullOrEmpty(attributes[1])) continue;
                                    // 获得pos
                                    attributes[2] = subnode.GetAttribute("pos");
                                    // 检查结果
                                    if (string.IsNullOrEmpty(attributes[2])) continue;
                                    // 获得ne
                                    attributes[3] = subnode.GetAttribute("ne");
                                    // 检查结果
                                    if (string.IsNullOrEmpty(attributes[3])) continue;
                                    // 获得parent
                                    attributes[4] = subnode.GetAttribute("parent");
                                    // 检查结果
                                    if (string.IsNullOrEmpty(attributes[4])) continue;
                                    // 获得relate
                                    attributes[5] = subnode.GetAttribute("relate");
                                    // 检查结果
                                    if (string.IsNullOrEmpty(attributes[5])) continue;

                                    // 检查arg
                                    foreach (XmlElement arg
                                        in subnode.GetElementsByTagName("arg"))
                                    {
                                        sb.Append('|');
                                        sb.Append(arg.GetAttribute("beg"));
                                        sb.Append('-'); sb.Append(arg.GetAttribute("end"));
                                    }
                                    // 获得arg
                                    if (sb.Length > 0) attributes[6] = sb.ToString().Substring(1); sb.Clear();

                                    // 启动进程
                                    tasks.Add(factory.StartNew(() =>
                                        LTPContent.AddContent(sid, attributes)));
                                    /*
                                    // 记录日志
                                    LogTool.LogMessage(string.Format(
                                        "\t\"{0}\" ({1}, {2}, {3}, {4}, {5}, {6})", kvp.Value[1], kvp.Value[0],
                                        kvp.Value[2], kvp.Value[3], kvp.Value[4], kvp.Value[5], kvp.Value[6]));
                                    */
                                }

                                // 检查任务数量
                                if (tasks.Count >= 10000) { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                            }
                        }
                    }
                }
                // 打印记录
                LogTool.LogMessage("LTPXMLTool", "ImportXML",
                    string.Format("{0} items processed !", total));

                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("LTPXMLTool", "ImportXML", "等待任务结束！");

                // 关闭阅读器
                sr.Close();
                // 记录日志
                LogTool.LogMessage("LTPXMLTool", "ImportXML", "文件流已经关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("LTPXMLTool", "ImportXML", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查任务数量
                if (tasks.Count > 0)
                { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
                // 记录日志
                LogTool.LogMessage("LTPXMLTool", "ImportXML", "任务全部结束！");
            }

            // 记录日志
            LogTool.LogMessage("\ttotal = " + total);
            // 记录日志
            LogTool.LogMessage("LTPXMLTool", "ImportXML", "数据输入完毕！");
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备导入XML数据，原表及其数据将被删除！");
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
                LTPContent.CreateTable();
                // 导入数据
                ImportXML(fileName);
                // 关闭计时器
                watch.Stop();
                // 打印结果
                Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
            }
        }
    }
#endif   
}
