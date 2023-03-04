// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_SlideShow : APM_SlideShow_Base
{
}

internal partial class APM_SlideShow_Base : ISpecification<PdfDictionary>
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
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_SlideShow>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_SlideShow>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_SlideShow>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_SlideShow>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_SlideShow>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_SlideShow>($"Unknown field {extra} for version 1.9");
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
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_14 { get; } = new List<string> 
    {
        "Type", "Subtype", "Resources", "StartResource"
    };
    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Type", "Subtype", "Resources", "StartResource"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Type", "Subtype", "Resources", "StartResource"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "Subtype", "Resources", "StartResource"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "Subtype", "Resources", "StartResource"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "Subtype", "Resources", "StartResource"
    };
    


}

/// <summary>
/// SlideShow_Type Table 308
/// </summary>
internal partial class APM_SlideShow_Type : APM_SlideShow_Type_Base
{
}


internal partial class APM_SlideShow_Type_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SlideShow_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_SlideShow_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "SlideShow")) 
        {
            ctx.Fail<APM_SlideShow_Type>($"Invalid value {val}, allowed are: [SlideShow]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// SlideShow_Subtype 
/// </summary>
internal partial class APM_SlideShow_Subtype : APM_SlideShow_Subtype_Base
{
}


internal partial class APM_SlideShow_Subtype_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SlideShow_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_SlideShow_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "Embedded")) 
        {
            ctx.Fail<APM_SlideShow_Subtype>($"Invalid value {val}, allowed are: [Embedded]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// SlideShow_Resources 
/// </summary>
internal partial class APM_SlideShow_Resources : APM_SlideShow_Resources_Base
{
}


internal partial class APM_SlideShow_Resources_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SlideShow_Resources";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_SlideShow_Resources>(obj, "Resources", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // TODO trees
        
    }


}

/// <summary>
/// SlideShow_StartResource 
/// </summary>
internal partial class APM_SlideShow_StartResource : APM_SlideShow_StartResource_Base
{
}


internal partial class APM_SlideShow_StartResource_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SlideShow_StartResource";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfString, APM_SlideShow_StartResource>(obj, "StartResource", IndirectRequirement.Either);
        if (val == null) { return; }
        // TODO special case
        // no value restrictions
        // no linked objects
        
    }


}

