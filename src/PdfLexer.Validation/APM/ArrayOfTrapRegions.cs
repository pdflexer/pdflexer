// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOfTrapRegions : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfTrapRegions";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOfTrapRegions_x, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOfTrapRegions_* Table 404, TrapRegion cell
/// </summary>
internal partial class APM_ArrayOfTrapRegions_x : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfTrapRegions_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        for (var i = 0; i<obj.Count; i+=1) 
        {
            CheckSingle(i);
        }
        void CheckSingle(int n) 
        {
            var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_ArrayOfTrapRegions_x>(obj, n, IndirectRequirement.MustBeIndirect);
            if (val == null) { return; }
            // no special cases
            // no value restrictions
            ctx.Run<APM_TrapRegion, PdfDictionary>(stack, val, obj);
            
        }
    }
}

