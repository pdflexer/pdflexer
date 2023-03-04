// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_ActionSetOCGState : APM_ActionSetOCGState_Base
{
}

internal partial class APM_ActionSetOCGState_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "ActionSetOCGState";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_ActionSetOCGState_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionSetOCGState_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionSetOCGState_Next, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionSetOCGState_State, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionSetOCGState_PreserveRB, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_ActionSetOCGState>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_ActionSetOCGState>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_ActionSetOCGState>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_ActionSetOCGState>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_ActionSetOCGState>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_ActionSetOCGState>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_ActionSetOCGState_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Type", "S", "Next", "State", "PreserveRB"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Type", "S", "Next", "State", "PreserveRB"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "S", "Next", "State", "PreserveRB"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "S", "Next", "State", "PreserveRB"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "S", "Next", "State", "PreserveRB"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "S", "Next", "State", "PreserveRB"
    };
    


}

/// <summary>
/// ActionSetOCGState_Type Table 196 and Table 217
/// </summary>
internal partial class APM_ActionSetOCGState_Type : APM_ActionSetOCGState_Type_Base
{
}


internal partial class APM_ActionSetOCGState_Type_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSetOCGState_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_ActionSetOCGState_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "Action")) 
        {
            ctx.Fail<APM_ActionSetOCGState_Type>($"Invalid value {val}, allowed are: [Action]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// ActionSetOCGState_S 
/// </summary>
internal partial class APM_ActionSetOCGState_S : APM_ActionSetOCGState_S_Base
{
}


internal partial class APM_ActionSetOCGState_S_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSetOCGState_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_ActionSetOCGState_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "SetOCGState")) 
        {
            ctx.Fail<APM_ActionSetOCGState_S>($"Invalid value {val}, allowed are: [SetOCGState]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// ActionSetOCGState_Next 
/// </summary>
internal partial class APM_ActionSetOCGState_Next : APM_ActionSetOCGState_Next_Base
{
}


internal partial class APM_ActionSetOCGState_Next_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSetOCGState_Next";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ActionSetOCGState_Next>(obj, "Next", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_ArrayOfActions, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    if (APM_ActionGoTo.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ActionGoTo, PdfDictionary>(stack, val, obj);
                    } else if (APM_ActionGoToR.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ActionGoToR, PdfDictionary>(stack, val, obj);
                    } else if (APM_ActionLaunch.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ActionLaunch, PdfDictionary>(stack, val, obj);
                    } else if (APM_ActionThread.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ActionThread, PdfDictionary>(stack, val, obj);
                    } else if (APM_ActionURI.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ActionURI, PdfDictionary>(stack, val, obj);
                    } else if (APM_ActionSound.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ActionSound, PdfDictionary>(stack, val, obj);
                    } else if (APM_ActionMovie.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ActionMovie, PdfDictionary>(stack, val, obj);
                    } else if (APM_ActionHide.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ActionHide, PdfDictionary>(stack, val, obj);
                    } else if (APM_ActionNamed.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ActionNamed, PdfDictionary>(stack, val, obj);
                    } else if (APM_ActionSubmitForm.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ActionSubmitForm, PdfDictionary>(stack, val, obj);
                    } else if (APM_ActionResetForm.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ActionResetForm, PdfDictionary>(stack, val, obj);
                    } else if (APM_ActionImportData.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ActionImportData, PdfDictionary>(stack, val, obj);
                    } else if (APM_ActionSetOCGState.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ActionSetOCGState, PdfDictionary>(stack, val, obj);
                    } else if (APM_ActionRendition.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ActionRendition, PdfDictionary>(stack, val, obj);
                    } else if (APM_ActionTransition.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ActionTransition, PdfDictionary>(stack, val, obj);
                    } else if (APM_ActionECMAScript.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ActionECMAScript, PdfDictionary>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_ActionSetOCGState_Next>("Next did not match any allowable types: '[ActionGoTo,ActionGoToR,fn:SinceVersion(1.6,ActionGoToE),fn:SinceVersion(2.0,ActionGoToDp),ActionLaunch,ActionThread,ActionURI,ActionSound,ActionMovie,ActionHide,ActionNamed,ActionSubmitForm,ActionResetForm,ActionImportData,ActionSetOCGState,ActionRendition,ActionTransition,fn:SinceVersion(1.6,ActionGoTo3DView),ActionECMAScript,fn:SinceVersion(2.0,ActionRichMediaExecute)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ActionSetOCGState_Next>("Next is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ActionSetOCGState_State 
/// </summary>
internal partial class APM_ActionSetOCGState_State : APM_ActionSetOCGState_State_Base
{
}


internal partial class APM_ActionSetOCGState_State_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSetOCGState_State";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_ActionSetOCGState_State>(obj, "State", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfOCGState, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// ActionSetOCGState_PreserveRB 
/// </summary>
internal partial class APM_ActionSetOCGState_PreserveRB : APM_ActionSetOCGState_PreserveRB_Base
{
}


internal partial class APM_ActionSetOCGState_PreserveRB_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSetOCGState_PreserveRB";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_ActionSetOCGState_PreserveRB>(obj, "PreserveRB", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

