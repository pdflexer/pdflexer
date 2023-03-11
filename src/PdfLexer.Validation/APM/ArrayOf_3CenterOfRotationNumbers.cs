// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOf_3CenterOfRotationNumbers : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_3CenterOfRotationNumbers";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOf_3CenterOfRotationNumbers_0, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf_3CenterOfRotationNumbers_1, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf_3CenterOfRotationNumbers_2, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOf_3CenterOfRotationNumbers_0 
/// </summary>
internal partial class APM_ArrayOf_3CenterOfRotationNumbers_0 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_3CenterOfRotationNumbers_0";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfNumber, APM_ArrayOf_3CenterOfRotationNumbers_0>(obj, 0, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOf_3CenterOfRotationNumbers_1 
/// </summary>
internal partial class APM_ArrayOf_3CenterOfRotationNumbers_1 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_3CenterOfRotationNumbers_1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfNumber, APM_ArrayOf_3CenterOfRotationNumbers_1>(obj, 1, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOf_3CenterOfRotationNumbers_2 
/// </summary>
internal partial class APM_ArrayOf_3CenterOfRotationNumbers_2 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_3CenterOfRotationNumbers_2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfNumber, APM_ArrayOf_3CenterOfRotationNumbers_2>(obj, 2, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }
}

