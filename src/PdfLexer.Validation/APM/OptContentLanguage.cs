// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_OptContentLanguage : APM_OptContentLanguage__Base
{
}

internal partial class APM_OptContentLanguage__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "OptContentLanguage";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_OptContentLanguage_Lang, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OptContentLanguage_Preferred, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_OptContentLanguage>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_OptContentLanguage>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_OptContentLanguage>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_OptContentLanguage>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_OptContentLanguage>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_OptContentLanguage>($"Unknown field {extra} for version 2.0");
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

    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Lang", "Preferred"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Lang", "Preferred"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Lang", "Preferred"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Lang", "Preferred"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Lang", "Preferred"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Lang", "Preferred"
    };
    


}

/// <summary>
/// OptContentLanguage_Lang Table 100, Language cell
/// </summary>
internal partial class APM_OptContentLanguage_Lang : APM_OptContentLanguage_Lang__Base
{
}


internal partial class APM_OptContentLanguage_Lang__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OptContentLanguage_Lang";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfString, APM_OptContentLanguage_Lang>(obj, "Lang", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// OptContentLanguage_Preferred 
/// </summary>
internal partial class APM_OptContentLanguage_Preferred : APM_OptContentLanguage_Preferred__Base
{
}


internal partial class APM_OptContentLanguage_Preferred__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OptContentLanguage_Preferred";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_OptContentLanguage_Preferred>(obj, "Preferred", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "ON" || val == "OFF")) 
        {
            ctx.Fail<APM_OptContentLanguage_Preferred>($"Invalid value {val}, allowed are: [ON,OFF]");
        }
        // no linked objects
        
    }


}

