// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_ShadingType6 : APM_ShadingType6__Base
{
}

internal partial class APM_ShadingType6__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "ShadingType6";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_ShadingType6_BitsPerCoordinate, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType6_BitsPerComponent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType6_BitsPerFlag, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType6_Decode, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType6_Function, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType6_ShadingType, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType6_ColorSpace, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType6_Background, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType6_BBox, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType6_AntiAlias, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType6_Length, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType6_Filter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType6_DecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType6_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType6_FFilter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType6_FDecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType6_DL, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType6>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType6>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType6>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType6>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType6>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType6>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType6>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType6>($"Unknown field {extra} for version 2.0");
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
        "BitsPerCoordinate", "BitsPerComponent", "BitsPerFlag", "Decode", "Function", "ShadingType", "ColorSpace", "Background", "BBox", "AntiAlias", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "BitsPerCoordinate", "BitsPerComponent", "BitsPerFlag", "Decode", "Function", "ShadingType", "ColorSpace", "Background", "BBox", "AntiAlias", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "BitsPerCoordinate", "BitsPerComponent", "BitsPerFlag", "Decode", "Function", "ShadingType", "ColorSpace", "Background", "BBox", "AntiAlias", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "BitsPerCoordinate", "BitsPerComponent", "BitsPerFlag", "Decode", "Function", "ShadingType", "ColorSpace", "Background", "BBox", "AntiAlias", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "BitsPerCoordinate", "BitsPerComponent", "BitsPerFlag", "Decode", "Function", "ShadingType", "ColorSpace", "Background", "BBox", "AntiAlias", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "BitsPerCoordinate", "BitsPerComponent", "BitsPerFlag", "Decode", "Function", "ShadingType", "ColorSpace", "Background", "BBox", "AntiAlias", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "BitsPerCoordinate", "BitsPerComponent", "BitsPerFlag", "Decode", "Function", "ShadingType", "ColorSpace", "Background", "BBox", "AntiAlias", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "BitsPerCoordinate", "BitsPerComponent", "BitsPerFlag", "Decode", "Function", "ShadingType", "ColorSpace", "Background", "BBox", "AntiAlias", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    


}

/// <summary>
/// ShadingType6_BitsPerCoordinate Table 5 and Table 77 and Table 83
/// </summary>
internal partial class APM_ShadingType6_BitsPerCoordinate : APM_ShadingType6_BitsPerCoordinate__Base
{
}


