// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_PaperMetaData : APM_PaperMetaData__Base
{
}

internal partial class APM_PaperMetaData__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "PaperMetaData";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_PaperMetaData_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PaperMetaData_Version, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PaperMetaData_Resolution, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PaperMetaData_Caption, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PaperMetaData_Symbology, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PaperMetaData_Width, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PaperMetaData_Height, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PaperMetaData_XSymWidth, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PaperMetaData_YSymHeight, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PaperMetaData_ECC, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PaperMetaData_nCodeWordRow, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PaperMetaData_nCodeWordCol, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_PaperMetaData>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_PaperMetaData>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_PaperMetaData>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_PaperMetaData>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_PaperMetaData_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "Version", "Resolution", "Caption", "Symbology", "Width", "Height", "XSymWidth", "YSymHeight", "ECC", "nCodeWordRow", "nCodeWordCol"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "Version", "Resolution", "Caption", "Symbology", "Width", "Height", "XSymWidth", "YSymHeight", "ECC", "nCodeWordRow", "nCodeWordCol"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "Version", "Resolution", "Caption", "Symbology", "Width", "Height", "XSymWidth", "YSymHeight", "ECC", "nCodeWordRow", "nCodeWordCol"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "Version", "Resolution", "Caption", "Symbology", "Width", "Height", "XSymWidth", "YSymHeight", "ECC", "nCodeWordRow", "nCodeWordCol"
    };
    


}

/// <summary>
/// PaperMetaData_Type Adobe Extension Level 3, Table 8.39b
/// </summary>
internal partial class APM_PaperMetaData_Type : APM_PaperMetaData_Type__Base
{
}


