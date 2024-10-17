using System;
using System.Linq;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;

namespace NLP
{
    public class BPTrainer
    {
        static void Train(BPNetwork network, int times)
        {
#if DEBUG
            Debug.Assert(network != null);
            Debug.Assert(network.GetInputCount() == 18);
            Debug.Assert(network.GetOutputCount() == 3);
#endif
            // 记录日志
            LogTool.LogMessage("BPTrainer", "Train", "加载数据记录！");

            // 检查参数
            if (times <= 0) times = 50;
            // 记录日志
            LogTool.LogMessage(string.Format("\ttimes = {0}", times));

            // 创建Hash
            HashSet<string> values = new HashSet<string>();
            // 指令字符串
            string cmdString = string.Format(
                "SELECT TOP {0} [content] " +
                "   FROM [dbo].[AttributeContent] " + 
                "   WHERE [noun] >= 0.9 OR [verb] >= 0.9 OR [adjective] >= 0.9 ORDER BY NEWID();", times);

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                LogTool.LogMessage("BPTrainer", "Train", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                LogTool.LogMessage("BPTrainer", "Train", "T-SQL指令已创建！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                LogTool.LogMessage("BPTrainer", "Train", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 检查结果
                    if (reader.IsDBNull(0)) continue;
                    // 获得content
                    string value = reader.GetString(0);
                    // 检查结果
                    if (string.IsNullOrEmpty(value)) continue;
                    // 检查结果
                    if (!values.Contains(value)) values.Add(value);
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                LogTool.LogMessage("BPTrainer", "Train", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("BPTrainer", "Train", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                LogTool.LogMessage("BPTrainer", "Train", "数据库链接已关闭！");
            }

            // 记录日志
            LogTool.LogMessage("BPTrainer", "Train", "数据记录已加载！");

            // 记录日志
            LogTool.LogMessage("BPTrainer", "Train", "开始训练神经网络！");
            // 开始训练
            foreach (string value in values) Train(network, value);
            // 记录日志
            LogTool.LogMessage("BPTrainer", "Train", "神经网络训练完毕！");
        }

        static void Train(BPNetwork network, string value)
        {
#if DEBUG
            Debug.Assert(network != null);
            Debug.Assert(!string.IsNullOrEmpty(value));
#endif

#if _USE_CLR
            double[]
#elif _USE_CONSOLE
            double[]?
#endif
            // 获得输出矢量
            expects = AttributeCache.GetVector(value);
            // 检查结果
            if (expects == null || expects.Length != 3) return;

//#if _USE_CONSOLE
            //Console.WriteLine(string.Format("\t名词 = {0}", expects[0]));
            //Console.WriteLine(string.Format("\t动词 = {0}", expects[1]));
            //Console.WriteLine(string.Format("\t形容词 = {0}", expects[2]));
//#endif
            // 获得输入矢量
            double[] inputs = Grammar.GetSpecGamma(value);
#if DEBUG
            Debug.Assert(inputs != null && inputs.Length == 18);
#endif
            // 迭代次数
            int count = 0;
            // 开始循环处理
            do
            {
                // 计数
                count++;

                // 设置输入值
                network.SetInputs(inputs);
                // 正向传播
                network.Forward();
#if _USE_CONSOLE
                // 检查计数器
                if(count == 1)
                {
                    double[] outputs = network.GetOutputs();
                    Console.WriteLine(string.Format("value = \"{0}\"", value));
                    Console.WriteLine(string.Format("\t名词 = {0}", expects[0]));
                    Console.WriteLine(string.Format("\t动词 = {0}", expects[1]));
                    Console.WriteLine(string.Format("\t形容词 = {0}", expects[2]));
                    Console.WriteLine(string.Format("第{0}次迭代！", count));
                    Console.WriteLine(string.Format("\t名词 = {0}", outputs[0]));
                    Console.WriteLine(string.Format("\t动词 = {0}", outputs[1]));
                    Console.WriteLine(string.Format("\t形容词 = {0}", outputs[2]));
                }
#endif
                // 设置输出值
                network.SetOutputs(expects);
                // 检查误差
                if (network.GetError() < 1.0e-4)
                {
#if _USE_CONSOLE
                    if(count > 1)
                    {
                        double[] outputs = network.GetOutputs();
                        Console.WriteLine(string.Format("第{0}次迭代！", count));
                        Console.WriteLine(string.Format("\t名词 = {0}", outputs[0]));
                        Console.WriteLine(string.Format("\t动词 = {0}", outputs[1]));
                        Console.WriteLine(string.Format("\t形容词 = {0}", outputs[2]));
                        //Console.WriteLine(string.Format("\t误差 = {0}", network.GetError()));
                    }
#endif
                    break;
                }
                // 反向传播
                network.Backword();

            } while (true);
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备训练神经网络，可能需要花费很长时间！");
            Console.WriteLine("确认是否执行（Yes/No）？");
            // 检查确认
            if (!ConsoleTool.Confirm()) return;

            // 开启日志
            LogTool.SetLog(true);
            // 创建计时器
            Stopwatch watch = new Stopwatch();
            // 开启计时器
            watch.Start();
            // 生成神经网络
            BPNetwork network = new BPNetwork();
            // 初始化网络
            network.Initialize(18, 3, 6);
            // 循环处理
            // 训练神经网络
            Train(network, 1000);
            // 打印结果
            network.Print();
            // 关闭计时器
            watch.Stop();
            // 打印结果
            Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
        }
#endif
    }
}
