// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_MediaScreenParameters : APM_MediaScreenParameters__Base
{
}

internal partial class APM_MediaScreenParameters__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "MediaScreenParameters";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_MediaScreenParameters_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MediaScreenParameters_MH, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MediaScreenParameters_BE, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_MediaScreenParameters>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_MediaScreenParameters>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_MediaScreenParameters>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_MediaScreenParameters>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_MediaScreenParameters>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_MediaScreenParameters>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_MediaScreenParameters_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Type", "MH", "BE"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Type", "MH", "BE"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "MH", "BE"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "MH", "BE"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "MH", "BE"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "MH", "BE"
    };
    


}

/// <summary>
/// MediaScreenParameters_Type Table 293
/// </summary>
internal partial class APM_MediaScreenParameters_Type : APM_MediaScreenParameters_Type__Base
{
}


internal partial class APM_MediaScreenParameters_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MediaScreenParameters_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_MediaScreenParameters_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.MediaScreenParams)) 
        {
            ctx.Fail<APM_MediaScreenParameters_Type>($"Invalid value {val}, allowed are: [MediaScreenParams]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// MediaScreenParameters_MH 
/// </summary>
internal partial class APM_MediaScreenParameters_MH : APM_MediaScreenParameters_MH__Base
{
}


internal partial class APM_MediaScreenParameters_MH__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MediaScreenParameters_MH";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_MediaScreenParameters_MH>(obj, "MH", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_MediaScreenParametersMHBE, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// MediaScreenParameters_BE 
/// </summary>
internal partial class APM_MediaScreenParameters_BE : APM_MediaScreenParameters_BE__Base
{
}


internal partial class APM_MediaScreenParameters_BE__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MediaScreenParameters_BE";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_MediaScreenParameters_BE>(obj, "BE", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_MediaScreenParametersMHBE, PdfDictionary>(stack, val, obj);
        
    }


}

