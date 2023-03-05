// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_SignatureReferenceUR : APM_SignatureReferenceUR__Base
{
}

internal partial class APM_SignatureReferenceUR__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "SignatureReferenceUR";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_SignatureReferenceUR_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SignatureReferenceUR_TransformMethod, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SignatureReferenceUR_TransformParams, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SignatureReferenceUR_Data, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SignatureReferenceUR_DigestMethod, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_SignatureReferenceUR>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_SignatureReferenceUR>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_SignatureReferenceUR>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_SignatureReferenceUR>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_SignatureReferenceUR>($"Unknown field {extra} for version 1.9");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_SignatureReferenceUR_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Type", "TransformMethod", "TransformParams", "Data", "DigestMethod"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Type", "TransformMethod", "TransformParams", "Data", "DigestMethod"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "TransformMethod", "TransformParams", "Data", "DigestMethod"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "TransformMethod", "TransformParams", "Data", "DigestMethod"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "TransformMethod", "TransformParams", "Data", "DigestMethod"
    };
    


}

/// <summary>
/// SignatureReferenceUR_Type Table 256
/// </summary>
internal partial class APM_SignatureReferenceUR_Type : APM_SignatureReferenceUR_Type__Base
{
}


internal partial class APM_SignatureReferenceUR_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SignatureReferenceUR_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_SignatureReferenceUR_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "SigRef")) 
        {
            ctx.Fail<APM_SignatureReferenceUR_Type>($"Invalid value {val}, allowed are: [SigRef]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// SignatureReferenceUR_TransformMethod 
/// </summary>
internal partial class APM_SignatureReferenceUR_TransformMethod : APM_SignatureReferenceUR_TransformMethod__Base
{
}


internal partial class APM_SignatureReferenceUR_TransformMethod__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SignatureReferenceUR_TransformMethod";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_SignatureReferenceUR_TransformMethod>(obj, "TransformMethod", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "UR" || val == "UR3")) 
        {
            ctx.Fail<APM_SignatureReferenceUR_TransformMethod>($"Invalid value {val}, allowed are: [UR,UR3]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// SignatureReferenceUR_TransformParams 
/// </summary>
internal partial class APM_SignatureReferenceUR_TransformParams : APM_SignatureReferenceUR_TransformParams__Base
{
}


internal partial class APM_SignatureReferenceUR_TransformParams__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SignatureReferenceUR_TransformParams";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_SignatureReferenceUR_TransformParams>(obj, "TransformParams", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_URTransformParameters, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// SignatureReferenceUR_Data 
/// </summary>
internal partial class APM_SignatureReferenceUR_Data : APM_SignatureReferenceUR_Data__Base
{
}


internal partial class APM_SignatureReferenceUR_Data__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SignatureReferenceUR_Data";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_SignatureReferenceUR_Data>(obj, "Data", IndirectRequirement.Either);
        if (utval == null) { return; }
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
            case PdfObjectType.NumericObj:
                {
                    var val =  (PdfIntNumber)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
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
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_SignatureReferenceUR_Data>("Data is required to be indirect when a stream"); return; }
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
                ctx.Fail<APM_SignatureReferenceUR_Data>("Data is required to one of 'array;boolean;dictionary;integer;name;stream;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// SignatureReferenceUR_DigestMethod see https://github.com/pdf-association/pdf-issues/issues/117
/// </summary>
internal partial class APM_SignatureReferenceUR_DigestMethod : APM_SignatureReferenceUR_DigestMethod__Base
{
}


internal partial class APM_SignatureReferenceUR_DigestMethod__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SignatureReferenceUR_DigestMethod";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_SignatureReferenceUR_DigestMethod>(obj, "DigestMethod", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!((ctx.Version <= 2.0m && val == "MD5") || (ctx.Version <= 2.0m && val == "SHA1") || (ctx.Version < 2.0m || (ctx.Version >= 2.0m && val == "SHA256")) || (ctx.Version < 2.0m || (ctx.Version >= 2.0m && val == "SHA384")) || (ctx.Version < 2.0m || (ctx.Version >= 2.0m && val == "SHA512")) || (ctx.Version < 2.0m || (ctx.Version >= 2.0m && val == "RIPEMD160")) || (ctx.Extensions.Contains("ISO_TS_32001") && val == "SHA3-256") || (ctx.Extensions.Contains("ISO_TS_32001") && val == "SHA3-384") || (ctx.Extensions.Contains("ISO_TS_32001") && val == "SHA3-512") || (ctx.Extensions.Contains("ISO_TS_32001") && val == "SHAKE256"))) 
        {
            ctx.Fail<APM_SignatureReferenceUR_DigestMethod>($"Invalid value {val}, allowed are: [fn:Deprecated(2.0,MD5),fn:Deprecated(2.0,SHA1),fn:SinceVersion(2.0,SHA256),fn:SinceVersion(2.0,SHA384),fn:SinceVersion(2.0,SHA512),fn:SinceVersion(2.0,RIPEMD160),fn:Extension(ISO_TS_32001,SHA3-256),fn:Extension(ISO_TS_32001,SHA3-384),fn:Extension(ISO_TS_32001,SHA3-512),fn:Extension(ISO_TS_32001,SHAKE256)]");
        }
        // no linked objects
        
    }


}

