// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_OPIVersion20 : APM_OPIVersion20__Base
{
}

internal partial class APM_OPIVersion20__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "OPIVersion20";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_OPIVersion20_2_0, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_OPIVersion20>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_OPIVersion20>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_OPIVersion20>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_OPIVersion20>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_OPIVersion20>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_OPIVersion20>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_OPIVersion20>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_OPIVersion20>($"Unknown field {extra} for version 1.9");
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

    public static List<string> AllowedFields_12 { get; } = new List<string> 
    {
        "2.0"
    };
    public static List<string> AllowedFields_13 { get; } = new List<string> 
    {
        "2.0"
    };
    public static List<string> AllowedFields_14 { get; } = new List<string> 
    {
        "2.0"
    };
    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "2.0"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "2.0"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "2.0"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "2.0"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "2.0"
    };
    


}

/// <summary>
/// OPIVersion20_2.0 Table 405
/// </summary>
internal partial class APM_OPIVersion20_2_0 : APM_OPIVersion20_2_0__Base
{
}


internal partial class APM_OPIVersion20_2_0__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OPIVersion20_2.0";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_OPIVersion20_2_0>(obj, "2_0", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_OPIVersion20Dict, PdfDictionary>(stack, val, obj);
        
    }


}
