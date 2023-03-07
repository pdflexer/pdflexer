// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOfNamesInPtData : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfNamesInPtData";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOfNamesInPtData_x, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOfNamesInPtData_* Table 272, Names cell
/// </summary>
internal partial class APM_ArrayOfNamesInPtData_x : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfNamesInPtData_*";
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
            var val = ctx.GetOptional<PdfName, APM_ArrayOfNamesInPtData_x>(obj, n, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            
            
            if (!(val == PdfName.LAT || val == PdfName.LON || val == PdfName.ALT)) 
            {
                ctx.Fail<APM_ArrayOfNamesInPtData_x>($"Invalid value {val}, allowed are: [LAT,LON,ALT]");
            }
            // no linked objects
            
        }
    }
}
