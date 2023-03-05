// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_3DBackground : APM_3DBackground__Base
{
}

internal partial class APM_3DBackground__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "3DBackground";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_3DBackground_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DBackground_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DBackground_CS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DBackground_C, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DBackground_EA, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_3DBackground>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_3DBackground>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_3DBackground>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_3DBackground>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_3DBackground>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_3DBackground_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Type", "Subtype", "CS", "C", "EA"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "Subtype", "CS", "C", "EA"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "Subtype", "CS", "C", "EA"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "Subtype", "CS", "C", "EA"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "Subtype", "CS", "C", "EA"
    };
    


}

/// <summary>
/// 3DBackground_Type Table 317
/// </summary>
internal partial class APM_3DBackground_Type : APM_3DBackground_Type__Base
{
}


internal partial class APM_3DBackground_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DBackground_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_3DBackground_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "3DBG")) 
        {
            ctx.Fail<APM_3DBackground_Type>($"Invalid value {val}, allowed are: [3DBG]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// 3DBackground_Subtype 
/// </summary>
internal partial class APM_3DBackground_Subtype : APM_3DBackground_Subtype__Base
{
}


internal partial class APM_3DBackground_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DBackground_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_3DBackground_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "SC")) 
        {
            ctx.Fail<APM_3DBackground_Subtype>($"Invalid value {val}, allowed are: [SC]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// 3DBackground_CS 
/// </summary>
internal partial class APM_3DBackground_CS : APM_3DBackground_CS__Base
{
}


internal partial class APM_3DBackground_CS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DBackground_CS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_3DBackground_CS>(obj, "CS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "DeviceRGB")) 
        {
            ctx.Fail<APM_3DBackground_CS>($"Invalid value {val}, allowed are: [DeviceRGB]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// 3DBackground_C 
/// </summary>
internal partial class APM_3DBackground_C : APM_3DBackground_C__Base
{
}


internal partial class APM_3DBackground_C__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DBackground_C";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_3DBackground_C>(obj, "C", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_3RGBNumbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DBackground_EA 
/// </summary>
internal partial class APM_3DBackground_EA : APM_3DBackground_EA__Base
{
}


internal partial class APM_3DBackground_EA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DBackground_EA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_3DBackground_EA>(obj, "EA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

