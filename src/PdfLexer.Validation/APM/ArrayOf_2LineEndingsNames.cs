// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOf_2LineEndingsNames : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_2LineEndingsNames";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOf_2LineEndingsNames_0, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOf_2LineEndingsNames_1, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false; // TODO
    }
}

/// <summary>
/// ArrayOf_2LineEndingsNames_0 Table 179
/// </summary>
internal partial class APM_ArrayOf_2LineEndingsNames_0 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_2LineEndingsNames_0";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_ArrayOf_2LineEndingsNames_0>(obj, 0, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "Square" || val == "Circle" || val == "Diamond" || val == "OpenArrow" || val == "ClosedArrow" || val == "None" || val == "Butt" || val == "ROpenArrow" || val == "RClosedArrow" || val == "Slash")) 
        {
            ctx.Fail<APM_ArrayOf_2LineEndingsNames_0>($"Invalid value {val}, allowed are: [Square,Circle,Diamond,OpenArrow,ClosedArrow,None,Butt,ROpenArrow,RClosedArrow,Slash]");
        }
        }
        // no linked objects
        
    }
}

/// <summary>
/// ArrayOf_2LineEndingsNames_1 
/// </summary>
internal partial class APM_ArrayOf_2LineEndingsNames_1 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOf_2LineEndingsNames_1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_ArrayOf_2LineEndingsNames_1>(obj, 1, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "Square" || val == "Circle" || val == "Diamond" || val == "OpenArrow" || val == "ClosedArrow" || val == "None" || val == "Butt" || val == "ROpenArrow" || val == "RClosedArrow" || val == "Slash")) 
        {
            ctx.Fail<APM_ArrayOf_2LineEndingsNames_1>($"Invalid value {val}, allowed are: [Square,Circle,Diamond,OpenArrow,ClosedArrow,None,Butt,ROpenArrow,RClosedArrow,Slash]");
        }
        }
        // no linked objects
        
    }
}

