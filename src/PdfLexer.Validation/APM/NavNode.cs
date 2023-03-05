// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_NavNode : APM_NavNode__Base
{
}

internal partial class APM_NavNode__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "NavNode";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_NavNode_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_NavNode_NA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_NavNode_PA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_NavNode_Next, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_NavNode_Prev, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_NavNode_Dur, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_NavNode>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_NavNode>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_NavNode>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_NavNode>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_NavNode>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_NavNode>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_NavNode_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "NA", "PA", "Next", "Prev", "Dur"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "NA", "PA", "Next", "Prev", "Dur"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "NA", "PA", "Next", "Prev", "Dur"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "NA", "PA", "Next", "Prev", "Dur"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "NA", "PA", "Next", "Prev", "Dur"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "NA", "PA", "Next", "Prev", "Dur"
    };
    


}

/// <summary>
/// NavNode_Type Table 165
/// </summary>
internal partial class APM_NavNode_Type : APM_NavNode_Type__Base
{
}


internal partial class APM_NavNode_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "NavNode_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_NavNode_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "NavNode")) 
        {
            ctx.Fail<APM_NavNode_Type>($"Invalid value {val}, allowed are: [NavNode]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// NavNode_NA 
/// </summary>
internal partial class APM_NavNode_NA : APM_NavNode_NA__Base
{
}


internal partial class APM_NavNode_NA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "NavNode_NA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_NavNode_NA>(obj, "NA", IndirectRequirement.Either);
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
        } else if ((ctx.Version >= 1.6m && APM_ActionGoToE.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_ActionGoToE, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 2.0m && APM_ActionGoToDp.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_ActionGoToDp, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 1.6m && APM_ActionGoTo3DView.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_ActionGoTo3DView, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 2.0m && APM_ActionRichMediaExecute.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_ActionRichMediaExecute, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_NavNode_NA>("NA did not match any allowable types: '[ActionGoTo,ActionGoToR,fn:SinceVersion(1.6,ActionGoToE),fn:SinceVersion(2.0,ActionGoToDp),ActionLaunch,ActionThread,ActionURI,ActionSound,ActionMovie,ActionHide,ActionNamed,ActionSubmitForm,ActionResetForm,ActionImportData,ActionSetOCGState,ActionRendition,ActionTransition,fn:SinceVersion(1.6,ActionGoTo3DView),ActionECMAScript,fn:SinceVersion(2.0,ActionRichMediaExecute)]'");
        }
        
    }


}

/// <summary>
/// NavNode_PA 
/// </summary>
internal partial class APM_NavNode_PA : APM_NavNode_PA__Base
{
}


internal partial class APM_NavNode_PA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "NavNode_PA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_NavNode_PA>(obj, "PA", IndirectRequirement.Either);
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
        } else if ((ctx.Version >= 1.6m && APM_ActionGoToE.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_ActionGoToE, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 2.0m && APM_ActionGoToDp.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_ActionGoToDp, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 1.6m && APM_ActionGoTo3DView.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_ActionGoTo3DView, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 2.0m && APM_ActionRichMediaExecute.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_ActionRichMediaExecute, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_NavNode_PA>("PA did not match any allowable types: '[ActionGoTo,ActionGoToR,fn:SinceVersion(1.6,ActionGoToE),fn:SinceVersion(2.0,ActionGoToDp),ActionLaunch,ActionThread,ActionURI,ActionSound,ActionMovie,ActionHide,ActionNamed,ActionSubmitForm,ActionResetForm,ActionImportData,ActionSetOCGState,ActionRendition,ActionTransition,fn:SinceVersion(1.6,ActionGoTo3DView),ActionECMAScript,fn:SinceVersion(2.0,ActionRichMediaExecute)]'");
        }
        
    }


}

/// <summary>
/// NavNode_Next 
/// </summary>
internal partial class APM_NavNode_Next : APM_NavNode_Next__Base
{
}


internal partial class APM_NavNode_Next__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "NavNode_Next";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_NavNode_Next>(obj, "Next", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_NavNode, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// NavNode_Prev 
/// </summary>
internal partial class APM_NavNode_Prev : APM_NavNode_Prev__Base
{
}


internal partial class APM_NavNode_Prev__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "NavNode_Prev";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_NavNode_Prev>(obj, "Prev", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_NavNode, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// NavNode_Dur 
/// </summary>
internal partial class APM_NavNode_Dur : APM_NavNode_Dur__Base
{
}


internal partial class APM_NavNode_Dur__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "NavNode_Dur";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_NavNode_Dur>(obj, "Dur", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

