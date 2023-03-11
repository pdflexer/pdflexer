// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOf3DTransMatrix : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf3DTransMatrix";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOf3DTransMatrix_0, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf3DTransMatrix_1, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf3DTransMatrix_2, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf3DTransMatrix_3, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf3DTransMatrix_4, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf3DTransMatrix_5, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf3DTransMatrix_6, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf3DTransMatrix_7, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf3DTransMatrix_8, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf3DTransMatrix_9, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf3DTransMatrix_10, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf3DTransMatrix_11, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOf3DTransMatrix_0 Table 269
/// </summary>
internal partial class APM_ArrayOf3DTransMatrix_0 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf3DTransMatrix_0";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfNumber, APM_ArrayOf3DTransMatrix_0>(obj, 0, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOf3DTransMatrix_1 
/// </summary>
internal partial class APM_ArrayOf3DTransMatrix_1 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf3DTransMatrix_1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfNumber, APM_ArrayOf3DTransMatrix_1>(obj, 1, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOf3DTransMatrix_2 
/// </summary>
internal partial class APM_ArrayOf3DTransMatrix_2 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf3DTransMatrix_2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfNumber, APM_ArrayOf3DTransMatrix_2>(obj, 2, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOf3DTransMatrix_3 
/// </summary>
internal partial class APM_ArrayOf3DTransMatrix_3 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf3DTransMatrix_3";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfNumber, APM_ArrayOf3DTransMatrix_3>(obj, 3, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOf3DTransMatrix_4 
/// </summary>
internal partial class APM_ArrayOf3DTransMatrix_4 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf3DTransMatrix_4";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfNumber, APM_ArrayOf3DTransMatrix_4>(obj, 4, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOf3DTransMatrix_5 
/// </summary>
internal partial class APM_ArrayOf3DTransMatrix_5 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf3DTransMatrix_5";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfNumber, APM_ArrayOf3DTransMatrix_5>(obj, 5, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOf3DTransMatrix_6 
/// </summary>
internal partial class APM_ArrayOf3DTransMatrix_6 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf3DTransMatrix_6";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfNumber, APM_ArrayOf3DTransMatrix_6>(obj, 6, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOf3DTransMatrix_7 
/// </summary>
internal partial class APM_ArrayOf3DTransMatrix_7 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf3DTransMatrix_7";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfNumber, APM_ArrayOf3DTransMatrix_7>(obj, 7, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOf3DTransMatrix_8 
/// </summary>
internal partial class APM_ArrayOf3DTransMatrix_8 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf3DTransMatrix_8";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfNumber, APM_ArrayOf3DTransMatrix_8>(obj, 8, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOf3DTransMatrix_9 
/// </summary>
internal partial class APM_ArrayOf3DTransMatrix_9 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf3DTransMatrix_9";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfNumber, APM_ArrayOf3DTransMatrix_9>(obj, 9, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOf3DTransMatrix_10 
/// </summary>
internal partial class APM_ArrayOf3DTransMatrix_10 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf3DTransMatrix_10";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfNumber, APM_ArrayOf3DTransMatrix_10>(obj, 10, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOf3DTransMatrix_11 
/// </summary>
internal partial class APM_ArrayOf3DTransMatrix_11 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf3DTransMatrix_11";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfNumber, APM_ArrayOf3DTransMatrix_11>(obj, 11, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }
}

