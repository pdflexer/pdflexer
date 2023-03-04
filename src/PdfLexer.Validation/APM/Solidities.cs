// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_Solidities : APM_Solidities_Base
{
}

internal partial class APM_Solidities_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "Solidities";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_Solidities_Default, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Solidities_CatchAll, PdfDictionary>(stack, obj, parent);
        
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        return false;
    }

    


}

/// <summary>
/// Solidities_Default Table 72
/// </summary>
internal partial class APM_Solidities_Default : APM_Solidities_Default_Base
{
}


internal partial class APM_Solidities_Default_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Solidities_Default";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_Solidities_Default>(obj, "Default", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        IPdfObject @Default = val;
        if (!((gte(@Default,0)&&lte(@Default,1)))) 
        {
            ctx.Fail<APM_Solidities_Default>($"Invalid value {val}, allowed are: [fn:Eval((@Default>=0) && (@Default<=1))]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// Solidities_* 
/// </summary>
internal partial class APM_Solidities_CatchAll : APM_Solidities_CatchAll_Base
{
}


internal partial class APM_Solidities_CatchAll_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Solidities_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        foreach (var key in obj.Keys)
        {
            if (AllVals.Contains(key)) { continue; }
            
            
            IPdfObject? val = ctx.GetOptional<PdfNumber, APM_Solidities_CatchAll>(obj, key, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            {
            
            
            if (!((gte(val,0)&&lte(val,1)))) 
            {
                ctx.Fail<APM_Solidities_CatchAll>($"Invalid value {val}, allowed are: [fn:Eval((@*>=0) && (@*<=1))]");
            }
            }
            // no linked objects
            
        }
        
    }

    public static HashSet<string> AllVals = new HashSet<string> { "Default" };
}

