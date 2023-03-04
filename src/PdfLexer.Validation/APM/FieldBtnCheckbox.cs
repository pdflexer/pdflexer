// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_FieldBtnCheckbox : APM_FieldBtnCheckbox_Base
{
}

internal partial class APM_FieldBtnCheckbox_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "FieldBtnCheckbox";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_FieldBtnCheckbox_FT, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldBtnCheckbox_Parent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldBtnCheckbox_Kids, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldBtnCheckbox_T, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldBtnCheckbox_TU, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldBtnCheckbox_TM, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldBtnCheckbox_Ff, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldBtnCheckbox_AA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldBtnCheckbox_DA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldBtnCheckbox_Q, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldBtnCheckbox_DS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldBtnCheckbox_RV, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldBtnCheckbox_Opt, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldBtnCheckbox_V, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldBtnCheckbox_DV, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_FieldBtnCheckbox>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_FieldBtnCheckbox>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_FieldBtnCheckbox>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_FieldBtnCheckbox>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_FieldBtnCheckbox>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_FieldBtnCheckbox>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_FieldBtnCheckbox>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_FieldBtnCheckbox>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_FieldBtnCheckbox>($"Unknown field {extra} for version 2.0");
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

    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "Ff", "DA", "Q", "V", "DV"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "AA", "DA", "Q", "V", "DV"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "AA", "DA", "Q", "Opt", "V", "DV"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "AA", "DA", "Q", "DS", "RV", "Opt", "V", "DV"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "AA", "DA", "Q", "DS", "RV", "Opt", "V", "DV"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "AA", "DA", "Q", "DS", "RV", "Opt", "V", "DV"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "AA", "DA", "Q", "DS", "RV", "Opt", "V", "DV"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "AA", "DA", "Q", "DS", "RV", "Opt", "V", "DV"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "AA", "DA", "Q", "DS", "RV", "Opt", "V", "DV"
    };
    


}

/// <summary>
/// FieldBtnCheckbox_FT Table 226
/// </summary>
internal partial class APM_FieldBtnCheckbox_FT : APM_FieldBtnCheckbox_FT_Base
{
}


internal partial class APM_FieldBtnCheckbox_FT_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldBtnCheckbox_FT";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FieldBtnCheckbox_FT>(obj, "FT", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "Btn")) 
        {
            ctx.Fail<APM_FieldBtnCheckbox_FT>($"Invalid value {val}, allowed are: [Btn]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// FieldBtnCheckbox_Parent 
/// </summary>
internal partial class APM_FieldBtnCheckbox_Parent : APM_FieldBtnCheckbox_Parent_Base
{
}


internal partial class APM_FieldBtnCheckbox_Parent_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldBtnCheckbox_Parent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_FieldBtnCheckbox_Parent>(obj, "Parent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_FieldTx.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_FieldTx, PdfDictionary>(stack, val, obj);
        } else if (APM_FieldBtnPush.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_FieldBtnPush, PdfDictionary>(stack, val, obj);
        } else if (APM_FieldBtnCheckbox.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_FieldBtnCheckbox, PdfDictionary>(stack, val, obj);
        } else if (APM_FieldBtnRadio.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_FieldBtnRadio, PdfDictionary>(stack, val, obj);
        } else if (APM_FieldChoice.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_FieldChoice, PdfDictionary>(stack, val, obj);
        } else if (APM_Field.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_Field, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_FieldBtnCheckbox_Parent>("Parent did not match any allowable types: '[FieldTx,FieldBtnPush,FieldBtnCheckbox,FieldBtnRadio,FieldChoice,fn:SinceVersion(1.3,FieldSig),Field]'");
        }
        
    }


}

/// <summary>
/// FieldBtnCheckbox_Kids 
/// </summary>
internal partial class APM_FieldBtnCheckbox_Kids : APM_FieldBtnCheckbox_Kids_Base
{
}


internal partial class APM_FieldBtnCheckbox_Kids_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldBtnCheckbox_Kids";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_FieldBtnCheckbox_Kids>(obj, "Kids", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfFields, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// FieldBtnCheckbox_T 
/// </summary>
internal partial class APM_FieldBtnCheckbox_T : APM_FieldBtnCheckbox_T_Base
{
}


