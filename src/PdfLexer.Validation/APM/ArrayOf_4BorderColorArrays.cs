// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOf_4BorderColorArrays : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_4BorderColorArrays";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOf_4BorderColorArrays_0, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf_4BorderColorArrays_1, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf_4BorderColorArrays_2, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf_4BorderColorArrays_3, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOf_4BorderColorArrays_0 
/// </summary>
internal partial class APM_ArrayOf_4BorderColorArrays_0 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_4BorderColorArrays_0";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ArrayOf_4BorderColorArrays_0>(obj, 0, IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ArrayOf_4BorderColorArrays_0>("0 is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_ArrayOf_3RGBNumbers, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NullObj:
                {
                    var val =  (PdfNull)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_ArrayOf_4BorderColorArrays_0>("0 is required to one of 'array;null', was " + utval.Type);
                return;
        }
    }
}

/// <summary>
/// ArrayOf_4BorderColorArrays_1 
/// </summary>
internal partial class APM_ArrayOf_4BorderColorArrays_1 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_4BorderColorArrays_1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ArrayOf_4BorderColorArrays_1>(obj, 1, IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ArrayOf_4BorderColorArrays_1>("1 is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_ArrayOf_3RGBNumbers, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NullObj:
                {
                    var val =  (PdfNull)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_ArrayOf_4BorderColorArrays_1>("1 is required to one of 'array;null', was " + utval.Type);
                return;
        }
    }
}

/// <summary>
/// ArrayOf_4BorderColorArrays_2 
/// </summary>
internal partial class APM_ArrayOf_4BorderColorArrays_2 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_4BorderColorArrays_2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ArrayOf_4BorderColorArrays_2>(obj, 2, IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ArrayOf_4BorderColorArrays_2>("2 is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_ArrayOf_3RGBNumbers, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NullObj:
                {
                    var val =  (PdfNull)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_ArrayOf_4BorderColorArrays_2>("2 is required to one of 'array;null', was " + utval.Type);
                return;
        }
    }
}

/// <summary>
/// ArrayOf_4BorderColorArrays_3 
/// </summary>
internal partial class APM_ArrayOf_4BorderColorArrays_3 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_4BorderColorArrays_3";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ArrayOf_4BorderColorArrays_3>(obj, 3, IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ArrayOf_4BorderColorArrays_3>("3 is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_ArrayOf_3RGBNumbers, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NullObj:
                {
                    var val =  (PdfNull)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_ArrayOf_4BorderColorArrays_3>("3 is required to one of 'array;null', was " + utval.Type);
                return;
        }
    }
}
