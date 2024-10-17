using System;
using System.Text;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Runtime.Versioning;

namespace NLP
{
    public class ChineseTool
    {
        public static bool IsChinese(char value)
        {
            // 转换成整数
            int newValue = value & 0xFFFF;
            // 返回结果
            return newValue >= 0x4E00 && newValue <= 0x9FA5;
        }

        public static bool IsChinese(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (!IsChinese(value[i])) return false;
            }
            // 返回结果
            return true;
        }

        public static int NonChinese(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 逆向搜索非中文字符
            for (int i = value.Length - 1; i >= 0; i--)
            {
                if (!IsChinese(value[i])) return i;
            }
            // 返回结果
            return -1;
        }

#if _USE_CLR
        public static string TraditionalConvert(string value)
#elif _USE_CONSOLE
        public static string? TraditionalConvert(string value)
#endif
        {
#if DEBUG
            //Debug.Assert(OperatingSystem.IsWindows());
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 转繁体
            return Strings.StrConv(value, VbStrConv.TraditionalChinese, 0);
        }

#if _USE_CLR
        public static string SimplifiedConvert(string value)
#elif _USE_CONSOLE
        public static string? SimplifiedConvert(string value)
#endif
        {
#if DEBUG
            //Debug.Assert(OperatingSystem.IsWindows());
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 转简体
            return Strings.StrConv(value, VbStrConv.SimplifiedChinese, 0);
        }

        public static string WideConvert(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 创建字符串
            StringBuilder sb = new StringBuilder(value.Length);
            // 循环处理
            foreach (char c in value)
            {
                // 特殊处理
                if (c == 32) sb.Append((char)12288);
                // 检查字符范围
                else if (c < 33) sb.Append(c);
                else if (c > 126) sb.Append(c);
                // 转换成全角
                else sb.Append((char)(c + 65248));
            }
            // 返回结果
            return sb.ToString();

            // 半角转全角
            //return Strings.StrConv(value, VbStrConv.Wide, 0);
        }

        public static string NarrowConvert(string value)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif
            // 创建字符串
            StringBuilder sb = new StringBuilder(value.Length);
            // 循环处理
            foreach (char c in value)
            {
                // 特殊处理
                if (c == 12288) sb.Append(' ');
                // 检查字符范围
                else if (c < 65281) sb.Append(c);
                else if (c > 65374) sb.Append(c);
                // 转换成半角
                else sb.Append((char)(c - 65248));
            }
            //返回结果
            return sb.ToString();

            // 全角转半角
            //return Strings.StrConv(value, VbStrConv.Narrow, 0);
        }
    }
}
