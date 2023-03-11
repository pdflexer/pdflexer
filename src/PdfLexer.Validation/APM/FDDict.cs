// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_FDDict : APM_FDDict__Base
{
}

internal partial class APM_FDDict__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "FDDict";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_FDDict_CatchAll, PdfDictionary>(stack, obj, parent);
        
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        return false;
    }

    


}

/// <summary>
/// FDDict_* Table 122 and Clause 9.8.3.3
/// </summary>
internal partial class APM_FDDict_CatchAll : APM_FDDict_CatchAll__Base
{
}


internal partial class APM_FDDict_CatchAll__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FDDict_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        foreach (var key in obj.Keys)
        {
            if (AllVals.Contains(key)) { continue; }
            
            
            var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_FDDict_CatchAll>(obj, key, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            // no value restrictions
            ctx.Run<APM_CIDFontDescriptorMetrics, PdfDictionary>(stack, val, obj);
            
        }
        
    }

    public static HashSet<string> AllVals = new HashSet<string> {  };
}

