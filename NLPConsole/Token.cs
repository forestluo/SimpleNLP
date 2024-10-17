namespace NLP
{
    public class Token
    {
#if _USE_CLR
        public static string GetDescription(char cToken)
#elif _USE_CONSOLE
        public static string? GetDescription(char cToken)
#endif
        {
            if (cToken <= 0x007F)
            {
                return "C0控制符及基本拉丁文";
            }
            else if (cToken <= 0x00FF) { return "C1控制符及拉丁文补充-1"; }
            else if (cToken <= 0x017F) { return "拉丁文扩展-A"; }
            else if (cToken <= 0x024F) { return "拉丁文扩展-B"; }
            else if (cToken <= 0x02AF) { return "国际音标扩展"; }
            else if (cToken <= 0x02FF) { return "空白修饰字母"; }
            else if (cToken <= 0x036F) { return "结合用读音符号"; }
            else if (cToken <= 0x03FF) { return "希腊文及科普特文"; }
            else if (cToken <= 0x04FF) { return "西里尔字母"; }
            else if (cToken <= 0x052F) { return "西里尔字母补充"; }
            else if (cToken <= 0x058F) { return "亚美尼亚语"; }
            else if (cToken <= 0x05FF) { return "希伯来文"; }
            else if (cToken <= 0x06FF) { return "阿拉伯文"; }
            else if (cToken <= 0x074F) { return "叙利亚文"; }
            else if (cToken <= 0x077F) { return "阿拉伯文补充"; }
            else if (cToken <= 0x07BF) { return "马尔代夫语"; }
            else if (cToken <= 0x07FF) { return "西非书面語言"; }
            else if (cToken <= 0x085F) { return "阿维斯塔语及巴列维语"; }
            else if (cToken <= 0x087F) { return "Mandaic"; }
            else if (cToken <= 0x08AF) { return "撒马利亚语"; }
            else if (cToken <= 0x097F) { return "天城文书"; }
            else if (cToken <= 0x09FF) { return "孟加拉语"; }
            else if (cToken <= 0x0A7F) { return "锡克教文"; }
            else if (cToken <= 0x0AFF) { return "古吉拉特文"; }
            else if (cToken <= 0x0B7F) { return "奥里亚文"; }
            else if (cToken <= 0x0BFF) { return "泰米尔文"; }
            else if (cToken <= 0x0C7F) { return "泰卢固文"; }
            else if (cToken <= 0x0CFF) { return "卡纳达文"; }
            else if (cToken <= 0x0D7F) { return "德拉维族语"; }
            else if (cToken <= 0x0DFF) { return "僧伽罗语"; }
            else if (cToken <= 0x0E7F) { return "泰文"; }
            else if (cToken <= 0x0EFF) { return "老挝文"; }
            else if (cToken <= 0x0FFF) { return "藏文"; }
            else if (cToken <= 0x109F) { return "缅甸语"; }
            else if (cToken <= 0x10FF) { return "格鲁吉亚语"; }
            else if (cToken <= 0x11FF) { return "朝鲜文"; }
            else if (cToken <= 0x137F) { return "埃塞俄比亚语"; }
            else if (cToken <= 0x139F) { return "埃塞俄比亚语补充"; }
            else if (cToken <= 0x13FF) { return "切罗基语"; }
            else if (cToken <= 0x167F) { return "统一加拿大土著语音节"; }
            else if (cToken <= 0x169F) { return "欧甘字母"; }
            else if (cToken <= 0x16FF) { return "如尼文"; }
            else if (cToken <= 0x171F) { return "塔加拉语"; }
            else if (cToken <= 0x173F) { return "Hanunóo"; }
            else if (cToken <= 0x175F) { return "Buhid"; }
            else if (cToken <= 0x177F) { return "Tagbanwa"; }
            else if (cToken <= 0x17FF) { return "高棉语"; }
            else if (cToken <= 0x18AF) { return "蒙古文"; }
            else if (cToken <= 0x18FF) { return "Cham"; }
            else if (cToken <= 0x194F) { return "Limbu"; }
            else if (cToken <= 0x197F) { return "德宏泰语"; }
            else if (cToken <= 0x19DF) { return "新傣仂语"; }
            else if (cToken <= 0x19FF) { return "高棉语记号"; }
            else if (cToken <= 0x1A1F) { return "Buginese"; }
            else if (cToken <= 0x1A5F) { return "Batak"; }
            else if (cToken <= 0x1AEF) { return "Lanna"; }
            else if (cToken <= 0x1B7F) { return "巴厘语"; }
            else if (cToken <= 0x1BB0) { return "巽他语"; }
            else if (cToken <= 0x1BFF) { return "Pahawh Hmong"; }
            else if (cToken <= 0x1C4F) { return "雷布查语"; }
            else if (cToken <= 0x1C7F) { return "Ol Chiki"; }
            else if (cToken <= 0x1CDF) { return "曼尼普尔语"; }
            else if (cToken <= 0x1D7F) { return "语音学扩展"; }
            else if (cToken <= 0x1DBF) { return "语音学扩展补充"; }
            else if (cToken <= 0x1DFF) { return "结合用读音符号补充"; }
            else if (cToken <= 0x1EFF) { return "拉丁文扩充附加"; }
            else if (cToken <= 0x1FFF) { return "希腊语扩充"; }
            else if (cToken <= 0x206F) { return "常用标点"; }
            else if (cToken <= 0x209F) { return "上标及下标"; }
            // 货币符号
            else if (cToken <= 0x20CF) { return "货币符号"; }
            else if (cToken <= 0x20FF) { return "组合用记号"; }
            else if (cToken <= 0x214F) { return "字母式符号"; }
            else if (cToken <= 0x218F) { return "数字形式"; }
            else if (cToken <= 0x21FF) { return "箭头"; }
            else if (cToken <= 0x22FF) { return "数学运算符"; }
            else if (cToken <= 0x23FF) { return "杂项工业符号"; }
            else if (cToken <= 0x243F) { return "控制图片"; }
            else if (cToken <= 0x245F) { return "光学识别符"; }
            else if (cToken <= 0x24FF) { return "封闭式字母数字"; }
            else if (cToken <= 0x257F) { return "制表符"; }
            else if (cToken <= 0x259F) { return "方块元素"; }
            else if (cToken <= 0x25FF) { return "几何图形"; }
            else if (cToken <= 0x26FF) { return "杂项符号"; }
            else if (cToken <= 0x27BF) { return "印刷符号"; }
            else if (cToken <= 0x27EF) { return "杂项数学符号-A"; }
            else if (cToken <= 0x27FF) { return "追加箭头-A"; }
            else if (cToken <= 0x28FF) { return "盲文点字模型"; }
            else if (cToken <= 0x297F) { return "追加箭头-B"; }
            else if (cToken <= 0x29FF) { return "杂项数学符号-B"; }
            else if (cToken <= 0x2AFF) { return "追加数学运算符"; }
            else if (cToken <= 0x2BFF) { return "杂项符号和箭头"; }
            else if (cToken <= 0x2C5F) { return "格拉哥里字母"; }
            else if (cToken <= 0x2C7F) { return "拉丁文扩展-C"; }
            else if (cToken <= 0x2CFF) { return "古埃及语"; }
            else if (cToken <= 0x2D2F) { return "格鲁吉亚语补充"; }
            else if (cToken <= 0x2D7F) { return "提非纳文"; }
            else if (cToken <= 0x2DDF) { return "埃塞俄比亚语扩展"; }
            else if (cToken <= 0x2E7F) { return "追加标点"; }
            else if (cToken <= 0x2EFF) { return "CJK 部首补充"; }
            else if (cToken <= 0x2FDF) { return "康熙字典部首"; }
            else if (cToken <= 0x2FFF) { return "表意文字描述符"; }
            else if (cToken <= 0x303F) { return "CJK 符号和标点"; }
            else if (cToken <= 0x309F) { return "日文平假名"; }
            else if (cToken <= 0x30FF) { return "日文片假名"; }
            else if (cToken <= 0x312F) { return "注音字母"; }
            else if (cToken <= 0x318F) { return "朝鲜文兼容字母"; }
            else if (cToken <= 0x319F) { return "象形字注释标志"; }
            else if (cToken <= 0x31BF) { return "注音字母扩展"; }
            else if (cToken <= 0x31EF) { return "CJK 笔画"; }
            else if (cToken <= 0x31FF) { return "日文片假名语音扩展"; }
            else if (cToken <= 0x32FF) { return "封闭式 CJK 文字和月份"; }
            else if (cToken <= 0x33FF) { return "CJK 兼容"; }
            else if (cToken <= 0x4DBF) { return "CJK 统一表意符号扩展 A"; }
            else if (cToken <= 0x4DFF) { return "易经六十四卦符号"; }
            // 基础汉字
            else if (cToken <= 0x9FBF) { return "CJK 统一表意符号"; }
            else if (cToken <= 0xA48F) { return "彝文音节"; }
            else if (cToken <= 0xA4CF) { return "彝文字根"; }
            else if (cToken <= 0xA61F) { return "Vai"; }
            else if (cToken <= 0xA6FF) { return "统一加拿大土著语音节补充"; }
            else if (cToken <= 0xA71F) { return "声调修饰字母"; }
            else if (cToken <= 0xA7FF) { return "拉丁文扩展-D"; }
            else if (cToken <= 0xA82F) { return "Syloti Nagri"; }
            else if (cToken <= 0xA87F) { return "八思巴字"; }
            else if (cToken <= 0xA8DF) { return "Saurashtra"; }
            else if (cToken <= 0xA97F) { return "爪哇语"; }
            else if (cToken <= 0xA9DF) { return "Chakma"; }
            else if (cToken <= 0xAA3F) { return "Varang Kshiti"; }
            else if (cToken <= 0xAA6F) { return "Sorang Sompeng"; }
            else if (cToken <= 0xAADF) { return "Newari"; }
            else if (cToken <= 0xAB5F) { return "越南傣语"; }
            else if (cToken <= 0xABA0) { return "Kayah Li"; }
            else if (cToken <= 0xD7AF) { return "朝鲜文音节"; }
            // 不可见字符
            else if (cToken <= 0xDBFF) { return "High-half zone of UTF-16"; }
            // 不可见字符
            else if (cToken <= 0xDFFF) { return "Low-half zone of UTF-16"; }
            else if (cToken <= 0xF8FF) { return "自行使用区域"; }
            else if (cToken <= 0xFAFF) { return "CJK 兼容象形文字"; }
            else if (cToken <= 0xFB4F) { return "字母表達形式"; }
            else if (cToken <= 0xFDFF) { return "阿拉伯表達形式A"; }
            else if (cToken <= 0xFE0F) { return "变量选择符"; }
            else if (cToken <= 0xFE1F) { return "竖排形式"; }
            else if (cToken <= 0xFE2F) { return "组合用半符号"; }
            else if (cToken <= 0xFE4F) { return "CJK 兼容形式"; }
            else if (cToken <= 0xFE6F) { return "小型变体形式"; }
            else if (cToken <= 0xFEFF) { return "阿拉伯表達形式B"; }
            else if (cToken <= 0xFFEF) { return "半型及全型形式"; }
            // 不可见字符
            else if (cToken <= 0xFFFF) { return "特殊"; }
            // 返回结果
            return null;
        }
    }
}
