// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOf_4NumbersColorAnnotation : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_4NumbersColorAnnotation";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOf_4NumbersColorAnnotation_0, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf_4NumbersColorAnnotation_1, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf_4NumbersColorAnnotation_2, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf_4NumbersColorAnnotation_3, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOf_4NumbersColorAnnotation_0 Table 166, C cell
/// </summary>
internal partial class APM_ArrayOf_4NumbersColorAnnotation_0 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_4NumbersColorAnnotation_0";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfNumber, APM_ArrayOf_4NumbersColorAnnotation_0>(obj, 0, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        IPdfObject v = val;
        
        if (!((gte(v,0)&&lte(v,1)))) 
        {
            ctx.Fail<APM_ArrayOf_4NumbersColorAnnotation_0>($"Invalid value {val}, allowed are: [fn:Eval((@0>=0) && (@0<=1))]");
        }
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOf_4NumbersColorAnnotation_1 Array must be 0,1,3,4 long
/// </summary>
internal partial class APM_ArrayOf_4NumbersColorAnnotation_1 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_4NumbersColorAnnotation_1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfNumber, APM_ArrayOf_4NumbersColorAnnotation_1>(obj, 1, IndirectRequirement.Either);
        if (val == null) { return; }
        
        if (!((obj.Count > 1))) 
        {
            ctx.Fail<APM_ArrayOf_4NumbersColorAnnotation_1>($"Value failed special case check: fn:Eval(fn:IsPresent(2))");
        }
        IPdfObject v = val;
        
        if (!((gte(v,0)&&lte(v,1)))) 
        {
            ctx.Fail<APM_ArrayOf_4NumbersColorAnnotation_1>($"Invalid value {val}, allowed are: [fn:Eval((@1>=0) && (@1<=1))]");
        }
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOf_4NumbersColorAnnotation_2 
/// </summary>
internal partial class APM_ArrayOf_4NumbersColorAnnotation_2 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_4NumbersColorAnnotation_2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfNumber, APM_ArrayOf_4NumbersColorAnnotation_2>(obj, 2, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        IPdfObject v = val;
        
        if (!((gte(v,0)&&lte(v,1)))) 
        {
            ctx.Fail<APM_ArrayOf_4NumbersColorAnnotation_2>($"Invalid value {val}, allowed are: [fn:Eval((@2>=0) && (@2<=1))]");
        }
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOf_4NumbersColorAnnotation_3 
/// </summary>
internal partial class APM_ArrayOf_4NumbersColorAnnotation_3 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_4NumbersColorAnnotation_3";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfNumber, APM_ArrayOf_4NumbersColorAnnotation_3>(obj, 3, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        IPdfObject v = val;
        
        if (!((gte(v,0)&&lte(v,1)))) 
        {
            ctx.Fail<APM_ArrayOf_4NumbersColorAnnotation_3>($"Invalid value {val}, allowed are: [fn:Eval((@3>=0) && (@3<=1))]");
        }
        // no linked objects
        
    }
}

