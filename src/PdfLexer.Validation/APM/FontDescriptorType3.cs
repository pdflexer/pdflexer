// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_FontDescriptorType3 : APM_FontDescriptorType3__Base
{
}

internal partial class APM_FontDescriptorType3__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "FontDescriptorType3";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_FontDescriptorType3_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType3_FontName, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType3_FontFamily, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType3_FontStretch, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType3_FontWeight, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType3_Flags, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType3_FontBBox, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType3_ItalicAngle, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType3_Ascent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType3_Descent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType3_Leading, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType3_CapHeight, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType3_XHeight, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType3_StemV, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType3_StemH, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType3_AvgWidth, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType3_MaxWidth, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType3_MissingWidth, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_10.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType3>($"Unknown field {extra} for version 1.0");
                }
                break;
            case 1.1m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_11.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType3>($"Unknown field {extra} for version 1.1");
                }
                break;
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType3>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType3>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType3>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType3>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType3>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType3>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType3>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType3>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType3>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_FontDescriptorType3_Type, PdfDictionary>(new CallStack(), obj, null);
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
        "Type", "FontName", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth"
    };
    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth"
    };
    


}

/// <summary>
/// FontDescriptorType3_Type Table 120
/// </summary>
internal partial class APM_FontDescriptorType3_Type : APM_FontDescriptorType3_Type__Base
{
}


