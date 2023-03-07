// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_RequirementsEncryption : APM_RequirementsEncryption__Base
{
}

internal partial class APM_RequirementsEncryption__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "RequirementsEncryption";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_RequirementsEncryption_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RequirementsEncryption_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RequirementsEncryption_V, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RequirementsEncryption_RH, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RequirementsEncryption_Penalty, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RequirementsEncryption_Encrypt, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_RequirementsEncryption>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_RequirementsEncryption_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "S", "V", "RH", "Penalty", "Encrypt"
    };
    


}

/// <summary>
/// RequirementsEncryption_Type Table 273 and Table 274 and Table 275
/// </summary>
internal partial class APM_RequirementsEncryption_Type : APM_RequirementsEncryption_Type__Base
{
}


internal partial class APM_RequirementsEncryption_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsEncryption_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_RequirementsEncryption_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Requirement)) 
        {
            ctx.Fail<APM_RequirementsEncryption_Type>($"Invalid value {val}, allowed are: [Requirement]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// RequirementsEncryption_S 
/// </summary>
internal partial class APM_RequirementsEncryption_S : APM_RequirementsEncryption_S__Base
{
}


internal partial class APM_RequirementsEncryption_S__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsEncryption_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_RequirementsEncryption_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Encryption)) 
        {
            ctx.Fail<APM_RequirementsEncryption_S>($"Invalid value {val}, allowed are: [Encryption]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// RequirementsEncryption_V 
/// </summary>
internal partial class APM_RequirementsEncryption_V : APM_RequirementsEncryption_V__Base
{
}


internal partial class APM_RequirementsEncryption_V__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsEncryption_V";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_RequirementsEncryption_V>(obj, "V", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_Extensions, PdfDictionary>(stack, val, obj);
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
            
            default:
                ctx.Fail<APM_RequirementsEncryption_V>("V is required to one of 'dictionary;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// RequirementsEncryption_RH 
/// </summary>
internal partial class APM_RequirementsEncryption_RH : APM_RequirementsEncryption_RH__Base
{
}


internal partial class APM_RequirementsEncryption_RH__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsEncryption_RH";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_RequirementsEncryption_RH>(obj, "RH", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_ArrayOfRequirementsHandlers, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_RequirementsHandler, PdfDictionary>(stack, val, obj);
                    return;
                }
            
            default:
                ctx.Fail<APM_RequirementsEncryption_RH>("RH is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// RequirementsEncryption_Penalty 
/// </summary>
internal partial class APM_RequirementsEncryption_Penalty : APM_RequirementsEncryption_Penalty__Base
{
}


internal partial class APM_RequirementsEncryption_Penalty__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsEncryption_Penalty";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_RequirementsEncryption_Penalty>(obj, "Penalty", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var Penalty = obj.Get("Penalty");
        if (!((gte(Penalty,0)&&lte(Penalty,100)))) 
        {
            ctx.Fail<APM_RequirementsEncryption_Penalty>($"Invalid value {val}, allowed are: [fn:Eval((@Penalty>=0) && (@Penalty<=100))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// RequirementsEncryption_Encrypt 
/// </summary>
internal partial class APM_RequirementsEncryption_Encrypt : APM_RequirementsEncryption_Encrypt__Base
{
}


internal partial class APM_RequirementsEncryption_Encrypt__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsEncryption_Encrypt";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_RequirementsEncryption_Encrypt>(obj, "Encrypt", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_EncryptionStandard.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_EncryptionStandard, PdfDictionary>(stack, val, obj);
        } else if (APM_EncryptionPublicKey.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_EncryptionPublicKey, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_RequirementsEncryption_Encrypt>("Encrypt did not match any allowable types: '[EncryptionStandard,EncryptionPublicKey]'");
        }
        
    }


}
