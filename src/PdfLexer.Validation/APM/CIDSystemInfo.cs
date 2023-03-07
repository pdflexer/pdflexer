// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_CIDSystemInfo : APM_CIDSystemInfo__Base
{
}

internal partial class APM_CIDSystemInfo__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "CIDSystemInfo";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_CIDSystemInfo_Registry, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CIDSystemInfo_Ordering, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CIDSystemInfo_Supplement, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_CIDSystemInfo>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_CIDSystemInfo>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_CIDSystemInfo>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_CIDSystemInfo>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_CIDSystemInfo>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_CIDSystemInfo>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_CIDSystemInfo>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_CIDSystemInfo>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_CIDSystemInfo>($"Unknown field {extra} for version 2.0");
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
        "Registry", "Ordering", "Supplement"
    };
    public static List<string> AllowedFields_13 { get; } = new List<string> 
    {
        "Registry", "Ordering", "Supplement"
    };
    public static List<string> AllowedFields_14 { get; } = new List<string> 
    {
        "Registry", "Ordering", "Supplement"
    };
    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Registry", "Ordering", "Supplement"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Registry", "Ordering", "Supplement"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Registry", "Ordering", "Supplement"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Registry", "Ordering", "Supplement"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Registry", "Ordering", "Supplement"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Registry", "Ordering", "Supplement"
    };
    


}

/// <summary>
/// CIDSystemInfo_Registry Table 114
/// </summary>
internal partial class APM_CIDSystemInfo_Registry : APM_CIDSystemInfo_Registry__Base
{
}


internal partial class APM_CIDSystemInfo_Registry__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CIDSystemInfo_Registry";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfString, APM_CIDSystemInfo_Registry>(obj, "Registry", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// CIDSystemInfo_Ordering 
/// </summary>
internal partial class APM_CIDSystemInfo_Ordering : APM_CIDSystemInfo_Ordering__Base
{
}


internal partial class APM_CIDSystemInfo_Ordering__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CIDSystemInfo_Ordering";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfString, APM_CIDSystemInfo_Ordering>(obj, "Ordering", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// CIDSystemInfo_Supplement 
/// </summary>
internal partial class APM_CIDSystemInfo_Supplement : APM_CIDSystemInfo_Supplement__Base
{
}


internal partial class APM_CIDSystemInfo_Supplement__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CIDSystemInfo_Supplement";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_CIDSystemInfo_Supplement>(obj, "Supplement", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}
