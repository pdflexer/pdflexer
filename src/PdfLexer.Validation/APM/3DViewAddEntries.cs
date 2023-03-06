// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_3DViewAddEntries : APM_3DViewAddEntries__Base
{
}

internal partial class APM_3DViewAddEntries__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "3DViewAddEntries";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_3DViewAddEntries_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DViewAddEntries_XN, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DViewAddEntries_IN, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DViewAddEntries_MS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DViewAddEntries_MA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DViewAddEntries_C2W, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DViewAddEntries_U3DPath, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DViewAddEntries_CO, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DViewAddEntries_P, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DViewAddEntries_O, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DViewAddEntries_BG, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DViewAddEntries_RM, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DViewAddEntries_LS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DViewAddEntries_SA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DViewAddEntries_NA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DViewAddEntries_NR, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DViewAddEntries_Snapshot, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DViewAddEntries_Params, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_3DViewAddEntries>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_3DViewAddEntries>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_3DViewAddEntries>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_3DViewAddEntries>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_3DViewAddEntries_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Snapshot", "Params"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Snapshot", "Params"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Snapshot", "Params"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "XN", "IN", "MS", "MA", "C2W", "U3DPath", "CO", "P", "O", "BG", "RM", "LS", "SA", "NA", "NR", "Snapshot", "Params"
    };
    


}

/// <summary>
/// 3DViewAddEntries_Type Table 315 and Table 344 - for rich media only
/// </summary>
internal partial class APM_3DViewAddEntries_Type : APM_3DViewAddEntries_Type__Base
{
}


