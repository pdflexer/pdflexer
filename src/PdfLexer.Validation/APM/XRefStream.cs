// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_XRefStream : APM_XRefStream__Base
{
}

internal partial class APM_XRefStream__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "XRefStream";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_XRefStream_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XRefStream_Size, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XRefStream_Index, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XRefStream_Prev, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XRefStream_W, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XRefStream_Length, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XRefStream_Filter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XRefStream_DecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XRefStream_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XRefStream_FFilter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XRefStream_FDecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XRefStream_DL, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XRefStream_Root, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XRefStream_Info, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XRefStream_ID, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XRefStream_Encrypt, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XRefStream_AuthCode, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_XRefStream>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_XRefStream>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_XRefStream>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_XRefStream>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_XRefStream>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_XRefStream>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_XRefStream_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "Size", "Index", "Prev", "W", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL", "Root", "Info", "ID", "Encrypt"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "Size", "Index", "Prev", "W", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL", "Root", "Info", "ID", "Encrypt"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "Size", "Index", "Prev", "W", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL", "Root", "Info", "ID", "Encrypt"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "Size", "Index", "Prev", "W", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL", "Root", "Info", "ID", "Encrypt"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "Size", "Index", "Prev", "W", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL", "Root", "Info", "ID", "Encrypt"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "Size", "Index", "Prev", "W", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL", "Root", "Info", "ID", "Encrypt", "AuthCode"
    };
    


}

/// <summary>
/// XRefStream_Type Table 5 and Table 15 and Table 17
/// </summary>
internal partial class APM_XRefStream_Type : APM_XRefStream_Type__Base
{
}


