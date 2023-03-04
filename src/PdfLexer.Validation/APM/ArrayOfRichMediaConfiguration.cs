// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOfRichMediaConfiguration : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfRichMediaConfiguration";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOfRichMediaConfiguration_x, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false; // TODO
    }
}

/// <summary>
/// ArrayOfRichMediaConfiguration_* Table 341
/// </summary>
internal partial class APM_ArrayOfRichMediaConfiguration_x : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfRichMediaConfiguration_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        for (var i = 0; i<obj.Count; i+=1) 
        {
            CheckSingle(i);
        }
        void CheckSingle(int n) 
        {
            var val = ctx.GetOptional<PdfDictionary, APM_ArrayOfRichMediaConfiguration_x>(obj, n, IndirectRequirement.MustBeIndirect);
            if (val == null) { return; }
            // no special cases
            // no value restrictions
            ctx.Run<APM_RichMediaConfiguration, PdfDictionary>(stack, val, obj);
            
        }
    }
}

