// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_XObjectFormPSpassthrough : APM_XObjectFormPSpassthrough__Base
{
}

internal partial class APM_XObjectFormPSpassthrough__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "XObjectFormPSpassthrough";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_XObjectFormPSpassthrough_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPSpassthrough_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPSpassthrough_Subtype2, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPSpassthrough_Level1, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPSpassthrough_PS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPSpassthrough_FormType, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPSpassthrough_BBox, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPSpassthrough_Matrix, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPSpassthrough_Length, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPSpassthrough_Filter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPSpassthrough_DecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPSpassthrough_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPSpassthrough_FFilter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_XObjectFormPSpassthrough_FDecodeParms, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.1m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_11.Contains(x)))
                {
                    ctx.Fail<APM_XObjectFormPSpassthrough>($"Unknown field {extra} for version 1.1");
                }
                break;
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_XObjectFormPSpassthrough>($"Unknown field {extra} for version 1.2");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_XObjectFormPSpassthrough_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_11 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Subtype2", "Level1", "PS", "FormType", "BBox", "Matrix", "Length", "Filter", "DecodeParms", "F"
    };
    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Subtype2", "Level1", "PS", "FormType", "BBox", "Matrix", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms"
    };
    


}

/// <summary>
/// XObjectFormPSpassthrough_Type Table 5 and 
/// </summary>
internal partial class APM_XObjectFormPSpassthrough_Type : APM_XObjectFormPSpassthrough_Type__Base
{
}


