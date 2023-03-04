// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_HalftoneType16 : APM_HalftoneType16_Base
{
}

internal partial class APM_HalftoneType16_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "HalftoneType16";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_HalftoneType16_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType16_HalftoneType, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType16_HalftoneName, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType16_Width, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType16_Height, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType16_Width2, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType16_Height2, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType16_TransferFunction, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType16_Length, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType16_Filter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType16_DecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType16_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType16_FFilter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType16_FDecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType16_DL, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_HalftoneType16>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_HalftoneType16>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_HalftoneType16>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_HalftoneType16>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_HalftoneType16>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_HalftoneType16>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_HalftoneType16>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_HalftoneType16>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_HalftoneType16_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "HalftoneType", "HalftoneName", "Width", "Height", "Width2", "Height2", "TransferFunction", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "HalftoneType", "HalftoneName", "Width", "Height", "Width2", "Height2", "TransferFunction", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "HalftoneType", "HalftoneName", "Width", "Height", "Width2", "Height2", "TransferFunction", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "HalftoneType", "HalftoneName", "Width", "Height", "Width2", "Height2", "TransferFunction", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "HalftoneType", "HalftoneName", "Width", "Height", "Width2", "Height2", "TransferFunction", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "HalftoneType", "HalftoneName", "Width", "Height", "Width2", "Height2", "TransferFunction", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "HalftoneType", "HalftoneName", "Width", "Height", "Width2", "Height2", "TransferFunction", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "HalftoneType", "HalftoneName", "Width", "Height", "Width2", "Height2", "TransferFunction", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    


}

/// <summary>
/// HalftoneType16_Type Table 5 and Table 131
/// </summary>
internal partial class APM_HalftoneType16_Type : APM_HalftoneType16_Type_Base
{
}


