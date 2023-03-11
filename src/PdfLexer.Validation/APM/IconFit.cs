// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_IconFit : APM_IconFit__Base
{
}

internal partial class APM_IconFit__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "IconFit";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_IconFit_SW, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_IconFit_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_IconFit_A, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_IconFit_FB, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_IconFit>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_IconFit>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_IconFit>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_IconFit>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_IconFit>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_IconFit>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_IconFit>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_IconFit>($"Unknown field {extra} for version 2.0");
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

    public static List<string> AllowedFields_13 { get; } = new List<string> 
    {
        "SW", "S", "A"
    };
    public static List<string> AllowedFields_14 { get; } = new List<string> 
    {
        "SW", "S", "A"
    };
    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "SW", "S", "A", "FB"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "SW", "S", "A", "FB"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "SW", "S", "A", "FB"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "SW", "S", "A", "FB"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "SW", "S", "A", "FB"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "SW", "S", "A", "FB"
    };
    


}

/// <summary>
/// IconFit_SW Table 250
/// </summary>
internal partial class APM_IconFit_SW : APM_IconFit_SW__Base
{
}


internal partial class APM_IconFit_SW__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "IconFit_SW";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_IconFit_SW>(obj, "SW", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.A || val == PdfName.B || val == PdfName.S || val == PdfName.N)) 
        {
            ctx.Fail<APM_IconFit_SW>($"Invalid value {val}, allowed are: [A,B,S,N]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// IconFit_S 
/// </summary>
internal partial class APM_IconFit_S : APM_IconFit_S__Base
{
}


internal partial class APM_IconFit_S__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "IconFit_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_IconFit_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.A || val == PdfName.P)) 
        {
            ctx.Fail<APM_IconFit_S>($"Invalid value {val}, allowed are: [A,P]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// IconFit_A 
/// </summary>
internal partial class APM_IconFit_A : APM_IconFit_A__Base
{
}


internal partial class APM_IconFit_A__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "IconFit_A";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_IconFit_A>(obj, "A", IndirectRequirement.Either);
        if (val == null) { return; }
        var A0 = val.Get(0);
        var A1 = val.Get(1);
        if (!(gte(A0,0)&&lte(A0,1)&&gte(A1,0)&&lte(A1,1))) 
        {
            ctx.Fail<APM_IconFit_A>($"Value failed special case check: fn:Eval((A::@0>=0) && (A::@0<=1) && (A::@1>=0) && (A::@1<=1))");
        }
        // no value restrictions
        ctx.Run<APM_ArrayOf_2Numbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// IconFit_FB 
/// </summary>
internal partial class APM_IconFit_FB : APM_IconFit_FB__Base
{
}


internal partial class APM_IconFit_FB__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "IconFit_FB";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfBoolean, APM_IconFit_FB>(obj, "FB", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

