// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_BorderStyle : APM_BorderStyle__Base
{
}

internal partial class APM_BorderStyle__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "BorderStyle";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_BorderStyle_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_BorderStyle_W, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_BorderStyle_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_BorderStyle_D, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
            case 1.3m:
            case 1.4m:
            case 1.5m:
            case 1.6m:
            case 1.7m:
            case 1.8m:
            case 1.9m:
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12_13_14_15_16_17_18_19_20.Contains(x)))
                {
                    ctx.Fail<APM_BorderStyle>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_BorderStyle_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_12_13_14_15_16_17_18_19_20 { get; } = new List<string> 
    {
        "D", "S", "Type", "W"
    };
    


}

/// <summary>
/// BorderStyle_Type Table 168
/// </summary>
internal partial class APM_BorderStyle_Type : APM_BorderStyle_Type__Base
{
}


internal partial class APM_BorderStyle_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "BorderStyle_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_BorderStyle_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Border)) 
        {
            ctx.Fail<APM_BorderStyle_Type>($"Invalid value {val}, allowed are: [Border]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// BorderStyle_W 
/// </summary>
internal partial class APM_BorderStyle_W : APM_BorderStyle_W__Base
{
}


internal partial class APM_BorderStyle_W__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "BorderStyle_W";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfNumber, APM_BorderStyle_W>(obj, "W", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var W = obj.Get("W");
        if (!(gte(W,0.0m))) 
        {
            ctx.Fail<APM_BorderStyle_W>($"Invalid value {val}, allowed are: [fn:Eval(@W>=0.0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// BorderStyle_S 
/// </summary>
internal partial class APM_BorderStyle_S : APM_BorderStyle_S__Base
{
}


internal partial class APM_BorderStyle_S__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "BorderStyle_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_BorderStyle_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.S || val == PdfName.B || val == PdfName.D || val == PdfName.I || val == PdfName.U)) 
        {
            ctx.Fail<APM_BorderStyle_S>($"Invalid value {val}, allowed are: [S,B,D,I,U]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// BorderStyle_D 
/// </summary>
internal partial class APM_BorderStyle_D : APM_BorderStyle_D__Base
{
}


internal partial class APM_BorderStyle_D__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "BorderStyle_D";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_BorderStyle_D>(obj, "D", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:Ignore, not pertinent to validation
        // no value restrictions
        ctx.Run<APM_ArrayOfDashPatterns, PdfArray>(stack, val, obj);
        
    }


}

