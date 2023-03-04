// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_GraphicsStateParameterMap : APM_GraphicsStateParameterMap_Base
{
}

internal partial class APM_GraphicsStateParameterMap_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "GraphicsStateParameterMap";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_GraphicsStateParameterMap_CatchAll, PdfDictionary>(stack, obj, parent);
        
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        return false;
    }

    


}

/// <summary>
/// GraphicsStateParameterMap_* Table 34
/// </summary>
internal partial class APM_GraphicsStateParameterMap_CatchAll : APM_GraphicsStateParameterMap_CatchAll_Base
{
}


internal partial class APM_GraphicsStateParameterMap_CatchAll_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "GraphicsStateParameterMap_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        foreach (var key in obj.Keys)
        {
            if (AllVals.Contains(key)) { continue; }
            
            
            var val = ctx.GetOptional<PdfDictionary, APM_GraphicsStateParameterMap_CatchAll>(obj, key, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            // no value restrictions
            ctx.Run<APM_GraphicsStateParameter, PdfDictionary>(stack, val, obj);
            
        }
        
    }

    public static HashSet<string> AllVals = new HashSet<string> {  };
}

