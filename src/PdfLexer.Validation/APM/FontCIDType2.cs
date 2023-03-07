// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_FontCIDType2 : APM_FontCIDType2__Base
{
}

internal partial class APM_FontCIDType2__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "FontCIDType2";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_FontCIDType2_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontCIDType2_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontCIDType2_BaseFont, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontCIDType2_CIDSystemInfo, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontCIDType2_FontDescriptor, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontCIDType2_DW, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontCIDType2_W, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontCIDType2_DW2, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontCIDType2_W2, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontCIDType2_CIDToGIDMap, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontCIDType2_ToUnicode, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_FontCIDType2>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_FontCIDType2>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_FontCIDType2>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_FontCIDType2>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_FontCIDType2>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_FontCIDType2>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_FontCIDType2>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_FontCIDType2>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_FontCIDType2>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_FontCIDType2_Type, PdfDictionary>(new CallStack(), obj, null);
        c.Run<APM_FontCIDType2_Subtype, PdfDictionary>(new CallStack(), obj, null);
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
/// FontCIDType2_Type Table 115
/// </summary>
internal partial class APM_FontCIDType2_Type : APM_FontCIDType2_Type__Base
{
}


internal partial class APM_FontCIDType2_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType2_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FontCIDType2_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Font)) 
        {
            ctx.Fail<APM_FontCIDType2_Type>($"Invalid value {val}, allowed are: [Font]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontCIDType2_Subtype 
/// </summary>
internal partial class APM_FontCIDType2_Subtype : APM_FontCIDType2_Subtype__Base
{
}


internal partial class APM_FontCIDType2_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType2_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FontCIDType2_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.CIDFontType2)) 
        {
            ctx.Fail<APM_FontCIDType2_Subtype>($"Invalid value {val}, allowed are: [CIDFontType2]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontCIDType2_BaseFont 
/// </summary>
internal partial class APM_FontCIDType2_BaseFont : APM_FontCIDType2_BaseFont__Base
{
}


internal partial class APM_FontCIDType2_BaseFont__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType2_BaseFont";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FontCIDType2_BaseFont>(obj, "BaseFont", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontCIDType2_CIDSystemInfo 
/// </summary>
internal partial class APM_FontCIDType2_CIDSystemInfo : APM_FontCIDType2_CIDSystemInfo__Base
{
}


internal partial class APM_FontCIDType2_CIDSystemInfo__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType2_CIDSystemInfo";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_FontCIDType2_CIDSystemInfo>(obj, "CIDSystemInfo", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_CIDSystemInfo, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// FontCIDType2_FontDescriptor https://github.com/pdf-association/pdf-issues/issues/106
/// </summary>
internal partial class APM_FontCIDType2_FontDescriptor : APM_FontCIDType2_FontDescriptor__Base
{
}


internal partial class APM_FontCIDType2_FontDescriptor__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType2_FontDescriptor";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        // TODO complex IR
        var val = ctx.GetRequired<PdfDictionary, APM_FontCIDType2_FontDescriptor>(obj, "FontDescriptor", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_FontDescriptorCIDType2, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// FontCIDType2_DW 
/// </summary>
internal partial class APM_FontCIDType2_DW : APM_FontCIDType2_DW__Base
{
}


internal partial class APM_FontCIDType2_DW__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType2_DW";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontCIDType2_DW>(obj, "DW", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontCIDType2_W 
/// </summary>
internal partial class APM_FontCIDType2_W : APM_FontCIDType2_W__Base
{
}


internal partial class APM_FontCIDType2_W__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType2_W";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_FontCIDType2_W>(obj, "W", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfCIDGlyphMetricsW, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// FontCIDType2_DW2 
/// </summary>
internal partial class APM_FontCIDType2_DW2 : APM_FontCIDType2_DW2__Base
{
}


internal partial class APM_FontCIDType2_DW2__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType2_DW2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_FontCIDType2_DW2>(obj, "DW2", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_2Numbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// FontCIDType2_W2 
/// </summary>
internal partial class APM_FontCIDType2_W2 : APM_FontCIDType2_W2__Base
{
}


internal partial class APM_FontCIDType2_W2__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType2_W2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_FontCIDType2_W2>(obj, "W2", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfCIDGlyphMetricsW2, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// FontCIDType2_CIDToGIDMap 
/// </summary>
internal partial class APM_FontCIDType2_CIDToGIDMap : APM_FontCIDType2_CIDToGIDMap__Base
{
}


internal partial class APM_FontCIDType2_CIDToGIDMap__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType2_CIDToGIDMap";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_FontCIDType2_CIDToGIDMap>(obj, "CIDToGIDMap", IndirectRequirement.Either);
        
        var FontDescriptorFontFile2 = obj.Get("FontDescriptor")?.Get("FontFile2");
        if (((ctx.Version >= 2.0m&&(FontDescriptorFontFile2 != null))) && utval == null) {
            ctx.Fail<APM_FontCIDType2_CIDToGIDMap>("CIDToGIDMap is required"); return;
        } else if (utval == null) {
            return;
        }
        
        switch (utval.Type) 
        {
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    
                    
                    if (!(val == PdfName.Identity)) 
                    {
                        ctx.Fail<APM_FontCIDType2_CIDToGIDMap>($"Invalid value {val}, allowed are: [Identity]");
                    }
                    // no linked objects
                    return;
                }
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_FontCIDType2_CIDToGIDMap>("CIDToGIDMap is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
                    return;
                }
            
            default:
                ctx.Fail<APM_FontCIDType2_CIDToGIDMap>("CIDToGIDMap is required to one of 'name;stream', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// FontCIDType2_ToUnicode 
/// </summary>
internal partial class APM_FontCIDType2_ToUnicode : APM_FontCIDType2_ToUnicode__Base
{
}


internal partial class APM_FontCIDType2_ToUnicode__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontCIDType2_ToUnicode";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_FontCIDType2_ToUnicode>(obj, "ToUnicode", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}
