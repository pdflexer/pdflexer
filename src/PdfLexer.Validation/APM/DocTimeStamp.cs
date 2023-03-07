// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_DocTimeStamp : APM_DocTimeStamp__Base
{
}

internal partial class APM_DocTimeStamp__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "DocTimeStamp";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_DocTimeStamp_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocTimeStamp_Filter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocTimeStamp_SubFilter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocTimeStamp_Contents, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocTimeStamp_Cert, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocTimeStamp_ByteRange, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocTimeStamp_Reference, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocTimeStamp_Changes, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocTimeStamp_Name, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocTimeStamp_M, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocTimeStamp_Location, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocTimeStamp_Reason, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocTimeStamp_ContactInfo, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocTimeStamp_R, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocTimeStamp_V, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocTimeStamp_Prop_Build, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocTimeStamp_Prop_AuthTime, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocTimeStamp_Prop_AuthType, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocTimeStamp_ADBE_Build, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_DocTimeStamp>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_DocTimeStamp>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_DocTimeStamp>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_DocTimeStamp>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_DocTimeStamp>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_DocTimeStamp>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_DocTimeStamp>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_DocTimeStamp>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_DocTimeStamp_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "Filter", "SubFilter", "Contents", "Cert", "ByteRange", "Changes", "Name", "M", "Location", "Reason", "ContactInfo", "R", "ADBE_Build"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "Filter", "SubFilter", "Contents", "Cert", "ByteRange", "Changes", "Name", "M", "Location", "Reason", "ContactInfo", "R", "ADBE_Build"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "Filter", "SubFilter", "Contents", "Cert", "ByteRange", "Reference", "Changes", "Name", "M", "Location", "Reason", "ContactInfo", "R", "V", "Prop_Build", "Prop_AuthTime", "Prop_AuthType"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "Filter", "SubFilter", "Contents", "Cert", "ByteRange", "Reference", "Changes", "Name", "M", "Location", "Reason", "ContactInfo", "R", "V", "Prop_Build", "Prop_AuthTime", "Prop_AuthType"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "Filter", "SubFilter", "Contents", "Cert", "ByteRange", "Reference", "Changes", "Name", "M", "Location", "Reason", "ContactInfo", "R", "V", "Prop_Build", "Prop_AuthTime", "Prop_AuthType"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "Filter", "SubFilter", "Contents", "Cert", "ByteRange", "Reference", "Changes", "Name", "M", "Location", "Reason", "ContactInfo", "R", "V", "Prop_Build", "Prop_AuthTime", "Prop_AuthType"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "Filter", "SubFilter", "Contents", "Cert", "ByteRange", "Reference", "Changes", "Name", "M", "Location", "Reason", "ContactInfo", "R", "V", "Prop_Build", "Prop_AuthTime", "Prop_AuthType"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "Filter", "SubFilter", "Contents", "Cert", "ByteRange", "Reference", "Changes", "Name", "M", "Location", "Reason", "ContactInfo", "R", "V", "Prop_Build", "Prop_AuthTime", "Prop_AuthType"
    };
    


}

/// <summary>
/// DocTimeStamp_Type Table 255
/// </summary>
internal partial class APM_DocTimeStamp_Type : APM_DocTimeStamp_Type__Base
{
}


