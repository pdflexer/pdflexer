// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_Thumbnail : APM_Thumbnail__Base
{
}

internal partial class APM_Thumbnail__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "Thumbnail";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_Thumbnail_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Thumbnail_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Thumbnail_Width, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Thumbnail_Height, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Thumbnail_ColorSpace, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Thumbnail_BitsPerComponent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Thumbnail_Decode, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Thumbnail_Length, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Thumbnail_Filter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Thumbnail_DecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Thumbnail_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Thumbnail_FFilter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Thumbnail_FDecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Thumbnail_DL, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.0m:
            case 1.1m:
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_10_11_12.Contains(x)))
                {
                    ctx.Fail<APM_Thumbnail>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            case 1.3m:
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13_14.Contains(x)))
                {
                    ctx.Fail<APM_Thumbnail>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            case 1.5m:
            case 1.6m:
            case 1.7m:
            case 1.8m:
            case 1.9m:
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15_16_17_18_19_20.Contains(x)))
                {
                    ctx.Fail<APM_Thumbnail>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_Thumbnail_Type, PdfDictionary>(new CallStack(), obj, null);
        c.Run<APM_Thumbnail_Subtype, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_10_11_12 { get; } = new HashSet<string> 
    {
        "BitsPerComponent", "ColorSpace", "Decode", "Height", "Subtype", "Type", "Width"
    };
    public static HashSet<string> AllowedFields_13_14 { get; } = new HashSet<string> 
    {
        "BitsPerComponent", "ColorSpace", "Decode", "DecodeParms", "F", "FDecodeParms", "FFilter", "Filter", "Height", "Length", "Subtype", "Type", "Width"
    };
    public static HashSet<string> AllowedFields_15_16_17_18_19_20 { get; } = new HashSet<string> 
    {
        "BitsPerComponent", "ColorSpace", "Decode", "DecodeParms", "DL", "F", "FDecodeParms", "FFilter", "Filter", "Height", "Length", "Subtype", "Type", "Width"
    };
    


}

/// <summary>
/// Thumbnail_Type Table 5 and Table 87 and Clause 12.3.4
/// </summary>
internal partial class APM_Thumbnail_Type : APM_Thumbnail_Type__Base
{
}