internal partial class APM_FontDescriptorType3_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType3_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FontDescriptorType3_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.FontDescriptor)) 
        {
            ctx.Fail<APM_FontDescriptorType3_Type>($"Invalid value {val}, allowed are: [FontDescriptor]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType3_FontName 
/// </summary>
internal partial class APM_FontDescriptorType3_FontName : APM_FontDescriptorType3_FontName__Base
{
}


internal partial class APM_FontDescriptorType3_FontName__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType3_FontName";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_FontDescriptorType3_FontName>(obj, "FontName", IndirectRequirement.Either);
        if (val == null) { return; }
        var FontName = obj.Get("FontName");
        var parentName = parent?.Get("Name");
        if (!(eq(FontName,parentName))) 
        {
            ctx.Fail<APM_FontDescriptorType3_FontName>($"Value failed special case check: fn:Eval(@FontName==parent::@Name)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType3_FontFamily 
/// </summary>
internal partial class APM_FontDescriptorType3_FontFamily : APM_FontDescriptorType3_FontFamily__Base
{
}


internal partial class APM_FontDescriptorType3_FontFamily__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType3_FontFamily";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FontDescriptorType3_FontFamily>(obj, "FontFamily", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType3_FontStretch 
/// </summary>
internal partial class APM_FontDescriptorType3_FontStretch : APM_FontDescriptorType3_FontStretch__Base
{
}


internal partial class APM_FontDescriptorType3_FontStretch__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType3_FontStretch";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_FontDescriptorType3_FontStretch>(obj, "FontStretch", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.UltraCondensed || val == PdfName.ExtraCondensed || val == PdfName.Condensed || val == PdfName.SemiCondensed || val == PdfName.Normal || val == PdfName.SemiExpanded || val == PdfName.Expanded || val == PdfName.ExtraExpanded || val == PdfName.UltraExpanded)) 
        {
            ctx.Fail<APM_FontDescriptorType3_FontStretch>($"Invalid value {val}, allowed are: [UltraCondensed,ExtraCondensed,Condensed,SemiCondensed,Normal,SemiExpanded,Expanded,ExtraExpanded,UltraExpanded]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType3_FontWeight 
/// </summary>
internal partial class APM_FontDescriptorType3_FontWeight : APM_FontDescriptorType3_FontWeight__Base
{
}


internal partial class APM_FontDescriptorType3_FontWeight__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType3_FontWeight";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FontDescriptorType3_FontWeight>(obj, "FontWeight", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 100m || val == 200m || val == 300m || val == 400m || val == 500m || val == 600m || val == 700m || val == 800m || val == 900m)) 
        {
            ctx.Fail<APM_FontDescriptorType3_FontWeight>($"Invalid value {val}, allowed are: [100,200,300,400,500,600,700,800,900]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType3_Flags Table 121
/// </summary>
internal partial class APM_FontDescriptorType3_Flags : APM_FontDescriptorType3_Flags__Base
{
}


internal partial class APM_FontDescriptorType3_Flags__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType3_Flags";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_FontDescriptorType3_Flags>(obj, "Flags", IndirectRequirement.Either);
        if (val == null) { return; }
        
        if (!(BitsClear(val,0b00000000000000000000000000010000)&&BitsClear(val,0b00000000000000001111111110000000)&&BitsClear(val,0b11111111111110000000000000000000))) 
        {
            ctx.Fail<APM_FontDescriptorType3_Flags>($"Value failed special case check: fn:Eval(fn:BitClear(5) && fn:BitsClear(8,16) && fn:BitsClear(20,32))");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType3_FontBBox 
/// </summary>
internal partial class APM_FontDescriptorType3_FontBBox : APM_FontDescriptorType3_FontBBox__Base
{
}


internal partial class APM_FontDescriptorType3_FontBBox__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType3_FontBBox";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_FontDescriptorType3_FontBBox>(obj, "FontBBox", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType3_ItalicAngle 
/// </summary>
internal partial class APM_FontDescriptorType3_ItalicAngle : APM_FontDescriptorType3_ItalicAngle__Base
{
}


internal partial class APM_FontDescriptorType3_ItalicAngle__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType3_ItalicAngle";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_FontDescriptorType3_ItalicAngle>(obj, "ItalicAngle", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType3_Ascent 
/// </summary>
internal partial class APM_FontDescriptorType3_Ascent : APM_FontDescriptorType3_Ascent__Base
{
}


internal partial class APM_FontDescriptorType3_Ascent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType3_Ascent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorType3_Ascent>(obj, "Ascent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType3_Descent https://github.com/pdf-association/pdf-issues/issues/190
/// </summary>
internal partial class APM_FontDescriptorType3_Descent : APM_FontDescriptorType3_Descent__Base
{
}


internal partial class APM_FontDescriptorType3_Descent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType3_Descent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorType3_Descent>(obj, "Descent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var Descent = obj.Get("Descent");
        if (!(lte(Descent,0))) 
        {
            ctx.Fail<APM_FontDescriptorType3_Descent>($"Invalid value {val}, allowed are: [fn:Eval(@Descent<=0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType3_Leading 
/// </summary>
internal partial class APM_FontDescriptorType3_Leading : APM_FontDescriptorType3_Leading__Base
{
}


internal partial class APM_FontDescriptorType3_Leading__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType3_Leading";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorType3_Leading>(obj, "Leading", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType3_CapHeight 
/// </summary>
internal partial class APM_FontDescriptorType3_CapHeight : APM_FontDescriptorType3_CapHeight__Base
{
}


internal partial class APM_FontDescriptorType3_CapHeight__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType3_CapHeight";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorType3_CapHeight>(obj, "CapHeight", IndirectRequirement.Either);
        if ((FontHasLatinChars(obj)) && val == null) {
            ctx.Fail<APM_FontDescriptorType3_CapHeight>("CapHeight is required when 'fn:IsRequired(fn:FontHasLatinChars())"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType3_XHeight 
/// </summary>
internal partial class APM_FontDescriptorType3_XHeight : APM_FontDescriptorType3_XHeight__Base
{
}


internal partial class APM_FontDescriptorType3_XHeight__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType3_XHeight";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorType3_XHeight>(obj, "XHeight", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType3_StemV 
/// </summary>
internal partial class APM_FontDescriptorType3_StemV : APM_FontDescriptorType3_StemV__Base
{
}


internal partial class APM_FontDescriptorType3_StemV__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType3_StemV";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorType3_StemV>(obj, "StemV", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType3_StemH 
/// </summary>
internal partial class APM_FontDescriptorType3_StemH : APM_FontDescriptorType3_StemH__Base
{
}


internal partial class APM_FontDescriptorType3_StemH__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType3_StemH";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorType3_StemH>(obj, "StemH", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType3_AvgWidth 
/// </summary>
internal partial class APM_FontDescriptorType3_AvgWidth : APM_FontDescriptorType3_AvgWidth__Base
{
}


internal partial class APM_FontDescriptorType3_AvgWidth__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType3_AvgWidth";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorType3_AvgWidth>(obj, "AvgWidth", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType3_MaxWidth 
/// </summary>
internal partial class APM_FontDescriptorType3_MaxWidth : APM_FontDescriptorType3_MaxWidth__Base
{
}


internal partial class APM_FontDescriptorType3_MaxWidth__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType3_MaxWidth";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorType3_MaxWidth>(obj, "MaxWidth", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType3_MissingWidth 
/// </summary>
internal partial class APM_FontDescriptorType3_MissingWidth : APM_FontDescriptorType3_MissingWidth__Base
{
}


internal partial class APM_FontDescriptorType3_MissingWidth__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType3_MissingWidth";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorType3_MissingWidth>(obj, "MissingWidth", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}
