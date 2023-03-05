// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_RequirementsDigSig : APM_RequirementsDigSig__Base
{
}

internal partial class APM_RequirementsDigSig__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "RequirementsDigSig";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_RequirementsDigSig_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RequirementsDigSig_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RequirementsDigSig_V, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RequirementsDigSig_RH, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RequirementsDigSig_Penalty, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RequirementsDigSig_DigSig, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_RequirementsDigSig>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_RequirementsDigSig_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "S", "V", "RH", "Penalty", "DigSig"
    };
    


}

/// <summary>
/// RequirementsDigSig_Type Table 273 and Table 274 and Table 275
/// </summary>
internal partial class APM_RequirementsDigSig_Type : APM_RequirementsDigSig_Type__Base
{
}


internal partial class APM_RequirementsDigSig_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsDigSig_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_RequirementsDigSig_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "Requirement")) 
        {
            ctx.Fail<APM_RequirementsDigSig_Type>($"Invalid value {val}, allowed are: [Requirement]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// RequirementsDigSig_S 
/// </summary>
internal partial class APM_RequirementsDigSig_S : APM_RequirementsDigSig_S__Base
{
}


internal partial class APM_RequirementsDigSig_S__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsDigSig_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_RequirementsDigSig_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "DigSig")) 
        {
            ctx.Fail<APM_RequirementsDigSig_S>($"Invalid value {val}, allowed are: [DigSig]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// RequirementsDigSig_V 
/// </summary>
internal partial class APM_RequirementsDigSig_V : APM_RequirementsDigSig_V__Base
{
}


internal partial class APM_RequirementsDigSig_V__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsDigSig_V";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_RequirementsDigSig_V>(obj, "V", IndirectRequirement.Either);
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
                ctx.Fail<APM_RequirementsDigSig_V>("V is required to one of 'dictionary;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// RequirementsDigSig_RH 
/// </summary>
internal partial class APM_RequirementsDigSig_RH : APM_RequirementsDigSig_RH__Base
{
}


internal partial class APM_RequirementsDigSig_RH__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsDigSig_RH";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_RequirementsDigSig_RH>(obj, "RH", IndirectRequirement.Either);
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
                ctx.Fail<APM_RequirementsDigSig_RH>("RH is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// RequirementsDigSig_Penalty 
/// </summary>
internal partial class APM_RequirementsDigSig_Penalty : APM_RequirementsDigSig_Penalty__Base
{
}


internal partial class APM_RequirementsDigSig_Penalty__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsDigSig_Penalty";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_RequirementsDigSig_Penalty>(obj, "Penalty", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var Penalty = obj.Get("Penalty");
        if (!((gte(Penalty,0)&&lte(Penalty,100)))) 
        {
            ctx.Fail<APM_RequirementsDigSig_Penalty>($"Invalid value {val}, allowed are: [fn:Eval((@Penalty>=0) && (@Penalty<=100))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// RequirementsDigSig_DigSig 
/// </summary>
internal partial class APM_RequirementsDigSig_DigSig : APM_RequirementsDigSig_DigSig__Base
{
}


internal partial class APM_RequirementsDigSig_DigSig__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsDigSig_DigSig";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_RequirementsDigSig_DigSig>(obj, "DigSig", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_Signature.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_Signature, PdfDictionary>(stack, val, obj);
        } else if (APM_DocTimeStamp.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_DocTimeStamp, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_RequirementsDigSig_DigSig>("DigSig did not match any allowable types: '[Signature,DocTimeStamp]'");
        }
        
    }


}

