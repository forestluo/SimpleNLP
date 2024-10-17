using System;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace NLP
{
    public class DBTool
    {
        // 最大线程数
        public static readonly int MAX_THREADS = 12;
        // 超时时间
        private static readonly int COMMAND_TIMEOUT = 30;
#if _USE_CLR
        public static readonly string CONNECT_STRING = "context connection = true";
#elif _USE_CONSOLE
        public static readonly string CONNECT_STRING =
            "Data Source=MSITRIDENT3;Initial Catalog=nldb3;Integrated Security=TRUE";
#endif

        public static void ExecuteNonQuery(string cmdString)
        {
            ExecuteNonQuery(cmdString, COMMAND_TIMEOUT);
        }

        public static void ExecuteNonQuery(string cmdString, int timeout)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(cmdString));
#endif
            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("DBTool", "ExecuteNonQuery", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 设置超时时间
                sqlCommand.CommandTimeout = timeout;
                // 记录日志
                //LogTool.LogMessage("DBTool", "ExecuteNonQuery", "T-SQL指令已创建！");

                // 执行指令
                sqlCommand.ExecuteNonQuery();
                // 记录日志
                //LogTool.LogMessage("DBTool", "ExecuteNonQuery", "T-SQL指令已执行！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("DBTool", "ExecuteNonQuery", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("DBTool", "ExecuteNonQuery", "数据库链接已关闭！");
            }
        }

        public static void ExecuteNonQuery(string cmdString, Dictionary<string, string> parameters)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(cmdString));
#endif
            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("DBTool", "ExecuteNonQuery", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                //LogTool.LogMessage("DBTool", "ExecuteNonQuery", "T-SQL指令已创建！");

                // 设定参数
                foreach (KeyValuePair<string, string> kvp in parameters)
                {
                    // 参数
                    string value = kvp.Value;
                    // 增加参数
                    sqlCommand.Parameters.AddWithValue(kvp.Key,
                        string.IsNullOrEmpty(value) ? (object)DBNull.Value : value);
                }
                // 记录日志
                //LogTool.LogMessage("DBTool", "ExecuteNonQuery", "T-SQL参数已设定！");

                // 执行指令
                sqlCommand.ExecuteNonQuery();
                // 记录日志
                //LogTool.LogMessage("DBTool", "ExecuteNonQuery", "T-SQL指令已执行！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("DBTool", "ExecuteNonQuery", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("Common", "ExecuteNonQuery", "数据库链接已关闭！");
            }
        }

        public static bool TableExists(string tableName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(tableName));
#endif
            string cmdString =
                "SELECT TOP 1 [id],[name] FROM sysObjects WHERE ID = OBJECT_ID(@SqlName) AND XTYPE = 'U';";

            // 对象ID
            int tid = -1;
            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("DBTool", "TableExists", "数据链接已开启！");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                //LogTool.LogMessage("DBTool", "TableExists", "T-SQL指令已创建！");

                // 设定参数
                sqlCommand.Parameters.AddWithValue("SqlName", tableName);
                // 记录日志
                //LogTool.LogMessage("DBTool", "TableExists", "T-SQL参数已设定！");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                //LogTool.LogMessage("DBTool", "TableExists", "T-SQL指令已执行！");

                // 循环处理
                while (reader.Read())
                {
                    // 获得长度
                    tid = reader.GetInt32(0);
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                //LogTool.LogMessage("DBTool", "TableExists", "数据阅读器已关闭！");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("DBTool", "TableExists", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("DBTool", "TableExists", "数据库链接已关闭！");
            }

            // 返回结果
            return tid > 0 ? true : false;
        }

        public static int GetMinLength(string tableName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(tableName));
#endif
            // 指令字符串
            string cmdString =
                string.Format("SELECT MIN([length]) FROM [dbo].[{0}];", tableName);

            // 设置参数
            int length = -1;
            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("DBTool", "GetMinLength", "数据库链接已开启!");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                //LogTool.LogMessage("DBTool", "GetMinLength", "T-SQL指令已创建!");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                //LogTool.LogMessage("DBTool", "GetMinLength", "T-SQL指令已执行!");

                // 循环处理
                while (reader.Read())
                {
                    // 获得长度
                    length = reader.GetInt32(0);
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                //LogTool.LogMessage("DBTool", "GetMinLength", "数据阅读器已关闭!");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("DBTool", "GetMinLength", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("DBTool", "GetMinLength", "数据库链接已关闭!");
            }

            // 返回结果
            return length;
        }

        public static int GetMaxLength(string tableName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(tableName));
