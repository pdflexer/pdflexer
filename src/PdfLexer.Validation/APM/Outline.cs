// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_Outline : APM_Outline_Base
{
}

internal partial class APM_Outline_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "Outline";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_Outline_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Outline_First, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Outline_Last, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Outline_Count, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_10.Contains(x)))
                {
                    ctx.Fail<APM_Outline>($"Unknown field {extra} for version 1.0");
                }
                break;
            case 1.1m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_11.Contains(x)))
                {
                    ctx.Fail<APM_Outline>($"Unknown field {extra} for version 1.1");
                }
                break;
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_Outline>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_Outline>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_Outline>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_Outline>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_Outline>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_Outline>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_Outline>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_Outline>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_Outline>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_Outline_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_10 { get; } = new List<string> 
    {
        "Type", "First", "Last", "Count"
    };
    public static List<string> AllowedFields_11 { get; } = new List<string> 
    {
        "Type", "First", "Last", "Count"
    };
    public static List<string> AllowedFields_12 { get; } = new List<string> 
    {
        "Type", "First", "Last", "Count"
    };
    public static List<string> AllowedFields_13 { get; } = new List<string> 
    {
        "Type", "First", "Last", "Count"
    };
    public static List<string> AllowedFields_14 { get; } = new List<string> 
    {
        "Type", "First", "Last", "Count"
    };
    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Type", "First", "Last", "Count"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Type", "First", "Last", "Count"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "First", "Last", "Count"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "First", "Last", "Count"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "First", "Last", "Count"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "First", "Last", "Count"
    };
    


}

/// <summary>
/// Outline_Type Table 150
/// </summary>
internal partial class APM_Outline_Type : APM_Outline_Type_Base
{
}


internal partial class APM_Outline_Type_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Outline_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_Outline_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "Outlines")) 
        {
            ctx.Fail<APM_Outline_Type>($"Invalid value {val}, allowed are: [Outlines]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// Outline_First 
/// </summary>
internal partial class APM_Outline_First : APM_Outline_First_Base
{
}


internal partial class APM_Outline_First_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Outline_First";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Outline_First>(obj, "First", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_OutlineItem, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// Outline_Last 
/// </summary>
internal partial class APM_Outline_Last : APM_Outline_Last_Base
{
}


internal partial class APM_Outline_Last_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Outline_Last";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Outline_Last>(obj, "Last", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_OutlineItem, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// Outline_Count 
/// </summary>
internal partial class APM_Outline_Count : APM_Outline_Count_Base
{
}


internal partial class APM_Outline_Count_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Outline_Count";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_Outline_Count>(obj, "Count", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        IPdfObject @Count = val;
        if (!(gte(@Count,0))) 
        {
            ctx.Fail<APM_Outline_Count>($"Invalid value {val}, allowed are: [fn:Eval(@Count>=0)]");
        }
        }
        // no linked objects
        
    }


}

