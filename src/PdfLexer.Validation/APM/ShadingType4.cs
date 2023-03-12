// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_ShadingType4 : APM_ShadingType4__Base
{
}

internal partial class APM_ShadingType4__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "ShadingType4";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_ShadingType4_BitsPerCoordinate, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType4_BitsPerComponent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType4_BitsPerFlag, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType4_Decode, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType4_Function, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType4_ShadingType, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType4_ColorSpace, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType4_Background, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType4_BBox, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType4_AntiAlias, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType4_Length, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType4_Filter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType4_DecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType4_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType4_FFilter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType4_FDecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ShadingType4_DL, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType4>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType4>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType4>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType4>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType4>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType4>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType4>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_ShadingType4>($"Unknown field {extra} for version 2.0");
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
/// ShadingType4_BitsPerCoordinate Table 5 and Table 77 and Table 81
/// </summary>
internal partial class APM_ShadingType4_BitsPerCoordinate : APM_ShadingType4_BitsPerCoordinate__Base
{
}


internal partial class APM_ShadingType4_BitsPerCoordinate__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType4_BitsPerCoordinate";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_ShadingType4_BitsPerCoordinate>(obj, "BitsPerCoordinate", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 1 || val == 2 || val == 4 || val == 8 || val == 12 || val == 16 || val == 24 || val == 32)) 
        {
            ctx.Fail<APM_ShadingType4_BitsPerCoordinate>($"Invalid value {val}, allowed are: [1,2,4,8,12,16,24,32]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ShadingType4_BitsPerComponent 
/// </summary>
internal partial class APM_ShadingType4_BitsPerComponent : APM_ShadingType4_BitsPerComponent__Base
{
}


internal partial class APM_ShadingType4_BitsPerComponent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType4_BitsPerComponent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_ShadingType4_BitsPerComponent>(obj, "BitsPerComponent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 1 || val == 2 || val == 4 || val == 8 || val == 12 || val == 16)) 
        {
            ctx.Fail<APM_ShadingType4_BitsPerComponent>($"Invalid value {val}, allowed are: [1,2,4,8,12,16]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ShadingType4_BitsPerFlag 
/// </summary>
internal partial class APM_ShadingType4_BitsPerFlag : APM_ShadingType4_BitsPerFlag__Base
{
}


internal partial class APM_ShadingType4_BitsPerFlag__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType4_BitsPerFlag";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_ShadingType4_BitsPerFlag>(obj, "BitsPerFlag", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 2 || val == 4 || val == 8)) 
        {
            ctx.Fail<APM_ShadingType4_BitsPerFlag>($"Invalid value {val}, allowed are: [2,4,8]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ShadingType4_Decode 
/// </summary>
internal partial class APM_ShadingType4_Decode : APM_ShadingType4_Decode__Base
{
}


internal partial class APM_ShadingType4_Decode__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType4_Decode";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_ShadingType4_Decode>(obj, "Decode", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfNumbersGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// ShadingType4_Function 
/// </summary>
internal partial class APM_ShadingType4_Function : APM_ShadingType4_Function__Base
{
}


internal partial class APM_ShadingType4_Function__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType4_Function";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ShadingType4_Function>(obj, "Function", IndirectRequirement.Either);
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
                        ctx.Fail<APM_ShadingType4_Function>("Function did not match any allowable types: '[FunctionType2,FunctionType3]'");
                    }
                    return;
                }
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_ShadingType4_Function>("Function is required to be indirect when a stream"); return; }
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
                        ctx.Fail<APM_ShadingType4_Function>("Function did not match any allowable types: '[FunctionType0,FunctionType4]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ShadingType4_Function>("Function is required to one of 'array;dictionary;stream', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ShadingType4_ShadingType 
/// </summary>
internal partial class APM_ShadingType4_ShadingType : APM_ShadingType4_ShadingType__Base
{
}


internal partial class APM_ShadingType4_ShadingType__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType4_ShadingType";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_ShadingType4_ShadingType>(obj, "ShadingType", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 4)) 
        {
            ctx.Fail<APM_ShadingType4_ShadingType>($"Invalid value {val}, allowed are: [4]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ShadingType4_ColorSpace except pattern
/// </summary>
internal partial class APM_ShadingType4_ColorSpace : APM_ShadingType4_ColorSpace__Base
{
}


internal partial class APM_ShadingType4_ColorSpace__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType4_ColorSpace";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ShadingType4_ColorSpace>(obj, "ColorSpace", IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ShadingType4_ColorSpace>("ColorSpace is required"); return; }
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
                        ctx.Fail<APM_ShadingType4_ColorSpace>("ColorSpace did not match any allowable types: '[CalGrayColorSpace,CalRGBColorSpace,LabColorSpace,ICCBasedColorSpace,IndexedColorSpace,SeparationColorSpace,DeviceNColorSpace]'");
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
                        ctx.Fail<APM_ShadingType4_ColorSpace>($"Invalid value {val}, allowed are: [DeviceCMYK,DeviceRGB,DeviceGray]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_ShadingType4_ColorSpace>("ColorSpace is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ShadingType4_Background 
/// </summary>
internal partial class APM_ShadingType4_Background : APM_ShadingType4_Background__Base
{
}


internal partial class APM_ShadingType4_Background__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType4_Background";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_ShadingType4_Background>(obj, "Background", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfNumbersGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// ShadingType4_BBox 
/// </summary>
internal partial class APM_ShadingType4_BBox : APM_ShadingType4_BBox__Base
{
}


internal partial class APM_ShadingType4_BBox__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType4_BBox";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_ShadingType4_BBox>(obj, "BBox", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ShadingType4_AntiAlias 
/// </summary>
internal partial class APM_ShadingType4_AntiAlias : APM_ShadingType4_AntiAlias__Base
{
}


internal partial class APM_ShadingType4_AntiAlias__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType4_AntiAlias";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfBoolean, APM_ShadingType4_AntiAlias>(obj, "AntiAlias", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ShadingType4_Length 
/// </summary>
internal partial class APM_ShadingType4_Length : APM_ShadingType4_Length__Base
{
}


internal partial class APM_ShadingType4_Length__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType4_Length";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_ShadingType4_Length>(obj, "Length", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ShadingType4_Filter 
/// </summary>
internal partial class APM_ShadingType4_Filter : APM_ShadingType4_Filter__Base
{
}


internal partial class APM_ShadingType4_Filter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType4_Filter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ShadingType4_Filter>(obj, "Filter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_ShadingType4_Filter>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
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
                        ctx.Fail<APM_ShadingType4_Filter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,fn:SinceVersion(1.5,Crypt)]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_ShadingType4_Filter>("Filter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ShadingType4_DecodeParms 
/// </summary>
internal partial class APM_ShadingType4_DecodeParms : APM_ShadingType4_DecodeParms__Base
{
}


internal partial class APM_ShadingType4_DecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType4_DecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ShadingType4_DecodeParms>(obj, "DecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_ShadingType4_DecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
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
                        ctx.Fail<APM_ShadingType4_DecodeParms>("DecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,fn:SinceVersion(1.5,FilterCrypt)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ShadingType4_DecodeParms>("DecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ShadingType4_F 
/// </summary>
internal partial class APM_ShadingType4_F : APM_ShadingType4_F__Base
{
}


internal partial class APM_ShadingType4_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType4_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ShadingType4_F>(obj, "F", IndirectRequirement.Either);
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
                ctx.Fail<APM_ShadingType4_F>("F is required to one of 'dictionary;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ShadingType4_FFilter 
/// </summary>
internal partial class APM_ShadingType4_FFilter : APM_ShadingType4_FFilter__Base
{
}


internal partial class APM_ShadingType4_FFilter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType4_FFilter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ShadingType4_FFilter>(obj, "FFilter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_ShadingType4_FFilter>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
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
                        ctx.Fail<APM_ShadingType4_FFilter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,fn:SinceVersion(1.5,Crypt)]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_ShadingType4_FFilter>("FFilter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ShadingType4_FDecodeParms 
/// </summary>
internal partial class APM_ShadingType4_FDecodeParms : APM_ShadingType4_FDecodeParms__Base
{
}


internal partial class APM_ShadingType4_FDecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType4_FDecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ShadingType4_FDecodeParms>(obj, "FDecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_ShadingType4_FDecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
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
                        ctx.Fail<APM_ShadingType4_FDecodeParms>("FDecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,fn:SinceVersion(1.5,FilterCrypt)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ShadingType4_FDecodeParms>("FDecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ShadingType4_DL 
/// </summary>
internal partial class APM_ShadingType4_DL : APM_ShadingType4_DL__Base
{
}


internal partial class APM_ShadingType4_DL__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ShadingType4_DL";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfIntNumber, APM_ShadingType4_DL>(obj, "DL", IndirectRequirement.Either);
        if (val == null) { return; }
        var DL = obj.Get("DL");
        if (!(gte(DL,0))) 
        {
            ctx.Fail<APM_ShadingType4_DL>($"Value failed special case check: fn:Eval(@DL>=0)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

