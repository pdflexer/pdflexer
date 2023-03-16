// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_CollectionSplit : APM_CollectionSplit__Base
{
}

internal partial class APM_CollectionSplit__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "CollectionSplit";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_CollectionSplit_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CollectionSplit_Direction, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CollectionSplit_Position, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.7m:
            case 1.8m:
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17_18_19.Contains(x)))
                {
                    ctx.Fail<APM_CollectionSplit>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_CollectionSplit>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_CollectionSplit_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_17_18_19 { get; } = new List<string> 
    {
        "Direction", "Position"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Direction", "Position", "Type"
    };
    


}

/// <summary>
/// CollectionSplit_Type Table 158
/// </summary>
internal partial class APM_CollectionSplit_Type : APM_CollectionSplit_Type__Base
{
}


internal partial class APM_CollectionSplit_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionSplit_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_CollectionSplit_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.CollectionSplit)) 
        {
            ctx.Fail<APM_CollectionSplit_Type>($"Invalid value {val}, allowed are: [CollectionSplit]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// CollectionSplit_Direction 
/// </summary>
internal partial class APM_CollectionSplit_Direction : APM_CollectionSplit_Direction__Base
{
}


internal partial class APM_CollectionSplit_Direction__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionSplit_Direction";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_CollectionSplit_Direction>(obj, "Direction", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.H || val == PdfName.V || val == PdfName.N)) 
        {
            ctx.Fail<APM_CollectionSplit_Direction>($"Invalid value {val}, allowed are: [H,V,N]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// CollectionSplit_Position 
/// </summary>
internal partial class APM_CollectionSplit_Position : APM_CollectionSplit_Position__Base
{
}


internal partial class APM_CollectionSplit_Position__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionSplit_Position";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfNumber, APM_CollectionSplit_Position>(obj, "Position", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:Ignore, not pertinent to validation
        
        var Position = obj.Get("Position");
        if (!((gte(Position,0)&&lte(Position,100)))) 
        {
            ctx.Fail<APM_CollectionSplit_Position>($"Invalid value {val}, allowed are: [fn:Eval((@Position>=0) && (@Position<=100))]");
        }
        // no linked objects
        
    }


}

