using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NLP
{
    public class FilterTool
    {
        // 过滤规则
        private static readonly string[][] FILTER_RULES =
        {
            new string[] {"(\\u0020)\\s", " "},

            new string[] {"('){2,}", "'" },
            new string[] {"(`){2,}", "`" },
            new string[] {"(<){2,}", "<" },
            new string[] {"(>){2,}", ">" },
            new string[] {"(-){2,}", "—" },
            new string[] {"(、){2,}", "、" },
            new string[] {"(～){2,}", "～" },
            new string[] {"(—){2,}", "—" },

            new string[] {"(…){2,}", "…" },
            new string[] {"(\\.){3,}", "…" },

            new string[] {"，(，|：|\\s)*，", "，" },
            new string[] {"，(，|：|\\s)*：", "：" },
            new string[] {"，(，|：|\\s)*。", "。" },
            new string[] {"，(，|：|\\s)*；", "；" },
            new string[] {"，(，|：|\\s)*？", "？" },
            new string[] {"，(，|：|\\s)*！", "！" },

            new string[] {"：(，|：|\\s)*，", "：" },
            new string[] {"：(，|：|\\s)*：", "：" },
            new string[] {"：(，|：|\\s)*。", "。" },
            new string[] {"：(，|：|\\s)*；", "；" },
            new string[] {"：(，|：|\\s)*？", "？" },
            new string[] {"：(，|：|\\s)*！", "！" },

            new string[] {"。(，|：|。|；|？|！|\\s)+", "。" },
            new string[] {"；(，|：|。|；|？|！|\\s)+", "；" },
            new string[] {"？(，|：|。|；|？|！|\\s)+", "？" },
            new string[] {"！(，|：|。|；|？|！|\\s)+", "！" },

            new string[] {"<(br|hr|input)((\\s|\\.)*)/>", " " },
            new string[] {"<(img|doc|url|input)((\\s|\\.)*)>", " " },
            new string[] {"<[a-zA-Z]+\\s*[^>]*>(.*?)</[a-zA-Z]+>", "$1" },

            new string[] {"\\s(\\<|\\>|【|】|〈|〉|“|”|‘|’|《|》|\\(|\\)|（|）|［|］|｛|｝|…|～|—|、|？|！|；|。|：|，)", "$1" },
            new string[] {"(\\<|\\>|【|】|〈|〉|“|”|‘|’|《|》|\\(|\\)|（|）|［|］|｛|｝|…|～|—|、|？|！|；|。|：|，)\\s", "$1" }
        };

        public static string FilterContent(string content)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(content));
#endif
            // 参数
            bool flag;
            // 循环处理
            do
            {
                // 设置标记
                flag = false;
                // 循环处理
                foreach (string[] rule in FILTER_RULES)
                {
                    // 执行替换指令
                    string newContent = Regex.Replace(content, rule[0], rule[1]);
                    // 检查结果
                    if (!newContent.Equals(content)) { content = newContent; flag = true; }
                }

            } while (flag);
            // 返回结果
            return content;
        }

        public static string AdjustContent(string content)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(content));
#endif
            // 调整子符串
            content = AdjustContent(content, '\"');
            content = AdjustContent(content, '\'');
            // 返回结果
            return content;
        }

        private static string AdjustContent(string content, char value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(content));
#endif
            // 查询匹配
            Match match = Regex.Match(content,
                string.Format("(:|：)(\\s)*{0}", value));
            // 检查结果
            if (!match.Success) return content;
            // 获得索引
            int index = match.Index;

            // 声明参数
            bool flag = true;
            // 字符串
            StringBuilder sb = new StringBuilder();
            // 循环处理
            for (int i = index; i >= 0; i--)
            {
                // 检查字符
                if (content[i] != value)
                {
                    // 增加字符
                    sb.Append(content[i]);
                }
                else
                {
                    // 增加字符
                    flag = !flag; sb.Append(flag ? '“' : '”');
                }
            }

            // 获得输出
            string output = sb.ToString();
            // 反转字符串
            sb.Clear();
            // 反转字符串
            for (int i = output.Length - 1; i >= 0; i--) sb.Append(output[i]);

            // 声明参数
            flag = false;
            // 循环处理
            for (int i = index + 1; i < content.Length; i++)
            {
                // 检查字符
                if (content[i] != value)
                {
                    // 增加字符
                    sb.Append(content[i]);
                }
                else
                {
                    // 增加字符 
                    flag = !flag; sb.Append(flag ? '“' : '”');
                }
            }
            // 返回结果
            return sb.ToString();
        }
    }
}
