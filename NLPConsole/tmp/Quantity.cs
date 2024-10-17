using System.Text;
using System.Linq;
using System.Diagnostics;

namespace NLP
{
    public class Quantity
    {
        private static readonly string[] UNIT_SYMBOLS =
        new string[]
        {
            "′", "″", "°", "A", "Bq", "C", "℃", "cal", "cm", "d",
            "db", "ev", "F", "℉", "g", "Gy", "H", "hz", "j", "K", "kg",
            "km", "kn", "kpa", "kw", "kwh", "kΩ", "l", "lm", "lx", "m",
            "mg", "min", "ml", "mm", "ms", "mΩ", "n", "nm", "ns", "pa",
            "ps", "rad", "s", "sr", "Sv", "T", "tex", "u", "V", "w", "wb",
            "um", "us", "Ω"
        }.OrderByDescending(s => s.Length).ToArray();

        private static readonly string[] UNIT_NAMES =
        new string[]
        {
            "埃", "安", "安培", "盎司", "巴", "百克", "百米", "百帕", "百升",
            "磅", "贝", "贝尔", "标准大气压", "长吨", "尺", "寸", "打兰", "大气压",
            "担", "电子伏特", "斗", "度", "短吨", "吨", "吨标准煤", "吨当量煤",
            "尔格", "法拉", "费密", "分贝", "分克", "分米", "分升", "分钟",
            "弗隆", "伏", "伏安", "伏特", "格令", "公斤", "公里", "公亩", "公顷",
            "光年", "海里", "毫", "毫安", "毫巴", "毫伏", "毫克", "毫米", "毫米汞柱",
            "毫米水柱", "毫升", "合", "赫兹", "亨", "亨利", "弧度", "华氏度", "及耳",
            "加仑", "焦耳", "角分", "角秒", "节", "斤", "开尔文", "开氏度", "坎德拉",
            "克", "克拉", "夸脱", "兰氏度", "勒", "勒克斯", "厘", "厘克", "厘米",
            "厘升", "里", "立方", "立方分米", "立方厘米", "立方码", "立方米", "立方英尺",
            "立方英寸", "两", "量滴", "列氏度", "流", "流明", "马力", "码", "美担", "米",
            "米制马力", "密耳", "秒", "摩尔", "亩", "纳法", "纳米", "奈培", "年", "牛顿",
            "欧", "欧姆", "帕", "帕斯卡", "配克", "皮法", "皮米", "品脱", "平方尺", "平方寸",
            "平方分", "平方分米", "平方公里", "平方毫米", "平方厘", "平方厘米", "平方里",
            "平方码", "平方米", "平方千米", "平方英尺", "平方英寸", "平方英里", "平方丈",
            "蒲式耳", "千伏", "千卡", "千克", "千米", "千帕", "千升", "千瓦", "钱", "顷",
            "球面度", "勺", "摄氏度", "升", "十克", "十米", "十升", "石", "人",
            "市尺", "市斤", "市里", "市亩", "市升", "特", "特克斯", "特斯拉", "天文单位",
            "桶", "瓦", "万伏", "微法", "微克", "微米", "微升", "韦", "韦伯", "西", "西门子",
            "像素", "小时", "英磅", "英尺", "英寸", "英担", "英里", "英亩", "英钱", "英石",
            "英寻", "英制马力", "丈", "兆瓦", "转"
        }.OrderByDescending(s => s.Length).ToArray();

