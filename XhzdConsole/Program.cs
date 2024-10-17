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
    // Http客户端
    private static HttpClient
        client = new HttpClient();

    public static async Task GetHtml(string url, string item)
    {
        try
        {
            // 打印描述
            Console.WriteLine(string.Format("item.Value = \"{0}\"", item));
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
            Match match = Regex.Match(responseBody, "(〈(.*?)〉)|(<(名|动|動|形|代|数|量|副|介|助|连|语|象|方)>)");
            // 检查结果
            while (match.Success)
            {
                // 获得描述
                string description = match.Value;
                // 检查结果
                if(description.Length <= 3)
                {
                    // 打印描述
                    Console.WriteLine(string.Format("item.Description = \"{0}\"", description));
                    // 创建文件
                    StreamWriter sw = new StreamWriter("xhzd.txt", true);

                    if (description.Equals("〈名〉")) description = "\"名词\"";
                    else if (description.Equals("〈动〉")) description = "\"动词\"";
                    else if (description.Equals("〈動〉")) description = "\"动词\"";
                    else if (description.Equals("〈形〉")) description = "\"形容词\"";
                    else if (description.Equals("〈代〉")) description = "\"代词\"";
                    else if (description.Equals("〈数〉")) description = "\"数词\"";
                    else if (description.Equals("〈量〉")) description = "\"量词\"";
                    else if (description.Equals("〈副〉")) description = "\"副词\"";
                    else if (description.Equals("〈介〉")) description = "\"介词\"";
                    else if (description.Equals("〈助〉")) description = "\"助词\"";
                    else if (description.Equals("〈连〉")) description = "\"连词\"";
                    else if (description.Equals("〈语〉")) description = "\"感叹词\"";
                    else if (description.Equals("〈象〉")) description = "\"拟声词\"";
                    else if (description.Equals("〈方〉")) description = "\"习惯用语\"";
                    // 输出文件
                    sw.WriteLine("\"" + item + "\"," + description); sw.Flush(); sw.Close();
                }
                // 下一个
                match = match.NextMatch();
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
        // 计数器
        int total = 0;
        // 指令字符串
        string cmdString =
            "SELECT [content] FROM [dbo].[TokenContent] " +
            "   WHERE dbo.NLPIsChinese([content]) = 1";

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

                // 执行Http请求
                GetHtml("https://zidian.gushici.net/search/zi.php?zi=", value).Wait();
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
        LogTool.LogMessage("Program", "Main", "所有请求已经执行完毕！");
    }
}
