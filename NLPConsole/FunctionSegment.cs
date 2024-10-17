using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NLP
{
    public class FunctionSegment
    {
        public int Index { get; set; }
        public int Count { set; get; }
        public int Length { get; set; }
#if _USE_CLR
        public string Value { get; set; }
        public string Remark { get; set; }
#elif _USE_CONSOLE
        public string? Value { get; set; }
        public string? Remark { get; set; }
#endif

        public void Print()
        {
            // 打印内容
            LogTool.LogMessage(string.
                Format("\t({0}):[count({1}),index([{2}])] = \"{3}\"",
                    Remark, Count, Index, Value));
        }

        public bool Equals(FunctionSegment segment)
        {
#if DEBUG
            Debug.Assert(segment != null);
#endif
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(Value));
            Debug.Assert(!string.IsNullOrEmpty(segment.Value));
#endif
            // 检查参数
            if(!string.Equals(Value,segment.Value)) return false;
            // 检查参数
            if (string.IsNullOrEmpty(Remark))
            {
                return string.IsNullOrEmpty(segment.Remark);
            }
            // 返回结果
            return string.IsNullOrEmpty(segment.Remark) ?
                false : string.Equals(Remark, segment.Remark);
        }

        public static char GetChar(FunctionSegment[] qses, int index)
        {
#if _USE_CLR
            // 查询结果
            FunctionSegment qs = GetIncluded(qses, index);
#elif _USE_CONSOLE
            // 查询结果
            FunctionSegment? qs = GetIncluded(qses, index);
#endif
            // 检查结果
            if (qs == null) return '\0';
#if DEBUG
            Debug.Assert(qs != null && qs.Value != null);
#endif
            // 返回结果
            return qs.Value[index - qs.Index];
        }

#if _USE_CLR
        public static FunctionSegment GetIncluded(FunctionSegment[] qses, int index)
#elif _USE_CONSOLE
        public static FunctionSegment? GetIncluded(FunctionSegment[] qses, int index)
#endif
        {
#if DEBUG
            Debug.Assert(qses != null);
#endif
            foreach (FunctionSegment qs in qses)
            {
                // 检查索引位置
                if (index >= qs.Index &&
                    index < qs.Index + qs.Length) return qs;
            }
            // 返回结果
            return null;
        }

        public static FunctionSegment[] GetAllMatched(string content, string[][] templates)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(content));
#endif
            // 创建对象
            List<FunctionSegment> fses
                = new List<FunctionSegment>();
            // 循环处理
            foreach (string[] item in templates)
            {
                // 检查匹配
                // 此函数非常消耗时间！！！
                Match match = Regex.Match(content, item[0]);
                // 检查结果
                while (match.Success)
                {
                    // 创建对象
                    FunctionSegment fs =
                        new FunctionSegment();
                    // 设置数值
                    fs.Count = 1;
                    fs.Remark = item[1];
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
        public static FunctionSegment[] Extract(string content, string[][] templates)
#elif _USE_CONSOLE
        public static FunctionSegment[]? Extract(string content, string[][] templates)
#endif
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(content));
            Debug.Assert(templates != null && templates.Length > 0);
#endif
            // 获得匹配的项目
            FunctionSegment[] output =
                FunctionSegment.GetAllMatched(content, templates);
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
                    qs.Count = 1;
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
    }
}
