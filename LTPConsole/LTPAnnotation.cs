using System.Text.RegularExpressions;

namespace NLP
{
    public class LTPAnnotation
    {
        private static readonly string[] NES =
        {
            "Nh", "Ni", "Ns"
        };

        private static readonly string[][] RELATIONS =
        {
            new string[] { "SBV", "主谓关系" },
            new string[] { "VOB", "动宾关系" },
            new string[] { "IOB", "间宾关系" },
            new string[] { "FOB", "前置宾语" },
            new string[] { "DBL", "兼语" },
            new string[] { "ATT", "定中关系" },
            new string[] { "ADV", "状中结构" },
            new string[] { "CMP", "动补结构" },
            new string[] { "COO", "并列关系" },
            new string[] { "POB", "介宾关系" },
            new string[] { "LAD", "左附加关系" },
            new string[] { "RAD", "右附加关系" },
            new string[] { "IS", "独立结构" },
            new string[] { "HED", "核心关系" },
        };

        private static readonly string[][] DEPENDS =
        {
            // 反关系 r*
            new string[] { "r", "反关系" },
            // 嵌套关系 d*
            new string[] { "d", "嵌套关系" },

            new string[] { "AGT", "施事" },
            new string[] { "EXP", "当事" },
            new string[] { "PAT", "受事" },
            new string[] { "CONT", "客事" },
            new string[] { "DATV", "涉事" },
            new string[] { "LINK", "系事" },
            new string[] { "TOOL", "工具" },
            new string[] { "MATL", "材料" },
            new string[] { "MANN", "方式" },
            new string[] { "SCO", "范围" },
            new string[] { "REAS", "缘由" },
            new string[] { "TIME", "时间" },
            new string[] { "LOC", "空间" },
            new string[] { "MEAS", "度量" },
            new string[] { "STAT", "状态" },
            new string[] { "FEAT", "修饰" },

            new string[] { "eCOO", "并列关系" },
            new string[] { "ePREC", "先行关系" },
            new string[] { "eSUCC", "后继关系" },
            new string[] { "mPUNC", "标点标记" },
            new string[] { "mNEG", "否定标记" },
            new string[] { "mRELA", "关系标记" },
            new string[] { "mDEPD", "依附标记" },
        };

        private static readonly string[][] ROLES =
        {
            new string[] { "ARG0", "causers or experiencers" },
            new string[] { "ARG1", "patient" },
            new string[] { "ARG2", "range" },
            new string[] { "ARG3", "starting point" },
            new string[] { "ARG4", "end point" },
            new string[] { "ADV", "adverbial" },
            new string[] { "BNF", "beneficiary" },
            new string[] { "CND", "condition" },
            new string[] { "CRD", "coordinated arguments" },
            new string[] { "DGR", "degree" },
            new string[] { "DIR", "direction" },
            new string[] { "DIS", "discourse marker" },
            new string[] { "EXT", "extent" },
            new string[] { "FRQ", "frequency" },
            new string[] { "LOC", "locative" },
            new string[] { "MNR", "manner" },
            new string[] { "PRP", "purpose or reason" },
            new string[] { "QTY", "quantity" },
            new string[] { "TMP", "temporal" },
            new string[] { "TPC", "topic" },
            new string[] { "PRD", "predicate" },
            new string[] { "PSR", "possessor" },
            new string[] { "PSE", "possessee" },
        };

        private static readonly string[][] TAGS =
        {
            new string[] { "a", "adjective" },
            new string[] { "b", "other noun-modifier" },
            new string[] { "c", "conjunction" },
            new string[] { "d", "adverb" },
            new string[] { "e", "exclamation" },
            new string[] { "g", "morpheme" },
            new string[] { "h", "prefix" },
            new string[] { "i", "idiom" },
            new string[] { "j", "abbreviation" },
            new string[] { "k", "suffix" },
            new string[] { "m", "number" },
            new string[] { "n", "general noun" },
            new string[] { "nd", "direction noun" },
            new string[] { "nh", "person name" },
            new string[] { "ni", "organization name" },
            new string[] { "nl", "location noun" },
            new string[] { "ns", "geographical name" },
            new string[] { "nt", "temporal noun" },
            new string[] { "nz", "other proper noun" },
            new string[] { "o", "onomatopoeia" },
            new string[] { "p", "preposition" },
            new string[] { "q", "quantity" },
            new string[] { "r", "pronoun" },
            new string[] { "u", "auxiliary" },
            new string[] { "v", "verb" },
            new string[] { "wp", "punctuation" },
            new string[] { "ws", "foreign words" },
            new string[] { "x", "non-lexeme" },
            new string[] { "z", "descriptive words" }
        };
    }
}
