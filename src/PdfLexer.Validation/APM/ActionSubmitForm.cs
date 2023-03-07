// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_ActionSubmitForm : APM_ActionSubmitForm__Base
{
}

internal partial class APM_ActionSubmitForm__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "ActionSubmitForm";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_ActionSubmitForm_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionSubmitForm_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionSubmitForm_Fields, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionSubmitForm_Flags, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionSubmitForm_CharSet, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_ActionSubmitForm>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_ActionSubmitForm>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_ActionSubmitForm>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_ActionSubmitForm>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_ActionSubmitForm>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_ActionSubmitForm>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_ActionSubmitForm>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_ActionSubmitForm>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_ActionSubmitForm>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        return false;
    }

    public static List<string> AllowedFields_12 { get; } = new List<string> 
    {
        "S", "F", "Fields", "Flags", "CharSet"
    };
    public static List<string> AllowedFields_13 { get; } = new List<string> 
    {
        "S", "F", "Fields", "Flags", "CharSet"
    };
    public static List<string> AllowedFields_14 { get; } = new List<string> 
    {
        "S", "F", "Fields", "Flags", "CharSet"
    };
    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "S", "F", "Fields", "Flags", "CharSet"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "S", "F", "Fields", "Flags", "CharSet"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "S", "F", "Fields", "Flags", "CharSet"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "S", "F", "Fields", "Flags", "CharSet"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "S", "F", "Fields", "Flags", "CharSet"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "S", "F", "Fields", "Flags", "CharSet"
    };
    


}

/// <summary>
/// ActionSubmitForm_S Table 196 and Table 239
/// </summary>
internal partial class APM_ActionSubmitForm_S : APM_ActionSubmitForm_S__Base
{
}


internal partial class APM_ActionSubmitForm_S__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSubmitForm_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_ActionSubmitForm_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.SubmitForm)) 
        {
            ctx.Fail<APM_ActionSubmitForm_S>($"Invalid value {val}, allowed are: [SubmitForm]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ActionSubmitForm_F 
/// </summary>
internal partial class APM_ActionSubmitForm_F : APM_ActionSubmitForm_F__Base
{
}


internal partial class APM_ActionSubmitForm_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSubmitForm_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ActionSubmitForm_F>(obj, "F", IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ActionSubmitForm_F>("F is required"); return; }
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
                ctx.Fail<APM_ActionSubmitForm_F>("F is required to one of 'dictionary;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// ActionSubmitForm_Fields 
/// </summary>
internal partial class APM_ActionSubmitForm_Fields : APM_ActionSubmitForm_Fields__Base
{
}


internal partial class APM_ActionSubmitForm_Fields__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSubmitForm_Fields";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_ActionSubmitForm_Fields>(obj, "Fields", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfFieldID, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// ActionSubmitForm_Flags Table 240, https://github.com/pdf-association/pdf-issues/issues/122
/// </summary>
internal partial class APM_ActionSubmitForm_Flags : APM_ActionSubmitForm_Flags__Base
{
}


internal partial class APM_ActionSubmitForm_Flags__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSubmitForm_Flags";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_ActionSubmitForm_Flags>(obj, "Flags", IndirectRequirement.Either);
        if (val == null) { return; }
        
        if (!((BitsClear(val,0b00000000000000000001000000000000)&&BitsClear(val,0b11111111111111111100000000000000)))) 
        {
            ctx.Fail<APM_ActionSubmitForm_Flags>($"Value failed special case check: fn:Eval(fn:BitClear(13) && fn:BitsClear(15,32))");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// ActionSubmitForm_CharSet see https://github.com/pdf-association/pdf-issues/issues/122
/// </summary>
internal partial class APM_ActionSubmitForm_CharSet : APM_ActionSubmitForm_CharSet__Base
{
}


internal partial class APM_ActionSubmitForm_CharSet__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionSubmitForm_CharSet";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_ActionSubmitForm_CharSet>(obj, "CharSet", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // TODO value checks string
        // no linked objects
        
    }


}
