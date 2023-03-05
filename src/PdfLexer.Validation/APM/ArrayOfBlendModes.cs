// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOfBlendModes : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfBlendModes";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOfBlendModes_x, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOfBlendModes_* Table 134 and Table 135
/// </summary>
internal partial class APM_ArrayOfBlendModes_x : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfBlendModes_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        for (var i = 0; i<obj.Count; i+=1) 
        {
            CheckSingle(i);
        }
        void CheckSingle(int n) 
        {
            var val = ctx.GetOptional<PdfName, APM_ArrayOfBlendModes_x>(obj, n, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            
            
            if (!((ctx.Version <= 1.4m && val == "Compatible") || val == "Normal" || val == "Multiply" || val == "Screen" || val == "Difference" || val == "Darken" || val == "Lighten" || val == "ColorDodge" || val == "ColorBurn" || val == "Exclusion" || val == "HardLight" || val == "Overlay" || val == "SoftLight" || val == "Luminosity" || val == "Hue" || val == "Saturation" || val == "Color")) 
            {
                ctx.Fail<APM_ArrayOfBlendModes_x>($"Invalid value {val}, allowed are: [fn:Deprecated(1.4,Compatible),Normal,Multiply,Screen,Difference,Darken,Lighten,ColorDodge,ColorBurn,Exclusion,HardLight,Overlay,SoftLight,Luminosity,Hue,Saturation,Color]");
            }
            // no linked objects
            
        }
    }
}

