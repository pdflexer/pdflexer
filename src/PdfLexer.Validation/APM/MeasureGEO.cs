// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_MeasureGEO : APM_MeasureGEO__Base
{
}

internal partial class APM_MeasureGEO__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "MeasureGEO";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_MeasureGEO_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MeasureGEO_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MeasureGEO_Bounds, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MeasureGEO_GCS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MeasureGEO_DCS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MeasureGEO_PDU, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MeasureGEO_GPTS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MeasureGEO_LPTS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MeasureGEO_PCSM, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_MeasureGEO>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_MeasureGEO_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "Subtype", "PCSM"
    };
    


}

/// <summary>
/// MeasureGEO_Type Table 266 and Table 269
/// </summary>
internal partial class APM_MeasureGEO_Type : APM_MeasureGEO_Type__Base
{
}


internal partial class APM_MeasureGEO_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MeasureGEO_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_MeasureGEO_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "Measure")) 
        {
            ctx.Fail<APM_MeasureGEO_Type>($"Invalid value {val}, allowed are: [Measure]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// MeasureGEO_Subtype 
/// </summary>
internal partial class APM_MeasureGEO_Subtype : APM_MeasureGEO_Subtype__Base
{
}


internal partial class APM_MeasureGEO_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MeasureGEO_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_MeasureGEO_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "GEO")) 
        {
            ctx.Fail<APM_MeasureGEO_Subtype>($"Invalid value {val}, allowed are: [GEO]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// MeasureGEO_Bounds 
/// </summary>
internal partial class APM_MeasureGEO_Bounds : APM_MeasureGEO_Bounds__Base
{
}


internal partial class APM_MeasureGEO_Bounds__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MeasureGEO_Bounds";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_MeasureGEO_Bounds>(obj, "Bounds", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfNumbersGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// MeasureGEO_GCS 
/// </summary>
internal partial class APM_MeasureGEO_GCS : APM_MeasureGEO_GCS__Base
{
}


internal partial class APM_MeasureGEO_GCS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MeasureGEO_GCS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_MeasureGEO_GCS>(obj, "GCS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_GeographicCoordinateSystem.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_GeographicCoordinateSystem, PdfDictionary>(stack, val, obj);
        } else if (APM_ProjectedCoordinateSystem.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_ProjectedCoordinateSystem, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_MeasureGEO_GCS>("GCS did not match any allowable types: '[GeographicCoordinateSystem,ProjectedCoordinateSystem]'");
        }
        
    }


}

/// <summary>
/// MeasureGEO_DCS 
/// </summary>
internal partial class APM_MeasureGEO_DCS : APM_MeasureGEO_DCS__Base
{
}


internal partial class APM_MeasureGEO_DCS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MeasureGEO_DCS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_MeasureGEO_DCS>(obj, "DCS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_GeographicCoordinateSystem.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_GeographicCoordinateSystem, PdfDictionary>(stack, val, obj);
        } else if (APM_ProjectedCoordinateSystem.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_ProjectedCoordinateSystem, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_MeasureGEO_DCS>("DCS did not match any allowable types: '[GeographicCoordinateSystem,ProjectedCoordinateSystem]'");
        }
        
    }


}

/// <summary>
/// MeasureGEO_PDU Preferred Display Units
/// </summary>
internal partial class APM_MeasureGEO_PDU : APM_MeasureGEO_PDU__Base
{
}


internal partial class APM_MeasureGEO_PDU__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MeasureGEO_PDU";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_MeasureGEO_PDU>(obj, "PDU", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf3PDUNames, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// MeasureGEO_GPTS 
/// </summary>
internal partial class APM_MeasureGEO_GPTS : APM_MeasureGEO_GPTS__Base
{
}


internal partial class APM_MeasureGEO_GPTS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MeasureGEO_GPTS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_MeasureGEO_GPTS>(obj, "GPTS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfNumbersGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// MeasureGEO_LPTS 
/// </summary>
internal partial class APM_MeasureGEO_LPTS : APM_MeasureGEO_LPTS__Base
{
}


internal partial class APM_MeasureGEO_LPTS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MeasureGEO_LPTS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_MeasureGEO_LPTS>(obj, "LPTS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfNumbersGeneral, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// MeasureGEO_PCSM Projected Coordinate System Matrix
/// </summary>
internal partial class APM_MeasureGEO_PCSM : APM_MeasureGEO_PCSM__Base
{
}


internal partial class APM_MeasureGEO_PCSM__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MeasureGEO_PCSM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_MeasureGEO_PCSM>(obj, "PCSM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf3DTransMatrix, PdfArray>(stack, val, obj);
        
    }


}