internal partial class APM_PaperMetaData_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PaperMetaData_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_PaperMetaData_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.PaperMetaData)) 
        {
            ctx.Fail<APM_PaperMetaData_Type>($"Invalid value {val}, allowed are: [PaperMetaData]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// PaperMetaData_Version 
/// </summary>
internal partial class APM_PaperMetaData_Version : APM_PaperMetaData_Version__Base
{
}


internal partial class APM_PaperMetaData_Version__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PaperMetaData_Version";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_PaperMetaData_Version>(obj, "Version", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 1m)) 
        {
            ctx.Fail<APM_PaperMetaData_Version>($"Invalid value {val}, allowed are: [1]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// PaperMetaData_Resolution 
/// </summary>
internal partial class APM_PaperMetaData_Resolution : APM_PaperMetaData_Resolution__Base
{
}


internal partial class APM_PaperMetaData_Resolution__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PaperMetaData_Resolution";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_PaperMetaData_Resolution>(obj, "Resolution", IndirectRequirement.Either);
        if (val == null) { return; }
        var Resolution = obj.Get("Resolution");
        if (!(gt(Resolution,0))) 
        {
            ctx.Fail<APM_PaperMetaData_Resolution>($"Value failed special case check: fn:Eval(@Resolution>0)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// PaperMetaData_Caption 
/// </summary>
internal partial class APM_PaperMetaData_Caption : APM_PaperMetaData_Caption__Base
{
}


internal partial class APM_PaperMetaData_Caption__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PaperMetaData_Caption";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_PaperMetaData_Caption>(obj, "Caption", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// PaperMetaData_Symbology 
/// </summary>
internal partial class APM_PaperMetaData_Symbology : APM_PaperMetaData_Symbology__Base
{
}


internal partial class APM_PaperMetaData_Symbology__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PaperMetaData_Symbology";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_PaperMetaData_Symbology>(obj, "Symbology", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.PDF417 || val == PdfName.QRCode || val == PdfName.DataMatrix)) 
        {
            ctx.Fail<APM_PaperMetaData_Symbology>($"Invalid value {val}, allowed are: [PDF417,QRCode,DataMatrix]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// PaperMetaData_Width 
/// </summary>
internal partial class APM_PaperMetaData_Width : APM_PaperMetaData_Width__Base
{
}


internal partial class APM_PaperMetaData_Width__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PaperMetaData_Width";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_PaperMetaData_Width>(obj, "Width", IndirectRequirement.Either);
        if (val == null) { return; }
        var Width = obj.Get("Width");
        if (!(gt(Width,0))) 
        {
            ctx.Fail<APM_PaperMetaData_Width>($"Value failed special case check: fn:Eval(@Width>0)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// PaperMetaData_Height 
/// </summary>
internal partial class APM_PaperMetaData_Height : APM_PaperMetaData_Height__Base
{
}


internal partial class APM_PaperMetaData_Height__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PaperMetaData_Height";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_PaperMetaData_Height>(obj, "Height", IndirectRequirement.Either);
        if (val == null) { return; }
        var Height = obj.Get("Height");
        if (!(gt(Height,0))) 
        {
            ctx.Fail<APM_PaperMetaData_Height>($"Value failed special case check: fn:Eval(@Height>0)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// PaperMetaData_XSymWidth 
/// </summary>
internal partial class APM_PaperMetaData_XSymWidth : APM_PaperMetaData_XSymWidth__Base
{
}


internal partial class APM_PaperMetaData_XSymWidth__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PaperMetaData_XSymWidth";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_PaperMetaData_XSymWidth>(obj, "XSymWidth", IndirectRequirement.Either);
        if (val == null) { return; }
        var XSymWidth = obj.Get("XSymWidth");
        if (!(gt(XSymWidth,0))) 
        {
            ctx.Fail<APM_PaperMetaData_XSymWidth>($"Value failed special case check: fn:Eval(@XSymWidth>0)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// PaperMetaData_YSymHeight 
/// </summary>
internal partial class APM_PaperMetaData_YSymHeight : APM_PaperMetaData_YSymHeight__Base
{
}


internal partial class APM_PaperMetaData_YSymHeight__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PaperMetaData_YSymHeight";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_PaperMetaData_YSymHeight>(obj, "YSymHeight", IndirectRequirement.Either);
        if (val == null) { return; }
        var YSymHeight = obj.Get("YSymHeight");
        if (!(gt(YSymHeight,0))) 
        {
            ctx.Fail<APM_PaperMetaData_YSymHeight>($"Value failed special case check: fn:Eval(@YSymHeight>0)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// PaperMetaData_ECC 
/// </summary>
internal partial class APM_PaperMetaData_ECC : APM_PaperMetaData_ECC__Base
{
}


internal partial class APM_PaperMetaData_ECC__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PaperMetaData_ECC";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var Symbology = obj.Get("Symbology");
        var val = ctx.GetOptional<PdfIntNumber, APM_PaperMetaData_ECC>(obj, "ECC", IndirectRequirement.Either);
        if (((eq(Symbology,PdfName.PDF417)||eq(Symbology,PdfName.QRCode))) && val == null) {
            ctx.Fail<APM_PaperMetaData_ECC>("ECC is required when 'fn:IsRequired((@Symbology==PDF417) || (@Symbology==QRCode))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        
        var ECC = obj.Get("ECC");
        if (!((gte(ECC,0)&&((eq(Symbology,PdfName.PDF417)&&lte(ECC,8))||(eq(Symbology,PdfName.QRCode)&&lte(ECC,3)))))) 
        {
            ctx.Fail<APM_PaperMetaData_ECC>($"Invalid value {val}, allowed are: [fn:Eval((@ECC>=0) && (((@Symbology==PDF417) && (@ECC<=8)) || ((@Symbology==QRCode) && (@ECC<=3))))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// PaperMetaData_nCodeWordRow 
/// </summary>
internal partial class APM_PaperMetaData_nCodeWordRow : APM_PaperMetaData_nCodeWordRow__Base
{
}


internal partial class APM_PaperMetaData_nCodeWordRow__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PaperMetaData_nCodeWordRow";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_PaperMetaData_nCodeWordRow>(obj, "nCodeWordRow", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:IsMeaningful, not pertinent to validation
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// PaperMetaData_nCodeWordCol 
/// </summary>
internal partial class APM_PaperMetaData_nCodeWordCol : APM_PaperMetaData_nCodeWordCol__Base
{
}


internal partial class APM_PaperMetaData_nCodeWordCol__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PaperMetaData_nCodeWordCol";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_PaperMetaData_nCodeWordCol>(obj, "nCodeWordCol", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:IsMeaningful, not pertinent to validation
        // no value restrictions
        // no linked objects
        
    }


}
