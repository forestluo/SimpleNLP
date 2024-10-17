using System;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;

namespace NLP
{
    public class AttributeContent
    {
        public static void CreateTable()
        {
            // 记录日志
            LogTool.LogMessage("AttributeContent", "CreateTable", "创建数据表！");

            // 指令字符串
            string cmdString =
                // 删除之前的表
                "IF OBJECT_ID('AttributeContent') IS NOT NULL " +
                "DROP TABLE dbo.AttributeContent; " +
                // 创建数据表
                "CREATE TABLE dbo.AttributeContent " +
                "( " +
                // 短语编号
                "[gid]                  INT                     IDENTITY(1, 1)              NOT NULL, " +
                // 内容
                "[content]              NVARCHAR(32)            PRIMARY KEY                 NOT NULL, " +
                // 名词
                "[noun]                 FLOAT                   NOT NULL                    DEFAULT 0, " +
                // 动词
                "[verb]                 FLOAT                   NOT NULL                    DEFAULT 0, " +
                // 代词
                "[pronoun]              FLOAT                   NOT NULL                    DEFAULT 0, " +
                // 数词
                "[numeral]              FLOAT                   NOT NULL                    DEFAULT 0, " +
                // 形容词
                "[adjective]            FLOAT                   NOT NULL                    DEFAULT 0, " +
                // 量词
                "[quantifier]           FLOAT                   NOT NULL                    DEFAULT 0, " +
                // 副词
                "[adverb]               FLOAT                   NOT NULL                    DEFAULT 0, " +
                // 助词
                "[auxiliary]            FLOAT                   NOT NULL                    DEFAULT 0, " +
                // 介词
                "[preposition]          FLOAT                   NOT NULL                    DEFAULT 0, " +
                // 连词
                "[conjunction]          FLOAT                   NOT NULL                    DEFAULT 0, " +
                // 感叹词
                "[exclamation]          FLOAT                   NOT NULL                    DEFAULT 0, " +
                // 拟声词
                "[onomatopoeia]         FLOAT                   NOT NULL                    DEFAULT 0, " +
                // 其他
                "[others]               FLOAT                   NOT NULL                    DEFAULT 0, " +
                // 操作标志
                "[operation]            INT                     NOT NULL                    DEFAULT 0, " +
                // 结果状态
                "[consequence]          INT                     NOT NULL                    DEFAULT 0 " +
                "); ";

            // 执行指令
            DBTool.ExecuteNonQuery(cmdString);

            // 记录日志
            LogTool.LogMessage("AttributeContent", "CreateTable", "数据表已创建！");
        }

        protected static void AddContent(GrammarAttribute attribute)
        {
#if DEBUG
            Debug.Assert(attribute != null);
            Debug.Assert(attribute.Posibilities != null);
            Debug.Assert(attribute.Posibilities.Length == 13);
#endif

            // 生成批量处理语句
            string cmdString =
                "UPDATE [dbo].[AttributeContent] " +
                    "SET [noun] = @SqlNoun, [verb] = @SqlVerb, [pronoun] = @SqlPronoun, [numeral] = @SqlNumeral, " +
                    "[adjective] = @SqlAdjective, [quantifier] = @SqlQuantifier, [adverb] = @SqlAdverb, " +
                    "[auxiliary] = @SqlAuxiliary, [preposition] = @SqlPreposition, [conjunction] = @SqlConjunction, " +
                    "[exclamation] = @SqlExclamation, [onomatopoeia] = @SqlOnomatopoeia, [others] = @SqlOthers WHERE [content] = @SqlContent; " +
                "IF @@ROWCOUNT <= 0 " +
                    "INSERT INTO [dbo].[AttributeContent] " +
                    "([content], [noun], [verb], [pronoun], [numeral], [adjective], [quantifier], " +
                    "[adverb], [auxiliary], [preposition], [conjunction], [exclamation], [onomatopoeia], [others]) " +
                    "VALUES (@SqlContent, @SqlNoun, @SqlVerb, @SqlPronoun, @SqlNumeral, @SqlAdjective, @SqlQuantifier, " +
                    "@SqlAdverb, @SqlAuxiliary, @SqlPreposition, @SqlConjunction, @SqlExclamation, @SqlOnomatopoeia, @SqlOthers); ";

            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("AttributeContent", "AddAttribute", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                //LogTool.LogMessage("AttributeContent", "AddAttribute", "T-SQL指令已创建！");

                // 设置参数
                sqlCommand.Parameters.AddWithValue("SqlContent", attribute.Value);
                sqlCommand.Parameters.AddWithValue("SqlNoun", attribute.Posibilities[0]);
                sqlCommand.Parameters.AddWithValue("SqlVerb", attribute.Posibilities[1]);
                sqlCommand.Parameters.AddWithValue("SqlPronoun", attribute.Posibilities[2]);
                sqlCommand.Parameters.AddWithValue("SqlNumeral", attribute.Posibilities[3]);
                sqlCommand.Parameters.AddWithValue("SqlAdjective", attribute.Posibilities[4]);
                sqlCommand.Parameters.AddWithValue("SqlQuantifier", attribute.Posibilities[5]);
                sqlCommand.Parameters.AddWithValue("SqlAdverb", attribute.Posibilities[6]);
                sqlCommand.Parameters.AddWithValue("SqlAuxiliary", attribute.Posibilities[7]);
                sqlCommand.Parameters.AddWithValue("SqlPreposition", attribute.Posibilities[8]);
                sqlCommand.Parameters.AddWithValue("SqlConjunction", attribute.Posibilities[9]);
                sqlCommand.Parameters.AddWithValue("SqlExclamation", attribute.Posibilities[10]);
                sqlCommand.Parameters.AddWithValue("SqlOnomatopoeia", attribute.Posibilities[11]);
                sqlCommand.Parameters.AddWithValue("SqlOthers", attribute.Posibilities[12]);
                // 记录日志
                //LogTool.LogMessage("AttributeContent", "AddAttribute", "T-SQL参数已设定！");

                // 执行指令
                sqlCommand.ExecuteNonQuery();
                // 记录日志
                //LogTool.LogMessage("AttributeContent", "AddAttribute", "T-SQL指令已执行！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("AttributeContent", "AddAttribute", "unexpected exit ！");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("AttributeContent", "AddAttribute", "数据库链接已关闭！");
            }
            // 记录日志
            //LogTool.LogMessage("AttributeContent", "AddAttribute", "数据记录已保存！");
        }

#if _USE_CONSOLE
        public static void Main(string[] args)
        {
            Console.WriteLine("准备创建AttributeContent表，原表及其数据将被删除！");
            Console.WriteLine("确认是否执行（Yes/No）？");
            // 检查确认
            if (!ConsoleTool.Confirm()) return;

            // 开启日志
            LogTool.SetLog(true);
            // 创建计时器
            Stopwatch watch = new Stopwatch();
            // 开启计时器
            watch.Start();
            // 创建数据表
            CreateTable();
            // 关闭计时器
            watch.Stop();
            // 打印结果
            Console.WriteLine(string.Format("Time elapsed : {0} ms ", watch.ElapsedMilliseconds));
        }
#endif
    }
}
