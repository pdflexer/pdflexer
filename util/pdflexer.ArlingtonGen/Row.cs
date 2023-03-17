using PdfLexer;
using System.ComponentModel;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace pdflexer.ArlingtonGen;

internal record Row(string Key, string Type, string SinceVersion, string DeprecatedIn, string Required,
    string IndirectReference, string Inheritable, string DefaultValue, string PossibleValues,
    string SpecialCase, string Link, string Notes);

internal class CodeGen
{
    public CodeGen(string rootType, string objName)
    {
        RootType = rootType;
        RootObj = objName;
    }


    public string CreateParentFile(List<Row> rows)
    {
        var code = $$"""
public partial class {{RootObj}} : ISpecification<{{RootType}}>
{
    public static string Name { get; } = "{{RootObj}}";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(ValidationContext ctx, {{RootType}} obj, IPdfObject? parent)
    {
{{string.Join("\n", rows.Select(x => $"        ctx.Run<{RootObj}_{x.Key}, {RootType}>(obj, parent);"))}}
    }
}




""";
        return code;
    }



    public bool IsComplexType(Row row)
    {
        return row.Type.Contains(";") || row.Type.Contains(":"); // multi or fn
    }

    public string GetComplexType(Row row)
    {
        var types = row.Type.Split(';');
        return $$"""
var (val, wasIR) = ctx.GetOptional<{{RootObj}}_{{row.Key}}>(obj, "{row.Key}", {{GetIndirectRef(row)}});
{{NullCheckRequired(row)}}
switch (val.Type) 
{
{{Ident(4, string.Join('\n', types.Select(x=> MultiCaseStatement(x, row))))}}
    default:
        ctx.Fail<{{RootObj}}_{{row.Key}}>("{{row.Key}} is required to one of '{{row.Type}}', was " + val.Type);
        return;
}
""";
    }
    public string MultiCaseStatement(string type, Row row)
    {
        return $$"""
case PdfObjectType.{{typemap[type]}}
{{Ident(4, IRCheckMulti(row, type))}}
{{Ident(4, GetLink(row ,type))}}
    return;
""";
    }

    public string GetLink(Row row)
    {
        if (string.IsNullOrEmpty(row.Link))
        {
            return "// no linked objects";
        }

        return $$"""
ctx.Run<{{row.Link.Trim('[').Trim(']')}}, PdfDictionary>(obj, parent);
""";
    }

    public string GetLink(Row row, string type)
    {
        if (string.IsNullOrEmpty(row.Link))
        {
            return "// no linked objects";
        }
        var types = row.Type.Split(';').ToList();
        var irs = row.Link.Split(";");
        var link = irs.Length == 1 ? irs[0] : irs[types.IndexOf(type)];

        return $$"""
ctx.Run<{{link.Trim('[').Trim(']')}}, PdfDictionary>(obj, parent);
""";
    }

    public string IRCheckMulti(Row row, string type)
    {
        var types = row.Type.Split(';').ToList();
        var irs = row.IndirectReference.Split(";");
        var requiresIndirect = irs.Length == 1 ? irs[0] : irs[types.IndexOf(type)];

        if (IsSimpleBool(requiresIndirect))
        {
            return GetSimpleBoolValue(requiresIndirect) ?
                $$"""if (!wasIR) { ctx.Fail<{{RootObj}}_{{row.Key}}>("{{row.Key}} is required to be indirect when a {{type}}"); return; }""" : "// no indirect obj reqs";
        }
        else
        {
            return $$"""
if ({{CreateRequiredCondition(row)}} && val == null) {
    ctx.Fail<{{RootObj}}_{{row.Key}}>("{{row.Key}} is required"); return;
} else {
    return;
}
""";
        }
    }

    public string NullCheckRequired(Row row)
    {
        if (IsSimpleRequired(row))
        {
            return GetSimpleBoolRequired(row) ? 
                $$"""if (val == null) { return; }"""
                : $$"""if (val == null) { ctx.Fail<{{RootObj}}_{{row.Key}}>("{{row.Key}} is required"); return; }""";
        }
        else
        {
            return $$"""
if ({{CreateRequiredCondition(row)}} && val == null) {
    ctx.Fail<{{RootObj}}_{{row.Key}}>("{{row.Key}} is required"); return;
} else {
    return;
}
""";
        }
    }

    public static string GetLexerType(Row row)
    {
        return typeDomMap[row.Type];
    }
    public static string CreateRequiredCondition(Row row)
    {
        var (f, v) = GetFuncValue(row.Required);
        Console.WriteLine(row.Required);
        return $"""false""";
    }

    public static string CreateConditionText(string name)
    {
        var (f, v) = GetFuncValue(name);
        if (IsFunc(v))
        {
            return HandleSingleFunc(f, CreateConditionText(v));
        }
        else
        {
            return HandleSingleFunc(f, HandleSingleStatement(v));
        }
    }

    public static string HandleSingleStatement(string input)
    {
        return "true";
    }

    public static string HandleSingleFunc(string func, string input)
    {
        return "true";
    }

    public static string Ident(int spaces, string value)
    {
        return string.Join('\n', value.Split('\n').Select(x => "".PadLeft(spaces, ' ') + x));
    }

    public static string GetIndirectRef(Row row)
    {
        if (funcRegex.IsMatch(row.IndirectReference))
        {
            return HandleIRFunc(row);
        }

        switch (row.IndirectReference)
        {
            case "fn:MustBeDirect()":
                return "IndirectRequirement.MustBeDirect";
            case "TRUE":
                return "IndirectRequirement.MustBeIndirect";
            case "FALSE":
            default:
                return "IndirectRequirement.Either";
        }
    }

