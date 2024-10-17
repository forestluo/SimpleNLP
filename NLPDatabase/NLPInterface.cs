using NLP;
using System.Linq;
using System.Collections;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

namespace NLPDatabase
{
    public class NLPInterface
    {
        [Microsoft.SqlServer.Server.SqlFunction]
        public static SqlString NLPGetVersion()
        {
            // 返回结果
            return new SqlString("NLDB v3.1.1.0");
        }

        [Microsoft.SqlServer.Server.SqlProcedure]
        public static void NLPCatchException(SqlString sqlTips)
        {
            // 检查参数
            if (sqlTips == null ||
                sqlTips.IsNull) return;
            // 获得内容
            string value = sqlTips.Value;
            // 检查参数
            if (value == null || value.Length <= 0) return;
            // 捕捉异常
            ExceptionLog.CatchException(value);
        }

        [Microsoft.SqlServer.Server.SqlProcedure]
        public static void NLPDeleteContent(SqlString sqlContent)
        {
            // 检查参数
            if (sqlContent == null ||
                sqlContent.IsNull) return;
            // 获得内容
            string value = sqlContent.Value;
            // 检查参数
            if (string.IsNullOrEmpty(value)) return;

            // 删除属性
            MarkCache.DeleteMarks(value);
            // 删除内容
            CoreCache.DeleteSegment(value);

            // 更新数据
            GammaTool.MakeStatistic(value);
            // 记录操作
            OperationLog.Log("NLPInterface.NLPDeleteContent", "del " + value);
        }

        [Microsoft.SqlServer.Server.SqlProcedure]
        public static void NLPAddContent(SqlString sqlContent, SqlString sqlClass, SqlString sqlSubclass)
        {
            // 检查参数
            if (sqlContent == null || sqlContent.IsNull) return;
            // 获得内容
            string value = sqlContent.Value;
            // 检查参数
            if (string.IsNullOrEmpty(value)) return;

            // 获得计数
            int count = CounterTool.GetCount(value, true);
            // 增加内容
            CoreCache.AddSegment(value, count < 0 ? 0 : count);
            // 记录操作
            OperationLog.Log("NLPInterface.AddContent", "add " + value);

            // 创建对象
            MarkAttribute mark = new MarkAttribute();
            // 设置参数
            mark.Index = 0;
            mark.Value = 
                (sqlClass != null && sqlClass.IsNull) ? sqlClass.Value : null;
            mark.Remark = 
                (sqlSubclass != null && sqlSubclass.IsNull) ? sqlSubclass.Value : null;
            // 检查主类别
            if(!string.IsNullOrEmpty(mark.Value) &&
                WordType.IsMajor(mark.Value))
            {
                // 检查次子类别
                if(string.IsNullOrEmpty(mark.Remark) ||
                    WordType.IsMinor(mark.Remark))
                {
                    // 增加属性
                    MarkCache.AddMark(value, mark);
                    // 记录操作
                    OperationLog.Log("NLPInterface.NLPAddContent",
                        "attr -add " + mark.Value +
                        (string.IsNullOrEmpty(mark.Remark) ? "" : " " + mark.Remark));
                }
            }
        }

        [Microsoft.SqlServer.Server.SqlFunction]
        public static SqlInt32 NLPIsChinese(SqlString sqlContent)
        {
            if (sqlContent == null ||
                sqlContent.IsNull) return -1;
            // 获得参数
            string value = sqlContent.Value;
            // 检查参数
            if (value == null || value.Length <= 0) return -1;
            // 返回结果
            return ChineseTool.IsChinese(value) ? 1 : 0;
        }

        [Microsoft.SqlServer.Server.SqlFunction]
        public static SqlString NLPClearContent(SqlString sqlContent)
        {
            if (sqlContent == null ||
                sqlContent.IsNull) return SqlString.Null;
            // 获得参数
            string value = sqlContent.Value;
            // 检查参数
            if (value == null || value.Length <= 0) return SqlString.Null;
            // 返回结果
            return MiscTool.ClearContent(value);
        }

        [Microsoft.SqlServer.Server.SqlFunction]
        public static SqlInt32 NLPGetCount(SqlString sqlContent, SqlInt32 sqlExtend)
        {
            if(sqlContent == null ||
                sqlContent.IsNull) return -1;
            // 获得参数
            string value = sqlContent.Value;
            // 检查参数
            if (value == null || value.Length <= 0) return -1;
            // 返回结果
            return CounterTool.GetCount(value, sqlExtend.Value > 0);
        }

