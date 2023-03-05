// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_CryptFilterPublicKey : APM_CryptFilterPublicKey__Base
{
}

internal partial class APM_CryptFilterPublicKey__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "CryptFilterPublicKey";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_CryptFilterPublicKey_Recipients, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CryptFilterPublicKey_EncryptMetadata, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CryptFilterPublicKey_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CryptFilterPublicKey_CFM, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CryptFilterPublicKey_AuthEvent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CryptFilterPublicKey_Length, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_CryptFilterPublicKey>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_CryptFilterPublicKey>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_CryptFilterPublicKey>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_CryptFilterPublicKey>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_CryptFilterPublicKey>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_CryptFilterPublicKey>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_CryptFilterPublicKey_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Recipients", "EncryptMetadata", "Type", "CFM", "AuthEvent", "Length"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Recipients", "EncryptMetadata", "Type", "CFM", "AuthEvent", "Length"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Recipients", "EncryptMetadata", "Type", "CFM", "AuthEvent", "Length"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Recipients", "EncryptMetadata", "Type", "CFM", "AuthEvent", "Length"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Recipients", "EncryptMetadata", "Type", "CFM", "AuthEvent", "Length"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Recipients", "EncryptMetadata", "Type", "CFM", "AuthEvent"
    };
    


}

/// <summary>
/// CryptFilterPublicKey_Recipients Table 25 and Table 27, https://github.com/pdf-association/pdf-issues/issues/16
/// </summary>
internal partial class APM_CryptFilterPublicKey_Recipients : APM_CryptFilterPublicKey_Recipients__Base
{
}


internal partial class APM_CryptFilterPublicKey_Recipients__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CryptFilterPublicKey_Recipients";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_CryptFilterPublicKey_Recipients>(obj, "Recipients", IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_CryptFilterPublicKey_Recipients>("Recipients is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_ArrayOfStringsByte, PdfArray>(stack, val, obj);
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
                ctx.Fail<APM_CryptFilterPublicKey_Recipients>("Recipients is required to one of 'array;string-byte', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// CryptFilterPublicKey_EncryptMetadata 
/// </summary>
internal partial class APM_CryptFilterPublicKey_EncryptMetadata : APM_CryptFilterPublicKey_EncryptMetadata__Base
{
}


internal partial class APM_CryptFilterPublicKey_EncryptMetadata__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CryptFilterPublicKey_EncryptMetadata";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_CryptFilterPublicKey_EncryptMetadata>(obj, "EncryptMetadata", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// CryptFilterPublicKey_Type 
/// </summary>
internal partial class APM_CryptFilterPublicKey_Type : APM_CryptFilterPublicKey_Type__Base
{
}


internal partial class APM_CryptFilterPublicKey_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CryptFilterPublicKey_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_CryptFilterPublicKey_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "CryptFilter")) 
        {
            ctx.Fail<APM_CryptFilterPublicKey_Type>($"Invalid value {val}, allowed are: [CryptFilter]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// CryptFilterPublicKey_CFM 
/// </summary>
internal partial class APM_CryptFilterPublicKey_CFM : APM_CryptFilterPublicKey_CFM__Base
{
}


internal partial class APM_CryptFilterPublicKey_CFM__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CryptFilterPublicKey_CFM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_CryptFilterPublicKey_CFM>(obj, "CFM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "None" || (ctx.Version <= 2.0m && val == "V2") || (ctx.Version <= 2.0m && (ctx.Version < 1.6m || (ctx.Version >= 1.6m && val == "AESV2"))) || (ctx.Version <= 2.0m && (ctx.Version == 1.7m && (ctx.Extensions.Contains("ADBE_Extn3") && val == "AESV3"))) || (ctx.Version < 2.0m || (ctx.Version >= 2.0m && val == "AESV3")) || (ctx.Version < 2.0m || (ctx.Version >= 2.0m && (ctx.Extensions.Contains("ISO_TS_32003") && val == "AESV4"))))) 
        {
            ctx.Fail<APM_CryptFilterPublicKey_CFM>($"Invalid value {val}, allowed are: [None,fn:Deprecated(2.0,V2),fn:Deprecated(2.0,fn:SinceVersion(1.6,AESV2)),fn:Deprecated(2.0,fn:IsPDFVersion(1.7,fn:Extension(ADBE_Extn3,AESV3))),fn:SinceVersion(2.0,AESV3),fn:SinceVersion(2.0,fn:Extension(ISO_TS_32003,AESV4))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// CryptFilterPublicKey_AuthEvent 
/// </summary>
internal partial class APM_CryptFilterPublicKey_AuthEvent : APM_CryptFilterPublicKey_AuthEvent__Base
{
}


internal partial class APM_CryptFilterPublicKey_AuthEvent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CryptFilterPublicKey_AuthEvent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_CryptFilterPublicKey_AuthEvent>(obj, "AuthEvent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "DocOpen" || val == "EFOpen")) 
        {
            ctx.Fail<APM_CryptFilterPublicKey_AuthEvent>($"Invalid value {val}, allowed are: [DocOpen,EFOpen]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// CryptFilterPublicKey_Length Expressed in BITS. https://github.com/pdf-association/pdf-issues/issues/184
/// </summary>
internal partial class APM_CryptFilterPublicKey_Length : APM_CryptFilterPublicKey_Length__Base
{
}


internal partial class APM_CryptFilterPublicKey_Length__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CryptFilterPublicKey_Length";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_CryptFilterPublicKey_Length>(obj, "Length", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var CFM = obj.Get("CFM");
        var Length = obj.Get("Length");
        // TODO required value checks
        if (!(gte(Length,40)&&lte(Length,128)&&eq(mod(Length,8),0))) 
        {
            ctx.Fail<APM_CryptFilterPublicKey_Length>($"Invalid value {val}, allowed are: [fn:RequiredValue(@CFM==AESV2,128),fn:RequiredValue(@CFM==AESV3,256),fn:Eval((@Length>=40) && (@Length<=128) && ((@Length mod 8)==0))]");
        }
        // no linked objects
        
    }


}

