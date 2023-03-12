// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_TimeStampDict : APM_TimeStampDict__Base
{
}

internal partial class APM_TimeStampDict__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "TimeStampDict";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_TimeStampDict_URL, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_TimeStampDict_Ff, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_TimeStampDict>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_TimeStampDict>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_TimeStampDict>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_TimeStampDict>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_TimeStampDict>($"Unknown field {extra} for version 2.0");
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

    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "URL", "Ff"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "URL", "Ff"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "URL", "Ff"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "URL", "Ff"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "URL", "Ff"
    };
    


}

/// <summary>
/// TimeStampDict_URL Table 237, TimeStamp cell
/// </summary>
internal partial class APM_TimeStampDict_URL : APM_TimeStampDict_URL__Base
{
}


internal partial class APM_TimeStampDict_URL__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "TimeStampDict_URL";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfString, APM_TimeStampDict_URL>(obj, "URL", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// TimeStampDict_Ff 
/// </summary>
internal partial class APM_TimeStampDict_Ff : APM_TimeStampDict_Ff__Base
{
}


internal partial class APM_TimeStampDict_Ff__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "TimeStampDict_Ff";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_TimeStampDict_Ff>(obj, "Ff", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 0 || val == 1)) 
        {
            ctx.Fail<APM_TimeStampDict_Ff>($"Invalid value {val}, allowed are: [0,1]");
        }
        // no linked objects
        
    }


}

