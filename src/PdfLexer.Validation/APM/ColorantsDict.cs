// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_ColorantsDict : APM_ColorantsDict__Base
{
}

internal partial class APM_ColorantsDict__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "ColorantsDict";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_ColorantsDict_CatchAll, PdfDictionary>(stack, obj, parent);
        
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        return false;
    }

    


}

/// <summary>
/// ColorantsDict_* Table 70, Colorants cell
/// </summary>
internal partial class APM_ColorantsDict_CatchAll : APM_ColorantsDict_CatchAll__Base
{
}


internal partial class APM_ColorantsDict_CatchAll__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ColorantsDict_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        foreach (var key in obj.Keys)
        {
            if (AllVals.Contains(key)) { continue; }
            
            
            var val = ctx.GetOptional<PdfArray, APM_ColorantsDict_CatchAll>(obj, key, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            // no value restrictions
            ctx.Run<APM_SeparationColorSpace, PdfArray>(stack, val, obj);
            
        }
        
    }

    public static HashSet<string> AllVals = new HashSet<string> {  };
}

