// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_FontDescriptorType1 : APM_FontDescriptorType1_Base
{
}

internal partial class APM_FontDescriptorType1_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "FontDescriptorType1";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_FontDescriptorType1_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType1_FontName, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType1_FontFamily, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType1_FontStretch, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType1_FontWeight, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType1_Flags, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType1_FontBBox, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType1_ItalicAngle, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType1_Ascent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType1_Descent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType1_Leading, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType1_CapHeight, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType1_XHeight, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType1_StemV, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType1_StemH, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType1_AvgWidth, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType1_MaxWidth, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType1_MissingWidth, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType1_FontFile, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType1_FontFile3, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontDescriptorType1_CharSet, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_10.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType1>($"Unknown field {extra} for version 1.0");
                }
                break;
            case 1.1m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_11.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType1>($"Unknown field {extra} for version 1.1");
                }
                break;
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType1>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType1>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType1>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType1>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType1>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType1>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType1>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType1>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_FontDescriptorType1>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_FontDescriptorType1_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_10 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile"
    };
    public static HashSet<string> AllowedFields_11 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "CharSet"
    };
    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile3", "CharSet"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile3", "CharSet"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile3", "CharSet"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile3", "CharSet"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile3", "CharSet"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile3", "CharSet"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile3", "CharSet"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile3", "CharSet"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "FontName", "FontFamily", "FontStretch", "FontWeight", "Flags", "FontBBox", "ItalicAngle", "Ascent", "Descent", "Leading", "CapHeight", "XHeight", "StemV", "StemH", "AvgWidth", "MaxWidth", "MissingWidth", "FontFile", "FontFile3"
    };
    


}

/// <summary>
/// FontDescriptorType1_Type Table 120
/// </summary>
internal partial class APM_FontDescriptorType1_Type : APM_FontDescriptorType1_Type_Base
{
}


