// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOf_3Integers : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_3Integers";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOf_3Integers_0, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf_3Integers_1, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf_3Integers_2, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOf_3Integers_0 number of pages altered
/// </summary>
internal partial class APM_ArrayOf_3Integers_0 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_3Integers_0";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_ArrayOf_3Integers_0>(obj, 0, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        IPdfObject v = val;
        
        if (!(gte(v,0))) 
        {
            ctx.Fail<APM_ArrayOf_3Integers_0>($"Invalid value {val}, allowed are: [fn:Eval(@0>=0)]");
        }
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOf_3Integers_1 number of fields altered
/// </summary>
internal partial class APM_ArrayOf_3Integers_1 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_3Integers_1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_ArrayOf_3Integers_1>(obj, 1, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        IPdfObject v = val;
        
        if (!(gte(v,0))) 
        {
            ctx.Fail<APM_ArrayOf_3Integers_1>($"Invalid value {val}, allowed are: [fn:Eval(@1>=0)]");
        }
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOf_3Integers_2 number of fields filled in
/// </summary>
internal partial class APM_ArrayOf_3Integers_2 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_3Integers_2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_ArrayOf_3Integers_2>(obj, 2, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        IPdfObject v = val;
        
        if (!(gte(v,0))) 
        {
            ctx.Fail<APM_ArrayOf_3Integers_2>($"Invalid value {val}, allowed are: [fn:Eval(@2>=0)]");
        }
        // no linked objects
        
    }
}
