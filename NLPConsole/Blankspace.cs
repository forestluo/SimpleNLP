using System.Diagnostics;using System.Text.RegularExpressions;

namespace NLP
{
    public class Blankspace
    {
        // Unicode空白字符
        //private readonly static
        //    char[] BLANKSPACE_CHARS =
        //    {
        //        // Unicode characters with White_Space property
        //        (char)0x0009, (char)0x000A, (char)0x000B, (char)0x000C, (char)0x000D,
        //        (char)0x0020, (char)0x0085, (char)0x00A0, (char)0x1680, (char)0x2000,
        //        (char)0x2001, (char)0x2002, (char)0x2003, (char)0x2004, (char)0x2005,
        //        (char)0x2006, (char)0x2007, (char)0x2008, (char)0x2009, (char)0x200A,
        //        (char)0x2028, (char)0x2029, (char)0x202F, (char)0x205F, (char)0x3000,
        //        // Related Unicode characters without White_Space property
        //        (char)0x180E, (char)0x200B, (char)0x200C, (char)0x200D, (char)0x2060, (char)0xFEFF
        //    };

        public static bool IsInvisible(char value)
        {
            // Unicode不可见区域
            switch ((int)value)
            {
                case 0x1680:
                case 0x180E:
                case 0x2028:
                case 0x2029:
                case 0x202F:
                case 0x205F:
                case 0x2060:
                case 0x3000:
                case 0xFEFF:
                    return true;
            }
            // Unicode不可见区域
            if (value >= 0xD7B0 &&
                value <= 0xF8FF) return true;
            // Unicode不可见区域
            if (value >= 0xFFF0 &&
                value <= 0xFFFF) return true;
            // Unicode不可见区域
            if (value >= 0x2000 &&
                value <= 0x200D) return true;
            // 返回结果
            return value < 32 || value == 0x7F;
        }

        public static bool IsBlankspace(char value)
        {
            // 返回结果
            return value == 0x20 ? true : IsInvisible(value);
        }

        public static string ClearBlankspace(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 返回结果
            return Regex.Replace(value, @"\s", "");
        }

        public static string ClearInvisible(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 将不可见字符替换成空格
            return Regex.Replace(value, @"([\x00-\x1F]|\x7F|\u1680|\u180E|[\u2000-\u200D]|[\u2028-\u2029]|\u202F|[\u205F-\u2060]|\u3000|[\uD7B0-\uF8FF]|\uFEFF|[\uFFF0-\uFFFF])+", " ");
        }
    }
}