        private static readonly string[] QUANTIFIER_NAMES =
        new string[]
        {
            "把", "班", "般", "板", "版", "瓣", "帮", "包", "抱", "杯", "辈", "辈子", "本",
            "笔", "边", "彪", "柄", "拨", "钵", "簸箕", "部", "部分", "步", "餐", "册", "层",
            "茬", "场", "车", "车皮", "匙", "池", "出", "处", "船", "串", "床", "丛", "簇",
            "沓", "代", "袋", "担", "刀", "道", "等", "滴", "点", "碟", "叠", "顶", "锭", "栋",
            "兜", "蔸", "堵", "段", "堆", "对", "队", "墩", "囤", "朵", "垛", "驮", "发", "番",
            "方", "房", "分", "份", "封", "幅", "副", "服", "竿", "杆", "缸", "格", "根",
            "钩", "股", "挂", "管", "罐", "贯", "桄", "锅", "行", "号", "盒", "痕", "泓", "壶",
            "户", "环", "伙", "辑", "集", "级", "剂", "季", "家", "夹", "架", "驾", "间", "肩",
            "件", "角", "窖", "截", "节", "介", "届", "进", "茎", "局", "具", "句", "卷", "开",
            "窠", "棵", "颗", "孔", "口", "块", "筐", "捆", "栏", "篮", "揽子", "粒", "例", "辆",
            "列", "领", "流", "绺", "溜", "笼", "垄", "篓", "炉", "路", "缕", "轮", "箩", "摞",
            "码", "脉", "毛", "枚", "门", "面", "名", "抹", "幕", "年", "排", "派", "盘", "泡",
            "盆", "棚", "捧", "批", "匹", "篇", "片", "瓢", "撇", "瓶", "期", "畦", "起", "腔",
            "墙", "锹", "丘", "曲", "阕", "群", "任", "色", "扇", "勺", "哨", "身", "声", "乘",
            "首", "手", "树", "束", "双", "丝", "艘", "所", "台", "抬", "滩", "摊", "坛", "潭",
            "塘", "堂", "樘", "套", "提", "屉", "挑", "条", "贴", "帖", "听", "挺", "通", "筒",
            "头", "团", "坨", "弯", "丸", "汪", "网", "尾", "味", "位", "瓮", "窝", "握", "席",
            "袭", "匣", "线", "箱", "项", "些", "宿", "眼", "页", "叶", "印张", "羽", "园", "员",
            "扎", "则", "盏", "章", "张", "着", "折", "针", "帧", "支", "枝", "纸", "盅", "站",
            "轴", "株", "注", "炷", "柱", "桩", "幢", "桌", "宗", "组", "嘴", "尊", "樽", "撮",
            "座", "平", "次", "重", "居", "个", "只", "岁", "种", "档", "类", "天", "日", "倍",
            "界",
        }.OrderByDescending(s => s.Length).ToArray();

        private static readonly string[] CURRENCY_NAMES =
        new string[]
        {
            "阿富汗尼", "埃斯库多", "澳大利亚元", "澳元", "巴波亚", "镑", "比尔", "比塞塔", "比索",
            "玻利瓦", "达拉西", "丹麦克郎", "迪拉姆", "第纳尔", "盾", "多步拉", "法郎", "菲律宾比索",
            "分", "福林", "港元", "格查尔", "古德", "瓜拉尼", "韩国元", "韩元", "基纳", "基普", "加拿大元",
            "加元", "角", "科多巴", "科朗", "克郎", "克鲁塞罗", "克瓦查", "宽札", "拉菲亚", "兰特",
            "厘", "里拉", "里兰吉尼", "里亚尔", "利昂", "列弗", "列克", "列伊", "卢布", "伦皮拉", "洛蒂",
            "马克", "梅蒂卡尔", "美分", "美元", "奈拉", "努扎姆", "挪威克郎", "欧元", "潘加", "普拉", "人民币",
            "日元", "瑞典克郎", "瑞士法郎", "塞迪", "苏克雷", "索尔", "塔卡", "塔拉", "台币", "泰铢", "图格里克",
            "瓦图", "乌吉亚", "先令", "谢克尔", "新加坡元", "新台币", "新西兰元", "新元", "印度卢布", "英镑",
            "元", "越南盾", "扎伊尔", "铢","兹罗提"
        }.OrderByDescending(s => s.Length).ToArray();

        public static string[] GetUnits()
        {
            return UNIT_NAMES;
        }

        public static string[] GetCurrencies()
        {
            return CURRENCY_NAMES;
        }

        public static string[] GetQuantifiers()
        {
            return QUANTIFIER_NAMES;
        }

        public static string RecoverRule(string rule)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(rule));
#endif

            if (rule.IndexOf("$a") >= 0)
                rule = rule.Replace("$a", GetRuleString("$a"));
            if (rule.IndexOf("$b") >= 0)
                rule = rule.Replace("$b", GetRuleString("$b"));
            if (rule.IndexOf("$c") >= 0)
                rule = rule.Replace("$c", GetRuleString("$c"));
            if (rule.IndexOf("$d") >= 0)
                rule = rule.Replace("$d", GetRuleString("$d"));
            if (rule.IndexOf("$e") >= 0)
                rule = rule.Replace("$e", GetRuleString("$e"));
            if (rule.IndexOf("$f") >= 0)
                rule = rule.Replace("$f", GetRuleString("$f"));
            if (rule.IndexOf("$h") >= 0)
                rule = rule.Replace("$h", GetRuleString("$h"));
            if (rule.IndexOf("$n") >= 0)
                rule = rule.Replace("$n", GetRuleString("$n"));
            if (rule.IndexOf("$s") >= 0)
                rule = rule.Replace("$s", GetRuleString("$s"));

