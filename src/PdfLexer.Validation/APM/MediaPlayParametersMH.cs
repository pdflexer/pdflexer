// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_MediaPlayParametersMH : APM_MediaPlayParametersMH__Base
{
}

internal partial class APM_MediaPlayParametersMH__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "MediaPlayParametersMH";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_MediaPlayParametersMH_V, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MediaPlayParametersMH_C, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MediaPlayParametersMH_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MediaPlayParametersMH_D, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MediaPlayParametersMH_A, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MediaPlayParametersMH_RC, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_MediaPlayParametersMH>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_MediaPlayParametersMH>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_MediaPlayParametersMH>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_MediaPlayParametersMH>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_MediaPlayParametersMH>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_MediaPlayParametersMH>($"Unknown field {extra} for version 2.0");
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

    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "V", "C", "F", "D", "A", "RC"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "V", "C", "F", "D", "A", "RC"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "V", "C", "F", "D", "A", "RC"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "V", "C", "F", "D", "A", "RC"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "V", "C", "F", "D", "A", "RC"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "V", "C", "F", "D", "A", "RC"
    };
    


}

/// <summary>
/// MediaPlayParametersMH_V Table 291 - percentage volume
/// </summary>
internal partial class APM_MediaPlayParametersMH_V : APM_MediaPlayParametersMH_V__Base
{
}


internal partial class APM_MediaPlayParametersMH_V__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MediaPlayParametersMH_V";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_MediaPlayParametersMH_V>(obj, "V", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var V = obj.Get("V");
        if (!((gte(V,0)&&lte(V,100)))) 
        {
            ctx.Fail<APM_MediaPlayParametersMH_V>($"Invalid value {val}, allowed are: [fn:Eval((@V>=0) && (@V<=100))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// MediaPlayParametersMH_C 
/// </summary>
internal partial class APM_MediaPlayParametersMH_C : APM_MediaPlayParametersMH_C__Base
{
}


internal partial class APM_MediaPlayParametersMH_C__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MediaPlayParametersMH_C";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_MediaPlayParametersMH_C>(obj, "C", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// MediaPlayParametersMH_F 
/// </summary>
internal partial class APM_MediaPlayParametersMH_F : APM_MediaPlayParametersMH_F__Base
{
}


internal partial class APM_MediaPlayParametersMH_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MediaPlayParametersMH_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_MediaPlayParametersMH_F>(obj, "F", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 0m || val == 1m || val == 2m || val == 3m || val == 4m || val == 5m)) 
        {
            ctx.Fail<APM_MediaPlayParametersMH_F>($"Invalid value {val}, allowed are: [0,1,2,3,4,5]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// MediaPlayParametersMH_D 
/// </summary>
internal partial class APM_MediaPlayParametersMH_D : APM_MediaPlayParametersMH_D__Base
{
}


internal partial class APM_MediaPlayParametersMH_D__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MediaPlayParametersMH_D";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_MediaPlayParametersMH_D>(obj, "D", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_MediaDuration, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// MediaPlayParametersMH_A 
/// </summary>
internal partial class APM_MediaPlayParametersMH_A : APM_MediaPlayParametersMH_A__Base
{
}


internal partial class APM_MediaPlayParametersMH_A__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MediaPlayParametersMH_A";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_MediaPlayParametersMH_A>(obj, "A", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// MediaPlayParametersMH_RC repeat count (zero means forever)
/// </summary>
internal partial class APM_MediaPlayParametersMH_RC : APM_MediaPlayParametersMH_RC__Base
{
}


internal partial class APM_MediaPlayParametersMH_RC__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MediaPlayParametersMH_RC";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_MediaPlayParametersMH_RC>(obj, "RC", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var RC = obj.Get("RC");
        if (!(gte(RC,0.0m))) 
        {
            ctx.Fail<APM_MediaPlayParametersMH_RC>($"Invalid value {val}, allowed are: [fn:Eval(@RC>=0.0)]");
        }
        // no linked objects
        
    }


}
