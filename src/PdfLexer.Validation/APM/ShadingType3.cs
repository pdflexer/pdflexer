// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_ShadingType3 : APM_ShadingType3__Base
{
}

internal partial class APM_ShadingType3__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "ShadingType3";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_ShadingType3_Coords, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType3_Domain, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType3_Function, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType3_Extend, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType3_ShadingType, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType3_ColorSpace, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType3_Background, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType3_BBox, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType3_AntiAlias, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType3>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType3>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType3>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType3>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType3>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType3>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType3>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType3>($"Unknown field {extra} for version 2.0");
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

    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Coords", "Domain", "Function", "Extend", "ShadingType", "ColorSpace", "Background", "BBox", "AntiAlias"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Coords", "Domain", "Function", "Extend", "ShadingType", "ColorSpace", "Background", "BBox", "AntiAlias"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Coords", "Domain", "Function", "Extend", "ShadingType", "ColorSpace", "Background", "BBox", "AntiAlias"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Coords", "Domain", "Function", "Extend", "ShadingType", "ColorSpace", "Background", "BBox", "AntiAlias"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Coords", "Domain", "Function", "Extend", "ShadingType", "ColorSpace", "Background", "BBox", "AntiAlias"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Coords", "Domain", "Function", "Extend", "ShadingType", "ColorSpace", "Background", "BBox", "AntiAlias"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Coords", "Domain", "Function", "Extend", "ShadingType", "ColorSpace", "Background", "BBox", "AntiAlias"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Coords", "Domain", "Function", "Extend", "ShadingType", "ColorSpace", "Background", "BBox", "AntiAlias"
    };
    


}

/// <summary>
/// ShadingType3_Coords Table 77 and Table 80
/// </summary>
internal partial class APM_ShadingType3_Coords : APM_ShadingType3_Coords__Base
{
}


internal partial class APM_ShadingType3_Coords__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType3_Coords";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfArray, APM_ShadingType3_Coords>(obj, "Coords", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_6Numbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// ShadingType3_Domain 
/// </summary>
internal partial class APM_ShadingType3_Domain : APM_ShadingType3_Domain__Base
{
}


internal partial class APM_ShadingType3_Domain__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType3_Domain";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_ShadingType3_Domain>(obj, "Domain", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_2Numbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// ShadingType3_Function 
/// </summary>
internal partial class APM_ShadingType3_Function : APM_ShadingType3_Function__Base
{
}


internal partial class APM_ShadingType3_Function__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType3_Function";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ShadingType3_Function>(obj, "Function", IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ShadingType3_Function>("Function is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_ArrayOfFunctions, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    if (APM_FunctionType2.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_FunctionType2, PdfDictionary>(stack, val, obj);
                    } else if (APM_FunctionType3.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_FunctionType3, PdfDictionary>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_ShadingType3_Function>("Function did not match any allowable types: '[FunctionType2,FunctionType3]'");
                    }
                    return;
                }
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_ShadingType3_Function>("Function is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    if (APM_FunctionType0.MatchesType(ctx, val.Dictionary)) 
                    {
                        ctx.Run<APM_FunctionType0, PdfDictionary>(stack, val.Dictionary, obj);
                    } else if (APM_FunctionType4.MatchesType(ctx, val.Dictionary)) 
                    {
                        ctx.Run<APM_FunctionType4, PdfDictionary>(stack, val.Dictionary, obj);
                    }else 
                    {
                        ctx.Fail<APM_ShadingType3_Function>("Function did not match any allowable types: '[FunctionType0,FunctionType4]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ShadingType3_Function>("Function is required to one of 'array;dictionary;stream', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ShadingType3_Extend 
/// </summary>
internal partial class APM_ShadingType3_Extend : APM_ShadingType3_Extend__Base
{
}


internal partial class APM_ShadingType3_Extend__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType3_Extend";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_ShadingType3_Extend>(obj, "Extend", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_2Booleans, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// ShadingType3_ShadingType 
/// </summary>
internal partial class APM_ShadingType3_ShadingType : APM_ShadingType3_ShadingType__Base
{
}


internal partial class APM_ShadingType3_ShadingType__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType3_ShadingType";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_ShadingType3_ShadingType>(obj, "ShadingType", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 3)) 
        {
            ctx.Fail<APM_ShadingType3_ShadingType>($"Invalid value {val}, allowed are: [3]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ShadingType3_ColorSpace except pattern
/// </summary>
internal partial class APM_ShadingType3_ColorSpace : APM_ShadingType3_ColorSpace__Base
{
}


internal partial class APM_ShadingType3_ColorSpace__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType3_ColorSpace";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ShadingType3_ColorSpace>(obj, "ColorSpace", IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ShadingType3_ColorSpace>("ColorSpace is required"); return; }
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
                    } else if (APM_LabColorSpace.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_LabColorSpace, PdfArray>(stack, val, obj);
                    } else if (APM_ICCBasedColorSpace.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ICCBasedColorSpace, PdfArray>(stack, val, obj);
                    } else if (APM_IndexedColorSpace.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_IndexedColorSpace, PdfArray>(stack, val, obj);
                    } else if (APM_SeparationColorSpace.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_SeparationColorSpace, PdfArray>(stack, val, obj);
                    } else if (APM_DeviceNColorSpace.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_DeviceNColorSpace, PdfArray>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_ShadingType3_ColorSpace>("ColorSpace did not match any allowable types: '[CalGrayColorSpace,CalRGBColorSpace,LabColorSpace,ICCBasedColorSpace,IndexedColorSpace,SeparationColorSpace,DeviceNColorSpace]'");
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
                        ctx.Fail<APM_ShadingType3_ColorSpace>($"Invalid value {val}, allowed are: [DeviceCMYK,DeviceRGB,DeviceGray]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_ShadingType3_ColorSpace>("ColorSpace is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ShadingType3_Background 
/// </summary>
internal partial class APM_ShadingType3_Background : APM_ShadingType3_Background__Base
{
}


internal partial class APM_ShadingType3_Background__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType3_Background";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_ShadingType3_Background>(obj, "Background", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfNumbersGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// ShadingType3_BBox 
/// </summary>
internal partial class APM_ShadingType3_BBox : APM_ShadingType3_BBox__Base
{
}


internal partial class APM_ShadingType3_BBox__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType3_BBox";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_ShadingType3_BBox>(obj, "BBox", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ShadingType3_AntiAlias 
/// </summary>
internal partial class APM_ShadingType3_AntiAlias : APM_ShadingType3_AntiAlias__Base
{
}


internal partial class APM_ShadingType3_AntiAlias__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType3_AntiAlias";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfBoolean, APM_ShadingType3_AntiAlias>(obj, "AntiAlias", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

