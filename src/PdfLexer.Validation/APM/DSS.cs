// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_DSS : APM_DSS__Base
{
}

internal partial class APM_DSS__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "DSS";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_DSS_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DSS_VRI, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DSS_Certs, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DSS_OCSPs, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DSS_CRLs, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_DSS>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_DSS_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Certs", "CRLs", "OCSPs", "Type", "VRI"
    };
    


}

/// <summary>
/// DSS_Type Table 261
/// </summary>
internal partial class APM_DSS_Type : APM_DSS_Type__Base
{
}


internal partial class APM_DSS_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DSS_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_DSS_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.DSS)) 
        {
            ctx.Fail<APM_DSS_Type>($"Invalid value {val}, allowed are: [DSS]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// DSS_VRI 
/// </summary>
internal partial class APM_DSS_VRI : APM_DSS_VRI__Base
{
}


internal partial class APM_DSS_VRI__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DSS_VRI";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_DSS_VRI>(obj, "VRI", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_VRIMap, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// DSS_Certs 
/// </summary>
internal partial class APM_DSS_Certs : APM_DSS_Certs__Base
{
}


internal partial class APM_DSS_Certs__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DSS_Certs";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_DSS_Certs>(obj, "Certs", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfStreamsGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// DSS_OCSPs 
/// </summary>
internal partial class APM_DSS_OCSPs : APM_DSS_OCSPs__Base
{
}


internal partial class APM_DSS_OCSPs__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DSS_OCSPs";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_DSS_OCSPs>(obj, "OCSPs", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfStreamsGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// DSS_CRLs 
/// </summary>
internal partial class APM_DSS_CRLs : APM_DSS_CRLs__Base
{
}


internal partial class APM_DSS_CRLs__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DSS_CRLs";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_DSS_CRLs>(obj, "CRLs", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfStreamsGeneral, PdfArray>(stack, val, obj);
        
    }


}

