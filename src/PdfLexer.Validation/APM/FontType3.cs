// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_FontType3 : APM_FontType3_Base
{
}

internal partial class APM_FontType3_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "FontType3";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_FontType3_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontType3_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontType3_Name, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontType3_FontBBox, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontType3_FontMatrix, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontType3_CharProcs, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontType3_Encoding, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontType3_FirstChar, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontType3_LastChar, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontType3_Widths, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontType3_FontDescriptor, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontType3_Resources, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FontType3_ToUnicode, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_10.Contains(x)))
                {
                    ctx.Fail<APM_FontType3>($"Unknown field {extra} for version 1.0");
                }
                break;
            case 1.1m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_11.Contains(x)))
                {
                    ctx.Fail<APM_FontType3>($"Unknown field {extra} for version 1.1");
                }
                break;
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_FontType3>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_FontType3>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_FontType3>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_FontType3>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_FontType3>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_FontType3>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_FontType3>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_FontType3>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_FontType3>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_FontType3_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_10 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "FontBBox", "FontMatrix", "CharProcs", "Encoding", "FirstChar", "LastChar", "Widths", "FontDescriptor"
    };
    public static HashSet<string> AllowedFields_11 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "FontBBox", "FontMatrix", "CharProcs", "Encoding", "FirstChar", "LastChar", "Widths", "FontDescriptor"
    };
    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "FontBBox", "FontMatrix", "CharProcs", "Encoding", "FirstChar", "LastChar", "Widths", "FontDescriptor", "Resources", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "FontBBox", "FontMatrix", "CharProcs", "Encoding", "FirstChar", "LastChar", "Widths", "FontDescriptor", "Resources", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "FontBBox", "FontMatrix", "CharProcs", "Encoding", "FirstChar", "LastChar", "Widths", "FontDescriptor", "Resources", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "FontBBox", "FontMatrix", "CharProcs", "Encoding", "FirstChar", "LastChar", "Widths", "FontDescriptor", "Resources", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "FontBBox", "FontMatrix", "CharProcs", "Encoding", "FirstChar", "LastChar", "Widths", "FontDescriptor", "Resources", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "FontBBox", "FontMatrix", "CharProcs", "Encoding", "FirstChar", "LastChar", "Widths", "FontDescriptor", "Resources", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "FontBBox", "FontMatrix", "CharProcs", "Encoding", "FirstChar", "LastChar", "Widths", "FontDescriptor", "Resources", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "FontBBox", "FontMatrix", "CharProcs", "Encoding", "FirstChar", "LastChar", "Widths", "FontDescriptor", "Resources", "ToUnicode"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Name", "FontBBox", "FontMatrix", "CharProcs", "Encoding", "FirstChar", "LastChar", "Widths", "FontDescriptor", "Resources", "ToUnicode"
    };
    


}

/// <summary>
/// FontType3_Type 
/// </summary>
internal partial class APM_FontType3_Type : APM_FontType3_Type_Base
{
}


internal partial class APM_FontType3_Type_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontType3_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FontType3_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "Font")) 
        {
            ctx.Fail<APM_FontType3_Type>($"Invalid value {val}, allowed are: [Font]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontType3_Subtype Table 110
/// </summary>
internal partial class APM_FontType3_Subtype : APM_FontType3_Subtype_Base
{
}


internal partial class APM_FontType3_Subtype_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontType3_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_FontType3_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "Type3")) 
        {
            ctx.Fail<APM_FontType3_Subtype>($"Invalid value {val}, allowed are: [Type3]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// FontType3_Name 
/// </summary>
internal partial class APM_FontType3_Name : APM_FontType3_Name_Base
{
}


internal partial class APM_FontType3_Name_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontType3_Name";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        PdfName? val;
        {
            
            if ((ctx.Version == 1.0m)) {
                val = ctx.GetRequired<PdfName, APM_FontType3_Name>(obj, "Name", IndirectRequirement.Either);
            } else {
                val = ctx.GetOptional<PdfName, APM_FontType3_Name>(obj, "Name", IndirectRequirement.Either);
            }
            if (val == null) { return; }
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontType3_FontBBox 
/// </summary>
internal partial class APM_FontType3_FontBBox : APM_FontType3_FontBBox_Base
{
}


internal partial class APM_FontType3_FontBBox_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontType3_FontBBox";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_FontType3_FontBBox>(obj, "FontBBox", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontType3_FontMatrix 
/// </summary>
internal partial class APM_FontType3_FontMatrix : APM_FontType3_FontMatrix_Base
{
}


internal partial class APM_FontType3_FontMatrix_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontType3_FontMatrix";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_FontType3_FontMatrix>(obj, "FontMatrix", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontType3_CharProcs 
/// </summary>
internal partial class APM_FontType3_CharProcs : APM_FontType3_CharProcs_Base
{
}


internal partial class APM_FontType3_CharProcs_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontType3_CharProcs";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_FontType3_CharProcs>(obj, "CharProcs", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_CharProcMap, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// FontType3_Encoding 
/// </summary>
internal partial class APM_FontType3_Encoding : APM_FontType3_Encoding_Base
{
}


internal partial class APM_FontType3_Encoding_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontType3_Encoding";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_FontType3_Encoding>(obj, "Encoding", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Encoding, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// FontType3_FirstChar 
/// </summary>
internal partial class APM_FontType3_FirstChar : APM_FontType3_FirstChar_Base
{
}


internal partial class APM_FontType3_FirstChar_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontType3_FirstChar";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_FontType3_FirstChar>(obj, "FirstChar", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontType3_LastChar 
/// </summary>
internal partial class APM_FontType3_LastChar : APM_FontType3_LastChar_Base
{
}


internal partial class APM_FontType3_LastChar_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontType3_LastChar";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_FontType3_LastChar>(obj, "LastChar", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FontType3_Widths 
/// </summary>
internal partial class APM_FontType3_Widths : APM_FontType3_Widths_Base
{
}


internal partial class APM_FontType3_Widths_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontType3_Widths";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_FontType3_Widths>(obj, "Widths", IndirectRequirement.Either);
        if (val == null) { return; }
        // TODO special case
        // no value restrictions
        ctx.Run<APM_ArrayOfNumbersGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// FontType3_FontDescriptor https://github.com/pdf-association/pdf-issues/issues/106
/// </summary>
internal partial class APM_FontType3_FontDescriptor : APM_FontType3_FontDescriptor_Base
{
}


internal partial class APM_FontType3_FontDescriptor_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontType3_FontDescriptor";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        PdfDictionary? val;
        {
            
            if (IsPDFTagged(obj)) {
                val = ctx.GetRequired<PdfDictionary, APM_FontType3_FontDescriptor>(obj, "FontDescriptor", IndirectRequirement.Either);
            } else {
                val = ctx.GetOptional<PdfDictionary, APM_FontType3_FontDescriptor>(obj, "FontDescriptor", IndirectRequirement.Either);
            }
            if (val == null) { return; }
        }
        // no special cases
        // no value restrictions
        ctx.Run<APM_FontDescriptorType3, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// FontType3_Resources 
/// </summary>
internal partial class APM_FontType3_Resources : APM_FontType3_Resources_Base
{
}


internal partial class APM_FontType3_Resources_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontType3_Resources";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_FontType3_Resources>(obj, "Resources", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Resource, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// FontType3_ToUnicode 
/// </summary>
internal partial class APM_FontType3_ToUnicode : APM_FontType3_ToUnicode_Base
{
}


internal partial class APM_FontType3_ToUnicode_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FontType3_ToUnicode";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_FontType3_ToUnicode>(obj, "ToUnicode", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

