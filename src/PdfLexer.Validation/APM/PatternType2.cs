// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_PatternType2 : APM_PatternType2__Base
{
}

internal partial class APM_PatternType2__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "PatternType2";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_PatternType2_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PatternType2_PatternType, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PatternType2_Shading, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PatternType2_Matrix, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_PatternType2_ExtGState, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_PatternType2>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_PatternType2>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_PatternType2>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_PatternType2>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_PatternType2>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_PatternType2>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_PatternType2>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_PatternType2>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_PatternType2_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_13 { get; } = new List<string> 
    {
        "Type", "PatternType", "Shading", "Matrix", "ExtGState"
    };
    public static List<string> AllowedFields_14 { get; } = new List<string> 
    {
        "Type", "PatternType", "Shading", "Matrix", "ExtGState"
    };
    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Type", "PatternType", "Shading", "Matrix", "ExtGState"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Type", "PatternType", "Shading", "Matrix", "ExtGState"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "PatternType", "Shading", "Matrix", "ExtGState"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "PatternType", "Shading", "Matrix", "ExtGState"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "PatternType", "Shading", "Matrix", "ExtGState"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "PatternType", "Shading", "Matrix", "ExtGState"
    };
    


}

/// <summary>
/// PatternType2_Type Table 75
/// </summary>
internal partial class APM_PatternType2_Type : APM_PatternType2_Type__Base
{
}


internal partial class APM_PatternType2_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PatternType2_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_PatternType2_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "Pattern")) 
        {
            ctx.Fail<APM_PatternType2_Type>($"Invalid value {val}, allowed are: [Pattern]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// PatternType2_PatternType 
/// </summary>
internal partial class APM_PatternType2_PatternType : APM_PatternType2_PatternType__Base
{
}


internal partial class APM_PatternType2_PatternType__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PatternType2_PatternType";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_PatternType2_PatternType>(obj, "PatternType", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 2)) 
        {
            ctx.Fail<APM_PatternType2_PatternType>($"Invalid value {val}, allowed are: [2]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// PatternType2_Shading 
/// </summary>
internal partial class APM_PatternType2_Shading : APM_PatternType2_Shading__Base
{
}


internal partial class APM_PatternType2_Shading__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PatternType2_Shading";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_PatternType2_Shading>(obj, "Shading", IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_PatternType2_Shading>("Shading is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    if (APM_ShadingType1.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ShadingType1, PdfDictionary>(stack, val, obj);
                    } else if (APM_ShadingType2.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ShadingType2, PdfDictionary>(stack, val, obj);
                    } else if (APM_ShadingType3.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ShadingType3, PdfDictionary>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_PatternType2_Shading>("Shading did not match any allowable types: '[ShadingType1,ShadingType2,ShadingType3]'");
                    }
                    return;
                }
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_PatternType2_Shading>("Shading is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    if (APM_ShadingType4.MatchesType(ctx, val.Dictionary)) 
                    {
                        ctx.Run<APM_ShadingType4, PdfDictionary>(stack, val.Dictionary, obj);
                    } else if (APM_ShadingType5.MatchesType(ctx, val.Dictionary)) 
                    {
                        ctx.Run<APM_ShadingType5, PdfDictionary>(stack, val.Dictionary, obj);
                    } else if (APM_ShadingType6.MatchesType(ctx, val.Dictionary)) 
                    {
                        ctx.Run<APM_ShadingType6, PdfDictionary>(stack, val.Dictionary, obj);
                    } else if (APM_ShadingType7.MatchesType(ctx, val.Dictionary)) 
                    {
                        ctx.Run<APM_ShadingType7, PdfDictionary>(stack, val.Dictionary, obj);
                    }else 
                    {
                        ctx.Fail<APM_PatternType2_Shading>("Shading did not match any allowable types: '[ShadingType4,ShadingType5,ShadingType6,ShadingType7]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_PatternType2_Shading>("Shading is required to one of 'dictionary;stream', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// PatternType2_Matrix 
/// </summary>
internal partial class APM_PatternType2_Matrix : APM_PatternType2_Matrix__Base
{
}


internal partial class APM_PatternType2_Matrix__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PatternType2_Matrix";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_PatternType2_Matrix>(obj, "Matrix", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// PatternType2_ExtGState 
/// </summary>
internal partial class APM_PatternType2_ExtGState : APM_PatternType2_ExtGState__Base
{
}


internal partial class APM_PatternType2_ExtGState__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "PatternType2_ExtGState";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_PatternType2_ExtGState>(obj, "ExtGState", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_GraphicsStateParameter, PdfDictionary>(stack, val, obj);
        
    }


}

