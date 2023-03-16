// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_MediaClipSection : APM_MediaClipSection__Base
{
}

internal partial class APM_MediaClipSection__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "MediaClipSection";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_MediaClipSection_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MediaClipSection_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MediaClipSection_N, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MediaClipSection_D, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MediaClipSection_Alt, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MediaClipSection_MH, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MediaClipSection_BE, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
            case 1.6m:
            case 1.7m:
            case 1.8m:
            case 1.9m:
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15_16_17_18_19_20.Contains(x)))
                {
                    ctx.Fail<APM_MediaClipSection>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_MediaClipSection_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_15_16_17_18_19_20 { get; } = new HashSet<string> 
    {
        "Alt", "BE", "D", "MH", "N", "S", "Type"
    };
    


}

/// <summary>
/// MediaClipSection_Type Table 284 and Table 288
/// </summary>
internal partial class APM_MediaClipSection_Type : APM_MediaClipSection_Type__Base
{
}


internal partial class APM_MediaClipSection_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MediaClipSection_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_MediaClipSection_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.MediaClip)) 
        {
            ctx.Fail<APM_MediaClipSection_Type>($"Invalid value {val}, allowed are: [MediaClip]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// MediaClipSection_S 
/// </summary>
internal partial class APM_MediaClipSection_S : APM_MediaClipSection_S__Base
{
}


internal partial class APM_MediaClipSection_S__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MediaClipSection_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfName, APM_MediaClipSection_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.MCS)) 
        {
            ctx.Fail<APM_MediaClipSection_S>($"Invalid value {val}, allowed are: [MCS]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// MediaClipSection_N 
/// </summary>
internal partial class APM_MediaClipSection_N : APM_MediaClipSection_N__Base
{
}


internal partial class APM_MediaClipSection_N__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MediaClipSection_N";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_MediaClipSection_N>(obj, "N", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// MediaClipSection_D 
/// </summary>
internal partial class APM_MediaClipSection_D : APM_MediaClipSection_D__Base
{
}


internal partial class APM_MediaClipSection_D__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MediaClipSection_D";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfDictionary, APM_MediaClipSection_D>(obj, "D", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_MediaClipSection.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_MediaClipSection, PdfDictionary>(stack, val, obj);
        } else if (APM_MediaClipData.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_MediaClipData, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_MediaClipSection_D>("D did not match any allowable types: '[MediaClipSection,MediaClipData]'");
        }
        
    }


}

/// <summary>
/// MediaClipSection_Alt 
/// </summary>
internal partial class APM_MediaClipSection_Alt : APM_MediaClipSection_Alt__Base
{
}


internal partial class APM_MediaClipSection_Alt__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MediaClipSection_Alt";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_MediaClipSection_Alt>(obj, "Alt", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfStringsText, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// MediaClipSection_MH 
/// </summary>
internal partial class APM_MediaClipSection_MH : APM_MediaClipSection_MH__Base
{
}


internal partial class APM_MediaClipSection_MH__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MediaClipSection_MH";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_MediaClipSection_MH>(obj, "MH", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_MediaClipSectionMHBE, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// MediaClipSection_BE 
/// </summary>
internal partial class APM_MediaClipSection_BE : APM_MediaClipSection_BE__Base
{
}


internal partial class APM_MediaClipSection_BE__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MediaClipSection_BE";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_MediaClipSection_BE>(obj, "BE", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_MediaClipSectionMHBE, PdfDictionary>(stack, val, obj);
        
    }


}

