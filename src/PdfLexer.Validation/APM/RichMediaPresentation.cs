// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_RichMediaPresentation : APM_RichMediaPresentation__Base
{
}

internal partial class APM_RichMediaPresentation__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "RichMediaPresentation";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_RichMediaPresentation_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaPresentation_Style, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaPresentation_Window, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaPresentation_Transparent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaPresentation_NavigationPane, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaPresentation_Toolbar, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaPresentation_PassContextClick, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaPresentation>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaPresentation>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaPresentation>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaPresentation>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_RichMediaPresentation_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "Style", "Window", "Transparent", "NavigationPane", "Toolbar", "PassContextClick"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "Style", "Window", "Transparent", "NavigationPane", "Toolbar", "PassContextClick"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "Style", "Window", "Transparent", "NavigationPane", "Toolbar", "PassContextClick"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "Style", "Window", "Transparent", "NavigationPane", "Toolbar", "PassContextClick"
    };
    


}

/// <summary>
/// RichMediaPresentation_Type Table 338
/// </summary>
internal partial class APM_RichMediaPresentation_Type : APM_RichMediaPresentation_Type__Base
{
}


internal partial class APM_RichMediaPresentation_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaPresentation_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_RichMediaPresentation_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "RichMediaPresentation")) 
        {
            ctx.Fail<APM_RichMediaPresentation_Type>($"Invalid value {val}, allowed are: [RichMediaPresentation]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// RichMediaPresentation_Style 
/// </summary>
internal partial class APM_RichMediaPresentation_Style : APM_RichMediaPresentation_Style__Base
{
}


internal partial class APM_RichMediaPresentation_Style__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaPresentation_Style";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_RichMediaPresentation_Style>(obj, "Style", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "Embedded" || val == "Windowed")) 
        {
            ctx.Fail<APM_RichMediaPresentation_Style>($"Invalid value {val}, allowed are: [Embedded,Windowed]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// RichMediaPresentation_Window 
/// </summary>
internal partial class APM_RichMediaPresentation_Window : APM_RichMediaPresentation_Window__Base
{
}


internal partial class APM_RichMediaPresentation_Window__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaPresentation_Window";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_RichMediaPresentation_Window>(obj, "Window", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_RichMediaWindow, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// RichMediaPresentation_Transparent 
/// </summary>
internal partial class APM_RichMediaPresentation_Transparent : APM_RichMediaPresentation_Transparent__Base
{
}


internal partial class APM_RichMediaPresentation_Transparent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaPresentation_Transparent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_RichMediaPresentation_Transparent>(obj, "Transparent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// RichMediaPresentation_NavigationPane 
/// </summary>
internal partial class APM_RichMediaPresentation_NavigationPane : APM_RichMediaPresentation_NavigationPane__Base
{
}


internal partial class APM_RichMediaPresentation_NavigationPane__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaPresentation_NavigationPane";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_RichMediaPresentation_NavigationPane>(obj, "NavigationPane", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// RichMediaPresentation_Toolbar 
/// </summary>
internal partial class APM_RichMediaPresentation_Toolbar : APM_RichMediaPresentation_Toolbar__Base
{
}


internal partial class APM_RichMediaPresentation_Toolbar__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaPresentation_Toolbar";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_RichMediaPresentation_Toolbar>(obj, "Toolbar", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// RichMediaPresentation_PassContextClick 
/// </summary>
internal partial class APM_RichMediaPresentation_PassContextClick : APM_RichMediaPresentation_PassContextClick__Base
{
}


internal partial class APM_RichMediaPresentation_PassContextClick__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaPresentation_PassContextClick";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_RichMediaPresentation_PassContextClick>(obj, "PassContextClick", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

