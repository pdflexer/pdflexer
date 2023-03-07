// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_ProjectedCoordinateSystem : APM_ProjectedCoordinateSystem__Base
{
}

internal partial class APM_ProjectedCoordinateSystem__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "ProjectedCoordinateSystem";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_ProjectedCoordinateSystem_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ProjectedCoordinateSystem_EPSG, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ProjectedCoordinateSystem_WKT, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_ProjectedCoordinateSystem>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_ProjectedCoordinateSystem>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_ProjectedCoordinateSystem>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_ProjectedCoordinateSystem>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_ProjectedCoordinateSystem_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "EPSG", "WKT"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "EPSG", "WKT"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "EPSG", "WKT"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "EPSG", "WKT"
    };
    


}

/// <summary>
/// ProjectedCoordinateSystem_Type Table 271
/// </summary>
internal partial class APM_ProjectedCoordinateSystem_Type : APM_ProjectedCoordinateSystem_Type__Base
{
}


internal partial class APM_ProjectedCoordinateSystem_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ProjectedCoordinateSystem_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_ProjectedCoordinateSystem_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.PROJCS)) 
        {
            ctx.Fail<APM_ProjectedCoordinateSystem_Type>($"Invalid value {val}, allowed are: [PROJCS]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ProjectedCoordinateSystem_EPSG 
/// </summary>
internal partial class APM_ProjectedCoordinateSystem_EPSG : APM_ProjectedCoordinateSystem_EPSG__Base
{
}


internal partial class APM_ProjectedCoordinateSystem_EPSG__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ProjectedCoordinateSystem_EPSG";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfIntNumber, APM_ProjectedCoordinateSystem_EPSG>(obj, "EPSG", IndirectRequirement.Either);
        if ((!obj.ContainsKey(PdfName.WKT)) && val == null) {
            ctx.Fail<APM_ProjectedCoordinateSystem_EPSG>("EPSG is required when 'fn:IsRequired(fn:Not(fn:IsPresent(WKT)))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ProjectedCoordinateSystem_WKT 
/// </summary>
internal partial class APM_ProjectedCoordinateSystem_WKT : APM_ProjectedCoordinateSystem_WKT__Base
{
}


internal partial class APM_ProjectedCoordinateSystem_WKT__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ProjectedCoordinateSystem_WKT";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfString, APM_ProjectedCoordinateSystem_WKT>(obj, "WKT", IndirectRequirement.Either);
        if ((!obj.ContainsKey(val)) && val == null) {
            ctx.Fail<APM_ProjectedCoordinateSystem_WKT>("WKT is required when 'fn:IsRequired(fn:Not(fn:IsPresent(EPSG)))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}
