// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_ActionGoTo : APM_ActionGoTo__Base
{
}

internal partial class APM_ActionGoTo__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "ActionGoTo";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_ActionGoTo_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionGoTo_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionGoTo_Next, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionGoTo_D, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionGoTo_SD, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.1m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_11.Contains(x)))
                {
                    ctx.Fail<APM_ActionGoTo>($"Unknown field {extra} for version 1.1");
                }
                break;
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_ActionGoTo>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_ActionGoTo>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_ActionGoTo>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_ActionGoTo>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_ActionGoTo>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_ActionGoTo>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_ActionGoTo>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_ActionGoTo>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_ActionGoTo>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_ActionGoTo_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_11 { get; } = new List<string> 
    {
        "Type", "S", "D"
    };
    public static List<string> AllowedFields_12 { get; } = new List<string> 
    {
        "Type", "S", "Next", "D"
    };
    public static List<string> AllowedFields_13 { get; } = new List<string> 
    {
        "Type", "S", "Next", "D"
    };
    public static List<string> AllowedFields_14 { get; } = new List<string> 
    {
        "Type", "S", "Next", "D"
    };
    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Type", "S", "Next", "D"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Type", "S", "Next", "D"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "S", "Next", "D"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "S", "Next", "D"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "S", "Next", "D"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "S", "Next", "D", "SD"
    };
    


}

/// <summary>
/// ActionGoTo_Type Table 196 and Table 202
/// </summary>
internal partial class APM_ActionGoTo_Type : APM_ActionGoTo_Type__Base
{
}


internal partial class APM_ActionGoTo_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionGoTo_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_ActionGoTo_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "Action")) 
        {
            ctx.Fail<APM_ActionGoTo_Type>($"Invalid value {val}, allowed are: [Action]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ActionGoTo_S 
/// </summary>
internal partial class APM_ActionGoTo_S : APM_ActionGoTo_S__Base
{
}


internal partial class APM_ActionGoTo_S__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionGoTo_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_ActionGoTo_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "GoTo")) 
        {
            ctx.Fail<APM_ActionGoTo_S>($"Invalid value {val}, allowed are: [GoTo]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ActionGoTo_Next 
/// </summary>
internal partial class APM_ActionGoTo_Next : APM_ActionGoTo_Next__Base
{
}


internal partial class APM_ActionGoTo_Next__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionGoTo_Next";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ActionGoTo_Next>(obj, "Next", IndirectRequirement.Either);
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
                    } else if ((ctx.Version < 1.6m || (ctx.Version >= 1.6m && APM_ActionGoToE.MatchesType(ctx, val)))) 
                    {
                        ctx.Run<APM_ActionGoToE, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version < 2.0m || (ctx.Version >= 2.0m && APM_ActionGoToDp.MatchesType(ctx, val)))) 
                    {
                        ctx.Run<APM_ActionGoToDp, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version == 1.2m && APM_ActionNOP.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_ActionNOP, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version == 1.2m && APM_ActionSetState.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_ActionSetState, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version < 1.5m || (ctx.Version >= 1.5m && APM_ActionSetOCGState.MatchesType(ctx, val)))) 
                    {
                        ctx.Run<APM_ActionSetOCGState, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version < 1.5m || (ctx.Version >= 1.5m && APM_ActionRendition.MatchesType(ctx, val)))) 
                    {
                        ctx.Run<APM_ActionRendition, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version < 1.5m || (ctx.Version >= 1.5m && APM_ActionTransition.MatchesType(ctx, val)))) 
                    {
                        ctx.Run<APM_ActionTransition, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version < 1.6m || (ctx.Version >= 1.6m && APM_ActionGoTo3DView.MatchesType(ctx, val)))) 
                    {
                        ctx.Run<APM_ActionGoTo3DView, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version < 1.3m || (ctx.Version >= 1.3m && APM_ActionECMAScript.MatchesType(ctx, val)))) 
                    {
                        ctx.Run<APM_ActionECMAScript, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version < 2.0m || (ctx.Version >= 2.0m && APM_ActionRichMediaExecute.MatchesType(ctx, val)))) 
                    {
                        ctx.Run<APM_ActionRichMediaExecute, PdfDictionary>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_ActionGoTo_Next>("Next did not match any allowable types: '[ActionGoTo,ActionGoToR,fn:SinceVersion(1.6,ActionGoToE),fn:SinceVersion(2.0,ActionGoToDp),ActionLaunch,fn:IsPDFVersion(1.2,ActionNOP),fn:IsPDFVersion(1.2,ActionSetState),ActionThread,ActionURI,ActionSound,ActionMovie,ActionHide,ActionNamed,ActionSubmitForm,ActionResetForm,ActionImportData,fn:SinceVersion(1.5,ActionSetOCGState),fn:SinceVersion(1.5,ActionRendition),fn:SinceVersion(1.5,ActionTransition),fn:SinceVersion(1.6,ActionGoTo3DView),fn:SinceVersion(1.3,ActionECMAScript),fn:SinceVersion(2.0,ActionRichMediaExecute)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ActionGoTo_Next>("Next is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ActionGoTo_D 
/// </summary>
internal partial class APM_ActionGoTo_D : APM_ActionGoTo_D__Base
{
}


internal partial class APM_ActionGoTo_D__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionGoTo_D";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ActionGoTo_D>(obj, "D", IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ActionGoTo_D>("D is required"); return; }
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
                        ctx.Fail<APM_ActionGoTo_D>("D did not match any allowable types: '[DestXYZArray,Dest0Array,Dest1Array,Dest4Array]'");
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
                ctx.Fail<APM_ActionGoTo_D>("D is required to one of 'array;name;string-byte', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ActionGoTo_SD 
/// </summary>
internal partial class APM_ActionGoTo_SD : APM_ActionGoTo_SD__Base
{
}


internal partial class APM_ActionGoTo_SD__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionGoTo_SD";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_ActionGoTo_SD>(obj, "SD", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_DestXYZStructArray.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_DestXYZStructArray, PdfArray>(stack, val, obj);
        } else if (APM_Dest0StructArray.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_Dest0StructArray, PdfArray>(stack, val, obj);
        } else if (APM_Dest1StructArray.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_Dest1StructArray, PdfArray>(stack, val, obj);
        } else if (APM_Dest4StructArray.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_Dest4StructArray, PdfArray>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_ActionGoTo_SD>("SD did not match any allowable types: '[DestXYZStructArray,Dest0StructArray,Dest1StructArray,Dest4StructArray]'");
        }
        
    }


}

