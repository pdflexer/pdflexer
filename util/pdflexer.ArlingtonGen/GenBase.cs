
using DotNext;
using pdflexer.ArlingtonGen.Expressions;
using PdfLexer;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace pdflexer.ArlingtonGen;

internal abstract class GenBase
{
    public Row Row { get; }

    public GenBase(Row row)
    {
        Row = row;
    }

    public abstract string CreateClass();
    public abstract string Key { get; }
    public abstract string RootName { get; }
    public static string Ident(int num, string value)
    {
        return string.Join('\n', value.Split('\n').Select(x => "".PadLeft(num, ' ') + x));
    }

    public bool IsComplexType(Row row)
    {
        return row.Type.Contains(";") || row.Type.Contains(":"); // multi or fn
    }

    public string GetVersion()
    {
        if (decimal.TryParse(Row.SinceVersion, out _))
        {
            if (decimal.TryParse(Row.DeprecatedIn, out _))
            {
                return $"version >= {Row.SinceVersion}m && version < {Row.DeprecatedIn}m";
            }
            // since version
            return $"version >= {Row.SinceVersion}m";
        }
        return "false";
    }

    public static bool AppliesTo(Row row, decimal version)
    {
        if (decimal.TryParse(row.SinceVersion, out var sv))
        {
            if (decimal.TryParse(row.DeprecatedIn, out var di))
            {
                return version >= sv && version < di;
            }
            // since version
            return version >= sv;
        }
        return false;
    }

    public string NullCheckRequired(string prop, string root, string key)
    {
        var req = new Required(Row);
        if (req.IsSimple())
        {
            return req.GetSimple() ?
                $$"""if ({{prop}} == null) { ctx.Fail<APM_{{root}}_{{key}}>("{{key}} is required"); return; }""" :
                $$"""if ({{prop}} == null) { return; }""";
        }
        else
        {
            var setters = string.Join('\n', req.GetComplexVars().Where(x => !string.IsNullOrEmpty(x)).Distinct().Select(x => GetSetter(x, "array", "val")));
            return $$"""

{{setters}}
if (({{req.GetComplex()}}) && {{prop}} == null) {
    ctx.Fail<APM_{{root}}_{{key}}>("{{key}} is required"); return;
} else if ({{prop}} == null) {
    return;
}

""";
        }
    }

    public string GetLink(string key)
    {
        if (string.IsNullOrEmpty(Row.Link))
        {
            return "// no linked objects";
        }

        return GetLinkType(Row.Type, Row.Link, key);
    }

    public string GetLink(string type, string key)
    {
        if (string.IsNullOrEmpty(Row.Link))
        {
            return "// no linked objects";
        }

        var types = Row.Type.Split(';').ToList();
        var irs = Row.Link.Split(";");
        var link = irs.Length == 1 ? irs[0] : irs[types.IndexOf(type)];

        return GetLinkType(type, link, key);
    }

    private string GetLinkType(string type, string link, string key)
    {
        if (string.IsNullOrEmpty(link) || link.Trim() == "[]")
        {
            return "// no linked objects";
        }

        if (type == "name-tree" || type == "number-tree")
        {
            return "// TODO trees";
        }

        var nm = link.Trim('[').Trim(']');
        // var linkType = Arrays.Contains(nm) ? "PdfArray" : "PdfDictionary";
        var linkType = type switch
        {
            "array" => "PdfArray",
            "stream" => "PdfDictionary",
            "dictionary" => "PdfDictionary",
            _ => throw new ApplicationException("Not known: " + type)
        };
        var val = type == "stream" ? "val.Dictionary" : "val";

        if (link.Contains(","))
        {

            var txt = "";
            bool first = true;
            foreach (var t in PossibleValues.SplitWithFns(link.Trim('[').Trim(']')).Where(x => !x.Contains(":")))
            {
                txt += $$"""
{{(first ? "if" : " else if")}} (APM_{{t}}.MatchesType(ctx, {{val}})) 
{
    ctx.Run<APM_{{t}}, {{linkType}}>(stack, {{val}}, obj);
}
""";
                first = false;
            }
            foreach (var t in PossibleValues.SplitWithFns(link.Trim('[').Trim(']')).Where(x => x.Contains(":")))
            {
                var objName = "";
                var exp = Exp.Tokenize(t);
                var sbe = new StringBuilder();
                using var scope = new EvalScope((w, v, a) => {
                    w.Append($"APM_");
                    objName = v.Text;
                    w.Append(v.Text);
                    w.Append($".MatchesType(ctx, {val})");
                });
                exp.ForEach(x => x.Write(sbe));
                txt += $$"""
{{(first ? "if" : " else if")}} ({{sbe.ToString()}}) 
{
    ctx.Run<APM_{{objName}}, {{linkType}}>(stack, {{val}}, obj);
}
""";
                first = false;
            }
            txt += $$"""
else 
{
    ctx.Fail<APM_{{RootName}}_{{Key}}>("{{key.Trim('"')}} did not match any allowable types: '{{link}}'");
}
""";
            return txt;
        }

        return $$"""
ctx.Run<APM_{{nm}}, {{linkType}}>(stack, {{val}}, obj);
""";
    }