internal partial class APM_XRefStream_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XRefStream_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_XRefStream_Type>(obj, "Type", IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "XRef")) 
        {
            ctx.Fail<APM_XRefStream_Type>($"Invalid value {val}, allowed are: [XRef]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// XRefStream_Size 
/// </summary>
internal partial class APM_XRefStream_Size : APM_XRefStream_Size__Base
{
}


internal partial class APM_XRefStream_Size__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XRefStream_Size";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_XRefStream_Size>(obj, "Size", IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        // no special cases
        
        var Size = obj.Get("Size");
        if (!(gt(Size,1))) 
        {
            ctx.Fail<APM_XRefStream_Size>($"Invalid value {val}, allowed are: [fn:Eval(@Size>1)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// XRefStream_Index Must be even array length. Object numbers can only be in one subsection.
/// </summary>
internal partial class APM_XRefStream_Index : APM_XRefStream_Index__Base
{
}


internal partial class APM_XRefStream_Index__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XRefStream_Index";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_XRefStream_Index>(obj, "Index", IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        var Index = obj.Get("Index");
        if (!((eq(mod(((Index as PdfArray)?.Count),2),0)&&ArraySortAscending(obj)))) 
        {
            ctx.Fail<APM_XRefStream_Index>($"Value failed special case check: fn:Eval(((fn:ArrayLength(Index) mod 2)==0) && fn:ArraySortAscending(Index,2))");
        }
        // no value restrictions
        ctx.Run<APM_ArrayOfXRefIndexIntegers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// XRefStream_Prev 
/// </summary>
internal partial class APM_XRefStream_Prev : APM_XRefStream_Prev__Base
{
}


internal partial class APM_XRefStream_Prev__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XRefStream_Prev";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_XRefStream_Prev>(obj, "Prev", IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        // no special cases
        
        var Prev = obj.Get("Prev");
        if (!((gte(Prev,0)&&lte(Prev,ctx.FileSize)))) 
        {
            ctx.Fail<APM_XRefStream_Prev>($"Invalid value {val}, allowed are: [fn:Eval((@Prev>=0) && (@Prev<=fn:FileSize()))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// XRefStream_W 
/// </summary>
internal partial class APM_XRefStream_W : APM_XRefStream_W__Base
{
}


internal partial class APM_XRefStream_W__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XRefStream_W";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_XRefStream_W>(obj, "W", IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfXRefWIntegers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// XRefStream_Length 
/// </summary>
internal partial class APM_XRefStream_Length : APM_XRefStream_Length__Base
{
}


internal partial class APM_XRefStream_Length__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XRefStream_Length";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_XRefStream_Length>(obj, "Length", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var Length = obj.Get("Length");
        if (!(gte(Length,0))) 
        {
            ctx.Fail<APM_XRefStream_Length>($"Invalid value {val}, allowed are: [fn:Eval(@Length>=0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// XRefStream_Filter Table 6
/// </summary>
internal partial class APM_XRefStream_Filter : APM_XRefStream_Filter__Base
{
}


internal partial class APM_XRefStream_Filter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XRefStream_Filter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_XRefStream_Filter>(obj, "Filter", IndirectRequirement.MustBeDirect);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    if (false && !wasIR) {
                        ctx.Fail<APM_XRefStream_Filter>("Filter is required to be indirect when a array");
                        return;
                    }
                    var DecodeParms = obj.Get("DecodeParms");
                    var Filter = obj.Get("Filter");
                    var Filter2 = obj.Get("Filter");
                    if (!((eq(((DecodeParms as PdfArray)?.Count),((Filter2 as PdfArray)?.Count))&&!Contains(Filter2, "Crypt")))) 
                    {
                        ctx.Fail<APM_XRefStream_Filter>($"Value failed special case check: fn:Eval((fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter)) && (fn:Not(fn:Contains(@Filter,Crypt))))");
                    }
                    // no value restrictions
                    ctx.Run<APM_ArrayOfCompressionFilterNames, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    if (false && !wasIR) {
                        ctx.Fail<APM_XRefStream_Filter>("Filter is required to be indirect when a name");
                        return;
                    }
                    // no special cases
                    
                    
                    if (!(val == "ASCIIHexDecode" || val == "ASCII85Decode" || val == "LZWDecode" || val == "FlateDecode" || val == "RunLengthDecode")) 
                    {
                        ctx.Fail<APM_XRefStream_Filter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_XRefStream_Filter>("Filter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// XRefStream_DecodeParms 
/// </summary>
internal partial class APM_XRefStream_DecodeParms : APM_XRefStream_DecodeParms__Base
{
}


internal partial class APM_XRefStream_DecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XRefStream_DecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_XRefStream_DecodeParms>(obj, "DecodeParms", IndirectRequirement.MustBeDirect);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    if (false && !wasIR) {
                        ctx.Fail<APM_XRefStream_DecodeParms>("DecodeParms is required to be indirect when a array");
                        return;
                    }
                    var DecodeParms = obj.Get("DecodeParms");
                    var Filter = obj.Get("Filter");
                    if (!(eq(((DecodeParms as PdfArray)?.Count),((Filter as PdfArray)?.Count)))) 
                    {
                        ctx.Fail<APM_XRefStream_DecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
                    }
                    // no value restrictions
                    ctx.Run<APM_ArrayOfDecodeParams, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    if (false && !wasIR) {
                        ctx.Fail<APM_XRefStream_DecodeParms>("DecodeParms is required to be indirect when a dictionary");
                        return;
                    }
                    // no special cases
                    // no value restrictions
                    if (APM_FilterLZWDecode.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_FilterLZWDecode, PdfDictionary>(stack, val, obj);
                    } else if (APM_FilterFlateDecode.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_FilterFlateDecode, PdfDictionary>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_XRefStream_DecodeParms>("DecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_XRefStream_DecodeParms>("DecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// XRefStream_F 
/// </summary>
internal partial class APM_XRefStream_F : APM_XRefStream_F__Base
{
}


internal partial class APM_XRefStream_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XRefStream_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_XRefStream_F>(obj, "F", IndirectRequirement.Either);
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
                    
                    if (!(AlwaysUnencrypted(obj))) 
                    {
                        ctx.Fail<APM_XRefStream_F>($"Value failed special case check: fn:Eval(fn:AlwaysUnencrypted())");
                    }
                    // no value restrictions
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_XRefStream_F>("F is required to one of 'dictionary;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// XRefStream_FFilter Table 6
/// </summary>
internal partial class APM_XRefStream_FFilter : APM_XRefStream_FFilter__Base
{
}


internal partial class APM_XRefStream_FFilter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XRefStream_FFilter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_XRefStream_FFilter>(obj, "FFilter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_XRefStream_FFilter>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
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
                    
                    
                    if (!(val == "ASCIIHexDecode" || val == "ASCII85Decode" || val == "LZWDecode" || val == "FlateDecode" || val == "RunLengthDecode")) 
                    {
                        ctx.Fail<APM_XRefStream_FFilter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_XRefStream_FFilter>("FFilter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// XRefStream_FDecodeParms 
/// </summary>
internal partial class APM_XRefStream_FDecodeParms : APM_XRefStream_FDecodeParms__Base
{
}


internal partial class APM_XRefStream_FDecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XRefStream_FDecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_XRefStream_FDecodeParms>(obj, "FDecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_XRefStream_FDecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
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
                    }else 
                    {
                        ctx.Fail<APM_XRefStream_FDecodeParms>("FDecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_XRefStream_FDecodeParms>("FDecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// XRefStream_DL 
/// </summary>
internal partial class APM_XRefStream_DL : APM_XRefStream_DL__Base
{
}


internal partial class APM_XRefStream_DL__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XRefStream_DL";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_XRefStream_DL>(obj, "DL", IndirectRequirement.Either);
        if (val == null) { return; }
        var DL = obj.Get("DL");
        if (!(gte(DL,0))) 
        {
            ctx.Fail<APM_XRefStream_DL>($"Value failed special case check: fn:Eval(@DL>=0)");
        }
        
        
        if (!(gte(DL,0))) 
        {
            ctx.Fail<APM_XRefStream_DL>($"Invalid value {val}, allowed are: [fn:Eval(@DL>=0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// XRefStream_Root 
/// </summary>
internal partial class APM_XRefStream_Root : APM_XRefStream_Root__Base
{
}


internal partial class APM_XRefStream_Root__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XRefStream_Root";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_XRefStream_Root>(obj, "Root", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Catalog, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// XRefStream_Info https://github.com/pdf-association/pdf-issues/issues/106
/// </summary>
internal partial class APM_XRefStream_Info : APM_XRefStream_Info__Base
{
}


internal partial class APM_XRefStream_Info__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XRefStream_Info";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_XRefStream_Info>(obj, "Info", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_DocInfo, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// XRefStream_ID 
/// </summary>
internal partial class APM_XRefStream_ID : APM_XRefStream_ID__Base
{
}


internal partial class APM_XRefStream_ID__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XRefStream_ID";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfArray, APM_XRefStream_ID>(obj, "ID", IndirectRequirement.Either);
        if (((ctx.Version >= 2.0m||obj.ContainsKey("Encrypt"))) && val == null) {
            ctx.Fail<APM_XRefStream_ID>("ID is required when 'fn:IsRequired(fn:SinceVersion(2.0) || fn:IsPresent(Encrypt))"); return;
        } else if (val == null) {
            return;
        }
        var ID0 = val.Get(0);
        var ID1 = val.Get(1);
        if (!((MustBeDirect(ID0)&&MustBeDirect(ID1)))) 
        {
            ctx.Fail<APM_XRefStream_ID>($"Value failed special case check: fn:Eval(fn:MustBeDirect(ID::0) && fn:MustBeDirect(ID::1))");
        }
        // no value restrictions
        ctx.Run<APM_ArrayOf_2StringsByte, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// XRefStream_Encrypt 
/// </summary>
internal partial class APM_XRefStream_Encrypt : APM_XRefStream_Encrypt__Base
{
}


internal partial class APM_XRefStream_Encrypt__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XRefStream_Encrypt";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_XRefStream_Encrypt>(obj, "Encrypt", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_EncryptionStandard.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_EncryptionStandard, PdfDictionary>(stack, val, obj);
        } else if (APM_EncryptionPublicKey.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_EncryptionPublicKey, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_XRefStream_Encrypt>("Encrypt did not match any allowable types: '[EncryptionStandard,EncryptionPublicKey]'");
        }
        
    }


}

/// <summary>
/// XRefStream_AuthCode ISO/TS 32004 integrity protection
/// </summary>
internal partial class APM_XRefStream_AuthCode : APM_XRefStream_AuthCode__Base
{
}


internal partial class APM_XRefStream_AuthCode__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XRefStream_AuthCode";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_XRefStream_AuthCode>(obj, "AuthCode", IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        var EncryptV = obj.Get("Encrypt")?.Get("V");
        if (!(gte(EncryptV,5))) 
        {
            ctx.Fail<APM_XRefStream_AuthCode>($"Value failed special case check: fn:Eval(Encrypt::@V>=5)");
        }
        // no value restrictions
        ctx.Run<APM_AuthCode, PdfDictionary>(stack, val, obj);
        
    }


}

