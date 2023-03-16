
using pdflexer.ArlingtonGen.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace pdflexer.ArlingtonGen;

internal class DictMain
{
    public DictMain(string objName)
    {
        Name = objName;
    }

    public string Name { get; }

    public string CreateClass(List<Row> rows)
    {
        VariableContext.VarSub = "var";
        VariableContext.VarName = "var";
        VariableContext.Vars.Clear();

        var (fields, defs) = GetAllowedFields(rows);

        var code = $$"""
using System.Linq;

internal partial class APM_{{Name}} : APM_{{Name}}__Base
{
}

internal partial class APM_{{Name}}__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "{{Name}}";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
{{string.Join("\n", rows.Where(x => !x.Key.Contains(":")).Select(x => $"        ctx.Run<APM_{Name}_{new DictChild(this, rows, x).Key}, PdfDictionary>(stack, obj, parent);"))}}
{{GenBase.Ident(8, ExtraCheck(rows, fields))}}
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
{{GenBase.Ident(8, GetMatcher(rows))}}
    }

{{GenBase.Ident(4, defs)}}


}

""";
        return code;
    }

    public string GetMatcher(List<Row> rows)
    {
        var t = rows.FirstOrDefault(x => x.Key == "Type");
        var st = rows.FirstOrDefault(x => x.Key == "Subtype");

        var txt = "";
        if (t != null)
        {
            var stc = new DictChild(this, rows, t);
            txt += $$"""
c.Run<APM_{{Name}}_{{stc.Key}}, PdfDictionary>(new CallStack(), obj, null);
""";
        }
        if (st != null)
        {
            var stc = new DictChild(this, rows, st);
            txt += $$"""

c.Run<APM_{{Name}}_{{stc.Key}}, PdfDictionary>(new CallStack(), obj, null);
""";
        }

        if (txt == "") { return "return false;"; }

        return $$"""
var c = ctx.Clone();
{{txt}}
if (c.Errors.Any())
{
    return false;
}
return true;
""";
    }


    public string ExtraCheck(List<Row> rows, Dictionary<decimal, string> versions)
    {
        if (rows.Any(x => x.Key == "*")) { return ""; }

            return $$""""
switch (ctx.Version) {
{{string.Join('\n', versions.GroupBy(x=>x.Value).Select(v=> $$"""
{{GenBase.Ident(4, (string.Join('\n', v.Select(kv=> $"case {kv.Key.ToString("0.0")}m:"))))}}
        foreach (var extra in obj.Keys.Where(x=> !AllowedFields_{{v.Key}}.Contains(x)))
        {
            ctx.Fail<APM_{{Name}}>($"Unknown field {extra} for version {ctx.Version}");
        }
        break;
"""))}}
    default:
        break;
}
"""";
    }

    public string AllowedFields(List<Row> rows)
    {
        if (rows.Any(x => x.Key == "*")) { return ""; }
        var values = "";
        List<string>? prev = null;
        for (var i = 1.0m; i <= 2.0m; i += 0.1m)
        {
            var matches = rows.Where(x => GenBase.AppliesTo(x, i)).ToList();
            if (matches.Count == 0) { continue; }
            var keys = matches.Select(x => "\"" + x.Key + "\"").OrderBy(x=>x).ToList();

            if (prev != null && prev.SequenceEqual(keys))
            {

            }
            if (matches.Count > 5)
            {
                values += $$"""
public static HashSet<string> AllowedFields_{{i.ToString("0.0").Replace(".", "")}} { get; } = new HashSet<string> 
{
    {{string.Join(", ", matches.Select(x => "\"" + x.Key + "\""))}}
};

""";
            }
            else
            {
                values += $$"""
public static List<string> AllowedFields_{{i.ToString("0.0").Replace(".", "")}} { get; } = new List<string> 
{
    {{string.Join(", ", matches.Select(x => "\"" + x.Key + "\""))}}
};

""";
            }
            prev = keys;
        }
        return values;
    }

    public (Dictionary<decimal, string> Fields, string Defs) GetAllowedFields(List<Row> rows)
    {
        if (rows.Any(x => x.Key == "*")) { return (new Dictionary<decimal, string>(), ""); }
        var values = "";
        List<string>? prev = null;

        var lu = new Dictionary<string, (List<string> Vals, List<decimal> Vers)>();

        for (var i = 1.0m; i <= 2.0m; i += 0.1m)
        {
            var matches = rows.Where(x => GenBase.AppliesTo(x, i)).ToList();
            if (matches.Count == 0) { continue; }
            var keys = matches.Select(x => "\"" + x.Key + "\"").OrderBy(x => x).ToList();
            var key = string.Join('-', keys);
            if (lu.TryGetValue(key, out var items))
            {
                items.Vers.Add(i);
            } else
            {
                lu[key] = (keys, new List<decimal> { i });
            }

            if (prev != null && prev.SequenceEqual(keys))
            {

            }
            prev = keys;
        }

        var fields = new Dictionary<decimal, string>();

        foreach (var kvp in lu)
        {
            var nm = string.Join('_', kvp.Value.Vers.OrderBy(x => x).Select(x => x.ToString("0.0").Replace(".", "")));

            foreach (var ver in kvp.Value.Vers)
            {
                fields[ver] = nm;
            }

            if (kvp.Value.Vals.Count > 5)
            {
                values += $$"""
public static HashSet<string> AllowedFields_{{nm}} { get; } = new HashSet<string> 
{
    {{string.Join(", ", kvp.Value.Vals)}}
};

""";
            }
            else
            {
                values += $$"""
public static List<string> AllowedFields_{{nm}} { get; } = new List<string> 
{
    {{string.Join(", ", kvp.Value.Vals)}}
};

""";
            }
        }
        

        return (fields,values);
    }
}

