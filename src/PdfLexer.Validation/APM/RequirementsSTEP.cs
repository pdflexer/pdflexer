// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_RequirementsSTEP : APM_RequirementsSTEP_Base
{
}

internal partial class APM_RequirementsSTEP_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "RequirementsSTEP";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_RequirementsSTEP_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RequirementsSTEP_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RequirementsSTEP_V, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RequirementsSTEP_RH, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RequirementsSTEP_Penalty, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
        
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_RequirementsSTEP_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    


}

/// <summary>
/// RequirementsSTEP_Type Table 273 and ISO/TS 24064
/// </summary>
internal partial class APM_RequirementsSTEP_Type : APM_RequirementsSTEP_Type_Base
{
}


internal partial class APM_RequirementsSTEP_Type_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsSTEP_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_RequirementsSTEP_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "Requirement")) 
        {
            ctx.Fail<APM_RequirementsSTEP_Type>($"Invalid value {val}, allowed are: [Requirement]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// RequirementsSTEP_S 
/// </summary>
internal partial class APM_RequirementsSTEP_S : APM_RequirementsSTEP_S_Base
{
}


internal partial class APM_RequirementsSTEP_S_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsSTEP_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_RequirementsSTEP_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "STEP")) 
        {
            ctx.Fail<APM_RequirementsSTEP_S>($"Invalid value {val}, allowed are: [STEP]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// RequirementsSTEP_V 
/// </summary>
internal partial class APM_RequirementsSTEP_V : APM_RequirementsSTEP_V_Base
{
}


internal partial class APM_RequirementsSTEP_V_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsSTEP_V";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_RequirementsSTEP_V>(obj, "V", IndirectRequirement.Either);
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
                ctx.Fail<APM_RequirementsSTEP_V>("V is required to one of 'dictionary;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// RequirementsSTEP_RH 
/// </summary>
internal partial class APM_RequirementsSTEP_RH : APM_RequirementsSTEP_RH_Base
{
}


internal partial class APM_RequirementsSTEP_RH_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsSTEP_RH";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_RequirementsSTEP_RH>(obj, "RH", IndirectRequirement.Either);
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
                ctx.Fail<APM_RequirementsSTEP_RH>("RH is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// RequirementsSTEP_Penalty 
/// </summary>
internal partial class APM_RequirementsSTEP_Penalty : APM_RequirementsSTEP_Penalty_Base
{
}


internal partial class APM_RequirementsSTEP_Penalty_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RequirementsSTEP_Penalty";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_RequirementsSTEP_Penalty>(obj, "Penalty", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        IPdfObject @Penalty = val;
        if (!((gte(@Penalty,0)&&lte(@Penalty,100)))) 
        {
            ctx.Fail<APM_RequirementsSTEP_Penalty>($"Invalid value {val}, allowed are: [fn:Eval((@Penalty>=0) && (@Penalty<=100))]");
        }
        }
        // no linked objects
        
    }


}

