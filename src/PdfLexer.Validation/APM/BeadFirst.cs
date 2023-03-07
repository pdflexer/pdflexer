// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_BeadFirst : APM_BeadFirst__Base
{
}

internal partial class APM_BeadFirst__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "BeadFirst";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_BeadFirst_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_BeadFirst_T, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_BeadFirst_N, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_BeadFirst_V, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_BeadFirst_P, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_BeadFirst_R, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.1m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_11.Contains(x)))
                {
                    ctx.Fail<APM_BeadFirst>($"Unknown field {extra} for version 1.1");
                }
                break;
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_BeadFirst>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_BeadFirst>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_BeadFirst>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_BeadFirst>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_BeadFirst>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_BeadFirst>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_BeadFirst>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_BeadFirst>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_BeadFirst>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_BeadFirst_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_11 { get; } = new HashSet<string> 
    {
        "Type", "T", "N", "V", "P", "R"
    };
    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "T", "N", "V", "P", "R"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "T", "N", "V", "P", "R"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "T", "N", "V", "P", "R"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "T", "N", "V", "P", "R"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "T", "N", "V", "P", "R"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "T", "N", "V", "P", "R"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "T", "N", "V", "P", "R"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "T", "N", "V", "P", "R"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "T", "N", "V", "P", "R"
    };
    


}

/// <summary>
/// BeadFirst_Type Table 163
/// </summary>
internal partial class APM_BeadFirst_Type : APM_BeadFirst_Type__Base
{
}


internal partial class APM_BeadFirst_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "BeadFirst_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_BeadFirst_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Bead)) 
        {
            ctx.Fail<APM_BeadFirst_Type>($"Invalid value {val}, allowed are: [Bead]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// BeadFirst_T 
/// </summary>
internal partial class APM_BeadFirst_T : APM_BeadFirst_T__Base
{
}


internal partial class APM_BeadFirst_T__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "BeadFirst_T";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_BeadFirst_T>(obj, "T", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Thread, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// BeadFirst_N 
/// </summary>
internal partial class APM_BeadFirst_N : APM_BeadFirst_N__Base
{
}


internal partial class APM_BeadFirst_N__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "BeadFirst_N";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_BeadFirst_N>(obj, "N", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_BeadFirst.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_BeadFirst, PdfDictionary>(stack, val, obj);
        } else if (APM_Bead.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_Bead, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_BeadFirst_N>("N did not match any allowable types: '[BeadFirst,Bead]'");
        }
        
    }


}

/// <summary>
/// BeadFirst_V 
/// </summary>
internal partial class APM_BeadFirst_V : APM_BeadFirst_V__Base
{
}


internal partial class APM_BeadFirst_V__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "BeadFirst_V";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_BeadFirst_V>(obj, "V", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_BeadFirst.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_BeadFirst, PdfDictionary>(stack, val, obj);
        } else if (APM_Bead.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_Bead, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_BeadFirst_V>("V did not match any allowable types: '[BeadFirst,Bead]'");
        }
        
    }


}

/// <summary>
/// BeadFirst_P 
/// </summary>
internal partial class APM_BeadFirst_P : APM_BeadFirst_P__Base
{
}


internal partial class APM_BeadFirst_P__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "BeadFirst_P";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_BeadFirst_P>(obj, "P", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_PageObject, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// BeadFirst_R 
/// </summary>
internal partial class APM_BeadFirst_R : APM_BeadFirst_R__Base
{
}


internal partial class APM_BeadFirst_R__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "BeadFirst_R";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_BeadFirst_R>(obj, "R", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}
