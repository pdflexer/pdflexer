// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_FieldTx : APM_FieldTx__Base
{
}

internal partial class APM_FieldTx__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "FieldTx";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_FieldTx_FT, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldTx_Parent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldTx_Kids, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldTx_T, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldTx_TU, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldTx_TM, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldTx_Ff, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldTx_V, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldTx_DV, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldTx_AA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldTx_DA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldTx_Q, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldTx_DS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldTx_RV, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FieldTx_MaxLen, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_FieldTx>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_FieldTx>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_FieldTx>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_FieldTx>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_FieldTx>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_FieldTx>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_FieldTx>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_FieldTx>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_FieldTx>($"Unknown field {extra} for version 2.0");
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
        "FT", "Parent", "Kids", "T", "Ff", "V", "DV", "DA", "Q", "MaxLen"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "V", "DV", "AA", "DA", "Q", "MaxLen"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "V", "DV", "AA", "DA", "Q", "MaxLen"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "V", "DV", "AA", "DA", "Q", "DS", "RV", "MaxLen"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "V", "DV", "AA", "DA", "Q", "DS", "RV", "MaxLen"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "V", "DV", "AA", "DA", "Q", "DS", "RV", "MaxLen"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "V", "DV", "AA", "DA", "Q", "DS", "RV", "MaxLen"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "V", "DV", "AA", "DA", "Q", "DS", "RV", "MaxLen"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "FT", "Parent", "Kids", "T", "TU", "TM", "Ff", "V", "DV", "AA", "DA", "Q", "DS", "RV", "MaxLen"
    };
    


}

/// <summary>
/// FieldTx_FT Table 226, Table 228 and Table 231
/// </summary>
internal partial class APM_FieldTx_FT : APM_FieldTx_FT__Base
{
}


internal partial class APM_FieldTx_FT__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldTx_FT";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FieldTx_FT>(obj, "FT", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "Tx")) 
        {
            ctx.Fail<APM_FieldTx_FT>($"Invalid value {val}, allowed are: [Tx]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FieldTx_Parent 
/// </summary>
internal partial class APM_FieldTx_Parent : APM_FieldTx_Parent__Base
{
}


internal partial class APM_FieldTx_Parent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldTx_Parent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_FieldTx_Parent>(obj, "Parent", IndirectRequirement.Either);
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
        } else if ((ctx.Version < 1.3m || (ctx.Version >= 1.3m && APM_FieldSig.MatchesType(ctx, val)))) 
        {
            ctx.Run<APM_FieldSig, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_FieldTx_Parent>("Parent did not match any allowable types: '[FieldTx,FieldBtnPush,FieldBtnCheckbox,FieldBtnRadio,FieldChoice,fn:SinceVersion(1.3,FieldSig),Field]'");
        }
        
    }


}

/// <summary>
/// FieldTx_Kids 
/// </summary>
internal partial class APM_FieldTx_Kids : APM_FieldTx_Kids__Base
{
}


internal partial class APM_FieldTx_Kids__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldTx_Kids";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_FieldTx_Kids>(obj, "Kids", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfFields, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// FieldTx_T 
/// </summary>
internal partial class APM_FieldTx_T : APM_FieldTx_T__Base
{
}


internal partial class APM_FieldTx_T__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldTx_T";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FieldTx_T>(obj, "T", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FieldTx_TU 
/// </summary>
internal partial class APM_FieldTx_TU : APM_FieldTx_TU__Base
{
}


internal partial class APM_FieldTx_TU__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldTx_TU";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FieldTx_TU>(obj, "TU", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FieldTx_TM 
/// </summary>
internal partial class APM_FieldTx_TM : APM_FieldTx_TM__Base
{
}


