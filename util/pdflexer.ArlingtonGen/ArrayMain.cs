using pdflexer.ArlingtonGen.Expressions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdflexer.ArlingtonGen;

internal class ArrayMain
{
    public ArrayMain(string objName)
    {
        Name = objName;
    }

    public string Name { get; }

    public string CreateClass(List<Row> rows)
    {
        VariableContext.VarSub = "val";
        VariableContext.VarName = "val";
        VariableContext.Vars.Clear();
        var code = $$"""
internal partial class APM_{{Name}} : ISpecification<PdfArray>
{
    public static string Name { get; } = "{{Name}}";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
{{string.Join("\n", rows.Select(x => $"        ctx.Run<APM_{Name}_{new ArrayChild(this, x, rows).Key}, PdfArray>(stack, obj, parent);"))}}

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

""";
        return code;
    }
}
