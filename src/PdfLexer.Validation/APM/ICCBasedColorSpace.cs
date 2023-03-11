// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ICCBasedColorSpace : ISpecification<PdfArray>
{
    public static string Name { get; } = "ICCBasedColorSpace";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ICCBasedColorSpace_0, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ICCBasedColorSpace_1, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ICCBasedColorSpace_0 Clause 8.6.5.5
/// </summary>
internal partial class APM_ICCBasedColorSpace_0 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ICCBasedColorSpace_0";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfName, APM_ICCBasedColorSpace_0>(obj, 0, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.ICCBased)) 
        {
            ctx.Fail<APM_ICCBasedColorSpace_0>($"Invalid value {val}, allowed are: [ICCBased]");
        }
        // no linked objects
        
    }
}

/// <summary>
/// ICCBasedColorSpace_1 
/// </summary>
internal partial class APM_ICCBasedColorSpace_1 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ICCBasedColorSpace_1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfStream, APM_ICCBasedColorSpace_1>(obj, 1, IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ICCProfileStream, PdfDictionary>(stack, val.Dictionary, obj);
        
    }
}

