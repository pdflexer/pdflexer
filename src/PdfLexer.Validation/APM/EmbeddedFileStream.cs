// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_EmbeddedFileStream : APM_EmbeddedFileStream__Base
{
}

internal partial class APM_EmbeddedFileStream__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "EmbeddedFileStream";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_EmbeddedFileStream_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EmbeddedFileStream_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EmbeddedFileStream_Params, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EmbeddedFileStream_Length, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EmbeddedFileStream_Filter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EmbeddedFileStream_DecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EmbeddedFileStream_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EmbeddedFileStream_FFilter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EmbeddedFileStream_FDecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EmbeddedFileStream_DL, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.3m:
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13_14.Contains(x)))
                {
                    ctx.Fail<APM_EmbeddedFileStream>($"Unknown field {extra} for version {ctx.Version}");
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
                    ctx.Fail<APM_EmbeddedFileStream>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_EmbeddedFileStream_Type, PdfDictionary>(new CallStack(), obj, null);
        c.Run<APM_EmbeddedFileStream_Subtype, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_13_14 { get; } = new HashSet<string> 
    {
        "DecodeParms", "F", "FDecodeParms", "FFilter", "Filter", "Length", "Params", "Subtype", "Type"
    };
    public static HashSet<string> AllowedFields_15_16_17_18_19_20 { get; } = new HashSet<string> 
    {
        "DecodeParms", "DL", "F", "FDecodeParms", "FFilter", "Filter", "Length", "Params", "Subtype", "Type"
    };
    


}

/// <summary>
/// EmbeddedFileStream_Type Table 5 and Table 44
/// </summary>
internal partial class APM_EmbeddedFileStream_Type : APM_EmbeddedFileStream_Type__Base
{
}


