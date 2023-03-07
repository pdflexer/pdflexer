// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_Name : APM_Name__Base
{
}

internal partial class APM_Name__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "Name";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_Name_Dests, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Name_AP, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Name_JavaScript, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Name_Pages, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Name_Templates, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Name_IDS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Name_URLS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Name_EmbeddedFiles, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Name_AlternatePresentations, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Name_Renditions, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Name_XFAResources, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_Name>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_Name>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_Name>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_Name>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_Name>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_Name>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_Name>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_Name>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_Name>($"Unknown field {extra} for version 2.0");
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
        "Dests"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Dests", "AP", "JavaScript", "Pages", "Templates", "IDS", "URLS"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Dests", "AP", "JavaScript", "Pages", "Templates", "IDS", "URLS", "EmbeddedFiles", "AlternatePresentations"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Dests", "AP", "JavaScript", "Pages", "Templates", "IDS", "URLS", "EmbeddedFiles", "AlternatePresentations", "Renditions"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Dests", "AP", "JavaScript", "Pages", "Templates", "IDS", "URLS", "EmbeddedFiles", "AlternatePresentations", "Renditions"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Dests", "AP", "JavaScript", "Pages", "Templates", "IDS", "URLS", "EmbeddedFiles", "AlternatePresentations", "Renditions", "XFAResources"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Dests", "AP", "JavaScript", "Pages", "Templates", "IDS", "URLS", "EmbeddedFiles", "AlternatePresentations", "Renditions", "XFAResources"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Dests", "AP", "JavaScript", "Pages", "Templates", "IDS", "URLS", "EmbeddedFiles", "AlternatePresentations", "Renditions", "XFAResources"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Dests", "AP", "JavaScript", "Pages", "Templates", "IDS", "URLS", "EmbeddedFiles", "Renditions", "XFAResources"
    };
    


}

/// <summary>
/// Name_Dests Table 32
/// </summary>
internal partial class APM_Name_Dests : APM_Name_Dests__Base
{
}


internal partial class APM_Name_Dests__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Name_Dests";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Name_Dests>(obj, "Dests", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // TODO trees
        
    }


}

/// <summary>
/// Name_AP 
/// </summary>
internal partial class APM_Name_AP : APM_Name_AP__Base
{
}


internal partial class APM_Name_AP__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Name_AP";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Name_AP>(obj, "AP", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // TODO trees
        
    }


}

/// <summary>
/// Name_JavaScript 
/// </summary>
internal partial class APM_Name_JavaScript : APM_Name_JavaScript__Base
{
}


internal partial class APM_Name_JavaScript__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Name_JavaScript";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Name_JavaScript>(obj, "JavaScript", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // TODO trees
        
    }


}

/// <summary>
/// Name_Pages 
/// </summary>
internal partial class APM_Name_Pages : APM_Name_Pages__Base
{
}


internal partial class APM_Name_Pages__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Name_Pages";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Name_Pages>(obj, "Pages", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // TODO trees
        
    }


}

/// <summary>
/// Name_Templates 
/// </summary>
internal partial class APM_Name_Templates : APM_Name_Templates__Base
{
}


internal partial class APM_Name_Templates__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Name_Templates";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Name_Templates>(obj, "Templates", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // TODO trees
        
    }


}

/// <summary>
/// Name_IDS 
/// </summary>
internal partial class APM_Name_IDS : APM_Name_IDS__Base
{
}


internal partial class APM_Name_IDS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Name_IDS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Name_IDS>(obj, "IDS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // TODO trees
        
    }


}

/// <summary>
/// Name_URLS 
/// </summary>
internal partial class APM_Name_URLS : APM_Name_URLS__Base
{
}


internal partial class APM_Name_URLS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Name_URLS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Name_URLS>(obj, "URLS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // TODO trees
        
    }


}

/// <summary>
/// Name_EmbeddedFiles 
/// </summary>
internal partial class APM_Name_EmbeddedFiles : APM_Name_EmbeddedFiles__Base
{
}


internal partial class APM_Name_EmbeddedFiles__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Name_EmbeddedFiles";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Name_EmbeddedFiles>(obj, "EmbeddedFiles", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // TODO trees
        
    }


}

/// <summary>
/// Name_AlternatePresentations 
/// </summary>
internal partial class APM_Name_AlternatePresentations : APM_Name_AlternatePresentations__Base
{
}


internal partial class APM_Name_AlternatePresentations__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Name_AlternatePresentations";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Name_AlternatePresentations>(obj, "AlternatePresentations", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // TODO trees
        
    }


}

/// <summary>
/// Name_Renditions 
/// </summary>
internal partial class APM_Name_Renditions : APM_Name_Renditions__Base
{
}


internal partial class APM_Name_Renditions__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Name_Renditions";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Name_Renditions>(obj, "Renditions", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // TODO trees
        
    }


}

/// <summary>
/// Name_XFAResources Adobe Extension Level 3
/// </summary>
internal partial class APM_Name_XFAResources : APM_Name_XFAResources__Base
{
}


internal partial class APM_Name_XFAResources__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Name_XFAResources";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Name_XFAResources>(obj, "XFAResources", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // TODO trees
        
    }


}
