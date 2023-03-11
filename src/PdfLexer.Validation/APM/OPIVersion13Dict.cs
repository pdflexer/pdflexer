// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_OPIVersion13Dict : APM_OPIVersion13Dict__Base
{
}

internal partial class APM_OPIVersion13Dict__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "OPIVersion13Dict";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_OPIVersion13Dict_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OPIVersion13Dict_Version, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OPIVersion13Dict_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OPIVersion13Dict_ID, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OPIVersion13Dict_Comments, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OPIVersion13Dict_Size, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OPIVersion13Dict_CropRect, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OPIVersion13Dict_CropFixed, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OPIVersion13Dict_Position, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OPIVersion13Dict_Resolution, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OPIVersion13Dict_ColorType, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OPIVersion13Dict_Color, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OPIVersion13Dict_Tint, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OPIVersion13Dict_Overprint, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OPIVersion13Dict_ImageType, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OPIVersion13Dict_GrayMap, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OPIVersion13Dict_Transparency, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OPIVersion13Dict_Tags, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_OPIVersion13Dict>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_OPIVersion13Dict>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_OPIVersion13Dict>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_OPIVersion13Dict>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_OPIVersion13Dict>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_OPIVersion13Dict>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_OPIVersion13Dict>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_OPIVersion13Dict>($"Unknown field {extra} for version 1.9");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_OPIVersion13Dict_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "Version", "F", "ID", "Comments", "Size", "CropRect", "CropFixed", "Position", "Resolution", "ColorType", "Color", "Tint", "Overprint", "ImageType", "GrayMap", "Transparency", "Tags"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "Version", "F", "ID", "Comments", "Size", "CropRect", "CropFixed", "Position", "Resolution", "ColorType", "Color", "Tint", "Overprint", "ImageType", "GrayMap", "Transparency", "Tags"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "Version", "F", "ID", "Comments", "Size", "CropRect", "CropFixed", "Position", "Resolution", "ColorType", "Color", "Tint", "Overprint", "ImageType", "GrayMap", "Transparency", "Tags"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "Version", "F", "ID", "Comments", "Size", "CropRect", "CropFixed", "Position", "Resolution", "ColorType", "Color", "Tint", "Overprint", "ImageType", "GrayMap", "Transparency", "Tags"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "Version", "F", "ID", "Comments", "Size", "CropRect", "CropFixed", "Position", "Resolution", "ColorType", "Color", "Tint", "Overprint", "ImageType", "GrayMap", "Transparency", "Tags"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "Version", "F", "ID", "Comments", "Size", "CropRect", "CropFixed", "Position", "Resolution", "ColorType", "Color", "Tint", "Overprint", "ImageType", "GrayMap", "Transparency", "Tags"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "Version", "F", "ID", "Comments", "Size", "CropRect", "CropFixed", "Position", "Resolution", "ColorType", "Color", "Tint", "Overprint", "ImageType", "GrayMap", "Transparency", "Tags"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "Version", "F", "ID", "Comments", "Size", "CropRect", "CropFixed", "Position", "Resolution", "ColorType", "Color", "Tint", "Overprint", "ImageType", "GrayMap", "Transparency", "Tags"
    };
    


}

/// <summary>
/// OPIVersion13Dict_Type Table 406
/// </summary>
internal partial class APM_OPIVersion13Dict_Type : APM_OPIVersion13Dict_Type__Base
{
}


