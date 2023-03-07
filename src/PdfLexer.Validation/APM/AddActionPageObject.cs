// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_AddActionPageObject : APM_AddActionPageObject__Base
{
}

internal partial class APM_AddActionPageObject__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "AddActionPageObject";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_AddActionPageObject_O, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AddActionPageObject_C, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_AddActionPageObject>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_AddActionPageObject>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_AddActionPageObject>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_AddActionPageObject>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_AddActionPageObject>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_AddActionPageObject>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_AddActionPageObject>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_AddActionPageObject>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_AddActionPageObject>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        return false;
    }

    public static List<string> AllowedFields_12 { get; } = new List<string> 
    {
        "O", "C"
    };
    public static List<string> AllowedFields_13 { get; } = new List<string> 
    {
        "O", "C"
    };
    public static List<string> AllowedFields_14 { get; } = new List<string> 
    {
        "O", "C"
    };
    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "O", "C"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "O", "C"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "O", "C"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "O", "C"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "O", "C"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "O", "C"
    };
    


}

/// <summary>
/// AddActionPageObject_O Table 198
/// </summary>
internal partial class APM_AddActionPageObject_O : APM_AddActionPageObject_O__Base
{
}


internal partial class APM_AddActionPageObject_O__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AddActionPageObject_O";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_AddActionPageObject_O>(obj, "O", IndirectRequirement.Either);
        if (val == null) { return; }
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
            ctx.Fail<APM_AddActionPageObject_O>("O did not match any allowable types: '[ActionGoTo,ActionGoToR,fn:SinceVersion(1.6,ActionGoToE),fn:SinceVersion(2.0,ActionGoToDp),ActionLaunch,fn:IsPDFVersion(1.2,ActionNOP),fn:IsPDFVersion(1.2,ActionSetState),ActionThread,ActionURI,ActionSound,ActionMovie,ActionHide,ActionNamed,ActionSubmitForm,ActionResetForm,ActionImportData,fn:SinceVersion(1.5,ActionSetOCGState),fn:SinceVersion(1.5,ActionRendition),fn:SinceVersion(1.5,ActionTransition),fn:SinceVersion(1.6,ActionGoTo3DView),fn:SinceVersion(1.3,ActionECMAScript),fn:SinceVersion(2.0,ActionRichMediaExecute)]'");
        }
        
    }


}

/// <summary>
/// AddActionPageObject_C 
/// </summary>
internal partial class APM_AddActionPageObject_C : APM_AddActionPageObject_C__Base
{
}


internal partial class APM_AddActionPageObject_C__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AddActionPageObject_C";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_AddActionPageObject_C>(obj, "C", IndirectRequirement.Either);
        if (val == null) { return; }
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
            ctx.Fail<APM_AddActionPageObject_C>("C did not match any allowable types: '[ActionGoTo,ActionGoToR,fn:SinceVersion(1.6,ActionGoToE),fn:SinceVersion(2.0,ActionGoToDp),ActionLaunch,fn:IsPDFVersion(1.2,ActionNOP),fn:IsPDFVersion(1.2,ActionSetState),ActionThread,ActionURI,ActionSound,ActionMovie,ActionHide,ActionNamed,ActionSubmitForm,ActionResetForm,ActionImportData,fn:SinceVersion(1.5,ActionSetOCGState),fn:SinceVersion(1.5,ActionRendition),fn:SinceVersion(1.5,ActionTransition),fn:SinceVersion(1.6,ActionGoTo3DView),fn:SinceVersion(1.3,ActionECMAScript),fn:SinceVersion(2.0,ActionRichMediaExecute)]'");
        }
        
    }


}
