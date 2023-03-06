// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_AAPL_ST : APM_AAPL_ST__Base
{
}

internal partial class APM_AAPL_ST__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "AAPL_ST";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_AAPL_ST_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AAPL_ST_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AAPL_ST_Offset, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AAPL_ST_Radius, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AAPL_ST_ColorSpace, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AAPL_ST_Color, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_AAPL_ST>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_AAPL_ST>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_AAPL_ST>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_AAPL_ST>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_AAPL_ST>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_AAPL_ST>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_AAPL_ST>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_AAPL_ST>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_AAPL_ST>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_AAPL_ST_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Offset", "Radius", "ColorSpace", "Color"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Offset", "Radius", "ColorSpace", "Color"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Offset", "Radius", "ColorSpace", "Color"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Offset", "Radius", "ColorSpace", "Color"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Offset", "Radius", "ColorSpace", "Color"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Offset", "Radius", "ColorSpace", "Color"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Offset", "Radius", "ColorSpace", "Color"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Offset", "Radius", "ColorSpace", "Color"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Offset", "Radius", "ColorSpace", "Color"
    };
    


}

/// <summary>
/// AAPL_ST_Type AAPL:ST in Graphics State
/// </summary>
internal partial class APM_AAPL_ST_Type : APM_AAPL_ST_Type__Base
{
}


internal partial class APM_AAPL_ST_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AAPL_ST_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_AAPL_ST_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "Style")) 
        {
            ctx.Fail<APM_AAPL_ST_Type>($"Invalid value {val}, allowed are: [Style]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AAPL_ST_Subtype 
/// </summary>
internal partial class APM_AAPL_ST_Subtype : APM_AAPL_ST_Subtype__Base
{
}


internal partial class APM_AAPL_ST_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AAPL_ST_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_AAPL_ST_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "Shadow")) 
        {
            ctx.Fail<APM_AAPL_ST_Subtype>($"Invalid value {val}, allowed are: [Shadow]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AAPL_ST_Offset X,Y offset for drop shadow
/// </summary>
internal partial class APM_AAPL_ST_Offset : APM_AAPL_ST_Offset__Base
{
}


internal partial class APM_AAPL_ST_Offset__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AAPL_ST_Offset";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_AAPL_ST_Offset>(obj, "Offset", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_2Numbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// AAPL_ST_Radius 
/// </summary>
internal partial class APM_AAPL_ST_Radius : APM_AAPL_ST_Radius__Base
{
}


internal partial class APM_AAPL_ST_Radius__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AAPL_ST_Radius";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_AAPL_ST_Radius>(obj, "Radius", IndirectRequirement.Either);
        if (val == null) { return; }
        var Radius = obj.Get("Radius");
        if (!(gt(Radius,0))) 
        {
            ctx.Fail<APM_AAPL_ST_Radius>($"Value failed special case check: fn:Eval(@Radius>0)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AAPL_ST_ColorSpace 
/// </summary>
internal partial class APM_AAPL_ST_ColorSpace : APM_AAPL_ST_ColorSpace__Base
{
}


internal partial class APM_AAPL_ST_ColorSpace__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AAPL_ST_ColorSpace";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_AAPL_ST_ColorSpace>(obj, "ColorSpace", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    if (APM_IndexedColorSpace.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_IndexedColorSpace, PdfArray>(stack, val, obj);
                    } else if ((ctx.Version >= 1.1m && APM_CalGrayColorSpace.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_CalGrayColorSpace, PdfArray>(stack, val, obj);
                    } else if ((ctx.Version >= 1.1m && APM_CalRGBColorSpace.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_CalRGBColorSpace, PdfArray>(stack, val, obj);
                    } else if ((ctx.Version >= 1.1m && APM_LabColorSpace.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_LabColorSpace, PdfArray>(stack, val, obj);
                    } else if ((ctx.Version >= 1.3m && APM_ICCBasedColorSpace.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_ICCBasedColorSpace, PdfArray>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_AAPL_ST_ColorSpace>("ColorSpace did not match any allowable types: '[fn:SinceVersion(1.1,CalGrayColorSpace),fn:SinceVersion(1.1,CalRGBColorSpace),fn:SinceVersion(1.1,LabColorSpace),fn:SinceVersion(1.3,ICCBasedColorSpace),IndexedColorSpace]'");
                    }
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    
                    
                    if (!(val == "DeviceCMYK" || val == "DeviceRGB" || val == "DeviceGray")) 
                    {
                        ctx.Fail<APM_AAPL_ST_ColorSpace>($"Invalid value {val}, allowed are: [DeviceCMYK,DeviceRGB,DeviceGray]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_AAPL_ST_ColorSpace>("ColorSpace is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// AAPL_ST_Color 
/// </summary>
internal partial class APM_AAPL_ST_Color : APM_AAPL_ST_Color__Base
{
}


internal partial class APM_AAPL_ST_Color__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AAPL_ST_Color";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_AAPL_ST_Color>(obj, "Color", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfNumbersGeneral, PdfArray>(stack, val, obj);
        
    }


}

