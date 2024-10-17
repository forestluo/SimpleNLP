using System;
using System.Data;
using System.Collections;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Text.RegularExpressions;

public partial class RegularExpression
{
    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlInt32 RegExOptions(SqlBoolean IgoreCase,
                                        SqlBoolean Multiline,
                                        SqlBoolean ExplicitCapture,
                                        SqlBoolean Compiled,
                                        SqlBoolean Singleline,
                                        SqlBoolean IgorePatternWhitesapce,
                                        SqlBoolean RightToLeft,
                                        SqlBoolean ECMAScript,
                                        SqlBoolean CultureInvariant)
    {
        // ����
        SqlInt32 options = 0;

        if (IgoreCase)
            options |= 0x01;
        if (Multiline)
            options |= 0x02;
        if (ExplicitCapture)
            options |= 0x04;
        if (Compiled)
            options |= 0x08;
        if (Singleline)
            options |= 0x10;
        if (IgorePatternWhitesapce)
            options |= 0x20;
        if (RightToLeft)
            options |= 0x40;
        if (ECMAScript)
            options |= 0x100;
        if (CultureInvariant)
            options |= 0x200;
        // ���ؽ��
        return options;
    }

    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlString RegExEscape(SqlString input)
    {
        // ������
        if (input.IsNull) return String.Empty;
        // ����ת����
        return Regex.Escape(input.Value);
    }

    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlString RegExMatch(SqlString pattern, SqlString input)
    {
        // ������
        if(input.IsNull || pattern.IsNull) return String.Empty;
        // ���ƥ����
        Match match = Regex.Match(input.Value, pattern.Value, RegexOptions.None);
        // ����ƥ����
        return match.Success ? match.Value : SqlString.Null;
    }

    //[Microsoft.SqlServer.Server.SqlFunction]
    //public static SqlString RegExMatchex(SqlString pattern, SqlString input, SqlInt32 Options)
    //{
    //    // ������
    //    if (input.IsNull || pattern.IsNull) return String.Empty;
    //    // ����ƥ����
    //    return Regex.Match(input.Value, pattern.Value, RegExOptionEnumeration(Options)).Value;
    //}

    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlBoolean RegExIsMatch(SqlString pattern, SqlString input)
    {
        // ������
        if (input.IsNull || pattern.IsNull) return SqlBoolean.False;
        // ����ƥ����
        return Regex.IsMatch(input.Value, pattern.Value, RegexOptions.None);
    }

    //[Microsoft.SqlServer.Server.SqlFunction]
    //public static SqlBoolean RegExIsMatchex(SqlString pattern, SqlString input, SqlInt32 Options)
    //{
    //    // ������
    //    if (input.IsNull || pattern.IsNull) return SqlBoolean.False;
    //    // ����ƥ����
    //    return Regex.IsMatch(input.Value, pattern.Value, RegExOptionEnumeration(Options));
    //}

    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlInt32 RegExIndex(SqlString pattern, SqlString input)
    {
        // ������
        if (input.IsNull || pattern.IsNull) return -1;
        // ���ƥ����
        Match match = Regex.Match(input.Value, pattern.Value, RegexOptions.None);
        // ����ƥ����
        return match.Success ? match.Index : -1;
    }

    //[Microsoft.SqlServer.Server.SqlFunction]
    //public static SqlInt32 RegExIndexex(SqlString pattern, SqlString input, SqlInt32 Options)
    //{
    //    // ������
    //    if (input.IsNull || pattern.IsNull) return -1;
    //    // ����ƥ����
    //    return Regex.Match(input.Value, pattern.Value, RegExOptionEnumeration(Options)).Index;
    //}

    [Microsoft.SqlServer.Server.SqlFunction
        (DataAccess = DataAccessKind.Read,
            FillRowMethodName = "RegExSplit_FillRow",
            TableDefinition = "SplitValue nvarchar(4000)")]
    public static IEnumerable RegExSplit(SqlString pattern, SqlString input)
    {
        // ������
        if (input.IsNull || pattern.IsNull) return null;
        // ���ؽ��
        return Regex.Split(input.Value, pattern.Value, RegexOptions.None);
    }

