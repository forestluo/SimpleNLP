using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace NLP
{
    public class CoreSegment
    {
        public int Index { set; get; }
        public int Count { set; get; }
        public int Length { set; get; }
#if _USE_CLR
        public string Value { set; get; }
#elif _USE_CONSOLE
        public string? Value { set; get; }
#endif

        public int Flag { set; get; }
        public double Gamma { set; get; }
#if _USE_CLR
        public CoreSegment[] Subsegs { set; get; }
        public MarkAttribute[] Marks { set; get; }
#elif _USE_CONSOLE
        public MarkAttribute[]? Marks { set; get; }
        public CoreSegment[]? Subsegs { set; get; }
#endif

        public bool IsLeaf()
        {
            // 返回结果
            return Subsegs == null
                || Subsegs.Length <= 0;
        }

        public bool IsNone()
        {
            // 返回结果
            return Marks == null
                || Marks.Length <= 0;
        }

        public bool IsNullOrEmpty()
        {
            // 返回结果
            return string.IsNullOrEmpty(Value);
        }

        public void Reload()
        {
            // 检查参数
            if (IsLeaf()) return;

#if DEBUG
            Debug.Assert(Subsegs != null);
#endif
            for (int i = 0; i < Subsegs.Length; i++)
            {
#if DEBUG
                Debug.Assert(Subsegs[i] != null);
                Debug.Assert(Subsegs[i].Value != null);
#endif
#if _USE_CLR
                // 加载数据
                CoreSegment segment =
                    CoreCache.GetSegment(Subsegs[i].Value);
#elif _USE_CONSOLE
                // 加载数据
                CoreSegment? segment =
                    CoreCache.GetSegment(Subsegs[i].Value!);
#endif
                // 数据不一定在CoreCache范围内
                if (segment != null) Subsegs[i] = segment;
            }
        }

#if _USE_CLR
        public CoreSegment SelectPath()
#elif _USE_CONSOLE
        public CoreSegment? SelectPath()
#endif
        {
            if (IsLeaf()) return null;
#if DEBUG
            Debug.Assert(Subsegs != null);
#endif
            // 按照一定的方式排序
            Subsegs = Subsegs.
                OrderByDescending(s => s.Flag).
                ThenByDescending(s => s.Gamma).ToArray();
            // 返回结果
            return Subsegs[0];
        }

#if _USE_CLR
        public string GetPath()
#elif _USE_CONSOLE
        public string? GetPath()
#endif
        {
            // 检查参数
            if (IsLeaf()) return null;
#if DEBUG
            Debug.Assert(Subsegs != null);
#endif
            // 创建对象
            StringBuilder sb = new StringBuilder();
            // 循环处理
            foreach (CoreSegment segment in Subsegs)
            {
                if (!segment.IsNullOrEmpty())
                {
                    sb.Append(string.Format("|{0}", segment.Value));
                }
            }
            // 返回结果
            string path =
                sb.ToString().Substring(1);
            // 检查结果
            // 未能成功分解
            return path.Equals(Value) ? null : path;
        }

        public void SetPath(string path)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(path));
#endif
            // 索引值
            int i = 0;
            // 链表
            List<CoreSegment> subsegs
                = new List<CoreSegment>();
            // 分割路径成分
            string[] descriptions = path.Split('|');
#if DEBUG
            Debug.Assert(descriptions != null);
#endif
            // 生成子路径
            foreach (string description in descriptions)
            {
                // 生成对象
                CoreSegment subseg
                    = new CoreSegment();
                // 设置数值
                subseg.Index = i;
                subseg.Value = description;
                subseg.Length = description.Length;
                subseg.Count =
                    CounterTool.GetCount(description, false);
                // 增加对象
                subsegs.Add(subseg); i += description.Length;
            }
            // 设置子路径
            this.Subsegs = subsegs.ToArray();
        }

        public double GetGamma()
        {
            // 检查参数
            if (IsLeaf()) return -1.0f;
#if DEBUG
            Debug.Assert(Subsegs != null && Subsegs.Length > 0);
#endif
            // 检查参数
            if (Subsegs.Length == 1)
                return Subsegs[0].Gamma;

            // 对象
            StringBuilder sb = new StringBuilder();
            // 累计值
            double result = 0.0f;
            // 循环处理
            foreach (CoreSegment segment in this.Subsegs)
            {
                // 检查内容
                if (segment.IsNullOrEmpty()) return -1.0f;
#if DEBUG
                Debug.Assert(segment.Value != null);
#endif
                // 增加内容
                sb.Append(segment.Value);
                // 获得数值
                segment.Count =
                    CounterTool.
                    GetCount(segment.Value, false);
                // 检查结果
                if (segment.Count <= 0) return -1.0f;

                // 累加结果
                result += 1.0f / (double)segment.Count;
            }
            // 获得数值
            int count = CounterTool.GetCount(sb.ToString(), true);
            // 检查结果
            if (count <= 0) count = 1;
            // 返回结果（最大的Gamma数值）
            return result * (double)count / (double)this.Subsegs.Length;
        }

        public void SetMarks()
        {
            if (IsLeaf()) return;
#if DEBUG
            Debug.Assert(Subsegs != null && Subsegs.Length > 0);
#endif
            // 循环处理（按照顺序）
            foreach (CoreSegment segment in Subsegs)
            {
                // 检查结果
                if (segment.IsLeaf()) continue;
#if DEBUG
                Debug.Assert(segment.Subsegs != null);
#endif
                // 循环处理
                foreach(CoreSegment subseg in segment.Subsegs)
                {
#if DEBUG
                    Debug.Assert(subseg.Value != null);
#endif
                    // 获得属性描述
                    subseg.Marks = MarkCache.GetMarks(subseg.Value);
                }
            }
        }

