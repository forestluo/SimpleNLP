using System;
using System.Diagnostics;

namespace NLP
{
    public class BPNetwork
    {
        // 学习率
        public double LearningRate { get; set; } = 0.618f;

        // 输入层
        private int InputCount;
#if _USE_CLR
        private double[] Inputs;
#elif _USE_CONSOLE
        private double[]? Inputs;
#endif
        // 隐藏层
        private int HiddenCount;
#if _USE_CLR
        private double[] Hiddens;
#elif _USE_CONSOLE
        private double[]? Hiddens;
#endif
        // 输出层
        private int OutputCount;
#if _USE_CLR
        private double[] Outputs;
#elif _USE_CONSOLE
        private double[]? Outputs;
#endif

        // 误差
#if _USE_CLR
        private double[] Errors;
#elif _USE_CONSOLE
        private double[]? Errors;
#endif
        // 偏置
#if _USE_CLR
        private double[] InputBiases;
        private double[] OutputBiases;
#elif _USE_CONSOLE
        private double[]? InputBiases;
        private double[]? OutputBiases;
#endif
        // 误差
        // 反向传播计算
#if _USE_CLR
        private double[] Deltas;
#elif _USE_CONSOLE
        private double[]? Deltas;
#endif

        // 输入层权重
#if _USE_CLR
        private double[][] InputWeights;
#elif _USE_CONSOLE
        private double[][]? InputWeights;
#endif
        // 输出层权重
#if _USE_CLR
        private double[][] OutputWeights;
#elif _USE_CONSOLE
        private double[][]? OutputWeights;
#endif

        public BPNetwork()
        {

        }

        public int GetInputCount()
        {
            return InputCount;
        }

        public int GetOutputCount()
        {
            return OutputCount;
        }

        public int GetHiddenCount()
        {
            return HiddenCount;
        }

        static double Sigmoid(double x)
        {
            // 返回结果
            return 1.0f / (1.0f + Math.Pow(Math.E, -x));
        }

        static double DeltaSigmoid(double x)
        {
            double value = Sigmoid(x);
            // 返回结果
            return value * (1.0f - value);
        }

        public void Initialize(int InputCount, int OutputCount, int HiddenCount)
        {
#if DEBUG
            Debug.Assert(InputCount > 1);
            Debug.Assert(OutputCount > 0);
            Debug.Assert(HiddenCount > 1);
#endif
            // 设置输入层
            this.InputCount = InputCount;
            // 设置输出层
            this.OutputCount = OutputCount;
            // 设置隐藏层
            this.HiddenCount = HiddenCount;

            // 创建输入数据
            Inputs = new double[InputCount];
            // 创建输出数据
            Outputs = new double[OutputCount];
            // 创建隐藏层数据
            Hiddens = new double[HiddenCount];

            // 创建误差数据
            Errors = new double[OutputCount];
            // 创建误差数据
            Deltas = new double[OutputCount];

            // 创建偏置数据
            InputBiases = new double[HiddenCount];
            // 创建输入层权重
            InputWeights = new double[HiddenCount][];
            for (int i = 0; i < HiddenCount; i++)
            {
                InputWeights[i] = new double[InputCount];
            }

            // 创建偏置数据
            OutputBiases = new double[OutputCount];
            // 创建输出层权重
            OutputWeights = new double[OutputCount][];
            for (int i = 0; i < OutputCount; i++)
            {
                OutputWeights[i] = new double[HiddenCount];
            }

            // 给定一个初始值
            Random random = new Random((int)DateTime.Now.Ticks);
            // 随机给定数值
            for (int i = 0; i < InputCount; i++)
                Inputs[i] = random.NextDouble();
            for (int i = 0; i < OutputCount; i++)
                Outputs[i] = random.NextDouble();
            for (int i = 0; i < HiddenCount; i++)
                for (int j = 0; j < InputCount; j++)
                    InputWeights[i][j] = random.NextDouble();
            for (int i = 0; i < OutputCount; i++)
                for (int j = 0; j < HiddenCount; j++)
                    OutputWeights[i][j] = random.NextDouble();
        }

        public double[] GetOutputs()
        {
#if DEBUG
            Debug.Assert(Outputs != null);
#endif
            return Outputs;
        }

