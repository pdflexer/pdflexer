// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_BoxStyle : APM_BoxStyle__Base
{
}

internal partial class APM_BoxStyle__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "BoxStyle";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_BoxStyle_C, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_BoxStyle_W, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_BoxStyle_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_BoxStyle_D, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.4m:
            case 1.5m:
            case 1.6m:
            case 1.7m:
            case 1.8m:
            case 1.9m:
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14_15_16_17_18_19_20.Contains(x)))
                {
                    ctx.Fail<APM_BoxStyle>($"Unknown field {extra} for version {ctx.Version}");
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

    public static List<string> AllowedFields_14_15_16_17_18_19_20 { get; } = new List<string> 
    {
        "C", "D", "S", "W"
    };
    


}

/// <summary>
/// BoxStyle_C Table 397
/// </summary>
internal partial class APM_BoxStyle_C : APM_BoxStyle_C__Base
{
}


internal partial class APM_BoxStyle_C__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "BoxStyle_C";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_BoxStyle_C>(obj, "C", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_3RGBNumbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// BoxStyle_W line width
/// </summary>
internal partial class APM_BoxStyle_W : APM_BoxStyle_W__Base
{
}


internal partial class APM_BoxStyle_W__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "BoxStyle_W";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfNumber, APM_BoxStyle_W>(obj, "W", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// BoxStyle_S style (solid or dashed)
/// </summary>
internal partial class APM_BoxStyle_S : APM_BoxStyle_S__Base
{
}


internal partial class APM_BoxStyle_S__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "BoxStyle_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_BoxStyle_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.S || val == PdfName.D)) 
        {
            ctx.Fail<APM_BoxStyle_S>($"Invalid value {val}, allowed are: [S,D]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// BoxStyle_D dash pattern
/// </summary>
internal partial class APM_BoxStyle_D : APM_BoxStyle_D__Base
{
}


internal partial class APM_BoxStyle_D__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "BoxStyle_D";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_BoxStyle_D>(obj, "D", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfDashPatterns, PdfArray>(stack, val, obj);
        
    }


}

