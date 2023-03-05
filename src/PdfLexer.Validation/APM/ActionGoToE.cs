// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_ActionGoToE : APM_ActionGoToE__Base
{
}

internal partial class APM_ActionGoToE__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "ActionGoToE";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_ActionGoToE_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionGoToE_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionGoToE_Next, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionGoToE_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionGoToE_D, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionGoToE_NewWindow, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionGoToE_T, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_ActionGoToE>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_ActionGoToE>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_ActionGoToE>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_ActionGoToE>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_ActionGoToE>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_ActionGoToE_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "F", "D", "NewWindow", "T"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "F", "D", "NewWindow", "T"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "F", "D", "NewWindow", "T"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "F", "D", "NewWindow", "T"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "F", "D", "NewWindow", "T"
    };
    


}

/// <summary>
/// ActionGoToE_Type Table 196 and Table 204
/// </summary>
internal partial class APM_ActionGoToE_Type : APM_ActionGoToE_Type__Base
{
}


internal partial class APM_ActionGoToE_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionGoToE_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_ActionGoToE_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "Action")) 
        {
            ctx.Fail<APM_ActionGoToE_Type>($"Invalid value {val}, allowed are: [Action]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ActionGoToE_S 
/// </summary>
internal partial class APM_ActionGoToE_S : APM_ActionGoToE_S__Base
{
}


internal partial class APM_ActionGoToE_S__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionGoToE_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_ActionGoToE_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "GoToE")) 
        {
            ctx.Fail<APM_ActionGoToE_S>($"Invalid value {val}, allowed are: [GoToE]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ActionGoToE_Next 
/// </summary>
internal partial class APM_ActionGoToE_Next : APM_ActionGoToE_Next__Base
{
}


internal partial class APM_ActionGoToE_Next__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionGoToE_Next";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ActionGoToE_Next>(obj, "Next", IndirectRequirement.Either);
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
                        ctx.Fail<APM_ActionGoToE_Next>("Next did not match any allowable types: '[ActionGoTo,ActionGoToR,ActionGoToE,fn:SinceVersion(2.0,ActionGoToDp),ActionLaunch,ActionThread,ActionURI,ActionSound,ActionMovie,ActionHide,ActionNamed,ActionSubmitForm,ActionResetForm,ActionImportData,ActionSetOCGState,ActionRendition,ActionTransition,ActionGoTo3DView,ActionECMAScript,fn:SinceVersion(2.0,ActionRichMediaExecute)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ActionGoToE_Next>("Next is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ActionGoToE_F 
/// </summary>
internal partial class APM_ActionGoToE_F : APM_ActionGoToE_F__Base
{
}


internal partial class APM_ActionGoToE_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionGoToE_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ActionGoToE_F>(obj, "F", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_FileSpecification, PdfDictionary>(stack, val, obj);
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
                ctx.Fail<APM_ActionGoToE_F>("F is required to one of 'dictionary;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ActionGoToE_D 
/// </summary>
internal partial class APM_ActionGoToE_D : APM_ActionGoToE_D__Base
{
}


internal partial class APM_ActionGoToE_D__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionGoToE_D";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ActionGoToE_D>(obj, "D", IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ActionGoToE_D>("D is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    if (APM_DestXYZArray.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_DestXYZArray, PdfArray>(stack, val, obj);
                    } else if (APM_Dest0Array.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_Dest0Array, PdfArray>(stack, val, obj);
                    } else if (APM_Dest1Array.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_Dest1Array, PdfArray>(stack, val, obj);
                    } else if (APM_Dest4Array.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_Dest4Array, PdfArray>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_ActionGoToE_D>("D did not match any allowable types: '[DestXYZArray,Dest0Array,Dest1Array,Dest4Array]'");
                    }
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
                ctx.Fail<APM_ActionGoToE_D>("D is required to one of 'array;name;string-byte', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ActionGoToE_NewWindow 
/// </summary>
internal partial class APM_ActionGoToE_NewWindow : APM_ActionGoToE_NewWindow__Base
{
}


internal partial class APM_ActionGoToE_NewWindow__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionGoToE_NewWindow";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_ActionGoToE_NewWindow>(obj, "NewWindow", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ActionGoToE_T 
/// </summary>
internal partial class APM_ActionGoToE_T : APM_ActionGoToE_T__Base
{
}


internal partial class APM_ActionGoToE_T__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionGoToE_T";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfDictionary, APM_ActionGoToE_T>(obj, "T", IndirectRequirement.Either);
        if ((!obj.ContainsKey("F")) && val == null) {
            ctx.Fail<APM_ActionGoToE_T>("T is required when 'fn:IsRequired(fn:Not(fn:IsPresent(F)))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Target, PdfDictionary>(stack, val, obj);
        
    }


}

