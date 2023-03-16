// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_FixedPrint : APM_FixedPrint__Base
{
}

internal partial class APM_FixedPrint__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "FixedPrint";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_FixedPrint_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FixedPrint_Matrix, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FixedPrint_H, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FixedPrint_V, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.6m:
            case 1.7m:
            case 1.8m:
            case 1.9m:
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16_17_18_19_20.Contains(x)))
                {
                    ctx.Fail<APM_FixedPrint>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_FixedPrint_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_16_17_18_19_20 { get; } = new List<string> 
    {
        "H", "Matrix", "Type", "V"
    };
    


}

/// <summary>
/// FixedPrint_Type Table 194
/// </summary>
internal partial class APM_FixedPrint_Type : APM_FixedPrint_Type__Base
{
}


internal partial class APM_FixedPrint_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FixedPrint_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfName, APM_FixedPrint_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.FixedPrint)) 
        {
            ctx.Fail<APM_FixedPrint_Type>($"Invalid value {val}, allowed are: [FixedPrint]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FixedPrint_Matrix 
/// </summary>
internal partial class APM_FixedPrint_Matrix : APM_FixedPrint_Matrix__Base
{
}


internal partial class APM_FixedPrint_Matrix__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FixedPrint_Matrix";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_FixedPrint_Matrix>(obj, "Matrix", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FixedPrint_H 
/// </summary>
internal partial class APM_FixedPrint_H : APM_FixedPrint_H__Base
{
}


internal partial class APM_FixedPrint_H__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FixedPrint_H";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfNumber, APM_FixedPrint_H>(obj, "H", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FixedPrint_V 
/// </summary>
internal partial class APM_FixedPrint_V : APM_FixedPrint_V__Base
{
}


internal partial class APM_FixedPrint_V__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FixedPrint_V";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfNumber, APM_FixedPrint_V>(obj, "V", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

