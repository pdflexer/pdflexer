// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_XObjectMap : APM_XObjectMap__Base
{
}

internal partial class APM_XObjectMap__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "XObjectMap";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_XObjectMap_CatchAll, PdfDictionary>(stack, obj, parent);
        
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        return false;
    }

    


}

/// <summary>
/// XObjectMap_* Table 34
/// </summary>
internal partial class APM_XObjectMap_CatchAll : APM_XObjectMap_CatchAll__Base
{
}


internal partial class APM_XObjectMap_CatchAll__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectMap_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        foreach (var key in obj.Keys)
        {
            if (AllVals.Contains(key)) { continue; }
            
            
            var val = ctx.GetOptional<PdfStream, APM_XObjectMap_CatchAll>(obj, key, IndirectRequirement.MustBeIndirect);
            if (val == null) { return; }
            // no special cases
            // no value restrictions
            if (APM_XObjectFormType1.MatchesType(ctx, val.Dictionary)) 
            {
                ctx.Run<APM_XObjectFormType1, PdfDictionary>(stack, val.Dictionary, obj);
            } else if (APM_XObjectImage.MatchesType(ctx, val.Dictionary)) 
            {
                ctx.Run<APM_XObjectImage, PdfDictionary>(stack, val.Dictionary, obj);
            } else if ((ctx.Version < 1.1m || (ctx.Version >= 1.1m && APM_XObjectFormPS.MatchesType(ctx, val.Dictionary)))) 
            {
                ctx.Run<APM_XObjectFormPS, PdfDictionary>(stack, val.Dictionary, obj);
            } else if ((ctx.Version < 1.1m || (ctx.Version >= 1.1m && APM_XObjectFormPSpassthrough.MatchesType(ctx, val.Dictionary)))) 
            {
                ctx.Run<APM_XObjectFormPSpassthrough, PdfDictionary>(stack, val.Dictionary, obj);
            }else 
            {
                ctx.Fail<APM_XObjectMap_CatchAll>("key did not match any allowable types: '[XObjectFormType1,XObjectImage,fn:SinceVersion(1.1,XObjectFormPS),fn:SinceVersion(1.1,XObjectFormPSpassthrough)]'");
            }
            
        }
        
    }

    public static HashSet<string> AllVals = new HashSet<string> {  };
}