        public void SetInputs(double[] Inputs)
        {
#if DEBUG
            Debug.Assert(this.Inputs != null);
            Debug.Assert(Inputs != null &&
                Inputs.Length == this.InputCount);
#endif
            // 拷贝数据
            Inputs.CopyTo(this.Inputs, 0);
        }

        public double GetError()
        {
#if DEBUG
            Debug.Assert(Errors != null);
#endif
            // 数值
            double value = 0.0;
            // 循环处理
            for (int i = 0; i < OutputCount; i++)
            {
                // 误差求和
                value += 0.5 * Errors[i] * Errors[i];
            }
            // 返回结果
            return value;
        }

        public void SetOutputs(double[] expects)
        {
#if DEBUG
            Debug.Assert(Errors != null);
            Debug.Assert(Outputs != null);
            Debug.Assert(expects != null &&
                expects.Length == this.OutputCount);
#endif
            // 循环处理
            for (int i = 0; i < OutputCount; i++)
            {
                // 计算误差
                Errors[i] = expects[i] - Outputs[i];
            }
        }

        public void Forward()
        {
#if DEBUG
            Debug.Assert(Inputs != null);
            Debug.Assert(Outputs != null);
            Debug.Assert(Hiddens != null);
            Debug.Assert(InputBiases != null);
            Debug.Assert(InputWeights != null);
            Debug.Assert(OutputBiases != null);
            Debug.Assert(OutputWeights != null);
#endif
            // 计算隐藏层数据
            for (int i = 0;i < HiddenCount;i ++)
            {
                // 数值
                double value = 0.0;
                // 循环处理
                for(int j = 0;j < InputCount;j ++)
                {
                    value += InputWeights[i][j] * Inputs[j];
                }
                // 设置输出值
                Hiddens[i] = Sigmoid(value + InputBiases[i]);
            }

            // 计算输出层数据
            for(int i = 0;i < OutputCount;i ++)
            {
                // 数值
                double value = 0.0;
                // 循环处理
                for (int j = 0; j < HiddenCount; j++)
                {
                    value += OutputWeights[i][j] * Hiddens[j];
                }
                // 设置输出值
                Outputs[i] = Sigmoid(value + OutputBiases[i]);
            }
        }

        public void Backword()
        {
#if DEBUG
            Debug.Assert(Errors != null);
            Debug.Assert(Deltas != null);
            Debug.Assert(Inputs != null);
            Debug.Assert(Outputs != null);
            Debug.Assert(Hiddens != null);
            Debug.Assert(InputBiases != null);
            Debug.Assert(InputWeights != null);
            Debug.Assert(OutputBiases != null);
            Debug.Assert(OutputWeights != null);
#endif
            // 计算隐层
            for (int i = 0;i < OutputCount;i ++)
            {
                // 链式反应
                double delta = - Errors[i];
                // 链式反应
                delta *= DeltaSigmoid(Outputs[i]);
                // 设置误差
                Deltas[i] = delta;
                // 设置新的偏置
                // 与学习率有关
                OutputBiases[i] -= LearningRate * delta;
                // 循环处理
                for (int j = 0;j < HiddenCount;j ++)
                {
                    // 设置新的权重
                    // 与学习率有关
                    OutputWeights[i][j] -= LearningRate * delta * Hiddens[j];
                }
            }

            // 计算输入层
            for(int i = 0;i < HiddenCount;i ++)
            {
                // 链式反应
                double delta = 0.0f;
                // 循环处理
                for (int j = 0;j < OutputCount;j ++)
                {
                    // 链式反应
                    delta += OutputWeights[j][i] * Deltas[j]; 
                }
                // 链式反应
                delta *= DeltaSigmoid(Hiddens[i]);
                // 与学习率有关
                // 设置新的偏置
                InputBiases[i] -= LearningRate * delta;
                // 循环处理
                for(int j = 0;j < InputCount; j ++)
                {
                    // 设置新的权重
                    // 与学习率有关
                    InputWeights[i][j] -= LearningRate * delta * Inputs[j];
                }
            }
        }

