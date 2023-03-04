// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_LabRangeArray : ISpecification<PdfArray>
{
    public static string Name { get; } = "LabRangeArray";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_LabRangeArray_0, PdfArray>(stack, obj, parent);
        ctx.Run<APM_LabRangeArray_1, PdfArray>(stack, obj, parent);
        ctx.Run<APM_LabRangeArray_2, PdfArray>(stack, obj, parent);
        ctx.Run<APM_LabRangeArray_3, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false; // TODO
    }
}

/// <summary>
/// LabRangeArray_0 Table 64, Range cell
/// </summary>
internal partial class APM_LabRangeArray_0 : ISpecification<PdfArray>
{
    public static string Name { get; } = "LabRangeArray_0";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_LabRangeArray_0>(obj, 0, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        IPdfObject v = val;
        
        if (!(lte(v,obj.Get(1)))) 
        {
            ctx.Fail<APM_LabRangeArray_0>($"Invalid value {val}, allowed are: [fn:Eval(@0<=@1)]");
        }
        }
        // no linked objects
        
    }
}

/// <summary>
/// LabRangeArray_1 
/// </summary>
internal partial class APM_LabRangeArray_1 : ISpecification<PdfArray>
{
    public static string Name { get; } = "LabRangeArray_1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_LabRangeArray_1>(obj, 1, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        IPdfObject v = val;
        
        if (!(gte(v,obj.Get(0)))) 
        {
            ctx.Fail<APM_LabRangeArray_1>($"Invalid value {val}, allowed are: [fn:Eval(@1>=@0)]");
        }
        }
        // no linked objects
        
    }
}

/// <summary>
/// LabRangeArray_2 
/// </summary>
internal partial class APM_LabRangeArray_2 : ISpecification<PdfArray>
{
    public static string Name { get; } = "LabRangeArray_2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_LabRangeArray_2>(obj, 2, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        IPdfObject v = val;
        
        if (!(lte(v,obj.Get(3)))) 
        {
            ctx.Fail<APM_LabRangeArray_2>($"Invalid value {val}, allowed are: [fn:Eval(@2<=@3)]");
        }
        }
        // no linked objects
        
    }
}

/// <summary>
/// LabRangeArray_3 
/// </summary>
internal partial class APM_LabRangeArray_3 : ISpecification<PdfArray>
{
    public static string Name { get; } = "LabRangeArray_3";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_LabRangeArray_3>(obj, 3, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        IPdfObject v = val;
        
        if (!(gte(v,obj.Get(2)))) 
        {
            ctx.Fail<APM_LabRangeArray_3>($"Invalid value {val}, allowed are: [fn:Eval(@3>=@2)]");
        }
        }
        // no linked objects
        
    }
}