internal partial class APM_OPIVersion13Dict_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OPIVersion13Dict_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_OPIVersion13Dict_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.OPI)) 
        {
            ctx.Fail<APM_OPIVersion13Dict_Type>($"Invalid value {val}, allowed are: [OPI]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// OPIVersion13Dict_Version 
/// </summary>
internal partial class APM_OPIVersion13Dict_Version : APM_OPIVersion13Dict_Version__Base
{
}


internal partial class APM_OPIVersion13Dict_Version__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OPIVersion13Dict_Version";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfNumber, APM_OPIVersion13Dict_Version>(obj, "Version", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 1.3m)) 
        {
            ctx.Fail<APM_OPIVersion13Dict_Version>($"Invalid value {val}, allowed are: [1.3]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// OPIVersion13Dict_F 
/// </summary>
internal partial class APM_OPIVersion13Dict_F : APM_OPIVersion13Dict_F__Base
{
}


internal partial class APM_OPIVersion13Dict_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OPIVersion13Dict_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_OPIVersion13Dict_F>(obj, "F", IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_OPIVersion13Dict_F>("F is required"); return; }
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
                ctx.Fail<APM_OPIVersion13Dict_F>("F is required to one of 'dictionary;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// OPIVersion13Dict_ID 
/// </summary>
internal partial class APM_OPIVersion13Dict_ID : APM_OPIVersion13Dict_ID__Base
{
}


internal partial class APM_OPIVersion13Dict_ID__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OPIVersion13Dict_ID";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_OPIVersion13Dict_ID>(obj, "ID", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// OPIVersion13Dict_Comments 
/// </summary>
internal partial class APM_OPIVersion13Dict_Comments : APM_OPIVersion13Dict_Comments__Base
{
}


internal partial class APM_OPIVersion13Dict_Comments__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OPIVersion13Dict_Comments";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_OPIVersion13Dict_Comments>(obj, "Comments", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// OPIVersion13Dict_Size 
/// </summary>
internal partial class APM_OPIVersion13Dict_Size : APM_OPIVersion13Dict_Size__Base
{
}


internal partial class APM_OPIVersion13Dict_Size__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OPIVersion13Dict_Size";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_OPIVersion13Dict_Size>(obj, "Size", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_2Integers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// OPIVersion13Dict_CropRect 
/// </summary>
internal partial class APM_OPIVersion13Dict_CropRect : APM_OPIVersion13Dict_CropRect__Base
{
}


internal partial class APM_OPIVersion13Dict_CropRect__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OPIVersion13Dict_CropRect";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_OPIVersion13Dict_CropRect>(obj, "CropRect", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// OPIVersion13Dict_CropFixed 
/// </summary>
internal partial class APM_OPIVersion13Dict_CropFixed : APM_OPIVersion13Dict_CropFixed__Base
{
}


internal partial class APM_OPIVersion13Dict_CropFixed__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OPIVersion13Dict_CropFixed";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_OPIVersion13Dict_CropFixed>(obj, "CropFixed", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_4Numbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// OPIVersion13Dict_Position 
/// </summary>
internal partial class APM_OPIVersion13Dict_Position : APM_OPIVersion13Dict_Position__Base
{
}


internal partial class APM_OPIVersion13Dict_Position__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OPIVersion13Dict_Position";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_OPIVersion13Dict_Position>(obj, "Position", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_8Numbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// OPIVersion13Dict_Resolution 
/// </summary>
internal partial class APM_OPIVersion13Dict_Resolution : APM_OPIVersion13Dict_Resolution__Base
{
}


internal partial class APM_OPIVersion13Dict_Resolution__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OPIVersion13Dict_Resolution";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_OPIVersion13Dict_Resolution>(obj, "Resolution", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_2Numbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// OPIVersion13Dict_ColorType 
/// </summary>
internal partial class APM_OPIVersion13Dict_ColorType : APM_OPIVersion13Dict_ColorType__Base
{
}


internal partial class APM_OPIVersion13Dict_ColorType__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OPIVersion13Dict_ColorType";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_OPIVersion13Dict_ColorType>(obj, "ColorType", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// OPIVersion13Dict_Color 
/// </summary>
internal partial class APM_OPIVersion13Dict_Color : APM_OPIVersion13Dict_Color__Base
{
}


internal partial class APM_OPIVersion13Dict_Color__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OPIVersion13Dict_Color";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_OPIVersion13Dict_Color>(obj, "Color", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfOPI13Color, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// OPIVersion13Dict_Tint 
/// </summary>
internal partial class APM_OPIVersion13Dict_Tint : APM_OPIVersion13Dict_Tint__Base
{
}


internal partial class APM_OPIVersion13Dict_Tint__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OPIVersion13Dict_Tint";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfNumber, APM_OPIVersion13Dict_Tint>(obj, "Tint", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// OPIVersion13Dict_Overprint 
/// </summary>
internal partial class APM_OPIVersion13Dict_Overprint : APM_OPIVersion13Dict_Overprint__Base
{
}


internal partial class APM_OPIVersion13Dict_Overprint__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OPIVersion13Dict_Overprint";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfBoolean, APM_OPIVersion13Dict_Overprint>(obj, "Overprint", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// OPIVersion13Dict_ImageType 
/// </summary>
internal partial class APM_OPIVersion13Dict_ImageType : APM_OPIVersion13Dict_ImageType__Base
{
}


internal partial class APM_OPIVersion13Dict_ImageType__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OPIVersion13Dict_ImageType";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_OPIVersion13Dict_ImageType>(obj, "ImageType", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_2Integers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// OPIVersion13Dict_GrayMap 
/// </summary>
internal partial class APM_OPIVersion13Dict_GrayMap : APM_OPIVersion13Dict_GrayMap__Base
{
}


internal partial class APM_OPIVersion13Dict_GrayMap__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OPIVersion13Dict_GrayMap";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_OPIVersion13Dict_GrayMap>(obj, "GrayMap", IndirectRequirement.Either);
        if (val == null) { return; }
        var GrayMap = obj.Get("GrayMap");
        if (!(eq(mod(((GrayMap as PdfArray)?.Count),2),0))) 
        {
            ctx.Fail<APM_OPIVersion13Dict_GrayMap>($"Value failed special case check: fn:Eval((fn:ArrayLength(GrayMap) mod 2)==0)");
        }
        // no value restrictions
        ctx.Run<APM_ArrayOfIntegersGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// OPIVersion13Dict_Transparency 
/// </summary>
internal partial class APM_OPIVersion13Dict_Transparency : APM_OPIVersion13Dict_Transparency__Base
{
}


internal partial class APM_OPIVersion13Dict_Transparency__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OPIVersion13Dict_Transparency";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfBoolean, APM_OPIVersion13Dict_Transparency>(obj, "Transparency", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// OPIVersion13Dict_Tags 
/// </summary>
internal partial class APM_OPIVersion13Dict_Tags : APM_OPIVersion13Dict_Tags__Base
{
}


internal partial class APM_OPIVersion13Dict_Tags__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OPIVersion13Dict_Tags";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_OPIVersion13Dict_Tags>(obj, "Tags", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfTags, PdfArray>(stack, val, obj);
        
    }


}

