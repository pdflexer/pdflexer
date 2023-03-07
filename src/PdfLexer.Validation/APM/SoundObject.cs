// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_SoundObject : APM_SoundObject__Base
{
}

internal partial class APM_SoundObject__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "SoundObject";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_SoundObject_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoundObject_R, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoundObject_C, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoundObject_B, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoundObject_E, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoundObject_CO, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoundObject_CP, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoundObject_Length, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoundObject_Filter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoundObject_DecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoundObject_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoundObject_FFilter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoundObject_FDecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoundObject_DL, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_SoundObject>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_SoundObject>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_SoundObject>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_SoundObject>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_SoundObject>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_SoundObject>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_SoundObject>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_SoundObject>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_SoundObject>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_SoundObject_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "R", "C", "B", "E", "CO", "CP", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "R", "C", "B", "E", "CO", "CP", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "R", "C", "B", "E", "CO", "CP", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "R", "C", "B", "E", "CO", "CP", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "R", "C", "B", "E", "CO", "CP", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "R", "C", "B", "E", "CO", "CP", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "R", "C", "B", "E", "CO", "CP", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "R", "C", "B", "E", "CO", "CP", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    


}

/// <summary>
/// SoundObject_Type Table 5 and Table 305
/// </summary>
internal partial class APM_SoundObject_Type : APM_SoundObject_Type__Base
{
}


internal partial class APM_SoundObject_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoundObject_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_SoundObject_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Sound)) 
        {
            ctx.Fail<APM_SoundObject_Type>($"Invalid value {val}, allowed are: [Sound]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// SoundObject_R sampling per second
/// </summary>
internal partial class APM_SoundObject_R : APM_SoundObject_R__Base
{
}


internal partial class APM_SoundObject_R__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoundObject_R";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_SoundObject_R>(obj, "R", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var R = obj.Get("R");
        if (!(gt(R,0))) 
        {
            ctx.Fail<APM_SoundObject_R>($"Invalid value {val}, allowed are: [fn:Eval(@R>0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// SoundObject_C number of sound channels
/// </summary>
internal partial class APM_SoundObject_C : APM_SoundObject_C__Base
{
}


internal partial class APM_SoundObject_C__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoundObject_C";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_SoundObject_C>(obj, "C", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var C = obj.Get("C");
        if (!(gt(C,0))) 
        {
            ctx.Fail<APM_SoundObject_C>($"Invalid value {val}, allowed are: [fn:Eval(@C>0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// SoundObject_B bits per sample value per channel
/// </summary>
internal partial class APM_SoundObject_B : APM_SoundObject_B__Base
{
}


internal partial class APM_SoundObject_B__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoundObject_B";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_SoundObject_B>(obj, "B", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var B = obj.Get("B");
        if (!(gt(B,0))) 
        {
            ctx.Fail<APM_SoundObject_B>($"Invalid value {val}, allowed are: [fn:Eval(@B>0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// SoundObject_E 
/// </summary>
internal partial class APM_SoundObject_E : APM_SoundObject_E__Base
{
}


internal partial class APM_SoundObject_E__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoundObject_E";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_SoundObject_E>(obj, "E", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Raw || val == PdfName.Signed || val == PdfName.muLaw || val == PdfName.ALaw)) 
        {
            ctx.Fail<APM_SoundObject_E>($"Invalid value {val}, allowed are: [Raw,Signed,muLaw,ALaw]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// SoundObject_CO 
/// </summary>
internal partial class APM_SoundObject_CO : APM_SoundObject_CO__Base
{
}


internal partial class APM_SoundObject_CO__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoundObject_CO";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_SoundObject_CO>(obj, "CO", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// SoundObject_CP various
/// </summary>
internal partial class APM_SoundObject_CP : APM_SoundObject_CP__Base
{
}


internal partial class APM_SoundObject_CP__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoundObject_CP";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_SoundObject_CP>(obj, "CP", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM__UniversalArray, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM__UniversalDictionary, PdfDictionary>(stack, val, obj);
                    return;
                }
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_SoundObject_CP>("CP is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
                    return;
                }
            
            default:
                ctx.Fail<APM_SoundObject_CP>("CP is required to one of 'array;dictionary;stream', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// SoundObject_Length 
/// </summary>
internal partial class APM_SoundObject_Length : APM_SoundObject_Length__Base
{
}


internal partial class APM_SoundObject_Length__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoundObject_Length";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_SoundObject_Length>(obj, "Length", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// SoundObject_Filter 
/// </summary>
internal partial class APM_SoundObject_Filter : APM_SoundObject_Filter__Base
{
}


internal partial class APM_SoundObject_Filter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoundObject_Filter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_SoundObject_Filter>(obj, "Filter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_SoundObject_Filter>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
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
                        ctx.Fail<APM_SoundObject_Filter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,fn:SinceVersion(1.5,Crypt)]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_SoundObject_Filter>("Filter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// SoundObject_DecodeParms 
/// </summary>
internal partial class APM_SoundObject_DecodeParms : APM_SoundObject_DecodeParms__Base
{
}


internal partial class APM_SoundObject_DecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoundObject_DecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_SoundObject_DecodeParms>(obj, "DecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_SoundObject_DecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(DecodeParms)==fn:ArrayLength(Filter))");
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
                        ctx.Fail<APM_SoundObject_DecodeParms>("DecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,fn:SinceVersion(1.5,FilterCrypt)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_SoundObject_DecodeParms>("DecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// SoundObject_F 
/// </summary>
internal partial class APM_SoundObject_F : APM_SoundObject_F__Base
{
}


internal partial class APM_SoundObject_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoundObject_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_SoundObject_F>(obj, "F", IndirectRequirement.Either);
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
                ctx.Fail<APM_SoundObject_F>("F is required to one of 'dictionary;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// SoundObject_FFilter 
/// </summary>
internal partial class APM_SoundObject_FFilter : APM_SoundObject_FFilter__Base
{
}


internal partial class APM_SoundObject_FFilter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoundObject_FFilter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_SoundObject_FFilter>(obj, "FFilter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_SoundObject_FFilter>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
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
                        ctx.Fail<APM_SoundObject_FFilter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,fn:SinceVersion(1.5,Crypt)]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_SoundObject_FFilter>("FFilter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// SoundObject_FDecodeParms 
/// </summary>
internal partial class APM_SoundObject_FDecodeParms : APM_SoundObject_FDecodeParms__Base
{
}


internal partial class APM_SoundObject_FDecodeParms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoundObject_FDecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_SoundObject_FDecodeParms>(obj, "FDecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_SoundObject_FDecodeParms>($"Value failed special case check: fn:Eval(fn:ArrayLength(FDecodeParms)==fn:ArrayLength(FFilter))");
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
                        ctx.Fail<APM_SoundObject_FDecodeParms>("FDecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,fn:SinceVersion(1.5,FilterCrypt)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_SoundObject_FDecodeParms>("FDecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// SoundObject_DL 
/// </summary>
internal partial class APM_SoundObject_DL : APM_SoundObject_DL__Base
{
}


internal partial class APM_SoundObject_DL__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoundObject_DL";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_SoundObject_DL>(obj, "DL", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var DL = obj.Get("DL");
        if (!(gte(DL,0))) 
        {
            ctx.Fail<APM_SoundObject_DL>($"Invalid value {val}, allowed are: [fn:Eval(@DL>=0)]");
        }
        // no linked objects
        
    }


}

