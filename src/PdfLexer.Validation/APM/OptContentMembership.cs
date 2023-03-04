// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_OptContentMembership : APM_OptContentMembership_Base
{
}

internal partial class APM_OptContentMembership_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "OptContentMembership";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_OptContentMembership_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OptContentMembership_OCGs, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OptContentMembership_P, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OptContentMembership_VE, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_OptContentMembership>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_OptContentMembership>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_OptContentMembership>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_OptContentMembership>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_OptContentMembership>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_OptContentMembership>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_OptContentMembership_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Type", "OCGs", "P"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Type", "OCGs", "P", "VE"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "OCGs", "P", "VE"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "OCGs", "P", "VE"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "OCGs", "P", "VE"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "OCGs", "P", "VE"
    };
    


}

/// <summary>
/// OptContentMembership_Type Table 97
/// </summary>
internal partial class APM_OptContentMembership_Type : APM_OptContentMembership_Type_Base
{
}


internal partial class APM_OptContentMembership_Type_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OptContentMembership_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_OptContentMembership_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "OCMD")) 
        {
            ctx.Fail<APM_OptContentMembership_Type>($"Invalid value {val}, allowed are: [OCMD]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// OptContentMembership_OCGs 
/// </summary>
internal partial class APM_OptContentMembership_OCGs : APM_OptContentMembership_OCGs_Base
{
}


internal partial class APM_OptContentMembership_OCGs_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OptContentMembership_OCGs";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_OptContentMembership_OCGs>(obj, "OCGs", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_ArrayOfOCG, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_OptContentGroup, PdfDictionary>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NullObj:
                {
                    var val =  (PdfNull)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_OptContentMembership_OCGs>("OCGs is required to one of 'array;dictionary;null', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// OptContentMembership_P 
/// </summary>
internal partial class APM_OptContentMembership_P : APM_OptContentMembership_P_Base
{
}


internal partial class APM_OptContentMembership_P_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OptContentMembership_P";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_OptContentMembership_P>(obj, "P", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "AllOn" || val == "AnyOn" || val == "AnyOff" || val == "AllOff")) 
        {
            ctx.Fail<APM_OptContentMembership_P>($"Invalid value {val}, allowed are: [AllOn,AnyOn,AnyOff,AllOff]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// OptContentMembership_VE 
/// </summary>
internal partial class APM_OptContentMembership_VE : APM_OptContentMembership_VE_Base
{
}


internal partial class APM_OptContentMembership_VE_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OptContentMembership_VE";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_OptContentMembership_VE>(obj, "VE", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_VisibilityExpressionArray, PdfArray>(stack, val, obj);
        
    }


}

