using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace NLP
{
    public class CoreMatcher
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

            // 生成对象
            List<CoreSegment> segments =
                new List<CoreSegment>();

            // 获得总数
            segment.Count = CounterTool.GetCount(content, true);

            // 循环处理
            for (int i = 0; i <= content.Length; i++)
            {
                // 生成对象
                CoreSegment coreSegment
                    = new CoreSegment();
                // 设置索引
                coreSegment.Index = i;
                // 设置方法标记
                coreSegment.Flag = 0x80 | (i & 0x7F);
                // 设置总数
                coreSegment.Count = segment.Count;
                // 设置内容
                coreSegment.Value = content;
                // 设置长度
                coreSegment.Length = content.Length;
                // 获得路径
                if (MaxMatch(coreSegment, content, i))
                {
                    // 检查重复情况
                    if (!segments.Any(item => item.Equals(coreSegment)))
                    {
                        // 获得Gamma数值
                        coreSegment.Gamma = coreSegment.GetGamma();
                        // 检查结果
                        if (coreSegment.Gamma > 0.0f) segments.Add(coreSegment);
                    }
                }
            }

            // 返回结果（逆序）
            segment.Subsegs = segments.OrderByDescending(s => s.Gamma).ToArray();
            // 返回结果
            return segment;
        }

        private static bool MaxMatch(CoreSegment segment, string content, int index)
        {
#if DEBUG
            Debug.Assert(segment != null && !string.IsNullOrEmpty(content));
#endif
            // 检查参数
            if (index < 0 || index > content.Length) return false;

            // 设置参数
            segment.Index = index;
            segment.Value = content;
            segment.Length = content.Length;
            //segment.Count = CounterTool.GetCount(content, true);

            // 生成链表
            List<CoreSegment> segments = new List<CoreSegment>();

            // 往左实施最大逆向匹配法
            for (int i = index - 1; i >= 0; i--)
            {
                for (int j = 0; j <= i; j++)
                {
                    // 长度
                    int length = i - j + 1;
                    // 截取字符串
                    string value =
                        content.Substring(j, length);
                    // 检查结果
                    if (value == null ||
                        value.Length != length) continue;
                    // 查询结果
                    // 先查询Count
                    int count = CounterTool.GetCount(value, false);
                    // 检查结果
                    if (count <= 0) continue;
                    // 创建对象
                    CoreSegment coreSegment = new CoreSegment();
                    // 设置参数
                    coreSegment.Index = j;
                    coreSegment.Count = count;
                    coreSegment.Value = value;
                    coreSegment.Length = length;
                    coreSegment.Gamma = length <= 1 ?
                        1.0f : CoreCache.GetGamma(value);
                    // 加入链表
                    segments.Add(coreSegment); i = j; break;
                }
            }

            // 逆序
            segments = segments.OrderBy(s => s.Index).ToList();

            // 往右实施最大逆向匹配法
            for (int i = index; i < content.Length; i++)
            {
                for (int j = content.Length - 1; j >= i; j--)
                {
                    // 长度
                    int length = j - i + 1;
                    // 截取字符串
                    string value =
                        content.Substring(i, length);
                    // 检查结果
                    if (value == null ||
                        value.Length != length) continue;
                    // 查询结果
                    int count = CounterTool.GetCount(value, false);
                    // 检查结果
                    if (count <= 0) continue;
                    // 创建对象
                    CoreSegment coreSegment = new CoreSegment();
                    // 设置参数
                    coreSegment.Index = i;
                    coreSegment.Count = count;
                    coreSegment.Value = value;
                    coreSegment.Length = length;
                    coreSegment.Gamma = length <= 1 ?
                        1.0f : CoreCache.GetGamma(value);
                    // 加入链表
                    segments.Add(coreSegment); i = j; break;
                }
            }

            // 设置参数
            segment.Subsegs = segments.ToArray();
            // 返回结果
            return segments.Count > 1 ? true : false;
        }
    }
}
