using System.Data;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NLP
{
    public enum RealType
    {
        名词 = 1,
        代词 = 2,
        动词 = 3,
        形容词 = 4,
        数词 = 5,
        量词 = 6,
        数量词 = 7,
        方位词 = 8,
    };

    public enum FunctionType
    {
        副词 = 1,
        连词 = 2,
        介词 = 3,
        助词 = 4,
        衬词 = 5,
        感叹词 = 6,
        拟声词 = 7,
    };

    public enum GrammarType
    {
        名词 = 1,
        代词 = 2,
        动词 = 3,
        形容词 = 4,
        数词 = 5,
        量词 = 6,
        数量词 = 7,
        方位词 = 8,

        副词 = 9,
        连词 = 10,
        介词 = 11,
        助词 = 12,
        衬词 = 13,
        感叹词 = 14,
        拟声词 = 15,
    };

    public class Grammar
    {
#if _USE_CLR
        public static string GetName(RealType type)
#elif _USE_CONSOLE
        public static string? GetName(RealType type)
#endif
        {
            switch (type)
            {
                case RealType.名词: return WordType.NOUN;
                case RealType.代词: return WordType.PRONOUN;
                case RealType.动词: return WordType.VERB;
                case RealType.形容词: return WordType.ADJECTIVE;
                case RealType.数词: return WordType.NUMERAL;
                case RealType.量词: return WordType.QUANTIFIER;
                case RealType.数量词: return WordType.QUANTITY;
                case RealType.方位词: return WordType.LOCATIVE;
                default: break;
            }
            // 返回结果
            return null;
        }

#if _USE_CLR
        public static string GetName(FunctionType type)
#elif _USE_CONSOLE
        public static string? GetName(FunctionType type)
#endif
        {
            switch (type)
            {
                case FunctionType.副词: return WordType.ADVERB;
                case FunctionType.连词: return WordType.CONJUNCTION;
                case FunctionType.介词: return WordType.PREPOSITION;
                case FunctionType.助词: return WordType.AUXILIARY;
                case FunctionType.衬词: return WordType.PADDING;
                case FunctionType.感叹词: return WordType.EXCLAMATION;
                case FunctionType.拟声词: return WordType.ONOMATOPOEIA;
                default: break;
            }
            // 返回结果
            return null;
        }

#if _USE_CLR
        public static string GetName(GrammarType type)
#elif _USE_CONSOLE
        public static string? GetName(GrammarType type)
#endif
        {
            switch (type)
            {
                case GrammarType.名词: return WordType.NOUN;
                case GrammarType.代词: return WordType.PRONOUN;
                case GrammarType.动词: return WordType.VERB;
                case GrammarType.形容词: return WordType.ADJECTIVE;
                case GrammarType.数词: return WordType.NUMERAL;
                case GrammarType.量词: return WordType.QUANTIFIER;
                case GrammarType.数量词: return WordType.QUANTITY;
                case GrammarType.方位词: return WordType.LOCATIVE;

                case GrammarType.副词: return WordType.ADVERB;
                case GrammarType.连词: return WordType.CONJUNCTION;
                case GrammarType.介词: return WordType.PREPOSITION;
                case GrammarType.助词: return WordType.AUXILIARY;
                case GrammarType.衬词: return WordType.PADDING;
                case GrammarType.感叹词: return WordType.EXCLAMATION;
                case GrammarType.拟声词: return WordType.ONOMATOPOEIA;
                default: break;
            }
            // 返回结果
            return null;
        }

        public static double GetQtyGamma(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 获得量词
            string[] quantifiers = Quantity.GetQuantifiers();
#if _USE_CONSOLE
            // 获得计数
            Console.WriteLine(string.Format(
                "\t[数量词]频次统计：{0}",
                CounterTool.GetCount(quantifiers)));
            // 单词计数
            Console.WriteLine(string.Format(
                "\t[\"{0}\"]频次统计：{1}", value,
                CounterTool.GetCount(value, true)));
            // 后缀计数
            Console.WriteLine(string.Format(
                "\t[数量词]+[\"{0}\"]频次统计：{1}", value,
                CounterTool.GetCount(false, value, quantifiers)));
            // 相关系数
            Console.WriteLine(string.Format(
                "\t[数量词]+[\"{0}\"]相关系数：{1}", value,
                GammaTool.GetGamma(false, value, quantifiers)));
#endif
            // 返回结果
            return GammaTool.GetGamma(false, value, quantifiers);
        }

        public static double GetAuxGamma(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 获得助词
            string[] auxiliaries = FunctionWord.
                GetFunctions(WordType.AUXILIARY);
#if _USE_CONSOLE
            // 获得计数
            Console.WriteLine(string.Format(
                "\t[助词]频次统计：{0}",
                CounterTool.GetCount(auxiliaries)));
            // 单词计数
            Console.WriteLine(string.Format(
                "\t[\"{0}\"]频次统计：{1}", value,
                CounterTool.GetCount(value, true)));
            // 后缀计数
            Console.WriteLine(string.Format(
                "\t[\"{0}\"]+[助词]频次统计：{1}", value,
                CounterTool.GetCount(true, value, auxiliaries)));
            // 相关系数
            Console.WriteLine(string.Format(
                "\t[\"{0}\"]+[助词]相关系数：{1}", value,
                GammaTool.GetGamma(true, value, auxiliaries)));
#endif
            // 返回结果
            return GammaTool.GetGamma(true, value, auxiliaries);
        }

        public static double GetAdvGamma(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 获得副词
            string[] adverbs = FunctionWord.
                GetFunctions(WordType.ADVERB);
#if _USE_CONSOLE
            // 获得计数
            Console.WriteLine(string.Format(
                "\t[副词]频次统计：{0}",
                CounterTool.GetCount(adverbs)));
            // 单词计数
            Console.WriteLine(string.Format(
                "\t[\"{0}\"]频次统计：{1}", value,
                CounterTool.GetCount(value, true)));
            // 后缀计数
            Console.WriteLine(string.Format(
                "\t[副词]+[\"{0}\"]频次统计：{1}", value,
                CounterTool.GetCount(false, value, adverbs)));
            // 相关系数
            Console.WriteLine(string.Format(
                "\t[副词]+[\"{0}\"]相关系数：{1}", value,
                GammaTool.GetGamma(false, value, adverbs)));
#endif
            // 返回结果
            return GammaTool.GetGamma(false, value, adverbs);
        }

        public static double GetLocGamma(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 获得方位词
            string[] locatives = FunctionWord.
                GetFunctions(WordType.LOCATIVE);
#if _USE_CONSOLE
            // 获得计数
            Console.WriteLine(string.Format(
                "\t[方位词]频次统计：{0}",
                CounterTool.GetCount(locatives)));
            // 单词计数
            Console.WriteLine(string.Format(
                "\t[\"{0}\"]频次统计：{1}", value,
                CounterTool.GetCount(value, true)));
            // 后缀计数
            Console.WriteLine(string.Format(
                "\t[\"{0}\"]+[方位词]频次统计：{1}", value,
                CounterTool.GetCount(true, value, locatives)));
            // 相关系数
            Console.WriteLine(string.Format(
                "\t[\"{0}\"]+[方位词]相关系数：{1}", value,
                GammaTool.GetGamma(true, value, locatives)));
#endif
            // 返回结果
            return GammaTool.GetGamma(true, value, locatives);
        }

        public static double GetPFunGamma(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 获得虚词
            string[] functions = FunctionWord.GetFunctions();
#if _USE_CONSOLE
            // 获得计数
            Console.WriteLine(string.Format(
                "\t[虚词]频次统计：{0}",
                CounterTool.GetCount(functions)));
            // 单词计数
            Console.WriteLine(string.Format(
                "\t[\"{0}\"]频次统计：{1}", value,
                CounterTool.GetCount(value, true)));
            // 后缀计数
            Console.WriteLine(string.Format(
                "\t[\"{0}\"]+[虚词]频次统计：{1}", value,
                CounterTool.GetCount(true, value, functions)));
            // 相关系数
            Console.WriteLine(string.Format(
                "\t[\"{0}\"]+[虚词]相关系数：{1}", value,
                GammaTool.GetGamma(true, value, functions)));
#endif
            // 返回结果
            return GammaTool.GetGamma(true, value, functions);
        }

        public static double GetSFunGamma(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 获得虚词
            string[] functions = FunctionWord.GetFunctions();
#if _USE_CONSOLE
            // 获得计数
            Console.WriteLine(string.Format(
                "\t[方位词]频次统计：{0}",
                CounterTool.GetCount(functions)));
            // 单词计数
            Console.WriteLine(string.Format(
                "\t[\"{0}\"]频次统计：{1}", value,
                CounterTool.GetCount(value, true)));
            // 后缀计数
            Console.WriteLine(string.Format(
                "\t[虚词]+[\"{0}\"]频次统计：{1}", value,
                CounterTool.GetCount(false, value, functions)));
            // 相关系数
            Console.WriteLine(string.Format(
                "\t[虚词]+[\"{0}\"]相关系数：{1}", value,
                GammaTool.GetGamma(false, value, functions)));
#endif
            // 返回结果
            return GammaTool.GetGamma(false, value, functions);
        }

        public static double GetPrepGamma(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 获得介词
            string[] prepostitions = FunctionWord.
                GetFunctions(WordType.PREPOSITION);
#if _USE_CONSOLE
            // 获得计数
            Console.WriteLine(string.Format(
                "\t[介词]频次统计：{0}",
                CounterTool.GetCount(prepostitions)));
            // 单词计数
            Console.WriteLine(string.Format(
                "\t[\"{0}\"]频次统计：{1}", value,
                CounterTool.GetCount(value, true)));
            // 后缀计数
            Console.WriteLine(string.Format(
                "\t[介词]+[\"{0}\"]频次统计：{1}", value,
                CounterTool.GetCount(false, value, prepostitions)));
            // 相关系数
            Console.WriteLine(string.Format(
                "\t[介词]+[\"{0}\"]相关系数：{1}", value,
                GammaTool.GetGamma(false, value, prepostitions)));
#endif
            // 返回结果
            return GammaTool.GetGamma(false, value, prepostitions);
        }

        public static double GetPronGamma(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 获得代词
            string[] pronouns = FunctionWord.
                GetFunctions(WordType.PRONOUN);
#if _USE_CONSOLE
            // 获得计数
            Console.WriteLine(string.Format(
                "\t[代词]频次统计：{0}",
                CounterTool.GetCount(pronouns)));
            // 单词计数
            Console.WriteLine(string.Format(
                "\t[\"{0}\"]频次统计：{1}", value,
                CounterTool.GetCount(value, true)));
            // 后缀计数
            Console.WriteLine(string.Format(
                "\t[代词]+[\"{0}\"]频次统计：{1}", value,
                CounterTool.GetCount(false, value, pronouns)));
            // 相关系数
            Console.WriteLine(string.Format(
                "\t[代词]+[\"{0}\"]相关系数：{1}", value,
                GammaTool.GetGamma(false, value, pronouns)));
#endif
            // 返回结果
            return GammaTool.GetGamma(false, value, pronouns);
        }

        public static double GetConjGamma(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 获得连词
            string[] conjunctions = FunctionWord.
                GetFunctions(WordType.CONJUNCTION);
#if _USE_CONSOLE
            // 获得计数
            Console.WriteLine(string.Format(
                "\t[连词]频次统计：{0}",
                CounterTool.GetCount(conjunctions)));
            // 单词计数
            Console.WriteLine(string.Format(
                "\t[\"{0}\"]频次统计：{1}", value,
                CounterTool.GetCount(value, true)));
            // 后缀计数
            Console.WriteLine(string.Format(
                "\t[连词]+[\"{0}\"]频次统计：{1}", value,
                CounterTool.GetCount(false, value, conjunctions)));
            // 相关系数
            Console.WriteLine(string.Format(
                "\t[连词]+[\"{0}\"]相关系数：{1}", value,
                GammaTool.GetGamma(false, value, conjunctions)));
#endif
            // 返回结果
            return GammaTool.GetGamma(false, value, conjunctions);
        }

        public static double[] GetSpecGamma(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 创建链表
            List<double> values = new List<double>();
            // 前缀
            values.Add(GammaTool.GetGamma(new string[] { "不", value }));
            values.Add(GammaTool.GetGamma(new string[] { "太", value }));
            values.Add(GammaTool.GetGamma(new string[] { "很", value }));
            // 后缀
            values.Add(GammaTool.GetGamma(new string[] { value, "着" }));
            values.Add(GammaTool.GetGamma(new string[] { value, "了" }));
            values.Add(GammaTool.GetGamma(new string[] { value, "过" }));
            // 前缀
            values.Add(GammaTool.GetGamma(new string[] { "用", value }));
            values.Add(GammaTool.GetGamma(new string[] { "把", value }));
            values.Add(GammaTool.GetGamma(new string[] { "被", value }));
            // 后缀
            values.Add(GammaTool.GetGamma(new string[] { value, "的" }));
            values.Add(GammaTool.GetGamma(new string[] { value, "地" }));
            values.Add(GammaTool.GetGamma(new string[] { value, "得" }));
            // 前缀
            values.Add(GammaTool.GetGamma(new string[] { value, "应该" }));
            values.Add(GammaTool.GetGamma(new string[] { value, "可以" }));
            values.Add(GammaTool.GetGamma(new string[] { value, "必须" }));
            // 后缀
            values.Add(GammaTool.GetGamma(new string[] { value, "前" }));
            values.Add(GammaTool.GetGamma(new string[] { value, "内" }));
            values.Add(GammaTool.GetGamma(new string[] { value, "间" }));
            // 返回结果
            return values.ToArray();
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            // 检查参数个数
            if (args.Length < 3)
            {
                Console.WriteLine("Main : invalid options !"); return;
            }

            // 开启日志
            LogTool.SetLog(true);
            // 创建计时器
            Stopwatch watch = new Stopwatch();
            // 开启计时器
            watch.Start();
            // 检查参数
            if (args[1].Equals("/c"))
            {
                Console.WriteLine("准备扫描下列数据！");
                // 获得输入
                string[] inputs = new string[args.Length - 2];
                // 拷贝数据
                for (int i = 0; i < inputs.Length; i++) inputs[i] = args[i + 2];

                // 打印参数
                for (int i = 0; i < inputs.Length; i++)
                {
                    Console.WriteLine(string.Format(
                        "\tinputs(\"{0}\").count = {1}",
                        inputs[i], CounterTool.GetCount(inputs[i], true)));
                }
            }
            else if (args[1].Equals("/g"))
            {
                // 获得解析
                string[] inputs = args[2].Split('|');
                // 检查结果
                if (inputs.Length >= 1)
                {
                    // 获得字符串
                    string value = args[2].Replace("|", "");
                    // 打印结果
                    Console.WriteLine(string.Format(
                        "\t\"{0}\".count = {1}",
                        value, CounterTool.GetCount(value, true)));
                    // 打印结果
                    foreach (string input in inputs)
                    {
                        Console.WriteLine(string.Format(
                            "\tinput[\"{0}\"].count = {1}",
                            input, CounterTool.GetCount(input, true)));
                    }
                    // 打印结果
                    Console.WriteLine(string.Format(
                        "\t\"{0}\".gamma = {1}",
                        args[2], GammaTool.GetGamma(inputs)));
                }
                else
                {
                    Console.WriteLine("Main : insufficient options !");
                }
            }
            else if (args[1].Equals("/qty")) GetQtyGamma(args[2]);
            else if (args[1].Equals("/aux")) GetAuxGamma(args[2]);
            else if (args[1].Equals("/adv")) GetAdvGamma(args[2]);
            else if (args[1].Equals("/loc")) GetLocGamma(args[2]);
            else if (args[1].Equals("/prep")) GetPrepGamma(args[2]);
            else if (args[1].Equals("/pron")) GetPronGamma(args[2]);
            else if (args[1].Equals("/conj")) GetConjGamma(args[2]);
            else if (args[1].Equals("/pfun")) GetPFunGamma(args[2]);
            else if (args[1].Equals("/sfun")) GetSFunGamma(args[2]);
            else if (args[1].Equals("/spec"))
            {
                // 获得结果
                double[] outputs = GetSpecGamma(args[2]);
                // 打印结果
                Console.WriteLine(string.Format("\t\"不{0}\".gamma = {1}", args[2], outputs[0]));
                Console.WriteLine(string.Format("\t\"太{0}\".gamma = {1}", args[2], outputs[1]));
                Console.WriteLine(string.Format("\t\"很{0}\".gamma = {1}", args[2], outputs[2]));
                Console.WriteLine(string.Format("\t\"{0}着\".gamma = {1}", args[2], outputs[3]));
                Console.WriteLine(string.Format("\t\"{0}了\".gamma = {1}", args[2], outputs[4]));
                Console.WriteLine(string.Format("\t\"{0}过\".gamma = {1}", args[2], outputs[5]));
                Console.WriteLine(string.Format("\t\"用{0}\".gamma = {1}", args[2], outputs[6]));
                Console.WriteLine(string.Format("\t\"把{0}\".gamma = {1}", args[2], outputs[7]));
                Console.WriteLine(string.Format("\t\"被{0}\".gamma = {1}", args[2], outputs[8]));
                Console.WriteLine(string.Format("\t\"{0}的\".gamma = {1}", args[2], outputs[9]));
                Console.WriteLine(string.Format("\t\"{0}地\".gamma = {1}", args[2], outputs[10]));
                Console.WriteLine(string.Format("\t\"{0}得\".gamma = {1}", args[2], outputs[11]));
                Console.WriteLine(string.Format("\t\"应该{0}\".gamma = {1}", args[2], outputs[12]));
                Console.WriteLine(string.Format("\t\"可以{0}\".gamma = {1}", args[2], outputs[13]));
                Console.WriteLine(string.Format("\t\"必须{0}\".gamma = {1}", args[2], outputs[14]));
                Console.WriteLine(string.Format("\t\"{0}前\".gamma = {1}", args[2], outputs[15]));
                Console.WriteLine(string.Format("\t\"{0}内\".gamma = {1}", args[2], outputs[16]));
                Console.WriteLine(string.Format("\t\"{0}间\".gamma = {1}", args[2], outputs[17]));
            }
            else
            {
                Console.WriteLine("Main : invalid options !");
            }
                // 关闭计时器
                watch.Stop();
                // 打印结果
                Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
            }
#endif
    }
}
