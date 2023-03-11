// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_SoftwareIdentifier : APM_SoftwareIdentifier__Base
{
}

internal partial class APM_SoftwareIdentifier__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "SoftwareIdentifier";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_SoftwareIdentifier_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoftwareIdentifier_U, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoftwareIdentifier_L, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoftwareIdentifier_LI, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoftwareIdentifier_H, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoftwareIdentifier_HI, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SoftwareIdentifier_OS, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_SoftwareIdentifier>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_SoftwareIdentifier>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_SoftwareIdentifier>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_SoftwareIdentifier>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_SoftwareIdentifier>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_SoftwareIdentifier>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_SoftwareIdentifier_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "U", "L", "LI", "H", "HI", "OS"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "U", "L", "LI", "H", "HI", "OS"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "U", "L", "LI", "H", "HI", "OS"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "U", "L", "LI", "H", "HI", "OS"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "U", "L", "LI", "H", "HI", "OS"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "U", "L", "LI", "H", "HI", "OS"
    };
    


}

/// <summary>
/// SoftwareIdentifier_Type Table 303
/// </summary>
internal partial class APM_SoftwareIdentifier_Type : APM_SoftwareIdentifier_Type__Base
{
}


internal partial class APM_SoftwareIdentifier_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoftwareIdentifier_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_SoftwareIdentifier_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.SoftwareIdentifier)) 
        {
            ctx.Fail<APM_SoftwareIdentifier_Type>($"Invalid value {val}, allowed are: [SoftwareIdentifier]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// SoftwareIdentifier_U 
/// </summary>
internal partial class APM_SoftwareIdentifier_U : APM_SoftwareIdentifier_U__Base
{
}


internal partial class APM_SoftwareIdentifier_U__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoftwareIdentifier_U";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfString, APM_SoftwareIdentifier_U>(obj, "U", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// SoftwareIdentifier_L 
/// </summary>
internal partial class APM_SoftwareIdentifier_L : APM_SoftwareIdentifier_L__Base
{
}


internal partial class APM_SoftwareIdentifier_L__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoftwareIdentifier_L";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_SoftwareIdentifier_L>(obj, "L", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfSoftwareVersions, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// SoftwareIdentifier_LI 
/// </summary>
internal partial class APM_SoftwareIdentifier_LI : APM_SoftwareIdentifier_LI__Base
{
}


internal partial class APM_SoftwareIdentifier_LI__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoftwareIdentifier_LI";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfBoolean, APM_SoftwareIdentifier_LI>(obj, "LI", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// SoftwareIdentifier_H 
/// </summary>
internal partial class APM_SoftwareIdentifier_H : APM_SoftwareIdentifier_H__Base
{
}


internal partial class APM_SoftwareIdentifier_H__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoftwareIdentifier_H";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_SoftwareIdentifier_H>(obj, "H", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfSoftwareVersions, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// SoftwareIdentifier_HI 
/// </summary>
internal partial class APM_SoftwareIdentifier_HI : APM_SoftwareIdentifier_HI__Base
{
}


internal partial class APM_SoftwareIdentifier_HI__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoftwareIdentifier_HI";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfBoolean, APM_SoftwareIdentifier_HI>(obj, "HI", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// SoftwareIdentifier_OS 
/// </summary>
internal partial class APM_SoftwareIdentifier_OS : APM_SoftwareIdentifier_OS__Base
{
}


internal partial class APM_SoftwareIdentifier_OS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SoftwareIdentifier_OS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_SoftwareIdentifier_OS>(obj, "OS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfStringsByte, PdfArray>(stack, val, obj);
        
    }


}

