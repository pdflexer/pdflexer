// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_FontFile2 : APM_FontFile2__Base
{
}

internal partial class APM_FontFile2__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "FontFile2";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_FontFile2_Length1, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontFile2_Length2, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontFile2_Length3, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontFile2_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontFile2_Length, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontFile2_Filter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontFile2_DecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontFile2_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontFile2_FFilter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontFile2_FDecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontFile2_DL, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
            case 1.3m:
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12_13_14.Contains(x)))
                {
                    ctx.Fail<APM_FontFile2>($"Unknown field {extra} for version {ctx.Version}");
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
                    ctx.Fail<APM_FontFile2>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        
        c.Run<APM_FontFile2_Subtype, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_12_13_14 { get; } = new HashSet<string> 
    {
        "DecodeParms", "F", "FDecodeParms", "FFilter", "Filter", "Length", "Length1", "Length2", "Length3", "Subtype"
    };
    public static HashSet<string> AllowedFields_15_16_17_18_19_20 { get; } = new HashSet<string> 
    {
        "DecodeParms", "DL", "F", "FDecodeParms", "FFilter", "Filter", "Length", "Length1", "Length2", "Length3", "Subtype"
    };
    


}

/// <summary>
/// FontFile2_Length1 Table 5 and Table 125
/// </summary>
internal partial class APM_FontFile2_Length1 : APM_FontFile2_Length1__Base
{
}


internal partial class APM_FontFile2_Length1__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontFile2_Length1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var parentparentSubtype = stack.GetParent(2)?.Get("parent");
        var (val, wasIR) = ctx.GetOptional<PdfIntNumber, APM_FontFile2_Length1>(obj, "Length1", IndirectRequirement.Either);
        if ((eq(parentparentSubtype,PdfName.TrueType)) && val == null) {
            ctx.Fail<APM_FontFile2_Length1>("Length1 is required when 'fn:IsRequired(parent::parent::@Subtype==TrueType)"); return;
        } else if (val == null) {
            return;
        }
        var Length1 = obj.Get("Length1");
        if (!(gte(Length1,0))) 
        {
            ctx.Fail<APM_FontFile2_Length1>($"Value failed special case check: fn:Eval(@Length1>=0)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontFile2_Length2 
/// </summary>
internal partial class APM_FontFile2_Length2 : APM_FontFile2_Length2__Base
{
}


internal partial class APM_FontFile2_Length2__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontFile2_Length2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfIntNumber, APM_FontFile2_Length2>(obj, "Length2", IndirectRequirement.Either);
        if (val == null) { return; }
        var Length2 = obj.Get("Length2");
        if (!(gte(Length2,0))) 
        {
            ctx.Fail<APM_FontFile2_Length2>($"Value failed special case check: fn:Eval(@Length2>=0)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontFile2_Length3 
/// </summary>
internal partial class APM_FontFile2_Length3 : APM_FontFile2_Length3__Base
{
}


internal partial class APM_FontFile2_Length3__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontFile2_Length3";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfIntNumber, APM_FontFile2_Length3>(obj, "Length3", IndirectRequirement.Either);
        if (val == null) { return; }
        var Length3 = obj.Get("Length3");
        if (!(gte(Length3,0))) 
        {
            ctx.Fail<APM_FontFile2_Length3>($"Value failed special case check: fn:Eval(@Length3>=0)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontFile2_Subtype 
/// </summary>
internal partial class APM_FontFile2_Subtype : APM_FontFile2_Subtype__Base
{
}


internal partial class APM_FontFile2_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontFile2_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_FontFile2_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontFile2_Length 
/// </summary>
internal partial class APM_FontFile2_Length : APM_FontFile2_Length__Base
{
}


internal partial class APM_FontFile2_Length__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontFile2_Length";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfIntNumber, APM_FontFile2_Length>(obj, "Length", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontFile2_Filter 
/// </summary>
internal partial class APM_FontFile2_Filter : APM_FontFile2_Filter__Base
{
}


internal partial class APM_FontFile2_Filter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontFile2_Filter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_FontFile2_Filter>(obj, "Filter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_FontFile2_Filter>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
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
                        ctx.Fail<APM_FontFile2_Filter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,fn:SinceVersion(1.5,Crypt)]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_FontFile2_Filter>("Filter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// FontFile2_DecodeParms 
/// </summary>
internal partial class APM_FontFile2_DecodeParms : APM_FontFile2_DecodeParms__Base
{
}


internal partial class APM_FontFile2_DecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontFile2_DecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_FontFile2_DecodeParms>(obj, "DecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_FontFile2_DecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
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
                        ctx.Fail<APM_FontFile2_DecodeParms>("DecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,fn:SinceVersion(1.5,FilterCrypt)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_FontFile2_DecodeParms>("DecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// FontFile2_F 
/// </summary>
internal partial class APM_FontFile2_F : APM_FontFile2_F__Base
{
}


internal partial class APM_FontFile2_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontFile2_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_FontFile2_F>(obj, "F", IndirectRequirement.Either);
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
                ctx.Fail<APM_FontFile2_F>("F is required to one of 'dictionary;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// FontFile2_FFilter 
/// </summary>
internal partial class APM_FontFile2_FFilter : APM_FontFile2_FFilter__Base
{
}


internal partial class APM_FontFile2_FFilter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontFile2_FFilter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_FontFile2_FFilter>(obj, "FFilter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_FontFile2_FFilter>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
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
                        ctx.Fail<APM_FontFile2_FFilter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,fn:SinceVersion(1.5,Crypt)]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_FontFile2_FFilter>("FFilter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// FontFile2_FDecodeParms 
/// </summary>
internal partial class APM_FontFile2_FDecodeParms : APM_FontFile2_FDecodeParms__Base
{
}


internal partial class APM_FontFile2_FDecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontFile2_FDecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_FontFile2_FDecodeParms>(obj, "FDecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_FontFile2_FDecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
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
                        ctx.Fail<APM_FontFile2_FDecodeParms>("FDecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,fn:SinceVersion(1.5,FilterCrypt)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_FontFile2_FDecodeParms>("FDecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// FontFile2_DL 
/// </summary>
internal partial class APM_FontFile2_DL : APM_FontFile2_DL__Base
{
}


internal partial class APM_FontFile2_DL__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontFile2_DL";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfIntNumber, APM_FontFile2_DL>(obj, "DL", IndirectRequirement.Either);
        if (val == null) { return; }
        var DL = obj.Get("DL");
        if (!(gte(DL,0))) 
        {
            ctx.Fail<APM_FontFile2_DL>($"Value failed special case check: fn:Eval(@DL>=0)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

