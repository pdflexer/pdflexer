// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOfNamesGeneral : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfNamesGeneral";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOfNamesGeneral_x, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOfNamesGeneral_* 
/// </summary>
internal partial class APM_ArrayOfNamesGeneral_x : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfNamesGeneral_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        for (var i = 0; i<obj.Count; i+=1) 
        {
            CheckSingle(i);
        }
        void CheckSingle(int n) 
        {
            var val = ctx.GetOptional<PdfName, APM_ArrayOfNamesGeneral_x>(obj, n, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            // no value restrictions
            // no linked objects
            
        }
    }
}

