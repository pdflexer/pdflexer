// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_Field : APM_Field__Base
{
}

internal partial class APM_Field__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "Field";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_Field_Parent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Field_Kids, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Field_T, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Field_TU, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Field_TM, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Field_Ff, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Field_AA, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_Field>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_Field>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_Field>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_Field>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_Field>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_Field>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_Field>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_Field>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_Field>($"Unknown field {extra} for version 2.0");
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
        "Parent", "Kids", "T", "Ff"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Parent", "Kids", "T", "TU", "TM", "Ff", "AA"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Parent", "Kids", "T", "TU", "TM", "Ff", "AA"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Parent", "Kids", "T", "TU", "TM", "Ff", "AA"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Parent", "Kids", "T", "TU", "TM", "Ff", "AA"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Parent", "Kids", "T", "TU", "TM", "Ff", "AA"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Parent", "Kids", "T", "TU", "TM", "Ff", "AA"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Parent", "Kids", "T", "TU", "TM", "Ff", "AA"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Parent", "Kids", "T", "TU", "TM", "Ff", "AA"
    };
    


}

/// <summary>
/// Field_Parent Table 226
/// </summary>
internal partial class APM_Field_Parent : APM_Field_Parent__Base
{
}


internal partial class APM_Field_Parent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Field_Parent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_Field_Parent>(obj, "Parent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_FieldTx.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_FieldTx, PdfDictionary>(stack, val, obj);
        } else if (APM_FieldBtnPush.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_FieldBtnPush, PdfDictionary>(stack, val, obj);
        } else if (APM_FieldBtnCheckbox.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_FieldBtnCheckbox, PdfDictionary>(stack, val, obj);
        } else if (APM_FieldBtnRadio.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_FieldBtnRadio, PdfDictionary>(stack, val, obj);
        } else if (APM_FieldChoice.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_FieldChoice, PdfDictionary>(stack, val, obj);
        } else if (APM_Field.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_Field, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 1.3m && APM_FieldSig.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_FieldSig, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_Field_Parent>("Parent did not match any allowable types: '[FieldTx,FieldBtnPush,FieldBtnCheckbox,FieldBtnRadio,FieldChoice,fn:SinceVersion(1.3,FieldSig),Field]'");
        }
        
    }


}

/// <summary>
/// Field_Kids 
/// </summary>
internal partial class APM_Field_Kids : APM_Field_Kids__Base
{
}


internal partial class APM_Field_Kids__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Field_Kids";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_Field_Kids>(obj, "Kids", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfFields, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// Field_T 
/// </summary>
internal partial class APM_Field_T : APM_Field_T__Base
{
}


internal partial class APM_Field_T__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Field_T";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_Field_T>(obj, "T", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Field_TU 
/// </summary>
internal partial class APM_Field_TU : APM_Field_TU__Base
{
}


internal partial class APM_Field_TU__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Field_TU";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_Field_TU>(obj, "TU", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Field_TM 
/// </summary>
internal partial class APM_Field_TM : APM_Field_TM__Base
{
}


internal partial class APM_Field_TM__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Field_TM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_Field_TM>(obj, "TM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Field_Ff 
/// </summary>
internal partial class APM_Field_Ff : APM_Field_Ff__Base
{
}


internal partial class APM_Field_Ff__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Field_Ff";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfIntNumber, APM_Field_Ff>(obj, "Ff", IndirectRequirement.Either);
        if (val == null) { return; }
        
        if (!(BitsClear(val,0b11111111111111111111111111111000))) 
        {
            ctx.Fail<APM_Field_Ff>($"Value failed special case check: fn:Eval(fn:BitsClear(4,32))");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Field_AA 
/// </summary>
internal partial class APM_Field_AA : APM_Field_AA__Base
{
}


internal partial class APM_Field_AA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Field_AA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_Field_AA>(obj, "AA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_AddActionFormField, PdfDictionary>(stack, val, obj);
        
    }


}

