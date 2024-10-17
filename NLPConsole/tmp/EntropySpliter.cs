using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace NLP
{
    public class EntropySpliter
    {
        public static CoreSegment Split(string content)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(content));
#endif
            // 创建对象
            CoreSegment segment = new CoreSegment();
            // 设置参数
            segment.Value = content;
            segment.Length = content.Length;
            segment.Count = MiscTool.GetCount(content, true);

            // 生成分支
            MaxSplit(segment);
            // 搜集分支
            List<CoreSegment> segments
                = new List<CoreSegment>();
            Collect(segment, segments);
            // 生成路径
            segment.Subsegs = new CoreSegment[1];
            segment.Subsegs[0] = new CoreSegment();
            // 设置参数
            segment.Subsegs[0].Index = -4;
            segment.Subsegs[0].Method = 0x08;
            segment.Subsegs[0].Count = segment.Count;
            segment.Subsegs[0].Value = segment.Value;
            segment.Subsegs[0].Length = segment.Length;
            segment.Subsegs[0].Subsegs = segments.ToArray();
            segment.Subsegs[0].Gamma = segment.Subsegs[0].GetGamma();
            // 检查结果
            if (segments.Count == 1)
            {
                segment.Gamma = segment.Subsegs[0].Gamma;
                segment.Count = segment.Subsegs[0].Count;
            }
            // 返回结果
            return segment;
        }

        private static void Collect(CoreSegment segment, List<CoreSegment> segments)
        {
#if DEBUG
            Debug.Assert(segment != null && segment.Length > 0);
#endif
            // 检查分支
            // 已没有分支
            if (segment.IsLeaf()) { segments.Add(segment); return; }
#if DEBUG
            Debug.Assert(segment.Subsegs != null);
#endif
            // 循环带入
            foreach (CoreSegment item in segment.Subsegs) Collect(item, segments);
        }

        private static void MaxSplit(CoreSegment segment)
        {
#if DEBUG
            Debug.Assert(segment != null && segment.Length > 0);
            Debug.Assert(segment.Value != null);
#endif
            // 检查参数
            if (segment.Length == 1)
            {
                // 设置参数
                segment.Gamma = 1.0f;
                // 查询计数
                segment.Count =
                    TokenCache.GetCount(segment.Value[0]); return;
            }
            else if (segment.Length == 2)
            {
                // 计算Gamma
                segment.Gamma = GammaTool.
                    GetGamma(segment.Value);
                // 查询计数
                segment.Count = MiscTool.
                    GetCount(segment.Value, false);
                // 检查结果
                if (segment.Count <= 0)
                {
                    // 参数
                    CoreSegment subseg;
                    // 创建子对象
                    segment.Subsegs = new CoreSegment[2];

                    // 左侧数据
                    subseg = new CoreSegment();
                    // 设置参数
                    subseg.Index = 0;
                    subseg.Length = 1;
                    subseg.Gamma = 1.0f;
                    subseg.Value =
                        segment.Value.Substring(0, 1);
                    subseg.Count =
                        TokenCache.GetCount(subseg.Value[0]);
                    // 设置参数
                    segment.Subsegs[0] = subseg;

                    // 右侧数据
                    subseg = new CoreSegment();
                    // 设置参数
                    subseg.Index = 1;
                    subseg.Length = 1;
                    subseg.Gamma = 1.0f;
                    subseg.Value =
                        segment.Value.Substring(1);
                    subseg.Count =
                        TokenCache.GetCount(subseg.Value[0]);
                    // 设置参数
                    segment.Subsegs[1] = subseg;
                }
                // 返回
                return;
            }

            // 生成对象
            List<CoreSegment> segments
                = new List<CoreSegment>();
            // 执行循环
            for (int j = 2; j <= segment.Length - 1; j++)
            {
                // 检查长度
                for (int i = 0; i <= segment.Length - j; i++)
                {
                    // 获得子字符串
                    string value = segment.Value.Substring(i, j);
                    // 检查结果
                    if (value == null || value.Length != j) continue;

                    // 获得计数
                    int count = MiscTool.GetCount(value, false);
                    // 检查结果
                    if (count > 0)
                    {
                        // 创建对象
                        CoreSegment coreSegment
                            = new CoreSegment();
                        // 检查参数
                        if (value.Length <= 1)
                            coreSegment.Gamma = 1.0f;
                        else
                        {
                            // 设置参数
                            coreSegment.Gamma =
                                CoreCache.GetGamma(value);
                        }
                        // 获得信息熵
                        coreSegment.Entropy = Entropy.GetEntropy(count);
                        // 设置参数
                        coreSegment.Index = i; coreSegment.Length = j;
                        coreSegment.Value = value; coreSegment.Count = count;
                        // 加入列表
                        segments.Add(coreSegment);
                    }
                }
            }
            // 排序（按照信息熵排序）
            segments = segments.OrderByDescending(s => s.Entropy).ToList();
            // 检查结果
            // 没有查询到结果
            if (segments.Count <= 0)
            {
                // 创建数组
                segment.Subsegs =
                    new CoreSegment[segment.Length];
                // 设置数值
                segment.Gamma =
                    GammaTool.GetGamma(segment.Value);
                // 循环赋值
                for (int i = 0; i < segment.Length; i++)
                {
                    // 创建对象
                    segment.Subsegs[i] = new CoreSegment();
                    // 设置参数
                    segment.Subsegs[i].Length = 1;
                    segment.Subsegs[i].Index = segment.Index + i;
                    segment.Subsegs[i].Value = segment.Value.Substring(i, 1);
                    // 设置参数
                    segment.Subsegs[i].Gamma = 1.0f;
                    segment.Subsegs[i].Count =
                        TokenCache.GetCount(segment.Value[0]);
                    // 检查结果
                    if (segment.Subsegs[i].Count <= 0) segment.Subsegs[i].Count = 1;
                }
                // 返回结果
                return;
            }

            // 获得计数最小值者
            CoreSegment middleSegment = segments.First();
            // 清理链表
            segments.Clear();
            // 检查结果
            if (middleSegment.Index > 0)
            {
                // 创建对象
                CoreSegment leftSegment = new CoreSegment();
                // 设置参数
                leftSegment.Index = 0;
                leftSegment.Length = middleSegment.Index;
                leftSegment.Value = segment.Value.Substring(0, leftSegment.Length);
                // 加入对象
                segments.Add(leftSegment);
            }
            // 加入对象
            segments.Add(middleSegment);
            // 检查结果
            if (middleSegment.Index + middleSegment.Length < segment.Value.Length)
            {
                // 创建对象
                CoreSegment rightSegment = new CoreSegment();
                // 设置参数
                rightSegment.Index = middleSegment.Index + middleSegment.Length;
                rightSegment.Length = segment.Length - rightSegment.Index;
                rightSegment.Value = segment.Value.Substring(rightSegment.Index);
                // 加入对象
                segments.Add(rightSegment);
            }
            // 获得数组
            segment.Subsegs = segments.ToArray();
            // 循环递归
            foreach (CoreSegment item in segment.Subsegs)
            {
                if (item != middleSegment) MaxSplit(item);
            }
        }
    }
}
