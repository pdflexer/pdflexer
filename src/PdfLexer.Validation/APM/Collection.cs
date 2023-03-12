// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_Collection : APM_Collection__Base
{
}

internal partial class APM_Collection__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "Collection";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_Collection_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Collection_Schema, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Collection_D, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Collection_View, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Collection_Navigator, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Collection_Colors, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Collection_Sort, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Collection_Folders, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Collection_Split, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Collection_Resources, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_Collection>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_Collection>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_Collection>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_Collection>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_Collection_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "Schema", "D", "View", "Navigator", "Colors", "Sort", "Folders", "Split", "Resources"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "Schema", "D", "View", "Navigator", "Colors", "Sort", "Folders", "Split", "Resources"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "Schema", "D", "View", "Navigator", "Colors", "Sort", "Folders", "Split", "Resources"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "Schema", "D", "View", "Navigator", "Colors", "Sort", "Folders", "Split", "Resources"
    };
    


}

/// <summary>
/// Collection_Type Table 153
/// </summary>
internal partial class APM_Collection_Type : APM_Collection_Type__Base
{
}


internal partial class APM_Collection_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Collection_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_Collection_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Collection)) 
        {
            ctx.Fail<APM_Collection_Type>($"Invalid value {val}, allowed are: [Collection]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// Collection_Schema 
/// </summary>
internal partial class APM_Collection_Schema : APM_Collection_Schema__Base
{
}


internal partial class APM_Collection_Schema__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Collection_Schema";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_Collection_Schema>(obj, "Schema", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_CollectionSchema, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// Collection_D 
/// </summary>
internal partial class APM_Collection_D : APM_Collection_D__Base
{
}


internal partial class APM_Collection_D__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Collection_D";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_Collection_D>(obj, "D", IndirectRequirement.Either);
        if (((ctx.Version >= 2.0m && IsEncryptedWrapper(obj))) && val == null) {
            ctx.Fail<APM_Collection_D>("D is required when 'fn:IsRequired(fn:SinceVersion(2.0,fn:IsEncryptedWrapper()))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Collection_View 
/// </summary>
internal partial class APM_Collection_View : APM_Collection_View__Base
{
}


internal partial class APM_Collection_View__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Collection_View";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_Collection_View>(obj, "View", IndirectRequirement.Either);
        if (((ctx.Version >= 2.0m && IsEncryptedWrapper(obj))) && val == null) {
            ctx.Fail<APM_Collection_View>("View is required when 'fn:IsRequired(fn:SinceVersion(2.0,fn:IsEncryptedWrapper()))"); return;
        } else if (val == null) {
            return;
        }
        var View = obj.Get("View");
        if (!((ctx.Version >= 2.0m && (IsEncryptedWrapper(obj)&&eq(View,PdfName.H))||(obj.ContainsKey(PdfName.Navigator)&&eq(View,PdfName.C))||!eq(View,PdfName.C)))) 
        {
            ctx.Fail<APM_Collection_View>($"Value failed special case check: fn:Eval(fn:SinceVersion(2.0,((fn:IsEncryptedWrapper() && (@View==H)) || (fn:IsPresent(Navigator) && (@View==C)) || (@View!=C))))");
        }
        
        
        if (!(val == PdfName.D || val == PdfName.T || val == PdfName.H || (ctx.Version == 1.7m && (ctx.Extensions.Contains(PdfName.ADBE_Extn3) && val == PdfName.C)) || (ctx.Version >= 2.0m && val == PdfName.C))) 
        {
            ctx.Fail<APM_Collection_View>($"Invalid value {val}, allowed are: [D,T,H,fn:IsPDFVersion(1.7,fn:Extension(ADBE_Extn3,C)),fn:SinceVersion(2.0,C)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// Collection_Navigator 
/// </summary>
internal partial class APM_Collection_Navigator : APM_Collection_Navigator__Base
{
}


internal partial class APM_Collection_Navigator__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Collection_Navigator";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var View = obj.Get("View");
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_Collection_Navigator>(obj, "Navigator", IndirectRequirement.MustBeIndirect);
        if ((eq(View,PdfName.C)) && val == null) {
            ctx.Fail<APM_Collection_Navigator>("Navigator is required when 'fn:IsRequired(@View==C)"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Navigator, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// Collection_Colors 
/// </summary>
internal partial class APM_Collection_Colors : APM_Collection_Colors__Base
{
}


internal partial class APM_Collection_Colors__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Collection_Colors";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_Collection_Colors>(obj, "Colors", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_CollectionColors, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// Collection_Sort 
/// </summary>
internal partial class APM_Collection_Sort : APM_Collection_Sort__Base
{
}


internal partial class APM_Collection_Sort__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Collection_Sort";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_Collection_Sort>(obj, "Sort", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_CollectionSort, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// Collection_Folders 
/// </summary>
internal partial class APM_Collection_Folders : APM_Collection_Folders__Base
{
}


internal partial class APM_Collection_Folders__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Collection_Folders";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_Collection_Folders>(obj, "Folders", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_CollectionFolder, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// Collection_Split 
/// </summary>
internal partial class APM_Collection_Split : APM_Collection_Split__Base
{
}


internal partial class APM_Collection_Split__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Collection_Split";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_Collection_Split>(obj, "Split", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_CollectionSplit, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// Collection_Resources Adobe Extension Level 3 only
/// </summary>
internal partial class APM_Collection_Resources : APM_Collection_Resources__Base
{
}


internal partial class APM_Collection_Resources__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Collection_Resources";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_Collection_Resources>(obj, "Resources", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // TODO trees
        
    }


}