internal partial class APM_HalftoneType16_Type_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType16_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_HalftoneType16_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "Halftone")) 
        {
            ctx.Fail<APM_HalftoneType16_Type>($"Invalid value {val}, allowed are: [Halftone]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// HalftoneType16_HalftoneType 
/// </summary>
internal partial class APM_HalftoneType16_HalftoneType : APM_HalftoneType16_HalftoneType_Base
{
}


internal partial class APM_HalftoneType16_HalftoneType_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType16_HalftoneType";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_HalftoneType16_HalftoneType>(obj, "HalftoneType", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == 16)) 
        {
            ctx.Fail<APM_HalftoneType16_HalftoneType>($"Invalid value {val}, allowed are: [16]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// HalftoneType16_HalftoneName 
/// </summary>
internal partial class APM_HalftoneType16_HalftoneName : APM_HalftoneType16_HalftoneName_Base
{
}


internal partial class APM_HalftoneType16_HalftoneName_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType16_HalftoneName";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_HalftoneType16_HalftoneName>(obj, "HalftoneName", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// HalftoneType16_Width 
/// </summary>
internal partial class APM_HalftoneType16_Width : APM_HalftoneType16_Width_Base
{
}


internal partial class APM_HalftoneType16_Width_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType16_Width";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_HalftoneType16_Width>(obj, "Width", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        IPdfObject @Width = val;
        if (!(gt(@Width,0))) 
        {
            ctx.Fail<APM_HalftoneType16_Width>($"Invalid value {val}, allowed are: [fn:Eval(@Width>0)]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// HalftoneType16_Height 
/// </summary>
internal partial class APM_HalftoneType16_Height : APM_HalftoneType16_Height_Base
{
}


internal partial class APM_HalftoneType16_Height_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType16_Height";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_HalftoneType16_Height>(obj, "Height", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        IPdfObject @Height = val;
        if (!(gt(@Height,0))) 
        {
            ctx.Fail<APM_HalftoneType16_Height>($"Invalid value {val}, allowed are: [fn:Eval(@Height>0)]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// HalftoneType16_Width2 
/// </summary>
internal partial class APM_HalftoneType16_Width2 : APM_HalftoneType16_Width2_Base
{
}


internal partial class APM_HalftoneType16_Width2_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType16_Width2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_HalftoneType16_Width2>(obj, "Width2", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        IPdfObject @Width2 = val;
        if (!(gt(@Width2,0))) 
        {
            ctx.Fail<APM_HalftoneType16_Width2>($"Invalid value {val}, allowed are: [fn:Eval(@Width2>0)]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// HalftoneType16_Height2 
/// </summary>
internal partial class APM_HalftoneType16_Height2 : APM_HalftoneType16_Height2_Base
{
}


internal partial class APM_HalftoneType16_Height2_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType16_Height2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_HalftoneType16_Height2>(obj, "Height2", IndirectRequirement.Either);
        if (val == null) { return; }
        // TODO special case
        {
        
        IPdfObject @Height2 = val;
        if (!(gt(@Height2,0))) 
        {
            ctx.Fail<APM_HalftoneType16_Height2>($"Invalid value {val}, allowed are: [fn:Eval(@Height2>0)]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// HalftoneType16_TransferFunction 
/// </summary>
internal partial class APM_HalftoneType16_TransferFunction : APM_HalftoneType16_TransferFunction_Base
{
}


internal partial class APM_HalftoneType16_TransferFunction_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType16_TransferFunction";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_HalftoneType16_TransferFunction>(obj, "TransferFunction", IndirectRequirement.Either);
        {
            var parentHalftoneType = parent?.Get("@HalftoneType");
            if ((eq(parentHalftoneType,5)) && utval == null) {
                ctx.Fail<APM_HalftoneType16_TransferFunction>("TransferFunction is required"); return;
            } else if (utval == null) {
                return;
            }
        }
        switch (utval.Type) 
        {
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
                        ctx.Fail<APM_HalftoneType16_TransferFunction>("TransferFunction did not match any allowable types: '[FunctionType2,FunctionType3]'");
                    }
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    {
                    
                    
                    if (!(val == "Identity")) 
                    {
                        ctx.Fail<APM_HalftoneType16_TransferFunction>($"Invalid value {val}, allowed are: [Identity]");
                    }
                    }
                    // no linked objects
                    return;
                }
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_HalftoneType16_TransferFunction>("TransferFunction is required to be indirect when a stream"); return; }
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
                        ctx.Fail<APM_HalftoneType16_TransferFunction>("TransferFunction did not match any allowable types: '[FunctionType0,FunctionType4]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_HalftoneType16_TransferFunction>("TransferFunction is required to one of 'dictionary;name;stream', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// HalftoneType16_Length 
/// </summary>
internal partial class APM_HalftoneType16_Length : APM_HalftoneType16_Length_Base
{
}


internal partial class APM_HalftoneType16_Length_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType16_Length";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_HalftoneType16_Length>(obj, "Length", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// HalftoneType16_Filter 
/// </summary>
internal partial class APM_HalftoneType16_Filter : APM_HalftoneType16_Filter_Base
{
}


internal partial class APM_HalftoneType16_Filter_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType16_Filter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_HalftoneType16_Filter>(obj, "Filter", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // TODO special case
                    // no value restrictions
                    ctx.Run<APM_ArrayOfCompressionFilterNames, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    {
                    
                    
                    if (!(val == "ASCIIHexDecode" || val == "ASCII85Decode" || val == "LZWDecode" || val == "FlateDecode" || val == "RunLengthDecode" || ctx.Version >= 1.5m && val == "Crypt")) 
                    {
                        ctx.Fail<APM_HalftoneType16_Filter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,fn:SinceVersion(1.5,Crypt)]");
                    }
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_HalftoneType16_Filter>("Filter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// HalftoneType16_DecodeParms 
/// </summary>
internal partial class APM_HalftoneType16_DecodeParms : APM_HalftoneType16_DecodeParms_Base
{
}


internal partial class APM_HalftoneType16_DecodeParms_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType16_DecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_HalftoneType16_DecodeParms>(obj, "DecodeParms", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // TODO special case
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
                        ctx.Fail<APM_HalftoneType16_DecodeParms>("DecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,fn:SinceVersion(1.5,FilterCrypt)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_HalftoneType16_DecodeParms>("DecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// HalftoneType16_F 
/// </summary>
internal partial class APM_HalftoneType16_F : APM_HalftoneType16_F_Base
{
}


internal partial class APM_HalftoneType16_F_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType16_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_HalftoneType16_F>(obj, "F", IndirectRequirement.Either);
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
                ctx.Fail<APM_HalftoneType16_F>("F is required to one of 'dictionary;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// HalftoneType16_FFilter 
/// </summary>
internal partial class APM_HalftoneType16_FFilter : APM_HalftoneType16_FFilter_Base
{
}


internal partial class APM_HalftoneType16_FFilter_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType16_FFilter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_HalftoneType16_FFilter>(obj, "FFilter", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // TODO special case
                    // no value restrictions
                    ctx.Run<APM_ArrayOfCompressionFilterNames, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    {
                    
                    
                    if (!(val == "ASCIIHexDecode" || val == "ASCII85Decode" || val == "LZWDecode" || val == "FlateDecode" || val == "RunLengthDecode" || ctx.Version >= 1.5m && val == "Crypt")) 
                    {
                        ctx.Fail<APM_HalftoneType16_FFilter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,fn:SinceVersion(1.5,Crypt)]");
                    }
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_HalftoneType16_FFilter>("FFilter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// HalftoneType16_FDecodeParms 
/// </summary>
internal partial class APM_HalftoneType16_FDecodeParms : APM_HalftoneType16_FDecodeParms_Base
{
}


internal partial class APM_HalftoneType16_FDecodeParms_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType16_FDecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_HalftoneType16_FDecodeParms>(obj, "FDecodeParms", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // TODO special case
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
                        ctx.Fail<APM_HalftoneType16_FDecodeParms>("FDecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,fn:SinceVersion(1.5,FilterCrypt)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_HalftoneType16_FDecodeParms>("FDecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// HalftoneType16_DL 
/// </summary>
internal partial class APM_HalftoneType16_DL : APM_HalftoneType16_DL_Base
{
}


internal partial class APM_HalftoneType16_DL_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType16_DL";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_HalftoneType16_DL>(obj, "DL", IndirectRequirement.Either);
        if (val == null) { return; }
        // TODO special case
        // no value restrictions
        // no linked objects
        
    }


}

