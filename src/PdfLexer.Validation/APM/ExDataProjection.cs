// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_ExDataProjection : APM_ExDataProjection__Base
{
}

internal partial class APM_ExDataProjection__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "ExDataProjection";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_ExDataProjection_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ExDataProjection_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_ExDataProjection_M3DREF, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_ExDataProjection>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_ExDataProjection>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_ExDataProjection>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_ExDataProjection>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_ExDataProjection_Type, PdfDictionary>(new CallStack(), obj, null);
        c.Run<APM_ExDataProjection_Subtype, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "Subtype", "M3DREF"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "Subtype", "M3DREF"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "Subtype", "M3DREF"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "Subtype", "M3DREF"
    };
    


}

/// <summary>
/// ExDataProjection_Type Table 332
/// </summary>
internal partial class APM_ExDataProjection_Type : APM_ExDataProjection_Type__Base
{
}


internal partial class APM_ExDataProjection_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ExDataProjection_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_ExDataProjection_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.ExData)) 
        {
            ctx.Fail<APM_ExDataProjection_Type>($"Invalid value {val}, allowed are: [ExData]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ExDataProjection_Subtype 
/// </summary>
internal partial class APM_ExDataProjection_Subtype : APM_ExDataProjection_Subtype__Base
{
}


internal partial class APM_ExDataProjection_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ExDataProjection_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_ExDataProjection_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.N3DM)) 
        {
            ctx.Fail<APM_ExDataProjection_Subtype>($"Invalid value {val}, allowed are: [3DM]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// ExDataProjection_M3DREF 
/// </summary>
internal partial class APM_ExDataProjection_M3DREF : APM_ExDataProjection_M3DREF__Base
{
}


internal partial class APM_ExDataProjection_M3DREF__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "ExDataProjection_M3DREF";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_ExDataProjection_M3DREF>(obj, "M3DREF", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_3DMeasure3DC.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_3DMeasure3DC, PdfDictionary>(stack, val, obj);
        } else if (APM_3DMeasureAD3.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_3DMeasureAD3, PdfDictionary>(stack, val, obj);
        } else if (APM_3DMeasureLD3.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_3DMeasureLD3, PdfDictionary>(stack, val, obj);
        } else if (APM_3DMeasurePD3.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_3DMeasurePD3, PdfDictionary>(stack, val, obj);
        } else if (APM_3DMeasureRD3.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_3DMeasureRD3, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_ExDataProjection_M3DREF>("M3DREF did not match any allowable types: '[3DMeasure3DC,3DMeasureAD3,3DMeasureLD3,3DMeasurePD3,3DMeasureRD3]'");
        }
        
    }


}
