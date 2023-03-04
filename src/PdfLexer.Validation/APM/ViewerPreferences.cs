// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_ViewerPreferences : APM_ViewerPreferences_Base
{
}

internal partial class APM_ViewerPreferences_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "ViewerPreferences";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_ViewerPreferences_HideToolbar, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ViewerPreferences_HideMenubar, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ViewerPreferences_HideWindowUI, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ViewerPreferences_FitWindow, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ViewerPreferences_CenterWindow, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ViewerPreferences_DisplayDocTitle, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ViewerPreferences_NonFullScreenPageMode, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ViewerPreferences_Direction, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ViewerPreferences_ViewArea, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ViewerPreferences_ViewClip, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ViewerPreferences_PrintArea, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ViewerPreferences_PrintClip, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ViewerPreferences_PrintScaling, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ViewerPreferences_Duplex, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ViewerPreferences_PickTrayByPDFSize, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ViewerPreferences_PrintPageRange, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ViewerPreferences_NumCopies, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ViewerPreferences_Enforce, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_ViewerPreferences>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_ViewerPreferences>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_ViewerPreferences>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_ViewerPreferences>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_ViewerPreferences>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_ViewerPreferences>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_ViewerPreferences>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_ViewerPreferences>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_ViewerPreferences>($"Unknown field {extra} for version 2.0");
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

    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "HideToolbar", "HideMenubar", "HideWindowUI", "FitWindow", "CenterWindow", "NonFullScreenPageMode"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "HideToolbar", "HideMenubar", "HideWindowUI", "FitWindow", "CenterWindow", "NonFullScreenPageMode", "Direction"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "HideToolbar", "HideMenubar", "HideWindowUI", "FitWindow", "CenterWindow", "DisplayDocTitle", "NonFullScreenPageMode", "Direction", "ViewArea", "ViewClip", "PrintArea", "PrintClip"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "HideToolbar", "HideMenubar", "HideWindowUI", "FitWindow", "CenterWindow", "DisplayDocTitle", "NonFullScreenPageMode", "Direction", "ViewArea", "ViewClip", "PrintArea", "PrintClip"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "HideToolbar", "HideMenubar", "HideWindowUI", "FitWindow", "CenterWindow", "DisplayDocTitle", "NonFullScreenPageMode", "Direction", "ViewArea", "ViewClip", "PrintArea", "PrintClip", "PrintScaling"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "HideToolbar", "HideMenubar", "HideWindowUI", "FitWindow", "CenterWindow", "DisplayDocTitle", "NonFullScreenPageMode", "Direction", "ViewArea", "ViewClip", "PrintArea", "PrintClip", "PrintScaling", "Duplex", "PickTrayByPDFSize", "PrintPageRange", "NumCopies"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "HideToolbar", "HideMenubar", "HideWindowUI", "FitWindow", "CenterWindow", "DisplayDocTitle", "NonFullScreenPageMode", "Direction", "ViewArea", "ViewClip", "PrintArea", "PrintClip", "PrintScaling", "Duplex", "PickTrayByPDFSize", "PrintPageRange", "NumCopies"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "HideToolbar", "HideMenubar", "HideWindowUI", "FitWindow", "CenterWindow", "DisplayDocTitle", "NonFullScreenPageMode", "Direction", "ViewArea", "ViewClip", "PrintArea", "PrintClip", "PrintScaling", "Duplex", "PickTrayByPDFSize", "PrintPageRange", "NumCopies"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "HideToolbar", "HideMenubar", "HideWindowUI", "FitWindow", "CenterWindow", "DisplayDocTitle", "NonFullScreenPageMode", "Direction", "PrintScaling", "Duplex", "PickTrayByPDFSize", "PrintPageRange", "NumCopies"
    };
    


}

/// <summary>
/// ViewerPreferences_HideToolbar Table 147
/// </summary>
internal partial class APM_ViewerPreferences_HideToolbar : APM_ViewerPreferences_HideToolbar_Base
{
}


internal partial class APM_ViewerPreferences_HideToolbar_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ViewerPreferences_HideToolbar";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_ViewerPreferences_HideToolbar>(obj, "HideToolbar", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ViewerPreferences_HideMenubar 
/// </summary>
internal partial class APM_ViewerPreferences_HideMenubar : APM_ViewerPreferences_HideMenubar_Base
{
}


