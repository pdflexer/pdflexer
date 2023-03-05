// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_DestXYZStructArray : ISpecification<PdfArray>
{
    public static string Name { get; } = "DestXYZStructArray";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_DestXYZStructArray_0, PdfArray>(stack, obj, parent);
        ctx.Run<APM_DestXYZStructArray_1, PdfArray>(stack, obj, parent);
        ctx.Run<APM_DestXYZStructArray_2, PdfArray>(stack, obj, parent);
        ctx.Run<APM_DestXYZStructArray_3, PdfArray>(stack, obj, parent);
        ctx.Run<APM_DestXYZStructArray_4, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// DestXYZStructArray_0 Table 149
/// </summary>
internal partial class APM_DestXYZStructArray_0 : ISpecification<PdfArray>
{
    public static string Name { get; } = "DestXYZStructArray_0";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_DestXYZStructArray_0>(obj, 0, IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_DestXYZStructArray_0>("0 is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_StructElem, PdfDictionary>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // TODO special case: fn:InKeyMap(trailer::Catalog::Dests)
                    // no value restrictions
                    // no linked objects
                    return;
                }
            case PdfObjectType.StringObj:
                {
                    var val =  (PdfString)utval;
                    // no indirect obj reqs
                    // TODO special case: fn:InNameTree(trailer::Catalog::Names::Dests)
                    // no value restrictions
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_DestXYZStructArray_0>("0 is required to one of 'dictionary;name;string-byte', was " + utval.Type);
                return;
        }
    }
}

/// <summary>
/// DestXYZStructArray_1 
/// </summary>
internal partial class APM_DestXYZStructArray_1 : ISpecification<PdfArray>
{
    public static string Name { get; } = "DestXYZStructArray_1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_DestXYZStructArray_1>(obj, 1, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "XYZ")) 
        {
            ctx.Fail<APM_DestXYZStructArray_1>($"Invalid value {val}, allowed are: [XYZ]");
        }
        // no linked objects
        
    }
}

/// <summary>
/// DestXYZStructArray_2 left
/// </summary>
internal partial class APM_DestXYZStructArray_2 : ISpecification<PdfArray>
{
    public static string Name { get; } = "DestXYZStructArray_2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_DestXYZStructArray_2>(obj, 2, IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_DestXYZStructArray_2>("2 is required"); return; }
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
                    // no value restrictions
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_DestXYZStructArray_2>("2 is required to one of 'null;number', was " + utval.Type);
                return;
        }
    }
}

/// <summary>
/// DestXYZStructArray_3 top
/// </summary>
internal partial class APM_DestXYZStructArray_3 : ISpecification<PdfArray>
{
    public static string Name { get; } = "DestXYZStructArray_3";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_DestXYZStructArray_3>(obj, 3, IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_DestXYZStructArray_3>("3 is required"); return; }
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
                    // no value restrictions
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_DestXYZStructArray_3>("3 is required to one of 'null;number', was " + utval.Type);
                return;
        }
    }
}

/// <summary>
/// DestXYZStructArray_4 zoom
/// </summary>
internal partial class APM_DestXYZStructArray_4 : ISpecification<PdfArray>
{
    public static string Name { get; } = "DestXYZStructArray_4";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_DestXYZStructArray_4>(obj, 4, IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_DestXYZStructArray_4>("4 is required"); return; }
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
                    // no value restrictions
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_DestXYZStructArray_4>("4 is required to one of 'null;number', was " + utval.Type);
                return;
        }
    }
}