    protected string GetSingleComplexType(string key)
    {
        VariableContext.Vars[key] = "utval";
        var ir = new IndirectRef(Row).GetIndirectRefEnum();
        return $$"""
var (utval, wasIR) = ctx.GetOptional<APM_{{RootName}}_{{Key}}>(obj, {{key}}, {{ir}});
{{NullCheckRequired("utval", RootName, Key)}}
{{GetSingleComplexTypeChecks("utval")}}
""";
    }

    protected string GetSingleComplexTypeChecks(string varName)
    {
        var types = Row.Type.Split(';');
        return $$"""
switch ({{varName}}.Type) 
{
{{Ident(4, BuildCaseStatement(types))}}
    default:
        ctx.Fail<APM_{{RootName}}_{{Key}}>("{{Row.Key}} is required to one of '{{Row.Type}}', was " + {{varName}}.Type);
        return;
}
""";
    }

    protected string GetSingleSimpleType(string key)
    {
        var ir = new IndirectRef(Row).GetIndirectRefEnum();
        var req = new Required(Row);

        var type = GetLexerType();

        VariableContext.Vars[key] = "val";

        var txt = "";
        if (req.IsSimple())
        {
            
            txt += req.GetSimple() ? $"""var val = ctx.GetRequired<{type}, APM_{RootName}_{Key}>(obj, {key}, {ir});"""
                : $"""
var val = ctx.GetOptional<{type}, APM_{RootName}_{Key}>(obj, {key}, {ir});
""";
            txt += "\nif (val == null) { return; }\n";
        }
        else
        {
            txt += $$"""
{{string.Join('\n', req.GetComplexVars().Distinct().Select(x=> GetSetter(x, "array", "val")))}}
var val = ctx.GetOptional<{{type}}, APM_{{RootName}}_{{Key}}>(obj, {{key}}, {{ir}});
if (({{req.GetComplex()}}) && val == null) {
    ctx.Fail<APM_{{RootName}}_{{Key}}>("{{Key}} is required when '{{Row.Required}}"); return;
} else if (val == null) {
    return;
}

""";
        }
        txt += GetSingleSimpleTypeChecks(key);
        return txt;
    }

    protected string GetSingleSimpleTypeChecks(string key)
    {
        var sc = new SpecialCase(this, Row);
        var pv = new PossibleValues(this);
        var txt = "";
        txt += sc.GetSpecialCase() + '\n';
        txt += pv.GetPossibleValueCheck() + '\n';
        txt += GetLink(key);
        txt += "\n";
        return txt;
    }

    protected string BuildCaseStatement(string[] types)
    {
        var fns = types.Where(x => x.Contains("fn:"));
        var txt = "";
        if (fns.Any())
        {
            txt += "// TODO funcs: " + string.Join(",", fns) + "\n";
        }
        var orig = VariableContext.Vars.ToDictionary(x => x.Key, x => x.Value);
        foreach (var type in types.Where(x => !x.Contains("fn:")).GroupBy(x => typemap[x]))
        {
            VariableContext.Vars = orig.ToDictionary(x => x.Key, x => x.Value);
            var vals = type.ToList();
            if (vals.Count == 1)
            {
                txt += MultiCaseStatement(vals[0]);
            }
            else
            {
                txt += "\n// TODO MC " + string.Join(";", types) + "\n";
            }
        }

        return txt;
    }

    private string MultiCaseStatement(string type)
    {
        if (type.StartsWith("fn:")) { return "// TODO: " + type; }
        var sc = new SpecialCase(this, Row);
        var pv = new PossibleValues(this);
        return $$"""
case PdfObjectType.{{typemap[type]}}:
    {
{{Ident(8, $"var val =  ({typeDomMap[type]})utval;")}}
{{Ident(8, IRCheckMulti(type))}}
{{Ident(8, sc.GetSpecialCase(type))}}
{{Ident(8, pv.GetPossibleValueCheck(type))}}
{{Ident(8, GetLink(type, Key))}}
        return;
    }

""";
    }


    private static Regex badVar = new Regex("@[0-9\\*]+");