internal partial class APM_ViewerPreferences_HideMenubar_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ViewerPreferences_HideMenubar";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_ViewerPreferences_HideMenubar>(obj, "HideMenubar", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ViewerPreferences_HideWindowUI 
/// </summary>
internal partial class APM_ViewerPreferences_HideWindowUI : APM_ViewerPreferences_HideWindowUI_Base
{
}


internal partial class APM_ViewerPreferences_HideWindowUI_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ViewerPreferences_HideWindowUI";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_ViewerPreferences_HideWindowUI>(obj, "HideWindowUI", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ViewerPreferences_FitWindow 
/// </summary>
internal partial class APM_ViewerPreferences_FitWindow : APM_ViewerPreferences_FitWindow_Base
{
}


internal partial class APM_ViewerPreferences_FitWindow_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ViewerPreferences_FitWindow";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_ViewerPreferences_FitWindow>(obj, "FitWindow", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ViewerPreferences_CenterWindow 
/// </summary>
internal partial class APM_ViewerPreferences_CenterWindow : APM_ViewerPreferences_CenterWindow_Base
{
}


internal partial class APM_ViewerPreferences_CenterWindow_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ViewerPreferences_CenterWindow";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_ViewerPreferences_CenterWindow>(obj, "CenterWindow", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ViewerPreferences_DisplayDocTitle 
/// </summary>
internal partial class APM_ViewerPreferences_DisplayDocTitle : APM_ViewerPreferences_DisplayDocTitle_Base
{
}


internal partial class APM_ViewerPreferences_DisplayDocTitle_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ViewerPreferences_DisplayDocTitle";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_ViewerPreferences_DisplayDocTitle>(obj, "DisplayDocTitle", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ViewerPreferences_NonFullScreenPageMode 
/// </summary>
internal partial class APM_ViewerPreferences_NonFullScreenPageMode : APM_ViewerPreferences_NonFullScreenPageMode_Base
{
}


