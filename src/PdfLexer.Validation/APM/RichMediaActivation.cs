// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_RichMediaActivation : APM_RichMediaActivation__Base
{
}

internal partial class APM_RichMediaActivation__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "RichMediaActivation";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_RichMediaActivation_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaActivation_Condition, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaActivation_Animation, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaActivation_View, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaActivation_Configuration, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaActivation_Presentation, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaActivation_Scripts, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaActivation>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaActivation>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaActivation>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaActivation>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_RichMediaActivation_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "Condition", "Animation", "View", "Configuration", "Presentation", "Scripts"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "Condition", "Animation", "View", "Configuration", "Presentation", "Scripts"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "Condition", "Animation", "View", "Configuration", "Presentation", "Scripts"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "Condition", "Animation", "View", "Configuration", "Presentation", "Scripts"
    };
    


}

/// <summary>
/// RichMediaActivation_Type Table 335
/// </summary>
internal partial class APM_RichMediaActivation_Type : APM_RichMediaActivation_Type__Base
{
}


internal partial class APM_RichMediaActivation_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaActivation_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_RichMediaActivation_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.RichMediaActivation)) 
        {
            ctx.Fail<APM_RichMediaActivation_Type>($"Invalid value {val}, allowed are: [RichMediaActivation]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// RichMediaActivation_Condition 
/// </summary>
internal partial class APM_RichMediaActivation_Condition : APM_RichMediaActivation_Condition__Base
{
}


internal partial class APM_RichMediaActivation_Condition__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaActivation_Condition";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_RichMediaActivation_Condition>(obj, "Condition", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.XA || val == PdfName.PO || val == PdfName.PV)) 
        {
            ctx.Fail<APM_RichMediaActivation_Condition>($"Invalid value {val}, allowed are: [XA,PO,PV]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// RichMediaActivation_Animation 
/// </summary>
internal partial class APM_RichMediaActivation_Animation : APM_RichMediaActivation_Animation__Base
{
}


internal partial class APM_RichMediaActivation_Animation__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaActivation_Animation";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_RichMediaActivation_Animation>(obj, "Animation", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_RichMediaAnimation, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// RichMediaActivation_View 
/// </summary>
internal partial class APM_RichMediaActivation_View : APM_RichMediaActivation_View__Base
{
}


internal partial class APM_RichMediaActivation_View__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaActivation_View";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_RichMediaActivation_View>(obj, "View", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_3DView.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_3DView, PdfDictionary>(stack, val, obj);
        } else if (APM_3DViewAddEntries.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_3DViewAddEntries, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_RichMediaActivation_View>("View did not match any allowable types: '[3DView,3DViewAddEntries]'");
        }
        
    }


}

/// <summary>
/// RichMediaActivation_Configuration https://github.com/pdf-association/pdf-issues/issues/166
/// </summary>
internal partial class APM_RichMediaActivation_Configuration : APM_RichMediaActivation_Configuration__Base
{
}


internal partial class APM_RichMediaActivation_Configuration__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaActivation_Configuration";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_RichMediaActivation_Configuration>(obj, "Configuration", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_RichMediaConfiguration, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// RichMediaActivation_Presentation 
/// </summary>
internal partial class APM_RichMediaActivation_Presentation : APM_RichMediaActivation_Presentation__Base
{
}


internal partial class APM_RichMediaActivation_Presentation__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaActivation_Presentation";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_RichMediaActivation_Presentation>(obj, "Presentation", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_RichMediaPresentation, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// RichMediaActivation_Scripts https://github.com/pdf-association/pdf-issues/issues/59
/// </summary>
internal partial class APM_RichMediaActivation_Scripts : APM_RichMediaActivation_Scripts__Base
{
}


internal partial class APM_RichMediaActivation_Scripts__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaActivation_Scripts";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_RichMediaActivation_Scripts>(obj, "Scripts", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfIndirectFileSpecifications, PdfArray>(stack, val, obj);
        
    }


}

