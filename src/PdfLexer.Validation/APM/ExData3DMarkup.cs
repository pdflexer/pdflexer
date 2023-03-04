// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_ExData3DMarkup : APM_ExData3DMarkup_Base
{
}

internal partial class APM_ExData3DMarkup_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "ExData3DMarkup";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_ExData3DMarkup_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ExData3DMarkup_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ExData3DMarkup_3DA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ExData3DMarkup_3DV, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ExData3DMarkup_MD5, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_ExData3DMarkup>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_ExData3DMarkup>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_ExData3DMarkup>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_ExData3DMarkup>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_ExData3DMarkup_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "Subtype", "3DA", "3DV", "MD5"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "Subtype", "3DA", "3DV", "MD5"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "Subtype", "3DA", "3DV", "MD5"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "Subtype", "3DA", "3DV", "MD5"
    };
    


}

/// <summary>
/// ExData3DMarkup_Type Table 324
/// </summary>
internal partial class APM_ExData3DMarkup_Type : APM_ExData3DMarkup_Type_Base
{
}


internal partial class APM_ExData3DMarkup_Type_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ExData3DMarkup_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_ExData3DMarkup_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "ExData")) 
        {
            ctx.Fail<APM_ExData3DMarkup_Type>($"Invalid value {val}, allowed are: [ExData]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// ExData3DMarkup_Subtype 
/// </summary>
internal partial class APM_ExData3DMarkup_Subtype : APM_ExData3DMarkup_Subtype_Base
{
}


internal partial class APM_ExData3DMarkup_Subtype_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ExData3DMarkup_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_ExData3DMarkup_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "Markup3D")) 
        {
            ctx.Fail<APM_ExData3DMarkup_Subtype>($"Invalid value {val}, allowed are: [Markup3D]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// ExData3DMarkup_3DA 
/// </summary>
internal partial class APM_ExData3DMarkup_3DA : APM_ExData3DMarkup_3DA_Base
{
}


internal partial class APM_ExData3DMarkup_3DA_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ExData3DMarkup_3DA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ExData3DMarkup_3DA>(obj, "3DA", IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ExData3DMarkup_3DA>("3DA is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_Annot3D, PdfDictionary>(stack, val, obj);
                    return;
                }
            case PdfObjectType.StringObj:
                {
                    var val =  (PdfString)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_ExData3DMarkup_3DA>("3DA is required to one of 'dictionary;string-text', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ExData3DMarkup_3DV 
/// </summary>
internal partial class APM_ExData3DMarkup_3DV : APM_ExData3DMarkup_3DV_Base
{
}


internal partial class APM_ExData3DMarkup_3DV_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ExData3DMarkup_3DV";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_ExData3DMarkup_3DV>(obj, "3DV", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_3DView, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// ExData3DMarkup_MD5 
/// </summary>
internal partial class APM_ExData3DMarkup_MD5 : APM_ExData3DMarkup_MD5_Base
{
}


internal partial class APM_ExData3DMarkup_MD5_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ExData3DMarkup_MD5";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_ExData3DMarkup_MD5>(obj, "MD5", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