        public bool ImportFile(string fileName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(fileName));
#endif
            // 记录日志
            LogTool.LogMessage("BPNetwork", "ImportFile", "开始输入数据！");

            try
            {
                // 创建文件流
                FileStream fs = new FileStream(fileName, FileMode.Open);
                // 创建输出流
                BinaryReader br = new BinaryReader(fs);
                // 记录日志
                LogTool.LogMessage("BPNetwork", "ImportFile", "文件流已开启！");

                // 读取标识符
                string version = br.ReadString();
                // 检查结果
                if(string.IsNullOrEmpty(version) || !version.Equals("BPNetwork"))
                {
                    // 记录日志
                    LogTool.LogMessage("BPNetwork", "ImportFile", "无法识别的文件！");
                    return false;
                }

                // 读取InputCount;
                InputCount = br.ReadInt32();
                // 读取OutputCount;
                OutputCount = br.ReadInt32();
                // 读取HiddenCount;
                HiddenCount = br.ReadInt32();
                // 读取LearningRate;
                LearningRate = br.ReadDouble();

                // 初始化
                Initialize(InputCount, OutputCount, HiddenCount);
#if DEBUG
                Debug.Assert(Errors != null);
                Debug.Assert(Deltas != null);
                Debug.Assert(Inputs != null);
                Debug.Assert(Outputs != null);
                Debug.Assert(Hiddens != null);
                Debug.Assert(InputBiases != null);
                Debug.Assert(InputWeights != null);
                Debug.Assert(OutputBiases != null);
                Debug.Assert(OutputWeights != null);
#endif

                // 输入数据
                for (int i = 0; i < InputCount; i++)
                {
                    InputBiases[i] = br.ReadDouble();
                }
                // 输入数据
                for (int i = 0; i < HiddenCount; i++)
                    for (int j = 0; j < InputCount; j++)
                {
                    InputWeights[i][j] = br.ReadDouble();
                }
                // 输入数据
                for (int i = 0; i < OutputCount; i++)
                {
                    OutputBiases[i] = br.ReadDouble();
                }
                // 输入数据
                for (int i = 0; i < OutputCount; i++)
                    for (int j = 0; j < HiddenCount; j++)
                {
                    OutputWeights[i][j] = br.ReadDouble();
                }
                // 关闭文件流
                fs.Close();
                // 记录日志
                LogTool.LogMessage("BPNetwork", "ImportFile", "文件流已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("BPNetwork", "ImportFile", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
                // 返回结果
                return false;
            }
            // 记录日志
            LogTool.LogMessage("BPNetwork", "ImportFile", "数据输出完毕！");
            // 返回结果
            return true;
        }

        public void ExportFile(string fileName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(fileName));
#endif
            // 记录日志
            LogTool.LogMessage("BPNetwork", "ExportFile", "开始输出数据！");

