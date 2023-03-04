// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_FontDescriptorCIDType2 : APM_FontDescriptorCIDType2_Base
{
}

internal partial class APM_FontDescriptorCIDType2_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "FontDescriptorCIDType2";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_FontDescriptorCIDType2_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_FontName, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_FontFamily, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_FontStretch, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_FontWeight, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_Flags, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_FontBBox, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_ItalicAngle, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_Ascent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_Descent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_Leading, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_CapHeight, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_XHeight, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_StemV, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_StemH, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_AvgWidth, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_MaxWidth, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_MissingWidth, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_FontFile, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_FontFile2, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_Style, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_Lang, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_FD, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorCIDType2_CIDSet, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorCIDType2>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorCIDType2>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorCIDType2>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorCIDType2>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorCIDType2>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorCIDType2>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorCIDType2>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorCIDType2>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorCIDType2>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_FontDescriptorCIDType2_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile2", "Style", "FD", "CIDSet"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile2", "Style", "FD", "CIDSet"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile2", "Style", "FD", "CIDSet"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile2", "Style", "Lang", "FD", "CIDSet"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile2", "Style", "Lang", "FD", "CIDSet"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile2", "Style", "Lang", "FD", "CIDSet"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile2", "Style", "Lang", "FD", "CIDSet"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile2", "Style", "Lang", "FD", "CIDSet"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile2", "Style", "Lang", "FD"
    };
    


}

/// <summary>
/// FontDescriptorCIDType2_Type Table 120 and Table 122
/// </summary>
internal partial class APM_FontDescriptorCIDType2_Type : APM_FontDescriptorCIDType2_Type_Base
{
}


