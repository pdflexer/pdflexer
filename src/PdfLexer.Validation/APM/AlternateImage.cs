// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_AlternateImage : APM_AlternateImage__Base
{
}

internal partial class APM_AlternateImage__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "AlternateImage";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_AlternateImage_Image, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AlternateImage_DefaultForPrinting, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AlternateImage_OC, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_AlternateImage>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_AlternateImage>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_AlternateImage>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_AlternateImage>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_AlternateImage>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_AlternateImage>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_AlternateImage>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_AlternateImage>($"Unknown field {extra} for version 2.0");
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
        "Image", "DefaultForPrinting"
    };
    public static List<string> AllowedFields_14 { get; } = new List<string> 
    {
        "Image", "DefaultForPrinting"
    };
    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Image", "DefaultForPrinting", "OC"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Image", "DefaultForPrinting", "OC"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Image", "DefaultForPrinting", "OC"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Image", "DefaultForPrinting", "OC"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Image", "DefaultForPrinting", "OC"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Image", "DefaultForPrinting", "OC"
    };
    


}

/// <summary>
/// AlternateImage_Image Table 89
/// </summary>
internal partial class APM_AlternateImage_Image : APM_AlternateImage_Image__Base
{
}


internal partial class APM_AlternateImage_Image__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AlternateImage_Image";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfStream, APM_AlternateImage_Image>(obj, "Image", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_XObjectImage, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// AlternateImage_DefaultForPrinting 
/// </summary>
internal partial class APM_AlternateImage_DefaultForPrinting : APM_AlternateImage_DefaultForPrinting__Base
{
}


internal partial class APM_AlternateImage_DefaultForPrinting__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AlternateImage_DefaultForPrinting";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_AlternateImage_DefaultForPrinting>(obj, "DefaultForPrinting", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AlternateImage_OC 
/// </summary>
internal partial class APM_AlternateImage_OC : APM_AlternateImage_OC__Base
{
}


internal partial class APM_AlternateImage_OC__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AlternateImage_OC";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_AlternateImage_OC>(obj, "OC", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_OptContentGroup.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_OptContentGroup, PdfDictionary>(stack, val, obj);
        } else if (APM_OptContentMembership.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_OptContentMembership, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_AlternateImage_OC>("OC did not match any allowable types: '[OptContentGroup,OptContentMembership]'");
        }
        
    }


}
