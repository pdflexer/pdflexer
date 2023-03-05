// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_GeographicCoordinateSystem : APM_GeographicCoordinateSystem__Base
{
}

internal partial class APM_GeographicCoordinateSystem__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "GeographicCoordinateSystem";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_GeographicCoordinateSystem_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_GeographicCoordinateSystem_EPSG, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_GeographicCoordinateSystem_WKT, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
        
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_GeographicCoordinateSystem_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    


}

/// <summary>
/// GeographicCoordinateSystem_Type Table 270
/// </summary>
internal partial class APM_GeographicCoordinateSystem_Type : APM_GeographicCoordinateSystem_Type__Base
{
}


internal partial class APM_GeographicCoordinateSystem_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "GeographicCoordinateSystem_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_GeographicCoordinateSystem_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "GEOGCS")) 
        {
            ctx.Fail<APM_GeographicCoordinateSystem_Type>($"Invalid value {val}, allowed are: [GEOGCS]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// GeographicCoordinateSystem_EPSG 
/// </summary>
internal partial class APM_GeographicCoordinateSystem_EPSG : APM_GeographicCoordinateSystem_EPSG__Base
{
}


internal partial class APM_GeographicCoordinateSystem_EPSG__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "GeographicCoordinateSystem_EPSG";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_GeographicCoordinateSystem_EPSG>(obj, "EPSG", IndirectRequirement.Either);
        if (val == null) { return; }
        
        if (!(!obj.ContainsKey("WKT"))) 
        {
            ctx.Fail<APM_GeographicCoordinateSystem_EPSG>($"Value failed special case check: fn:Eval(fn:Not(fn:IsPresent(WKT)))");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// GeographicCoordinateSystem_WKT 
/// </summary>
internal partial class APM_GeographicCoordinateSystem_WKT : APM_GeographicCoordinateSystem_WKT__Base
{
}


internal partial class APM_GeographicCoordinateSystem_WKT__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "GeographicCoordinateSystem_WKT";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_GeographicCoordinateSystem_WKT>(obj, "WKT", IndirectRequirement.Either);
        if (val == null) { return; }
        
        if (!(!obj.ContainsKey("EPSG"))) 
        {
            ctx.Fail<APM_GeographicCoordinateSystem_WKT>($"Value failed special case check: fn:Eval(fn:Not(fn:IsPresent(EPSG)))");
        }
        // no value restrictions
        // no linked objects
        
    }


}

