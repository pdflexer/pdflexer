// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_SignatureBuildPropDict : APM_SignatureBuildPropDict__Base
{
}

internal partial class APM_SignatureBuildPropDict__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "SignatureBuildPropDict";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_SignatureBuildPropDict_Filter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SignatureBuildPropDict_PubSec, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SignatureBuildPropDict_App, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SignatureBuildPropDict_SigQ, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_SignatureBuildPropDict>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_SignatureBuildPropDict>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_SignatureBuildPropDict>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_SignatureBuildPropDict>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_SignatureBuildPropDict>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_SignatureBuildPropDict>($"Unknown field {extra} for version 2.0");
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
        "Filter", "PubSec", "App", "SigQ"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Filter", "PubSec", "App", "SigQ"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Filter", "PubSec", "App", "SigQ"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Filter", "PubSec", "App", "SigQ"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Filter", "PubSec", "App", "SigQ"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Filter", "PubSec", "App", "SigQ"
    };
    


}

/// <summary>
/// SignatureBuildPropDict_Filter Adobe "Digital Signature Build Dictionary Specification" Table 1
/// </summary>
internal partial class APM_SignatureBuildPropDict_Filter : APM_SignatureBuildPropDict_Filter__Base
{
}


internal partial class APM_SignatureBuildPropDict_Filter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SignatureBuildPropDict_Filter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_SignatureBuildPropDict_Filter>(obj, "Filter", IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        
        // no special cases
        // no value restrictions
        ctx.Run<APM_SignatureBuildDataDict, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// SignatureBuildPropDict_PubSec 
/// </summary>
internal partial class APM_SignatureBuildPropDict_PubSec : APM_SignatureBuildPropDict_PubSec__Base
{
}


internal partial class APM_SignatureBuildPropDict_PubSec__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SignatureBuildPropDict_PubSec";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_SignatureBuildPropDict_PubSec>(obj, "PubSec", IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        
        // no special cases
        // no value restrictions
        ctx.Run<APM_SignatureBuildDataDict, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// SignatureBuildPropDict_App 
/// </summary>
internal partial class APM_SignatureBuildPropDict_App : APM_SignatureBuildPropDict_App__Base
{
}


internal partial class APM_SignatureBuildPropDict_App__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SignatureBuildPropDict_App";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_SignatureBuildPropDict_App>(obj, "App", IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        
        // no special cases
        // no value restrictions
        ctx.Run<APM_SignatureBuildDataAppDict, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// SignatureBuildPropDict_SigQ 
/// </summary>
internal partial class APM_SignatureBuildPropDict_SigQ : APM_SignatureBuildPropDict_SigQ__Base
{
}


internal partial class APM_SignatureBuildPropDict_SigQ__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SignatureBuildPropDict_SigQ";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_SignatureBuildPropDict_SigQ>(obj, "SigQ", IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        
        // no special cases
        // no value restrictions
        ctx.Run<APM_SignatureBuildDataSigQDict, PdfDictionary>(stack, val, obj);
        
    }


}

