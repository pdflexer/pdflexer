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
        var c = ctx.Clone();
        c.Run<APM_ArrayOf_2LineEndingsNames_0, PdfArray>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
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
        var (val, wasIR) = ctx.GetRequired<PdfName, APM_ArrayOf_2LineEndingsNames_0>(obj, 0, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Square || val == PdfName.Circle || val == PdfName.Diamond || val == PdfName.OpenArrow || val == PdfName.ClosedArrow || val == PdfName.None || val == PdfName.Butt || val == PdfName.ROpenArrow || val == PdfName.RClosedArrow || val == PdfName.Slash)) 
        {
            ctx.Fail<APM_ArrayOf_2LineEndingsNames_0>($"Invalid value {val}, allowed are: [Square,Circle,Diamond,OpenArrow,ClosedArrow,None,Butt,ROpenArrow,RClosedArrow,Slash]");
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
        var (val, wasIR) = ctx.GetRequired<PdfName, APM_ArrayOf_2LineEndingsNames_1>(obj, 1, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Square || val == PdfName.Circle || val == PdfName.Diamond || val == PdfName.OpenArrow || val == PdfName.ClosedArrow || val == PdfName.None || val == PdfName.Butt || val == PdfName.ROpenArrow || val == PdfName.RClosedArrow || val == PdfName.Slash)) 
        {
            ctx.Fail<APM_ArrayOf_2LineEndingsNames_1>($"Invalid value {val}, allowed are: [Square,Circle,Diamond,OpenArrow,ClosedArrow,None,Butt,ROpenArrow,RClosedArrow,Slash]");
        }
        // no linked objects
        
    }
}

