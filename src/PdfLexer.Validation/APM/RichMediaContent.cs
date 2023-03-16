// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_RichMediaContent : APM_RichMediaContent__Base
{
}

internal partial class APM_RichMediaContent__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "RichMediaContent";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_RichMediaContent_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaContent_Assets, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaContent_Configurations, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaContent_Views, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.7m:
            case 1.8m:
            case 1.9m:
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17_18_19_20.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaContent>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_RichMediaContent_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_17_18_19_20 { get; } = new List<string> 
    {
        "Assets", "Configurations", "Type", "Views"
    };
    


}

/// <summary>
/// RichMediaContent_Type Table 341
/// </summary>
internal partial class APM_RichMediaContent_Type : APM_RichMediaContent_Type__Base
{
}


internal partial class APM_RichMediaContent_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaContent_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_RichMediaContent_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.RichMediaContent)) 
        {
            ctx.Fail<APM_RichMediaContent_Type>($"Invalid value {val}, allowed are: [RichMediaContent]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// RichMediaContent_Assets 
/// </summary>
internal partial class APM_RichMediaContent_Assets : APM_RichMediaContent_Assets__Base
{
}


internal partial class APM_RichMediaContent_Assets__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaContent_Assets";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_RichMediaContent_Assets>(obj, "Assets", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // TODO trees
        
    }


}

/// <summary>
/// RichMediaContent_Configurations https://github.com/pdf-association/pdf-issues/issues/166
/// </summary>
internal partial class APM_RichMediaContent_Configurations : APM_RichMediaContent_Configurations__Base
{
}


internal partial class APM_RichMediaContent_Configurations__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaContent_Configurations";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfArray, APM_RichMediaContent_Configurations>(obj, "Configurations", IndirectRequirement.Either);
        if (val == null) { return; }
        var Configurations = obj.Get("Configurations");
        if (!(gt(((Configurations as PdfArray)?.Count),0))) 
        {
            ctx.Fail<APM_RichMediaContent_Configurations>($"Value failed special case check: fn:Eval(fn:ArrayLength(Configurations)>0)");
        }
        // no value restrictions
        ctx.Run<APM_ArrayOfRichMediaConfiguration, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// RichMediaContent_Views 
/// </summary>
internal partial class APM_RichMediaContent_Views : APM_RichMediaContent_Views__Base
{
}


internal partial class APM_RichMediaContent_Views__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaContent_Views";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_RichMediaContent_Views>(obj, "Views", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf3DView, PdfArray>(stack, val, obj);
        
    }


}

