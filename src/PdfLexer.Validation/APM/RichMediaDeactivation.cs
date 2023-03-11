// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_RichMediaDeactivation : APM_RichMediaDeactivation__Base
{
}

internal partial class APM_RichMediaDeactivation__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "RichMediaDeactivation";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_RichMediaDeactivation_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaDeactivation_Condition, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaDeactivation>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaDeactivation>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaDeactivation>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaDeactivation>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_RichMediaDeactivation_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "Condition"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "Condition"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "Condition"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "Condition"
    };
    


}

/// <summary>
/// RichMediaDeactivation_Type Table 336
/// </summary>
internal partial class APM_RichMediaDeactivation_Type : APM_RichMediaDeactivation_Type__Base
{
}


internal partial class APM_RichMediaDeactivation_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaDeactivation_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_RichMediaDeactivation_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.RichMediaDeactivation)) 
        {
            ctx.Fail<APM_RichMediaDeactivation_Type>($"Invalid value {val}, allowed are: [RichMediaDeactivation]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// RichMediaDeactivation_Condition 
/// </summary>
internal partial class APM_RichMediaDeactivation_Condition : APM_RichMediaDeactivation_Condition__Base
{
}


internal partial class APM_RichMediaDeactivation_Condition__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaDeactivation_Condition";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_RichMediaDeactivation_Condition>(obj, "Condition", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.XD || val == PdfName.PC || val == PdfName.PI)) 
        {
            ctx.Fail<APM_RichMediaDeactivation_Condition>($"Invalid value {val}, allowed are: [XD,PC,PI]");
        }
        // no linked objects
        
    }


}