internal partial class APM_XObjectFormPSpassthrough_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPSpassthrough_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_XObjectFormPSpassthrough_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "XObject")) 
        {
            ctx.Fail<APM_XObjectFormPSpassthrough_Type>($"Invalid value {val}, allowed are: [XObject]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// XObjectFormPSpassthrough_Subtype 
/// </summary>
internal partial class APM_XObjectFormPSpassthrough_Subtype : APM_XObjectFormPSpassthrough_Subtype__Base
{
}


internal partial class APM_XObjectFormPSpassthrough_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPSpassthrough_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_XObjectFormPSpassthrough_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "PS")) 
        {
            ctx.Fail<APM_XObjectFormPSpassthrough_Subtype>($"Invalid value {val}, allowed are: [PS]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// XObjectFormPSpassthrough_Subtype2 
/// </summary>
internal partial class APM_XObjectFormPSpassthrough_Subtype2 : APM_XObjectFormPSpassthrough_Subtype2__Base
{
}


internal partial class APM_XObjectFormPSpassthrough_Subtype2__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPSpassthrough_Subtype2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_XObjectFormPSpassthrough_Subtype2>(obj, "Subtype2", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "PS")) 
        {
            ctx.Fail<APM_XObjectFormPSpassthrough_Subtype2>($"Invalid value {val}, allowed are: [PS]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// XObjectFormPSpassthrough_Level1 
/// </summary>
internal partial class APM_XObjectFormPSpassthrough_Level1 : APM_XObjectFormPSpassthrough_Level1__Base
{
}


internal partial class APM_XObjectFormPSpassthrough_Level1__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPSpassthrough_Level1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_XObjectFormPSpassthrough_Level1>(obj, "Level1", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// XObjectFormPSpassthrough_PS 
/// </summary>
internal partial class APM_XObjectFormPSpassthrough_PS : APM_XObjectFormPSpassthrough_PS__Base
{
}


internal partial class APM_XObjectFormPSpassthrough_PS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPSpassthrough_PS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfStream, APM_XObjectFormPSpassthrough_PS>(obj, "PS", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// XObjectFormPSpassthrough_FormType 
/// </summary>
internal partial class APM_XObjectFormPSpassthrough_FormType : APM_XObjectFormPSpassthrough_FormType__Base
{
}


internal partial class APM_XObjectFormPSpassthrough_FormType__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPSpassthrough_FormType";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfIntNumber, APM_XObjectFormPSpassthrough_FormType>(obj, "FormType", IndirectRequirement.Either);
        if ((ctx.Version < 1.3m) && val == null) {
            ctx.Fail<APM_XObjectFormPSpassthrough_FormType>("FormType is required when 'fn:IsRequired(fn:BeforeVersion(1.3))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        
        
        if (!(val == 1)) 
        {
            ctx.Fail<APM_XObjectFormPSpassthrough_FormType>($"Invalid value {val}, allowed are: [1]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// XObjectFormPSpassthrough_BBox 
/// </summary>
internal partial class APM_XObjectFormPSpassthrough_BBox : APM_XObjectFormPSpassthrough_BBox__Base
{
}


internal partial class APM_XObjectFormPSpassthrough_BBox__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPSpassthrough_BBox";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_XObjectFormPSpassthrough_BBox>(obj, "BBox", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// XObjectFormPSpassthrough_Matrix 
/// </summary>
internal partial class APM_XObjectFormPSpassthrough_Matrix : APM_XObjectFormPSpassthrough_Matrix__Base
{
}


internal partial class APM_XObjectFormPSpassthrough_Matrix__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPSpassthrough_Matrix";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfArray, APM_XObjectFormPSpassthrough_Matrix>(obj, "Matrix", IndirectRequirement.Either);
        if ((ctx.Version < 1.3m) && val == null) {
            ctx.Fail<APM_XObjectFormPSpassthrough_Matrix>("Matrix is required when 'fn:IsRequired(fn:BeforeVersion(1.3))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// XObjectFormPSpassthrough_Length 
/// </summary>
internal partial class APM_XObjectFormPSpassthrough_Length : APM_XObjectFormPSpassthrough_Length__Base
{
}


internal partial class APM_XObjectFormPSpassthrough_Length__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPSpassthrough_Length";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_XObjectFormPSpassthrough_Length>(obj, "Length", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// XObjectFormPSpassthrough_Filter 
/// </summary>
internal partial class APM_XObjectFormPSpassthrough_Filter : APM_XObjectFormPSpassthrough_Filter__Base
{
}


internal partial class APM_XObjectFormPSpassthrough_Filter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPSpassthrough_Filter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_XObjectFormPSpassthrough_Filter>(obj, "Filter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_XObjectFormPSpassthrough_Filter>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
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
                    
                    
                    if (!(val == "ASCIIHexDecode" || val == "ASCII85Decode" || val == "LZWDecode" || (ctx.Version < 1.2m || (ctx.Version >= 1.2m && val == "FlateDecode")) || val == "RunLengthDecode" || (ctx.Version < 1.5m || (ctx.Version >= 1.5m && val == "Crypt")))) 
                    {
                        ctx.Fail<APM_XObjectFormPSpassthrough_Filter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,fn:SinceVersion(1.2,FlateDecode),RunLengthDecode,fn:SinceVersion(1.5,Crypt)]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_XObjectFormPSpassthrough_Filter>("Filter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// XObjectFormPSpassthrough_DecodeParms 
/// </summary>
internal partial class APM_XObjectFormPSpassthrough_DecodeParms : APM_XObjectFormPSpassthrough_DecodeParms__Base
{
}


internal partial class APM_XObjectFormPSpassthrough_DecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPSpassthrough_DecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_XObjectFormPSpassthrough_DecodeParms>(obj, "DecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_XObjectFormPSpassthrough_DecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
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
                        ctx.Fail<APM_XObjectFormPSpassthrough_DecodeParms>("DecodeParms did not match any allowable types: '[FilterLZWDecode,fn:SinceVersion(1.2,FilterFlateDecode),fn:SinceVersion(1.5,FilterCrypt)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_XObjectFormPSpassthrough_DecodeParms>("DecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// XObjectFormPSpassthrough_F 
/// </summary>
internal partial class APM_XObjectFormPSpassthrough_F : APM_XObjectFormPSpassthrough_F__Base
{
}


internal partial class APM_XObjectFormPSpassthrough_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPSpassthrough_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_XObjectFormPSpassthrough_F>(obj, "F", IndirectRequirement.Either);
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
                ctx.Fail<APM_XObjectFormPSpassthrough_F>("F is required to one of 'dictionary;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// XObjectFormPSpassthrough_FFilter 
/// </summary>
internal partial class APM_XObjectFormPSpassthrough_FFilter : APM_XObjectFormPSpassthrough_FFilter__Base
{
}


internal partial class APM_XObjectFormPSpassthrough_FFilter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPSpassthrough_FFilter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_XObjectFormPSpassthrough_FFilter>(obj, "FFilter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_XObjectFormPSpassthrough_FFilter>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
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
                    
                    
                    if (!(val == "ASCIIHexDecode" || val == "ASCII85Decode" || val == "LZWDecode" || val == "FlateDecode" || val == "RunLengthDecode" || (ctx.Version < 1.5m || (ctx.Version >= 1.5m && val == "Crypt")))) 
                    {
                        ctx.Fail<APM_XObjectFormPSpassthrough_FFilter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,fn:SinceVersion(1.5,Crypt)]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_XObjectFormPSpassthrough_FFilter>("FFilter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// XObjectFormPSpassthrough_FDecodeParms 
/// </summary>
internal partial class APM_XObjectFormPSpassthrough_FDecodeParms : APM_XObjectFormPSpassthrough_FDecodeParms__Base
{
}


internal partial class APM_XObjectFormPSpassthrough_FDecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "XObjectFormPSpassthrough_FDecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_XObjectFormPSpassthrough_FDecodeParms>(obj, "FDecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_XObjectFormPSpassthrough_FDecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
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
                        ctx.Fail<APM_XObjectFormPSpassthrough_FDecodeParms>("FDecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_XObjectFormPSpassthrough_FDecodeParms>("FDecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