internal partial class APM_FontDescriptorCIDType2_Type_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FontDescriptorCIDType2_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "FontDescriptor")) 
        {
            ctx.Fail<APM_FontDescriptorCIDType2_Type>($"Invalid value {val}, allowed are: [FontDescriptor]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_FontName 
/// </summary>
internal partial class APM_FontDescriptorCIDType2_FontName : APM_FontDescriptorCIDType2_FontName_Base
{
}


internal partial class APM_FontDescriptorCIDType2_FontName_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_FontName";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FontDescriptorCIDType2_FontName>(obj, "FontName", IndirectRequirement.Either);
        if (val == null) { return; }
        // TODO special case
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_FontFamily 
/// </summary>
internal partial class APM_FontDescriptorCIDType2_FontFamily : APM_FontDescriptorCIDType2_FontFamily_Base
{
}


internal partial class APM_FontDescriptorCIDType2_FontFamily_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_FontFamily";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FontDescriptorCIDType2_FontFamily>(obj, "FontFamily", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_FontStretch 
/// </summary>
internal partial class APM_FontDescriptorCIDType2_FontStretch : APM_FontDescriptorCIDType2_FontStretch_Base
{
}


internal partial class APM_FontDescriptorCIDType2_FontStretch_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_FontStretch";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_FontDescriptorCIDType2_FontStretch>(obj, "FontStretch", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "UltraCondensed" || val == "ExtraCondensed" || val == "Condensed" || val == "SemiCondensed" || val == "Normal" || val == "SemiExpanded" || val == "Expanded" || val == "ExtraExpanded" || val == "UltraExpanded")) 
        {
            ctx.Fail<APM_FontDescriptorCIDType2_FontStretch>($"Invalid value {val}, allowed are: [UltraCondensed,ExtraCondensed,Condensed,SemiCondensed,Normal,SemiExpanded,Expanded,ExtraExpanded,UltraExpanded]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_FontWeight 
/// </summary>
internal partial class APM_FontDescriptorCIDType2_FontWeight : APM_FontDescriptorCIDType2_FontWeight_Base
{
}


internal partial class APM_FontDescriptorCIDType2_FontWeight_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_FontWeight";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FontDescriptorCIDType2_FontWeight>(obj, "FontWeight", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == 100 || val == 200 || val == 300 || val == 400 || val == 500 || val == 600 || val == 700 || val == 800 || val == 900)) 
        {
            ctx.Fail<APM_FontDescriptorCIDType2_FontWeight>($"Invalid value {val}, allowed are: [100,200,300,400,500,600,700,800,900]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_Flags Table 121
/// </summary>
internal partial class APM_FontDescriptorCIDType2_Flags : APM_FontDescriptorCIDType2_Flags_Base
{
}


internal partial class APM_FontDescriptorCIDType2_Flags_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_Flags";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_FontDescriptorCIDType2_Flags>(obj, "Flags", IndirectRequirement.Either);
        if (val == null) { return; }
        // TODO special case
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_FontBBox 
/// </summary>
internal partial class APM_FontDescriptorCIDType2_FontBBox : APM_FontDescriptorCIDType2_FontBBox_Base
{
}


internal partial class APM_FontDescriptorCIDType2_FontBBox_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_FontBBox";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_FontDescriptorCIDType2_FontBBox>(obj, "FontBBox", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_ItalicAngle 
/// </summary>
internal partial class APM_FontDescriptorCIDType2_ItalicAngle : APM_FontDescriptorCIDType2_ItalicAngle_Base
{
}


internal partial class APM_FontDescriptorCIDType2_ItalicAngle_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_ItalicAngle";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_FontDescriptorCIDType2_ItalicAngle>(obj, "ItalicAngle", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_Ascent 
/// </summary>
internal partial class APM_FontDescriptorCIDType2_Ascent : APM_FontDescriptorCIDType2_Ascent_Base
{
}


internal partial class APM_FontDescriptorCIDType2_Ascent_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_Ascent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_FontDescriptorCIDType2_Ascent>(obj, "Ascent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_Descent https://github.com/pdf-association/pdf-issues/issues/190
/// </summary>
internal partial class APM_FontDescriptorCIDType2_Descent : APM_FontDescriptorCIDType2_Descent_Base
{
}


internal partial class APM_FontDescriptorCIDType2_Descent_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_Descent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_FontDescriptorCIDType2_Descent>(obj, "Descent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        IPdfObject @Descent = val;
        if (!(lte(@Descent,0))) 
        {
            ctx.Fail<APM_FontDescriptorCIDType2_Descent>($"Invalid value {val}, allowed are: [fn:Eval(@Descent<=0)]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_Leading 
/// </summary>
internal partial class APM_FontDescriptorCIDType2_Leading : APM_FontDescriptorCIDType2_Leading_Base
{
}


internal partial class APM_FontDescriptorCIDType2_Leading_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_Leading";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorCIDType2_Leading>(obj, "Leading", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_CapHeight 
/// </summary>
internal partial class APM_FontDescriptorCIDType2_CapHeight : APM_FontDescriptorCIDType2_CapHeight_Base
{
}


internal partial class APM_FontDescriptorCIDType2_CapHeight_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_CapHeight";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        PdfNumber? val;
        {
            
            if (FontHasLatinChars(obj)) {
                val = ctx.GetRequired<PdfNumber, APM_FontDescriptorCIDType2_CapHeight>(obj, "CapHeight", IndirectRequirement.Either);
            } else {
                val = ctx.GetOptional<PdfNumber, APM_FontDescriptorCIDType2_CapHeight>(obj, "CapHeight", IndirectRequirement.Either);
            }
            if (val == null) { return; }
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_XHeight 
/// </summary>
internal partial class APM_FontDescriptorCIDType2_XHeight : APM_FontDescriptorCIDType2_XHeight_Base
{
}


internal partial class APM_FontDescriptorCIDType2_XHeight_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_XHeight";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorCIDType2_XHeight>(obj, "XHeight", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_StemV 
/// </summary>
internal partial class APM_FontDescriptorCIDType2_StemV : APM_FontDescriptorCIDType2_StemV_Base
{
}


internal partial class APM_FontDescriptorCIDType2_StemV_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_StemV";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_FontDescriptorCIDType2_StemV>(obj, "StemV", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_StemH 
/// </summary>
internal partial class APM_FontDescriptorCIDType2_StemH : APM_FontDescriptorCIDType2_StemH_Base
{
}


internal partial class APM_FontDescriptorCIDType2_StemH_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_StemH";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorCIDType2_StemH>(obj, "StemH", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_AvgWidth 
/// </summary>
internal partial class APM_FontDescriptorCIDType2_AvgWidth : APM_FontDescriptorCIDType2_AvgWidth_Base
{
}


internal partial class APM_FontDescriptorCIDType2_AvgWidth_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_AvgWidth";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorCIDType2_AvgWidth>(obj, "AvgWidth", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_MaxWidth 
/// </summary>
internal partial class APM_FontDescriptorCIDType2_MaxWidth : APM_FontDescriptorCIDType2_MaxWidth_Base
{
}


internal partial class APM_FontDescriptorCIDType2_MaxWidth_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_MaxWidth";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorCIDType2_MaxWidth>(obj, "MaxWidth", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_MissingWidth 
/// </summary>
internal partial class APM_FontDescriptorCIDType2_MissingWidth : APM_FontDescriptorCIDType2_MissingWidth_Base
{
}


internal partial class APM_FontDescriptorCIDType2_MissingWidth_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_MissingWidth";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorCIDType2_MissingWidth>(obj, "MissingWidth", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_FontFile Table 124
/// </summary>
internal partial class APM_FontDescriptorCIDType2_FontFile : APM_FontDescriptorCIDType2_FontFile_Base
{
}


internal partial class APM_FontDescriptorCIDType2_FontFile_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_FontFile";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_FontDescriptorCIDType2_FontFile>(obj, "FontFile", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // TODO special case
        // no value restrictions
        ctx.Run<APM_FontFile, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_FontFile2 Table 124
/// </summary>
internal partial class APM_FontDescriptorCIDType2_FontFile2 : APM_FontDescriptorCIDType2_FontFile2_Base
{
}


internal partial class APM_FontDescriptorCIDType2_FontFile2_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_FontFile2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_FontDescriptorCIDType2_FontFile2>(obj, "FontFile2", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // TODO special case
        // no value restrictions
        ctx.Run<APM_FontFile2, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_Style 
/// </summary>
internal partial class APM_FontDescriptorCIDType2_Style : APM_FontDescriptorCIDType2_Style_Base
{
}


internal partial class APM_FontDescriptorCIDType2_Style_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_Style";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_FontDescriptorCIDType2_Style>(obj, "Style", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_StyleDict, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_Lang 
/// </summary>
internal partial class APM_FontDescriptorCIDType2_Lang : APM_FontDescriptorCIDType2_Lang_Base
{
}


internal partial class APM_FontDescriptorCIDType2_Lang_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_Lang";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_FontDescriptorCIDType2_Lang>(obj, "Lang", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_FD 
/// </summary>
internal partial class APM_FontDescriptorCIDType2_FD : APM_FontDescriptorCIDType2_FD_Base
{
}


internal partial class APM_FontDescriptorCIDType2_FD_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_FD";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_FontDescriptorCIDType2_FD>(obj, "FD", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_FDDict, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// FontDescriptorCIDType2_CIDSet 
/// </summary>
internal partial class APM_FontDescriptorCIDType2_CIDSet : APM_FontDescriptorCIDType2_CIDSet_Base
{
}


internal partial class APM_FontDescriptorCIDType2_CIDSet_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorCIDType2_CIDSet";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_FontDescriptorCIDType2_CIDSet>(obj, "CIDSet", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