internal partial class APM_EmbeddedFileStream_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EmbeddedFileStream_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_EmbeddedFileStream_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.EmbeddedFile)) 
        {
            ctx.Fail<APM_EmbeddedFileStream_Type>($"Invalid value {val}, allowed are: [EmbeddedFile]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// EmbeddedFileStream_Subtype https://github.com/pdf-association/pdf-issues/issues/156 - also check every page/Annots/RichMediaContent/Assets
/// </summary>
internal partial class APM_EmbeddedFileStream_Subtype : APM_EmbeddedFileStream_Subtype__Base
{
}


internal partial class APM_EmbeddedFileStream_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EmbeddedFileStream_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_EmbeddedFileStream_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (((ctx.Version >= 2.0m && IsAssociatedFile(obj))) && val == null) {
            ctx.Fail<APM_EmbeddedFileStream_Subtype>("Subtype is required when 'fn:IsRequired(fn:SinceVersion(2.0,fn:IsAssociatedFile()))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// EmbeddedFileStream_Params 
/// </summary>
internal partial class APM_EmbeddedFileStream_Params : APM_EmbeddedFileStream_Params__Base
{
}


internal partial class APM_EmbeddedFileStream_Params__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EmbeddedFileStream_Params";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_EmbeddedFileStream_Params>(obj, "Params", IndirectRequirement.Either);
        if (((ctx.Version >= 2.0m && IsAssociatedFile(obj))) && val == null) {
            ctx.Fail<APM_EmbeddedFileStream_Params>("Params is required when 'fn:IsRequired(fn:SinceVersion(2.0,fn:IsAssociatedFile()))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        ctx.Run<APM_EmbeddedFileParameter, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// EmbeddedFileStream_Length 
/// </summary>
internal partial class APM_EmbeddedFileStream_Length : APM_EmbeddedFileStream_Length__Base
{
}


internal partial class APM_EmbeddedFileStream_Length__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EmbeddedFileStream_Length";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_EmbeddedFileStream_Length>(obj, "Length", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// EmbeddedFileStream_Filter 
/// </summary>
internal partial class APM_EmbeddedFileStream_Filter : APM_EmbeddedFileStream_Filter__Base
{
}


internal partial class APM_EmbeddedFileStream_Filter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EmbeddedFileStream_Filter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_EmbeddedFileStream_Filter>(obj, "Filter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_EmbeddedFileStream_Filter>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
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
                        ctx.Fail<APM_EmbeddedFileStream_Filter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,CCITTFaxDecode,fn:SinceVersion(1.4,JBIG2Decode),DCTDecode,fn:SinceVersion(1.5,JPXDecode),fn:SinceVersion(1.5,Crypt)]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_EmbeddedFileStream_Filter>("Filter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// EmbeddedFileStream_DecodeParms 
/// </summary>
internal partial class APM_EmbeddedFileStream_DecodeParms : APM_EmbeddedFileStream_DecodeParms__Base
{
}


internal partial class APM_EmbeddedFileStream_DecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EmbeddedFileStream_DecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_EmbeddedFileStream_DecodeParms>(obj, "DecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_EmbeddedFileStream_DecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
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
                        ctx.Fail<APM_EmbeddedFileStream_DecodeParms>("DecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,FilterCCITTFaxDecode,fn:SinceVersion(1.4,FilterJBIG2Decode),FilterDCTDecode,fn:SinceVersion(1.5,FilterCrypt)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_EmbeddedFileStream_DecodeParms>("DecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// EmbeddedFileStream_F 
/// </summary>
internal partial class APM_EmbeddedFileStream_F : APM_EmbeddedFileStream_F__Base
{
}


internal partial class APM_EmbeddedFileStream_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EmbeddedFileStream_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_EmbeddedFileStream_F>(obj, "F", IndirectRequirement.Either);
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
                ctx.Fail<APM_EmbeddedFileStream_F>("F is required to one of 'dictionary;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// EmbeddedFileStream_FFilter 
/// </summary>
internal partial class APM_EmbeddedFileStream_FFilter : APM_EmbeddedFileStream_FFilter__Base
{
}


internal partial class APM_EmbeddedFileStream_FFilter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EmbeddedFileStream_FFilter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_EmbeddedFileStream_FFilter>(obj, "FFilter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_EmbeddedFileStream_FFilter>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
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
                        ctx.Fail<APM_EmbeddedFileStream_FFilter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,CCITTFaxDecode,fn:SinceVersion(1.4,JBIG2Decode),DCTDecode,fn:SinceVersion(1.5,JPXDecode),fn:SinceVersion(1.5,Crypt)]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_EmbeddedFileStream_FFilter>("FFilter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// EmbeddedFileStream_FDecodeParms 
/// </summary>
internal partial class APM_EmbeddedFileStream_FDecodeParms : APM_EmbeddedFileStream_FDecodeParms__Base
{
}


internal partial class APM_EmbeddedFileStream_FDecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EmbeddedFileStream_FDecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_EmbeddedFileStream_FDecodeParms>(obj, "FDecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_EmbeddedFileStream_FDecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
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
                        ctx.Fail<APM_EmbeddedFileStream_FDecodeParms>("FDecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,FilterCCITTFaxDecode,fn:SinceVersion(1.4,FilterJBIG2Decode),FilterDCTDecode,fn:SinceVersion(1.5,FilterCrypt)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_EmbeddedFileStream_FDecodeParms>("FDecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// EmbeddedFileStream_DL 
/// </summary>
internal partial class APM_EmbeddedFileStream_DL : APM_EmbeddedFileStream_DL__Base
{
}


internal partial class APM_EmbeddedFileStream_DL__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EmbeddedFileStream_DL";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfIntNumber, APM_EmbeddedFileStream_DL>(obj, "DL", IndirectRequirement.Either);
        if (val == null) { return; }
        var DL = obj.Get("DL");
        if (!(gte(DL,0))) 
        {
            ctx.Fail<APM_EmbeddedFileStream_DL>($"Value failed special case check: fn:Eval(@DL>=0)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

