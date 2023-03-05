// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_FontDescriptorTrueType : APM_FontDescriptorTrueType__Base
{
}

internal partial class APM_FontDescriptorTrueType__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "FontDescriptorTrueType";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_FontDescriptorTrueType_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorTrueType_FontName, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorTrueType_FontFamily, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorTrueType_FontStretch, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorTrueType_FontWeight, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorTrueType_Flags, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorTrueType_FontBBox, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorTrueType_ItalicAngle, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorTrueType_Ascent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorTrueType_Descent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorTrueType_Leading, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorTrueType_CapHeight, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorTrueType_XHeight, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorTrueType_StemV, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorTrueType_StemH, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorTrueType_AvgWidth, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorTrueType_MaxWidth, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorTrueType_MissingWidth, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorTrueType_FontFile, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorTrueType_FontFile2, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_10.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorTrueType>($"Unknown field {extra} for version 1.0");
                }
                break;
            case 1.1m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_11.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorTrueType>($"Unknown field {extra} for version 1.1");
                }
                break;
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorTrueType>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorTrueType>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorTrueType>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorTrueType>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorTrueType>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorTrueType>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorTrueType>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorTrueType>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorTrueType>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_FontDescriptorTrueType_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_10 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth"
    };
    public static HashSet<string> AllowedFields_11 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile"
    };
    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile2"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile2"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile2"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile2"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile2"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile2"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile2"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile2"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile2"
    };
    


}

/// <summary>
/// FontDescriptorTrueType_Type Table 120
/// </summary>
internal partial class APM_FontDescriptorTrueType_Type : APM_FontDescriptorTrueType_Type__Base
{
}


