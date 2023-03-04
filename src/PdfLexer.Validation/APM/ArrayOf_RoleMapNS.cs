// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOf_RoleMapNS : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_RoleMapNS";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOf_RoleMapNS_0, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf_RoleMapNS_1, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false; // TODO
    }
}

/// <summary>
/// ArrayOf_RoleMapNS_0 Table 356
/// </summary>
internal partial class APM_ArrayOf_RoleMapNS_0 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_RoleMapNS_0";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_ArrayOf_RoleMapNS_0>(obj, 0, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOf_RoleMapNS_1 
/// </summary>
internal partial class APM_ArrayOf_RoleMapNS_1 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_RoleMapNS_1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_ArrayOf_RoleMapNS_1>(obj, 1, IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Namespace, PdfDictionary>(stack, val, obj);
        
    }
}