internal partial class APM_ViewerPreferences_NonFullScreenPageMode_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ViewerPreferences_NonFullScreenPageMode";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_ViewerPreferences_NonFullScreenPageMode>(obj, "NonFullScreenPageMode", IndirectRequirement.Either);
        if (val == null) { return; }
        // TODO special case
        {
        
        
        if (!(val == "UseNone" || val == "UseOutlines" || val == "UseThumbs" || val == "UseOC")) 
        {
            ctx.Fail<APM_ViewerPreferences_NonFullScreenPageMode>($"Invalid value {val}, allowed are: [UseNone,UseOutlines,UseThumbs,UseOC]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// ViewerPreferences_Direction 
/// </summary>
internal partial class APM_ViewerPreferences_Direction : APM_ViewerPreferences_Direction_Base
{
}


internal partial class APM_ViewerPreferences_Direction_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ViewerPreferences_Direction";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_ViewerPreferences_Direction>(obj, "Direction", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "L2R" || val == "R2L")) 
        {
            ctx.Fail<APM_ViewerPreferences_Direction>($"Invalid value {val}, allowed are: [L2R,R2L]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// ViewerPreferences_ViewArea 
/// </summary>
internal partial class APM_ViewerPreferences_ViewArea : APM_ViewerPreferences_ViewArea_Base
{
}


internal partial class APM_ViewerPreferences_ViewArea_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ViewerPreferences_ViewArea";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_ViewerPreferences_ViewArea>(obj, "ViewArea", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "MediaBox" || val == "CropBox" || val == "BleedBox" || val == "TrimBox" || val == "ArtBox")) 
        {
            ctx.Fail<APM_ViewerPreferences_ViewArea>($"Invalid value {val}, allowed are: [MediaBox,CropBox,BleedBox,TrimBox,ArtBox]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// ViewerPreferences_ViewClip 
/// </summary>
internal partial class APM_ViewerPreferences_ViewClip : APM_ViewerPreferences_ViewClip_Base
{
}


internal partial class APM_ViewerPreferences_ViewClip_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ViewerPreferences_ViewClip";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_ViewerPreferences_ViewClip>(obj, "ViewClip", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "MediaBox" || val == "CropBox" || val == "BleedBox" || val == "TrimBox" || val == "ArtBox")) 
        {
            ctx.Fail<APM_ViewerPreferences_ViewClip>($"Invalid value {val}, allowed are: [MediaBox,CropBox,BleedBox,TrimBox,ArtBox]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// ViewerPreferences_PrintArea 
/// </summary>
internal partial class APM_ViewerPreferences_PrintArea : APM_ViewerPreferences_PrintArea_Base
{
}


internal partial class APM_ViewerPreferences_PrintArea_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ViewerPreferences_PrintArea";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_ViewerPreferences_PrintArea>(obj, "PrintArea", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "MediaBox" || val == "CropBox" || val == "BleedBox" || val == "TrimBox" || val == "ArtBox")) 
        {
            ctx.Fail<APM_ViewerPreferences_PrintArea>($"Invalid value {val}, allowed are: [MediaBox,CropBox,BleedBox,TrimBox,ArtBox]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// ViewerPreferences_PrintClip 
/// </summary>
internal partial class APM_ViewerPreferences_PrintClip : APM_ViewerPreferences_PrintClip_Base
{
}


internal partial class APM_ViewerPreferences_PrintClip_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ViewerPreferences_PrintClip";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_ViewerPreferences_PrintClip>(obj, "PrintClip", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "MediaBox" || val == "CropBox" || val == "BleedBox" || val == "TrimBox" || val == "ArtBox")) 
        {
            ctx.Fail<APM_ViewerPreferences_PrintClip>($"Invalid value {val}, allowed are: [MediaBox,CropBox,BleedBox,TrimBox,ArtBox]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// ViewerPreferences_PrintScaling 
/// </summary>
internal partial class APM_ViewerPreferences_PrintScaling : APM_ViewerPreferences_PrintScaling_Base
{
}


internal partial class APM_ViewerPreferences_PrintScaling_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ViewerPreferences_PrintScaling";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_ViewerPreferences_PrintScaling>(obj, "PrintScaling", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "None" || val == "AppDefault" || val == "*")) 
        {
            ctx.Fail<APM_ViewerPreferences_PrintScaling>($"Invalid value {val}, allowed are: [None,AppDefault,*]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// ViewerPreferences_Duplex 
/// </summary>
internal partial class APM_ViewerPreferences_Duplex : APM_ViewerPreferences_Duplex_Base
{
}


internal partial class APM_ViewerPreferences_Duplex_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ViewerPreferences_Duplex";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_ViewerPreferences_Duplex>(obj, "Duplex", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "Simplex" || val == "DuplexFlipShortEdge" || val == "DuplexFlipLongEdge")) 
        {
            ctx.Fail<APM_ViewerPreferences_Duplex>($"Invalid value {val}, allowed are: [Simplex,DuplexFlipShortEdge,DuplexFlipLongEdge]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// ViewerPreferences_PickTrayByPDFSize 
/// </summary>
internal partial class APM_ViewerPreferences_PickTrayByPDFSize : APM_ViewerPreferences_PickTrayByPDFSize_Base
{
}


internal partial class APM_ViewerPreferences_PickTrayByPDFSize_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ViewerPreferences_PickTrayByPDFSize";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_ViewerPreferences_PickTrayByPDFSize>(obj, "PickTrayByPDFSize", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ViewerPreferences_PrintPageRange 
/// </summary>
internal partial class APM_ViewerPreferences_PrintPageRange : APM_ViewerPreferences_PrintPageRange_Base
{
}


internal partial class APM_ViewerPreferences_PrintPageRange_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ViewerPreferences_PrintPageRange";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_ViewerPreferences_PrintPageRange>(obj, "PrintPageRange", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfIntegersGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// ViewerPreferences_NumCopies 
/// </summary>
internal partial class APM_ViewerPreferences_NumCopies : APM_ViewerPreferences_NumCopies_Base
{
}


internal partial class APM_ViewerPreferences_NumCopies_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ViewerPreferences_NumCopies";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_ViewerPreferences_NumCopies>(obj, "NumCopies", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        IPdfObject @NumCopies = val;
        if (!(gt(@NumCopies,0))) 
        {
            ctx.Fail<APM_ViewerPreferences_NumCopies>($"Invalid value {val}, allowed are: [fn:Eval(@NumCopies>0)]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// ViewerPreferences_Enforce 
/// </summary>
internal partial class APM_ViewerPreferences_Enforce : APM_ViewerPreferences_Enforce_Base
{
}


internal partial class APM_ViewerPreferences_Enforce_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ViewerPreferences_Enforce";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_ViewerPreferences_Enforce>(obj, "Enforce", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfNamesForEnforce, PdfArray>(stack, val, obj);
        
    }


}

