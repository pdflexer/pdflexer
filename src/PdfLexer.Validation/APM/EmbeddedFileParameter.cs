// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_EmbeddedFileParameter : APM_EmbeddedFileParameter__Base
{
}

internal partial class APM_EmbeddedFileParameter__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "EmbeddedFileParameter";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_EmbeddedFileParameter_Size, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EmbeddedFileParameter_CreationDate, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EmbeddedFileParameter_ModDate, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EmbeddedFileParameter_Mac, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EmbeddedFileParameter_CheckSum, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_EmbeddedFileParameter>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_EmbeddedFileParameter>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_EmbeddedFileParameter>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_EmbeddedFileParameter>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_EmbeddedFileParameter>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_EmbeddedFileParameter>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_EmbeddedFileParameter>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_EmbeddedFileParameter>($"Unknown field {extra} for version 2.0");
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

    public static List<string> AllowedFields_13 { get; } = new List<string> 
    {
        "Size", "CreationDate", "ModDate", "Mac", "CheckSum"
    };
    public static List<string> AllowedFields_14 { get; } = new List<string> 
    {
        "Size", "CreationDate", "ModDate", "Mac", "CheckSum"
    };
    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Size", "CreationDate", "ModDate", "Mac", "CheckSum"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Size", "CreationDate", "ModDate", "Mac", "CheckSum"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Size", "CreationDate", "ModDate", "Mac", "CheckSum"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Size", "CreationDate", "ModDate", "Mac", "CheckSum"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Size", "CreationDate", "ModDate", "Mac", "CheckSum"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Size", "CreationDate", "ModDate", "CheckSum"
    };
    


}

/// <summary>
/// EmbeddedFileParameter_Size Table 45
/// </summary>
internal partial class APM_EmbeddedFileParameter_Size : APM_EmbeddedFileParameter_Size__Base
{
}


internal partial class APM_EmbeddedFileParameter_Size__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EmbeddedFileParameter_Size";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_EmbeddedFileParameter_Size>(obj, "Size", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var Size = obj.Get("Size");
        if (!(gte(Size,0))) 
        {
            ctx.Fail<APM_EmbeddedFileParameter_Size>($"Invalid value {val}, allowed are: [fn:Eval(@Size>=0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// EmbeddedFileParameter_CreationDate 
/// </summary>
internal partial class APM_EmbeddedFileParameter_CreationDate : APM_EmbeddedFileParameter_CreationDate__Base
{
}


internal partial class APM_EmbeddedFileParameter_CreationDate__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EmbeddedFileParameter_CreationDate";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_EmbeddedFileParameter_CreationDate>(obj, "CreationDate", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// EmbeddedFileParameter_ModDate 
/// </summary>
internal partial class APM_EmbeddedFileParameter_ModDate : APM_EmbeddedFileParameter_ModDate__Base
{
}


internal partial class APM_EmbeddedFileParameter_ModDate__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EmbeddedFileParameter_ModDate";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfString, APM_EmbeddedFileParameter_ModDate>(obj, "ModDate", IndirectRequirement.Either);
        if (((ctx.Version < 2.0m || IsAssociatedFile(obj))) && val == null) {
            ctx.Fail<APM_EmbeddedFileParameter_ModDate>("ModDate is required when 'fn:IsRequired(fn:SinceVersion(2.0,fn:IsAssociatedFile()))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// EmbeddedFileParameter_Mac 
/// </summary>
internal partial class APM_EmbeddedFileParameter_Mac : APM_EmbeddedFileParameter_Mac__Base
{
}


internal partial class APM_EmbeddedFileParameter_Mac__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EmbeddedFileParameter_Mac";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_EmbeddedFileParameter_Mac>(obj, "Mac", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Mac, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// EmbeddedFileParameter_CheckSum MD5 checksum
/// </summary>
internal partial class APM_EmbeddedFileParameter_CheckSum : APM_EmbeddedFileParameter_CheckSum__Base
{
}


internal partial class APM_EmbeddedFileParameter_CheckSum__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EmbeddedFileParameter_CheckSum";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_EmbeddedFileParameter_CheckSum>(obj, "CheckSum", IndirectRequirement.Either);
        if (val == null) { return; }
        var CheckSum = obj.Get("CheckSum");
        if (!(eq(StringLength(CheckSum),16))) 
        {
            ctx.Fail<APM_EmbeddedFileParameter_CheckSum>($"Value failed special case check: fn:Eval(fn:StringLength(CheckSum)==16)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

