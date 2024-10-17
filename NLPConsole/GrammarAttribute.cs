using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLP
{
    public class MarkAttribute
    {
        public int Index { set; get; }
#if _USE_CLR
        public string Value { get; set; }
        public string Remark { get; set; }
#elif _USE_CONSOLE
        public string? Value { get; set; }
        public string? Remark { get; set; }
#endif

        public bool IsNullOrEmpty()
        {
            // 返回结果
            return string.IsNullOrEmpty(Value);
        }
    }

    public class GrammarAttribute : MarkAttribute
    {
#if _USE_CLR
        public double[] Posibilities { get; set; }
#elif _USE_CONSOLE
        public double[]? Posibilities { get; set; }
#endif

        public new bool IsNullOrEmpty()
        {
            // 返回结果
            if ((this as MarkAttribute).IsNullOrEmpty()) return true;
            // 返回结果
            return Posibilities != null && Posibilities.Length == 13;
        }

        public double GetPosibility(GrammarType type)
        {
            // 检查参数
            if (Posibilities == null ||
                Posibilities.Length != 13) return -1.0;
            // 检查类型
            switch (type)
            {
            case GrammarType.名词:
                // 设置可能性
                return Posibilities[0];
            case GrammarType.动词:
                // 设置可能性
                return Posibilities[1];
            case GrammarType.代词:
                // 设置可能性
                return Posibilities[2];
            case GrammarType.数词:
                // 设置可能性
                return Posibilities[3];
            case GrammarType.形容词:
                // 设置可能性
                return Posibilities[4];
            case GrammarType.量词:
                // 设置可能性
                return Posibilities[5];
            case GrammarType.副词:
                // 设置可能性
                return Posibilities[6];
            case GrammarType.助词:
                // 设置可能性
                return Posibilities[7];
            case GrammarType.介词:
                // 设置可能性
                return Posibilities[8];
            case GrammarType.连词:
                // 设置可能性
                return Posibilities[9];
            case GrammarType.感叹词:
                // 设置可能性
                return Posibilities[10];
            case GrammarType.拟声词:
                // 设置可能性
                return Posibilities[11];
            }
            // 返回缺省值
            return Posibilities[12];
        }

        public void SetPosibility(GrammarType type, double posibility)
        {
            // 检查参数
            if (Posibilities == null ||
                Posibilities.Length != 13)
            {
                // 生成数组
                Posibilities = new double[13];
            }
            // 检查类型
            switch (type)
            {
            case GrammarType.名词:
                // 设置可能性
                Posibilities[0] = posibility; break;
            case GrammarType.动词:
                // 设置可能性
                Posibilities[1] = posibility; break;
            case GrammarType.代词:
                // 设置可能性
                Posibilities[2] = posibility; break;
            case GrammarType.数词:
                // 设置可能性
                Posibilities[3] = posibility; break;
            case GrammarType.形容词:
                // 设置可能性
                Posibilities[4] = posibility; break;
            case GrammarType.量词:
                // 设置可能性
                Posibilities[5] = posibility; break;
            case GrammarType.副词:
                // 设置可能性
                Posibilities[6] = posibility; break;
            case GrammarType.助词:
                // 设置可能性
                Posibilities[7] = posibility; break;
            case GrammarType.介词:
                // 设置可能性
                Posibilities[8] = posibility; break;
            case GrammarType.连词:
                // 设置可能性
                Posibilities[9] = posibility; break;
            case GrammarType.感叹词:
                // 设置可能性
                Posibilities[10] = posibility; break;
            case GrammarType.拟声词:
                // 设置可能性
                Posibilities[11] = posibility; break;
            default:
                // 设置可能性
                Posibilities[12] = posibility; break;
            }
        }
    }
}
