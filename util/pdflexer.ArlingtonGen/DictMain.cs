
using pdflexer.ArlingtonGen.Expressions;
using System.Security.Cryptography.X509Certificates;

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
{{GenBase.Ident(8, ExtraCheck(rows))}}
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
{{GenBase.Ident(8, GetMatcher(rows))}}
    }

{{GenBase.Ident(4, AllowedFields(rows))}}


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


    public string ExtraCheck(List<Row> rows)
    {
        if (rows.Any(x => x.Key == "*")) { return ""; }
        var versions = new List<decimal>();
        for (var i = 1.0m; i <= 2.0m; i += 0.1m)
        {
            var matches = rows.Where(x => GenBase.AppliesTo(x, i)).ToList();
            if (matches.Count == 0) { continue; }
            versions.Add(i);
        }
            return $$""""
switch (ctx.Version) {
{{string.Join('\n', versions.Select( v=> $$"""
    case {{v.ToString("0.0")}}m:
        foreach (var extra in obj.Keys.Where(x=> !AllowedFields_{{v.ToString("0.0").Replace(".", "")}}.Contains(x)))
        {
            ctx.Fail<APM_{{Name}}>($"Unknown field {extra} for version {{v}}");
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
        for (var i = 1.0m; i <= 2.0m; i += 0.1m)
        {
            var matches = rows.Where(x => GenBase.AppliesTo(x, i)).ToList();
            if (matches.Count == 0) { continue; }
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
        }
        return values;
    }
}

