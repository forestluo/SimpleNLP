// See https://aka.ms/new-console-template for more information
using NLP;
using LTP;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 2)
        {
            if (args[0].Equals("-count")) LTPCounter.Main(args);
            else if (args[0].Equals("-input")) LTPXMLTool.Main(args);
            else if (args[0].Equals("-create"))
            {
                if (args[1].Equals("content")) LTPContent.Main(args);
                if (args[1].Equals("attribute")) LTPAttributeContent.Main(args);
                else
                {
                    Console.WriteLine("Main : invalid options !");
                }
            }
            else if (args[0].Equals("-export")) FileExporter.Main(args);
            else if (args[0].Equals("-import")) FileImporter.Main(args);
            else if (args[0].Equals("-generate"))
            {
                if (args[1].Equals("attribute")) LTPAttributeCache.Main(args);
                else
                {
                    Console.WriteLine("Main : invalid options !");
                }
            }
            else
            {
                Console.WriteLine("Main : invalid options !");
            }
        }
        else
        {
            Console.WriteLine("Usage : LTPConsole [options] [arguments]");
            Console.WriteLine("\t（1）统计行数");
            Console.WriteLine("\t-count [file name]");
            Console.WriteLine("\t（2）输入数据");
            Console.WriteLine("\t-input [file name]");
            Console.WriteLine("\t（3）创建数据");
            Console.WriteLine("\t-create [content | attribute]");
            Console.WriteLine("\t（4）输出文件");
            Console.WriteLine("\t-export [file name]");
            Console.WriteLine("\t（5）输入文件");
            Console.WriteLine("\t-import [file name]");
            Console.WriteLine("\t（6）生成数据");
            Console.WriteLine("\t-generate [attribute]");
        }
    }
}