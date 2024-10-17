using System.Data;
using System.Text;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace NLP
{
    public class ShortTool
    {

#if _USE_CONSOLE
		public static void Main(string[] args)
		{
			Console.WriteLine("准备提取ShortContent内容，原表及其数据将被更改！");
			Console.WriteLine("确认是否执行（Yes/No）？");
			// 检查取人
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
			ShortContent.CreateTable();
			// 提取内容
			ExtractShorts();
			// 卸载数据
			ShortCache.ClearShorts();
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
