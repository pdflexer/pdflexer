// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_ObjectStream : APM_ObjectStream__Base
{
}

internal partial class APM_ObjectStream__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "ObjectStream";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_ObjectStream_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ObjectStream_N, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ObjectStream_First, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ObjectStream_Extends, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ObjectStream_Length, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ObjectStream_Filter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ObjectStream_DecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ObjectStream_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ObjectStream_FFilter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ObjectStream_FDecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ObjectStream_DL, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_ObjectStream>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_ObjectStream>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_ObjectStream>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_ObjectStream>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_ObjectStream>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_ObjectStream>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_ObjectStream_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "N", "First", "Extends", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "N", "First", "Extends", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "N", "First", "Extends", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "N", "First", "Extends", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "N", "First", "Extends", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "N", "First", "Extends", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    


}

/// <summary>
/// ObjectStream_Type Table 5 and Table 16
/// </summary>
internal partial class APM_ObjectStream_Type : APM_ObjectStream_Type__Base
{
}


internal partial class APM_ObjectStream_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ObjectStream_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_ObjectStream_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.ObjStm)) 
        {
            ctx.Fail<APM_ObjectStream_Type>($"Invalid value {val}, allowed are: [ObjStm]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ObjectStream_N 
/// </summary>
internal partial class APM_ObjectStream_N : APM_ObjectStream_N__Base
{
}


internal partial class APM_ObjectStream_N__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ObjectStream_N";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_ObjectStream_N>(obj, "N", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var N = obj.Get("N");
        if (!(gte(N,0))) 
        {
            ctx.Fail<APM_ObjectStream_N>($"Invalid value {val}, allowed are: [fn:Eval(@N>=0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ObjectStream_First 
/// </summary>
internal partial class APM_ObjectStream_First : APM_ObjectStream_First__Base
{
}


internal partial class APM_ObjectStream_First__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ObjectStream_First";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_ObjectStream_First>(obj, "First", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var First = obj.Get("First");
        if (!(gte(First,0))) 
        {
            ctx.Fail<APM_ObjectStream_First>($"Invalid value {val}, allowed are: [fn:Eval(@First>=0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ObjectStream_Extends forms a directed acyclic graph
/// </summary>
internal partial class APM_ObjectStream_Extends : APM_ObjectStream_Extends__Base
{
}


internal partial class APM_ObjectStream_Extends__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ObjectStream_Extends";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_ObjectStream_Extends>(obj, "Extends", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // TODO special case: fn:NoCycle()
        // no value restrictions
        ctx.Run<APM_ObjectStream, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// ObjectStream_Length 
/// </summary>
internal partial class APM_ObjectStream_Length : APM_ObjectStream_Length__Base
{
}


internal partial class APM_ObjectStream_Length__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ObjectStream_Length";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_ObjectStream_Length>(obj, "Length", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ObjectStream_Filter 
/// </summary>
internal partial class APM_ObjectStream_Filter : APM_ObjectStream_Filter__Base
{
}


internal partial class APM_ObjectStream_Filter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ObjectStream_Filter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ObjectStream_Filter>(obj, "Filter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_ObjectStream_Filter>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
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
                        ctx.Fail<APM_ObjectStream_Filter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,Crypt]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_ObjectStream_Filter>("Filter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ObjectStream_DecodeParms 
/// </summary>
internal partial class APM_ObjectStream_DecodeParms : APM_ObjectStream_DecodeParms__Base
{
}


internal partial class APM_ObjectStream_DecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ObjectStream_DecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ObjectStream_DecodeParms>(obj, "DecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_ObjectStream_DecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
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
                        ctx.Fail<APM_ObjectStream_DecodeParms>("DecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,FilterCrypt]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ObjectStream_DecodeParms>("DecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ObjectStream_F 
/// </summary>
internal partial class APM_ObjectStream_F : APM_ObjectStream_F__Base
{
}


internal partial class APM_ObjectStream_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ObjectStream_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ObjectStream_F>(obj, "F", IndirectRequirement.Either);
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
                ctx.Fail<APM_ObjectStream_F>("F is required to one of 'dictionary;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ObjectStream_FFilter 
/// </summary>
internal partial class APM_ObjectStream_FFilter : APM_ObjectStream_FFilter__Base
{
}


internal partial class APM_ObjectStream_FFilter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ObjectStream_FFilter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ObjectStream_FFilter>(obj, "FFilter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_ObjectStream_FFilter>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
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
                        ctx.Fail<APM_ObjectStream_FFilter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,Crypt]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_ObjectStream_FFilter>("FFilter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ObjectStream_FDecodeParms 
/// </summary>
internal partial class APM_ObjectStream_FDecodeParms : APM_ObjectStream_FDecodeParms__Base
{
}


internal partial class APM_ObjectStream_FDecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ObjectStream_FDecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ObjectStream_FDecodeParms>(obj, "FDecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_ObjectStream_FDecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
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
                        ctx.Fail<APM_ObjectStream_FDecodeParms>("FDecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,FilterCrypt]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_ObjectStream_FDecodeParms>("FDecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ObjectStream_DL 
/// </summary>
internal partial class APM_ObjectStream_DL : APM_ObjectStream_DL__Base
{
}


internal partial class APM_ObjectStream_DL__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ObjectStream_DL";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_ObjectStream_DL>(obj, "DL", IndirectRequirement.Either);
        if (val == null) { return; }
        var DL = obj.Get("DL");
        if (!(gte(DL,0))) 
        {
            ctx.Fail<APM_ObjectStream_DL>($"Value failed special case check: fn:Eval(@DL>=0)");
        }
        
        
        if (!(gte(DL,0))) 
        {
            ctx.Fail<APM_ObjectStream_DL>($"Invalid value {val}, allowed are: [fn:Eval(@DL>=0)]");
        }
        // no linked objects
        
    }


}
