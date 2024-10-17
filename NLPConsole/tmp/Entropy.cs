using System.Diagnostics;

namespace NLP
{
    public class Entropy
    {
        // 需要初始化
        private static double T;

        public static double GetEntropy(int count)
        {
#if DEBUG
            Debug.Assert(count > 0);
#endif
            // 返回结果
            return -1.0f * count / T * Math.Log(count / T);
        }

        public static double GetEntropy(string content)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(content));
#endif
            // 获得计数
            return GetEntropy(MiscTool.GetCount(content, true));
        }

        public static bool Initialize()
        {
            // 获得计数
            int count = TokenCache.GetCount('的');
            // 检查结果
            if(count <= 0)
            {
                // 打印日志
                LogTool.LogMessage("Entropy", "Initialize", "初始化失败！");
                return false;
            }

            try
            {
                // 计算T值
                T = GetTValue(count);
                // 检查结果
                if (T > 0)
                {
                    // 打印日志
                    LogTool.LogMessage("Entropy", "Initialize", "初始化成功！");
                    LogTool.LogMessage(String.Format("\tT = {0}", T)); return true;
                }
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("Entropy", "Initialize", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            // 打印日志
            LogTool.LogMessage("Entropy", "Initialize", "初始化失败！");
            // 返回结果
            return false;
        }

        private static double GetTValue(int count)
        {
            // 设置初始值
            double y = - 1.0f;
            double f = count;
            double x = f * 0.5f;
            // 循环处理
            do
            {
                // 获得下一个数值
                y = NextValue(x, f);
//#if DEBUG
//                Console.WriteLine(
//                    string.Format("\ty = {0}", y));
//#endif
                // 检查结果
                if (Math.Abs(y - x) <= 1.0e-8) break;
                // 传递参数
                x = y;

            } while (true);
            // 返回结果
            return y;
        }

        private static double NextValue(double x, double f)
        {
            // 计算中间值
            double value =
               Math.Log(f) - Math.Log(x);
            value = 1 + (value - x / f) / (value + 1);
            // 返回结果
            return x * value;
        }
    }
}
