// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_Permissions : APM_Permissions__Base
{
}

internal partial class APM_Permissions__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "Permissions";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_Permissions_DocMDP, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Permissions_UR3, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_Permissions>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_Permissions>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_Permissions>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_Permissions>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_Permissions>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_Permissions>($"Unknown field {extra} for version 2.0");
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

    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "DocMDP", "UR3"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "DocMDP", "UR3"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "DocMDP", "UR3"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "DocMDP", "UR3"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "DocMDP", "UR3"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "DocMDP"
    };
    


}

/// <summary>
/// Permissions_DocMDP Table 263 and https://github.com/pdf-association/pdf-issues/issues/218
/// </summary>
internal partial class APM_Permissions_DocMDP : APM_Permissions_DocMDP__Base
{
}


internal partial class APM_Permissions_DocMDP__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Permissions_DocMDP";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_Permissions_DocMDP>(obj, "DocMDP", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        var DocMDPReference = val.Get("Reference");
        if (!(gte(((DocMDPReference as PdfArray)?.Count),1))) 
        {
            ctx.Fail<APM_Permissions_DocMDP>($"Value failed special case check: fn:Eval(fn:ArrayLength(DocMDP::Reference)>=1)");
        }
        // no value restrictions
        ctx.Run<APM_Signature, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// Permissions_UR3 
/// </summary>
internal partial class APM_Permissions_UR3 : APM_Permissions_UR3__Base
{
}


internal partial class APM_Permissions_UR3__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Permissions_UR3";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_Permissions_UR3>(obj, "UR3", IndirectRequirement.Either);
        if (val == null) { return; }
        var UR3Reference0TransformMethod = val.Get("Reference")?.Get(0)?.Get("TransformMethod");
        if (!(eq(UR3Reference0TransformMethod,val))) 
        {
            ctx.Fail<APM_Permissions_UR3>($"Value failed special case check: fn:Eval(UR3::Reference::0::@TransformMethod==UR3)");
        }
        // no value restrictions
        ctx.Run<APM_Signature, PdfDictionary>(stack, val, obj);
        
    }


}

