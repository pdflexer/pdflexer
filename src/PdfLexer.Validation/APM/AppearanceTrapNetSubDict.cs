// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_AppearanceTrapNetSubDict : APM_AppearanceTrapNetSubDict__Base
{
}

internal partial class APM_AppearanceTrapNetSubDict__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "AppearanceTrapNetSubDict";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_AppearanceTrapNetSubDict_CatchAll, PdfDictionary>(stack, obj, parent);
        
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        return false;
    }

    


}

/// <summary>
/// AppearanceTrapNetSubDict_* Table 170 and Clause 12.5.5
/// </summary>
internal partial class APM_AppearanceTrapNetSubDict_CatchAll : APM_AppearanceTrapNetSubDict_CatchAll__Base
{
}


internal partial class APM_AppearanceTrapNetSubDict_CatchAll__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AppearanceTrapNetSubDict_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        foreach (var key in obj.Keys)
        {
            if (AllVals.Contains(key)) { continue; }
            
            
            var val = ctx.GetOptional<PdfStream, APM_AppearanceTrapNetSubDict_CatchAll>(obj, key, IndirectRequirement.MustBeIndirect);
            if (val == null) { return; }
            // no special cases
            // no value restrictions
            ctx.Run<APM_XObjectFormTrapNet, PdfDictionary>(stack, val.Dictionary, obj);
            
        }
        
    }

    public static HashSet<string> AllVals = new HashSet<string> {  };
}

