// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_DestOutputProfileRef : APM_DestOutputProfileRef__Base
{
}

internal partial class APM_DestOutputProfileRef__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "DestOutputProfileRef";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_DestOutputProfileRef_CheckSum, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DestOutputProfileRef_ColorantTable, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DestOutputProfileRef_ICCVersion, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DestOutputProfileRef_ProfileCS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DestOutputProfileRef_ProfileName, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DestOutputProfileRef_URLs, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_DestOutputProfileRef>($"Unknown field {extra} for version 2.0");
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

    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "CheckSum", "ColorantTable", "ICCVersion", "ProfileCS", "ProfileName", "URLs"
    };
    


}

/// <summary>
/// DestOutputProfileRef_CheckSum Table 402
/// </summary>
internal partial class APM_DestOutputProfileRef_CheckSum : APM_DestOutputProfileRef_CheckSum__Base
{
}


internal partial class APM_DestOutputProfileRef_CheckSum__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DestOutputProfileRef_CheckSum";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_DestOutputProfileRef_CheckSum>(obj, "CheckSum", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DestOutputProfileRef_ColorantTable 
/// </summary>
internal partial class APM_DestOutputProfileRef_ColorantTable : APM_DestOutputProfileRef_ColorantTable__Base
{
}


internal partial class APM_DestOutputProfileRef_ColorantTable__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DestOutputProfileRef_ColorantTable";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_DestOutputProfileRef_ColorantTable>(obj, "ColorantTable", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfNamesGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// DestOutputProfileRef_ICCVersion 
/// </summary>
internal partial class APM_DestOutputProfileRef_ICCVersion : APM_DestOutputProfileRef_ICCVersion__Base
{
}


internal partial class APM_DestOutputProfileRef_ICCVersion__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DestOutputProfileRef_ICCVersion";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_DestOutputProfileRef_ICCVersion>(obj, "ICCVersion", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DestOutputProfileRef_ProfileCS 
/// </summary>
internal partial class APM_DestOutputProfileRef_ProfileCS : APM_DestOutputProfileRef_ProfileCS__Base
{
}


internal partial class APM_DestOutputProfileRef_ProfileCS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DestOutputProfileRef_ProfileCS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_DestOutputProfileRef_ProfileCS>(obj, "ProfileCS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DestOutputProfileRef_ProfileName 
/// </summary>
internal partial class APM_DestOutputProfileRef_ProfileName : APM_DestOutputProfileRef_ProfileName__Base
{
}


internal partial class APM_DestOutputProfileRef_ProfileName__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DestOutputProfileRef_ProfileName";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_DestOutputProfileRef_ProfileName>(obj, "ProfileName", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DestOutputProfileRef_URLs 
/// </summary>
internal partial class APM_DestOutputProfileRef_URLs : APM_DestOutputProfileRef_URLs__Base
{
}


internal partial class APM_DestOutputProfileRef_URLs__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DestOutputProfileRef_URLs";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_DestOutputProfileRef_URLs>(obj, "URLs", IndirectRequirement.Either);
        if (val == null) { return; }
        var URLs = obj.Get("URLs");
        if (!(gt(((URLs as PdfArray)?.Count),0))) 
        {
            ctx.Fail<APM_DestOutputProfileRef_URLs>($"Value failed special case check: fn:Eval(fn:ArrayLength(URLs)>0)");
        }
        // no value restrictions
        ctx.Run<APM_ArrayOfURLs, PdfArray>(stack, val, obj);
        
    }


}
