// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_FieldChoice : APM_FieldChoice__Base
{
}

internal partial class APM_FieldChoice__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "FieldChoice";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_FieldChoice_FT, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldChoice_Parent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldChoice_Kids, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldChoice_T, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldChoice_TU, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldChoice_TM, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldChoice_Ff, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldChoice_V, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldChoice_DV, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldChoice_AA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldChoice_DA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldChoice_Q, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldChoice_DS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldChoice_RV, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldChoice_Opt, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldChoice_TI, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldChoice_I, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_FieldChoice>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_FieldChoice>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_FieldChoice>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_FieldChoice>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_FieldChoice>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_FieldChoice>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_FieldChoice>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_FieldChoice>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_FieldChoice>($"Unknown field {extra} for version 2.0");
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
        "FT", "Parent", "Kids", "T", "Ff", "V", "DV", "DA", "Q", "TI"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "V", "DV", "AA", "DA", "Q", "TI"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "V", "DV", "AA", "DA", "Q", "Opt", "TI", "I"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "V", "DV", "AA", "DA", "Q", "DS", "RV", "Opt", "TI", "I"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "V", "DV", "AA", "DA", "Q", "DS", "RV", "Opt", "TI", "I"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "V", "DV", "AA", "DA", "Q", "DS", "RV", "Opt", "TI", "I"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "V", "DV", "AA", "DA", "Q", "DS", "RV", "Opt", "TI", "I"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "V", "DV", "AA", "DA", "Q", "DS", "RV", "Opt", "TI", "I"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "V", "DV", "AA", "DA", "Q", "DS", "RV", "Opt", "TI", "I"
    };
    


}

/// <summary>
/// FieldChoice_FT Table 226 and Table 234
/// </summary>
internal partial class APM_FieldChoice_FT : APM_FieldChoice_FT__Base
{
}


internal partial class APM_FieldChoice_FT__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldChoice_FT";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FieldChoice_FT>(obj, "FT", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Ch)) 
        {
            ctx.Fail<APM_FieldChoice_FT>($"Invalid value {val}, allowed are: [Ch]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FieldChoice_Parent 
/// </summary>
internal partial class APM_FieldChoice_Parent : APM_FieldChoice_Parent__Base
{
}


internal partial class APM_FieldChoice_Parent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldChoice_Parent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_FieldChoice_Parent>(obj, "Parent", IndirectRequirement.Either);
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
        } else if ((ctx.Version >= 1.3m && APM_FieldSig.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_FieldSig, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_FieldChoice_Parent>("Parent did not match any allowable types: '[FieldTx,FieldBtnPush,FieldBtnCheckbox,FieldBtnRadio,FieldChoice,fn:SinceVersion(1.3,FieldSig),Field]'");
        }
        
    }


}

/// <summary>
/// FieldChoice_Kids 
/// </summary>
internal partial class APM_FieldChoice_Kids : APM_FieldChoice_Kids__Base
{
}


internal partial class APM_FieldChoice_Kids__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldChoice_Kids";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_FieldChoice_Kids>(obj, "Kids", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfFields, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// FieldChoice_T 
/// </summary>
internal partial class APM_FieldChoice_T : APM_FieldChoice_T__Base
{
}


internal partial class APM_FieldChoice_T__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldChoice_T";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FieldChoice_T>(obj, "T", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FieldChoice_TU 
/// </summary>
internal partial class APM_FieldChoice_TU : APM_FieldChoice_TU__Base
{
}


internal partial class APM_FieldChoice_TU__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldChoice_TU";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FieldChoice_TU>(obj, "TU", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FieldChoice_TM 
/// </summary>
internal partial class APM_FieldChoice_TM : APM_FieldChoice_TM__Base
{
}


