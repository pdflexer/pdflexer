// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_OptContentZoom : APM_OptContentZoom__Base
{
}

internal partial class APM_OptContentZoom__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "OptContentZoom";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_OptContentZoom_min, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OptContentZoom_max, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_OptContentZoom>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_OptContentZoom>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_OptContentZoom>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_OptContentZoom>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_OptContentZoom>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_OptContentZoom>($"Unknown field {extra} for version 2.0");
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

    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "min", "max"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "min", "max"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "min", "max"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "min", "max"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "min", "max"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "min", "max"
    };
    


}

/// <summary>
/// OptContentZoom_min Table 100, Zoom cell
/// </summary>
internal partial class APM_OptContentZoom_min : APM_OptContentZoom_min__Base
{
}


internal partial class APM_OptContentZoom_min__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OptContentZoom_min";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfNumber, APM_OptContentZoom_min>(obj, "min", IndirectRequirement.Either);
        if ((!obj.ContainsKey(PdfName.max)) && val == null) {
            ctx.Fail<APM_OptContentZoom_min>("min is required when 'fn:IsRequired(fn:Not(fn:IsPresent(max)))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        
        var min = obj.Get("min");
        var max = obj.Get("max");
        if (!((gte(min,0.0m)&&lte(min,max)))) 
        {
            ctx.Fail<APM_OptContentZoom_min>($"Invalid value {val}, allowed are: [fn:Eval((@min>=0.0) && (@min<=@max))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// OptContentZoom_max Default is really infinity
/// </summary>
internal partial class APM_OptContentZoom_max : APM_OptContentZoom_max__Base
{
}


internal partial class APM_OptContentZoom_max__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OptContentZoom_max";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfNumber, APM_OptContentZoom_max>(obj, "max", IndirectRequirement.Either);
        if ((!obj.ContainsKey(val)) && val == null) {
            ctx.Fail<APM_OptContentZoom_max>("max is required when 'fn:IsRequired(fn:Not(fn:IsPresent(min)))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        
        var max = obj.Get("max");
        var min = obj.Get("min");
        if (!((gte(max,0.0m)&&gte(max,min)))) 
        {
            ctx.Fail<APM_OptContentZoom_max>($"Invalid value {val}, allowed are: [fn:Eval((@max>=0.0) && (@max>=@min))]");
        }
        // no linked objects
        
    }


}

