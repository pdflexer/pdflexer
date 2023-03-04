// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_CalRGBColorSpace : ISpecification<PdfArray>
{
    public static string Name { get; } = "CalRGBColorSpace";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_CalRGBColorSpace_0, PdfArray>(stack, obj, parent);
        ctx.Run<APM_CalRGBColorSpace_1, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false; // TODO
    }
}

/// <summary>
/// CalRGBColorSpace_0 Clause 8.6.5.3
/// </summary>
internal partial class APM_CalRGBColorSpace_0 : ISpecification<PdfArray>
{
    public static string Name { get; } = "CalRGBColorSpace_0";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_CalRGBColorSpace_0>(obj, 0, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "CalRGB")) 
        {
            ctx.Fail<APM_CalRGBColorSpace_0>($"Invalid value {val}, allowed are: [CalRGB]");
        }
        }
        // no linked objects
        
    }
}

/// <summary>
/// CalRGBColorSpace_1 
/// </summary>
internal partial class APM_CalRGBColorSpace_1 : ISpecification<PdfArray>
{
    public static string Name { get; } = "CalRGBColorSpace_1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_CalRGBColorSpace_1>(obj, 1, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_CalRGBDict, PdfDictionary>(stack, val, obj);
        
    }
}