internal partial class APM_FieldTx_TM__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldTx_TM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FieldTx_TM>(obj, "TM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FieldTx_Ff Table 231
/// </summary>
internal partial class APM_FieldTx_Ff : APM_FieldTx_Ff__Base
{
}


internal partial class APM_FieldTx_Ff__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldTx_Ff";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FieldTx_Ff>(obj, "Ff", IndirectRequirement.Either);
        if (val == null) { return; }
        
        if (!(BitsClear(obj)&&ctx.Version < 1.4m && BitClear(obj)&&BitClear(obj)&&ctx.Version < 1.4m && BitsClear(obj)&&ctx.Version < 1.5m && BitsClear(obj)&&BitsClear(obj))) 
        {
            ctx.Fail<APM_FieldTx_Ff>($"Value failed special case check: fn:Eval(fn:BitsClear(15,20) && fn:BeforeVersion(1.4,fn:BitClear(21)) && fn:BitClear(22) && fn:BeforeVersion(1.4,fn:BitsClear(23,24)) && fn:BeforeVersion(1.5,fn:BitsClear(25,26)) && fn:BitsClear(27,32))");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FieldTx_V 
/// </summary>
internal partial class APM_FieldTx_V : APM_FieldTx_V__Base
{
}


internal partial class APM_FieldTx_V__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldTx_V";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_FieldTx_V>(obj, "V", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_FieldTx_V>("V is required to be indirect when a stream"); return; }
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
                ctx.Fail<APM_FieldTx_V>("V is required to one of 'stream;string-text', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// FieldTx_DV 
/// </summary>
internal partial class APM_FieldTx_DV : APM_FieldTx_DV__Base
{
}


internal partial class APM_FieldTx_DV__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldTx_DV";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_FieldTx_DV>(obj, "DV", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_FieldTx_DV>("DV is required to be indirect when a stream"); return; }
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
                ctx.Fail<APM_FieldTx_DV>("DV is required to one of 'stream;string-text', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// FieldTx_AA 
/// </summary>
internal partial class APM_FieldTx_AA : APM_FieldTx_AA__Base
{
}


internal partial class APM_FieldTx_AA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldTx_AA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_FieldTx_AA>(obj, "AA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_AddActionFormField, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// FieldTx_DA 
/// </summary>
internal partial class APM_FieldTx_DA : APM_FieldTx_DA__Base
{
}


internal partial class APM_FieldTx_DA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldTx_DA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfString, APM_FieldTx_DA>(obj, "DA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FieldTx_Q 
/// </summary>
internal partial class APM_FieldTx_Q : APM_FieldTx_Q__Base
{
}


internal partial class APM_FieldTx_Q__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldTx_Q";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FieldTx_Q>(obj, "Q", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 0 || val == 1 || val == 2)) 
        {
            ctx.Fail<APM_FieldTx_Q>($"Invalid value {val}, allowed are: [0,1,2]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FieldTx_DS 
/// </summary>
internal partial class APM_FieldTx_DS : APM_FieldTx_DS__Base
{
}


internal partial class APM_FieldTx_DS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldTx_DS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FieldTx_DS>(obj, "DS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FieldTx_RV 
/// </summary>
internal partial class APM_FieldTx_RV : APM_FieldTx_RV__Base
{
}


internal partial class APM_FieldTx_RV__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldTx_RV";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_FieldTx_RV>(obj, "RV", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_FieldTx_RV>("RV is required to be indirect when a stream"); return; }
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
                ctx.Fail<APM_FieldTx_RV>("RV is required to one of 'stream;string-text', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// FieldTx_MaxLen Table 232 and https://github.com/pdf-association/pdf-issues/issues/191
/// </summary>
internal partial class APM_FieldTx_MaxLen : APM_FieldTx_MaxLen__Base
{
}


internal partial class APM_FieldTx_MaxLen__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FieldTx_MaxLen";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FieldTx_MaxLen>(obj, "MaxLen", IndirectRequirement.Either);
        if (val == null) { return; }
        var MaxLen = obj.Get("MaxLen");
        if (!(gte(MaxLen,0))) 
        {
            ctx.Fail<APM_FieldTx_MaxLen>($"Value failed special case check: fn:Eval(@MaxLen>=0)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