            if (rule.IndexOf("$q") >= 0)
                rule = rule.Replace("$q", GetRuleString("$q"));
            if (rule.IndexOf("$u") >= 0)
                rule = rule.Replace("$u", GetRuleString("$u"));
            if (rule.IndexOf("$v") >= 0)
                rule = rule.Replace("$v", GetRuleString("$v"));
            if (rule.IndexOf("$y") >= 0)
                rule = rule.Replace("$y", GetRuleString("$y"));
            if (rule.IndexOf("$z") >= 0)
                rule = rule.Replace("$z", GetRuleString("$z"));
            // 返回结果
            return rule;
        }

#if _USE_CLR
        public static string GetRuleString(string type)
#elif _USE_CONSOLE
        public static string? GetRuleString(string type)
#endif
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(type));
#endif
            // 检查类型
            if (type == "$a")
            {
                // 返回结果
                return "(((?!(，|：|；|、|…|—|《|》|\\。|\\？|\\！|\\s)).)*)";
            }
            // 检查类型
            if (type == "$b")
            {
                // 返回结果
                return "(((?!(，|：|；|、|…|—|《|》|\\。|\\？|\\！|\\s)).)+)";
            }
            // 检查类型
            if (type == "$c")
            {
                // 返回结果
                return "[一|二|三|四|五|六|七|八|九]";
            }
            // 检查类型
            if (type == "$d")
            {
                // 返回结果
                return "(-?[1-9]\\d*|0)";
            }
            // 检查类型
            if (type == "$e")
            {
                // 返回结果
                return "[A-Za-z]";
            }
            // 检查类型
            if (type == "$f")
            {
                // 返回结果
                return "(-?([1-9]\\d*|[1-9]\\d*\\.\\d+|0\\.\\d+))";
            }
            // 检查类型
            if (type == "$h")
            {
                // 返回结果
                return "[\u4E00-\u9FA5]";
            }
            // 检查类型
            if (type == "$n")
            {
                // 返回结果
                return "(\\d+)";
            }
            // 检查类型
            if (type == "$s")
            {
                // 返回结果
                return "[A-Za-z]+[A-Za-z'' ]*";
            }
            // 检查类型
            if (type == "$q")
            {
                // 创建对象
                StringBuilder sb = new StringBuilder();
                // 增加字符
                sb.Append('(');
                // 循环处理
                foreach (string s in QUANTIFIER_NAMES)
                {
                    sb.Append(string.Format("{0}|", s));
                }
                // 移除最后一个字符
                sb.Remove(sb.Length - 1, 1); sb.Append(')');
                // 返回结果
                return sb.ToString();
            }
            // 检查类型
            if (type == "$u")
            {
                // 创建对象
                StringBuilder sb = new StringBuilder();
                // 增加字符
                sb.Append('(');
                // 循环处理
                foreach (string s in UNIT_NAMES)
                {
                    sb.Append(string.Format("{0}|", s));
                }
                // 移除最后一个字符
                sb.Remove(sb.Length - 1, 1); sb.Append(')');
                // 返回结果
                return sb.ToString();
            }
            // 检查类型
            if (type == "$v")
            {
                // 创建对象
                StringBuilder sb = new StringBuilder();
                // 增加字符
                sb.Append('(');
                // 循环处理
                foreach (string s in UNIT_SYMBOLS)
                {
                    sb.Append(string.Format("{0}|", s));
                }
                // 移除最后一个字符
                sb.Remove(sb.Length - 1, 1); sb.Append(')');
                // 返回结果
                return sb.ToString();
            }
            // 检查类型
            if (type == "$y")
            {
                // 创建对象
                StringBuilder sb = new StringBuilder();
                // 增加字符
                sb.Append('(');
                // 循环处理
                foreach (string s in CURRENCY_NAMES)
                {
                    sb.Append(string.Format("{0}|", s));
                }
                // 移除最后一个字符
                sb.Remove(sb.Length - 1, 1); sb.Append(')');
                // 返回结果
                return sb.ToString();
            }
            //// 检查类型
            if (type == "$z")
            {
                // 返回结果
                return "([\u4E00-\u9FA5]+)";
            }
            // 检查类型
            if (type == "parameter")
            {
                // 返回结果
                return "(((?!(，|：|；|…|—|\\。|\\？|\\！)).)+)";
            }
            // 返回结果
            return null;
        }
    }
}