    internal string GetSetter(string value, string type, string name)
    {
        var typeDeclaration = type == "number" || type == "integer" ? "IPdfObject" : "var";
        var nm = value[0] == '@' ? value.Substring(1) : value;

        var cleansed = Regex.Replace(value, @"[^A-Za-z0-9]+", "");
        if (int.TryParse(cleansed, out _))
        {
            cleansed = "v" + cleansed;
        }
        int? i = null;
        while (VariableContext.Vars.ContainsValue(cleansed + i?.ToString()))
        {
            if (i == null) { i = 2; } else { i++; }
        }
        var clean = cleansed + i?.ToString();
        clean = clean.TrimStart('@');
        VariableContext.Vars[value] = clean;
        if (value.StartsWith("@"))
        {
            VariableContext.Vars[value.Substring(1)] = clean;
        }
        

        if (nm.Contains("::"))
        {
            var segs = nm.Split("::").ToList();
            if (segs.Last() == "" || segs.Last() == "*") 
            { 
                segs.RemoveAt(segs.Count - 1);
            }
            var txt = $"var {clean} = ";
            bool trim = true;
            if (segs[0] == "parent" && segs[1] == "parent")
            {
                txt += "stack.GetParent(2)";
                segs.RemoveAt(0);
                segs.RemoveAt(1);
                trim = false;
            } else if (segs[0] == "parent")
            {
                txt += "parent";
                segs.RemoveAt(0);
                trim = false;
            }
            else if (segs[0] == "*")
            {
                txt += "val";
                segs.RemoveAt(0);
                trim = true;
            }
            else
            {
                txt += "obj";
            }

            var props = string.Join("", segs.Select(x => $"?.Get(\"{x.TrimStart('@')}\")"));
            if (trim)
            {
                props = props.TrimStart('?');
            }
            return txt += props + ";";
        }
        if (int.TryParse(nm, out _))
        {
            return $"var {clean} = obj.Get({nm});";
        }
        return $"var {clean} = obj.Get(\"{nm}\");";
    }


    private string IRCheckMulti(string type)
    {
        var ir = new IndirectRef(Row);

        if (ir.TryGetSimple(type, out var val))
        {
            return val ?
                $$"""if (!wasIR) { ctx.Fail<APM_{{RootName}}_{{Key}}>("{{Row.Key}} is required to be indirect when a {{type}}"); return; }""" : "// no indirect obj reqs";
        }
        else
        {
            return $$"""
if ({{ir.GetComplex(type)}} && !wasIR) {
    ctx.Fail<APM_{{RootName}}_{{Key}}>("{{Row.Key}} is required to be indirect when a {{type}}");
    return;
}
""";
        }
    }

    internal static Regex FuncRegex = new Regex("^fn:([a-zA-z]+)\\((.*)\\)$", RegexOptions.Compiled);
    internal static Regex AnyFuncRegex = new Regex("fn:([a-zA-z0-9]+)\\(((?>\\((?<c>)|[^()]+|\\)(?<-c>))*(?(c)(?!)))\\)", RegexOptions.Compiled);
    internal static Regex ValueRegex = new Regex("@([a-zA-z0-9_\\-\\.*]+)", RegexOptions.Compiled);
    internal static Regex ValueOpRegex = new Regex("@([a-zA-z_\\-\\.]+)[ ]*(\\|\\||&&|==|>|<|>=|<=)[ ]*([^ ^)])+", RegexOptions.Compiled);
    internal static Regex RepeatedVarRegex = new Regex("@(([0-9]+[\\*]*)|\\*)", RegexOptions.Compiled);

    public static List<string> Arrays = new List<string>
    {

    };

    public string GetLexerType()
    {
        return typeDomMap[Row.Type];
    }

    protected static Dictionary<string, PdfObjectType> typemap = new Dictionary<string, PdfObjectType>
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
        {"number-tree", PdfObjectType.DictionaryObj },
        {"rectangle", PdfObjectType.ArrayObj },
        {"stream", PdfObjectType.StreamObj },
        {"string", PdfObjectType.StringObj },
        {"string-ascii", PdfObjectType.StringObj },
        {"string-byte", PdfObjectType.StringObj },
        {"string-text", PdfObjectType.StringObj },
    };

    protected static Dictionary<string, string> typeDomMap = new Dictionary<string, string>
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
        {"number-tree", "PdfDictionary" },
        {"rectangle", "PdfArray" },
        {"stream", "PdfStream" },
        {"string", "PdfString" },
        {"string-ascii", "PdfString" },
        {"string-byte", "PdfString" },
        {"string-text", "PdfString" },
    };
}
