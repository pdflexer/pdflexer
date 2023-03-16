// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_SignatureBuildDataDict : APM_SignatureBuildDataDict__Base
{
}

internal partial class APM_SignatureBuildDataDict__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "SignatureBuildDataDict";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_SignatureBuildDataDict_Name, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SignatureBuildDataDict_Date, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SignatureBuildDataDict_R, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SignatureBuildDataDict_PreRelease, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SignatureBuildDataDict_OS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SignatureBuildDataDict_NonEFontNoWarn, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SignatureBuildDataDict_TrustedMode, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SignatureBuildDataDict_V, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15_16.Contains(x)))
                {
                    ctx.Fail<APM_SignatureBuildDataDict>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            case 1.7m:
            case 1.8m:
            case 1.9m:
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17_18_19_20.Contains(x)))
                {
                    ctx.Fail<APM_SignatureBuildDataDict>($"Unknown field {extra} for version {ctx.Version}");
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

    public static HashSet<string> AllowedFields_15_16 { get; } = new HashSet<string> 
    {
        "Date", "Name", "NonEFontNoWarn", "OS", "PreRelease", "R", "TrustedMode", "V"
    };
    public static HashSet<string> AllowedFields_17_18_19_20 { get; } = new HashSet<string> 
    {
        "Date", "Name", "NonEFontNoWarn", "OS", "PreRelease", "R", "TrustedMode"
    };
    


}

/// <summary>
/// SignatureBuildDataDict_Name Adobe "Digital Signature Build Dictionary Specification" Table 2
/// </summary>
internal partial class APM_SignatureBuildDataDict_Name : APM_SignatureBuildDataDict_Name__Base
{
}


internal partial class APM_SignatureBuildDataDict_Name__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SignatureBuildDataDict_Name";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_SignatureBuildDataDict_Name>(obj, "Name", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// SignatureBuildDataDict_Date 
/// </summary>
internal partial class APM_SignatureBuildDataDict_Date : APM_SignatureBuildDataDict_Date__Base
{
}


internal partial class APM_SignatureBuildDataDict_Date__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SignatureBuildDataDict_Date";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_SignatureBuildDataDict_Date>(obj, "Date", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// SignatureBuildDataDict_R 
/// </summary>
internal partial class APM_SignatureBuildDataDict_R : APM_SignatureBuildDataDict_R__Base
{
}


internal partial class APM_SignatureBuildDataDict_R__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SignatureBuildDataDict_R";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfNumber, APM_SignatureBuildDataDict_R>(obj, "R", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// SignatureBuildDataDict_PreRelease 
/// </summary>
internal partial class APM_SignatureBuildDataDict_PreRelease : APM_SignatureBuildDataDict_PreRelease__Base
{
}


internal partial class APM_SignatureBuildDataDict_PreRelease__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SignatureBuildDataDict_PreRelease";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfBoolean, APM_SignatureBuildDataDict_PreRelease>(obj, "PreRelease", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// SignatureBuildDataDict_OS 
/// </summary>
internal partial class APM_SignatureBuildDataDict_OS : APM_SignatureBuildDataDict_OS__Base
{
}


internal partial class APM_SignatureBuildDataDict_OS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SignatureBuildDataDict_OS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_SignatureBuildDataDict_OS>(obj, "OS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfStringsText, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// SignatureBuildDataDict_NonEFontNoWarn 
/// </summary>
internal partial class APM_SignatureBuildDataDict_NonEFontNoWarn : APM_SignatureBuildDataDict_NonEFontNoWarn__Base
{
}


internal partial class APM_SignatureBuildDataDict_NonEFontNoWarn__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SignatureBuildDataDict_NonEFontNoWarn";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfBoolean, APM_SignatureBuildDataDict_NonEFontNoWarn>(obj, "NonEFontNoWarn", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// SignatureBuildDataDict_TrustedMode 
/// </summary>
internal partial class APM_SignatureBuildDataDict_TrustedMode : APM_SignatureBuildDataDict_TrustedMode__Base
{
}


internal partial class APM_SignatureBuildDataDict_TrustedMode__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SignatureBuildDataDict_TrustedMode";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfBoolean, APM_SignatureBuildDataDict_TrustedMode>(obj, "TrustedMode", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// SignatureBuildDataDict_V 
/// </summary>
internal partial class APM_SignatureBuildDataDict_V : APM_SignatureBuildDataDict_V__Base
{
}


internal partial class APM_SignatureBuildDataDict_V__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SignatureBuildDataDict_V";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m && version < 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfNumber, APM_SignatureBuildDataDict_V>(obj, "V", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

