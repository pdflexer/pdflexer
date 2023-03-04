// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_CMapStream : APM_CMapStream_Base
{
}

internal partial class APM_CMapStream_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "CMapStream";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_CMapStream_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CMapStream_CMapName, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CMapStream_CIDSystemInfo, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CMapStream_WMode, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CMapStream_UseCMap, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CMapStream_Length, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CMapStream_Filter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CMapStream_DecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CMapStream_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CMapStream_FFilter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CMapStream_FDecodeParms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CMapStream_DL, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_CMapStream>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_CMapStream>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_CMapStream>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_CMapStream>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_CMapStream>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_CMapStream>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_CMapStream>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_CMapStream>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_CMapStream>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_CMapStream_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "CMapName", "CIDSystemInfo", "WMode", "UseCMap", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "CMapName", "CIDSystemInfo", "WMode", "UseCMap", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "CMapName", "CIDSystemInfo", "WMode", "UseCMap", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "CMapName", "CIDSystemInfo", "WMode", "UseCMap", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "CMapName", "CIDSystemInfo", "WMode", "UseCMap", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "CMapName", "CIDSystemInfo", "WMode", "UseCMap", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "CMapName", "CIDSystemInfo", "WMode", "UseCMap", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "CMapName", "CIDSystemInfo", "WMode", "UseCMap", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "CMapName", "CIDSystemInfo", "WMode", "UseCMap", "Length", "Filter", "DecodeParms", "F", "FFilter", "FDecodeParms", "DL"
    };
    


}

/// <summary>
/// CMapStream_Type Table 5 and Table 118
/// </summary>
internal partial class APM_CMapStream_Type : APM_CMapStream_Type_Base
{
}


