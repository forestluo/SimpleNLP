using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NLP
{
    public class LocationName
    {
        public static string[] GetLocations() { return LOCATIONS; }

        public static bool IsLocationName(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            foreach(string location in LOCATIONS)
            {
                if (value.EndsWith(location)) return true;
            }
            // 返回结果
            return false;
        }

        private static readonly string[] LOCATIONS = new string[]
        {
            // 行政区划
            "域", "洲", "州", "国", "省", "市", "区", "乡", "镇", "村", "街", "屯", "郡", "坊", "坂", "畈", "巷", "片",
            // 高地
            "山", "丘", "岭", "崖", "台", "峰", "墩", "岗", "陂",
            // 岩石
            "岩", "界", "石", "角", "壁", "岚",
            // 沟壑
            "峪", "谷", "峡", "咀", "窝", "坑", "坳", "埚", "塆", "围", "川", "口", "垴", "槽",
            // 道路
            "路", "道", "门", "场", "桥", "站", "磜",
            // 水体
            "海", "湖", "江", "河", "溪", "泉", "沟", "水", "滩", "港", "湾", "沼", "泽", "池", "荡", "洼", "井", "闸", "涧", "塘", "潭", "畔",
            // 岛屿
            "岛", "屿", "港", "堤", "坞", "码头", "岸",
            // 洞穴
            "洞", "穴", "窟", "峒", "窑",
            // 建筑
            "城", "堡", "楼", "所", "苑", "库", "店", "墙", "坛", "馆", "庐", "垒", "关",
            // 社团
            "庄", "社", "家", "厂", "校", "营", "旗", "庄", "寨", "社", "院", "队", "斋", "处",
            // 房屋
            "府", "园", "屋", "堂", "室", "厅", "亭", "台", "阁", "庭", "房", "栈", "舱", "间", "厦", "寓",
            // 宗教
            "寺", "庙", "观", "庵", "陵", "塔", "坟", "祠", "碑", "祉", "冢",
            // 草木
            "地", "林", "田", "坡", "埂", "垄", "垅",
            // 广场
            "场", "原", "郊", "坪",
            // 南方地点
            "岌", "崀", "冚", "岽", "畲", "芨", "尾", "背", "头", "里", "冲",
        }.OrderByDescending(s => s.Length).ToArray();

        private static string GetRule()
        {
            // 创建对象
            StringBuilder sb = new StringBuilder();
            // 增加字符
            sb.Append("(");
            // 循环处理
            foreach (string location in LOCATIONS)
            {
                sb.Append(string.Format("{0}|", location));
            }
            // 移除最后一个字符
            sb.Remove(sb.Length - 1, 1); sb.Append(')');
            // 返回结果
            return sb.ToString();
        }

#if _USE_CLR
        public static CoreSegment[] Extract(string content)
#elif _USE_CONSOLE
        public static CoreSegment[]? Extract(string content)
#endif
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(content));
#endif
            // 生成对象
            List<CoreSegment> segments
                = new List<CoreSegment>();
            // 提取字符串
            Match match = Regex.Match(content, GetRule());
            // 循环处理
            while (match.Success)
            {
#if _USE_CLR
                CoreSegment
#elif _USE_CONSOLE
                CoreSegment?
#endif
                segment =
                    // 获得合适匹配项目
                    Extract(content, match.Index);
                // 检查结果
                if(segment != null) segments.Add(segment);
                // 下一个匹配项目
                match = match.NextMatch();
            }
            // 返回结果
            return segments.OrderBy(s => s.Index).ToArray();
        }

#if _USE_CLR
        private static CoreSegment Extract(string content, int index)
#elif _USE_CONSOLE
        private static CoreSegment? Extract(string content, int index)
#endif
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(content));
#endif
            // 生成对象
            List<CoreSegment> segments
                = new List<CoreSegment>();
            // 循环处理
            // 长度不能超过CounterContent的最大检测长度
            for (int i = index - 1;
                i >= 0 && index - i <= 5; i--)
            {
                // 截取字符串
                string value =
                    content.Substring(i, index - i + 1);
                // 检查内容
                // 中间包含了其他字符
                if (!ChineseTool.IsChinese(value)) break;
                // 检查数据
                if (CounterTool.GetCount(value, true) <= 0) break;
#if _USE_CLR
                CoreSegment
#elif _USE_CONSOLE
                CoreSegment?
#endif
                segment =
                    // 拆分字符串
                    CoreSegment.Split(value);
#if DEBUG
                Debug.Assert(segment != null);
#endif
                // 选择路径
                // 一般为R+算法，Gamma数值最大
                segment = segment.SelectPath();
#if DEBUG
                Debug.Assert(segment != null);
#endif
                // 加入到队列中
                if(segment.Gamma > 0) segments.Add(segment);
            }
            // 检查结果
            if (segments.Count <= 0) return null;
            // 选择Gamma数值最大者
            return segments.OrderByDescending(s => s.Gamma).First();
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
                CoreSegment[]? cses = Extract(content);
                // 检查结果
                if (cses == null || cses.Length <= 0)
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
                Console.WriteLine("打印LocationName.Extract(" + fid + ")内容！");
                Console.WriteLine("----------------------------------------");
                // 打印所有内容
                int index = 0;
                foreach (CoreSegment cs in
                    cses.OrderBy(s => s.Flag).ThenBy(s => s.Gamma).ToArray())
                {
                    // 检查结果
                    if (cs.IsLeaf()) continue;

                    LogTool.LogMessage(string.Format("\tsegment[{0}].Value = {1}", index, cs.Value));
                    LogTool.LogMessage(string.Format("\tsegment[{0}].Count = {1}", index, cs.Count));
                    LogTool.LogMessage(string.Format("\tsegment[{0}].Gamma = {1:0.00000000}", index, cs.Gamma)); index++;
                }

                // 随机往下
                fid = random.Next(maxID);
                // 等待键盘
                if (Console.ReadKey().Key == ConsoleKey.Escape) break;

            } while (true);
        }
#endif
    }
}
