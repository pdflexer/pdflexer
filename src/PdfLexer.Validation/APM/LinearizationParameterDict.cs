// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_LinearizationParameterDict : APM_LinearizationParameterDict__Base
{
}

internal partial class APM_LinearizationParameterDict__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "LinearizationParameterDict";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_LinearizationParameterDict_Linearized, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_LinearizationParameterDict_L, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_LinearizationParameterDict_H, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_LinearizationParameterDict_O, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_LinearizationParameterDict_E, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_LinearizationParameterDict_N, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_LinearizationParameterDict_T, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_LinearizationParameterDict_P, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_LinearizationParameterDict>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_LinearizationParameterDict>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_LinearizationParameterDict>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_LinearizationParameterDict>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_LinearizationParameterDict>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_LinearizationParameterDict>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_LinearizationParameterDict>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_LinearizationParameterDict>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_LinearizationParameterDict>($"Unknown field {extra} for version 2.0");
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

    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Linearized", "L", "H", "O", "E", "N", "T", "P"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Linearized", "L", "H", "O", "E", "N", "T", "P"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Linearized", "L", "H", "O", "E", "N", "T", "P"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Linearized", "L", "H", "O", "E", "N", "T", "P"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Linearized", "L", "H", "O", "E", "N", "T", "P"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Linearized", "L", "H", "O", "E", "N", "T", "P"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Linearized", "L", "H", "O", "E", "N", "T", "P"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Linearized", "L", "H", "O", "E", "N", "T", "P"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Linearized", "L", "H", "O", "E", "N", "T", "P"
    };
    


}

/// <summary>
/// LinearizationParameterDict_Linearized Table F.1 version - https://github.com/pdf-association/pdf-issues/issues/153
/// </summary>
internal partial class APM_LinearizationParameterDict_Linearized : APM_LinearizationParameterDict_Linearized__Base
{
}


internal partial class APM_LinearizationParameterDict_Linearized__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "LinearizationParameterDict_Linearized";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        // TODO complex IR
        var val = ctx.GetRequired<PdfNumber, APM_LinearizationParameterDict_Linearized>(obj, "Linearized", IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 1.0m)) 
        {
            ctx.Fail<APM_LinearizationParameterDict_Linearized>($"Invalid value {val}, allowed are: [1.0]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// LinearizationParameterDict_L length of file
/// </summary>
internal partial class APM_LinearizationParameterDict_L : APM_LinearizationParameterDict_L__Base
{
}


internal partial class APM_LinearizationParameterDict_L__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "LinearizationParameterDict_L";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        // TODO complex IR
        var val = ctx.GetRequired<PdfIntNumber, APM_LinearizationParameterDict_L>(obj, "L", IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        // no special cases
        
        var L = obj.Get("L");
        if (!(gt(L,0))) 
        {
            ctx.Fail<APM_LinearizationParameterDict_L>($"Invalid value {val}, allowed are: [fn:Eval(@L>0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// LinearizationParameterDict_H 
/// </summary>
internal partial class APM_LinearizationParameterDict_H : APM_LinearizationParameterDict_H__Base
{
}


internal partial class APM_LinearizationParameterDict_H__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "LinearizationParameterDict_H";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        // TODO complex IR
        var val = ctx.GetRequired<PdfArray, APM_LinearizationParameterDict_H>(obj, "H", IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_ArrayOf_2Integers.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_ArrayOf_2Integers, PdfArray>(stack, val, obj);
        } else if (APM_ArrayOf_4Integers.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_ArrayOf_4Integers, PdfArray>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_LinearizationParameterDict_H>("H did not match any allowable types: '[ArrayOf_2Integers,ArrayOf_4Integers]'");
        }
        
    }


}

/// <summary>
/// LinearizationParameterDict_O object number of the first pages page object
/// </summary>
internal partial class APM_LinearizationParameterDict_O : APM_LinearizationParameterDict_O__Base
{
}


internal partial class APM_LinearizationParameterDict_O__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "LinearizationParameterDict_O";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        // TODO complex IR
        var val = ctx.GetRequired<PdfIntNumber, APM_LinearizationParameterDict_O>(obj, "O", IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        // no special cases
        
        var O = obj.Get("O");
        if (!(gt(O,0))) 
        {
            ctx.Fail<APM_LinearizationParameterDict_O>($"Invalid value {val}, allowed are: [fn:Eval(@O>0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// LinearizationParameterDict_E offset of the end of the first page
/// </summary>
internal partial class APM_LinearizationParameterDict_E : APM_LinearizationParameterDict_E__Base
{
}


internal partial class APM_LinearizationParameterDict_E__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "LinearizationParameterDict_E";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        // TODO complex IR
        var val = ctx.GetRequired<PdfIntNumber, APM_LinearizationParameterDict_E>(obj, "E", IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        // no special cases
        
        var E = obj.Get("E");
        var L = obj.Get("L");
        if (!((gt(E,0)&&lte(E,L)))) 
        {
            ctx.Fail<APM_LinearizationParameterDict_E>($"Invalid value {val}, allowed are: [fn:Eval((@E>0) && (@E<=@L))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// LinearizationParameterDict_N number of pages
/// </summary>
internal partial class APM_LinearizationParameterDict_N : APM_LinearizationParameterDict_N__Base
{
}


internal partial class APM_LinearizationParameterDict_N__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "LinearizationParameterDict_N";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        // TODO complex IR
        var val = ctx.GetRequired<PdfIntNumber, APM_LinearizationParameterDict_N>(obj, "N", IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        // no special cases
        
        var N = obj.Get("N");
        if (!(gt(N,0))) 
        {
            ctx.Fail<APM_LinearizationParameterDict_N>($"Invalid value {val}, allowed are: [fn:Eval(@N>0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// LinearizationParameterDict_T 
/// </summary>
internal partial class APM_LinearizationParameterDict_T : APM_LinearizationParameterDict_T__Base
{
}


internal partial class APM_LinearizationParameterDict_T__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "LinearizationParameterDict_T";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        // TODO complex IR
        var val = ctx.GetRequired<PdfIntNumber, APM_LinearizationParameterDict_T>(obj, "T", IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        // no special cases
        
        var T = obj.Get("T");
        if (!(gt(T,0))) 
        {
            ctx.Fail<APM_LinearizationParameterDict_T>($"Invalid value {val}, allowed are: [fn:Eval(@T>0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// LinearizationParameterDict_P page index of first page
/// </summary>
internal partial class APM_LinearizationParameterDict_P : APM_LinearizationParameterDict_P__Base
{
}


internal partial class APM_LinearizationParameterDict_P__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "LinearizationParameterDict_P";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        // TODO complex IR
        var val = ctx.GetOptional<PdfIntNumber, APM_LinearizationParameterDict_P>(obj, "P", IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        // no special cases
        
        var P = obj.Get("P");
        if (!(gte(P,0))) 
        {
            ctx.Fail<APM_LinearizationParameterDict_P>($"Invalid value {val}, allowed are: [fn:Eval(@P>=0)]");
        }
        // no linked objects
        
    }


}

