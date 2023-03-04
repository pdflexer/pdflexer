// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_MediaOffsetTime : APM_MediaOffsetTime_Base
{
}

internal partial class APM_MediaOffsetTime_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "MediaOffsetTime";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_MediaOffsetTime_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MediaOffsetTime_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MediaOffsetTime_T, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_MediaOffsetTime>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_MediaOffsetTime>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_MediaOffsetTime>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_MediaOffsetTime>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_MediaOffsetTime>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_MediaOffsetTime>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_MediaOffsetTime_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Type", "S", "T"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Type", "S", "T"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "S", "T"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "S", "T"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "S", "T"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "S", "T"
    };
    


}

/// <summary>
/// MediaOffsetTime_Type Table 296 and Table 297
/// </summary>
internal partial class APM_MediaOffsetTime_Type : APM_MediaOffsetTime_Type_Base
{
}


internal partial class APM_MediaOffsetTime_Type_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MediaOffsetTime_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_MediaOffsetTime_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "MediaOffset")) 
        {
            ctx.Fail<APM_MediaOffsetTime_Type>($"Invalid value {val}, allowed are: [MediaOffset]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// MediaOffsetTime_S 
/// </summary>
internal partial class APM_MediaOffsetTime_S : APM_MediaOffsetTime_S_Base
{
}


internal partial class APM_MediaOffsetTime_S_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MediaOffsetTime_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_MediaOffsetTime_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "T")) 
        {
            ctx.Fail<APM_MediaOffsetTime_S>($"Invalid value {val}, allowed are: [T]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// MediaOffsetTime_T 
/// </summary>
internal partial class APM_MediaOffsetTime_T : APM_MediaOffsetTime_T_Base
{
}


internal partial class APM_MediaOffsetTime_T_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MediaOffsetTime_T";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_MediaOffsetTime_T>(obj, "T", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Timespan, PdfDictionary>(stack, val, obj);
        
    }


}

