using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NLP
{
    public class QuantityExtractor
    {
        public static readonly string QUANTITY = "数量词";

        private static readonly string[] QUANTITY_RULES =
        {
            ////////////////////////////////////////////////////////////////////////////////
            //
            // 加载正则规则。
            //
            ////////////////////////////////////////////////////////////////////////////////
            "\\d+$v", // 单位符号
            "\\d+$u", // 单位名称
            "\\d+$q", // 量词
            "\\d+$y", // 货币名称

            ",\\d{3}",
            "\\d+[至|比]",
            "[￥|＄]\\d+",

            "$c+$u", // 单位名称
            "$c+$q", // 量词
            "$c+$y", // 货币名称

            "[\\.|+|\\-|*|/|<|=|#|＋|－|×|÷]+[A-Za-z\\d]+",
            "[A-Za-z\\d]+[\\.|~|'|\"|:|+|\\-|*|/|>|=|#|：|～|＋|－|×|÷]+",

            "[十|百|千|万|亿][多|余]?[十|百|千|万|亿]?$c{1,2}",
            "$c{1,2}[十|百|千|万|亿][多|余]?[个|十|百|千|万|亿]?($u|$q|$y)?",

            "\\d+[十|百|千|万|亿][多|余]?[个|十|百|千|万|亿]?($u|$q|$y)?",

            "每$u",
            "第$c", //序数
            "第\\d+", //序数

            "$c+分之",
            "[百]?分之$c+",
            "[\\d+|$c+]个百分点", //百分点

            "[上|中|下]午",
            "(\\d+|$c+)个(月|小时)",
            "(\\d+|$c+)(周|年|季度|刻钟)", 
        };

        private static FunctionSegment[] GetAllMatched(string content)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(content));
#endif
            // 创建对象
            List<FunctionSegment> fses
                = new List<FunctionSegment>();
            // 循环处理
            foreach (string rule in QUANTITY_RULES)
            {
                // 恢复规则
                string newRule = Quantity.RecoverRule(rule);
                // 检查匹配
                // 此函数非常消耗时间！！！
                Match match = Regex.Match(content, newRule);
                // 检查结果
                while (match.Success)
                {
                    // 创建对象
                    FunctionSegment fs =
                        new FunctionSegment();
                    // 设置数值
                    fs.Remark = QUANTITY;
                    fs.Index = match.Index;
                    fs.Value = match.Value;
                    fs.Length = match.Length;
                    // 加入列表
                    fses.Add(fs);
                    // 下一个
                    match = match.NextMatch();
                }
            }
            // 按照索引数值排序
            return fses.OrderBy(f => f.Index).ToList().ToArray();
        }

#if _USE_CLR
        public static FunctionSegment[] Extract(string content)
#elif _USE_CONSOLE
        public static FunctionSegment[]? Extract(string content)
#endif
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(content));
#endif
            // 获得匹配的项目
            FunctionSegment[] output = GetAllMatched(content);
            // 检查结果
            if (output == null || output.Length <= 0) return null;

            // 创建对象
            List<FunctionSegment> qses =
                new List<FunctionSegment>();
            // 循环处理
            for (int i = 0; i < content.Length; i++)
            {
                // 获得字符
                char c = FunctionSegment.GetChar(output, i);
                // 检查结果
                if (c != '\0')
                {
                    // 创建对象
                    FunctionSegment qs =
                        new FunctionSegment();
                    // 记录索引位置
                    qs.Index = i;
                    qs.Remark = QUANTITY;
                    // 创建对象
                    StringBuilder sb =
                        new StringBuilder();
                    // 循环处理
                    while (c != '\0')
                    {
                        // 增加字符
                        sb.Append(c);
                        // 增加计数
                        i++;
                        // 检查字符
                        c = FunctionSegment.GetChar(output, i);
                    }
                    // 设置参数
                    qs.Length = sb.Length; qs.Value = sb.ToString();
                    // 增加对象
                    qses.Add(qs);
                }
            }
            // 返回结果
            return qses.ToArray();
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            int fid = -1;
            // 尝试解析
            if (!int.TryParse(args[1], out fid))
            {
                Console.WriteLine("请输入正确的参数！"); return;
            }

            // 获得最大ID号码
            int maxID = FilteredContent.GetMaxID();
            // 打印信息
            Console.WriteLine("\tmax id = " + maxID);
            // 生成随机数
            Random random = new Random(fid + (int)DateTime.Now.Ticks);

            do
            {
                // 获得内容
                string? content =
                    FilteredCache.LoadContent(fid);
                // 检查内容
                if (string.IsNullOrEmpty(content))
                {
                    fid = random.Next(maxID); continue;
                }

                // 提取数量词
                FunctionSegment[]? qses = Extract(content);
                // 检查结果
                if (qses == null || qses.Length <= 0)
                {
                    fid = random.Next(maxID); continue;
                }

                // 打印原始内容
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("打印FilteredContent.GetContent(" + fid + ")内容！");
                Console.WriteLine("----------------------------------------");
                Console.WriteLine(content);

                // 打印提取后的内容
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("打印QuantityExtractor.Extract(" + fid + ")内容！");
                Console.WriteLine("----------------------------------------");
                // 打印所有内容
                foreach (FunctionSegment qs in qses) qs.Print();

                // 随机往下
                fid = random.Next(maxID);
                // 等待键盘
                if (Console.ReadKey().Key == ConsoleKey.Escape) break;

            } while (true);
        }
#endif
    }
}
