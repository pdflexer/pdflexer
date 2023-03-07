// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_RichMediaWindow : APM_RichMediaWindow__Base
{
}

internal partial class APM_RichMediaWindow__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "RichMediaWindow";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_RichMediaWindow_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaWindow_Width, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaWindow_Height, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaWindow_Position, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaWindow>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaWindow>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaWindow>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaWindow>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_RichMediaWindow_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "Width", "Height", "Position"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "Width", "Height", "Position"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "Width", "Height", "Position"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "Width", "Height", "Position"
    };
    


}

/// <summary>
/// RichMediaWindow_Type Table 339
/// </summary>
internal partial class APM_RichMediaWindow_Type : APM_RichMediaWindow_Type__Base
{
}


internal partial class APM_RichMediaWindow_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaWindow_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_RichMediaWindow_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.RichMediaWindow)) 
        {
            ctx.Fail<APM_RichMediaWindow_Type>($"Invalid value {val}, allowed are: [RichMediaWindow]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// RichMediaWindow_Width 
/// </summary>
internal partial class APM_RichMediaWindow_Width : APM_RichMediaWindow_Width__Base
{
}


internal partial class APM_RichMediaWindow_Width__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaWindow_Width";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_RichMediaWindow_Width>(obj, "Width", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_RichMediaWidth, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// RichMediaWindow_Height 
/// </summary>
internal partial class APM_RichMediaWindow_Height : APM_RichMediaWindow_Height__Base
{
}


internal partial class APM_RichMediaWindow_Height__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaWindow_Height";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_RichMediaWindow_Height>(obj, "Height", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_RichMediaHeight, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// RichMediaWindow_Position 
/// </summary>
internal partial class APM_RichMediaWindow_Position : APM_RichMediaWindow_Position__Base
{
}


internal partial class APM_RichMediaWindow_Position__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaWindow_Position";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_RichMediaWindow_Position>(obj, "Position", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_RichMediaPosition, PdfDictionary>(stack, val, obj);
        
    }


}
