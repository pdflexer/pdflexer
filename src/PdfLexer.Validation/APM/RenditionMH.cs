// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_RenditionMH : APM_RenditionMH__Base
{
}

internal partial class APM_RenditionMH__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "RenditionMH";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_RenditionMH_C, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
            case 1.6m:
            case 1.7m:
            case 1.8m:
            case 1.9m:
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15_16_17_18_19_20.Contains(x)))
                {
                    ctx.Fail<APM_RenditionMH>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        return false;
    }

    public static List<string> AllowedFields_15_16_17_18_19_20 { get; } = new List<string> 
    {
        "C"
    };
    


}

/// <summary>
/// RenditionMH_C Table 278
/// </summary>
internal partial class APM_RenditionMH_C : APM_RenditionMH_C__Base
{
}


internal partial class APM_RenditionMH_C__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RenditionMH_C";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_RenditionMH_C>(obj, "C", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_MediaCriteria, PdfDictionary>(stack, val, obj);
        
    }


}

