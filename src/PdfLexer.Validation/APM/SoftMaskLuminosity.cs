// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_SoftMaskLuminosity : APM_SoftMaskLuminosity__Base
{
}

internal partial class APM_SoftMaskLuminosity__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "SoftMaskLuminosity";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_SoftMaskLuminosity_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoftMaskLuminosity_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoftMaskLuminosity_G, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoftMaskLuminosity_BC, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoftMaskLuminosity_TR, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_SoftMaskLuminosity>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_SoftMaskLuminosity>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_SoftMaskLuminosity>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_SoftMaskLuminosity>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_SoftMaskLuminosity>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_SoftMaskLuminosity>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_SoftMaskLuminosity>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_SoftMaskLuminosity_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_14 { get; } = new List<string> 
    {
        "Type", "S", "G", "BC", "TR"
    };
    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Type", "S", "G", "BC", "TR"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Type", "S", "G", "BC", "TR"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "S", "G", "BC", "TR"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "S", "G", "BC", "TR"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "S", "G", "BC", "TR"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "S", "G", "BC", "TR"
    };
    


}

/// <summary>
/// SoftMaskLuminosity_Type Table 142
/// </summary>
internal partial class APM_SoftMaskLuminosity_Type : APM_SoftMaskLuminosity_Type__Base
{
}


internal partial class APM_SoftMaskLuminosity_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoftMaskLuminosity_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_SoftMaskLuminosity_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Mask)) 
        {
            ctx.Fail<APM_SoftMaskLuminosity_Type>($"Invalid value {val}, allowed are: [Mask]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// SoftMaskLuminosity_S 
/// </summary>
internal partial class APM_SoftMaskLuminosity_S : APM_SoftMaskLuminosity_S__Base
{
}


internal partial class APM_SoftMaskLuminosity_S__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoftMaskLuminosity_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_SoftMaskLuminosity_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Luminosity)) 
        {
            ctx.Fail<APM_SoftMaskLuminosity_S>($"Invalid value {val}, allowed are: [Luminosity]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// SoftMaskLuminosity_G 
/// </summary>
internal partial class APM_SoftMaskLuminosity_G : APM_SoftMaskLuminosity_G__Base
{
}


internal partial class APM_SoftMaskLuminosity_G__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoftMaskLuminosity_G";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfStream, APM_SoftMaskLuminosity_G>(obj, "G", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        var GGroupS = val.Get("Group")?.Get("S");
        var GGroupCS = val.Get("Group")?.Get("CS");
        if (!((eq(GGroupS,PdfName.Transparency)&&(GGroupCS != null)))) 
        {
            ctx.Fail<APM_SoftMaskLuminosity_G>($"Value failed special case check: fn:Eval((G::Group::@S==Transparency) && fn:IsPresent(G::Group::CS))");
        }
        // no value restrictions
        ctx.Run<APM_XObjectFormType1, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// SoftMaskLuminosity_BC 
/// </summary>
internal partial class APM_SoftMaskLuminosity_BC : APM_SoftMaskLuminosity_BC__Base
{
}


internal partial class APM_SoftMaskLuminosity_BC__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoftMaskLuminosity_BC";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_SoftMaskLuminosity_BC>(obj, "BC", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfNumbersGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// SoftMaskLuminosity_TR 
/// </summary>
internal partial class APM_SoftMaskLuminosity_TR : APM_SoftMaskLuminosity_TR__Base
{
}


internal partial class APM_SoftMaskLuminosity_TR__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoftMaskLuminosity_TR";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_SoftMaskLuminosity_TR>(obj, "TR", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    if (APM_FunctionType2.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_FunctionType2, PdfDictionary>(stack, val, obj);
                    } else if (APM_FunctionType3.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_FunctionType3, PdfDictionary>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_SoftMaskLuminosity_TR>("TR did not match any allowable types: '[FunctionType2,FunctionType3]'");
                    }
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    
                    
                    if (!(val == PdfName.Identity)) 
                    {
                        ctx.Fail<APM_SoftMaskLuminosity_TR>($"Invalid value {val}, allowed are: [Identity]");
                    }
                    // no linked objects
                    return;
                }
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_SoftMaskLuminosity_TR>("TR is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    if (APM_FunctionType0.MatchesType(ctx, val.Dictionary)) 
                    {
                        ctx.Run<APM_FunctionType0, PdfDictionary>(stack, val.Dictionary, obj);
                    } else if (APM_FunctionType4.MatchesType(ctx, val.Dictionary)) 
                    {
                        ctx.Run<APM_FunctionType4, PdfDictionary>(stack, val.Dictionary, obj);
                    }else 
                    {
                        ctx.Fail<APM_SoftMaskLuminosity_TR>("TR did not match any allowable types: '[FunctionType0,FunctionType4]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_SoftMaskLuminosity_TR>("TR is required to one of 'dictionary;name;stream', was " + utval.Type);
                return;
        }
    }


}
