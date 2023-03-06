// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_RichMediaWidth : APM_RichMediaWidth__Base
{
}

internal partial class APM_RichMediaWidth__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "RichMediaWidth";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_RichMediaWidth_Default, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaWidth_Max, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaWidth_Min, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaWidth>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaWidth>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaWidth>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaWidth>($"Unknown field {extra} for version 2.0");
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

    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Default", "Max", "Min"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Default", "Max", "Min"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Default", "Max", "Min"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Default", "Max", "Min"
    };
    


}

/// <summary>
/// RichMediaWidth_Default Table 339
/// </summary>
internal partial class APM_RichMediaWidth_Default : APM_RichMediaWidth_Default__Base
{
}


internal partial class APM_RichMediaWidth_Default__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaWidth_Default";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_RichMediaWidth_Default>(obj, "Default", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var Default = obj.Get("Default");
        if (!(gt(Default,0))) 
        {
            ctx.Fail<APM_RichMediaWidth_Default>($"Invalid value {val}, allowed are: [fn:Eval(@Default>0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// RichMediaWidth_Max 
/// </summary>
internal partial class APM_RichMediaWidth_Max : APM_RichMediaWidth_Max__Base
{
}


internal partial class APM_RichMediaWidth_Max__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaWidth_Max";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_RichMediaWidth_Max>(obj, "Max", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var Max = obj.Get("Max");
        if (!(gt(Max,0))) 
        {
            ctx.Fail<APM_RichMediaWidth_Max>($"Invalid value {val}, allowed are: [fn:Eval(@Max>0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// RichMediaWidth_Min 
/// </summary>
internal partial class APM_RichMediaWidth_Min : APM_RichMediaWidth_Min__Base
{
}


internal partial class APM_RichMediaWidth_Min__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaWidth_Min";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_RichMediaWidth_Min>(obj, "Min", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var Min = obj.Get("Min");
        if (!(gt(Min,0))) 
        {
            ctx.Fail<APM_RichMediaWidth_Min>($"Invalid value {val}, allowed are: [fn:Eval(@Min>0)]");
        }
        // no linked objects
        
    }


}

