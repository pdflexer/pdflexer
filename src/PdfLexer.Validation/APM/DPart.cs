// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_DPart : APM_DPart__Base
{
}

internal partial class APM_DPart__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "DPart";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_DPart_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DPart_Parent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DPart_DParts, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DPart_Start, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DPart_End, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DPart_DPM, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DPart_AF, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DPart_Metadata, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_DPart>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_DPart>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_DPart>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_DPart>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_DPart>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_DPart_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "Parent", "DParts", "Start", "End", "DPM"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "Parent", "DParts", "Start", "End", "DPM"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "Parent", "DParts", "Start", "End", "DPM"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "Parent", "DParts", "Start", "End", "DPM"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "Parent", "DParts", "Start", "End", "DPM", "AF", "Metadata"
    };
    


}

/// <summary>
/// DPart_Type Table 409
/// </summary>
internal partial class APM_DPart_Type : APM_DPart_Type__Base
{
}


internal partial class APM_DPart_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DPart_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_DPart_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.DPart)) 
        {
            ctx.Fail<APM_DPart_Type>($"Invalid value {val}, allowed are: [DPart]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// DPart_Parent 
/// </summary>
internal partial class APM_DPart_Parent : APM_DPart_Parent__Base
{
}


internal partial class APM_DPart_Parent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DPart_Parent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_DPart_Parent>(obj, "Parent", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_DPart.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_DPart, PdfDictionary>(stack, val, obj);
        } else if (APM_DPartRoot.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_DPartRoot, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_DPart_Parent>("Parent did not match any allowable types: '[DPart,DPartRoot]'");
        }
        
    }


}

/// <summary>
/// DPart_DParts 
/// </summary>
internal partial class APM_DPart_DParts : APM_DPart_DParts__Base
{
}


internal partial class APM_DPart_DParts__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DPart_DParts";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfArray, APM_DPart_DParts>(obj, "DParts", IndirectRequirement.Either);
        if ((!obj.ContainsKey(PdfName.Start)) && val == null) {
            ctx.Fail<APM_DPart_DParts>("DParts is required when 'fn:IsRequired(fn:Not(fn:IsPresent(Start)))"); return;
        } else if (val == null) {
            return;
        }
        var DParts = obj.Get("DParts");
        if (!(gt(((DParts as PdfArray)?.Count),0))) 
        {
            ctx.Fail<APM_DPart_DParts>($"Value failed special case check: fn:Eval(fn:ArrayLength(DParts)>0)");
        }
        // no value restrictions
        ctx.Run<APM_ArrayOfDPartArrays, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// DPart_Start 
/// </summary>
internal partial class APM_DPart_Start : APM_DPart_Start__Base
{
}


internal partial class APM_DPart_Start__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DPart_Start";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfDictionary, APM_DPart_Start>(obj, "Start", IndirectRequirement.MustBeIndirect);
        if ((!obj.ContainsKey(val)) && val == null) {
            ctx.Fail<APM_DPart_Start>("Start is required when 'fn:IsRequired(fn:Not(fn:IsPresent(DParts)))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        ctx.Run<APM_PageObject, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// DPart_End 
/// </summary>
internal partial class APM_DPart_End : APM_DPart_End__Base
{
}


internal partial class APM_DPart_End__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DPart_End";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_DPart_End>(obj, "End", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_PageObject, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// DPart_DPM 
/// </summary>
internal partial class APM_DPart_DPM : APM_DPart_DPM__Base
{
}


internal partial class APM_DPart_DPM__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DPart_DPM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_DPart_DPM>(obj, "DPM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_DPM, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// DPart_AF 
/// </summary>
internal partial class APM_DPart_AF : APM_DPart_AF__Base
{
}


internal partial class APM_DPart_AF__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DPart_AF";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_DPart_AF>(obj, "AF", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfFileSpecifications, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// DPart_Metadata 
/// </summary>
internal partial class APM_DPart_Metadata : APM_DPart_Metadata__Base
{
}


internal partial class APM_DPart_Metadata__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DPart_Metadata";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_DPart_Metadata>(obj, "Metadata", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Metadata, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

