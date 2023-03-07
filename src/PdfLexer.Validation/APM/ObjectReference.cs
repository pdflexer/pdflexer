// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_ObjectReference : APM_ObjectReference__Base
{
}

internal partial class APM_ObjectReference__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "ObjectReference";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_ObjectReference_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ObjectReference_Pg, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ObjectReference_Obj, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_ObjectReference>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_ObjectReference>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_ObjectReference>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_ObjectReference>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_ObjectReference>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_ObjectReference>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_ObjectReference>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_ObjectReference>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_ObjectReference_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_13 { get; } = new List<string> 
    {
        "Type", "Pg", "Obj"
    };
    public static List<string> AllowedFields_14 { get; } = new List<string> 
    {
        "Type", "Pg", "Obj"
    };
    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Type", "Pg", "Obj"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Type", "Pg", "Obj"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "Pg", "Obj"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "Pg", "Obj"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "Pg", "Obj"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "Pg", "Obj"
    };
    


}

/// <summary>
/// ObjectReference_Type Table 358
/// </summary>
internal partial class APM_ObjectReference_Type : APM_ObjectReference_Type__Base
{
}


internal partial class APM_ObjectReference_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ObjectReference_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_ObjectReference_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.OBJR)) 
        {
            ctx.Fail<APM_ObjectReference_Type>($"Invalid value {val}, allowed are: [OBJR]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ObjectReference_Pg 
/// </summary>
internal partial class APM_ObjectReference_Pg : APM_ObjectReference_Pg__Base
{
}


internal partial class APM_ObjectReference_Pg__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ObjectReference_Pg";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_ObjectReference_Pg>(obj, "Pg", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_PageObject, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// ObjectReference_Obj 
/// </summary>
internal partial class APM_ObjectReference_Obj : APM_ObjectReference_Obj__Base
{
}


internal partial class APM_ObjectReference_Obj__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ObjectReference_Obj";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ObjectReference_Obj>(obj, "Obj", IndirectRequirement.MustBeIndirect);
        if (utval == null) { ctx.Fail<APM_ObjectReference_Obj>("Obj is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    if (!wasIR) { ctx.Fail<APM_ObjectReference_Obj>("Obj is required to be indirect when a array"); return; }
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM__UniversalArray, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    if (!wasIR) { ctx.Fail<APM_ObjectReference_Obj>("Obj is required to be indirect when a dictionary"); return; }
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM__UniversalDictionary, PdfDictionary>(stack, val, obj);
                    return;
                }
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_ObjectReference_Obj>("Obj is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_XObjectFormType1, PdfDictionary>(stack, val.Dictionary, obj);
                    return;
                }
            
            default:
                ctx.Fail<APM_ObjectReference_Obj>("Obj is required to one of 'array;dictionary;stream', was " + utval.Type);
                return;
        }
    }


}