#if _USE_CLR
        public string GetDescription()
#elif _USE_CONSOLE
        public string? GetDescription()
#endif
        {
            // 检查参数
            if (Marks == null ||
                Marks.Length <= 0) return null;

            StringBuilder sb = new StringBuilder();
            // 循环处理
            foreach (MarkAttribute attribute in Marks)
            {
#if DEBUG
                Debug.Assert(!string.IsNullOrEmpty(attribute.Value));
#endif
                sb.Append('|'); sb.Append(attribute.Value);
                // 检查参数
                if(!string.IsNullOrEmpty(attribute.Remark))
                {
                    sb.Append('.'); sb.Append(attribute.Remark);
                }
            }
            // 返回结果
            return sb.ToString().Substring(1);
        }

        public void Print()
        {
            // 检查参数
            if (IsLeaf()) return;
            // 打印原始内容
            LogTool.LogMessage(string.Format("\tValue = \"{0}\"", Value));
            //LogTool.LogMessage(string.Format("\tSid = {0}", Sid));
            LogTool.LogMessage(string.Format("\tCount = {0}", Count));
#if DEBUG
            Debug.Assert(Subsegs != null && Subsegs.Length > 0);
#endif
            // 循环处理（按照顺序）
            foreach (CoreSegment segment in
                Subsegs.OrderBy(s => s.Flag).ThenBy(s => s.Gamma).ToArray())
            {
                // 检查结果
                if (segment.IsLeaf()) continue;

                //LogTool.LogMessage(string.Format("\tsegment.Sid = {0}", segment.Sid));
                //LogTool.LogMessage(string.Format("\tsegment.Count = {0}", segment.Count));
                LogTool.LogMessage(string.Format
                    ("\tMethod = {0}[{1}]|{2}|{3}|{4}|{5}",
                    (segment.Flag & 0x080) != 0 ? "M" : "-",
                    (segment.Index < 0) ? "-" : segment.Index.ToString(),
                    (segment.Flag & 0x100) != 0 ? "C" : "-",
                    (segment.Flag & 0x200) != 0 ? "R" : "-",
                    (segment.Flag & 0x400) != 0 ? "R+" : "-",
                    (segment.Flag & 0x800) != 0 ? "F" : "-"));
                LogTool.LogMessage(string.Format("\tGamma = {0:0.00000000}", segment.Gamma));
#if DEBUG
                Debug.Assert(segment.Subsegs != null && segment.Subsegs.Length > 0);
#endif
                // 打印切分结果
                for (int i = 0; i < segment.Subsegs.Length; i++)
                {
#if _USE_CLR
                    string description = segment.Subsegs[i].GetDescription();
#elif _USE_CONSOLE
                    string? description = segment.Subsegs[i].GetDescription();
#endif
                    if (string.
                        IsNullOrEmpty(description))
                        description = string.Empty;
                    else description = " |" + description;
                    // 记录日志
                    LogTool.LogMessage(
                        string.Format("\t[{0}]({1}\"{2}\" : {3}{4}) = {5:0.00000000}",
                                    i,
                                    (segment.Subsegs[i].Flag & 0x800) != 0 ? "*" : "",
                                    segment.Subsegs[i].Length > 2 ?
                                    segment.Subsegs[i].GetPath() : segment.Subsegs[i].Value,
                                    segment.Subsegs[i].Count, description, segment.Subsegs[i].Gamma)); ;
                }
            }
        }

        public bool Equals(CoreSegment segment)
        {
#if DEBUG
            Debug.Assert(segment != null);
#endif
            // 检查参数
            if (IsLeaf() || segment.IsLeaf()) return false;
#if DEBUG
            Debug.Assert(this.Subsegs != null && segment.Subsegs != null);
#endif
            // 检查参数
            if (this.Subsegs.Length != segment.Subsegs.Length) return false;
            // 循环处理
            for (int i = 0; i < this.Subsegs.Length && i < segment.Subsegs.Length; i++)
            {
                if (!string.Equals(this.Subsegs[i].Value, segment.Subsegs[i].Value)) return false;
            }
            // 返回结果
            return true;
        }

        public static CoreSegment Split(string content)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(content));
#endif
            // 创建对象
            List<CoreSegment> segments
                = new List<CoreSegment>();
            // 尝试切分
            // 获得切分结果
            CoreSegment mainSegment =
                CoreMatcher.Split(content);
#if DEBUG
            Debug.Assert(mainSegment != null);