internal partial class APM_Thumbnail_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Thumbnail_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_Thumbnail_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.XObject)) 
        {
            ctx.Fail<APM_Thumbnail_Type>($"Invalid value {val}, allowed are: [XObject]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// Thumbnail_Subtype 
/// </summary>
internal partial class APM_Thumbnail_Subtype : APM_Thumbnail_Subtype__Base
{
}


internal partial class APM_Thumbnail_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Thumbnail_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_Thumbnail_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Image)) 
        {
            ctx.Fail<APM_Thumbnail_Subtype>($"Invalid value {val}, allowed are: [Image]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// Thumbnail_Width 
/// </summary>
internal partial class APM_Thumbnail_Width : APM_Thumbnail_Width__Base
{
}


internal partial class APM_Thumbnail_Width__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Thumbnail_Width";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_Thumbnail_Width>(obj, "Width", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Thumbnail_Height 
/// </summary>
internal partial class APM_Thumbnail_Height : APM_Thumbnail_Height__Base
{
}


internal partial class APM_Thumbnail_Height__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Thumbnail_Height";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_Thumbnail_Height>(obj, "Height", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Thumbnail_ColorSpace 
/// </summary>
internal partial class APM_Thumbnail_ColorSpace : APM_Thumbnail_ColorSpace__Base
{
}


internal partial class APM_Thumbnail_ColorSpace__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Thumbnail_ColorSpace";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_Thumbnail_ColorSpace>(obj, "ColorSpace", IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_Thumbnail_ColorSpace>("ColorSpace is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_IndexedColorSpace, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    
                    
                    if (!(val == PdfName.DeviceRGB || val == PdfName.DeviceGray)) 
                    {
                        ctx.Fail<APM_Thumbnail_ColorSpace>($"Invalid value {val}, allowed are: [DeviceRGB,DeviceGray]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_Thumbnail_ColorSpace>("ColorSpace is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// Thumbnail_BitsPerComponent 
/// </summary>
internal partial class APM_Thumbnail_BitsPerComponent : APM_Thumbnail_BitsPerComponent__Base
{
}


internal partial class APM_Thumbnail_BitsPerComponent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Thumbnail_BitsPerComponent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_Thumbnail_BitsPerComponent>(obj, "BitsPerComponent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 1 || val == 2 || val == 4 || val == 8 || val == 16)) 
        {
            ctx.Fail<APM_Thumbnail_BitsPerComponent>($"Invalid value {val}, allowed are: [1,2,4,8,16]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// Thumbnail_Decode 
/// </summary>
internal partial class APM_Thumbnail_Decode : APM_Thumbnail_Decode__Base
{
}


internal partial class APM_Thumbnail_Decode__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Thumbnail_Decode";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_Thumbnail_Decode>(obj, "Decode", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfNumbersGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// Thumbnail_Length 
/// </summary>
internal partial class APM_Thumbnail_Length : APM_Thumbnail_Length__Base
{
}


internal partial class APM_Thumbnail_Length__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Thumbnail_Length";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_Thumbnail_Length>(obj, "Length", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Thumbnail_Filter 
/// </summary>
internal partial class APM_Thumbnail_Filter : APM_Thumbnail_Filter__Base
{
}


internal partial class APM_Thumbnail_Filter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Thumbnail_Filter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_Thumbnail_Filter>(obj, "Filter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_Thumbnail_Filter>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
                    }
                    // no value restrictions
                    ctx.Run<APM_ArrayOfFilterNames, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    
                    
                    if (!(val == PdfName.ASCIIHexDecode || val == PdfName.ASCII85Decode || val == PdfName.LZWDecode || val == PdfName.FlateDecode || val == PdfName.RunLengthDecode || val == PdfName.CCITTFaxDecode || (ctx.Version >= 1.4m && val == PdfName.JBIG2Decode) || val == PdfName.DCTDecode || (ctx.Version >= 1.5m && val == PdfName.JPXDecode) || (ctx.Version >= 1.5m && val == PdfName.Crypt))) 
                    {
                        ctx.Fail<APM_Thumbnail_Filter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,CCITTFaxDecode,fn:SinceVersion(1.4,JBIG2Decode),DCTDecode,fn:SinceVersion(1.5,JPXDecode),fn:SinceVersion(1.5,Crypt)]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_Thumbnail_Filter>("Filter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// Thumbnail_DecodeParms 
/// </summary>
internal partial class APM_Thumbnail_DecodeParms : APM_Thumbnail_DecodeParms__Base
{
}


internal partial class APM_Thumbnail_DecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Thumbnail_DecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_Thumbnail_DecodeParms>(obj, "DecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_Thumbnail_DecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
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
                    } else if (APM_FilterCCITTFaxDecode.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_FilterCCITTFaxDecode, PdfDictionary>(stack, val, obj);
                    } else if (APM_FilterDCTDecode.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_FilterDCTDecode, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version >= 1.4m && APM_FilterJBIG2Decode.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_FilterJBIG2Decode, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version >= 1.5m && APM_FilterCrypt.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_FilterCrypt, PdfDictionary>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_Thumbnail_DecodeParms>("DecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,FilterCCITTFaxDecode,fn:SinceVersion(1.4,FilterJBIG2Decode),FilterDCTDecode,fn:SinceVersion(1.5,FilterCrypt)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_Thumbnail_DecodeParms>("DecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// Thumbnail_F 
/// </summary>
internal partial class APM_Thumbnail_F : APM_Thumbnail_F__Base
{
}


internal partial class APM_Thumbnail_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Thumbnail_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_Thumbnail_F>(obj, "F", IndirectRequirement.Either);
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
                ctx.Fail<APM_Thumbnail_F>("F is required to one of 'dictionary;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// Thumbnail_FFilter 
/// </summary>
internal partial class APM_Thumbnail_FFilter : APM_Thumbnail_FFilter__Base
{
}


internal partial class APM_Thumbnail_FFilter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Thumbnail_FFilter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_Thumbnail_FFilter>(obj, "FFilter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_Thumbnail_FFilter>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
                    }
                    // no value restrictions
                    ctx.Run<APM_ArrayOfFilterNames, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    
                    
                    if (!(val == PdfName.ASCIIHexDecode || val == PdfName.ASCII85Decode || val == PdfName.LZWDecode || val == PdfName.FlateDecode || val == PdfName.RunLengthDecode || val == PdfName.CCITTFaxDecode || (ctx.Version >= 1.4m && val == PdfName.JBIG2Decode) || val == PdfName.DCTDecode || (ctx.Version >= 1.5m && val == PdfName.JPXDecode) || (ctx.Version >= 1.5m && val == PdfName.Crypt))) 
                    {
                        ctx.Fail<APM_Thumbnail_FFilter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,CCITTFaxDecode,fn:SinceVersion(1.4,JBIG2Decode),DCTDecode,fn:SinceVersion(1.5,JPXDecode),fn:SinceVersion(1.5,Crypt)]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_Thumbnail_FFilter>("FFilter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// Thumbnail_FDecodeParms 
/// </summary>
internal partial class APM_Thumbnail_FDecodeParms : APM_Thumbnail_FDecodeParms__Base
{
}


internal partial class APM_Thumbnail_FDecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Thumbnail_FDecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_Thumbnail_FDecodeParms>(obj, "FDecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_Thumbnail_FDecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
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
                    } else if (APM_FilterCCITTFaxDecode.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_FilterCCITTFaxDecode, PdfDictionary>(stack, val, obj);
                    } else if (APM_FilterDCTDecode.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_FilterDCTDecode, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version >= 1.4m && APM_FilterJBIG2Decode.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_FilterJBIG2Decode, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version >= 1.5m && APM_FilterCrypt.MatchesType(ctx, val))) 
                    {
                        ctx.Run<APM_FilterCrypt, PdfDictionary>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_Thumbnail_FDecodeParms>("FDecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,FilterCCITTFaxDecode,fn:SinceVersion(1.4,FilterJBIG2Decode),FilterDCTDecode,fn:SinceVersion(1.5,FilterCrypt)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_Thumbnail_FDecodeParms>("FDecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// Thumbnail_DL 
/// </summary>
internal partial class APM_Thumbnail_DL : APM_Thumbnail_DL__Base
{
}


internal partial class APM_Thumbnail_DL__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Thumbnail_DL";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfIntNumber, APM_Thumbnail_DL>(obj, "DL", IndirectRequirement.Either);
        if (val == null) { return; }
        var DL = obj.Get("DL");
        if (!(gte(DL,0))) 
        {
            ctx.Fail<APM_Thumbnail_DL>($"Value failed special case check: fn:Eval(@DL>=0)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