internal partial class APM_3DViewAddEntries_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DViewAddEntries_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_3DViewAddEntries_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "3DView")) 
        {
            ctx.Fail<APM_3DViewAddEntries_Type>($"Invalid value {val}, allowed are: [3DView]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// 3DViewAddEntries_XN 
/// </summary>
internal partial class APM_3DViewAddEntries_XN : APM_3DViewAddEntries_XN__Base
{
}


internal partial class APM_3DViewAddEntries_XN__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DViewAddEntries_XN";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfString, APM_3DViewAddEntries_XN>(obj, "XN", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// 3DViewAddEntries_IN 
/// </summary>
internal partial class APM_3DViewAddEntries_IN : APM_3DViewAddEntries_IN__Base
{
}


internal partial class APM_3DViewAddEntries_IN__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DViewAddEntries_IN";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_3DViewAddEntries_IN>(obj, "IN", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// 3DViewAddEntries_MS 
/// </summary>
internal partial class APM_3DViewAddEntries_MS : APM_3DViewAddEntries_MS__Base
{
}


internal partial class APM_3DViewAddEntries_MS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DViewAddEntries_MS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_3DViewAddEntries_MS>(obj, "MS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// 3DViewAddEntries_MA 
/// </summary>
internal partial class APM_3DViewAddEntries_MA : APM_3DViewAddEntries_MA__Base
{
}


internal partial class APM_3DViewAddEntries_MA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DViewAddEntries_MA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_3DViewAddEntries_MA>(obj, "MA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf3DMeasure, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DViewAddEntries_C2W 
/// </summary>
internal partial class APM_3DViewAddEntries_C2W : APM_3DViewAddEntries_C2W__Base
{
}


internal partial class APM_3DViewAddEntries_C2W__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DViewAddEntries_C2W";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var MS = obj.Get("MS");
        var val = ctx.GetOptional<PdfArray, APM_3DViewAddEntries_C2W>(obj, "C2W", IndirectRequirement.Either);
        if ((eq(MS,"M")) && val == null) {
            ctx.Fail<APM_3DViewAddEntries_C2W>("C2W is required when 'fn:IsRequired(@MS==M)"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf3DTransMatrix, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DViewAddEntries_U3DPath 
/// </summary>
internal partial class APM_3DViewAddEntries_U3DPath : APM_3DViewAddEntries_U3DPath__Base
{
}


internal partial class APM_3DViewAddEntries_U3DPath__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DViewAddEntries_U3DPath";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_3DViewAddEntries_U3DPath>(obj, "U3DPath", IndirectRequirement.Either);
        
        var MS = obj.Get("MS");
        if ((eq(MS,"U3D")) && utval == null) {
            ctx.Fail<APM_3DViewAddEntries_U3DPath>("U3DPath is required"); return;
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
                ctx.Fail<APM_3DViewAddEntries_U3DPath>("U3DPath is required to one of 'array;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// 3DViewAddEntries_CO 
/// </summary>
internal partial class APM_3DViewAddEntries_CO : APM_3DViewAddEntries_CO__Base
{
}


internal partial class APM_3DViewAddEntries_CO__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DViewAddEntries_CO";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_3DViewAddEntries_CO>(obj, "CO", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// 3DViewAddEntries_P 
/// </summary>
internal partial class APM_3DViewAddEntries_P : APM_3DViewAddEntries_P__Base
{
}


internal partial class APM_3DViewAddEntries_P__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DViewAddEntries_P";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_3DViewAddEntries_P>(obj, "P", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Projection, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DViewAddEntries_O 
/// </summary>
internal partial class APM_3DViewAddEntries_O : APM_3DViewAddEntries_O__Base
{
}


internal partial class APM_3DViewAddEntries_O__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DViewAddEntries_O";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_3DViewAddEntries_O>(obj, "O", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_XObjectFormType1, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// 3DViewAddEntries_BG 
/// </summary>
internal partial class APM_3DViewAddEntries_BG : APM_3DViewAddEntries_BG__Base
{
}


internal partial class APM_3DViewAddEntries_BG__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DViewAddEntries_BG";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_3DViewAddEntries_BG>(obj, "BG", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_3DBackground, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DViewAddEntries_RM 
/// </summary>
internal partial class APM_3DViewAddEntries_RM : APM_3DViewAddEntries_RM__Base
{
}


internal partial class APM_3DViewAddEntries_RM__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DViewAddEntries_RM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_3DViewAddEntries_RM>(obj, "RM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_3DRenderMode, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DViewAddEntries_LS 
/// </summary>
internal partial class APM_3DViewAddEntries_LS : APM_3DViewAddEntries_LS__Base
{
}


internal partial class APM_3DViewAddEntries_LS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DViewAddEntries_LS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_3DViewAddEntries_LS>(obj, "LS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_3DLightingScheme, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DViewAddEntries_SA 
/// </summary>
internal partial class APM_3DViewAddEntries_SA : APM_3DViewAddEntries_SA__Base
{
}


internal partial class APM_3DViewAddEntries_SA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DViewAddEntries_SA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_3DViewAddEntries_SA>(obj, "SA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf3DCrossSection, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DViewAddEntries_NA 
/// </summary>
internal partial class APM_3DViewAddEntries_NA : APM_3DViewAddEntries_NA__Base
{
}


internal partial class APM_3DViewAddEntries_NA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DViewAddEntries_NA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_3DViewAddEntries_NA>(obj, "NA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf3DNode, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DViewAddEntries_NR 
/// </summary>
internal partial class APM_3DViewAddEntries_NR : APM_3DViewAddEntries_NR__Base
{
}


internal partial class APM_3DViewAddEntries_NR__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DViewAddEntries_NR";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_3DViewAddEntries_NR>(obj, "NR", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// 3DViewAddEntries_Snapshot 
/// </summary>
internal partial class APM_3DViewAddEntries_Snapshot : APM_3DViewAddEntries_Snapshot__Base
{
}


internal partial class APM_3DViewAddEntries_Snapshot__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DViewAddEntries_Snapshot";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_3DViewAddEntries_Snapshot>(obj, "Snapshot", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_XObjectImage, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// 3DViewAddEntries_Params 
/// </summary>
internal partial class APM_3DViewAddEntries_Params : APM_3DViewAddEntries_Params__Base
{
}


internal partial class APM_3DViewAddEntries_Params__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DViewAddEntries_Params";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_3DViewAddEntries_Params>(obj, "Params", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfViewParams, PdfArray>(stack, val, obj);
        
    }


}