            try
            {
                // 保存至本地磁盘
                // 创建文件流
                FileStream fs = new FileStream(fileName, FileMode.Create);
                // 创建输出流
                BinaryWriter bw = new BinaryWriter(fs);
                // 记录日志
                LogTool.LogMessage("BPNetwork", "ExportFile", "文件流已开启！");

                // 输出数据
                bw.Write("BPNetwork");
                bw.Write(InputCount);
                bw.Write(OutputCount);
                bw.Write(HiddenCount);
                bw.Write(LearningRate);
#if DEBUG
                Debug.Assert(Errors != null);
                Debug.Assert(Deltas != null);
                Debug.Assert(Inputs != null);
                Debug.Assert(Outputs != null);
                Debug.Assert(Hiddens != null);
                Debug.Assert(InputBiases != null);
                Debug.Assert(InputWeights != null);
                Debug.Assert(OutputBiases != null);
                Debug.Assert(OutputWeights != null);
#endif
                // 输出数据
                for (int i = 0; i < InputCount; i++) bw.Write(InputBiases[i]);
                for (int i = 0; i < HiddenCount; i++)
                    for (int j = 0; j < InputCount; j++) bw.Write(InputWeights[i][j]);
                // 输出数据
                for (int i = 0; i < OutputCount; i++) bw.Write(OutputBiases[i]);
                for (int i = 0; i < OutputCount; i++)
                    for (int j = 0; j < HiddenCount; j++) bw.Write(OutputWeights[i][j]);
                // 刷新打印流
                bw.Flush();
                // 关闭打印流
                bw.Close();
                // 关闭文件流
                fs.Close();
                // 记录日志
                LogTool.LogMessage("BPNetwork", "ExportFile", "文件流已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("BPNetwork", "ExportFile", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            // 记录日志
            LogTool.LogMessage("BPNetwork", "ExportFile", "数据输出完毕！");
        }

        public void Print()
        {
#if DEBUG
            Debug.Assert(Errors != null);
            Debug.Assert(Deltas != null);
            Debug.Assert(Inputs != null);
            Debug.Assert(Outputs != null);
            Debug.Assert(Hiddens != null);
            Debug.Assert(InputBiases != null);
            Debug.Assert(InputWeights != null);
            Debug.Assert(OutputBiases != null);
            Debug.Assert(OutputWeights != null);
#endif
            LogTool.LogMessage("BPNetwork", "Print",
                "打印数据！");
            LogTool.LogMessage("\tInputCount = " + InputCount);
            LogTool.LogMessage("\tOutputCount = " + OutputCount);
            LogTool.LogMessage("\tHiddenCount = " + HiddenCount);
            LogTool.LogMessage("\tLearningRate = " + LearningRate);
            // 打印输入端数据
            for (int i = 0; i < HiddenCount; i++)
                LogTool.LogMessage(
                    string.Format("\tInputBiases[{0}] = {1}", i, InputBiases[i]));
            for (int i = 0; i < HiddenCount; i++)
                for(int j = 0;j < InputCount; j ++)
                    LogTool.LogMessage(
                        string.Format("\tInputWeights[{0}][{1}] = {2}", i,j, InputWeights[i][j]));
            // 打印输出端数据
            for (int i = 0; i < OutputCount; i++)
                LogTool.LogMessage(
                    string.Format("\tOutputBiases[{0}] = {1}", i, OutputBiases[i]));
            for (int i = 0; i < OutputCount; i++)
                for (int j = 0; j < HiddenCount; j++)
                    LogTool.LogMessage(
                        string.Format("\tInputWeights[{0}][{1}] = {2}", i, j, OutputWeights[i][j]));
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            // 开启日志
            LogTool.SetLog(true);
            // 创建计时器
            Stopwatch watch = new Stopwatch();
            // 开启计时器
            watch.Start();
            // 生成神经网络
            BPNetwork network = new BPNetwork();
            // 初始化
            network.Initialize(2, 2, 2);
            // 设置学习率
            network.LearningRate = 0.5;
            // 设置标定用值
            double[] Inputs = new double[2] { 0.1, 0.2 };
            double[] expects = new double[2] { 0.01, 0.99 };
            // 设置输入权重
            network.InputWeights = new double[][]
                {
                    // W1和W2
                    new double[] { 0.1, 0.2 },
                    // W3和W4
                    new double[] { 0.3, 0.4 },
                };
            // 设置输出权重
            network.OutputWeights = new double[][]
                {
                    // W5和W6
                    new double[] { 0.5, 0.6 },
                    // W7和W8
                    new double[] { 0.7, 0.8 },
                };
            // 设置输入偏置
            network.InputBiases = new double[] { 0.55, 0.56 };
            // 设置输出偏置
            network.OutputBiases = new double[] { 0.66, 0.67 };

            // 设置输入值
            network.SetInputs(Inputs);
            // 计算一次正向传播
            network.Forward();
            // 获得输出值
            double[] Outputs = network.GetOutputs();
            // 打印输出值
            //Outputs[0] = 0.7989476413779711
            //Outputs[1] = 0.8390480283342561
            Console.WriteLine("\toutputs[0] = " + Outputs[0]);
            Console.WriteLine("\toutputs[1] = " + Outputs[1]);
            // 设置预期数值
            network.SetOutputs(expects);
            // 打印误差
            //error = 0.3226124392928197
            Console.WriteLine("\terror = " + network.GetError());
            // 计算一次反向传播
            network.Backword();
            // 关闭计时器
            watch.Stop();
            // 打印结果
            Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
        }
#endif
    }
}
