// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_3DStream : APM_3DStream__Base
{
}

internal partial class APM_3DStream__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "3DStream";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_3DStream_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DStream_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DStream_VA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DStream_DV, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DStream_Resources, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DStream_OnInstantiate, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DStream_AN, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DStream_ColorSpace, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DStream_Length, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DStream_Filter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DStream_DecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DStream_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DStream_FFilter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DStream_FDecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DStream_DL, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_3DStream>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_3DStream>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_3DStream>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_3DStream>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_3DStream>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_3DStream_Type, PdfDictionary>(new CallStack(), obj, null);
        c.Run<APM_3DStream_Subtype, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "VA", "DV", "Resources", "OnInstantiate", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "VA", "DV", "Resources", "OnInstantiate", "AN", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "VA", "DV", "Resources", "OnInstantiate", "AN", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "VA", "DV", "Resources", "OnInstantiate", "AN", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "VA", "DV", "Resources", "OnInstantiate", "AN", "ColorSpace", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    


}

/// <summary>
/// 3DStream_Type Table 5 and Table 311
/// </summary>
internal partial class APM_3DStream_Type : APM_3DStream_Type__Base
{
}


internal partial class APM_3DStream_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DStream_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_3DStream_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.N3D)) 
        {
            ctx.Fail<APM_3DStream_Type>($"Invalid value {val}, allowed are: [3D]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// 3DStream_Subtype 
/// </summary>
internal partial class APM_3DStream_Subtype : APM_3DStream_Subtype__Base
{
}


internal partial class APM_3DStream_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DStream_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfName, APM_3DStream_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.U3D || val == PdfName.PRC || (ctx.Version < 2.0m || (ctx.Extensions.Contains(PdfName.ISO_TS_24064) && val == PdfName.STEP)))) 
        {
            ctx.Fail<APM_3DStream_Subtype>($"Invalid value {val}, allowed are: [U3D,PRC,fn:SinceVersion(2.0,fn:Extension(ISO_TS_24064,STEP))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// 3DStream_VA 
/// </summary>
internal partial class APM_3DStream_VA : APM_3DStream_VA__Base
{
}


internal partial class APM_3DStream_VA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DStream_VA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_3DStream_VA>(obj, "VA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf3DView, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DStream_DV 
/// </summary>
internal partial class APM_3DStream_DV : APM_3DStream_DV__Base
{
}


internal partial class APM_3DStream_DV__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DStream_DV";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_3DStream_DV>(obj, "DV", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_3DView, PdfDictionary>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NumericObj:
                {
                    var val =  (PdfIntNumber)utval;
                    // no indirect obj reqs
                    // no special cases
                    
                    var DV = obj.Get("DV");
                    var VA = obj.Get("VA");
                    if (!((gte(DV,0)&&lt(DV,((VA as PdfArray)?.Count))))) 
                    {
                        ctx.Fail<APM_3DStream_DV>($"Invalid value {val}, allowed are: [fn:Eval((@DV>=0) && (@DV<fn:ArrayLength(VA)))]");
                    }
                    // no linked objects
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    
                    
                    if (!(val == PdfName.F || val == PdfName.L)) 
                    {
                        ctx.Fail<APM_3DStream_DV>($"Invalid value {val}, allowed are: [F,L]");
                    }
                    // no linked objects
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
                ctx.Fail<APM_3DStream_DV>("DV is required to one of 'dictionary;integer;name;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// 3DStream_Resources 
/// </summary>
internal partial class APM_3DStream_Resources : APM_3DStream_Resources__Base
{
}


internal partial class APM_3DStream_Resources__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DStream_Resources";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_3DStream_Resources>(obj, "Resources", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // TODO trees
        
    }


}

/// <summary>
/// 3DStream_OnInstantiate 
/// </summary>
internal partial class APM_3DStream_OnInstantiate : APM_3DStream_OnInstantiate__Base
{
}


internal partial class APM_3DStream_OnInstantiate__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DStream_OnInstantiate";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfStream, APM_3DStream_OnInstantiate>(obj, "OnInstantiate", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// 3DStream_AN 
/// </summary>
internal partial class APM_3DStream_AN : APM_3DStream_AN__Base
{
}


internal partial class APM_3DStream_AN__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DStream_AN";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_3DStream_AN>(obj, "AN", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_3DAnimationStyle, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DStream_ColorSpace 
/// </summary>
internal partial class APM_3DStream_ColorSpace : APM_3DStream_ColorSpace__Base
{
}


internal partial class APM_3DStream_ColorSpace__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DStream_ColorSpace";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_3DStream_ColorSpace>(obj, "ColorSpace", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    if (APM_CalRGBColorSpace.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_CalRGBColorSpace, PdfArray>(stack, val, obj);
                    } else if (APM_ICCBasedColorSpace.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ICCBasedColorSpace, PdfArray>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_3DStream_ColorSpace>("ColorSpace did not match any allowable types: '[CalRGBColorSpace,ICCBasedColorSpace]'");
                    }
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    
                    
                    if (!(val == PdfName.DeviceRGB)) 
                    {
                        ctx.Fail<APM_3DStream_ColorSpace>($"Invalid value {val}, allowed are: [DeviceRGB]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_3DStream_ColorSpace>("ColorSpace is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// 3DStream_Length 
/// </summary>
internal partial class APM_3DStream_Length : APM_3DStream_Length__Base
{
}


internal partial class APM_3DStream_Length__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DStream_Length";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_3DStream_Length>(obj, "Length", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// 3DStream_Filter 
/// </summary>
internal partial class APM_3DStream_Filter : APM_3DStream_Filter__Base
{
}


internal partial class APM_3DStream_Filter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DStream_Filter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_3DStream_Filter>(obj, "Filter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_3DStream_Filter>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
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
                    
                    
                    if (!(val == PdfName.ASCIIHexDecode || val == PdfName.ASCII85Decode || val == PdfName.LZWDecode || val == PdfName.FlateDecode || val == PdfName.RunLengthDecode || val == PdfName.Crypt)) 
                    {
                        ctx.Fail<APM_3DStream_Filter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,Crypt]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_3DStream_Filter>("Filter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// 3DStream_DecodeParms 
/// </summary>
internal partial class APM_3DStream_DecodeParms : APM_3DStream_DecodeParms__Base
{
}


internal partial class APM_3DStream_DecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DStream_DecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_3DStream_DecodeParms>(obj, "DecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_3DStream_DecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
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
                    } else if (APM_FilterCrypt.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_FilterCrypt, PdfDictionary>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_3DStream_DecodeParms>("DecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,FilterCrypt]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_3DStream_DecodeParms>("DecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// 3DStream_F 
/// </summary>
internal partial class APM_3DStream_F : APM_3DStream_F__Base
{
}


internal partial class APM_3DStream_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DStream_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_3DStream_F>(obj, "F", IndirectRequirement.Either);
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
                ctx.Fail<APM_3DStream_F>("F is required to one of 'dictionary;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// 3DStream_FFilter 
/// </summary>
internal partial class APM_3DStream_FFilter : APM_3DStream_FFilter__Base
{
}


internal partial class APM_3DStream_FFilter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DStream_FFilter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_3DStream_FFilter>(obj, "FFilter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_3DStream_FFilter>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
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
                    
                    
                    if (!(val == PdfName.ASCIIHexDecode || val == PdfName.ASCII85Decode || val == PdfName.LZWDecode || val == PdfName.FlateDecode || val == PdfName.RunLengthDecode || val == PdfName.Crypt)) 
                    {
                        ctx.Fail<APM_3DStream_FFilter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,Crypt]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_3DStream_FFilter>("FFilter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// 3DStream_FDecodeParms 
/// </summary>
internal partial class APM_3DStream_FDecodeParms : APM_3DStream_FDecodeParms__Base
{
}


internal partial class APM_3DStream_FDecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DStream_FDecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_3DStream_FDecodeParms>(obj, "FDecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_3DStream_FDecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
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
                    } else if (APM_FilterCrypt.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_FilterCrypt, PdfDictionary>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_3DStream_FDecodeParms>("FDecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,FilterCrypt]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_3DStream_FDecodeParms>("FDecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// 3DStream_DL 
/// </summary>
internal partial class APM_3DStream_DL : APM_3DStream_DL__Base
{
}


internal partial class APM_3DStream_DL__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DStream_DL";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfIntNumber, APM_3DStream_DL>(obj, "DL", IndirectRequirement.Either);
        if (val == null) { return; }
        var DL = obj.Get("DL");
        if (!(gte(DL,0))) 
        {
            ctx.Fail<APM_3DStream_DL>($"Value failed special case check: fn:Eval(@DL>=0)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

