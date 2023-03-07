// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_VRI : APM_VRI__Base
{
}

internal partial class APM_VRI__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "VRI";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_VRI_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_VRI_Cert, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_VRI_CRL, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_VRI_OCSP, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_VRI_TU, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_VRI_TS, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_VRI>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_VRI_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "Cert", "CRL", "OCSP", "TU", "TS"
    };
    


}

/// <summary>
/// VRI_Type Table 262
/// </summary>
internal partial class APM_VRI_Type : APM_VRI_Type__Base
{
}


internal partial class APM_VRI_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "VRI_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_VRI_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.VRI)) 
        {
            ctx.Fail<APM_VRI_Type>($"Invalid value {val}, allowed are: [VRI]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// VRI_Cert 
/// </summary>
internal partial class APM_VRI_Cert : APM_VRI_Cert__Base
{
}


internal partial class APM_VRI_Cert__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "VRI_Cert";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_VRI_Cert>(obj, "Cert", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfStreamsGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// VRI_CRL 
/// </summary>
internal partial class APM_VRI_CRL : APM_VRI_CRL__Base
{
}


internal partial class APM_VRI_CRL__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "VRI_CRL";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var parentCRLs = parent?.Get("CRLs");
        var val = ctx.GetOptional<PdfArray, APM_VRI_CRL>(obj, "CRL", IndirectRequirement.Either);
        if ((gt(((parentCRLs as PdfArray)?.Count),0)) && val == null) {
            ctx.Fail<APM_VRI_CRL>("CRL is required when 'fn:IsRequired(fn:ArrayLength(parent::CRLs)>0)"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfStreamsGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// VRI_OCSP 
/// </summary>
internal partial class APM_VRI_OCSP : APM_VRI_OCSP__Base
{
}


internal partial class APM_VRI_OCSP__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "VRI_OCSP";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var parentOCSPs = parent?.Get("OCSPs");
        var val = ctx.GetOptional<PdfArray, APM_VRI_OCSP>(obj, "OCSP", IndirectRequirement.Either);
        if ((gt(((parentOCSPs as PdfArray)?.Count),0)) && val == null) {
            ctx.Fail<APM_VRI_OCSP>("OCSP is required when 'fn:IsRequired(fn:ArrayLength(parent::OCSPs)>0)"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfStreamsGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// VRI_TU 
/// </summary>
internal partial class APM_VRI_TU : APM_VRI_TU__Base
{
}


internal partial class APM_VRI_TU__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "VRI_TU";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_VRI_TU>(obj, "TU", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// VRI_TS 
/// </summary>
internal partial class APM_VRI_TS : APM_VRI_TS__Base
{
}


internal partial class APM_VRI_TS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "VRI_TS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_VRI_TS>(obj, "TS", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}
