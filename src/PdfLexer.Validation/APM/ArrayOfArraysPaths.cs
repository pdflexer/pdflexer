// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOfArraysPaths : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfArraysPaths";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOfArraysPaths_0, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOfArraysPaths_x, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOfArraysPaths_0 Table 181 or Table 185 - must be moveto
/// </summary>
internal partial class APM_ArrayOfArraysPaths_0 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfArraysPaths_0";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_ArrayOfArraysPaths_0>(obj, 0, IndirectRequirement.Either);
        if (val == null) { return; }
        
        if (!(eq(((val as PdfArray)?.Count),2))) 
        {
            ctx.Fail<APM_ArrayOfArraysPaths_0>($"Value failed special case check: fn:Eval(fn:ArrayLength(0)==2)");
        }
        // no value restrictions
        ctx.Run<APM_ArrayOfPaths, PdfArray>(stack, val, obj);
        
    }
}

/// <summary>
/// ArrayOfArraysPaths_* must be lineto or curveto
/// </summary>
internal partial class APM_ArrayOfArraysPaths_x : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfArraysPaths_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        for (var i = 0; i<obj.Count; i+=1) 
        {
            CheckSingle(i);
        }
        void CheckSingle(int n) 
        {
            var val = ctx.GetOptional<PdfArray, APM_ArrayOfArraysPaths_x>(obj, n, IndirectRequirement.Either);
            if (val == null) { return; }
            
            if (!((eq(((val as PdfArray)?.Count),2)||eq(((val as PdfArray)?.Count),6)))) 
            {
                ctx.Fail<APM_ArrayOfArraysPaths_x>($"Value failed special case check: fn:Eval((fn:ArrayLength(*)==2) || (fn:ArrayLength(*)==6))");
            }
            // no value restrictions
            ctx.Run<APM_ArrayOfPaths, PdfArray>(stack, val, obj);
            
        }
    }
}