    internal static Regex funcRegex = new Regex("^fn:([a-zA-z]+)\\((.*)\\)$", RegexOptions.Compiled);

    public static bool IsSimpleRequired(Row row)
    {
        switch (row.Required)
        {
            case "FALSE":
            case "TRUE":
                return true;
            default:
                return false;
        }
    }

    public static bool GetSimpleBoolRequired(Row row)
    {
        switch (row.Required)
        {
            case "FALSE":
                return false;
            case "TRUE":
            default:
                return true;
        }
    }

    public static bool IsSimpleBool(string val)
    {
        switch (val)
        {
            case "[FALSE]":
            case "[TRUE]":
            case "FALSE":
            case "TRUE":
                return true;
            default:
                return false;
        }
    }

    public static bool GetSimpleBoolValue(string val)
    {
        switch (val)
        {
            case "[FALSE]":
            case "FALSE":
                return false;
            case "[TRUE]":
            case "TRUE":
            default:
                return true;
        }
    }

    public static string HandleIRFunc(Row value)
    {
        Console.WriteLine(value);
        return "IndirectRequirement.Either";
    }

    public static string HandleReqFunc(Row value)
    {
        Console.WriteLine(value);
        GetFuncValue(value.Required);
        return "true";
    }

    public static (string Func, string Input) GetFuncValue(string name)
    {
        var match = funcRegex.Match(name);
        return (match.Groups[1].Value, match.Groups[2].Value);
    }

    public static bool IsFunc(string name)
    {
        return funcRegex.IsMatch(name);
    }


    internal static Dictionary<string, PdfObjectType> typemap = new Dictionary<string, PdfObjectType>
    {
        {"array", PdfObjectType.ArrayObj },
        {"bitmask", PdfObjectType.NumericObj },
        {"boolean", PdfObjectType.BooleanObj },
        {"date", PdfObjectType.StringObj },
        {"dictionary", PdfObjectType.DictionaryObj },
        {"integer", PdfObjectType.NumericObj },
        {"matrix", PdfObjectType.ArrayObj },
        {"name", PdfObjectType.NameObj },
        {"name-tree", PdfObjectType.DictionaryObj },
        {"null", PdfObjectType.NullObj },
        {"number", PdfObjectType.NumericObj },
        {"number-tree", PdfObjectType.NumericObj },
        {"rectangle", PdfObjectType.ArrayObj },
        {"stream", PdfObjectType.StreamObj },
        {"string", PdfObjectType.StringObj },
        {"string-ascii", PdfObjectType.StringObj },
        {"string-byte", PdfObjectType.StringObj },
        {"string-text", PdfObjectType.StringObj },
    };

    internal static Dictionary<string, string> typeDomMap = new Dictionary<string, string>
    {
        {"array", "PdfArray" },
        {"bitmask", "PdfIntNumber" },
        {"boolean", "PdfBoolean" },
        {"date", "PdfString" },
        {"dictionary", "PdfDictionary" },
        {"integer", "PdfIntNumber" },
        {"matrix", "PdfArray" },
        {"name", "PdfName" },
        {"name-tree", "PdfDictionary" },
        {"null", "PdfNull" },
        {"number", "PdfNumber" },
        {"number-tree", "NumberTree" },
        {"rectangle", "PdfArray" },
        {"stream", "PdfStream" },
        {"string", "PdfString" },
        {"string-ascii", "PdfString" },
        {"string-byte", "PdfString" },
        {"string-text", "PdfString" },
    };

    internal static Dictionary<PdfObjectType, string> typeRawnames = new Dictionary<PdfObjectType, string>
    {
        {PdfObjectType.ArrayObj,  "PdfArray" },
        {PdfObjectType.NumericObj, "PdfNumber" },
        {PdfObjectType.BooleanObj, "PdfBoolean" },
        {PdfObjectType.StringObj, "PdfString" },
        {PdfObjectType.DictionaryObj, "PdfDictionary" },
        {PdfObjectType.NameObj, "PdfName" },
        {PdfObjectType.NullObj, "PdfNull" },
    };

    public string RootType { get; }
    public string RootObj { get; }
}

internal static class Util
{

    internal static string CreateDictProperty(Row row)
    {
        if (string.IsNullOrWhiteSpace(row.Link) && string.IsNullOrWhiteSpace(row.Link))
        {

        }

        var type = ""; // Maps.typeDomMap[row.Type];
        var valsettxt = "value";
        if (row.IndirectReference == "TRUE")
        {
            valsettxt += ".Indirectly()";
        }

        var gettxt = $"return NativeObject.Get<{type}>(PdfName.{row.Key})";
        if (!string.IsNullOrEmpty(row.DefaultValue))
        {
            if (row.DefaultValue.StartsWith("@"))
            {
                var fallback = row.DefaultValue.TrimStart('@');
                gettxt = $"return NativeObject.GetWithFallbackKey<{type}>(PdfName.{row.Key}, PdfName.{fallback})";
            }
            else
            {
                gettxt = $"return NativeObject.Get<{type}>(PdfName.{row.Key}) ?? {row.DefaultValue}";
            }
        }


        var data = $@"
public {type} {row.Key} {{
    get {{ {gettxt}; }}
    set {{ NativeObject[PdfName.{row.Key}] = {valsettxt}; }}
}}
";
        return data;
    }
}

