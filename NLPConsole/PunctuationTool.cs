using System.Text;
using System.Diagnostics;

namespace NLP
{
    public class PunctuationTool
    {
        public static string WideConvert(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 创建字符串
            StringBuilder sb = new StringBuilder(value.Length);
            // 循环处理
            foreach (char item in value)
            {
                // 转换成全角
                switch (item)
                {
                    case ',':
                        sb.Append('，');
                        break;
                    case '∶':
                    case ':':
                        sb.Append('：');
                        break;
                    case ';':
                        sb.Append('；');
                        break;
                    case '?':
                        sb.Append('？');
                        break;
                    case '!':
                        sb.Append('！');
                        break;
                    case '〝':
                        sb.Append('“');
                        break;
                    case '〞':
                        sb.Append('”');
                        break;
                    case '「':
                        sb.Append('“');
                        break;
                    case '」':
                        sb.Append('”');
                        break;
                    case '『':
                        sb.Append('“');
                        break;
                    case '』':
                        sb.Append('”');
                        break;
                    //case '(':
                    //    sb.Append('（');
                    //    break;
                    //case ')':
                    //    sb.Append('）');
                    //    break;
                    //case '[':
                    //    sb.Append('［');
                    //    break;
                    //case ']':
                    //    sb.Append('］');
                    //    break;
                    //case '{':
                    //    sb.Append('｛');
                    //    break;
                    //case '}':
                    //    sb.Append('｝');
                    //    break;
                    default:
                        sb.Append(item);
                        break;
                }
            }
            // 返回结果
            return sb.ToString();
        }
    }
}
