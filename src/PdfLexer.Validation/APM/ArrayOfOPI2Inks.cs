// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOfOPI2Inks : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfOPI2Inks";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOfOPI2Inks_0, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOfOPI2Inks_1x, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOfOPI2Inks_2x, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOfOPI2Inks_0 Table 407, Inks cell
/// </summary>
internal partial class APM_ArrayOfOPI2Inks_0 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfOPI2Inks_0";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_ArrayOfOPI2Inks_0>(obj, 0, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.monochrome)) 
        {
            ctx.Fail<APM_ArrayOfOPI2Inks_0>($"Invalid value {val}, allowed are: [monochrome]");
        }
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOfOPI2Inks_1* 
/// </summary>
internal partial class APM_ArrayOfOPI2Inks_1x : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfOPI2Inks_1*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        for (var i = 1; i<obj.Count; i+=3) 
        {
            CheckSingle(i);
        }
        void CheckSingle(int n) 
        {
            var val = ctx.GetRequired<PdfString, APM_ArrayOfOPI2Inks_1x>(obj, n, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            // no value restrictions
            // no linked objects
            
        }
    }
}

/// <summary>
/// ArrayOfOPI2Inks_2* 
/// </summary>
internal partial class APM_ArrayOfOPI2Inks_2x : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfOPI2Inks_2*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        for (var i = 2; i<obj.Count; i+=3) 
        {
            CheckSingle(i);
        }
        void CheckSingle(int n) 
        {
            var val = ctx.GetRequired<PdfNumber, APM_ArrayOfOPI2Inks_2x>(obj, n, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            IPdfObject v = val;
            
            if (!((gte(v,0)&&lte(v,1)))) 
            {
                ctx.Fail<APM_ArrayOfOPI2Inks_2x>($"Invalid value {val}, allowed are: [fn:Eval((@2*>=0) && (@2*<=1))]");
            }
            // no linked objects
            
        }
    }
}
