// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_FontDescriptorCIDType0 : APM_FontDescriptorCIDType0__Base
{
}

internal partial class APM_FontDescriptorCIDType0__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "FontDescriptorCIDType0";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_FontDescriptorCIDType0_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_FontName, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_FontFamily, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_FontStretch, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_FontWeight, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_Flags, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_FontBBox, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_ItalicAngle, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_Ascent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_Descent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_Leading, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_CapHeight, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_XHeight, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_StemV, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_StemH, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_AvgWidth, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_MaxWidth, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_MissingWidth, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_FontFile, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_FontFile3, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_Style, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_Lang, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_FD, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType0_CIDSet, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorCIDType0>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorCIDType0>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorCIDType0>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorCIDType0>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorCIDType0>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorCIDType0>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorCIDType0>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorCIDType0>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorCIDType0>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_FontDescriptorCIDType0_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile3", "Style", "FD", "CIDSet"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile3", "Style", "FD", "CIDSet"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile3", "Style", "FD", "CIDSet"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile3", "Style", "Lang", "FD", "CIDSet"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile3", "Style", "Lang", "FD", "CIDSet"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile3", "Style", "Lang", "FD", "CIDSet"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile3", "Style", "Lang", "FD", "CIDSet"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile3", "Style", "Lang", "FD", "CIDSet"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile3", "Style", "Lang", "FD"
    };
    


}

/// <summary>
/// FontDescriptorCIDType0_Type Table 120 and Table 122
/// </summary>
internal partial class APM_FontDescriptorCIDType0_Type : APM_FontDescriptorCIDType0_Type__Base
{
}


