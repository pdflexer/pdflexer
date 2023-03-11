// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_CollectionSubitem : APM_CollectionSubitem__Base
{
}

internal partial class APM_CollectionSubitem__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "CollectionSubitem";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_CollectionSubitem_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CollectionSubitem_D, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CollectionSubitem_P, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_CollectionSubitem>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_CollectionSubitem>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_CollectionSubitem>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_CollectionSubitem>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_CollectionSubitem_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "D", "P"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "D", "P"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "D", "P"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "D", "P"
    };
    


}

/// <summary>
/// CollectionSubitem_Type Table 47
/// </summary>
internal partial class APM_CollectionSubitem_Type : APM_CollectionSubitem_Type__Base
{
}


internal partial class APM_CollectionSubitem_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionSubitem_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_CollectionSubitem_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.CollectionSubitem)) 
        {
            ctx.Fail<APM_CollectionSubitem_Type>($"Invalid value {val}, allowed are: [CollectionSubitem]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// CollectionSubitem_D 
/// </summary>
internal partial class APM_CollectionSubitem_D : APM_CollectionSubitem_D__Base
{
}


internal partial class APM_CollectionSubitem_D__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionSubitem_D";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_CollectionSubitem_D>(obj, "D", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.StringObj:
                {
            
                    // TODO MC date;string-text
            
                    var val =  (PdfString)utval;
                    if (IsDate(val)) 
                    {
                        // date
                        // no indirect obj reqs
                        // no special cases
                        // no value restrictions
                        // no linked objects
                    } else if (true) 
                    {
                        // string-text
                        // no indirect obj reqs
                        // no special cases
                        // no value restrictions
                        // no linked objects
                    }
                    return;
                }
            case PdfObjectType.NumericObj:
                {
                    var val =  (PdfNumber)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_CollectionSubitem_D>("D is required to one of 'date;number;string-text', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// CollectionSubitem_P 
/// </summary>
internal partial class APM_CollectionSubitem_P : APM_CollectionSubitem_P__Base
{
}


internal partial class APM_CollectionSubitem_P__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionSubitem_P";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_CollectionSubitem_P>(obj, "P", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

