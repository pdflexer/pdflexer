// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_ActionGoTo3DView : APM_ActionGoTo3DView__Base
{
}

internal partial class APM_ActionGoTo3DView__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "ActionGoTo3DView";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_ActionGoTo3DView_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionGoTo3DView_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionGoTo3DView_Next, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionGoTo3DView_TA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionGoTo3DView_V, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.6m:
            case 1.7m:
            case 1.8m:
            case 1.9m:
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16_17_18_19_20.Contains(x)))
                {
                    ctx.Fail<APM_ActionGoTo3DView>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_ActionGoTo3DView_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_16_17_18_19_20 { get; } = new List<string> 
    {
        "Next", "S", "TA", "Type", "V"
    };
    


}

/// <summary>
/// ActionGoTo3DView_Type Table 196 and Table 220
/// </summary>
internal partial class APM_ActionGoTo3DView_Type : APM_ActionGoTo3DView_Type__Base
{
}


internal partial class APM_ActionGoTo3DView_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionGoTo3DView_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_ActionGoTo3DView_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Action)) 
        {
            ctx.Fail<APM_ActionGoTo3DView_Type>($"Invalid value {val}, allowed are: [Action]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ActionGoTo3DView_S 
/// </summary>
internal partial class APM_ActionGoTo3DView_S : APM_ActionGoTo3DView_S__Base
{
}


internal partial class APM_ActionGoTo3DView_S__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionGoTo3DView_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfName, APM_ActionGoTo3DView_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.GoTo3DView)) 
        {
            ctx.Fail<APM_ActionGoTo3DView_S>($"Invalid value {val}, allowed are: [GoTo3DView]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ActionGoTo3DView_Next 
/// </summary>
internal partial class APM_ActionGoTo3DView_Next : APM_ActionGoTo3DView_Next__Base
{
}


internal partial class APM_ActionGoTo3DView_Next__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionGoTo3DView_Next";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ActionGoTo3DView_Next>(obj, "Next", IndirectRequirement.Either);
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
                    } else if (APM_ActionGoToE.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ActionGoToE, PdfDictionary>(stack, val, obj);
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
                    } else if (APM_ActionGoTo3DView.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ActionGoTo3DView, PdfDictionary>(stack, val, obj);
                    } else if (APM_ActionECMAScript.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ActionECMAScript, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version >= 2.0m && APM_ActionGoToDp.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_ActionGoToDp, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version >= 2.0m && APM_ActionRichMediaExecute.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_ActionRichMediaExecute, PdfDictionary>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_ActionGoTo3DView_Next>("Next did not match any allowable types: '[ActionGoTo,ActionGoToR,ActionGoToE,fn:SinceVersion(2.0,ActionGoToDp),ActionLaunch,ActionThread,ActionURI,ActionSound,ActionMovie,ActionHide,ActionNamed,ActionSubmitForm,ActionResetForm,ActionImportData,ActionSetOCGState,ActionRendition,ActionTransition,ActionGoTo3DView,ActionECMAScript,fn:SinceVersion(2.0,ActionRichMediaExecute)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ActionGoTo3DView_Next>("Next is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ActionGoTo3DView_TA 
/// </summary>
internal partial class APM_ActionGoTo3DView_TA : APM_ActionGoTo3DView_TA__Base
{
}


internal partial class APM_ActionGoTo3DView_TA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionGoTo3DView_TA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfDictionary, APM_ActionGoTo3DView_TA>(obj, "TA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_Annot3D.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_Annot3D, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 2.0m && APM_AnnotRichMedia.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_AnnotRichMedia, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_ActionGoTo3DView_TA>("TA did not match any allowable types: '[Annot3D,fn:SinceVersion(2.0,AnnotRichMedia)]'");
        }
        
    }


}

/// <summary>
/// ActionGoTo3DView_V 
/// </summary>
internal partial class APM_ActionGoTo3DView_V : APM_ActionGoTo3DView_V__Base
{
}


internal partial class APM_ActionGoTo3DView_V__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionGoTo3DView_V";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ActionGoTo3DView_V>(obj, "V", IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ActionGoTo3DView_V>("V is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_3DView, PdfDictionary>(stack, val, obj);
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
                ctx.Fail<APM_ActionGoTo3DView_V>("V is required to one of 'dictionary;integer;name;string-text', was " + utval.Type);
                return;
        }
    }


}