internal partial class APM_FieldChoice_TM__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldChoice_TM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FieldChoice_TM>(obj, "TM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FieldChoice_Ff Table 233
/// </summary>
internal partial class APM_FieldChoice_Ff : APM_FieldChoice_Ff__Base
{
}


internal partial class APM_FieldChoice_Ff__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldChoice_Ff";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FieldChoice_Ff>(obj, "Ff", IndirectRequirement.Either);
        if (val == null) { return; }
        
        if (!(BitsClear(val,0b00000000000000000011111111111000)&&BitsClear(val,0b00000000000100000000000000000000)&&(ctx.Version >= 1.4m || BitsClear(val,0b00000000011000000000000000000000))&&BitsClear(val,0b00000011100000000000000000000000)&&(ctx.Version > 1.5m && BitsClear(val,0b00000100000000000000000000000000))&&BitsClear(val,0b11111000000000000000000000000000))) 
        {
            ctx.Fail<APM_FieldChoice_Ff>($"Value failed special case check: fn:Eval(fn:BitsClear(4,14) && fn:BitClear(21) && fn:BeforeVersion(1.4,fn:BitsClear(22,23)) && fn:BitsClear(24,26) && fn:BeforeVersion(1.5,fn:BitClear(27)) && fn:BitsClear(28,32))");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FieldChoice_V 
/// </summary>
internal partial class APM_FieldChoice_V : APM_FieldChoice_V__Base
{
}


internal partial class APM_FieldChoice_V__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldChoice_V";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_FieldChoice_V>(obj, "V", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_ArrayOfStringsText, PdfArray>(stack, val, obj);
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
                ctx.Fail<APM_FieldChoice_V>("V is required to one of 'array;string-text', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// FieldChoice_DV 
/// </summary>
internal partial class APM_FieldChoice_DV : APM_FieldChoice_DV__Base
{
}


internal partial class APM_FieldChoice_DV__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldChoice_DV";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_FieldChoice_DV>(obj, "DV", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_ArrayOfStringsText, PdfArray>(stack, val, obj);
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
                ctx.Fail<APM_FieldChoice_DV>("DV is required to one of 'array;string-text', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// FieldChoice_AA 
/// </summary>
internal partial class APM_FieldChoice_AA : APM_FieldChoice_AA__Base
{
}


internal partial class APM_FieldChoice_AA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldChoice_AA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_FieldChoice_AA>(obj, "AA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_AddActionFormField, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// FieldChoice_DA 
/// </summary>
internal partial class APM_FieldChoice_DA : APM_FieldChoice_DA__Base
{
}


internal partial class APM_FieldChoice_DA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldChoice_DA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfString, APM_FieldChoice_DA>(obj, "DA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FieldChoice_Q 
/// </summary>
internal partial class APM_FieldChoice_Q : APM_FieldChoice_Q__Base
{
}


internal partial class APM_FieldChoice_Q__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldChoice_Q";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FieldChoice_Q>(obj, "Q", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 0m || val == 1m || val == 2m)) 
        {
            ctx.Fail<APM_FieldChoice_Q>($"Invalid value {val}, allowed are: [0,1,2]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FieldChoice_DS 
/// </summary>
internal partial class APM_FieldChoice_DS : APM_FieldChoice_DS__Base
{
}


internal partial class APM_FieldChoice_DS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldChoice_DS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FieldChoice_DS>(obj, "DS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FieldChoice_RV 
/// </summary>
internal partial class APM_FieldChoice_RV : APM_FieldChoice_RV__Base
{
}


internal partial class APM_FieldChoice_RV__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldChoice_RV";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_FieldChoice_RV>(obj, "RV", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_FieldChoice_RV>("RV is required to be indirect when a stream"); return; }
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
                ctx.Fail<APM_FieldChoice_RV>("RV is required to one of 'stream;string-text', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// FieldChoice_Opt Table 234
/// </summary>
internal partial class APM_FieldChoice_Opt : APM_FieldChoice_Opt__Base
{
}


internal partial class APM_FieldChoice_Opt__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldChoice_Opt";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_FieldChoice_Opt>(obj, "Opt", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfFieldChoiceOpt, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// FieldChoice_TI 
/// </summary>
internal partial class APM_FieldChoice_TI : APM_FieldChoice_TI__Base
{
}


internal partial class APM_FieldChoice_TI__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldChoice_TI";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FieldChoice_TI>(obj, "TI", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var TI = obj.Get("TI");
        var Opt = obj.Get("Opt");
        if (!((gte(TI,0)&&lt(TI,((Opt as PdfArray)?.Count))))) 
        {
            ctx.Fail<APM_FieldChoice_TI>($"Invalid value {val}, allowed are: [fn:Eval((@TI>=0) && (@TI<fn:ArrayLength(Opt)))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FieldChoice_I This entry shall be used when two or more elements in the Opt array have different names but the same export value or when the value of the choice field is an array. If the items identified by this entry differ from those in the V entry of the field dictionary (see discussion following this Table), the V entry shall be used
/// </summary>
internal partial class APM_FieldChoice_I : APM_FieldChoice_I__Base
{
}


internal partial class APM_FieldChoice_I__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldChoice_I";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_FieldChoice_I>(obj, "I", IndirectRequirement.Either);
        if (val == null) { return; }
        var I = obj.Get("I");
        if (!(ArraySortAscending(I, 1))) 
        {
            ctx.Fail<APM_FieldChoice_I>($"Value failed special case check: fn:Eval(fn:ArraySortAscending(I,1))");
        }
        // no value restrictions
        ctx.Run<APM_ArrayOfNonNegativeIntegersGeneral, PdfArray>(stack, val, obj);
        
    }


}

