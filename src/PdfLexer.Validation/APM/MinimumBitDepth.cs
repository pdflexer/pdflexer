// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_MinimumBitDepth : APM_MinimumBitDepth__Base
{
}

internal partial class APM_MinimumBitDepth__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "MinimumBitDepth";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_MinimumBitDepth_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MinimumBitDepth_V, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MinimumBitDepth_M, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_MinimumBitDepth>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_MinimumBitDepth>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_MinimumBitDepth>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_MinimumBitDepth>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_MinimumBitDepth>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_MinimumBitDepth>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_MinimumBitDepth_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Type", "V", "M"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Type", "V", "M"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "V", "M"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "V", "M"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "V", "M"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "V", "M"
    };
    


}

/// <summary>
/// MinimumBitDepth_Type Table 280
/// </summary>
internal partial class APM_MinimumBitDepth_Type : APM_MinimumBitDepth_Type__Base
{
}


internal partial class APM_MinimumBitDepth_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MinimumBitDepth_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_MinimumBitDepth_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.MinBitDepth)) 
        {
            ctx.Fail<APM_MinimumBitDepth_Type>($"Invalid value {val}, allowed are: [MinBitDepth]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// MinimumBitDepth_V 
/// </summary>
internal partial class APM_MinimumBitDepth_V : APM_MinimumBitDepth_V__Base
{
}


internal partial class APM_MinimumBitDepth_V__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MinimumBitDepth_V";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_MinimumBitDepth_V>(obj, "V", IndirectRequirement.Either);
        if (val == null) { return; }
        var V = obj.Get("V");
        if (!(gt(V,0))) 
        {
            ctx.Fail<APM_MinimumBitDepth_V>($"Value failed special case check: fn:Eval(@V>0)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// MinimumBitDepth_M Table 304
/// </summary>
internal partial class APM_MinimumBitDepth_M : APM_MinimumBitDepth_M__Base
{
}


internal partial class APM_MinimumBitDepth_M__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MinimumBitDepth_M";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_MinimumBitDepth_M>(obj, "M", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 0m || val == 1m || val == 2m || val == 3m || val == 4m || val == 5m || val == 6m)) 
        {
            ctx.Fail<APM_MinimumBitDepth_M>($"Invalid value {val}, allowed are: [0,1,2,3,4,5,6]");
        }
        // no linked objects
        
    }


}

