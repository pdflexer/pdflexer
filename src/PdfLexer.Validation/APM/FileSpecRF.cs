// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_FileSpecRF : APM_FileSpecRF_Base
{
}

internal partial class APM_FileSpecRF_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "FileSpecRF";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_FileSpecRF_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FileSpecRF_UF, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_FileSpecRF>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_FileSpecRF>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_FileSpecRF>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_FileSpecRF>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_FileSpecRF>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_FileSpecRF>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_FileSpecRF>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_FileSpecRF>($"Unknown field {extra} for version 2.0");
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

    public static List<string> AllowedFields_13 { get; } = new List<string> 
    {
        "F"
    };
    public static List<string> AllowedFields_14 { get; } = new List<string> 
    {
        "F"
    };
    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "F"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "F"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "F", "UF"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "F", "UF"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "F", "UF"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "F", "UF"
    };
    


}

/// <summary>
/// FileSpecRF_F Table 43, RF cell
/// </summary>
internal partial class APM_FileSpecRF_F : APM_FileSpecRF_F_Base
{
}


internal partial class APM_FileSpecRF_F_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FileSpecRF_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_FileSpecRF_F>(obj, "F", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // TODO special case
        // no value restrictions
        ctx.Run<APM_RelatedFilesArray, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// FileSpecRF_UF 
/// </summary>
internal partial class APM_FileSpecRF_UF : APM_FileSpecRF_UF_Base
{
}


internal partial class APM_FileSpecRF_UF_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FileSpecRF_UF";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_FileSpecRF_UF>(obj, "UF", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // TODO special case
        // no value restrictions
        ctx.Run<APM_RelatedFilesArray, PdfArray>(stack, val, obj);
        
    }


}

