// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_RelatedFilesArray : ISpecification<PdfArray>
{
    public static string Name { get; } = "RelatedFilesArray";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_RelatedFilesArray_0x, PdfArray>(stack, obj, parent);
        ctx.Run<APM_RelatedFilesArray_1x, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// RelatedFilesArray_0* Clause 7.11.4.2 alternating string/stream array elements
/// </summary>
internal partial class APM_RelatedFilesArray_0x : ISpecification<PdfArray>
{
    public static string Name { get; } = "RelatedFilesArray_0*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        for (var i = 0; i<obj.Count; i+=2) 
        {
            CheckSingle(i);
        }
        void CheckSingle(int n) 
        {
            var (val, wasIR) = ctx.GetRequired<PdfString, APM_RelatedFilesArray_0x>(obj, n, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            // no value restrictions
            // no linked objects
            
        }
    }
}

/// <summary>
/// RelatedFilesArray_1* 
/// </summary>
internal partial class APM_RelatedFilesArray_1x : ISpecification<PdfArray>
{
    public static string Name { get; } = "RelatedFilesArray_1*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        for (var i = 1; i<obj.Count; i+=2) 
        {
            CheckSingle(i);
        }
        void CheckSingle(int n) 
        {
            var (val, wasIR) = ctx.GetRequired<PdfStream, APM_RelatedFilesArray_1x>(obj, n, IndirectRequirement.MustBeIndirect);
            if (val == null) { return; }
            // no special cases
            // no value restrictions
            ctx.Run<APM_EmbeddedFileStream, PdfDictionary>(stack, val.Dictionary, obj);
            
        }
    }
}

