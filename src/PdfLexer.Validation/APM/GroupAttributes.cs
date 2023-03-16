// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_GroupAttributes : APM_GroupAttributes__Base
{
}

internal partial class APM_GroupAttributes__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "GroupAttributes";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_GroupAttributes_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_GroupAttributes_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_GroupAttributes_CS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_GroupAttributes_I, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_GroupAttributes_K, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.4m:
            case 1.5m:
            case 1.6m:
            case 1.7m:
            case 1.8m:
            case 1.9m:
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14_15_16_17_18_19_20.Contains(x)))
                {
                    ctx.Fail<APM_GroupAttributes>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_GroupAttributes_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_14_15_16_17_18_19_20 { get; } = new List<string> 
    {
        "CS", "I", "K", "S", "Type"
    };
    


}

/// <summary>
/// GroupAttributes_Type Table 94 and Table 145
/// </summary>
internal partial class APM_GroupAttributes_Type : APM_GroupAttributes_Type__Base
{
}


internal partial class APM_GroupAttributes_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "GroupAttributes_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_GroupAttributes_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Group)) 
        {
            ctx.Fail<APM_GroupAttributes_Type>($"Invalid value {val}, allowed are: [Group]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// GroupAttributes_S 
/// </summary>
internal partial class APM_GroupAttributes_S : APM_GroupAttributes_S__Base
{
}


internal partial class APM_GroupAttributes_S__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "GroupAttributes_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfName, APM_GroupAttributes_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Transparency)) 
        {
            ctx.Fail<APM_GroupAttributes_S>($"Invalid value {val}, allowed are: [Transparency]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// GroupAttributes_CS 
/// </summary>
internal partial class APM_GroupAttributes_CS : APM_GroupAttributes_CS__Base
{
}


internal partial class APM_GroupAttributes_CS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "GroupAttributes_CS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_GroupAttributes_CS>(obj, "CS", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    if (APM_CalGrayColorSpace.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_CalGrayColorSpace, PdfArray>(stack, val, obj);
                    } else if (APM_CalRGBColorSpace.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_CalRGBColorSpace, PdfArray>(stack, val, obj);
                    } else if (APM_ICCBasedColorSpace.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ICCBasedColorSpace, PdfArray>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_GroupAttributes_CS>("CS did not match any allowable types: '[CalGrayColorSpace,CalRGBColorSpace,ICCBasedColorSpace]'");
                    }
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    
                    
                    if (!(val == PdfName.DeviceCMYK || val == PdfName.DeviceRGB || val == PdfName.DeviceGray)) 
                    {
                        ctx.Fail<APM_GroupAttributes_CS>($"Invalid value {val}, allowed are: [DeviceCMYK,DeviceRGB,DeviceGray]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_GroupAttributes_CS>("CS is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// GroupAttributes_I 
/// </summary>
internal partial class APM_GroupAttributes_I : APM_GroupAttributes_I__Base
{
}


internal partial class APM_GroupAttributes_I__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "GroupAttributes_I";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfBoolean, APM_GroupAttributes_I>(obj, "I", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// GroupAttributes_K 
/// </summary>
internal partial class APM_GroupAttributes_K : APM_GroupAttributes_K__Base
{
}


internal partial class APM_GroupAttributes_K__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "GroupAttributes_K";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfBoolean, APM_GroupAttributes_K>(obj, "K", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

