// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOfXRefWIntegers : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfXRefWIntegers";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOfXRefWIntegers_0, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOfXRefWIntegers_1, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOfXRefWIntegers_2, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOfXRefWIntegers_0 Table 17, W cell
/// </summary>
internal partial class APM_ArrayOfXRefWIntegers_0 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfXRefWIntegers_0";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_ArrayOfXRefWIntegers_0>(obj, 0, IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        
        // no special cases
        IPdfObject v = val;
        
        if (!(gte(v,0))) 
        {
            ctx.Fail<APM_ArrayOfXRefWIntegers_0>($"Invalid value {val}, allowed are: [fn:Eval(@0>=0)]");
        }
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOfXRefWIntegers_1 
/// </summary>
internal partial class APM_ArrayOfXRefWIntegers_1 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfXRefWIntegers_1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_ArrayOfXRefWIntegers_1>(obj, 1, IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        
        // no special cases
        IPdfObject v = val;
        
        if (!(gt(v,0))) 
        {
            ctx.Fail<APM_ArrayOfXRefWIntegers_1>($"Invalid value {val}, allowed are: [fn:Eval(@1>0)]");
        }
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOfXRefWIntegers_2 
/// </summary>
internal partial class APM_ArrayOfXRefWIntegers_2 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfXRefWIntegers_2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_ArrayOfXRefWIntegers_2>(obj, 2, IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        
        // no special cases
        IPdfObject v = val;
        
        if (!(gte(v,0))) 
        {
            ctx.Fail<APM_ArrayOfXRefWIntegers_2>($"Invalid value {val}, allowed are: [fn:Eval(@2>=0)]");
        }
        // no linked objects
        
    }
}