internal partial class APM_FontDescriptorCIDType0_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FontDescriptorCIDType0_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "FontDescriptor")) 
        {
            ctx.Fail<APM_FontDescriptorCIDType0_Type>($"Invalid value {val}, allowed are: [FontDescriptor]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_FontName 
/// </summary>
internal partial class APM_FontDescriptorCIDType0_FontName : APM_FontDescriptorCIDType0_FontName__Base
{
}


internal partial class APM_FontDescriptorCIDType0_FontName__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_FontName";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FontDescriptorCIDType0_FontName>(obj, "FontName", IndirectRequirement.Either);
        if (val == null) { return; }
        var FontName = obj.Get("FontName");
        var parentBaseFont = parent?.Get("BaseFont");
        if (!(eq(FontName,parentBaseFont))) 
        {
            ctx.Fail<APM_FontDescriptorCIDType0_FontName>($"Value failed special case check: fn:Eval(@FontName==parent::@BaseFont)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_FontFamily 
/// </summary>
internal partial class APM_FontDescriptorCIDType0_FontFamily : APM_FontDescriptorCIDType0_FontFamily__Base
{
}


internal partial class APM_FontDescriptorCIDType0_FontFamily__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_FontFamily";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FontDescriptorCIDType0_FontFamily>(obj, "FontFamily", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_FontStretch 
/// </summary>
internal partial class APM_FontDescriptorCIDType0_FontStretch : APM_FontDescriptorCIDType0_FontStretch__Base
{
}


internal partial class APM_FontDescriptorCIDType0_FontStretch__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_FontStretch";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_FontDescriptorCIDType0_FontStretch>(obj, "FontStretch", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "UltraCondensed" || val == "ExtraCondensed" || val == "Condensed" || val == "SemiCondensed" || val == "Normal" || val == "SemiExpanded" || val == "Expanded" || val == "ExtraExpanded" || val == "UltraExpanded")) 
        {
            ctx.Fail<APM_FontDescriptorCIDType0_FontStretch>($"Invalid value {val}, allowed are: [UltraCondensed,ExtraCondensed,Condensed,SemiCondensed,Normal,SemiExpanded,Expanded,ExtraExpanded,UltraExpanded]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_FontWeight 
/// </summary>
internal partial class APM_FontDescriptorCIDType0_FontWeight : APM_FontDescriptorCIDType0_FontWeight__Base
{
}


internal partial class APM_FontDescriptorCIDType0_FontWeight__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_FontWeight";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FontDescriptorCIDType0_FontWeight>(obj, "FontWeight", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 100 || val == 200 || val == 300 || val == 400 || val == 500 || val == 600 || val == 700 || val == 800 || val == 900)) 
        {
            ctx.Fail<APM_FontDescriptorCIDType0_FontWeight>($"Invalid value {val}, allowed are: [100,200,300,400,500,600,700,800,900]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_Flags Table 121
/// </summary>
internal partial class APM_FontDescriptorCIDType0_Flags : APM_FontDescriptorCIDType0_Flags__Base
{
}


internal partial class APM_FontDescriptorCIDType0_Flags__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_Flags";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_FontDescriptorCIDType0_Flags>(obj, "Flags", IndirectRequirement.Either);
        if (val == null) { return; }
        
        if (!(BitsClear(val,0b00000000000000000000000000010000)&&BitsClear(val,0b00000000000000001111111110000000)&&BitsClear(val,0b11111111111110000000000000000000))) 
        {
            ctx.Fail<APM_FontDescriptorCIDType0_Flags>($"Value failed special case check: fn:Eval(fn:BitClear(5) && fn:BitsClear(8,16) && fn:BitsClear(20,32))");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_FontBBox 
/// </summary>
internal partial class APM_FontDescriptorCIDType0_FontBBox : APM_FontDescriptorCIDType0_FontBBox__Base
{
}


internal partial class APM_FontDescriptorCIDType0_FontBBox__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_FontBBox";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_FontDescriptorCIDType0_FontBBox>(obj, "FontBBox", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_ItalicAngle 
/// </summary>
internal partial class APM_FontDescriptorCIDType0_ItalicAngle : APM_FontDescriptorCIDType0_ItalicAngle__Base
{
}


internal partial class APM_FontDescriptorCIDType0_ItalicAngle__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_ItalicAngle";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_FontDescriptorCIDType0_ItalicAngle>(obj, "ItalicAngle", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_Ascent 
/// </summary>
internal partial class APM_FontDescriptorCIDType0_Ascent : APM_FontDescriptorCIDType0_Ascent__Base
{
}


internal partial class APM_FontDescriptorCIDType0_Ascent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_Ascent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_FontDescriptorCIDType0_Ascent>(obj, "Ascent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_Descent https://github.com/pdf-association/pdf-issues/issues/190
/// </summary>
internal partial class APM_FontDescriptorCIDType0_Descent : APM_FontDescriptorCIDType0_Descent__Base
{
}


internal partial class APM_FontDescriptorCIDType0_Descent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_Descent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_FontDescriptorCIDType0_Descent>(obj, "Descent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var Descent = obj.Get("Descent");
        if (!(lte(Descent,0))) 
        {
            ctx.Fail<APM_FontDescriptorCIDType0_Descent>($"Invalid value {val}, allowed are: [fn:Eval(@Descent<=0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_Leading 
/// </summary>
internal partial class APM_FontDescriptorCIDType0_Leading : APM_FontDescriptorCIDType0_Leading__Base
{
}


internal partial class APM_FontDescriptorCIDType0_Leading__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_Leading";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorCIDType0_Leading>(obj, "Leading", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_CapHeight 
/// </summary>
internal partial class APM_FontDescriptorCIDType0_CapHeight : APM_FontDescriptorCIDType0_CapHeight__Base
{
}


internal partial class APM_FontDescriptorCIDType0_CapHeight__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_CapHeight";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorCIDType0_CapHeight>(obj, "CapHeight", IndirectRequirement.Either);
        if ((FontHasLatinChars(obj)) && val == null) {
            ctx.Fail<APM_FontDescriptorCIDType0_CapHeight>("CapHeight is required when 'fn:IsRequired(fn:FontHasLatinChars())"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_XHeight 
/// </summary>
internal partial class APM_FontDescriptorCIDType0_XHeight : APM_FontDescriptorCIDType0_XHeight__Base
{
}


internal partial class APM_FontDescriptorCIDType0_XHeight__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_XHeight";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorCIDType0_XHeight>(obj, "XHeight", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_StemV 
/// </summary>
internal partial class APM_FontDescriptorCIDType0_StemV : APM_FontDescriptorCIDType0_StemV__Base
{
}


internal partial class APM_FontDescriptorCIDType0_StemV__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_StemV";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_FontDescriptorCIDType0_StemV>(obj, "StemV", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_StemH 
/// </summary>
internal partial class APM_FontDescriptorCIDType0_StemH : APM_FontDescriptorCIDType0_StemH__Base
{
}


internal partial class APM_FontDescriptorCIDType0_StemH__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_StemH";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorCIDType0_StemH>(obj, "StemH", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_AvgWidth 
/// </summary>
internal partial class APM_FontDescriptorCIDType0_AvgWidth : APM_FontDescriptorCIDType0_AvgWidth__Base
{
}


internal partial class APM_FontDescriptorCIDType0_AvgWidth__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_AvgWidth";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorCIDType0_AvgWidth>(obj, "AvgWidth", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_MaxWidth 
/// </summary>
internal partial class APM_FontDescriptorCIDType0_MaxWidth : APM_FontDescriptorCIDType0_MaxWidth__Base
{
}


internal partial class APM_FontDescriptorCIDType0_MaxWidth__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_MaxWidth";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorCIDType0_MaxWidth>(obj, "MaxWidth", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_MissingWidth 
/// </summary>
internal partial class APM_FontDescriptorCIDType0_MissingWidth : APM_FontDescriptorCIDType0_MissingWidth__Base
{
}


internal partial class APM_FontDescriptorCIDType0_MissingWidth__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_MissingWidth";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorCIDType0_MissingWidth>(obj, "MissingWidth", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_FontFile Table 124
/// </summary>
internal partial class APM_FontDescriptorCIDType0_FontFile : APM_FontDescriptorCIDType0_FontFile__Base
{
}


internal partial class APM_FontDescriptorCIDType0_FontFile__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_FontFile";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_FontDescriptorCIDType0_FontFile>(obj, "FontFile", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        
        if (!(!obj.ContainsKey("FontFile3"))) 
        {
            ctx.Fail<APM_FontDescriptorCIDType0_FontFile>($"Value failed special case check: fn:Eval(fn:Not(fn:IsPresent(FontFile3)))");
        }
        // no value restrictions
        ctx.Run<APM_FontFile, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_FontFile3 Table 124
/// </summary>
internal partial class APM_FontDescriptorCIDType0_FontFile3 : APM_FontDescriptorCIDType0_FontFile3__Base
{
}


internal partial class APM_FontDescriptorCIDType0_FontFile3__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_FontFile3";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_FontDescriptorCIDType0_FontFile3>(obj, "FontFile3", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        
        if (!(!obj.ContainsKey("FontFile"))) 
        {
            ctx.Fail<APM_FontDescriptorCIDType0_FontFile3>($"Value failed special case check: fn:Eval(fn:Not(fn:IsPresent(FontFile)))");
        }
        // no value restrictions
        ctx.Run<APM_FontFile3CIDType0, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_Style 
/// </summary>
internal partial class APM_FontDescriptorCIDType0_Style : APM_FontDescriptorCIDType0_Style__Base
{
}


internal partial class APM_FontDescriptorCIDType0_Style__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_Style";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_FontDescriptorCIDType0_Style>(obj, "Style", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_StyleDict, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_Lang 
/// </summary>
internal partial class APM_FontDescriptorCIDType0_Lang : APM_FontDescriptorCIDType0_Lang__Base
{
}


internal partial class APM_FontDescriptorCIDType0_Lang__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_Lang";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_FontDescriptorCIDType0_Lang>(obj, "Lang", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_FD 
/// </summary>
internal partial class APM_FontDescriptorCIDType0_FD : APM_FontDescriptorCIDType0_FD__Base
{
}


internal partial class APM_FontDescriptorCIDType0_FD__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_FD";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_FontDescriptorCIDType0_FD>(obj, "FD", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_FDDict, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// FontDescriptorCIDType0_CIDSet 
/// </summary>
internal partial class APM_FontDescriptorCIDType0_CIDSet : APM_FontDescriptorCIDType0_CIDSet__Base
{
}


internal partial class APM_FontDescriptorCIDType0_CIDSet__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType0_CIDSet";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_FontDescriptorCIDType0_CIDSet>(obj, "CIDSet", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

