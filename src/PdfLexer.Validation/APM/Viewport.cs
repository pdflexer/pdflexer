// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_Viewport : APM_Viewport__Base
{
}

internal partial class APM_Viewport__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "Viewport";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_Viewport_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Viewport_BBox, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Viewport_Name, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Viewport_Measure, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Viewport_PtData, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_Viewport>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_Viewport>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_Viewport>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_Viewport>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_Viewport>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_Viewport_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Type", "BBox", "Name", "Measure"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "BBox", "Name", "Measure"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "BBox", "Name", "Measure"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "BBox", "Name", "Measure"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "BBox", "Name", "Measure", "PtData"
    };
    


}

/// <summary>
/// Viewport_Type Table 265
/// </summary>
internal partial class APM_Viewport_Type : APM_Viewport_Type__Base
{
}


internal partial class APM_Viewport_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Viewport_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_Viewport_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Viewport)) 
        {
            ctx.Fail<APM_Viewport_Type>($"Invalid value {val}, allowed are: [Viewport]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// Viewport_BBox 
/// </summary>
internal partial class APM_Viewport_BBox : APM_Viewport_BBox__Base
{
}


internal partial class APM_Viewport_BBox__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Viewport_BBox";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_Viewport_BBox>(obj, "BBox", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Viewport_Name 
/// </summary>
internal partial class APM_Viewport_Name : APM_Viewport_Name__Base
{
}


internal partial class APM_Viewport_Name__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Viewport_Name";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_Viewport_Name>(obj, "Name", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Viewport_Measure 
/// </summary>
internal partial class APM_Viewport_Measure : APM_Viewport_Measure__Base
{
}


internal partial class APM_Viewport_Measure__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Viewport_Measure";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Viewport_Measure>(obj, "Measure", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_MeasureRL.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_MeasureRL, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 2.0m && APM_MeasureGEO.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_MeasureGEO, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_Viewport_Measure>("Measure did not match any allowable types: '[MeasureRL,fn:SinceVersion(2.0,MeasureGEO)]'");
        }
        
    }


}

/// <summary>
/// Viewport_PtData 
/// </summary>
internal partial class APM_Viewport_PtData : APM_Viewport_PtData__Base
{
}


internal partial class APM_Viewport_PtData__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Viewport_PtData";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Viewport_PtData>(obj, "PtData", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_PointData, PdfDictionary>(stack, val, obj);
        
    }


}