    //[Microsoft.SqlServer.Server.SqlFunction
    //    (DataAccess = DataAccessKind.Read,
    //        FillRowMethodName = "RegExSplit_FillRow",
    //        TableDefinition = "SplitValue nvarchar(4000)")]
    //public static IEnumerable RegExSplitex(SqlString pattern, SqlString input, SqlInt32 Options)
    //{
    //    // ������
    //    if (input.IsNull || pattern.IsNull) return null;
    //    // ���ؽ��
    //    return Regex.Split(input.Value, pattern.Value, RegExOptionEnumeration(Options));
    //}

    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlString RegExReplace(SqlString pattern, SqlString input, SqlString replacement)
    {
        // ������
        if (input.IsNull || pattern.IsNull) return SqlString.Null;
        // ����ƥ����
        return Regex.Replace(input.Value, pattern.Value, replacement.Value, RegexOptions.None);
    }

    //[Microsoft.SqlServer.Server.SqlFunction]
    //public static SqlString RegExReplacex(SqlString pattern, SqlString input, SqlString replacement, SqlInt32 Options)
    //{
    //    // ������
    //    if (input.IsNull || pattern.IsNull) return SqlString.Null;
    //    // ����ƥ����
    //    return Regex.Replace(input.Value, pattern.Value, replacement.Value, RegExOptionEnumeration(Options));
    //}

    [Microsoft.SqlServer.Server.SqlFunction
        (DataAccess = DataAccessKind.Read,
            FillRowMethodName = "RegExMatches_FillRow",
            TableDefinition = "MatchValue nvarchar(4000), MatchIndex int, MatchLength int")]
    public static IEnumerable RegExMatches(SqlString pattern, SqlString input)
    {
        // ������
        if (input.IsNull || pattern.IsNull) return null;
        // ���ؽ��
        return Regex.Matches(input.Value, pattern.Value, RegexOptions.None);
    }

    //[Microsoft.SqlServer.Server.SqlFunction
    //    (DataAccess = DataAccessKind.Read,
    //        FillRowMethodName = "RegExMatches_FillRow",
    //        TableDefinition = "MatchValue nvarchar(4000), MatchIndex int, MatchLength int")]
    //public static IEnumerable RegExMatchesex(SqlString pattern, SqlString input, SqlInt32 Options)
    //{
    //    // ������
    //    if (input.IsNull || pattern.IsNull) return null;
    //    // ���ؽ��
    //    return Regex.Matches(input.Value, pattern.Value, RegExOptionEnumeration(Options));
    //}

    public static void RegExSplit_FillRow(object splitResultObj, out SqlString Value)
    {
        Value = (string)splitResultObj;
    }

    public static void RegExMatches_FillRow(object matchResultObj, out SqlString Value, out SqlInt32 Index, out SqlInt32 Length)
    {
        Match match = (Match)matchResultObj;

        Value = match.Value;
        Index = match.Index;
        Length = match.Length;
    }

    public static RegexOptions RegExOptionEnumeration(SqlInt32 Options)
    {
        // ����ѡ��
        RegexOptions options = new RegexOptions();
        // ���ó�ʼֵ
        options = RegexOptions.None;
        // ����Ƿ�Ϊ��
        if (!Options.IsNull)
        {
            if ((Options.Value & 0x01) != 0)
                options |= RegexOptions.IgnoreCase;
            if ((Options.Value & 0x02) != 0)
                options |= RegexOptions.Multiline;
            if ((Options.Value & 0x04) != 0)
                options |= RegexOptions.ExplicitCapture;
            if ((Options.Value & 0x08) != 0)
                options |= RegexOptions.Compiled;
            if ((Options.Value & 0x10) != 0)
                options |= RegexOptions.Singleline;
            if ((Options.Value & 0x20) != 0)
                options |= RegexOptions.IgnorePatternWhitespace;
            if ((Options.Value & 0x40) != 0)
                options |= RegexOptions.RightToLeft;
            if ((Options.Value & 0x100) != 0)
                options |= RegexOptions.ECMAScript;
            if ((Options.Value & 0x200) != 0)
                options |= RegexOptions.CultureInvariant;
        }
        // ���ؽ��
        return options;
    }
}