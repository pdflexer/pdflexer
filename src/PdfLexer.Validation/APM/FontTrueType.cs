// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_FontTrueType : APM_FontTrueType__Base
{
}

internal partial class APM_FontTrueType__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "FontTrueType";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_FontTrueType_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontTrueType_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontTrueType_Name, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontTrueType_BaseFont, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontTrueType_FirstChar, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontTrueType_LastChar, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontTrueType_Widths, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontTrueType_FontDescriptor, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontTrueType_Encoding, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontTrueType_ToUnicode, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_10.Contains(x)))
                {
                    ctx.Fail<APM_FontTrueType>($"Unknown field {extra} for version 1.0");
                }
                break;
            case 1.1m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_11.Contains(x)))
                {
                    ctx.Fail<APM_FontTrueType>($"Unknown field {extra} for version 1.1");
                }
                break;
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_FontTrueType>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_FontTrueType>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_FontTrueType>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_FontTrueType>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_FontTrueType>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_FontTrueType>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_FontTrueType>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_FontTrueType>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_FontTrueType>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_FontTrueType_Type, PdfDictionary>(new CallStack(), obj, null);
        c.Run<APM_FontTrueType_Subtype, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_10 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "BaseFont", "FirstChar", "LastChar", "Widths", "FontDescriptor", "Encoding"
    };
    public static HashSet<string> AllowedFields_11 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "BaseFont", "FirstChar", "LastChar", "Widths", "FontDescriptor", "Encoding"
    };
    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "BaseFont", "FirstChar", "LastChar", "Widths", "FontDescriptor", "Encoding", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "BaseFont", "FirstChar", "LastChar", "Widths", "FontDescriptor", "Encoding", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "BaseFont", "FirstChar", "LastChar", "Widths", "FontDescriptor", "Encoding", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "BaseFont", "FirstChar", "LastChar", "Widths", "FontDescriptor", "Encoding", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "BaseFont", "FirstChar", "LastChar", "Widths", "FontDescriptor", "Encoding", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "BaseFont", "FirstChar", "LastChar", "Widths", "FontDescriptor", "Encoding", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "BaseFont", "FirstChar", "LastChar", "Widths", "FontDescriptor", "Encoding", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "BaseFont", "FirstChar", "LastChar", "Widths", "FontDescriptor", "Encoding", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "BaseFont", "FirstChar", "LastChar", "Widths", "FontDescriptor", "Encoding", "ToUnicode"
    };
    


}

/// <summary>
/// FontTrueType_Type Table 109 and Clause 9.6.3
/// </summary>
internal partial class APM_FontTrueType_Type : APM_FontTrueType_Type__Base
{
}


