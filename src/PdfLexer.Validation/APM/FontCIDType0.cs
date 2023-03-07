// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_FontCIDType0 : APM_FontCIDType0__Base
{
}

internal partial class APM_FontCIDType0__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "FontCIDType0";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_FontCIDType0_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontCIDType0_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontCIDType0_BaseFont, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontCIDType0_CIDSystemInfo, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontCIDType0_FontDescriptor, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontCIDType0_DW, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontCIDType0_W, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontCIDType0_DW2, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontCIDType0_W2, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontCIDType0_CIDToGIDMap, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontCIDType0_ToUnicode, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_FontCIDType0>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_FontCIDType0>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_FontCIDType0>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_FontCIDType0>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_FontCIDType0>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_FontCIDType0>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_FontCIDType0>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_FontCIDType0>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_FontCIDType0>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_FontCIDType0_Type, PdfDictionary>(new CallStack(), obj, null);
        c.Run<APM_FontCIDType0_Subtype, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "BaseFont", "CIDSystemInfo", "FontDescriptor", "DW", "W", "DW2", "W2", "CIDToGIDMap", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "BaseFont", "CIDSystemInfo", "FontDescriptor", "DW", "W", "DW2", "W2", "CIDToGIDMap", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "BaseFont", "CIDSystemInfo", "FontDescriptor", "DW", "W", "DW2", "W2", "CIDToGIDMap", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "BaseFont", "CIDSystemInfo", "FontDescriptor", "DW", "W", "DW2", "W2", "CIDToGIDMap", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "BaseFont", "CIDSystemInfo", "FontDescriptor", "DW", "W", "DW2", "W2", "CIDToGIDMap", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "BaseFont", "CIDSystemInfo", "FontDescriptor", "DW", "W", "DW2", "W2", "CIDToGIDMap", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "BaseFont", "CIDSystemInfo", "FontDescriptor", "DW", "W", "DW2", "W2", "CIDToGIDMap", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "BaseFont", "CIDSystemInfo", "FontDescriptor", "DW", "W", "DW2", "W2", "CIDToGIDMap", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "BaseFont", "CIDSystemInfo", "FontDescriptor", "DW", "W", "DW2", "W2", "CIDToGIDMap", "ToUnicode"
    };
    


}

/// <summary>
/// FontCIDType0_Type Table 115
/// </summary>
internal partial class APM_FontCIDType0_Type : APM_FontCIDType0_Type__Base
{
}


internal partial class APM_FontCIDType0_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType0_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FontCIDType0_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Font)) 
        {
            ctx.Fail<APM_FontCIDType0_Type>($"Invalid value {val}, allowed are: [Font]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontCIDType0_Subtype 
/// </summary>
internal partial class APM_FontCIDType0_Subtype : APM_FontCIDType0_Subtype__Base
{
}


internal partial class APM_FontCIDType0_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType0_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FontCIDType0_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.CIDFontType0)) 
        {
            ctx.Fail<APM_FontCIDType0_Subtype>($"Invalid value {val}, allowed are: [CIDFontType0]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontCIDType0_BaseFont 
/// </summary>
internal partial class APM_FontCIDType0_BaseFont : APM_FontCIDType0_BaseFont__Base
{
}


internal partial class APM_FontCIDType0_BaseFont__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType0_BaseFont";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FontCIDType0_BaseFont>(obj, "BaseFont", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontCIDType0_CIDSystemInfo 
/// </summary>
internal partial class APM_FontCIDType0_CIDSystemInfo : APM_FontCIDType0_CIDSystemInfo__Base
{
}


internal partial class APM_FontCIDType0_CIDSystemInfo__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType0_CIDSystemInfo";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_FontCIDType0_CIDSystemInfo>(obj, "CIDSystemInfo", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_CIDSystemInfo, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// FontCIDType0_FontDescriptor https://github.com/pdf-association/pdf-issues/issues/106
/// </summary>
internal partial class APM_FontCIDType0_FontDescriptor : APM_FontCIDType0_FontDescriptor__Base
{
}


internal partial class APM_FontCIDType0_FontDescriptor__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType0_FontDescriptor";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        // TODO complex IR
        var val = ctx.GetRequired<PdfDictionary, APM_FontCIDType0_FontDescriptor>(obj, "FontDescriptor", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_FontDescriptorCIDType0, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// FontCIDType0_DW 
/// </summary>
internal partial class APM_FontCIDType0_DW : APM_FontCIDType0_DW__Base
{
}


internal partial class APM_FontCIDType0_DW__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType0_DW";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontCIDType0_DW>(obj, "DW", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontCIDType0_W 
/// </summary>
internal partial class APM_FontCIDType0_W : APM_FontCIDType0_W__Base
{
}


internal partial class APM_FontCIDType0_W__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType0_W";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_FontCIDType0_W>(obj, "W", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfCIDGlyphMetricsW, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// FontCIDType0_DW2 
/// </summary>
internal partial class APM_FontCIDType0_DW2 : APM_FontCIDType0_DW2__Base
{
}


internal partial class APM_FontCIDType0_DW2__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType0_DW2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_FontCIDType0_DW2>(obj, "DW2", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_2Numbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// FontCIDType0_W2 
/// </summary>
internal partial class APM_FontCIDType0_W2 : APM_FontCIDType0_W2__Base
{
}


internal partial class APM_FontCIDType0_W2__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType0_W2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_FontCIDType0_W2>(obj, "W2", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfCIDGlyphMetricsW2, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// FontCIDType0_CIDToGIDMap 
/// </summary>
internal partial class APM_FontCIDType0_CIDToGIDMap : APM_FontCIDType0_CIDToGIDMap__Base
{
}


internal partial class APM_FontCIDType0_CIDToGIDMap__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType0_CIDToGIDMap";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_FontCIDType0_CIDToGIDMap>(obj, "CIDToGIDMap", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    
                    
                    if (!(val == PdfName.Identity)) 
                    {
                        ctx.Fail<APM_FontCIDType0_CIDToGIDMap>($"Invalid value {val}, allowed are: [Identity]");
                    }
                    // no linked objects
                    return;
                }
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_FontCIDType0_CIDToGIDMap>("CIDToGIDMap is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
                    return;
                }
            
            default:
                ctx.Fail<APM_FontCIDType0_CIDToGIDMap>("CIDToGIDMap is required to one of 'name;stream', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// FontCIDType0_ToUnicode 
/// </summary>
internal partial class APM_FontCIDType0_ToUnicode : APM_FontCIDType0_ToUnicode__Base
{
}


internal partial class APM_FontCIDType0_ToUnicode__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType0_ToUnicode";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_FontCIDType0_ToUnicode>(obj, "ToUnicode", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}
