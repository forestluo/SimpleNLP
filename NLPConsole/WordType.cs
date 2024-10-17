using System.Diagnostics;

namespace NLP
{
    public class WordType
    {
        // 习惯用语
        public static readonly string IDIOM = "习惯用语";

        // 实词
        public static readonly string NOUN = "名词";
        public static readonly string VERB = "动词";
        public static readonly string PRONOUN = "代词";
        public static readonly string NUMERAL = "数词";
        public static readonly string LOCATIVE = "方位词";
        public static readonly string QUANTITY = "数量词";
        public static readonly string ADJECTIVE = "形容词";
        public static readonly string QUANTIFIER = "量词";
        // 实词类型
        public static readonly string[] REAL_TYPES =
        {
            NOUN, PRONOUN, VERB, ADJECTIVE,
            NUMERAL, QUANTIFIER, QUANTITY, LOCATIVE, 
        };

        // 虚词
        public static readonly string ADVERB = "副词";
        public static readonly string PADDING = "衬词";
        public static readonly string AUXILIARY = "助词";
        public static readonly string PREPOSITION = "介词";
        public static readonly string CONJUNCTION = "连词";
        public static readonly string EXCLAMATION = "感叹词";
        public static readonly string ONOMATOPOEIA = "拟声词";
        // 虚词类型
        public static readonly string[] FUNCTION_TYPES =
        {
            ADVERB, CONJUNCTION, PREPOSITION,
            AUXILIARY, PADDING, EXCLAMATION, ONOMATOPOEIA,
        };

        // 细节分类
        private static string[][] CLASSIFICATIONS =
        {
            new string[] {PADDING,
                            "区别", "前缀", "后缀"},

            new string[] {IDIOM,
                            "简称", "缩写", "方言", "口语", "成语", "歇后语"},

            new string[] {VERB,
                            "行为", "心理", "能愿", "趋向", "判断", "联系"},

            new string[] {NOUN,
                            "时间",
                            "方位", "处所", "地名",
                            "姓氏", "人名", "亲近名", "英文名", "日文名", "中文名",
                            "团体机构", "团体名", "机构名", "公司名", "组织名",
                            "专用", "动物名", "植物名", "药品名", "食品名", "商标名"},

            new string[] {ADJECTIVE,
                            "形状", "颜色", "状态", "性质"},

            new string[] {ADVERB,
                            "时间", "频率", "范围", "语气", "程度", "否定", "补充" },

            new string[] {PRONOUN,
                            "定指", "不指定", "人称", "疑问", "指示"},

            new string[] {PREPOSITION,
                            "处所方向", "时间起止", "状态方式", "原因", "目的", "比较", "排除"},

            new string[] {CONJUNCTION,
                            "并列", "选择", "转折", "承接", "递进", "因果", "假设", "条件"},

            new string[] {NUMERAL,
                            "基数", "序数", "分数", "倍数", "约数"},

            new string[] {QUANTIFIER,
                            "名量", "动量"},

            new string[] {AUXILIARY,
                            "结构", "动态", "语气"},

            new string[] {EXCLAMATION,
                            "喜悦", "悲痛", "愤怒", "惊讶", "呼唤", "应答"},

            new string[] {ONOMATOPOEIA,
                            "动物", "植物", "天气", "动作"},

            new string[] {QUANTITY,
                            "时间", "货币", "尺度", "体积", "面积", "重量", "温度"},
        };

        public static bool IsMajor(string major)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(major));
#endif
            foreach (string[] attribute in CLASSIFICATIONS)
            {
                if (major.Equals(attribute[0])) return true;
            }
            // 返回结果
            return false;
        }

        public static bool IsMinor(string minor)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(minor));
#endif
            foreach (string[] attribute in CLASSIFICATIONS)
            {
                for (int i = 1; i < attribute.Length; i++)
                {
                    if (minor.Equals(attribute[i])) return true;
                }
            }
            // 返回结果
            return false;
        }
    }
}
