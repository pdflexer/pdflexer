// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_DeviceNProcess : APM_DeviceNProcess_Base
{
}

internal partial class APM_DeviceNProcess_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "DeviceNProcess";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_DeviceNProcess_ColorSpace, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DeviceNProcess_Components, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_DeviceNProcess>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_DeviceNProcess>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_DeviceNProcess>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_DeviceNProcess>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_DeviceNProcess>($"Unknown field {extra} for version 2.0");
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

    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "ColorSpace", "Components"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "ColorSpace", "Components"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "ColorSpace", "Components"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "ColorSpace", "Components"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "ColorSpace", "Components"
    };
    


}

/// <summary>
/// DeviceNProcess_ColorSpace Table 71 - except Lab
/// </summary>
internal partial class APM_DeviceNProcess_ColorSpace : APM_DeviceNProcess_ColorSpace_Base
{
}


internal partial class APM_DeviceNProcess_ColorSpace_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DeviceNProcess_ColorSpace";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_DeviceNProcess_ColorSpace>(obj, "ColorSpace", IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_DeviceNProcess_ColorSpace>("ColorSpace is required"); return; }
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
                        ctx.Fail<APM_DeviceNProcess_ColorSpace>("ColorSpace did not match any allowable types: '[CalGrayColorSpace,CalRGBColorSpace,ICCBasedColorSpace]'");
                    }
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    {
                    
                    
                    if (!(val == "DeviceCMYK" || val == "DeviceRGB" || val == "DeviceGray")) 
                    {
                        ctx.Fail<APM_DeviceNProcess_ColorSpace>($"Invalid value {val}, allowed are: [DeviceCMYK,DeviceRGB,DeviceGray]");
                    }
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_DeviceNProcess_ColorSpace>("ColorSpace is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// DeviceNProcess_Components 
/// </summary>
internal partial class APM_DeviceNProcess_Components : APM_DeviceNProcess_Components_Base
{
}


internal partial class APM_DeviceNProcess_Components_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DeviceNProcess_Components";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_DeviceNProcess_Components>(obj, "Components", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfNamesGeneral, PdfArray>(stack, val, obj);
        
    }


}