internal partial class APM_FontDescriptorTrueType_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorTrueType_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FontDescriptorTrueType_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "FontDescriptor")) 
        {
            ctx.Fail<APM_FontDescriptorTrueType_Type>($"Invalid value {val}, allowed are: [FontDescriptor]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorTrueType_FontName 
/// </summary>
internal partial class APM_FontDescriptorTrueType_FontName : APM_FontDescriptorTrueType_FontName__Base
{
}


internal partial class APM_FontDescriptorTrueType_FontName__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorTrueType_FontName";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FontDescriptorTrueType_FontName>(obj, "FontName", IndirectRequirement.Either);
        if (val == null) { return; }
        var FontName = obj.Get("FontName");
        var parentBaseFont = parent?.Get("BaseFont");
        if (!(eq(FontName,parentBaseFont))) 
        {
            ctx.Fail<APM_FontDescriptorTrueType_FontName>($"Value failed special case check: fn:Eval(@FontName==parent::@BaseFont)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorTrueType_FontFamily 
/// </summary>
internal partial class APM_FontDescriptorTrueType_FontFamily : APM_FontDescriptorTrueType_FontFamily__Base
{
}


internal partial class APM_FontDescriptorTrueType_FontFamily__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorTrueType_FontFamily";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FontDescriptorTrueType_FontFamily>(obj, "FontFamily", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorTrueType_FontStretch 
/// </summary>
internal partial class APM_FontDescriptorTrueType_FontStretch : APM_FontDescriptorTrueType_FontStretch__Base
{
}


internal partial class APM_FontDescriptorTrueType_FontStretch__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorTrueType_FontStretch";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_FontDescriptorTrueType_FontStretch>(obj, "FontStretch", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "UltraCondensed" || val == "ExtraCondensed" || val == "Condensed" || val == "SemiCondensed" || val == "Normal" || val == "SemiExpanded" || val == "Expanded" || val == "ExtraExpanded" || val == "UltraExpanded")) 
        {
            ctx.Fail<APM_FontDescriptorTrueType_FontStretch>($"Invalid value {val}, allowed are: [UltraCondensed,ExtraCondensed,Condensed,SemiCondensed,Normal,SemiExpanded,Expanded,ExtraExpanded,UltraExpanded]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorTrueType_FontWeight 
/// </summary>
internal partial class APM_FontDescriptorTrueType_FontWeight : APM_FontDescriptorTrueType_FontWeight__Base
{
}


internal partial class APM_FontDescriptorTrueType_FontWeight__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorTrueType_FontWeight";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FontDescriptorTrueType_FontWeight>(obj, "FontWeight", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 100 || val == 200 || val == 300 || val == 400 || val == 500 || val == 600 || val == 700 || val == 800 || val == 900)) 
        {
            ctx.Fail<APM_FontDescriptorTrueType_FontWeight>($"Invalid value {val}, allowed are: [100,200,300,400,500,600,700,800,900]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorTrueType_Flags Table 121
/// </summary>
internal partial class APM_FontDescriptorTrueType_Flags : APM_FontDescriptorTrueType_Flags__Base
{
}


internal partial class APM_FontDescriptorTrueType_Flags__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorTrueType_Flags";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_FontDescriptorTrueType_Flags>(obj, "Flags", IndirectRequirement.Either);
        if (val == null) { return; }
        
        if (!(BitClear(obj)&&BitsClear(obj)&&BitsClear(obj))) 
        {
            ctx.Fail<APM_FontDescriptorTrueType_Flags>($"Value failed special case check: fn:Eval(fn:BitClear(5) && fn:BitsClear(8,16) && fn:BitsClear(20,32))");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorTrueType_FontBBox 
/// </summary>
internal partial class APM_FontDescriptorTrueType_FontBBox : APM_FontDescriptorTrueType_FontBBox__Base
{
}


internal partial class APM_FontDescriptorTrueType_FontBBox__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorTrueType_FontBBox";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_FontDescriptorTrueType_FontBBox>(obj, "FontBBox", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorTrueType_ItalicAngle 
/// </summary>
internal partial class APM_FontDescriptorTrueType_ItalicAngle : APM_FontDescriptorTrueType_ItalicAngle__Base
{
}


internal partial class APM_FontDescriptorTrueType_ItalicAngle__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorTrueType_ItalicAngle";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_FontDescriptorTrueType_ItalicAngle>(obj, "ItalicAngle", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorTrueType_Ascent 
/// </summary>
internal partial class APM_FontDescriptorTrueType_Ascent : APM_FontDescriptorTrueType_Ascent__Base
{
}


internal partial class APM_FontDescriptorTrueType_Ascent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorTrueType_Ascent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_FontDescriptorTrueType_Ascent>(obj, "Ascent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorTrueType_Descent https://github.com/pdf-association/pdf-issues/issues/190
/// </summary>
internal partial class APM_FontDescriptorTrueType_Descent : APM_FontDescriptorTrueType_Descent__Base
{
}


internal partial class APM_FontDescriptorTrueType_Descent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorTrueType_Descent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_FontDescriptorTrueType_Descent>(obj, "Descent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var Descent = obj.Get("Descent");
        if (!(lte(Descent,0))) 
        {
            ctx.Fail<APM_FontDescriptorTrueType_Descent>($"Invalid value {val}, allowed are: [fn:Eval(@Descent<=0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorTrueType_Leading 
/// </summary>
internal partial class APM_FontDescriptorTrueType_Leading : APM_FontDescriptorTrueType_Leading__Base
{
}


internal partial class APM_FontDescriptorTrueType_Leading__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorTrueType_Leading";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorTrueType_Leading>(obj, "Leading", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorTrueType_CapHeight 
/// </summary>
internal partial class APM_FontDescriptorTrueType_CapHeight : APM_FontDescriptorTrueType_CapHeight__Base
{
}


internal partial class APM_FontDescriptorTrueType_CapHeight__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorTrueType_CapHeight";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorTrueType_CapHeight>(obj, "CapHeight", IndirectRequirement.Either);
        if ((FontHasLatinChars(obj)) && val == null) {
            ctx.Fail<APM_FontDescriptorTrueType_CapHeight>("CapHeight is required when 'fn:IsRequired(fn:FontHasLatinChars())"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorTrueType_XHeight 
/// </summary>
internal partial class APM_FontDescriptorTrueType_XHeight : APM_FontDescriptorTrueType_XHeight__Base
{
}


internal partial class APM_FontDescriptorTrueType_XHeight__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorTrueType_XHeight";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorTrueType_XHeight>(obj, "XHeight", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorTrueType_StemV 
/// </summary>
internal partial class APM_FontDescriptorTrueType_StemV : APM_FontDescriptorTrueType_StemV__Base
{
}


internal partial class APM_FontDescriptorTrueType_StemV__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorTrueType_StemV";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_FontDescriptorTrueType_StemV>(obj, "StemV", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorTrueType_StemH 
/// </summary>
internal partial class APM_FontDescriptorTrueType_StemH : APM_FontDescriptorTrueType_StemH__Base
{
}


internal partial class APM_FontDescriptorTrueType_StemH__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorTrueType_StemH";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorTrueType_StemH>(obj, "StemH", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorTrueType_AvgWidth 
/// </summary>
internal partial class APM_FontDescriptorTrueType_AvgWidth : APM_FontDescriptorTrueType_AvgWidth__Base
{
}


internal partial class APM_FontDescriptorTrueType_AvgWidth__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorTrueType_AvgWidth";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorTrueType_AvgWidth>(obj, "AvgWidth", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorTrueType_MaxWidth 
/// </summary>
internal partial class APM_FontDescriptorTrueType_MaxWidth : APM_FontDescriptorTrueType_MaxWidth__Base
{
}


internal partial class APM_FontDescriptorTrueType_MaxWidth__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorTrueType_MaxWidth";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorTrueType_MaxWidth>(obj, "MaxWidth", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorTrueType_MissingWidth 
/// </summary>
internal partial class APM_FontDescriptorTrueType_MissingWidth : APM_FontDescriptorTrueType_MissingWidth__Base
{
}


internal partial class APM_FontDescriptorTrueType_MissingWidth__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorTrueType_MissingWidth";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorTrueType_MissingWidth>(obj, "MissingWidth", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorTrueType_FontFile Table 124
/// </summary>
internal partial class APM_FontDescriptorTrueType_FontFile : APM_FontDescriptorTrueType_FontFile__Base
{
}


internal partial class APM_FontDescriptorTrueType_FontFile__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorTrueType_FontFile";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_FontDescriptorTrueType_FontFile>(obj, "FontFile", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        
        if (!(!obj.ContainsKey("FontFile2"))) 
        {
            ctx.Fail<APM_FontDescriptorTrueType_FontFile>($"Value failed special case check: fn:Eval(fn:Not(fn:IsPresent(FontFile2)))");
        }
        // no value restrictions
        ctx.Run<APM_FontFileType1, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// FontDescriptorTrueType_FontFile2 Table 124
/// </summary>
internal partial class APM_FontDescriptorTrueType_FontFile2 : APM_FontDescriptorTrueType_FontFile2__Base
{
}


internal partial class APM_FontDescriptorTrueType_FontFile2__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorTrueType_FontFile2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_FontDescriptorTrueType_FontFile2>(obj, "FontFile2", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        
        if (!(!obj.ContainsKey("FontFile"))) 
        {
            ctx.Fail<APM_FontDescriptorTrueType_FontFile2>($"Value failed special case check: fn:Eval(fn:Not(fn:IsPresent(FontFile)))");
        }
        // no value restrictions
        ctx.Run<APM_FontFile2, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

