// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_MarkedContentReference : APM_MarkedContentReference__Base
{
}

internal partial class APM_MarkedContentReference__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "MarkedContentReference";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_MarkedContentReference_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MarkedContentReference_Pg, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MarkedContentReference_Stm, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MarkedContentReference_StmOwn, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MarkedContentReference_MCID, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_MarkedContentReference>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_MarkedContentReference>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_MarkedContentReference>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_MarkedContentReference>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_MarkedContentReference>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_MarkedContentReference>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_MarkedContentReference>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_MarkedContentReference>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_MarkedContentReference_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_13 { get; } = new List<string> 
    {
        "Type", "Pg", "Stm", "StmOwn", "MCID"
    };
    public static List<string> AllowedFields_14 { get; } = new List<string> 
    {
        "Type", "Pg", "Stm", "StmOwn", "MCID"
    };
    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Type", "Pg", "Stm", "StmOwn", "MCID"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Type", "Pg", "Stm", "StmOwn", "MCID"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "Pg", "Stm", "StmOwn", "MCID"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "Pg", "Stm", "StmOwn", "MCID"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "Pg", "Stm", "StmOwn", "MCID"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "Pg", "Stm", "StmOwn", "MCID"
    };
    


}

/// <summary>
/// MarkedContentReference_Type Table 357
/// </summary>
internal partial class APM_MarkedContentReference_Type : APM_MarkedContentReference_Type__Base
{
}


internal partial class APM_MarkedContentReference_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MarkedContentReference_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_MarkedContentReference_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.MCR)) 
        {
            ctx.Fail<APM_MarkedContentReference_Type>($"Invalid value {val}, allowed are: [MCR]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// MarkedContentReference_Pg 
/// </summary>
internal partial class APM_MarkedContentReference_Pg : APM_MarkedContentReference_Pg__Base
{
}


internal partial class APM_MarkedContentReference_Pg__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MarkedContentReference_Pg";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_MarkedContentReference_Pg>(obj, "Pg", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_PageObject, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// MarkedContentReference_Stm 
/// </summary>
internal partial class APM_MarkedContentReference_Stm : APM_MarkedContentReference_Stm__Base
{
}


internal partial class APM_MarkedContentReference_Stm__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MarkedContentReference_Stm";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_MarkedContentReference_Stm>(obj, "Stm", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_XObjectFormType1, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// MarkedContentReference_StmOwn 
/// </summary>
internal partial class APM_MarkedContentReference_StmOwn : APM_MarkedContentReference_StmOwn__Base
{
}


internal partial class APM_MarkedContentReference_StmOwn__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MarkedContentReference_StmOwn";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_MarkedContentReference_StmOwn>(obj, "StmOwn", IndirectRequirement.MustBeIndirect);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    if (!wasIR) { ctx.Fail<APM_MarkedContentReference_StmOwn>("StmOwn is required to be indirect when a array"); return; }
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM__UniversalArray, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    if (!wasIR) { ctx.Fail<APM_MarkedContentReference_StmOwn>("StmOwn is required to be indirect when a dictionary"); return; }
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM__UniversalDictionary, PdfDictionary>(stack, val, obj);
                    return;
                }
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_MarkedContentReference_StmOwn>("StmOwn is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
                    return;
                }
            
            default:
                ctx.Fail<APM_MarkedContentReference_StmOwn>("StmOwn is required to one of 'array;dictionary;stream', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// MarkedContentReference_MCID 
/// </summary>
internal partial class APM_MarkedContentReference_MCID : APM_MarkedContentReference_MCID__Base
{
}


internal partial class APM_MarkedContentReference_MCID__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MarkedContentReference_MCID";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_MarkedContentReference_MCID>(obj, "MCID", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

