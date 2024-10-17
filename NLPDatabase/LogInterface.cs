using NLP;
using System.Collections;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Runtime.CompilerServices;

public partial class LogInterface
{
    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlBoolean NLPSetLog(SqlBoolean sqlLog)
    {
        LogTool.
        SetLog(sqlLog.Value);
        return SqlBoolean.True;
    }

    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlBoolean NLPClearLogs()
    {
        LogTool.ClearLogs();
        return SqlBoolean.True;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    [Microsoft.SqlServer.Server.SqlFunction
        (DataAccess = DataAccessKind.Read,
            FillRowMethodName = "NLPGetLogs_FillRow",
            TableDefinition = "LogValue nvarchar(4000)")]
    public static IEnumerable NLPGetLogs()
    {
        // ·µ»Ø½á¹û
        return LogTool.GetLogs();
    }

    public static void NLPGetLogs_FillRow(object logResultObj, out SqlString Value)
    {
        Value = (string)logResultObj;
    }
}