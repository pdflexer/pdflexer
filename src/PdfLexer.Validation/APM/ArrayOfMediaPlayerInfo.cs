// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOfMediaPlayerInfo : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfMediaPlayerInfo";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOfMediaPlayerInfo_x, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOfMediaPlayerInfo_* Table 301, MU, A and NU cells
/// </summary>
internal partial class APM_ArrayOfMediaPlayerInfo_x : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfMediaPlayerInfo_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        for (var i = 0; i<obj.Count; i+=1) 
        {
            CheckSingle(i);
        }
        void CheckSingle(int n) 
        {
            var val = ctx.GetOptional<PdfDictionary, APM_ArrayOfMediaPlayerInfo_x>(obj, n, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            // no value restrictions
            ctx.Run<APM_MediaPlayerInfo, PdfDictionary>(stack, val, obj);
            
        }
    }
}

