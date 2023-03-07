// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_PageTreeNode : APM_PageTreeNode__Base
{
}

internal partial class APM_PageTreeNode__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "PageTreeNode";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_PageTreeNode_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PageTreeNode_Parent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PageTreeNode_Kids, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PageTreeNode_Count, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PageTreeNode_Resources, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PageTreeNode_MediaBox, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PageTreeNode_CropBox, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PageTreeNode_Rotate, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_10.Contains(x)))
                {
                    ctx.Fail<APM_PageTreeNode>($"Unknown field {extra} for version 1.0");
                }
                break;
            case 1.1m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_11.Contains(x)))
                {
                    ctx.Fail<APM_PageTreeNode>($"Unknown field {extra} for version 1.1");
                }
                break;
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_PageTreeNode>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_PageTreeNode>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_PageTreeNode>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_PageTreeNode>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_PageTreeNode>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_PageTreeNode>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_PageTreeNode>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_PageTreeNode>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_PageTreeNode>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_PageTreeNode_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_10 { get; } = new HashSet<string> 
    {
        "Type", "Parent", "Kids", "Count", "Resources", "MediaBox", "CropBox", "Rotate"
    };
    public static HashSet<string> AllowedFields_11 { get; } = new HashSet<string> 
    {
        "Type", "Parent", "Kids", "Count", "Resources", "MediaBox", "CropBox", "Rotate"
    };
    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "Parent", "Kids", "Count", "Resources", "MediaBox", "CropBox", "Rotate"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "Parent", "Kids", "Count", "Resources", "MediaBox", "CropBox", "Rotate"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "Parent", "Kids", "Count", "Resources", "MediaBox", "CropBox", "Rotate"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "Parent", "Kids", "Count", "Resources", "MediaBox", "CropBox", "Rotate"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "Parent", "Kids", "Count", "Resources", "MediaBox", "CropBox", "Rotate"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "Parent", "Kids", "Count", "Resources", "MediaBox", "CropBox", "Rotate"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "Parent", "Kids", "Count", "Resources", "MediaBox", "CropBox", "Rotate"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "Parent", "Kids", "Count", "Resources", "MediaBox", "CropBox", "Rotate"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "Parent", "Kids", "Count", "Resources", "MediaBox", "CropBox", "Rotate"
    };
    


}

/// <summary>
/// PageTreeNode_Type Table 30
/// </summary>
internal partial class APM_PageTreeNode_Type : APM_PageTreeNode_Type__Base
{
}


internal partial class APM_PageTreeNode_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PageTreeNode_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_PageTreeNode_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Pages)) 
        {
            ctx.Fail<APM_PageTreeNode_Type>($"Invalid value {val}, allowed are: [Pages]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// PageTreeNode_Parent 
/// </summary>
internal partial class APM_PageTreeNode_Parent : APM_PageTreeNode_Parent__Base
{
}


internal partial class APM_PageTreeNode_Parent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PageTreeNode_Parent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_PageTreeNode_Parent>(obj, "Parent", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_PageTreeNode.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_PageTreeNode, PdfDictionary>(stack, val, obj);
        } else if (APM_PageTreeNodeRoot.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_PageTreeNodeRoot, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_PageTreeNode_Parent>("Parent did not match any allowable types: '[PageTreeNode,PageTreeNodeRoot]'");
        }
        
    }


}

/// <summary>
/// PageTreeNode_Kids 
/// </summary>
internal partial class APM_PageTreeNode_Kids : APM_PageTreeNode_Kids__Base
{
}


internal partial class APM_PageTreeNode_Kids__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PageTreeNode_Kids";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_PageTreeNode_Kids>(obj, "Kids", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfPageTreeNodeKids, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// PageTreeNode_Count 
/// </summary>
internal partial class APM_PageTreeNode_Count : APM_PageTreeNode_Count__Base
{
}


internal partial class APM_PageTreeNode_Count__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PageTreeNode_Count";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_PageTreeNode_Count>(obj, "Count", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var Count = obj.Get("Count");
        if (!(gte(Count,0))) 
        {
            ctx.Fail<APM_PageTreeNode_Count>($"Invalid value {val}, allowed are: [fn:Eval(@Count>=0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// PageTreeNode_Resources Inheritable from Parent
/// </summary>
internal partial class APM_PageTreeNode_Resources : APM_PageTreeNode_Resources__Base
{
}


internal partial class APM_PageTreeNode_Resources__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PageTreeNode_Resources";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_PageTreeNode_Resources>(obj, "Resources", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Resource, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// PageTreeNode_MediaBox Inheritable from Parent
/// </summary>
internal partial class APM_PageTreeNode_MediaBox : APM_PageTreeNode_MediaBox__Base
{
}


internal partial class APM_PageTreeNode_MediaBox__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PageTreeNode_MediaBox";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_PageTreeNode_MediaBox>(obj, "MediaBox", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// PageTreeNode_CropBox Inheritable from Parent
/// </summary>
internal partial class APM_PageTreeNode_CropBox : APM_PageTreeNode_CropBox__Base
{
}


internal partial class APM_PageTreeNode_CropBox__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PageTreeNode_CropBox";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_PageTreeNode_CropBox>(obj, "CropBox", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// PageTreeNode_Rotate Inheritable from Parent
/// </summary>
internal partial class APM_PageTreeNode_Rotate : APM_PageTreeNode_Rotate__Base
{
}


internal partial class APM_PageTreeNode_Rotate__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PageTreeNode_Rotate";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_PageTreeNode_Rotate>(obj, "Rotate", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var Rotate = obj.Get("Rotate");
        if (!(eq(mod(Rotate,90),0))) 
        {
            ctx.Fail<APM_PageTreeNode_Rotate>($"Invalid value {val}, allowed are: [fn:Eval((@Rotate mod 90)==0)]");
        }
        // no linked objects
        
    }


}
