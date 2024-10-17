// See https://aka.ms/new-console-template for more information

using NLP;
using System.IO;
using System.Data;
using System.Net.Http;
using System.Diagnostics;
using System.Collections;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

public class Program
{
    // 任务集合
    private static List<string>
        items = new List<string>();
    // 创建文件
    private static StreamWriter sw
        = new StreamWriter("baidu.txt", true);

    public static void AddContent(string content, string source)
    {
#if DEBUG
        Debug.Assert(!string.IsNullOrEmpty(content));
#endif
        // 指令字符串
        string cmdString =
            "INSERT INTO [dbo].[RawContent] ([source], [content], [hash], [length]) " +
            "VALUES(@SqlSource, @SqlContent, HASHBYTES('SHA2_512', @SqlContent), LEN(@SqlContent));";

        // 参数字典
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        // 加入参数
        parameters.Add("SqlSource", source);
        parameters.Add("SqlContent", content);
        // 执行指令
        DBTool.ExecuteNonQuery(cmdString, parameters);
    }

    public static void Run()
    {
        // Http客户端
        HttpClient client = new HttpClient();

        string value;

        do
        {
            lock (items)
            {
                // 检查剩余数量
                if (items.Count <= 0)
                {
                    Console.WriteLine("没有任何可执行的任务！"); return;
                }
                // 获得第一个
                value = items.First(); items.RemoveAt(0);
            }
            // 打印描述
            Console.WriteLine(
                string.Format("item.Value = \"{0}\"", value));
            // 执行Http请求
            GetHtml(client, "https://baike.baidu.com/item/", value).Wait();

        } while (true);
    }

    public static async Task GetHtml(HttpClient client, string url, string item)
    {
        try
        {
            // 等待反馈信息
            HttpResponseMessage response = await client.GetAsync(url + item);
            // 确认反馈结果
            response.EnsureSuccessStatusCode();
            // 获得反馈页面内容
            string responseBody = await response.Content.ReadAsStringAsync();
            // 检查结果
            if (string.IsNullOrEmpty(responseBody))
            {
                Console.WriteLine("没有正确的返回结果！"); return;
            }
            // 搜索
            Match match = Regex.Match(responseBody,
                "<meta property=\"og:description\" content=\"(.*)\" />");
            // 检查结果
            if (!match.Success)
            {
                Console.WriteLine("没有找到合适的匹配项！"); return;
            }
            // 获得描述
            string description = match.Groups[1].Value;
            // 打印描述
            Console.WriteLine(string.Format("item.Description = \"{0}\"", description));

            lock(sw)
            {
                // 输出文件
                sw.WriteLine(item + ":=" + description); sw.Flush();
            }
        }
        catch (System.Exception ex)
        {
            // 记录日志
            LogTool.LogMessage("Program", "GetHtml", "unexpected exit !");
            LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
        }
    }

    public static void Main(string[] args)
    {
        /*
        // 计数器
        int total = 0;
        // 指令字符串
        string cmdString =
            "SELECT DISTINCT(content) FROM [dbo].[DictionaryContent]";

        // 创建数据库连接
        SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

        try
        {
            // 开启数据库连接
            sqlConnection.Open();
            // 记录日志
            LogTool.LogMessage("Program", "Main", "数据链接已开启！");

            // 创建指令
            SqlCommand sqlCommand =
                new SqlCommand(cmdString, sqlConnection);
            // 记录日志
            LogTool.LogMessage("Program", "Main", "T-SQL指令已创建！");

            // 创建数据阅读器
            SqlDataReader reader = sqlCommand.ExecuteReader();
            // 记录日志
            LogTool.LogMessage("Program", "Main", "T-SQL指令已执行！");

            // 循环处理
            while (reader.Read())
            {
                // 增加计数
                total++;
                // 检查计数
                if (total % 10000 == 0)
                {
                    // 打印记录
                    LogTool.LogMessage("Program", "Main",
                        string.Format("{0} entries loaded !", total));
                }

                // 获得内容
                string value = reader.GetString(0);
                // 检查结果
                if (string.IsNullOrEmpty(value)) continue;
                // 增加任务
                items.Add(value);
            }
            // 关闭数据阅读器
            reader.Close();

            // 打印记录
            LogTool.LogMessage("Program", "Main",
                string.Format("{0} entries loaded !", total));
            // 记录日志
            LogTool.LogMessage("Program", "Main", "数据阅读器已关闭！");

        }
        catch (System.Exception ex)
        {
            // 记录日志
            LogTool.LogMessage("Program", "Main", "unexpected exit !");
            LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
        }
        finally
        {
            // 检查状态并关闭连接
            if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
            // 记录日志
            LogTool.LogMessage("Program", "Main", "数据库链接已关闭！");
        }

        // 记录日志
        LogTool.LogMessage("\ttotal = " + total);
        // 记录日志
        LogTool.LogMessage("Program", "Main", "开始启动任务！");

        // 任务数组
        List<Task> tasks = new List<Task>();
        // 生成任务控制器
        TaskFactory factory = new TaskFactory(
            new LimitedConcurrencyLevelTaskScheduler(DBTool.MAX_THREADS));
        // 启动进程
        for(int i = 0;i < 10;i ++) tasks.Add(factory.StartNew(() => Run()));
        // 等待任务完成
        Task.WaitAll(tasks.ToArray());
        // 记录日志
        LogTool.LogMessage("Program", "Main", "所有请求已经执行完毕！");
        */

        // 计数器
        int total = 0;

        StreamReader sr = new StreamReader("baidu.txt");
        //循环处理
        while (sr.BaseStream.Position < sr.BaseStream.Length)
        {
            // 增加计数
            total++;
            // 检查计数
            if (total % 10000 == 0)
            {
                // 打印记录
                LogTool.LogMessage("Program", "Main",
                    string.Format("{0} entries loaded !", total));
            }

            // 读取一行
            string? line = sr.ReadLine();
            // 检查内容
            if (string.IsNullOrEmpty(line)) continue;
            // 截取字符串
            int index = line.IndexOf(":=");
            // 检查结果
            if (index < 0) continue;
            // 截取内容
            line = line.Substring(index + 2);
            // 检查内容
            if (string.IsNullOrEmpty(line)) continue;
            // 插入记录
            AddContent(line, "百度词条");
        }

    }
}
