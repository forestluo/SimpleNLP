using System.Text;
using System.Diagnostics;

namespace NLP
{
    public class Punctuation
    {
        public static bool IsPunctuation(char value)
        {
            // 返回结果
            // 注意：不包含次要分隔符！！
            return IsMajorSplitter(value) || IsPairSplitter(value);
        }

        public static bool HasPunctuation(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            for (int i = 0; i < value.Length; i++)
            {
                if (IsPunctuation(value[i])) return true;
            }
            // 返回结果
            return false;
        }

        // 次要分割符
        private static readonly
            char[] MINOR_SPLITTERS = { '、', '—', '·' };
        // 主要分割符
        private static readonly
            char[] MAJOR_SPLITTERS = { '。', '，', '；', '：', '？', '！', '…' };

        public static string GetMajorSplitters()
        {
            return "，|。|；|：|？|！|…";
        }

        public static string GetEndMajorSplitters()
        {
            return "。|；|？|！|…";
        }

        public static string GetNEndMajorSplitters()
        {
            return "，|：";
        }

        public static bool IsEndMajorSplitter(char value)
        {
            if (value == '。'
                || value == '；' || value == '？'
                || value == '！' || value == '…') return true;
            // 返回结果
            return false;
        }

        public static bool IsNEndMajorSplitter(char value)
        {
            if (value == '，' || value == '：') return true;
            // 返回结果
            return false;
        }

        public static bool IsMinorSplitter(char value)
        {
            foreach (char item in MINOR_SPLITTERS)
            {
                if (value == item) return true;
            }
            // 返回结果
            return false;
        }

        public static bool IsMajorSplitter(char value)
        {
            foreach (char item in MAJOR_SPLITTERS)
            {
                if (value == item) return true;
            }
            // 返回结果
            return false;
        }

        public static bool HasMajorSplitter(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            foreach (char item in value)
            {
                if (IsMajorSplitter(item)) return true;
            }
            // 返回结果
            return false;
        }

        // 成对分割符
        private static readonly
            char[] PAIR_SPLITTERS = { '“', '”', '（', '）', '《', '》',
                                  '‘', '’', '【', '】', '〈', '〉',
                                  '「', '」', '『', '』', '〔', '〕',
                                  '〖', '〗', '〝', '〞', '﹙', '﹚',
                                  '﹛', '﹜', '﹝', '﹞', '﹤', '﹥',
                                  // 半角字符对应的全角符号
                                  '［', '］', '｛', '｝'
                                  /*'(', ')', '[', ']', '{', '}', '<', '>'*/};

        public static string GetPairEnds()
        {
            return "”|）|》|’|】|〉|」|』|〕|〗|〞|﹚|﹜|﹞|﹥|］|｝";
        }

        public static string GetPairStarts()
        {
            return "“|（|《|‘|【|〈|「|『|〔|〖|〝|﹙|﹛|﹝|﹤|［|｛";
        }

        public static bool IsNarEndSplitter(char value)
        {
            if (value == '”' || value == '’'
                || value == '」' || value == '』' || value == '〞') return true;
            // 返回结果
            return false;
        }

        public static bool IsNarStartSplitter(char value)
        {
            if (value == '“' || value == '‘'
                || value == '「' || value == '『' || value == '〝') return true;
            // 返回结果
            return false;
        }

        public static bool IsPairSplitter(char value)
        {
            foreach (char item in PAIR_SPLITTERS)
            {
                // 返回结果
                if (value == item) return true;
            }
            // 返回结果
            return false;
        }

        public static char GetPairEnd(char value)
        {
            for (int i = 0; i < PAIR_SPLITTERS.Length; i += 2)
            {
                // 返回结果
                if (PAIR_SPLITTERS[i] == value)
                    return PAIR_SPLITTERS[i + 1];
            }
            // 返回结果
            return value;
        }

        public static bool HasPairSplitter(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            foreach (char item in value)
            {
                if (IsPairSplitter(item)) return true;
            }
            // 返回结果
            return false;
        }

        public static bool IsPairEnd(char value)
        {
            for (int i = 1; i < PAIR_SPLITTERS.Length; i += 2)
            {
                // 返回结果
                if (PAIR_SPLITTERS[i] == value) return true;
            }
            // 返回结果
            return false;
        }

        public static bool IsPairStart(char value)
        {
            for (int i = 0; i < PAIR_SPLITTERS.Length; i += 2)
            {
                // 返回结果
                if (PAIR_SPLITTERS[i] == value) return true;
            }
            // 返回结果
            return false;
        }

        public static bool IsPairMatched(char start, char end)
        {
            for (int i = 0; i < PAIR_SPLITTERS.Length; i += 2)
            {
                // 返回结果
                if (PAIR_SPLITTERS[i] == start &&
                    PAIR_SPLITTERS[i + 1] == end) return true;
            }
            // 返回结果
            return false;
        }
    }
}
