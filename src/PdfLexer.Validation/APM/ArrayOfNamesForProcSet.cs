// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOfNamesForProcSet : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfNamesForProcSet";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOfNamesForProcSet_x, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOfNamesForProcSet_* Table 34 and Table 346
/// </summary>
internal partial class APM_ArrayOfNamesForProcSet_x : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfNamesForProcSet_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m && version < 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        for (var i = 0; i<obj.Count; i+=1) 
        {
            CheckSingle(i);
        }
        void CheckSingle(int n) 
        {
            var val = ctx.GetOptional<PdfName, APM_ArrayOfNamesForProcSet_x>(obj, n, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            
            
            if (!(val == PdfName.PDF || val == PdfName.Text || val == PdfName.ImageB || val == PdfName.ImageC || val == PdfName.ImageI)) 
            {
                ctx.Fail<APM_ArrayOfNamesForProcSet_x>($"Invalid value {val}, allowed are: [PDF,Text,ImageB,ImageC,ImageI]");
            }
            // no linked objects
            
        }
    }
}

