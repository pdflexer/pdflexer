// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_RequirementsCollectionEditing : APM_RequirementsCollectionEditing_Base
{
}

internal partial class APM_RequirementsCollectionEditing_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "RequirementsCollectionEditing";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_RequirementsCollectionEditing_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RequirementsCollectionEditing_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RequirementsCollectionEditing_V, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RequirementsCollectionEditing_RH, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RequirementsCollectionEditing_Penalty, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_RequirementsCollectionEditing>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_RequirementsCollectionEditing_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "S", "V", "RH", "Penalty"
    };
    


}

/// <summary>
/// RequirementsCollectionEditing_Type Table 273 and Table 275
/// </summary>
internal partial class APM_RequirementsCollectionEditing_Type : APM_RequirementsCollectionEditing_Type_Base
{
}


internal partial class APM_RequirementsCollectionEditing_Type_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsCollectionEditing_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_RequirementsCollectionEditing_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "Requirement")) 
        {
            ctx.Fail<APM_RequirementsCollectionEditing_Type>($"Invalid value {val}, allowed are: [Requirement]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// RequirementsCollectionEditing_S 
/// </summary>
internal partial class APM_RequirementsCollectionEditing_S : APM_RequirementsCollectionEditing_S_Base
{
}


internal partial class APM_RequirementsCollectionEditing_S_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsCollectionEditing_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_RequirementsCollectionEditing_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "CollectionEditing")) 
        {
            ctx.Fail<APM_RequirementsCollectionEditing_S>($"Invalid value {val}, allowed are: [CollectionEditing]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// RequirementsCollectionEditing_V 
/// </summary>
internal partial class APM_RequirementsCollectionEditing_V : APM_RequirementsCollectionEditing_V_Base
{
}


internal partial class APM_RequirementsCollectionEditing_V_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsCollectionEditing_V";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_RequirementsCollectionEditing_V>(obj, "V", IndirectRequirement.Either);
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
                ctx.Fail<APM_RequirementsCollectionEditing_V>("V is required to one of 'dictionary;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// RequirementsCollectionEditing_RH 
/// </summary>
internal partial class APM_RequirementsCollectionEditing_RH : APM_RequirementsCollectionEditing_RH_Base
{
}


internal partial class APM_RequirementsCollectionEditing_RH_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsCollectionEditing_RH";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_RequirementsCollectionEditing_RH>(obj, "RH", IndirectRequirement.Either);
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
                ctx.Fail<APM_RequirementsCollectionEditing_RH>("RH is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// RequirementsCollectionEditing_Penalty 
/// </summary>
internal partial class APM_RequirementsCollectionEditing_Penalty : APM_RequirementsCollectionEditing_Penalty_Base
{
}


internal partial class APM_RequirementsCollectionEditing_Penalty_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsCollectionEditing_Penalty";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_RequirementsCollectionEditing_Penalty>(obj, "Penalty", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        IPdfObject @Penalty = val;
        if (!((gte(@Penalty,0)&&lte(@Penalty,100)))) 
        {
            ctx.Fail<APM_RequirementsCollectionEditing_Penalty>($"Invalid value {val}, allowed are: [fn:Eval((@Penalty>=0) && (@Penalty<=100))]");
        }
        }
        // no linked objects
        
    }


}