internal partial class APM_ShadingType6_BitsPerCoordinate__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType6_BitsPerCoordinate";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_ShadingType6_BitsPerCoordinate>(obj, "BitsPerCoordinate", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 1m || val == 2m || val == 4m || val == 8m || val == 12m || val == 16m || val == 24m || val == 32m)) 
        {
            ctx.Fail<APM_ShadingType6_BitsPerCoordinate>($"Invalid value {val}, allowed are: [1,2,4,8,12,16,24,32]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ShadingType6_BitsPerComponent 
/// </summary>
internal partial class APM_ShadingType6_BitsPerComponent : APM_ShadingType6_BitsPerComponent__Base
{
}


internal partial class APM_ShadingType6_BitsPerComponent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType6_BitsPerComponent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_ShadingType6_BitsPerComponent>(obj, "BitsPerComponent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 1m || val == 2m || val == 4m || val == 8m || val == 12m || val == 16m)) 
        {
            ctx.Fail<APM_ShadingType6_BitsPerComponent>($"Invalid value {val}, allowed are: [1,2,4,8,12,16]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ShadingType6_BitsPerFlag 
/// </summary>
internal partial class APM_ShadingType6_BitsPerFlag : APM_ShadingType6_BitsPerFlag__Base
{
}


internal partial class APM_ShadingType6_BitsPerFlag__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType6_BitsPerFlag";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_ShadingType6_BitsPerFlag>(obj, "BitsPerFlag", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ShadingType6_Decode 
/// </summary>
internal partial class APM_ShadingType6_Decode : APM_ShadingType6_Decode__Base
{
}


internal partial class APM_ShadingType6_Decode__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType6_Decode";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_ShadingType6_Decode>(obj, "Decode", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfNumbersGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// ShadingType6_Function 
/// </summary>
internal partial class APM_ShadingType6_Function : APM_ShadingType6_Function__Base
{
}


internal partial class APM_ShadingType6_Function__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType6_Function";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ShadingType6_Function>(obj, "Function", IndirectRequirement.Either);
        if (utval == null) { return; }
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
                        ctx.Fail<APM_ShadingType6_Function>("Function did not match any allowable types: '[FunctionType2,FunctionType3]'");
                    }
                    return;
                }
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_ShadingType6_Function>("Function is required to be indirect when a stream"); return; }
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
                        ctx.Fail<APM_ShadingType6_Function>("Function did not match any allowable types: '[FunctionType0,FunctionType4]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ShadingType6_Function>("Function is required to one of 'array;dictionary;stream', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ShadingType6_ShadingType 
/// </summary>
internal partial class APM_ShadingType6_ShadingType : APM_ShadingType6_ShadingType__Base
{
}


internal partial class APM_ShadingType6_ShadingType__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType6_ShadingType";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_ShadingType6_ShadingType>(obj, "ShadingType", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 6m)) 
        {
            ctx.Fail<APM_ShadingType6_ShadingType>($"Invalid value {val}, allowed are: [6]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ShadingType6_ColorSpace except pattern
/// </summary>
internal partial class APM_ShadingType6_ColorSpace : APM_ShadingType6_ColorSpace__Base
{
}


internal partial class APM_ShadingType6_ColorSpace__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType6_ColorSpace";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ShadingType6_ColorSpace>(obj, "ColorSpace", IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ShadingType6_ColorSpace>("ColorSpace is required"); return; }
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
                        ctx.Fail<APM_ShadingType6_ColorSpace>("ColorSpace did not match any allowable types: '[CalGrayColorSpace,CalRGBColorSpace,LabColorSpace,ICCBasedColorSpace,IndexedColorSpace,SeparationColorSpace,DeviceNColorSpace]'");
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
                        ctx.Fail<APM_ShadingType6_ColorSpace>($"Invalid value {val}, allowed are: [DeviceCMYK,DeviceRGB,DeviceGray]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_ShadingType6_ColorSpace>("ColorSpace is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ShadingType6_Background 
/// </summary>
internal partial class APM_ShadingType6_Background : APM_ShadingType6_Background__Base
{
}


internal partial class APM_ShadingType6_Background__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType6_Background";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_ShadingType6_Background>(obj, "Background", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfNumbersGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// ShadingType6_BBox 
/// </summary>
internal partial class APM_ShadingType6_BBox : APM_ShadingType6_BBox__Base
{
}


internal partial class APM_ShadingType6_BBox__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType6_BBox";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_ShadingType6_BBox>(obj, "BBox", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ShadingType6_AntiAlias 
/// </summary>
internal partial class APM_ShadingType6_AntiAlias : APM_ShadingType6_AntiAlias__Base
{
}


internal partial class APM_ShadingType6_AntiAlias__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType6_AntiAlias";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfBoolean, APM_ShadingType6_AntiAlias>(obj, "AntiAlias", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ShadingType6_Length 
/// </summary>
internal partial class APM_ShadingType6_Length : APM_ShadingType6_Length__Base
{
}


internal partial class APM_ShadingType6_Length__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType6_Length";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_ShadingType6_Length>(obj, "Length", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ShadingType6_Filter 
/// </summary>
internal partial class APM_ShadingType6_Filter : APM_ShadingType6_Filter__Base
{
}


internal partial class APM_ShadingType6_Filter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType6_Filter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ShadingType6_Filter>(obj, "Filter", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    var DecodeParms = obj.Get("DecodeParms");
                    var Filter = obj.Get("Filter");
                    if (!(eq(((DecodeParms as PdfArray)?.Count),((Filter as PdfArray)?.Count)))) 
                    {
                        ctx.Fail<APM_ShadingType6_Filter>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
                    }
                    // no value restrictions
                    ctx.Run<APM_ArrayOfCompressionFilterNames, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    
                    
                    if (!(val == PdfName.ASCIIHexDecode || val == PdfName.ASCII85Decode || val == PdfName.LZWDecode || val == PdfName.FlateDecode || val == PdfName.RunLengthDecode || (ctx.Version >= 1.5m && val == PdfName.Crypt))) 
                    {
                        ctx.Fail<APM_ShadingType6_Filter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,fn:SinceVersion(1.5,Crypt)]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_ShadingType6_Filter>("Filter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ShadingType6_DecodeParms 
/// </summary>
internal partial class APM_ShadingType6_DecodeParms : APM_ShadingType6_DecodeParms__Base
{
}


internal partial class APM_ShadingType6_DecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType6_DecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ShadingType6_DecodeParms>(obj, "DecodeParms", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    var DecodeParms = obj.Get("DecodeParms");
                    var Filter = obj.Get("Filter");
                    if (!(eq(((DecodeParms as PdfArray)?.Count),((Filter as PdfArray)?.Count)))) 
                    {
                        ctx.Fail<APM_ShadingType6_DecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
                    }
                    // no value restrictions
                    ctx.Run<APM_ArrayOfDecodeParams, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    if (APM_FilterLZWDecode.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_FilterLZWDecode, PdfDictionary>(stack, val, obj);
                    } else if (APM_FilterFlateDecode.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_FilterFlateDecode, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version >= 1.5m && APM_FilterCrypt.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_FilterCrypt, PdfDictionary>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_ShadingType6_DecodeParms>("DecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,fn:SinceVersion(1.5,FilterCrypt)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ShadingType6_DecodeParms>("DecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ShadingType6_F 
/// </summary>
internal partial class APM_ShadingType6_F : APM_ShadingType6_F__Base
{
}


internal partial class APM_ShadingType6_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType6_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ShadingType6_F>(obj, "F", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_FileSpecification, PdfDictionary>(stack, val, obj);
                    return;
                }
            case PdfObjectType.StringObj:
                {
                    var val =  (PdfString)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_ShadingType6_F>("F is required to one of 'dictionary;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ShadingType6_FFilter 
/// </summary>
internal partial class APM_ShadingType6_FFilter : APM_ShadingType6_FFilter__Base
{
}


internal partial class APM_ShadingType6_FFilter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType6_FFilter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ShadingType6_FFilter>(obj, "FFilter", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    var FDecodeParms = obj.Get("FDecodeParms");
                    var FFilter = obj.Get("FFilter");
                    if (!(eq(((FDecodeParms as PdfArray)?.Count),((FFilter as PdfArray)?.Count)))) 
                    {
                        ctx.Fail<APM_ShadingType6_FFilter>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
                    }
                    // no value restrictions
                    ctx.Run<APM_ArrayOfCompressionFilterNames, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    
                    
                    if (!(val == PdfName.ASCIIHexDecode || val == PdfName.ASCII85Decode || val == PdfName.LZWDecode || val == PdfName.FlateDecode || val == PdfName.RunLengthDecode || (ctx.Version >= 1.5m && val == PdfName.Crypt))) 
                    {
                        ctx.Fail<APM_ShadingType6_FFilter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,fn:SinceVersion(1.5,Crypt)]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_ShadingType6_FFilter>("FFilter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ShadingType6_FDecodeParms 
/// </summary>
internal partial class APM_ShadingType6_FDecodeParms : APM_ShadingType6_FDecodeParms__Base
{
}


internal partial class APM_ShadingType6_FDecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType6_FDecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ShadingType6_FDecodeParms>(obj, "FDecodeParms", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    var FDecodeParms = obj.Get("FDecodeParms");
                    var FFilter = obj.Get("FFilter");
                    if (!(eq(((FDecodeParms as PdfArray)?.Count),((FFilter as PdfArray)?.Count)))) 
                    {
                        ctx.Fail<APM_ShadingType6_FDecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
                    }
                    // no value restrictions
                    ctx.Run<APM_ArrayOfDecodeParams, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    if (APM_FilterLZWDecode.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_FilterLZWDecode, PdfDictionary>(stack, val, obj);
                    } else if (APM_FilterFlateDecode.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_FilterFlateDecode, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version >= 1.5m && APM_FilterCrypt.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_FilterCrypt, PdfDictionary>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_ShadingType6_FDecodeParms>("FDecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,fn:SinceVersion(1.5,FilterCrypt)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ShadingType6_FDecodeParms>("FDecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ShadingType6_DL 
/// </summary>
internal partial class APM_ShadingType6_DL : APM_ShadingType6_DL__Base
{
}


internal partial class APM_ShadingType6_DL__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType6_DL";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfIntNumber, APM_ShadingType6_DL>(obj, "DL", IndirectRequirement.Either);
        if (val == null) { return; }
        var DL = obj.Get("DL");
        if (!(gte(DL,0))) 
        {
            ctx.Fail<APM_ShadingType6_DL>($"Value failed special case check: fn:Eval(@DL>=0)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

