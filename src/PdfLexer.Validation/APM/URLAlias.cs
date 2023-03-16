// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_URLAlias : APM_URLAlias__Base
{
}

internal partial class APM_URLAlias__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "URLAlias";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_URLAlias_U, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_URLAlias_C, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.3m:
            case 1.4m:
            case 1.5m:
            case 1.6m:
            case 1.7m:
            case 1.8m:
            case 1.9m:
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13_14_15_16_17_18_19_20.Contains(x)))
                {
                    ctx.Fail<APM_URLAlias>($"Unknown field {extra} for version {ctx.Version}");
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

    public static List<string> AllowedFields_13_14_15_16_17_18_19_20 { get; } = new List<string> 
    {
        "C", "U"
    };
    


}

/// <summary>
/// URLAlias_U Table 392
/// </summary>
internal partial class APM_URLAlias_U : APM_URLAlias_U__Base
{
}


internal partial class APM_URLAlias_U__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "URLAlias_U";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfString, APM_URLAlias_U>(obj, "U", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// URLAlias_C 
/// </summary>
internal partial class APM_URLAlias_C : APM_URLAlias_C__Base
{
}


internal partial class APM_URLAlias_C__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "URLAlias_C";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_URLAlias_C>(obj, "C", IndirectRequirement.Either);
        if (val == null) { return; }
        var C = obj.Get("C");
        if (!(gte(((C as PdfArray)?.Count),1))) 
        {
            ctx.Fail<APM_URLAlias_C>($"Value failed special case check: fn:Eval(fn:ArrayLength(C)>=1)");
        }
        // no value restrictions
        ctx.Run<APM_ArrayOfArraysURLStrings, PdfArray>(stack, val, obj);
        
    }


}