internal partial class APM_DocTimeStamp_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocTimeStamp_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_DocTimeStamp_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.DocTimeStamp)) 
        {
            ctx.Fail<APM_DocTimeStamp_Type>($"Invalid value {val}, allowed are: [DocTimeStamp]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// DocTimeStamp_Filter Inheritable from Parent
/// </summary>
internal partial class APM_DocTimeStamp_Filter : APM_DocTimeStamp_Filter__Base
{
}


internal partial class APM_DocTimeStamp_Filter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocTimeStamp_Filter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_DocTimeStamp_Filter>(obj, "Filter", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DocTimeStamp_SubFilter 
/// </summary>
internal partial class APM_DocTimeStamp_SubFilter : APM_DocTimeStamp_SubFilter__Base
{
}


internal partial class APM_DocTimeStamp_SubFilter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocTimeStamp_SubFilter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_DocTimeStamp_SubFilter>(obj, "SubFilter", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.ETSIRFC3161)) 
        {
            ctx.Fail<APM_DocTimeStamp_SubFilter>($"Invalid value {val}, allowed are: [ETSI.RFC3161]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// DocTimeStamp_Contents 
/// </summary>
internal partial class APM_DocTimeStamp_Contents : APM_DocTimeStamp_Contents__Base
{
}


internal partial class APM_DocTimeStamp_Contents__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocTimeStamp_Contents";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfString, APM_DocTimeStamp_Contents>(obj, "Contents", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DocTimeStamp_Cert 
/// </summary>
internal partial class APM_DocTimeStamp_Cert : APM_DocTimeStamp_Cert__Base
{
}


internal partial class APM_DocTimeStamp_Cert__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocTimeStamp_Cert";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_DocTimeStamp_Cert>(obj, "Cert", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_ArrayOfStringsByte, PdfArray>(stack, val, obj);
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
                ctx.Fail<APM_DocTimeStamp_Cert>("Cert is required to one of 'array;string-byte', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// DocTimeStamp_ByteRange 
/// </summary>
internal partial class APM_DocTimeStamp_ByteRange : APM_DocTimeStamp_ByteRange__Base
{
}


internal partial class APM_DocTimeStamp_ByteRange__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocTimeStamp_ByteRange";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_DocTimeStamp_ByteRange>(obj, "ByteRange", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfIntegersGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// DocTimeStamp_Reference 
/// </summary>
internal partial class APM_DocTimeStamp_Reference : APM_DocTimeStamp_Reference__Base
{
}


internal partial class APM_DocTimeStamp_Reference__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocTimeStamp_Reference";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_DocTimeStamp_Reference>(obj, "Reference", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfSignatureReferences, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// DocTimeStamp_Changes 
/// </summary>
internal partial class APM_DocTimeStamp_Changes : APM_DocTimeStamp_Changes__Base
{
}


internal partial class APM_DocTimeStamp_Changes__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocTimeStamp_Changes";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_DocTimeStamp_Changes>(obj, "Changes", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_3Integers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// DocTimeStamp_Name 
/// </summary>
internal partial class APM_DocTimeStamp_Name : APM_DocTimeStamp_Name__Base
{
}


internal partial class APM_DocTimeStamp_Name__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocTimeStamp_Name";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_DocTimeStamp_Name>(obj, "Name", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DocTimeStamp_M 
/// </summary>
internal partial class APM_DocTimeStamp_M : APM_DocTimeStamp_M__Base
{
}


internal partial class APM_DocTimeStamp_M__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocTimeStamp_M";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_DocTimeStamp_M>(obj, "M", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DocTimeStamp_Location 
/// </summary>
internal partial class APM_DocTimeStamp_Location : APM_DocTimeStamp_Location__Base
{
}


internal partial class APM_DocTimeStamp_Location__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocTimeStamp_Location";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_DocTimeStamp_Location>(obj, "Location", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DocTimeStamp_Reason 
/// </summary>
internal partial class APM_DocTimeStamp_Reason : APM_DocTimeStamp_Reason__Base
{
}


internal partial class APM_DocTimeStamp_Reason__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocTimeStamp_Reason";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_DocTimeStamp_Reason>(obj, "Reason", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DocTimeStamp_ContactInfo 
/// </summary>
internal partial class APM_DocTimeStamp_ContactInfo : APM_DocTimeStamp_ContactInfo__Base
{
}


internal partial class APM_DocTimeStamp_ContactInfo__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocTimeStamp_ContactInfo";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_DocTimeStamp_ContactInfo>(obj, "ContactInfo", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DocTimeStamp_R 
/// </summary>
internal partial class APM_DocTimeStamp_R : APM_DocTimeStamp_R__Base
{
}


internal partial class APM_DocTimeStamp_R__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocTimeStamp_R";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_DocTimeStamp_R>(obj, "R", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DocTimeStamp_V 
/// </summary>
internal partial class APM_DocTimeStamp_V : APM_DocTimeStamp_V__Base
{
}


internal partial class APM_DocTimeStamp_V__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocTimeStamp_V";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_DocTimeStamp_V>(obj, "V", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DocTimeStamp_Prop_Build 
/// </summary>
internal partial class APM_DocTimeStamp_Prop_Build : APM_DocTimeStamp_Prop_Build__Base
{
}


internal partial class APM_DocTimeStamp_Prop_Build__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocTimeStamp_Prop_Build";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_DocTimeStamp_Prop_Build>(obj, "Prop_Build", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_SignatureBuildPropDict, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// DocTimeStamp_Prop_AuthTime 
/// </summary>
internal partial class APM_DocTimeStamp_Prop_AuthTime : APM_DocTimeStamp_Prop_AuthTime__Base
{
}


internal partial class APM_DocTimeStamp_Prop_AuthTime__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocTimeStamp_Prop_AuthTime";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_DocTimeStamp_Prop_AuthTime>(obj, "Prop_AuthTime", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DocTimeStamp_Prop_AuthType 
/// </summary>
internal partial class APM_DocTimeStamp_Prop_AuthType : APM_DocTimeStamp_Prop_AuthType__Base
{
}


internal partial class APM_DocTimeStamp_Prop_AuthType__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocTimeStamp_Prop_AuthType";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_DocTimeStamp_Prop_AuthType>(obj, "Prop_AuthType", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DocTimeStamp_ADBE_Build Adobe "Digital Signature Build Dictionary Specification"
/// </summary>
internal partial class APM_DocTimeStamp_ADBE_Build : APM_DocTimeStamp_ADBE_Build__Base
{
}


internal partial class APM_DocTimeStamp_ADBE_Build__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocTimeStamp_ADBE_Build";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_DocTimeStamp_ADBE_Build>(obj, "ADBE_Build", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}
