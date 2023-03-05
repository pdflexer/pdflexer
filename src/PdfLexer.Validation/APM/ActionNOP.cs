// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_ActionNOP : APM_ActionNOP__Base
{
}

internal partial class APM_ActionNOP__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "ActionNOP";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_ActionNOP_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ActionNOP_S, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
        
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_ActionNOP_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    


}

/// <summary>
/// ActionNOP_Type Table 196 and Adobe PDF 1.2
/// </summary>
internal partial class APM_ActionNOP_Type : APM_ActionNOP_Type__Base
{
}


internal partial class APM_ActionNOP_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionNOP_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_ActionNOP_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "Action")) 
        {
            ctx.Fail<APM_ActionNOP_Type>($"Invalid value {val}, allowed are: [Action]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ActionNOP_S only documented in Adobe PDF 1.2
/// </summary>
internal partial class APM_ActionNOP_S : APM_ActionNOP_S__Base
{
}


internal partial class APM_ActionNOP_S__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ActionNOP_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_ActionNOP_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "NOP")) 
        {
            ctx.Fail<APM_ActionNOP_S>($"Invalid value {val}, allowed are: [NOP]");
        }
        // no linked objects
        
    }


}

