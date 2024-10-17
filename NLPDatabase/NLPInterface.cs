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
            // ���ؽ��
            return new SqlString("NLDB v3.1.1.0");
        }

        [Microsoft.SqlServer.Server.SqlProcedure]
        public static void NLPCatchException(SqlString sqlTips)
        {
            // ������
            if (sqlTips == null ||
                sqlTips.IsNull) return;
            // �������
            string value = sqlTips.Value;
            // ������
            if (value == null || value.Length <= 0) return;
            // ��׽�쳣
            ExceptionLog.CatchException(value);
        }

        [Microsoft.SqlServer.Server.SqlProcedure]
        public static void NLPDeleteContent(SqlString sqlContent)
        {
            // ������
            if (sqlContent == null ||
                sqlContent.IsNull) return;
            // �������
            string value = sqlContent.Value;
            // ������
            if (string.IsNullOrEmpty(value)) return;

            // ɾ������
            MarkCache.DeleteMarks(value);
            // ɾ������
            CoreCache.DeleteSegment(value);

            // ��������
            GammaTool.MakeStatistic(value);
            // ��¼����
            OperationLog.Log("NLPInterface.NLPDeleteContent", "del " + value);
        }

        [Microsoft.SqlServer.Server.SqlProcedure]
        public static void NLPAddContent(SqlString sqlContent, SqlString sqlClass, SqlString sqlSubclass)
        {
            // ������
            if (sqlContent == null || sqlContent.IsNull) return;
            // �������
            string value = sqlContent.Value;
            // ������
            if (string.IsNullOrEmpty(value)) return;

            // ��ü���
            int count = CounterTool.GetCount(value, true);
            // ��������
            CoreCache.AddSegment(value, count < 0 ? 0 : count);
            // ��¼����
            OperationLog.Log("NLPInterface.AddContent", "add " + value);

            // ��������
            MarkAttribute mark = new MarkAttribute();
            // ���ò���
            mark.Index = 0;
            mark.Value = 
                (sqlClass != null && sqlClass.IsNull) ? sqlClass.Value : null;
            mark.Remark = 
                (sqlSubclass != null && sqlSubclass.IsNull) ? sqlSubclass.Value : null;
            // ��������
            if(!string.IsNullOrEmpty(mark.Value) &&
                WordType.IsMajor(mark.Value))
            {
                // ���������
                if(string.IsNullOrEmpty(mark.Remark) ||
                    WordType.IsMinor(mark.Remark))
                {
                    // ��������
                    MarkCache.AddMark(value, mark);
                    // ��¼����
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
            // ��ò���
            string value = sqlContent.Value;
            // ������
            if (value == null || value.Length <= 0) return -1;
            // ���ؽ��
            return ChineseTool.IsChinese(value) ? 1 : 0;
        }

        [Microsoft.SqlServer.Server.SqlFunction]
        public static SqlString NLPClearContent(SqlString sqlContent)
        {
            if (sqlContent == null ||
                sqlContent.IsNull) return SqlString.Null;
            // ��ò���
            string value = sqlContent.Value;
            // ������
            if (value == null || value.Length <= 0) return SqlString.Null;
            // ���ؽ��
            return MiscTool.ClearContent(value);
        }

        [Microsoft.SqlServer.Server.SqlFunction]
        public static SqlInt32 NLPGetCount(SqlString sqlContent, SqlInt32 sqlExtend)
        {
            if(sqlContent == null ||
                sqlContent.IsNull) return -1;
            // ��ò���
            string value = sqlContent.Value;
            // ������
            if (value == null || value.Length <= 0) return -1;
            // ���ؽ��
            return CounterTool.GetCount(value, sqlExtend.Value > 0);
        }

        [Microsoft.SqlServer.Server.SqlFunction
            (DataAccess = DataAccessKind.Read,
                FillRowMethodName = "NLPExtractQuantities_FillRow",
                TableDefinition = "QIndex int, QLength int, QValue nvarchar(4000)")]
        public static IEnumerable NLPExtractQuantities(SqlString sqlContent)
        {
            // ������
            if (sqlContent == null ||
                sqlContent.IsNull) return null;
            // ��ò���
            string value = sqlContent.Value;
            // ������
            if (value == null || value.Length <= 0) return null;
            // ���ؽ��
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
            // ������
            if (sqlContent == null ||
                sqlContent.IsNull) return null;
            // ��ò���
            string value = sqlContent.Value;
            // ������
            if (value == null || value.Length <= 0) return null;
            // ���ؽ��
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
            // ������
            if (sqlContent == null ||
                sqlContent.IsNull) return null;
            // ��ò���
            string value = sqlContent.Value;
            // ������
            if (value == null || value.Length <= 0) return null;
            // ��÷ָ���
            CoreSegment segment = CoreSegment.Split(value);
            // �����
            if (segment == null || segment.IsLeaf()) return null;
            // ���ؽ��
            return segment.Subsegs.
                OrderByDescending(s => s.Flag).ThenByDescending(s => s.Gamma).ToArray();
        }

        public static void NLPSplitAll_FillRow(object segResultObj,
            out SqlString Method, out SqlInt32 Count, out SqlDouble Gamma, out SqlString Segment)
        {
            CoreSegment segment = (CoreSegment)segResultObj;
            // ���ò���
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
            // ������
            if (sqlContent == null ||
                sqlContent.IsNull) return null;
            // ��ò���
            string value = sqlContent.Value;
            // ������
            if (value == null || value.Length <= 0) return null;
            // ��÷ָ���
            CoreSegment segment = GammaSpliter.Split(value);
            // �����
            if (segment == null || segment.IsLeaf()) return null;
            // ѡ���һ���ָ���
            segment = segment.SelectPath();
            // �����
            if (segment == null || segment.IsLeaf()) return null;
            // ����
            int Index = 0;
            // ������
            foreach(CoreSegment subseg in segment.Subsegs)
            {
                // ��������ֵ
                subseg.Index = Index; Index += subseg.Length;
            }
            // ���ؽ��
            return segment.Subsegs;
        }

        public static void NLPSplitRPlus_FillRow(object segResultObj,
            out SqlInt32 Index, out SqlDouble Gamma, out SqlInt32 Count, out SqlString Value)
        {
            CoreSegment segment = (CoreSegment)segResultObj;
            // ���ò���
            Index = segment.Index;
            Count = segment.Count;
            Value = segment.Value;
            Gamma = segment.Gamma;
        }
    }
}
