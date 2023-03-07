// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_ActionSound : APM_ActionSound__Base
{
}

internal partial class APM_ActionSound__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "ActionSound";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_ActionSound_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionSound_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionSound_Next, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionSound_Sound, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionSound_Volume, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionSound_Synchronous, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionSound_Repeat, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionSound_Mix, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_ActionSound>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_ActionSound>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_ActionSound>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_ActionSound>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_ActionSound>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_ActionSound>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_ActionSound>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_ActionSound>($"Unknown field {extra} for version 1.9");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_ActionSound_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "Sound", "Volume", "Synchronous", "Repeat", "Mix"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "Sound", "Volume", "Synchronous", "Repeat", "Mix"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "Sound", "Volume", "Synchronous", "Repeat", "Mix"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "Sound", "Volume", "Synchronous", "Repeat", "Mix"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "Sound", "Volume", "Synchronous", "Repeat", "Mix"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "Sound", "Volume", "Synchronous", "Repeat", "Mix"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "Sound", "Volume", "Synchronous", "Repeat", "Mix"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "S", "Next", "Sound", "Volume", "Synchronous", "Repeat", "Mix"
    };
    


}

/// <summary>
/// ActionSound_Type Table 196 and Table 212
/// </summary>
internal partial class APM_ActionSound_Type : APM_ActionSound_Type__Base
{
}


internal partial class APM_ActionSound_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSound_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_ActionSound_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Action)) 
        {
            ctx.Fail<APM_ActionSound_Type>($"Invalid value {val}, allowed are: [Action]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ActionSound_S 
/// </summary>
internal partial class APM_ActionSound_S : APM_ActionSound_S__Base
{
}


internal partial class APM_ActionSound_S__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSound_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_ActionSound_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Sound)) 
        {
            ctx.Fail<APM_ActionSound_S>($"Invalid value {val}, allowed are: [Sound]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ActionSound_Next 
/// </summary>
internal partial class APM_ActionSound_Next : APM_ActionSound_Next__Base
{
}


internal partial class APM_ActionSound_Next__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSound_Next";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ActionSound_Next>(obj, "Next", IndirectRequirement.Either);
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
                        ctx.Fail<APM_ActionSound_Next>("Next did not match any allowable types: '[ActionGoTo,ActionGoToR,fn:SinceVersion(1.6,ActionGoToE),fn:SinceVersion(2.0,ActionGoToDp),ActionLaunch,fn:IsPDFVersion(1.2,ActionNOP),fn:IsPDFVersion(1.2,ActionSetState),ActionThread,ActionURI,ActionSound,ActionMovie,ActionHide,ActionNamed,ActionSubmitForm,ActionResetForm,ActionImportData,fn:SinceVersion(1.5,ActionSetOCGState),fn:SinceVersion(1.5,ActionRendition),fn:SinceVersion(1.5,ActionTransition),fn:SinceVersion(1.6,ActionGoTo3DView),fn:SinceVersion(1.3,ActionECMAScript),fn:SinceVersion(2.0,ActionRichMediaExecute)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ActionSound_Next>("Next is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ActionSound_Sound 
/// </summary>
internal partial class APM_ActionSound_Sound : APM_ActionSound_Sound__Base
{
}


internal partial class APM_ActionSound_Sound__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSound_Sound";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfStream, APM_ActionSound_Sound>(obj, "Sound", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_SoundObject, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// ActionSound_Volume 
/// </summary>
internal partial class APM_ActionSound_Volume : APM_ActionSound_Volume__Base
{
}


internal partial class APM_ActionSound_Volume__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSound_Volume";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_ActionSound_Volume>(obj, "Volume", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var Volume = obj.Get("Volume");
        if (!((gte(Volume,-1)&&lte(Volume,1)))) 
        {
            ctx.Fail<APM_ActionSound_Volume>($"Invalid value {val}, allowed are: [fn:Eval((@Volume>=-1) && (@Volume<=1))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ActionSound_Synchronous 
/// </summary>
internal partial class APM_ActionSound_Synchronous : APM_ActionSound_Synchronous__Base
{
}


internal partial class APM_ActionSound_Synchronous__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSound_Synchronous";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_ActionSound_Synchronous>(obj, "Synchronous", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ActionSound_Repeat 
/// </summary>
internal partial class APM_ActionSound_Repeat : APM_ActionSound_Repeat__Base
{
}


internal partial class APM_ActionSound_Repeat__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSound_Repeat";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_ActionSound_Repeat>(obj, "Repeat", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ActionSound_Mix 
/// </summary>
internal partial class APM_ActionSound_Mix : APM_ActionSound_Mix__Base
{
}


internal partial class APM_ActionSound_Mix__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSound_Mix";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_ActionSound_Mix>(obj, "Mix", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}
