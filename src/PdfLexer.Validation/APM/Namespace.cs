// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_Namespace : APM_Namespace__Base
{
}

internal partial class APM_Namespace__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "Namespace";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_Namespace_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Namespace_NS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Namespace_Schema, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Namespace_RoleMapNS, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_Namespace>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_Namespace_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "NS", "Schema", "RoleMapNS"
    };
    


}

/// <summary>
/// Namespace_Type Table 356
/// </summary>
internal partial class APM_Namespace_Type : APM_Namespace_Type__Base
{
}


internal partial class APM_Namespace_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Namespace_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_Namespace_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Namespace)) 
        {
            ctx.Fail<APM_Namespace_Type>($"Invalid value {val}, allowed are: [Namespace]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// Namespace_NS 
/// </summary>
internal partial class APM_Namespace_NS : APM_Namespace_NS__Base
{
}


internal partial class APM_Namespace_NS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Namespace_NS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfString, APM_Namespace_NS>(obj, "NS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Namespace_Schema 
/// </summary>
internal partial class APM_Namespace_Schema : APM_Namespace_Schema__Base
{
}


internal partial class APM_Namespace_Schema__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Namespace_Schema";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_Namespace_Schema>(obj, "Schema", IndirectRequirement.Either);
        if (utval == null) { return; }
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
                ctx.Fail<APM_Namespace_Schema>("Schema is required to one of 'dictionary;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// Namespace_RoleMapNS 
/// </summary>
internal partial class APM_Namespace_RoleMapNS : APM_Namespace_RoleMapNS__Base
{
}


internal partial class APM_Namespace_RoleMapNS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Namespace_RoleMapNS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Namespace_RoleMapNS>(obj, "RoleMapNS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_RoleMapNS, PdfDictionary>(stack, val, obj);
        
    }


}
