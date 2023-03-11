// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_UserProperty : APM_UserProperty__Base
{
}

internal partial class APM_UserProperty__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "UserProperty";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_UserProperty_N, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_UserProperty_V, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_UserProperty_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_UserProperty_H, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_UserProperty>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_UserProperty>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_UserProperty>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_UserProperty>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_UserProperty>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        return false;
    }

    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "N", "V", "F", "H"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "N", "V", "F", "H"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "N", "V", "F", "H"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "N", "V", "F", "H"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "N", "V", "F", "H"
    };
    


}

/// <summary>
/// UserProperty_N Table 362
/// </summary>
internal partial class APM_UserProperty_N : APM_UserProperty_N__Base
{
}


internal partial class APM_UserProperty_N__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "UserProperty_N";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfString, APM_UserProperty_N>(obj, "N", IndirectRequirement.Either);
        if (val == null) { return; }
        var trailerCatalogMarkInfoUserProperties = ctx.Trailer.Get("Catalog")?.Get("MarkInfo")?.Get("UserProperties");
        if (!(eq(trailerCatalogMarkInfoUserProperties,PdfBoolean.True))) 
        {
            ctx.Fail<APM_UserProperty_N>($"Value failed special case check: fn:Eval(trailer::Catalog::MarkInfo::@UserProperties==true)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// UserProperty_V any PDF object
/// </summary>
internal partial class APM_UserProperty_V : APM_UserProperty_V__Base
{
}


internal partial class APM_UserProperty_V__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "UserProperty_V";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_UserProperty_V>(obj, "V", IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_UserProperty_V>("V is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM__UniversalArray, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.BooleanObj:
                {
                    var val =  (PdfBoolean)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
                    return;
                }
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM__UniversalDictionary, PdfDictionary>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
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
            case PdfObjectType.NumericObj:
                {
                    var val =  (PdfNumber)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
                    return;
                }
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_UserProperty_V>("V is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
                    return;
                }
            case PdfObjectType.StringObj:
                {
                    var val =  (PdfString)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_UserProperty_V>("V is required to one of 'array;boolean;dictionary;name;null;number;stream;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// UserProperty_F 
/// </summary>
internal partial class APM_UserProperty_F : APM_UserProperty_F__Base
{
}


internal partial class APM_UserProperty_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "UserProperty_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_UserProperty_F>(obj, "F", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// UserProperty_H 
/// </summary>
internal partial class APM_UserProperty_H : APM_UserProperty_H__Base
{
}


internal partial class APM_UserProperty_H__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "UserProperty_H";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfBoolean, APM_UserProperty_H>(obj, "H", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