internal partial class APM_CMapStream_Type_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CMapStream_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_CMapStream_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "CMap")) 
        {
            ctx.Fail<APM_CMapStream_Type>($"Invalid value {val}, allowed are: [CMap]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// CMapStream_CMapName 
/// </summary>
internal partial class APM_CMapStream_CMapName : APM_CMapStream_CMapName_Base
{
}


internal partial class APM_CMapStream_CMapName_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CMapStream_CMapName";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_CMapStream_CMapName>(obj, "CMapName", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// CMapStream_CIDSystemInfo 
/// </summary>
internal partial class APM_CMapStream_CIDSystemInfo : APM_CMapStream_CIDSystemInfo_Base
{
}


internal partial class APM_CMapStream_CIDSystemInfo_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CMapStream_CIDSystemInfo";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_CMapStream_CIDSystemInfo>(obj, "CIDSystemInfo", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_CIDSystemInfo, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// CMapStream_WMode 
/// </summary>
internal partial class APM_CMapStream_WMode : APM_CMapStream_WMode_Base
{
}


internal partial class APM_CMapStream_WMode_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CMapStream_WMode";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_CMapStream_WMode>(obj, "WMode", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == 1 || val == 0)) 
        {
            ctx.Fail<APM_CMapStream_WMode>($"Invalid value {val}, allowed are: [1,0]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// CMapStream_UseCMap Table 116
/// </summary>
internal partial class APM_CMapStream_UseCMap : APM_CMapStream_UseCMap_Base
{
}


internal partial class APM_CMapStream_UseCMap_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CMapStream_UseCMap";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_CMapStream_UseCMap>(obj, "UseCMap", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    {
                    
                    
                    if (!(val == "GB-EUC-H" || val == "GB-EUC-V" || val == "GBpc-EUC-H" || val == "GBpc-EUC-V" || val == "GBK-EUC-H" || val == "GBK-EUC-V" || val == "GBKp-EUC-H" || val == "GBKp-EUC-V" || val == "GBK2K-H" || val == "GBK2K-V" || val == "UniGB-UCS2-H" || val == "UniGB-UCS2-V" || val == "UniGB-UTF16-H" || val == "UniGB-UTF16-V" || val == "B5pc-H" || val == "B5pc-V" || val == "HKscs-B5-H" || val == "HKscs-B5-V" || val == "ETen-B5-H" || val == "ETen-B5-V" || val == "ETenms-B5-H" || val == "ETenms-B5-V" || val == "CNS-EUC-H" || val == "CNS-EUC-V" || val == "UniCNS-UCS2-H" || val == "UniCNS-UCS2-V" || val == "UniCNS-UTF16-H" || val == "UniCNS-UTF16-V" || val == "83pv-RKSJ-H" || val == "90ms-RKSJ-H" || val == "90ms-RKSJ-V" || val == "90msp-RKSJ-H" || val == "90msp-RKSJ-V" || val == "90pv-RKSJ-H" || val == "Add-RKSJ-H" || val == "Add-RKSJ-V" || val == "EUC-H" || val == "EUC-V" || val == "Ext-RKSJ-H" || val == "Ext-RKSJ-V" || val == "H" || val == "V" || val == "UniJIS-UCS2-H" || val == "UniJIS-UCS2-V" || val == "UniJIS-UCS2-HW-H" || val == "UniJIS-UCS2-HW-V" || val == "UniJIS-UTF16-H" || val == "UniJIS-UTF16-V" || val == "KSC-EUC-H" || val == "KSC-EUC-V" || val == "KSCms-UHC-H" || val == "KSCms-UHC-V" || val == "KSCms-UHC-HW-H" || val == "KSCms-UHC-HW-V" || val == "KSCpc-EUC-H" || val == "UniKS-UCS2-H" || val == "UniKS-UCS2-V" || val == "UniKS-UTF16-H" || val == "UniKS-UTF16-V" || val == "Identity-H" || val == "Identity-V")) 
                    {
                        ctx.Fail<APM_CMapStream_UseCMap>($"Invalid value {val}, allowed are: [GB-EUC-H,GB-EUC-V,GBpc-EUC-H,GBpc-EUC-V,GBK-EUC-H,GBK-EUC-V,GBKp-EUC-H,GBKp-EUC-V,GBK2K-H,GBK2K-V,UniGB-UCS2-H,UniGB-UCS2-V,UniGB-UTF16-H,UniGB-UTF16-V,B5pc-H,B5pc-V,HKscs-B5-H,HKscs-B5-V,ETen-B5-H,ETen-B5-V,ETenms-B5-H,ETenms-B5-V,CNS-EUC-H,CNS-EUC-V,UniCNS-UCS2-H,UniCNS-UCS2-V,UniCNS-UTF16-H,UniCNS-UTF16-V,83pv-RKSJ-H,90ms-RKSJ-H,90ms-RKSJ-V,90msp-RKSJ-H,90msp-RKSJ-V,90pv-RKSJ-H,Add-RKSJ-H,Add-RKSJ-V,EUC-H,EUC-V,Ext-RKSJ-H,Ext-RKSJ-V,H,V,UniJIS-UCS2-H,UniJIS-UCS2-V,UniJIS-UCS2-HW-H,UniJIS-UCS2-HW-V,UniJIS-UTF16-H,UniJIS-UTF16-V,KSC-EUC-H,KSC-EUC-V,KSCms-UHC-H,KSCms-UHC-V,KSCms-UHC-HW-H,KSCms-UHC-HW-V,KSCpc-EUC-H,UniKS-UCS2-H,UniKS-UCS2-V,UniKS-UTF16-H,UniKS-UTF16-V,Identity-H,Identity-V]");
                    }
                    }
                    // no linked objects
                    return;
                }
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_CMapStream_UseCMap>("UseCMap is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
                    return;
                }
            
            default:
                ctx.Fail<APM_CMapStream_UseCMap>("UseCMap is required to one of 'name;stream', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// CMapStream_Length 
/// </summary>
internal partial class APM_CMapStream_Length : APM_CMapStream_Length_Base
{
}


internal partial class APM_CMapStream_Length_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CMapStream_Length";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_CMapStream_Length>(obj, "Length", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// CMapStream_Filter 
/// </summary>
internal partial class APM_CMapStream_Filter : APM_CMapStream_Filter_Base
{
}


internal partial class APM_CMapStream_Filter_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CMapStream_Filter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_CMapStream_Filter>(obj, "Filter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_CMapStream_Filter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,fn:SinceVersion(1.5,Crypt)]");
                    }
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_CMapStream_Filter>("Filter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// CMapStream_DecodeParms 
/// </summary>
internal partial class APM_CMapStream_DecodeParms : APM_CMapStream_DecodeParms_Base
{
}


internal partial class APM_CMapStream_DecodeParms_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CMapStream_DecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_CMapStream_DecodeParms>(obj, "DecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_CMapStream_DecodeParms>("DecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,fn:SinceVersion(1.5,FilterCrypt)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_CMapStream_DecodeParms>("DecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// CMapStream_F 
/// </summary>
internal partial class APM_CMapStream_F : APM_CMapStream_F_Base
{
}


internal partial class APM_CMapStream_F_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CMapStream_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_CMapStream_F>(obj, "F", IndirectRequirement.Either);
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
                ctx.Fail<APM_CMapStream_F>("F is required to one of 'dictionary;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// CMapStream_FFilter 
/// </summary>
internal partial class APM_CMapStream_FFilter : APM_CMapStream_FFilter_Base
{
}


internal partial class APM_CMapStream_FFilter_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CMapStream_FFilter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_CMapStream_FFilter>(obj, "FFilter", IndirectRequirement.Either);
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
                        ctx.Fail<APM_CMapStream_FFilter>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,FlateDecode,RunLengthDecode,fn:SinceVersion(1.5,Crypt)]");
                    }
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_CMapStream_FFilter>("FFilter is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// CMapStream_FDecodeParms 
/// </summary>
internal partial class APM_CMapStream_FDecodeParms : APM_CMapStream_FDecodeParms_Base
{
}


internal partial class APM_CMapStream_FDecodeParms_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CMapStream_FDecodeParms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_CMapStream_FDecodeParms>(obj, "FDecodeParms", IndirectRequirement.Either);
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
                        ctx.Fail<APM_CMapStream_FDecodeParms>("FDecodeParms did not match any allowable types: '[FilterLZWDecode,FilterFlateDecode,fn:SinceVersion(1.5,FilterCrypt)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_CMapStream_FDecodeParms>("FDecodeParms is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// CMapStream_DL 
/// </summary>
internal partial class APM_CMapStream_DL : APM_CMapStream_DL_Base
{
}


internal partial class APM_CMapStream_DL_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CMapStream_DL";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_CMapStream_DL>(obj, "DL", IndirectRequirement.Either);
        if (val == null) { return; }
        // TODO special case
        // no value restrictions
        // no linked objects
        
    }


}