#endif
            // 进行合并
            if (!mainSegment.IsLeaf())
            {
#if DEBUG
                Debug.Assert(mainSegment.Subsegs != null);
#endif
                // 直接加入
                segments.AddRange(mainSegment.Subsegs.ToList());
            }

            // 对象参数
            CoreSegment subSegment;

            // 尝试切分结果
            subSegment = CoreSpliter.Split(content);
#if DEBUG
            Debug.Assert(subSegment != null);
#endif
            // 检查结果
            if (!subSegment.IsLeaf())
            {
#if DEBUG
                Debug.Assert(subSegment.Subsegs != null);
#endif
                foreach (CoreSegment seg in subSegment.Subsegs)
                {
                    bool markFlag = false;
                    // 循环处理
                    foreach (CoreSegment item in segments)
                    {
                        // 设置方法标记位
                        if (item.Equals(seg))
                        {
                            item.Flag |= seg.Flag; markFlag = true;
                        }
                    }
                    // 检查重复情况
                    if (!markFlag) segments.Add(seg);
                }
            }

            // 尝试切分结果
            subSegment = GammaSpliter.Split(content);
#if DEBUG
            Debug.Assert(subSegment != null);
#endif
            // 检查结果
            if (!subSegment.IsLeaf())
            {
#if DEBUG
                Debug.Assert(subSegment.Subsegs != null);
#endif
                foreach (CoreSegment seg in subSegment.Subsegs)
                {
                    bool markFlag = false;
                    // 循环处理
                    foreach (CoreSegment item in segments)
                    {
                        // 设置方法标记位
                        if (item.Equals(seg))
                        {
                            item.Flag |= seg.Flag; markFlag = true;
                        }
                    }
                    // 检查重复情况
                    if (!markFlag) segments.Add(seg);
                }
            }

            // 清理非重点内容
            segments = segments.
                Where(s => (s.Flag & 0xF00) != 0).ToList();
            // 加载标志位
            foreach (CoreSegment segment in segments) segment.Reload();
            // 直接返回结果集
            // 清理非重点内容
            mainSegment.Subsegs = segments.ToArray();
            // 返回结果
            return mainSegment;
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            int sid = -1;
            // 尝试解析
            if (!int.TryParse(args[1], out sid))
            {
                Console.WriteLine("请输入正确的参数！"); return;
            }

            // 获得最大ID号码
            int maxID = ShortContent.GetMaxID();
            // 打印信息
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("\tmax id = " + maxID);
            // 生成随机数
            Random random = new Random(sid + (int)DateTime.Now.Ticks);

            do
            {
                // 获得内容
                string? content =
                    ShortContent.GetContent(sid);
                // 检查内容
                if (string.IsNullOrEmpty(content))
                {
                    // 随机选择
                    sid = random.Next(maxID); continue;
                }

                // 创建对象
                CoreSegment segment = Split(content);
                // 检查结果
                if (segment == null || segment.IsLeaf())
                {
                    Console.WriteLine("----------------------------------------");
                    Console.WriteLine("CoreSegment.MinSplit(" + sid + ")未能成功！");
                    Console.WriteLine("----------------------------------------");
                    // 随机选择
                    sid = random.Next(maxID); continue;
                }

                // 选择路径
                CoreSegment? path = segment.SelectPath();
                // 检查结果
                if (path != null && (path.Flag & 0x500) == 0x500)
                {
                    //Console.WriteLine("----------------------------------------");
                    //Console.WriteLine("解析无歧义！");
                    //Console.WriteLine("----------------------------------------");
                    // 随机选择
                    sid = random.Next(maxID); continue;
                }

                // 打印原始内容
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("打印ShortContent.GetContent(" + sid + ")内容！");
                Console.WriteLine("----------------------------------------");
                Console.WriteLine(content);

                // 加载词性
                segment.SetMarks();
                // 打印清洗后的内容
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("打印CoreSegment.Split(" + sid + ")内容！");
                Console.WriteLine("----------------------------------------");
                // 打印所有内容
                segment.Print();

                // 读取一行
                string line = Console.ReadLine()!;
                // 检查结果
                if (line == null || line.Length <= 0)
                {
                    // 随机选择
                    sid = random.Next(maxID);
                }
                else if (line.Equals("quit") || line.Equals("exit")) break;
                else
                {
                    // 执行命令行
                    if(ConsoleTool.Execute(line))
                    {
                        // 记录操作
                        OperationLog.Log("CoreSegment.Main", line);
                    }
                    else
                    {
                        Console.WriteLine("请输入正确的参数！");
                        Console.WriteLine("Usage : reload");
                        Console.WriteLine("Usage : (add | del) [string]");
                        Console.WriteLine("Usage : attr -(add| del) [string] [major]");
                        Console.WriteLine("\tmajor : 名词|动词|形容词|副词|代词|介词|连词|数词|量词|助词|感叹词|拟声词|数量词|习惯用语");
                    }
                }

            } while (true);

            // 保存数据
            TokenCache.SaveTokens();
            ShortCache.SaveShorts();
            CoreCache.SaveSegments();
            DictionaryCache.SaveEntries();
        }
#endif
    }
}
