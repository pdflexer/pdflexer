// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOf_4BorderThicknessNumbers : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_4BorderThicknessNumbers";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOf_4BorderThicknessNumbers_0, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf_4BorderThicknessNumbers_1, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf_4BorderThicknessNumbers_2, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf_4BorderThicknessNumbers_3, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false; // TODO
    }
}

/// <summary>
/// ArrayOf_4BorderThicknessNumbers_0 Table 378 - before
/// </summary>
internal partial class APM_ArrayOf_4BorderThicknessNumbers_0 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_4BorderThicknessNumbers_0";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ArrayOf_4BorderThicknessNumbers_0>(obj, 0, IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ArrayOf_4BorderThicknessNumbers_0>("0 is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.NullObj:
                {
                    var val =  (PdfNull)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
                    return;
                }
            case PdfObjectType.NumericObj:
                {
                    var val =  (PdfNumber)utval;
                    // no indirect obj reqs
                    // no special cases
                    {
                    IPdfObject v = val;
                    
                    if (!(gte(v,0))) 
                    {
                        ctx.Fail<APM_ArrayOf_4BorderThicknessNumbers_0>($"Invalid value {val}, allowed are: [fn:Eval(@0>=0)]");
                    }
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_ArrayOf_4BorderThicknessNumbers_0>("0 is required to one of 'null;number', was " + utval.Type);
                return;
        }
    }
}

/// <summary>
/// ArrayOf_4BorderThicknessNumbers_1 after
/// </summary>
internal partial class APM_ArrayOf_4BorderThicknessNumbers_1 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_4BorderThicknessNumbers_1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ArrayOf_4BorderThicknessNumbers_1>(obj, 1, IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ArrayOf_4BorderThicknessNumbers_1>("1 is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.NullObj:
                {
                    var val =  (PdfNull)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
                    return;
                }
            case PdfObjectType.NumericObj:
                {
                    var val =  (PdfNumber)utval;
                    // no indirect obj reqs
                    // no special cases
                    {
                    IPdfObject v = val;
                    
                    if (!(gte(v,0))) 
                    {
                        ctx.Fail<APM_ArrayOf_4BorderThicknessNumbers_1>($"Invalid value {val}, allowed are: [fn:Eval(@1>=0)]");
                    }
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_ArrayOf_4BorderThicknessNumbers_1>("1 is required to one of 'null;number', was " + utval.Type);
                return;
        }
    }
}

/// <summary>
/// ArrayOf_4BorderThicknessNumbers_2 start
/// </summary>
internal partial class APM_ArrayOf_4BorderThicknessNumbers_2 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_4BorderThicknessNumbers_2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ArrayOf_4BorderThicknessNumbers_2>(obj, 2, IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ArrayOf_4BorderThicknessNumbers_2>("2 is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.NullObj:
                {
                    var val =  (PdfNull)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
                    return;
                }
            case PdfObjectType.NumericObj:
                {
                    var val =  (PdfNumber)utval;
                    // no indirect obj reqs
                    // no special cases
                    {
                    IPdfObject v = val;
                    
                    if (!(gte(v,0))) 
                    {
                        ctx.Fail<APM_ArrayOf_4BorderThicknessNumbers_2>($"Invalid value {val}, allowed are: [fn:Eval(@2>=0)]");
                    }
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_ArrayOf_4BorderThicknessNumbers_2>("2 is required to one of 'null;number', was " + utval.Type);
                return;
        }
    }
}

/// <summary>
/// ArrayOf_4BorderThicknessNumbers_3 end
/// </summary>
internal partial class APM_ArrayOf_4BorderThicknessNumbers_3 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_4BorderThicknessNumbers_3";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ArrayOf_4BorderThicknessNumbers_3>(obj, 3, IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ArrayOf_4BorderThicknessNumbers_3>("3 is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.NullObj:
                {
                    var val =  (PdfNull)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
                    return;
                }
            case PdfObjectType.NumericObj:
                {
                    var val =  (PdfNumber)utval;
                    // no indirect obj reqs
                    // no special cases
                    {
                    IPdfObject v = val;
                    
                    if (!(gte(v,0))) 
                    {
                        ctx.Fail<APM_ArrayOf_4BorderThicknessNumbers_3>($"Invalid value {val}, allowed are: [fn:Eval(@3>=0)]");
                    }
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_ArrayOf_4BorderThicknessNumbers_3>("3 is required to one of 'null;number', was " + utval.Type);
                return;
        }
    }
}

