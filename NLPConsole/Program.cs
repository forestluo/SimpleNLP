// See https://aka.ms/new-console-template for more information

using NLP;

public class Program
{
    static void Main(string[] args)
    {
        // 检查数据表
        if (!DBTool.TableExists("ExceptionLog")) ExceptionLog.CreateTable();

        // 检查参数
        if (args.Length >= 2 && args[0].Equals("-scan")) Grammar.Main(args);
        // 检查参数
        else if (args.Length == 2)
        {
            // 检查参数
            if (args[0].Equals("-r2s")) SentenceExtractor.Main(args);
            else if (args[0].Equals("-s2c")) CoreSegment.Main(args);
            else if (args[0].Equals("-f2q")) QuantityExtractor.Main(args);
            else if (args[0].Equals("-create")) ConsoleTool.CreateTable(args);
            else if (args[0].Equals("-import")) ConsoleTool.ImportFile(args);
            else if (args[0].Equals("-export")) ConsoleTool.ExportFile(args);
            else if (args[0].Equals("-generate")) ConsoleTool.GenerateTable(args);
            else if (args[0].Equals("-statistic"))
            {
                if (args[1].Equals("all")) CounterTool.Main(args);
                else if (args[1].Equals("core")) CoreCache.Main(args);
                else if (args[1].Equals("token")) TokenCache.Main(args);
                else if (args[1].Equals("short")) ShortCache.Main(args);
                else if (args[1].Equals("gamma")) GammaTool.Main(args);
                else if (args[1].Equals("dictionary")) DictionaryCache.Main(args);
                else
                {
                    Console.WriteLine("Main : invalid arguments !");
                }
            }
            else
            {
                Console.WriteLine("Main : invalid options !");
            }
        }
        else
        {
            Console.WriteLine("Usage : NLPConsole [options] [arguments]");
            Console.WriteLine("\t（1）测试提取数据：");
            Console.WriteLine("\t-(r2s | f2q | s2c) [id]");
            Console.WriteLine("\t（2）创建数据表：");
            Console.WriteLine("\t-create [raw | dictionary]");
            Console.WriteLine("\t（3）统计数据：");
            Console.WriteLine("\t-statistic [all | gamma]");
            Console.WriteLine("\t（4）生成数据：");
            Console.WriteLine("\t-generate [all | filtered | token | long | short | counter | core]");
            Console.WriteLine("\t（5）输出数据：");
            Console.WriteLine("\t-export [all | dictionary | raw | core | token | long | short | mark | counter | filtered]");
            Console.WriteLine("\t（6）输入数据：");
            Console.WriteLine("\t-import [all | dictionary | raw | core | token | long | short | mark | counter | filtered]");
            Console.WriteLine("\t（7）扫描数据：");
            Console.WriteLine("\t-scan [/c | /g | /qty | /adv | /adj | /aux | /loc | /conj | /prep | /spec] [value] ...");
        }
    }
}
