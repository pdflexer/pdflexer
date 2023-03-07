// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_URTransformParamEFArray : ISpecification<PdfArray>
{
    public static string Name { get; } = "URTransformParamEFArray";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_URTransformParamEFArray_x, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// URTransformParamEFArray_* Table 258, EF cell
/// </summary>
internal partial class APM_URTransformParamEFArray_x : ISpecification<PdfArray>
{
    public static string Name { get; } = "URTransformParamEFArray_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        for (var i = 0; i<obj.Count; i+=1) 
        {
            CheckSingle(i);
        }
        void CheckSingle(int n) 
        {
            var val = ctx.GetOptional<PdfName, APM_URTransformParamEFArray_x>(obj, n, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            
            
            if (!(val == PdfName.Create || val == PdfName.Delete || val == PdfName.Modify || val == PdfName.Import)) 
            {
                ctx.Fail<APM_URTransformParamEFArray_x>($"Invalid value {val}, allowed are: [Create,Delete,Modify,Import]");
            }
            // no linked objects
            
        }
    }
}

