// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOf4Functions : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf4Functions";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOf4Functions_0, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf4Functions_1, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf4Functions_2, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf4Functions_3, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOf4Functions_0 
/// </summary>
internal partial class APM_ArrayOf4Functions_0 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf4Functions_0";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ArrayOf4Functions_0>(obj, 0, IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ArrayOf4Functions_0>("0 is required"); return; }
        switch (utval.Type) 
        {
            // TODO funcs: fn:SinceVersion(1.3,dictionary)
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_ArrayOf4Functions_0>("0 is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    if (APM_FunctionType0.MatchesType(ctx, val.Dictionary)) 
                    {
                        ctx.Run<APM_FunctionType0, PdfDictionary>(stack, val.Dictionary, obj);
                    } else if ((ctx.Version < 1.3m || (ctx.Version >= 1.3m && APM_FunctionType4.MatchesType(ctx, val.Dictionary)))) 
                    {
                        ctx.Run<APM_FunctionType4, PdfDictionary>(stack, val.Dictionary, obj);
                    }else 
                    {
                        ctx.Fail<APM_ArrayOf4Functions_0>("0 did not match any allowable types: '[FunctionType0,fn:SinceVersion(1.3,FunctionType4)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ArrayOf4Functions_0>("0 is required to one of 'fn:SinceVersion(1.3,dictionary);stream', was " + utval.Type);
                return;
        }
    }
}

/// <summary>
/// ArrayOf4Functions_1 
/// </summary>
internal partial class APM_ArrayOf4Functions_1 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf4Functions_1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ArrayOf4Functions_1>(obj, 1, IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ArrayOf4Functions_1>("1 is required"); return; }
        switch (utval.Type) 
        {
            // TODO funcs: fn:SinceVersion(1.3,dictionary)
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_ArrayOf4Functions_1>("1 is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    if (APM_FunctionType0.MatchesType(ctx, val.Dictionary)) 
                    {
                        ctx.Run<APM_FunctionType0, PdfDictionary>(stack, val.Dictionary, obj);
                    } else if ((ctx.Version < 1.3m || (ctx.Version >= 1.3m && APM_FunctionType4.MatchesType(ctx, val.Dictionary)))) 
                    {
                        ctx.Run<APM_FunctionType4, PdfDictionary>(stack, val.Dictionary, obj);
                    }else 
                    {
                        ctx.Fail<APM_ArrayOf4Functions_1>("1 did not match any allowable types: '[FunctionType0,fn:SinceVersion(1.3,FunctionType4)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ArrayOf4Functions_1>("1 is required to one of 'fn:SinceVersion(1.3,dictionary);stream', was " + utval.Type);
                return;
        }
    }
}

/// <summary>
/// ArrayOf4Functions_2 
/// </summary>
internal partial class APM_ArrayOf4Functions_2 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf4Functions_2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ArrayOf4Functions_2>(obj, 2, IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ArrayOf4Functions_2>("2 is required"); return; }
        switch (utval.Type) 
        {
            // TODO funcs: fn:SinceVersion(1.3,dictionary)
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_ArrayOf4Functions_2>("2 is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    if (APM_FunctionType0.MatchesType(ctx, val.Dictionary)) 
                    {
                        ctx.Run<APM_FunctionType0, PdfDictionary>(stack, val.Dictionary, obj);
                    } else if ((ctx.Version < 1.3m || (ctx.Version >= 1.3m && APM_FunctionType4.MatchesType(ctx, val.Dictionary)))) 
                    {
                        ctx.Run<APM_FunctionType4, PdfDictionary>(stack, val.Dictionary, obj);
                    }else 
                    {
                        ctx.Fail<APM_ArrayOf4Functions_2>("2 did not match any allowable types: '[FunctionType0,fn:SinceVersion(1.3,FunctionType4)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ArrayOf4Functions_2>("2 is required to one of 'fn:SinceVersion(1.3,dictionary);stream', was " + utval.Type);
                return;
        }
    }
}

/// <summary>
/// ArrayOf4Functions_3 
/// </summary>
internal partial class APM_ArrayOf4Functions_3 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf4Functions_3";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ArrayOf4Functions_3>(obj, 3, IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ArrayOf4Functions_3>("3 is required"); return; }
        switch (utval.Type) 
        {
            // TODO funcs: fn:SinceVersion(1.3,dictionary)
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_ArrayOf4Functions_3>("3 is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    if (APM_FunctionType0.MatchesType(ctx, val.Dictionary)) 
                    {
                        ctx.Run<APM_FunctionType0, PdfDictionary>(stack, val.Dictionary, obj);
                    } else if ((ctx.Version < 1.3m || (ctx.Version >= 1.3m && APM_FunctionType4.MatchesType(ctx, val.Dictionary)))) 
                    {
                        ctx.Run<APM_FunctionType4, PdfDictionary>(stack, val.Dictionary, obj);
                    }else 
                    {
                        ctx.Fail<APM_ArrayOf4Functions_3>("3 did not match any allowable types: '[FunctionType0,fn:SinceVersion(1.3,FunctionType4)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ArrayOf4Functions_3>("3 is required to one of 'fn:SinceVersion(1.3,dictionary);stream', was " + utval.Type);
                return;
        }
    }
}

