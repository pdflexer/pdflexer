// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_VRIMap : APM_VRIMap_Base
{
}

internal partial class APM_VRIMap_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "VRIMap";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_VRIMap_CatchAll, PdfDictionary>(stack, obj, parent);
        
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        return false;
    }

    


}

/// <summary>
/// VRIMap_* Table 261, VRI cell
/// </summary>
internal partial class APM_VRIMap_CatchAll : APM_VRIMap_CatchAll_Base
{
}


internal partial class APM_VRIMap_CatchAll_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "VRIMap_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        foreach (var key in obj.Keys)
        {
            if (AllVals.Contains(key)) { continue; }
            
            
            var val = ctx.GetOptional<PdfDictionary, APM_VRIMap_CatchAll>(obj, key, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            // no value restrictions
            ctx.Run<APM_VRI, PdfDictionary>(stack, val, obj);
            
        }
        
    }

    public static HashSet<string> AllVals = new HashSet<string> {  };
}

