// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_ActionSetState : APM_ActionSetState_Base
{
}

internal partial class APM_ActionSetState_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "ActionSetState";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_ActionSetState_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionSetState_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionSetState_T, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionSetState_AS, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
        
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_ActionSetState_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    


}

/// <summary>
/// ActionSetState_Type Table 196 and Adobe PDF 1.2
/// </summary>
internal partial class APM_ActionSetState_Type : APM_ActionSetState_Type_Base
{
}


internal partial class APM_ActionSetState_Type_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSetState_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_ActionSetState_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "Action")) 
        {
            ctx.Fail<APM_ActionSetState_Type>($"Invalid value {val}, allowed are: [Action]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// ActionSetState_S only documented in Adobe PDF 1.2
/// </summary>
internal partial class APM_ActionSetState_S : APM_ActionSetState_S_Base
{
}


internal partial class APM_ActionSetState_S_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSetState_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_ActionSetState_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "SetState")) 
        {
            ctx.Fail<APM_ActionSetState_S>($"Invalid value {val}, allowed are: [SetState]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// ActionSetState_T only documented in Adobe PDF 1.2 so only a few annots
/// </summary>
internal partial class APM_ActionSetState_T : APM_ActionSetState_T_Base
{
}


internal partial class APM_ActionSetState_T_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSetState_T";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ActionSetState_T>(obj, "T", IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ActionSetState_T>("T is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_ArrayOfAnnots, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    if (!wasIR) { ctx.Fail<APM_ActionSetState_T>("T is required to be indirect when a dictionary"); return; }
                    // no special cases
                    // no value restrictions
                    if (APM_AnnotText.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_AnnotText, PdfDictionary>(stack, val, obj);
                    } else if (APM_AnnotLink.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_AnnotLink, PdfDictionary>(stack, val, obj);
                    } else if (APM_AnnotSound.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_AnnotSound, PdfDictionary>(stack, val, obj);
                    } else if (APM_AnnotMovie.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_AnnotMovie, PdfDictionary>(stack, val, obj);
                    } else if (APM_AnnotWidget.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_AnnotWidget, PdfDictionary>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_ActionSetState_T>("T did not match any allowable types: '[AnnotText,AnnotLink,AnnotSound,AnnotMovie,AnnotWidget]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ActionSetState_T>("T is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ActionSetState_AS only documented in Adobe PDF 1.2
/// </summary>
internal partial class APM_ActionSetState_AS : APM_ActionSetState_AS_Base
{
}


internal partial class APM_ActionSetState_AS_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSetState_AS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_ActionSetState_AS>(obj, "AS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

