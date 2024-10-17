using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;

namespace NLP
{
    public class LongTool
    {
		public static void ExtractLongs()
		{
			// 记录日志
			LogTool.LogMessage("LongTool", "ExtractLongs", "提取句子数据！");

			// 获得数组
			string[] contents = FilteredCache.GetArray();
#if DEBUG
			Debug.Assert(contents != null && contents.Length > 0);
#endif            
			// 记录日志                 
			LogTool.LogMessage(string.Format("\tcount = {0}", contents.Length));

			// 创建字典
			HashSet<string> longs = new HashSet<string>();

			// 计数器
			int total = 0;
			// 任务数组
			List<Task> tasks = new List<Task>();
			// 生成任务控制器
			TaskFactory factory = new TaskFactory(
				new LimitedConcurrencyLevelTaskScheduler(DBTool.MAX_THREADS));

			try
			{
				// 循环处理
				foreach(string content in contents)
				{
					// 增加计数
					total++;
					// 检查计数
					if (total % 10000 == 0)
					{
						// 打印记录
						LogTool.LogMessage("LongTool", "ExtractLongs", 
							string.Format("{0} items processed !", total));
					}

					// 检查结果
					if (string.IsNullOrEmpty(content)) continue;

					// 启动进程
					tasks.Add(factory.StartNew
					(() =>
					{
#if _USE_CLR
						string[] sentences
#elif _USE_CONSOLE
						string[]? sentences
#endif
						// 获得拆分结果                               
						= SentenceExtractor.Extract(content);
						// 检查结果
						if (sentences == null ||
								sentences.Length <= 0) return;
						// 循环处理
						foreach (string sentence in sentences)
                        {
							bool newFlag = false;
							// 同步锁定
							lock(longs)
                            {
								// 检查关键字
								if (!longs.Contains(sentence))
								{
									// 增加内容
									longs.Add(sentence); newFlag = true;
								}
							}
							// 加入内容
							if(newFlag) LongContent.AddContent(sentence);
						}
					}));

					// 检查任务数量
					if (tasks.Count >= 10000) { Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
				}
				// 打印记录
				LogTool.LogMessage("LongTool", "ExtractLongs", 
					string.Format("{0} items processed !", total));
			}
			catch (System.Exception ex)
			{
				// 记录日志
				LogTool.LogMessage("LongTool", "ExtractLongs", "unexpected exit ！");
				LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
			}
			finally
			{
				// 检查任务数量
				if (tasks.Count > 0)
				{ Task.WaitAll(tasks.ToArray()); tasks.Clear(); }
				// 记录日志
				LogTool.LogMessage("LongTool", "ExtractLongs", "任务全部结束！");
			}

			// 记录日志
			LogTool.LogMessage(string.Format("\tlong.count = {0}", total));
			// 记录日志
			LogTool.LogMessage("LongTool", "ExtractLongs", "句子提取完毕！");
		}

#if _USE_CONSOLE
		public static void Main(string[] args)
		{
			Console.WriteLine("准备提取LongContent内容，原表及其数据将被更改！");
			Console.WriteLine("确认是否执行（Yes/No）？");
			// 检查确认
			if (!ConsoleTool.Confirm()) return;

			// 开启日志
			LogTool.SetLog(true);
			// 创建计时器
			Stopwatch watch = new Stopwatch();
			// 开启计时器
			watch.Start();
			// 加载数据
			FilteredCache.LoadContents();
			// 创建数据表
			LongContent.CreateTable();
			// 提取内容
			ExtractLongs();
			// 卸载数据
			FilteredCache.ClearContents();
			// 关闭计时器
			watch.Stop();
			// 打印结果
			Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
		}
#endif
	}
}
