// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_FilterFlateDecode : APM_FilterFlateDecode__Base
{
}

internal partial class APM_FilterFlateDecode__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "FilterFlateDecode";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_FilterFlateDecode_Predictor, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FilterFlateDecode_Colors, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FilterFlateDecode_BitsPerComponent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FilterFlateDecode_Columns, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_FilterFlateDecode>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_FilterFlateDecode>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_FilterFlateDecode>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_FilterFlateDecode>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_FilterFlateDecode>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_FilterFlateDecode>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_FilterFlateDecode>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_FilterFlateDecode>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_FilterFlateDecode>($"Unknown field {extra} for version 2.0");
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

    public static List<string> AllowedFields_12 { get; } = new List<string> 
    {
        "Predictor", "Colors", "BitsPerComponent", "Columns"
    };
    public static List<string> AllowedFields_13 { get; } = new List<string> 
    {
        "Predictor", "Colors", "BitsPerComponent", "Columns"
    };
    public static List<string> AllowedFields_14 { get; } = new List<string> 
    {
        "Predictor", "Colors", "BitsPerComponent", "Columns"
    };
    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Predictor", "Colors", "BitsPerComponent", "Columns"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Predictor", "Colors", "BitsPerComponent", "Columns"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Predictor", "Colors", "BitsPerComponent", "Columns"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Predictor", "Colors", "BitsPerComponent", "Columns"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Predictor", "Colors", "BitsPerComponent", "Columns"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Predictor", "Colors", "BitsPerComponent", "Columns"
    };
    


}

/// <summary>
/// FilterFlateDecode_Predictor Table 8 and Table 10
/// </summary>
internal partial class APM_FilterFlateDecode_Predictor : APM_FilterFlateDecode_Predictor__Base
{
}


internal partial class APM_FilterFlateDecode_Predictor__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FilterFlateDecode_Predictor";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FilterFlateDecode_Predictor>(obj, "Predictor", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 1m || val == 2m || val == 10m || val == 11m || val == 12m || val == 13m || val == 14m || val == 15m)) 
        {
            ctx.Fail<APM_FilterFlateDecode_Predictor>($"Invalid value {val}, allowed are: [1,2,10,11,12,13,14,15]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FilterFlateDecode_Colors 
/// </summary>
internal partial class APM_FilterFlateDecode_Colors : APM_FilterFlateDecode_Colors__Base
{
}


internal partial class APM_FilterFlateDecode_Colors__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FilterFlateDecode_Colors";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FilterFlateDecode_Colors>(obj, "Colors", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:Ignore, not pertinent to validation
        
        var Colors = obj.Get("Colors");
        if (!((gte(Colors,1)&&(ctx.Version >= 1.3m || lte(Colors,4))))) 
        {
            ctx.Fail<APM_FilterFlateDecode_Colors>($"Invalid value {val}, allowed are: [fn:Eval((@Colors>=1) && fn:BeforeVersion(1.3,fn:Eval(@Colors<=4)))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FilterFlateDecode_BitsPerComponent 
/// </summary>
internal partial class APM_FilterFlateDecode_BitsPerComponent : APM_FilterFlateDecode_BitsPerComponent__Base
{
}


internal partial class APM_FilterFlateDecode_BitsPerComponent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FilterFlateDecode_BitsPerComponent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FilterFlateDecode_BitsPerComponent>(obj, "BitsPerComponent", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:Ignore, not pertinent to validation
        
        
        if (!(val == 1m || val == 2m || val == 4m || val == 8m || (ctx.Version >= 1.5m && val == 16m))) 
        {
            ctx.Fail<APM_FilterFlateDecode_BitsPerComponent>($"Invalid value {val}, allowed are: [1,2,4,8,fn:SinceVersion(1.5,16)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FilterFlateDecode_Columns 
/// </summary>
internal partial class APM_FilterFlateDecode_Columns : APM_FilterFlateDecode_Columns__Base
{
}


internal partial class APM_FilterFlateDecode_Columns__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FilterFlateDecode_Columns";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FilterFlateDecode_Columns>(obj, "Columns", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:Ignore, not pertinent to validation
        // no value restrictions
        // no linked objects
        
    }


}