        [Microsoft.SqlServer.Server.SqlFunction
            (DataAccess = DataAccessKind.Read,
                FillRowMethodName = "NLPExtractQuantities_FillRow",
                TableDefinition = "QIndex int, QLength int, QValue nvarchar(4000)")]
        public static IEnumerable NLPExtractQuantities(SqlString sqlContent)
        {
            // 检查参数
            if (sqlContent == null ||
                sqlContent.IsNull) return null;
            // 获得参数
            string value = sqlContent.Value;
            // 检查参数
            if (value == null || value.Length <= 0) return null;
            // 返回结果
            return QuantityExtractor.Extract(value);
        }

        public static void NLPExtractQuantities_FillRow(object logResultObj, out SqlInt32 Index, out SqlInt32 Length, out SqlString Value)
        {
            FunctionSegment q = (FunctionSegment)logResultObj;
            Index = q.Index;
            Length = q.Length;
            Value = q.Value;
        }

        [Microsoft.SqlServer.Server.SqlFunction
            (DataAccess = DataAccessKind.Read,
                FillRowMethodName = "NLPExtractSentences_FillRow",
                TableDefinition = "SentenceValue nvarchar(4000)")]
        public static IEnumerable NLPExtractSentences(SqlString sqlContent)
        {
            // 检查参数
            if (sqlContent == null ||
                sqlContent.IsNull) return null;
            // 获得参数
            string value = sqlContent.Value;
            // 检查参数
            if (value == null || value.Length <= 0) return null;
            // 返回结果
            return SentenceExtractor.Extract(value);
        }

        public static void NLPExtractSentences_FillRow(object logResultObj, out SqlString Value)
        {
            Value = (string)logResultObj;
        }

        [Microsoft.SqlServer.Server.SqlFunction
            (DataAccess = DataAccessKind.Read,
                FillRowMethodName = "NLPSplitAll_FillRow",
                TableDefinition = "NMethod nvarchar(64), NCount int, NGamma float, NSegment nvarchar(4000)")]
        public static IEnumerable NLPSplitAll(SqlString sqlContent)
        {
            // 检查参数
            if (sqlContent == null ||
                sqlContent.IsNull) return null;
            // 获得参数
            string value = sqlContent.Value;
            // 检查参数
            if (value == null || value.Length <= 0) return null;
            // 获得分割结果
            CoreSegment segment = CoreSegment.Split(value);
            // 检查结果
            if (segment == null || segment.IsLeaf()) return null;
            // 返回结果
            return segment.Subsegs.
                OrderByDescending(s => s.Flag).ThenByDescending(s => s.Gamma).ToArray();
        }

        public static void NLPSplitAll_FillRow(object segResultObj,
            out SqlString Method, out SqlInt32 Count, out SqlDouble Gamma, out SqlString Segment)
        {
            CoreSegment segment = (CoreSegment)segResultObj;
            // 设置参数
            Method = string.Format
                    ("{0}[{1}]|{2}|{3}|{4}",
                    (segment.Flag & 0x080) != 0 ? "M" : "-",
                    (segment.Index < 0) ? "-" : segment.Index.ToString(),
                    (segment.Flag & 0x100) != 0 ? "C" : "-",
                    (segment.Flag & 0x200) != 0 ? "R" : "-",
                    (segment.Flag & 0x400) != 0 ? "R+" : "-");
            Gamma = segment.Gamma;
            Segment = segment.GetPath();
            Count = segment.Subsegs != null ? segment.Subsegs.Length : 0;
        }

        [Microsoft.SqlServer.Server.SqlFunction
            (DataAccess = DataAccessKind.Read,
                FillRowMethodName = "NLPSplitRPlus_FillRow",
                TableDefinition = "NIndex int, NGamma float, NCount int, NValue nvarchar(4000)")]
        public static IEnumerable NLPSplitRPlus(SqlString sqlContent)
        {
            // 检查参数
            if (sqlContent == null ||
                sqlContent.IsNull) return null;
            // 获得参数
            string value = sqlContent.Value;
            // 检查参数
            if (value == null || value.Length <= 0) return null;
            // 获得分割结果
            CoreSegment segment = GammaSpliter.Split(value);
            // 检查结果
            if (segment == null || segment.IsLeaf()) return null;
            // 选择第一个分割结果
            segment = segment.SelectPath();
            // 检查结果
            if (segment == null || segment.IsLeaf()) return null;
            // 索引
            int Index = 0;
            // 整理结果
            foreach(CoreSegment subseg in segment.Subsegs)
            {
                // 设置索引值
                subseg.Index = Index; Index += subseg.Length;
            }
            // 返回结果
            return segment.Subsegs;
        }

        public static void NLPSplitRPlus_FillRow(object segResultObj,
            out SqlInt32 Index, out SqlDouble Gamma, out SqlInt32 Count, out SqlString Value)
        {
            CoreSegment segment = (CoreSegment)segResultObj;
            // 设置参数
            Index = segment.Index;
            Count = segment.Count;
            Value = segment.Value;
            Gamma = segment.Gamma;
        }
    }
}
