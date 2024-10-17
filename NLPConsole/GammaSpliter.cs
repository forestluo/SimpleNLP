using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace NLP
{
    public class GammaSpliter
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
            segment.Count = CounterTool.GetCount(content, true);

            // 生成分支
            MaxSplit(segment);
            // 搜集分支
            List<CoreSegment> segments
                = new List<CoreSegment>();
            Collect(segment, segments);

            // 创建主要分支
            CoreSegment majorSegment
                = new CoreSegment();
            // 设置参数
            majorSegment.Index = 0;
            majorSegment.Flag = 0x200;
            majorSegment.Count = segment.Count;
            majorSegment.Value = segment.Value;
            majorSegment.Length = segment.Length;
            majorSegment.Subsegs = segments.ToArray();
            majorSegment.Gamma = majorSegment.GetGamma();
            // 检查结果
            if (segments.Count == 1)
            {
                segment.Gamma = majorSegment.Gamma;
                segment.Count = majorSegment.Count;
            }

            // 创建次要分支
            CoreSegment minorSegment
                = new CoreSegment();
            // 设置参数
            minorSegment.Index = 0;
            minorSegment.Flag = 0x400;
            minorSegment.Count = segment.Count;
            minorSegment.Value = segment.Value;
            minorSegment.Length = segment.Length;
            //合并路径
            minorSegment.Subsegs =
                MaxMerge(segments.ToArray());
            minorSegment.Gamma = minorSegment.GetGamma();

            // 检查路径是否重复
            if(majorSegment.Equals(minorSegment))
            {
                // 设置方法标记
                majorSegment.Flag |= minorSegment.Flag;
                // 设置最终路径
                segment.Subsegs = new CoreSegment[] {majorSegment};
            }
            else
            {
                // 设置最终路径
                segment.Subsegs = new CoreSegment[] { majorSegment, minorSegment };
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
                segment.Count = CounterTool.
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
                    int count = CounterTool.GetCount(value, false);
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
                        // 设置参数
                        coreSegment.Index = i; coreSegment.Length = j;
                        coreSegment.Value = value; coreSegment.Count = count;
                        // 加入列表
                        segments.Add(coreSegment);
                    }
                }
            }
            // 排序（按照Gamma数值最大）
            segments = segments.OrderByDescending(s => s.Gamma).ToList();
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

            // 获得Gamma最大者
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
        
        private static CoreSegment[] MaxMerge(CoreSegment[] segments)
        {
#if DEBUG
            Debug.Assert(segments != null);
#endif
            // 检查长度
            // 长度为1，不需要再合并
            if (segments.Length == 1) return segments;
            // 创建链表
            List<CoreSegment> subsegs = new List<CoreSegment>();
            // 获得长度
            int length = segments.Length;
            // 执行循环（至少要有一次分割）
            for (int j = 2; j < length; j++)
            {
                // 检查长度
                for (int i = 0; i <= length - j; i++)
                {
                    // 创建对象
                    CoreSegment segment =
                        new CoreSegment();
                    // 创建子对象
                    segment.Subsegs =
                        new CoreSegment[j];
                    // 创建对象
                    StringBuilder sb =
                        new StringBuilder();
                    // 拼接字符串
                    for (int k = 0; k < j; k ++)
                    {
                        // 设置子对象
                        segment.Subsegs[k] =
                            segments[i + k];
                        // 扩张字符串
                        sb.Append(segments[i + k].Value);
                    }
                    // 检查结果
                    //if(sb.Length > 2)
                    {
                        // 获得字符串
                        segment.Index = i;
                        segment.Length = sb.Length;
                        segment.Value = sb.ToString();
                        segment.Count =
                            CounterTool.GetCount(segment.Value, false);
                        // 检查结果
                        if (segment.Count > 0)
                        {
                            // 获得Gamma
                            segment.Gamma =
                                CoreCache.GetGamma(segment.Value);
                            // 检查结果
                            if (segment.Gamma > 0.0f) subsegs.Add(segment);
                        }
                    }
                }
            }
            // 检查结果
            // 已找不出可以合并的内容
            if (subsegs.Count <= 0) return segments;
            // 进行排序（按照Gamma数值）
            subsegs = subsegs.
                OrderByDescending(s => s.Gamma).ToList();
            // 获得最大的数值对应项
            CoreSegment maxSegment = subsegs.First(); subsegs.Clear();
#if DEBUG
            Debug.Assert(maxSegment != null);
            Debug.Assert(maxSegment.Subsegs != null);
#endif

            // 确认索引位置
            for (int i = 0;i < segments.Length;i ++)
            {
                // 检查列表
                if (segments[i] != maxSegment.Subsegs[0])
                {
                    // 加入到链表中
                    subsegs.Add(segments[i]);
                }
                else
                {
                    // 加入到链表中
                    subsegs.Add(maxSegment);
#if DEBUG
                    // 逐步确认
                    for(int j = 0;j < maxSegment.Subsegs.Length;j ++)
                    {
                        Debug.Assert(segments[i + j] == maxSegment.Subsegs[j]);
                    }
#endif
                    // 增加索引
                    i += maxSegment.Subsegs.Length - 1;
                }
            }
            // 清理结果
            //maxSegment.Subsegs = null;
            // 返回结果
            return MaxMerge(subsegs.ToArray());
        }
    }
}
