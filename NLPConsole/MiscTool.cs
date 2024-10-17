using System;
using System.Linq;
using System.Data;
using System.Diagnostics;
using System.Collections;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NLP
{
    public class MiscTool
    {
        public static bool IsTooLong(string value)
        {
            // 返回结果
            return string.IsNullOrEmpty(value) ? false : value.Length > 450;
        }

#if _USE_CLR
        public static string ClearContent(string content)
#elif _USE_CONSOLE
        public static string? ClearContent(string content)
#endif
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(content));
#endif
            // 调整字符串
            content = FilterTool.AdjustContent(content);

            // 设置初始值
            int length = content.Length;
            // 循环处理
            while (true)
            {
                // 全角字符转半角字符
                content = ChineseTool.NarrowConvert(content);
                // 检查结果
                if (string.IsNullOrEmpty(content)) return null;

                // 清理不可见字符
                content = Blankspace.ClearInvisible(content);
                // 检查结果
                if (string.IsNullOrEmpty(content)) return null;

                // 循环处理
                int len;
                do
                {
                    // 获得字符串长度
                    len = content.Length;
                    // 转义XML
                    content = XMLTool.XMLUnescape(content);
                    // 检查结果
                    if (string.IsNullOrEmpty(content)) return null;

                } while (content.Length < len);

                // 检查结果
                if (content.Length >= length) break; else length = content.Length;
            }

            // 过滤字符串，替代不规则内容
            content = FilterTool.FilterContent(content);
            // 检查结果
            if(string.IsNullOrEmpty(content)) return null;

            // 半角转全角
            content = ChineseTool.WideConvert(content);
            // 检查结果
            if (string.IsNullOrEmpty(content)) return null;

            // 过滤字符串，替代不规则内容
            content = FilterTool.FilterContent(content);
            // 检查结果
            if (string.IsNullOrEmpty(content)) return null;

            // 全角转半角
            content = ChineseTool.NarrowConvert(content);
            // 检查结果
            if (string.IsNullOrEmpty(content)) return null;

            // 部分半角转全角
            content = PunctuationTool.WideConvert(content);
            // 检查结果
            if (string.IsNullOrEmpty(content)) return null;
            // 返回结果
            return content.Trim();
        }
    }
}