internal partial class APM_FieldBtnCheckbox_T_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldBtnCheckbox_T";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FieldBtnCheckbox_T>(obj, "T", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FieldBtnCheckbox_TU 
/// </summary>
internal partial class APM_FieldBtnCheckbox_TU : APM_FieldBtnCheckbox_TU_Base
{
}


internal partial class APM_FieldBtnCheckbox_TU_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldBtnCheckbox_TU";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FieldBtnCheckbox_TU>(obj, "TU", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FieldBtnCheckbox_TM 
/// </summary>
internal partial class APM_FieldBtnCheckbox_TM : APM_FieldBtnCheckbox_TM_Base
{
}


internal partial class APM_FieldBtnCheckbox_TM_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldBtnCheckbox_TM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FieldBtnCheckbox_TM>(obj, "TM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FieldBtnCheckbox_Ff Table 227 and Table 229
/// </summary>
internal partial class APM_FieldBtnCheckbox_Ff : APM_FieldBtnCheckbox_Ff_Base
{
}


internal partial class APM_FieldBtnCheckbox_Ff_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldBtnCheckbox_Ff";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FieldBtnCheckbox_Ff>(obj, "Ff", IndirectRequirement.Either);
        if (val == null) { return; }
        // TODO special case
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FieldBtnCheckbox_AA 
/// </summary>
internal partial class APM_FieldBtnCheckbox_AA : APM_FieldBtnCheckbox_AA_Base
{
}


internal partial class APM_FieldBtnCheckbox_AA_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldBtnCheckbox_AA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_FieldBtnCheckbox_AA>(obj, "AA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_AddActionFormField, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// FieldBtnCheckbox_DA 
/// </summary>
internal partial class APM_FieldBtnCheckbox_DA : APM_FieldBtnCheckbox_DA_Base
{
}


internal partial class APM_FieldBtnCheckbox_DA_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldBtnCheckbox_DA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfString, APM_FieldBtnCheckbox_DA>(obj, "DA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FieldBtnCheckbox_Q 
/// </summary>
internal partial class APM_FieldBtnCheckbox_Q : APM_FieldBtnCheckbox_Q_Base
{
}


internal partial class APM_FieldBtnCheckbox_Q_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldBtnCheckbox_Q";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FieldBtnCheckbox_Q>(obj, "Q", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == 0 || val == 1 || val == 2)) 
        {
            ctx.Fail<APM_FieldBtnCheckbox_Q>($"Invalid value {val}, allowed are: [0,1,2]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// FieldBtnCheckbox_DS 
/// </summary>
internal partial class APM_FieldBtnCheckbox_DS : APM_FieldBtnCheckbox_DS_Base
{
}


internal partial class APM_FieldBtnCheckbox_DS_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldBtnCheckbox_DS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FieldBtnCheckbox_DS>(obj, "DS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FieldBtnCheckbox_RV 
/// </summary>
internal partial class APM_FieldBtnCheckbox_RV : APM_FieldBtnCheckbox_RV_Base
{
}


internal partial class APM_FieldBtnCheckbox_RV_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldBtnCheckbox_RV";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_FieldBtnCheckbox_RV>(obj, "RV", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_FieldBtnCheckbox_RV>("RV is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
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
                ctx.Fail<APM_FieldBtnCheckbox_RV>("RV is required to one of 'stream;string-text', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// FieldBtnCheckbox_Opt Table 227 and Table 230
/// </summary>
internal partial class APM_FieldBtnCheckbox_Opt : APM_FieldBtnCheckbox_Opt_Base
{
}


internal partial class APM_FieldBtnCheckbox_Opt_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldBtnCheckbox_Opt";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_FieldBtnCheckbox_Opt>(obj, "Opt", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfStringsText, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// FieldBtnCheckbox_V 
/// </summary>
internal partial class APM_FieldBtnCheckbox_V : APM_FieldBtnCheckbox_V_Base
{
}


internal partial class APM_FieldBtnCheckbox_V_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldBtnCheckbox_V";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_FieldBtnCheckbox_V>(obj, "V", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FieldBtnCheckbox_DV 
/// </summary>
internal partial class APM_FieldBtnCheckbox_DV : APM_FieldBtnCheckbox_DV_Base
{
}


internal partial class APM_FieldBtnCheckbox_DV_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldBtnCheckbox_DV";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_FieldBtnCheckbox_DV>(obj, "DV", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

