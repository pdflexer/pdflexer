// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_ActionLaunch : APM_ActionLaunch__Base
{
}

internal partial class APM_ActionLaunch__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "ActionLaunch";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_ActionLaunch_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionLaunch_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionLaunch_Next, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionLaunch_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionLaunch_Win, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionLaunch_Mac, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionLaunch_Unix, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionLaunch_NewWindow, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.1m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_11.Contains(x)))
                {
                    ctx.Fail<APM_ActionLaunch>($"Unknown field {extra} for version 1.1");
                }
                break;
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_ActionLaunch>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_ActionLaunch>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_ActionLaunch>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_ActionLaunch>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_ActionLaunch>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_ActionLaunch>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_ActionLaunch>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_ActionLaunch>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_ActionLaunch>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_ActionLaunch_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_11 { get; } = new HashSet<string> 
    {
        "Type", "S", "F", "Win", "Mac", "Unix"
    };
    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "F", "Win", "Mac", "Unix", "NewWindow"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "F", "Win", "Mac", "Unix", "NewWindow"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "F", "Win", "Mac", "Unix", "NewWindow"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "F", "Win", "Mac", "Unix", "NewWindow"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "F", "Win", "Mac", "Unix", "NewWindow"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "F", "Win", "Mac", "Unix", "NewWindow"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "F", "Win", "Mac", "Unix", "NewWindow"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "F", "Win", "Mac", "Unix", "NewWindow"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "S", "Next", "F", "NewWindow"
    };
    


}

/// <summary>
/// ActionLaunch_Type Table 196 and Table 207
/// </summary>
internal partial class APM_ActionLaunch_Type : APM_ActionLaunch_Type__Base
{
}


internal partial class APM_ActionLaunch_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionLaunch_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_ActionLaunch_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "Action")) 
        {
            ctx.Fail<APM_ActionLaunch_Type>($"Invalid value {val}, allowed are: [Action]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ActionLaunch_S 
/// </summary>
internal partial class APM_ActionLaunch_S : APM_ActionLaunch_S__Base
{
}


internal partial class APM_ActionLaunch_S__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionLaunch_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_ActionLaunch_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "Launch")) 
        {
            ctx.Fail<APM_ActionLaunch_S>($"Invalid value {val}, allowed are: [Launch]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ActionLaunch_Next 
/// </summary>
internal partial class APM_ActionLaunch_Next : APM_ActionLaunch_Next__Base
{
}


internal partial class APM_ActionLaunch_Next__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionLaunch_Next";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ActionLaunch_Next>(obj, "Next", IndirectRequirement.Either);
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
                    } else if ((ctx.Version >= 1.6m && APM_ActionGoToE.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_ActionGoToE, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version >= 2.0m && APM_ActionGoToDp.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_ActionGoToDp, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version == 1.2m && APM_ActionNOP.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_ActionNOP, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version == 1.2m && APM_ActionSetState.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_ActionSetState, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version >= 1.5m && APM_ActionSetOCGState.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_ActionSetOCGState, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version >= 1.5m && APM_ActionRendition.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_ActionRendition, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version >= 1.5m && APM_ActionTransition.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_ActionTransition, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version >= 1.6m && APM_ActionGoTo3DView.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_ActionGoTo3DView, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version >= 1.3m && APM_ActionECMAScript.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_ActionECMAScript, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version >= 2.0m && APM_ActionRichMediaExecute.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_ActionRichMediaExecute, PdfDictionary>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_ActionLaunch_Next>("Next did not match any allowable types: '[ActionGoTo,ActionGoToR,fn:SinceVersion(1.6,ActionGoToE),fn:SinceVersion(2.0,ActionGoToDp),ActionLaunch,fn:IsPDFVersion(1.2,ActionNOP),fn:IsPDFVersion(1.2,ActionSetState),ActionThread,ActionURI,ActionSound,ActionMovie,ActionHide,ActionNamed,ActionSubmitForm,ActionResetForm,ActionImportData,fn:SinceVersion(1.5,ActionSetOCGState),fn:SinceVersion(1.5,ActionRendition),fn:SinceVersion(1.5,ActionTransition),fn:SinceVersion(1.6,ActionGoTo3DView),fn:SinceVersion(1.3,ActionECMAScript),fn:SinceVersion(2.0,ActionRichMediaExecute)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ActionLaunch_Next>("Next is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ActionLaunch_F 
/// </summary>
internal partial class APM_ActionLaunch_F : APM_ActionLaunch_F__Base
{
}


internal partial class APM_ActionLaunch_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionLaunch_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ActionLaunch_F>(obj, "F", IndirectRequirement.Either);
        
        
        if ((!obj.ContainsKey("Win")||obj.ContainsKey("Mac")||obj.ContainsKey("Unix")) && utval == null) {
            ctx.Fail<APM_ActionLaunch_F>("F is required"); return;
        } else if (utval == null) {
            return;
        }
        
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
                ctx.Fail<APM_ActionLaunch_F>("F is required to one of 'dictionary;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ActionLaunch_Win 
/// </summary>
internal partial class APM_ActionLaunch_Win : APM_ActionLaunch_Win__Base
{
}


internal partial class APM_ActionLaunch_Win__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionLaunch_Win";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_ActionLaunch_Win>(obj, "Win", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_MicrosoftWindowsLaunchParam, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// ActionLaunch_Mac 
/// </summary>
internal partial class APM_ActionLaunch_Mac : APM_ActionLaunch_Mac__Base
{
}


internal partial class APM_ActionLaunch_Mac__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionLaunch_Mac";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNull, APM_ActionLaunch_Mac>(obj, "Mac", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ActionLaunch_Unix 
/// </summary>
internal partial class APM_ActionLaunch_Unix : APM_ActionLaunch_Unix__Base
{
}


internal partial class APM_ActionLaunch_Unix__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionLaunch_Unix";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNull, APM_ActionLaunch_Unix>(obj, "Unix", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ActionLaunch_NewWindow 
/// </summary>
internal partial class APM_ActionLaunch_NewWindow : APM_ActionLaunch_NewWindow__Base
{
}


internal partial class APM_ActionLaunch_NewWindow__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionLaunch_NewWindow";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_ActionLaunch_NewWindow>(obj, "NewWindow", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

