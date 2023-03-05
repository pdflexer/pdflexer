// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_RichMediaCommand : APM_RichMediaCommand__Base
{
}

internal partial class APM_RichMediaCommand__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "RichMediaCommand";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_RichMediaCommand_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaCommand_C, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaCommand_A, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
        
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_RichMediaCommand_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    


}

/// <summary>
/// RichMediaCommand_Type Table 223
/// </summary>
internal partial class APM_RichMediaCommand_Type : APM_RichMediaCommand_Type__Base
{
}


internal partial class APM_RichMediaCommand_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaCommand_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_RichMediaCommand_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "RichMediaCommand")) 
        {
            ctx.Fail<APM_RichMediaCommand_Type>($"Invalid value {val}, allowed are: [RichMediaCommand]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// RichMediaCommand_C 
/// </summary>
internal partial class APM_RichMediaCommand_C : APM_RichMediaCommand_C__Base
{
}


internal partial class APM_RichMediaCommand_C__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaCommand_C";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfString, APM_RichMediaCommand_C>(obj, "C", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// RichMediaCommand_A 
/// </summary>
internal partial class APM_RichMediaCommand_A : APM_RichMediaCommand_A__Base
{
}


internal partial class APM_RichMediaCommand_A__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaCommand_A";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_RichMediaCommand_A>(obj, "A", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_RichMediaCommandArray, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.BooleanObj:
                {
                    var val =  (PdfBoolean)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
                    return;
                }
            
            // TODO MC array;boolean;integer;number;string-text
            case PdfObjectType.StringObj:
                {
                    var val =  (PdfString)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_RichMediaCommand_A>("A is required to one of 'array;boolean;integer;number;string-text', was " + utval.Type);
                return;
        }
    }


}

