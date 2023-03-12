using pdflexer.ArlingtonGen.Expressions;
using System.Data;

namespace pdflexer.ArlingtonGen;

internal class DictChild : GenBase
{
    public DictChild(DictMain main, List<Row> all, Row row) : base(row)
    {
        Root = main;
        Rows = all;
        Key = Row.Key.Replace("*", "CatchAll").Replace(".", "_");
    }

    public DictMain Root { get; }
    public List<Row> Rows { get; }
    public override string RootName { get => Root.Name; }

    public override string Key { get ; }

    public override string CreateClass()
    {
        VariableContext.VarSub = "val";
        VariableContext.VarName = "val";
        VariableContext.Vars.Clear();
        var key = "\"" + Key + "\"";
        var code = $$"""
/// <summary>
/// {{Root.Name}}_{{Row.Key}} {{Row.Notes}}
/// </summary>
internal partial class APM_{{Root.Name}}_{{Key}} : APM_{{Root.Name}}_{{Key}}__Base
{
}


internal partial class APM_{{Root.Name}}_{{Key}}__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "{{Root.Name}}_{{Row.Key}}";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return {{GetVersion()}}; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
{{Ident(8, IsRepeated() ? GetRepeated() : GetSingle(key))}}
    }

{{(IsRepeated() ? Vals() : null )}}
}

""";
        return code;
    }

    private string Vals()
    {
        return "    public static HashSet<string> AllVals = new HashSet<string> { " + string.Join(", ", Rows.Where(x => x.Key != "*").Select(x => $"\"{x.Key}\"")) + " };";
    }
    
    private bool IsRepeated()
    {
        return Row.Key == "*";
    }

    private string GetSingle(string key)
    {
        return IsComplexType(Row) ? GetSingleComplexType(key) : GetSingleSimpleType(key);
    }

    private string GetRepeated()
    {
        VariableContext.Vars["*"] = "val";
        VariableContext.Vars["@*"] = "val";
        return $$"""

foreach (var key in obj.Keys)
{
    if (AllVals.Contains(key)) { continue; }
    
    
{{Ident(4, IsComplexType(Row) ? GetSingleComplexType("key") : GetSingleSimpleType("key"))}}
}

""";
    }


}