internal partial class APM_FontDescriptorType1_Type_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FontDescriptorType1_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "FontDescriptor")) 
        {
            ctx.Fail<APM_FontDescriptorType1_Type>($"Invalid value {val}, allowed are: [FontDescriptor]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType1_FontName 
/// </summary>
internal partial class APM_FontDescriptorType1_FontName : APM_FontDescriptorType1_FontName_Base
{
}


internal partial class APM_FontDescriptorType1_FontName_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_FontName";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FontDescriptorType1_FontName>(obj, "FontName", IndirectRequirement.Either);
        if (val == null) { return; }
        // TODO special case
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType1_FontFamily 
/// </summary>
internal partial class APM_FontDescriptorType1_FontFamily : APM_FontDescriptorType1_FontFamily_Base
{
}


internal partial class APM_FontDescriptorType1_FontFamily_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_FontFamily";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FontDescriptorType1_FontFamily>(obj, "FontFamily", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType1_FontStretch 
/// </summary>
internal partial class APM_FontDescriptorType1_FontStretch : APM_FontDescriptorType1_FontStretch_Base
{
}


internal partial class APM_FontDescriptorType1_FontStretch_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_FontStretch";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_FontDescriptorType1_FontStretch>(obj, "FontStretch", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "UltraCondensed" || val == "ExtraCondensed" || val == "Condensed" || val == "SemiCondensed" || val == "Normal" || val == "SemiExpanded" || val == "Expanded" || val == "ExtraExpanded" || val == "UltraExpanded")) 
        {
            ctx.Fail<APM_FontDescriptorType1_FontStretch>($"Invalid value {val}, allowed are: [UltraCondensed,ExtraCondensed,Condensed,SemiCondensed,Normal,SemiExpanded,Expanded,ExtraExpanded,UltraExpanded]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType1_FontWeight 
/// </summary>
internal partial class APM_FontDescriptorType1_FontWeight : APM_FontDescriptorType1_FontWeight_Base
{
}


internal partial class APM_FontDescriptorType1_FontWeight_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_FontWeight";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FontDescriptorType1_FontWeight>(obj, "FontWeight", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == 100 || val == 200 || val == 300 || val == 400 || val == 500 || val == 600 || val == 700 || val == 800 || val == 900)) 
        {
            ctx.Fail<APM_FontDescriptorType1_FontWeight>($"Invalid value {val}, allowed are: [100,200,300,400,500,600,700,800,900]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType1_Flags Table 121
/// </summary>
internal partial class APM_FontDescriptorType1_Flags : APM_FontDescriptorType1_Flags_Base
{
}


internal partial class APM_FontDescriptorType1_Flags_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_Flags";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_FontDescriptorType1_Flags>(obj, "Flags", IndirectRequirement.Either);
        if (val == null) { return; }
        // TODO special case
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType1_FontBBox 
/// </summary>
internal partial class APM_FontDescriptorType1_FontBBox : APM_FontDescriptorType1_FontBBox_Base
{
}


internal partial class APM_FontDescriptorType1_FontBBox_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_FontBBox";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_FontDescriptorType1_FontBBox>(obj, "FontBBox", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType1_ItalicAngle 
/// </summary>
internal partial class APM_FontDescriptorType1_ItalicAngle : APM_FontDescriptorType1_ItalicAngle_Base
{
}


internal partial class APM_FontDescriptorType1_ItalicAngle_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_ItalicAngle";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_FontDescriptorType1_ItalicAngle>(obj, "ItalicAngle", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType1_Ascent 
/// </summary>
internal partial class APM_FontDescriptorType1_Ascent : APM_FontDescriptorType1_Ascent_Base
{
}


internal partial class APM_FontDescriptorType1_Ascent_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_Ascent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_FontDescriptorType1_Ascent>(obj, "Ascent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType1_Descent https://github.com/pdf-association/pdf-issues/issues/190
/// </summary>
internal partial class APM_FontDescriptorType1_Descent : APM_FontDescriptorType1_Descent_Base
{
}


internal partial class APM_FontDescriptorType1_Descent_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_Descent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_FontDescriptorType1_Descent>(obj, "Descent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        IPdfObject @Descent = val;
        if (!(lte(@Descent,0))) 
        {
            ctx.Fail<APM_FontDescriptorType1_Descent>($"Invalid value {val}, allowed are: [fn:Eval(@Descent<=0)]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType1_Leading 
/// </summary>
internal partial class APM_FontDescriptorType1_Leading : APM_FontDescriptorType1_Leading_Base
{
}


internal partial class APM_FontDescriptorType1_Leading_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_Leading";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorType1_Leading>(obj, "Leading", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType1_CapHeight 
/// </summary>
internal partial class APM_FontDescriptorType1_CapHeight : APM_FontDescriptorType1_CapHeight_Base
{
}


internal partial class APM_FontDescriptorType1_CapHeight_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_CapHeight";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorType1_CapHeight>(obj, "CapHeight", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType1_XHeight 
/// </summary>
internal partial class APM_FontDescriptorType1_XHeight : APM_FontDescriptorType1_XHeight_Base
{
}


internal partial class APM_FontDescriptorType1_XHeight_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_XHeight";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorType1_XHeight>(obj, "XHeight", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType1_StemV 
/// </summary>
internal partial class APM_FontDescriptorType1_StemV : APM_FontDescriptorType1_StemV_Base
{
}


internal partial class APM_FontDescriptorType1_StemV_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_StemV";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_FontDescriptorType1_StemV>(obj, "StemV", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType1_StemH 
/// </summary>
internal partial class APM_FontDescriptorType1_StemH : APM_FontDescriptorType1_StemH_Base
{
}


internal partial class APM_FontDescriptorType1_StemH_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_StemH";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorType1_StemH>(obj, "StemH", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType1_AvgWidth 
/// </summary>
internal partial class APM_FontDescriptorType1_AvgWidth : APM_FontDescriptorType1_AvgWidth_Base
{
}


internal partial class APM_FontDescriptorType1_AvgWidth_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_AvgWidth";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorType1_AvgWidth>(obj, "AvgWidth", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType1_MaxWidth 
/// </summary>
internal partial class APM_FontDescriptorType1_MaxWidth : APM_FontDescriptorType1_MaxWidth_Base
{
}


internal partial class APM_FontDescriptorType1_MaxWidth_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_MaxWidth";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorType1_MaxWidth>(obj, "MaxWidth", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType1_MissingWidth 
/// </summary>
internal partial class APM_FontDescriptorType1_MissingWidth : APM_FontDescriptorType1_MissingWidth_Base
{
}


internal partial class APM_FontDescriptorType1_MissingWidth_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_MissingWidth";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_FontDescriptorType1_MissingWidth>(obj, "MissingWidth", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontDescriptorType1_FontFile Table 124
/// </summary>
internal partial class APM_FontDescriptorType1_FontFile : APM_FontDescriptorType1_FontFile_Base
{
}


internal partial class APM_FontDescriptorType1_FontFile_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_FontFile";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_FontDescriptorType1_FontFile>(obj, "FontFile", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_FontFileType1, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// FontDescriptorType1_FontFile3 Table 124
/// </summary>
internal partial class APM_FontDescriptorType1_FontFile3 : APM_FontDescriptorType1_FontFile3_Base
{
}


internal partial class APM_FontDescriptorType1_FontFile3_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_FontFile3";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_FontDescriptorType1_FontFile3>(obj, "FontFile3", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_FontFile3Type1, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// FontDescriptorType1_CharSet 
/// </summary>
internal partial class APM_FontDescriptorType1_CharSet : APM_FontDescriptorType1_CharSet_Base
{
}


internal partial class APM_FontDescriptorType1_CharSet_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontDescriptorType1_CharSet";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_FontDescriptorType1_CharSet>(obj, "CharSet", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            
            // TODO MC string-ascii;string-byte
            
            default:
                ctx.Fail<APM_FontDescriptorType1_CharSet>("CharSet is required to one of 'string-ascii;string-byte', was " + utval.Type);
                return;
        }
    }


}

