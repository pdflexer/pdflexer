// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_CollectionFolder : APM_CollectionFolder__Base
{
}

internal partial class APM_CollectionFolder__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "CollectionFolder";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_CollectionFolder_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CollectionFolder_ID, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CollectionFolder_Name, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CollectionFolder_Parent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CollectionFolder_Child, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CollectionFolder_Next, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CollectionFolder_CI, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CollectionFolder_Desc, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CollectionFolder_CreationDate, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CollectionFolder_ModDate, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CollectionFolder_Thumb, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CollectionFolder_Free, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_CollectionFolder>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_CollectionFolder>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_CollectionFolder>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_CollectionFolder>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_CollectionFolder_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "ID", "Name", "Parent", "Child", "Next", "CI", "Desc", "CreationDate", "ModDate", "Thumb", "Free"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "ID", "Name", "Parent", "Child", "Next", "CI", "Desc", "CreationDate", "ModDate", "Thumb", "Free"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "ID", "Name", "Parent", "Child", "Next", "CI", "Desc", "CreationDate", "ModDate", "Thumb", "Free"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "ID", "Name", "Parent", "Child", "Next", "CI", "Desc", "CreationDate", "ModDate", "Thumb", "Free"
    };
    


}

/// <summary>
/// CollectionFolder_Type Table 159
/// </summary>
internal partial class APM_CollectionFolder_Type : APM_CollectionFolder_Type__Base
{
}


internal partial class APM_CollectionFolder_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionFolder_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_CollectionFolder_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Folder)) 
        {
            ctx.Fail<APM_CollectionFolder_Type>($"Invalid value {val}, allowed are: [Folder]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// CollectionFolder_ID 
/// </summary>
internal partial class APM_CollectionFolder_ID : APM_CollectionFolder_ID__Base
{
}


internal partial class APM_CollectionFolder_ID__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionFolder_ID";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_CollectionFolder_ID>(obj, "ID", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// CollectionFolder_Name 
/// </summary>
internal partial class APM_CollectionFolder_Name : APM_CollectionFolder_Name__Base
{
}


internal partial class APM_CollectionFolder_Name__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionFolder_Name";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfString, APM_CollectionFolder_Name>(obj, "Name", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// CollectionFolder_Parent 
/// </summary>
internal partial class APM_CollectionFolder_Parent : APM_CollectionFolder_Parent__Base
{
}


internal partial class APM_CollectionFolder_Parent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionFolder_Parent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_CollectionFolder_Parent>(obj, "Parent", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_CollectionFolder, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// CollectionFolder_Child 
/// </summary>
internal partial class APM_CollectionFolder_Child : APM_CollectionFolder_Child__Base
{
}


internal partial class APM_CollectionFolder_Child__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionFolder_Child";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_CollectionFolder_Child>(obj, "Child", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_CollectionFolder, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// CollectionFolder_Next 
/// </summary>
internal partial class APM_CollectionFolder_Next : APM_CollectionFolder_Next__Base
{
}


internal partial class APM_CollectionFolder_Next__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionFolder_Next";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_CollectionFolder_Next>(obj, "Next", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_CollectionFolder, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// CollectionFolder_CI 
/// </summary>
internal partial class APM_CollectionFolder_CI : APM_CollectionFolder_CI__Base
{
}


internal partial class APM_CollectionFolder_CI__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionFolder_CI";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_CollectionFolder_CI>(obj, "CI", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_CollectionItem, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// CollectionFolder_Desc 
/// </summary>
internal partial class APM_CollectionFolder_Desc : APM_CollectionFolder_Desc__Base
{
}


internal partial class APM_CollectionFolder_Desc__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionFolder_Desc";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_CollectionFolder_Desc>(obj, "Desc", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// CollectionFolder_CreationDate 
/// </summary>
internal partial class APM_CollectionFolder_CreationDate : APM_CollectionFolder_CreationDate__Base
{
}


internal partial class APM_CollectionFolder_CreationDate__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionFolder_CreationDate";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_CollectionFolder_CreationDate>(obj, "CreationDate", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// CollectionFolder_ModDate 
/// </summary>
internal partial class APM_CollectionFolder_ModDate : APM_CollectionFolder_ModDate__Base
{
}


internal partial class APM_CollectionFolder_ModDate__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionFolder_ModDate";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_CollectionFolder_ModDate>(obj, "ModDate", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// CollectionFolder_Thumb 
/// </summary>
internal partial class APM_CollectionFolder_Thumb : APM_CollectionFolder_Thumb__Base
{
}


internal partial class APM_CollectionFolder_Thumb__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionFolder_Thumb";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfStream, APM_CollectionFolder_Thumb>(obj, "Thumb", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Thumbnail, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// CollectionFolder_Free only used by root folder
/// </summary>
internal partial class APM_CollectionFolder_Free : APM_CollectionFolder_Free__Base
{
}


internal partial class APM_CollectionFolder_Free__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionFolder_Free";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_CollectionFolder_Free>(obj, "Free", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfNumbersGeneral, PdfArray>(stack, val, obj);
        
    }


}

