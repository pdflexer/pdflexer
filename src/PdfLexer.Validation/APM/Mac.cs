// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_Mac : APM_Mac__Base
{
}

internal partial class APM_Mac__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "Mac";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_Mac_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Mac_Creator, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Mac_ResFork, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_Mac>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_Mac>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_Mac>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_Mac>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_Mac>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_Mac>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_Mac>($"Unknown field {extra} for version 1.9");
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

    public static List<string> AllowedFields_13 { get; } = new List<string> 
    {
        "Subtype", "Creator", "ResFork"
    };
    public static List<string> AllowedFields_14 { get; } = new List<string> 
    {
        "Subtype", "Creator", "ResFork"
    };
    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Subtype", "Creator", "ResFork"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Subtype", "Creator", "ResFork"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Subtype", "Creator", "ResFork"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Subtype", "Creator", "ResFork"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Subtype", "Creator", "ResFork"
    };
    


}

/// <summary>
/// Mac_Subtype Table 47 in ISO 32000-1:2008
/// </summary>
internal partial class APM_Mac_Subtype : APM_Mac_Subtype__Base
{
}


internal partial class APM_Mac_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Mac_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_Mac_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Mac_Creator 
/// </summary>
internal partial class APM_Mac_Creator : APM_Mac_Creator__Base
{
}


internal partial class APM_Mac_Creator__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Mac_Creator";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_Mac_Creator>(obj, "Creator", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Mac_ResFork 
/// </summary>
internal partial class APM_Mac_ResFork : APM_Mac_ResFork__Base
{
}


internal partial class APM_Mac_ResFork__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Mac_ResFork";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_Mac_ResFork>(obj, "ResFork", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

