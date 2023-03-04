// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOfFonts : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfFonts";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOfFonts_x, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false; // TODO
    }
}

/// <summary>
/// ArrayOfFonts_* 
/// </summary>
internal partial class APM_ArrayOfFonts_x : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfFonts_*";
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
            var val = ctx.GetOptional<PdfDictionary, APM_ArrayOfFonts_x>(obj, n, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            // no value restrictions
            if (APM_FontType1.MatchesType(ctx, val)) 
            {
                ctx.Run<APM_FontType1, PdfDictionary>(stack, val, obj);
            } else if (APM_FontTrueType.MatchesType(ctx, val)) 
            {
                ctx.Run<APM_FontTrueType, PdfDictionary>(stack, val, obj);
            } else if (APM_FontMultipleMaster.MatchesType(ctx, val)) 
            {
                ctx.Run<APM_FontMultipleMaster, PdfDictionary>(stack, val, obj);
            } else if (APM_FontType3.MatchesType(ctx, val)) 
            {
                ctx.Run<APM_FontType3, PdfDictionary>(stack, val, obj);
            } else if (APM_FontType0.MatchesType(ctx, val)) 
            {
                ctx.Run<APM_FontType0, PdfDictionary>(stack, val, obj);
            } else if (APM_FontCIDType0.MatchesType(ctx, val)) 
            {
                ctx.Run<APM_FontCIDType0, PdfDictionary>(stack, val, obj);
            } else if (APM_FontCIDType2.MatchesType(ctx, val)) 
            {
                ctx.Run<APM_FontCIDType2, PdfDictionary>(stack, val, obj);
            }else 
            {
                ctx.Fail<APM_ArrayOfFonts_x>("n did not match any allowable types: '[FontType1,FontTrueType,FontMultipleMaster,FontType3,FontType0,FontCIDType0,FontCIDType2]'");
            }
            
        }
    }
}