#endif
            // 指令字符串
            string cmdString =
                string.Format("SELECT MAX([length]) FROM [dbo].[{0}];", tableName);

            // 设置参数
            int length = -1;
            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("DBTool", "GetMaxLength", "数据库链接已开启!");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                //LogTool.LogMessage("DBTool", "GetMaxLength", "T-SQL指令已创建!");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                //LogTool.LogMessage("DBTool", "GetMaxLength", "T-SQL指令已执行!");

                // 循环处理
                while (reader.Read())
                {
                    // 获得长度
                    length = reader.GetInt32(0);
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                //LogTool.LogMessage("DBTool", "GetMaxLength", "数据阅读器已关闭!");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("DBTool", "GetMaxLength", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("DBTool", "GetMaxLength", "数据库链接已关闭!");
            }

            // 返回结果
            return length;
        }

        public static int GetNextID(string tableName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(tableName));
#endif
            // 指令字符串
            string cmdString = string.Format(
                "SELECT IDENT_CURRENT('{0}') + IDENT_INCR('{1}');", tableName, tableName);

            // 设置参数
            int id = -1;
            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("DBTool", "GetNextID", "数据库链接已开启!");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                //LogTool.LogMessage("DBTool", "GetNextID", "T-SQL指令已创建!");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                //LogTool.LogMessage("DBTool", "GetNextID", "T-SQL指令已执行!");

                // 循环处理
                while (reader.Read())
                {
                    // 获得长度
                    id = Convert.ToInt32(reader.GetDecimal(0));
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                //LogTool.LogMessage("DBTool", "GetNextID", "数据阅读器已关闭!");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("DBTool", "GetNextID", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("DBTool", "GetNextID", "数据库链接已关闭!");
            }

            // 返回结果
            return id;
        }

        public static int GetMaxID(string tableName, string idName)
        {
#if DEBUG
            Debug.Assert(!string.IsNullOrEmpty(idName));
            Debug.Assert(!string.IsNullOrEmpty(tableName));
#endif
            // 指令字符串
            string cmdString =
                string.Format("SELECT MAX([{0}]) FROM [dbo].[{1}];", idName, tableName);

            // 设置参数
            int id = -1;
            // 创建数据库连接
            SqlConnection sqlConnection = new SqlConnection(DBTool.CONNECT_STRING);

            try
            {
                // 开启数据库连接
                sqlConnection.Open();
                // 记录日志
                //LogTool.LogMessage("DBTool", "GetMaxID", "数据库链接已开启!");

                // 创建指令
                SqlCommand sqlCommand =
                    new SqlCommand(cmdString, sqlConnection);
                // 记录日志
                //LogTool.LogMessage("DBTool", "GetMaxID", "T-SQL指令已创建!");

                // 创建数据阅读器
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // 记录日志
                //LogTool.LogMessage("DBTool", "GetMaxID", "T-SQL指令已执行!");

                // 循环处理
                while (reader.Read())
                {
                    // 获得id
                    id = reader.GetInt32(0);
                }
                // 关闭数据阅读器
                reader.Close();
                // 记录日志
                //LogTool.LogMessage("DBTool", "GetMaxID", "数据阅读器已关闭!");
            }
            catch (System.Exception ex)
            {
                // 记录日志
                LogTool.LogMessage("DBTool", "GetMaxID", "unexpected exit !");
                LogTool.LogMessage(string.Format("\texception.message = {0}", ex.Message));
            }
            finally
            {
                // 检查状态并关闭连接
                if (sqlConnection.State == ConnectionState.Open) sqlConnection.Close();
                // 记录日志
                //LogTool.LogMessage("DBTool", "GetMaxID", "数据库链接已关闭!");
            }

            // 返回结果
            return id;
        }
    }
}
