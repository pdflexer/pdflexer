// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_OptContentView : APM_OptContentView__Base
{
}

internal partial class APM_OptContentView__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "OptContentView";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_OptContentView_ViewState, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_OptContentView>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_OptContentView>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_OptContentView>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_OptContentView>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_OptContentView>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_OptContentView>($"Unknown field {extra} for version 2.0");
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
        "ViewState"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "ViewState"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "ViewState"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "ViewState"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "ViewState"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "ViewState"
    };
    


}

/// <summary>
/// OptContentView_ViewState Table 100, View cell
/// </summary>
internal partial class APM_OptContentView_ViewState : APM_OptContentView_ViewState__Base
{
}


internal partial class APM_OptContentView_ViewState__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OptContentView_ViewState";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_OptContentView_ViewState>(obj, "ViewState", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.ON || val == PdfName.OFF)) 
        {
            ctx.Fail<APM_OptContentView_ViewState>($"Invalid value {val}, allowed are: [ON,OFF]");
        }
        // no linked objects
        
    }


}
