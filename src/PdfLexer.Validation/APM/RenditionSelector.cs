// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_RenditionSelector : APM_RenditionSelector__Base
{
}

internal partial class APM_RenditionSelector__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "RenditionSelector";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_RenditionSelector_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RenditionSelector_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RenditionSelector_N, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RenditionSelector_MH, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RenditionSelector_BE, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RenditionSelector_R, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_RenditionSelector>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_RenditionSelector>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_RenditionSelector>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_RenditionSelector>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_RenditionSelector>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_RenditionSelector>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_RenditionSelector_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "S", "N", "MH", "BE", "R"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "S", "N", "MH", "BE", "R"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "S", "N", "MH", "BE", "R"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "S", "N", "MH", "BE", "R"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "S", "N", "MH", "BE", "R"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "S", "N", "MH", "BE", "R"
    };
    


}

/// <summary>
/// RenditionSelector_Type Table 277 and Table 283
/// </summary>
internal partial class APM_RenditionSelector_Type : APM_RenditionSelector_Type__Base
{
}


internal partial class APM_RenditionSelector_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RenditionSelector_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_RenditionSelector_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Rendition)) 
        {
            ctx.Fail<APM_RenditionSelector_Type>($"Invalid value {val}, allowed are: [Rendition]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// RenditionSelector_S 
/// </summary>
internal partial class APM_RenditionSelector_S : APM_RenditionSelector_S__Base
{
}


internal partial class APM_RenditionSelector_S__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RenditionSelector_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfName, APM_RenditionSelector_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.SR)) 
        {
            ctx.Fail<APM_RenditionSelector_S>($"Invalid value {val}, allowed are: [SR]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// RenditionSelector_N https://github.com/pdf-association/pdf-issues/issues/214
/// </summary>
internal partial class APM_RenditionSelector_N : APM_RenditionSelector_N__Base
{
}


internal partial class APM_RenditionSelector_N__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RenditionSelector_N";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_RenditionSelector_N>(obj, "N", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// RenditionSelector_MH 
/// </summary>
internal partial class APM_RenditionSelector_MH : APM_RenditionSelector_MH__Base
{
}


internal partial class APM_RenditionSelector_MH__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RenditionSelector_MH";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_RenditionSelector_MH>(obj, "MH", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_RenditionMH, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// RenditionSelector_BE 
/// </summary>
internal partial class APM_RenditionSelector_BE : APM_RenditionSelector_BE__Base
{
}


internal partial class APM_RenditionSelector_BE__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RenditionSelector_BE";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_RenditionSelector_BE>(obj, "BE", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_RenditionBE, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// RenditionSelector_R 
/// </summary>
internal partial class APM_RenditionSelector_R : APM_RenditionSelector_R__Base
{
}


internal partial class APM_RenditionSelector_R__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RenditionSelector_R";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfArray, APM_RenditionSelector_R>(obj, "R", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfRenditions, PdfArray>(stack, val, obj);
        
    }


}

