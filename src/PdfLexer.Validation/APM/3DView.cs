// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_3DView : APM_3DView__Base
{
}

internal partial class APM_3DView__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "3DView";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_3DView_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DView_XN, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DView_IN, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DView_MS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DView_MA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DView_C2W, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DView_U3DPath, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DView_CO, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DView_P, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DView_O, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DView_BG, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DView_RM, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DView_LS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DView_SA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DView_NA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DView_NR, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_3DView>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_3DView>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_3DView>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_3DView>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_3DView>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_3DView_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "XN", "IN", "MS", "C2W", "U3DPath", "CO", "P", "O", "BG"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "XN", "IN", "MS", "C2W", "U3DPath", "CO", "P", "O", "BG", "RM", "LS", "SA", "NA", "NR"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "XN", "IN", "MS", "C2W", "U3DPath", "CO", "P", "O", "BG", "RM", "LS", "SA", "NA", "NR"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "XN", "IN", "MS", "C2W", "U3DPath", "CO", "P", "O", "BG", "RM", "LS", "SA", "NA", "NR"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "XN", "IN", "MS", "MA", "C2W", "U3DPath", "CO", "P", "O", "BG", "RM", "LS", "SA", "NA", "NR"
    };
    


}

/// <summary>
/// 3DView_Type Table 315
/// </summary>
internal partial class APM_3DView_Type : APM_3DView_Type__Base
{
}


internal partial class APM_3DView_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DView_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_3DView_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.N3DView)) 
        {
            ctx.Fail<APM_3DView_Type>($"Invalid value {val}, allowed are: [3DView]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// 3DView_XN 
/// </summary>
internal partial class APM_3DView_XN : APM_3DView_XN__Base
{
}


internal partial class APM_3DView_XN__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DView_XN";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfString, APM_3DView_XN>(obj, "XN", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// 3DView_IN 
/// </summary>
internal partial class APM_3DView_IN : APM_3DView_IN__Base
{
}


internal partial class APM_3DView_IN__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DView_IN";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_3DView_IN>(obj, "IN", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// 3DView_MS 
/// </summary>
internal partial class APM_3DView_MS : APM_3DView_MS__Base
{
}


internal partial class APM_3DView_MS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DView_MS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_3DView_MS>(obj, "MS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.M || val == PdfName.U3D)) 
        {
            ctx.Fail<APM_3DView_MS>($"Invalid value {val}, allowed are: [M,U3D]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// 3DView_MA 
/// </summary>
internal partial class APM_3DView_MA : APM_3DView_MA__Base
{
}


internal partial class APM_3DView_MA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DView_MA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_3DView_MA>(obj, "MA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf3DMeasure, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DView_C2W 
/// </summary>
internal partial class APM_3DView_C2W : APM_3DView_C2W__Base
{
}


internal partial class APM_3DView_C2W__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DView_C2W";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var MS = obj.Get("MS");
        var val = ctx.GetOptional<PdfArray, APM_3DView_C2W>(obj, "C2W", IndirectRequirement.Either);
        if ((eq(MS,PdfName.M)) && val == null) {
            ctx.Fail<APM_3DView_C2W>("C2W is required when 'fn:IsRequired(@MS==M)"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf3DTransMatrix, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DView_U3DPath 
/// </summary>
internal partial class APM_3DView_U3DPath : APM_3DView_U3DPath__Base
{
}


internal partial class APM_3DView_U3DPath__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DView_U3DPath";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_3DView_U3DPath>(obj, "U3DPath", IndirectRequirement.Either);
        
        var MS = obj.Get("MS");
        if ((eq(MS,PdfName.U3D)) && utval == null) {
            ctx.Fail<APM_3DView_U3DPath>("U3DPath is required"); return;
        } else if (utval == null) {
            return;
        }
        
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
                ctx.Fail<APM_3DView_U3DPath>("U3DPath is required to one of 'array;string-text', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// 3DView_CO 
/// </summary>
internal partial class APM_3DView_CO : APM_3DView_CO__Base
{
}


internal partial class APM_3DView_CO__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DView_CO";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_3DView_CO>(obj, "CO", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// 3DView_P 
/// </summary>
internal partial class APM_3DView_P : APM_3DView_P__Base
{
}


internal partial class APM_3DView_P__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DView_P";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_3DView_P>(obj, "P", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Projection, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DView_O 
/// </summary>
internal partial class APM_3DView_O : APM_3DView_O__Base
{
}


internal partial class APM_3DView_O__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DView_O";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_3DView_O>(obj, "O", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // special case is an fn:IsMeaningful, not pertinent to validation
        // no value restrictions
        ctx.Run<APM_XObjectFormType1, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// 3DView_BG 
/// </summary>
internal partial class APM_3DView_BG : APM_3DView_BG__Base
{
}


internal partial class APM_3DView_BG__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DView_BG";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_3DView_BG>(obj, "BG", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_3DBackground, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DView_RM 
/// </summary>
internal partial class APM_3DView_RM : APM_3DView_RM__Base
{
}


internal partial class APM_3DView_RM__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DView_RM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_3DView_RM>(obj, "RM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_3DRenderMode, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DView_LS 
/// </summary>
internal partial class APM_3DView_LS : APM_3DView_LS__Base
{
}


internal partial class APM_3DView_LS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DView_LS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_3DView_LS>(obj, "LS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_3DLightingScheme, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DView_SA 
/// </summary>
internal partial class APM_3DView_SA : APM_3DView_SA__Base
{
}


internal partial class APM_3DView_SA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DView_SA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_3DView_SA>(obj, "SA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf3DCrossSection, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DView_NA 
/// </summary>
internal partial class APM_3DView_NA : APM_3DView_NA__Base
{
}


internal partial class APM_3DView_NA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DView_NA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_3DView_NA>(obj, "NA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf3DNode, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DView_NR 
/// </summary>
internal partial class APM_3DView_NR : APM_3DView_NR__Base
{
}


internal partial class APM_3DView_NR__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DView_NR";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_3DView_NR>(obj, "NR", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

