using pdflexer.ArlingtonGen.Expressions;

namespace pdflexer.ArlingtonGen;

internal class ArrayChild : GenBase
{
    public ArrayChild(ArrayMain main, Row row, List<Row> all) : base(row)
    {
        Root = main;
        Key = Row.Key.Replace("*", "x").Replace(".", "_");
        All = all;
    }

    public ArrayMain Root { get; }
    public override string RootName { get => Root.Name; }

    public override string Key { get; }
    public List<Row> All { get; }

    public override string CreateClass()
    {
        VariableContext.Vars.Clear();
        var code = $$"""
/// <summary>
/// {{Root.Name}}_{{Row.Key}} {{Row.Notes}}
/// </summary>
internal partial class APM_{{Root.Name}}_{{Key}} : ISpecification<PdfArray>
{
    public static string Name { get; } = "{{Root.Name}}_{{Row.Key}}";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return {{GetVersion()}}; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
{{Ident(8, IsRepeated() ? GetRepeated() : GetSingle())}}
    }
}

""";
        return code;
    }

    private bool IsRepeated()
    {
        return Row.Key.Contains("*");
    }


    private string GetRepeated()
    {
        if (Row.Key == "*")
        {
            VariableContext.Vars["*"] = "val";
            VariableContext.Vars["@*"] = "val";
            return $$"""
for (var i = 0; i<obj.Count; i+=1) 
{
    CheckSingle(i);
}
void CheckSingle(int n) 
{
{{Ident(4, IsComplexType(Row) ? GetSingleComplexType("n") : GetSingleSimpleType("n"))}}
}
""";
        }
        var n = int.Parse(Row.Key.TrimEnd('*'));
        // VariableContext.Vars[Row.Key] = "val";
        VariableContext.Vars["@" + Row.Key] = "val";
        var t = All.Count;

        return $$"""
for (var i = {{n}}; i<obj.Count; i+={{t}}) 
{
    CheckSingle(i);
}
void CheckSingle(int n) 
{
{{Ident(4, IsComplexType(Row) ? GetSingleComplexType("n") : GetSingleSimpleType("n"))}}
}
""";
    }

    private string GetSingle()
    {
        VariableContext.Vars["@"+Row.Key] = "val";
        return IsComplexType(Row) ? GetSingleComplexType(Key) : GetSingleSimpleType(Key);
    }
}
