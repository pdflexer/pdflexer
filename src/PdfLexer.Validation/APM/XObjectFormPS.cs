// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_XObjectFormPS : APM_XObjectFormPS__Base
{
}

internal partial class APM_XObjectFormPS__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "XObjectFormPS";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_XObjectFormPS_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPS_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPS_Level1, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPS_Length, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPS_Filter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPS_DecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPS_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPS_FFilter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPS_FDecodeParms, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.1m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_11.Contains(x)))
                {
                    ctx.Fail<APM_XObjectFormPS>($"Unknown field {extra} for version 1.1");
                }
                break;
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_XObjectFormPS>($"Unknown field {extra} for version 1.2");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_XObjectFormPS_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_11 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Level1", "Length", "Filter", "DecodeParms", "F"
    };
    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Level1", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms"
    };
    


}

/// <summary>
/// XObjectFormPS_Type Table 5 and
/// </summary>
internal partial class APM_XObjectFormPS_Type : APM_XObjectFormPS_Type__Base
{
}


internal partial class APM_XObjectFormPS_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPS_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_XObjectFormPS_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "XObject")) 
        {
            ctx.Fail<APM_XObjectFormPS_Type>($"Invalid value {val}, allowed are: [XObject]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// XObjectFormPS_Subtype 
/// </summary>
internal partial class APM_XObjectFormPS_Subtype : APM_XObjectFormPS_Subtype__Base
{
}


internal partial class APM_XObjectFormPS_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPS_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_XObjectFormPS_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "PS")) 
        {
            ctx.Fail<APM_XObjectFormPS_Subtype>($"Invalid value {val}, allowed are: [PS]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// XObjectFormPS_Level1 
/// </summary>
internal partial class APM_XObjectFormPS_Level1 : APM_XObjectFormPS_Level1__Base
{
}


internal partial class APM_XObjectFormPS_Level1__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPS_Level1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_XObjectFormPS_Level1>(obj, "Level1", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// XObjectFormPS_Length 
/// </summary>
internal partial class APM_XObjectFormPS_Length : APM_XObjectFormPS_Length__Base
{
}


internal partial class APM_XObjectFormPS_Length__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPS_Length";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_XObjectFormPS_Length>(obj, "Length", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// XObjectFormPS_Filter 
/// </summary>
internal partial class APM_XObjectFormPS_Filter : APM_XObjectFormPS_Filter__Base
{
}


internal partial class APM_XObjectFormPS_Filter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPS_Filter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_XObjectFormPS_Filter>(obj, "Filter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_XObjectFormPS_Filter>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
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
                    
                    
                    if (!(val == "ASCIIHexDecode" || val == "ASCII85Decode" || val == "LZWDecode" || (ctx.Version < 1.2m || (ctx.Version >= 1.2m && val == "FlateDecode")) || val == "RunLengthDecode")) 
                    {
                        ctx.Fail<APM_XObjectFormPS_Filter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,fn:SinceVersion(1.2,FlateDecode),RunLengthDecode]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_XObjectFormPS_Filter>("Filter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// XObjectFormPS_DecodeParms 
/// </summary>
internal partial class APM_XObjectFormPS_DecodeParms : APM_XObjectFormPS_DecodeParms__Base
{
}


internal partial class APM_XObjectFormPS_DecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPS_DecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_XObjectFormPS_DecodeParms>(obj, "DecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_XObjectFormPS_DecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
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
                    } else if ((ctx.Version < 1.2m || (ctx.Version >= 1.2m && APM_FilterFlateDecode.MatchesType(ctx, val)))) 
                    {
                        ctx.Run<APM_FilterFlateDecode, PdfDictionary>(stack, val, obj);
                    } else if ((ctx.Version < 1.5m || (ctx.Version >= 1.5m && APM_FilterCrypt.MatchesType(ctx, val)))) 
                    {
                        ctx.Run<APM_FilterCrypt, PdfDictionary>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_XObjectFormPS_DecodeParms>("DecodeParms did not match any allowable types: '[FilterLZWDecode,fn:SinceVersion(1.2,FilterFlateDecode),fn:SinceVersion(1.5,FilterCrypt)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_XObjectFormPS_DecodeParms>("DecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// XObjectFormPS_F 
/// </summary>
internal partial class APM_XObjectFormPS_F : APM_XObjectFormPS_F__Base
{
}


internal partial class APM_XObjectFormPS_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPS_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_XObjectFormPS_F>(obj, "F", IndirectRequirement.Either);
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
                ctx.Fail<APM_XObjectFormPS_F>("F is required to one of 'dictionary;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// XObjectFormPS_FFilter 
/// </summary>
internal partial class APM_XObjectFormPS_FFilter : APM_XObjectFormPS_FFilter__Base
{
}


internal partial class APM_XObjectFormPS_FFilter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPS_FFilter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_XObjectFormPS_FFilter>(obj, "FFilter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_XObjectFormPS_FFilter>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
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
                        ctx.Fail<APM_XObjectFormPS_FFilter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_XObjectFormPS_FFilter>("FFilter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// XObjectFormPS_FDecodeParms 
/// </summary>
internal partial class APM_XObjectFormPS_FDecodeParms : APM_XObjectFormPS_FDecodeParms__Base
{
}


internal partial class APM_XObjectFormPS_FDecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPS_FDecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_XObjectFormPS_FDecodeParms>(obj, "FDecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_XObjectFormPS_FDecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
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
                    } else if ((ctx.Version < 1.5m || (ctx.Version >= 1.5m && APM_FilterCrypt.MatchesType(ctx, val)))) 
                    {
                        ctx.Run<APM_FilterCrypt, PdfDictionary>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_XObjectFormPS_FDecodeParms>("FDecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,fn:SinceVersion(1.5,FilterCrypt)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_XObjectFormPS_FDecodeParms>("FDecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

