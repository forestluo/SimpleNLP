using NLP;
using System.IO;

namespace LTP
{
    public class LTPCounter
    {
        public static void Main(string[] args)
        {
            // 获得文件名
            string fileName = args[1];
            // 检查参数
            if(string.IsNullOrEmpty(fileName))
            {
                Console.WriteLine("无效的参数！"); return;
            }
            // 打印文件名
            Console.WriteLine(string.Format("准备开启{0}！",fileName));

            try
            {
                // 计数器
                int[] counters = new int[12];
                // 创建文件流
                StreamReader sr = new StreamReader(fileName);
                string line;
                // 循环处理
                while ((line = sr.ReadLine()!) != null)
                {
                    // 剪裁
                    line = line.Trim(); counters[0]++;
                    // 检查结果
                    if(counters[0] % 10000 == 0)
                    {
                        // 打印记录
                        LogTool.LogMessage("LTPCounter", "Main",
                            string.Format("{0} items processed !", counters[0]));
                    }

                    // 检查开头
                    if (line.StartsWith("<?xml")) counters[1]++;
                    else if(line.StartsWith("<xml4nlp>")) counters[2]++;
                    else if (line.StartsWith("<note")) counters[3]++;
                    else if (line.StartsWith("<doc>")) counters[4]++;
                    else if (line.StartsWith("<para")) counters[5]++;
                    else if (line.StartsWith("<sent")) counters[6]++;
                    else if (line.StartsWith("<word")) counters[7]++;
                    else if (line.StartsWith("</sent>")) counters[8]++;
                    else if (line.StartsWith("</para>")) counters[9]++;
                    else if (line.StartsWith("</doc>")) counters[10]++;
                    else if (line.StartsWith("</xml4nlp>")) counters[11]++;
                }
                // 关闭文件流
                sr.Close();
                // 打印记录
                LogTool.LogMessage("LTPCounter", "Main",
                    string.Format("{0} items processed !", counters[0]));

                // 打印结果
                Console.WriteLine(string.Format("total = {0}", counters[0]));
                Console.WriteLine(string.Format("\"<?xml\" ({0})", counters[1]));
                Console.WriteLine(string.Format("\"<xml4nlp>\" ({0})", counters[2]));
                Console.WriteLine(string.Format("\"<note\" ({0})", counters[3]));
                Console.WriteLine(string.Format("\"<doc>\" ({0})", counters[4]));
                Console.WriteLine(string.Format("\"<para\" ({0})", counters[5]));
                Console.WriteLine(string.Format("\"<sent\" ({0})", counters[6]));
                Console.WriteLine(string.Format("\"<word\" ({0})", counters[7]));
                Console.WriteLine(string.Format("\"</sent>\" ({0})", counters[8]));
                Console.WriteLine(string.Format("\"</para>\" ({0})", counters[9]));
                Console.WriteLine(string.Format("\"</doc>\" ({0})", counters[10]));
                Console.WriteLine(string.Format("\"</xml4nlp>\" ({0})", counters[11]));
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("LTPCounter", "Main", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
        }
    }
}
