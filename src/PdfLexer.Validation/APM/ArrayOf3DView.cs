// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOf3DView : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf3DView";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOf3DView_x, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOf3DView_* Table 311 and Table 341
/// </summary>
internal partial class APM_ArrayOf3DView_x : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf3DView_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        for (var i = 0; i<obj.Count; i+=1) 
        {
            CheckSingle(i);
        }
        void CheckSingle(int n) 
        {
            var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_ArrayOf3DView_x>(obj, n, IndirectRequirement.MustBeIndirect);
            if (val == null) { return; }
            // no special cases
            // no value restrictions
            ctx.Run<APM_3DView, PdfDictionary>(stack, val, obj);
            
        }
    }
}

