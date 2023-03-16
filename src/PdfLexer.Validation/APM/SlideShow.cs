// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_SlideShow : APM_SlideShow__Base
{
}

internal partial class APM_SlideShow__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "SlideShow";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_SlideShow_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SlideShow_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SlideShow_Resources, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SlideShow_StartResource, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.4m:
            case 1.5m:
            case 1.6m:
            case 1.7m:
            case 1.8m:
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14_15_16_17_18_19.Contains(x)))
                {
                    ctx.Fail<APM_SlideShow>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_SlideShow_Type, PdfDictionary>(new CallStack(), obj, null);
        c.Run<APM_SlideShow_Subtype, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_14_15_16_17_18_19 { get; } = new List<string> 
    {
        "Resources", "StartResource", "Subtype", "Type"
    };
    


}

/// <summary>
/// SlideShow_Type Table 308
/// </summary>
internal partial class APM_SlideShow_Type : APM_SlideShow_Type__Base
{
}


internal partial class APM_SlideShow_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SlideShow_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfName, APM_SlideShow_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.SlideShow)) 
        {
            ctx.Fail<APM_SlideShow_Type>($"Invalid value {val}, allowed are: [SlideShow]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// SlideShow_Subtype 
/// </summary>
internal partial class APM_SlideShow_Subtype : APM_SlideShow_Subtype__Base
{
}


internal partial class APM_SlideShow_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SlideShow_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfName, APM_SlideShow_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Embedded)) 
        {
            ctx.Fail<APM_SlideShow_Subtype>($"Invalid value {val}, allowed are: [Embedded]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// SlideShow_Resources 
/// </summary>
internal partial class APM_SlideShow_Resources : APM_SlideShow_Resources__Base
{
}


internal partial class APM_SlideShow_Resources__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SlideShow_Resources";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfDictionary, APM_SlideShow_Resources>(obj, "Resources", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // TODO trees
        
    }


}

/// <summary>
/// SlideShow_StartResource 
/// </summary>
internal partial class APM_SlideShow_StartResource : APM_SlideShow_StartResource__Base
{
}


internal partial class APM_SlideShow_StartResource__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SlideShow_StartResource";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfString, APM_SlideShow_StartResource>(obj, "StartResource", IndirectRequirement.Either);
        if (val == null) { return; }
        // TODO special case: fn:InNameTree(Resources)
        // no value restrictions
        // no linked objects
        
    }


}

