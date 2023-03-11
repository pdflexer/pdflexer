// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_ActionURI : APM_ActionURI__Base
{
}

internal partial class APM_ActionURI__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "ActionURI";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_ActionURI_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionURI_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionURI_Next, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionURI_URI, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionURI_IsMap, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.1m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_11.Contains(x)))
                {
                    ctx.Fail<APM_ActionURI>($"Unknown field {extra} for version 1.1");
                }
                break;
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_ActionURI>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_ActionURI>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_ActionURI>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_ActionURI>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_ActionURI>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_ActionURI>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_ActionURI>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_ActionURI>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_ActionURI>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_ActionURI_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_11 { get; } = new List<string> 
    {
        "Type", "S", "URI", "IsMap"
    };
    public static List<string> AllowedFields_12 { get; } = new List<string> 
    {
        "Type", "S", "Next", "URI", "IsMap"
    };
    public static List<string> AllowedFields_13 { get; } = new List<string> 
    {
        "Type", "S", "Next", "URI", "IsMap"
    };
    public static List<string> AllowedFields_14 { get; } = new List<string> 
    {
        "Type", "S", "Next", "URI", "IsMap"
    };
    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Type", "S", "Next", "URI", "IsMap"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Type", "S", "Next", "URI", "IsMap"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "S", "Next", "URI", "IsMap"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "S", "Next", "URI", "IsMap"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "S", "Next", "URI", "IsMap"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "S", "Next", "URI", "IsMap"
    };
    


}

/// <summary>
/// ActionURI_Type Table 196 and Table 210
/// </summary>
internal partial class APM_ActionURI_Type : APM_ActionURI_Type__Base
{
}


internal partial class APM_ActionURI_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionURI_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_ActionURI_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Action)) 
        {
            ctx.Fail<APM_ActionURI_Type>($"Invalid value {val}, allowed are: [Action]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ActionURI_S 
/// </summary>
internal partial class APM_ActionURI_S : APM_ActionURI_S__Base
{
}


internal partial class APM_ActionURI_S__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionURI_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfName, APM_ActionURI_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.URI)) 
        {
            ctx.Fail<APM_ActionURI_S>($"Invalid value {val}, allowed are: [URI]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ActionURI_Next 
/// </summary>
internal partial class APM_ActionURI_Next : APM_ActionURI_Next__Base
{
}


internal partial class APM_ActionURI_Next__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionURI_Next";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ActionURI_Next>(obj, "Next", IndirectRequirement.Either);
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
                        ctx.Fail<APM_ActionURI_Next>("Next did not match any allowable types: '[ActionGoTo,ActionGoToR,fn:SinceVersion(1.6,ActionGoToE),fn:SinceVersion(2.0,ActionGoToDp),ActionLaunch,ActionThread,fn:IsPDFVersion(1.2,ActionNOP),fn:IsPDFVersion(1.2,ActionSetState),ActionURI,ActionSound,ActionMovie,ActionHide,ActionNamed,ActionSubmitForm,ActionResetForm,ActionImportData,fn:SinceVersion(1.5,ActionSetOCGState),fn:SinceVersion(1.5,ActionRendition),fn:SinceVersion(1.5,ActionTransition),fn:SinceVersion(1.6,ActionGoTo3DView),fn:SinceVersion(1.3,ActionECMAScript),fn:SinceVersion(2.0,ActionRichMediaExecute)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ActionURI_Next>("Next is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ActionURI_URI 
/// </summary>
internal partial class APM_ActionURI_URI : APM_ActionURI_URI__Base
{
}


internal partial class APM_ActionURI_URI__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionURI_URI";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfString, APM_ActionURI_URI>(obj, "URI", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ActionURI_IsMap 
/// </summary>
internal partial class APM_ActionURI_IsMap : APM_ActionURI_IsMap__Base
{
}


internal partial class APM_ActionURI_IsMap__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionURI_IsMap";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfBoolean, APM_ActionURI_IsMap>(obj, "IsMap", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