internal partial class APM_FontTrueType_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontTrueType_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FontTrueType_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Font)) 
        {
            ctx.Fail<APM_FontTrueType_Type>($"Invalid value {val}, allowed are: [Font]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontTrueType_Subtype 
/// </summary>
internal partial class APM_FontTrueType_Subtype : APM_FontTrueType_Subtype__Base
{
}


internal partial class APM_FontTrueType_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontTrueType_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FontTrueType_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.TrueType)) 
        {
            ctx.Fail<APM_FontTrueType_Subtype>($"Invalid value {val}, allowed are: [TrueType]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontTrueType_Name 
/// </summary>
internal partial class APM_FontTrueType_Name : APM_FontTrueType_Name__Base
{
}


internal partial class APM_FontTrueType_Name__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontTrueType_Name";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfName, APM_FontTrueType_Name>(obj, "Name", IndirectRequirement.Either);
        if (((ctx.Version == 1.0m)) && val == null) {
            ctx.Fail<APM_FontTrueType_Name>("Name is required when 'fn:IsRequired(fn:IsPDFVersion(1.0))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontTrueType_BaseFont 
/// </summary>
internal partial class APM_FontTrueType_BaseFont : APM_FontTrueType_BaseFont__Base
{
}


internal partial class APM_FontTrueType_BaseFont__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontTrueType_BaseFont";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FontTrueType_BaseFont>(obj, "BaseFont", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontTrueType_FirstChar 
/// </summary>
internal partial class APM_FontTrueType_FirstChar : APM_FontTrueType_FirstChar__Base
{
}


internal partial class APM_FontTrueType_FirstChar__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontTrueType_FirstChar";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfIntNumber, APM_FontTrueType_FirstChar>(obj, "FirstChar", IndirectRequirement.Either);
        if (((ctx.Version >= 2.0m||NotStandard14Font(obj))) && val == null) {
            ctx.Fail<APM_FontTrueType_FirstChar>("FirstChar is required when 'fn:IsRequired(fn:SinceVersion(2.0) || fn:NotStandard14Font())"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontTrueType_LastChar 
/// </summary>
internal partial class APM_FontTrueType_LastChar : APM_FontTrueType_LastChar__Base
{
}


internal partial class APM_FontTrueType_LastChar__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontTrueType_LastChar";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfIntNumber, APM_FontTrueType_LastChar>(obj, "LastChar", IndirectRequirement.Either);
        if (((ctx.Version >= 2.0m||NotStandard14Font(obj))) && val == null) {
            ctx.Fail<APM_FontTrueType_LastChar>("LastChar is required when 'fn:IsRequired(fn:SinceVersion(2.0) || fn:NotStandard14Font())"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontTrueType_Widths 
/// </summary>
internal partial class APM_FontTrueType_Widths : APM_FontTrueType_Widths__Base
{
}


internal partial class APM_FontTrueType_Widths__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontTrueType_Widths";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfArray, APM_FontTrueType_Widths>(obj, "Widths", IndirectRequirement.Either);
        if (((ctx.Version >= 2.0m||NotStandard14Font(obj))) && val == null) {
            ctx.Fail<APM_FontTrueType_Widths>("Widths is required when 'fn:IsRequired(fn:SinceVersion(2.0) || fn:NotStandard14Font())"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfNumbersGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// FontTrueType_FontDescriptor 
/// </summary>
internal partial class APM_FontTrueType_FontDescriptor : APM_FontTrueType_FontDescriptor__Base
{
}


internal partial class APM_FontTrueType_FontDescriptor__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontTrueType_FontDescriptor";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfDictionary, APM_FontTrueType_FontDescriptor>(obj, "FontDescriptor", IndirectRequirement.MustBeIndirect);
        if (((ctx.Version >= 2.0m||NotStandard14Font(obj))) && val == null) {
            ctx.Fail<APM_FontTrueType_FontDescriptor>("FontDescriptor is required when 'fn:IsRequired(fn:SinceVersion(2.0) || fn:NotStandard14Font())"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        ctx.Run<APM_FontDescriptorTrueType, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// FontTrueType_Encoding 
/// </summary>
internal partial class APM_FontTrueType_Encoding : APM_FontTrueType_Encoding__Base
{
}


internal partial class APM_FontTrueType_Encoding__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontTrueType_Encoding";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_FontTrueType_Encoding>(obj, "Encoding", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_Encoding, PdfDictionary>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    
                    
                    if (!(val == PdfName.MacRomanEncoding || val == PdfName.MacExpertEncoding || val == PdfName.WinAnsiEncoding)) 
                    {
                        ctx.Fail<APM_FontTrueType_Encoding>($"Invalid value {val}, allowed are: [MacRomanEncoding,MacExpertEncoding,WinAnsiEncoding]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_FontTrueType_Encoding>("Encoding is required to one of 'dictionary;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// FontTrueType_ToUnicode 
/// </summary>
internal partial class APM_FontTrueType_ToUnicode : APM_FontTrueType_ToUnicode__Base
{
}


internal partial class APM_FontTrueType_ToUnicode__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontTrueType_ToUnicode";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_FontTrueType_ToUnicode>(obj, "ToUnicode", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}
