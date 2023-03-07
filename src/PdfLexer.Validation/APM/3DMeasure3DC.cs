// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_3DMeasure3DC : APM_3DMeasure3DC__Base
{
}

internal partial class APM_3DMeasure3DC__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "3DMeasure3DC";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_3DMeasure3DC_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasure3DC_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasure3DC_TRL, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasure3DC_A1, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasure3DC_N1, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasure3DC_TP, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasure3DC_TB, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasure3DC_TS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasure3DC_C, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasure3DC_UT, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasure3DC_S, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_3DMeasure3DC>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_3DMeasure3DC>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_3DMeasure3DC>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_3DMeasure3DC>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_3DMeasure3DC_Type, PdfDictionary>(new CallStack(), obj, null);
        c.Run<APM_3DMeasure3DC_Subtype, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "TRL", "A1", "N1", "TP", "TB", "TS", "C", "UT", "S"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "TRL", "A1", "N1", "TP", "TB", "TS", "C", "UT", "S"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "TRL", "A1", "N1", "TP", "TB", "TS", "C", "UT", "S"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "TRL", "A1", "N1", "TP", "TB", "TS", "C", "UT", "S"
    };
    


}

/// <summary>
/// 3DMeasure3DC_Type Table 326 and Table 331 3D comment note
/// </summary>
internal partial class APM_3DMeasure3DC_Type : APM_3DMeasure3DC_Type__Base
{
}


internal partial class APM_3DMeasure3DC_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasure3DC_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_3DMeasure3DC_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.N3DMeasure)) 
        {
            ctx.Fail<APM_3DMeasure3DC_Type>($"Invalid value {val}, allowed are: [3DMeasure]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// 3DMeasure3DC_Subtype 
/// </summary>
internal partial class APM_3DMeasure3DC_Subtype : APM_3DMeasure3DC_Subtype__Base
{
}


internal partial class APM_3DMeasure3DC_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasure3DC_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_3DMeasure3DC_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.N3DC)) 
        {
            ctx.Fail<APM_3DMeasure3DC_Subtype>($"Invalid value {val}, allowed are: [3DC]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// 3DMeasure3DC_TRL 
/// </summary>
internal partial class APM_3DMeasure3DC_TRL : APM_3DMeasure3DC_TRL__Base
{
}


internal partial class APM_3DMeasure3DC_TRL__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasure3DC_TRL";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_3DMeasure3DC_TRL>(obj, "TRL", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// 3DMeasure3DC_A1 
/// </summary>
internal partial class APM_3DMeasure3DC_A1 : APM_3DMeasure3DC_A1__Base
{
}


internal partial class APM_3DMeasure3DC_A1__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasure3DC_A1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_3DMeasure3DC_A1>(obj, "A1", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_3Numbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DMeasure3DC_N1 
/// </summary>
internal partial class APM_3DMeasure3DC_N1 : APM_3DMeasure3DC_N1__Base
{
}


internal partial class APM_3DMeasure3DC_N1__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasure3DC_N1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_3DMeasure3DC_N1>(obj, "N1", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// 3DMeasure3DC_TP 
/// </summary>
internal partial class APM_3DMeasure3DC_TP : APM_3DMeasure3DC_TP__Base
{
}


internal partial class APM_3DMeasure3DC_TP__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasure3DC_TP";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_3DMeasure3DC_TP>(obj, "TP", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_3Numbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DMeasure3DC_TB 
/// </summary>
internal partial class APM_3DMeasure3DC_TB : APM_3DMeasure3DC_TB__Base
{
}


internal partial class APM_3DMeasure3DC_TB__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasure3DC_TB";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_3DMeasure3DC_TB>(obj, "TB", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_2Integers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DMeasure3DC_TS text point size
/// </summary>
internal partial class APM_3DMeasure3DC_TS : APM_3DMeasure3DC_TS__Base
{
}


internal partial class APM_3DMeasure3DC_TS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasure3DC_TS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_3DMeasure3DC_TS>(obj, "TS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// 3DMeasure3DC_C 
/// </summary>
internal partial class APM_3DMeasure3DC_C : APM_3DMeasure3DC_C__Base
{
}


internal partial class APM_3DMeasure3DC_C__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasure3DC_C";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_3DMeasure3DC_C>(obj, "C", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_3RGBNumbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DMeasure3DC_UT 
/// </summary>
internal partial class APM_3DMeasure3DC_UT : APM_3DMeasure3DC_UT__Base
{
}


internal partial class APM_3DMeasure3DC_UT__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasure3DC_UT";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_3DMeasure3DC_UT>(obj, "UT", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// 3DMeasure3DC_S 
/// </summary>
internal partial class APM_3DMeasure3DC_S : APM_3DMeasure3DC_S__Base
{
}


internal partial class APM_3DMeasure3DC_S__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasure3DC_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_3DMeasure3DC_S>(obj, "S", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_AnnotProjection, PdfDictionary>(stack, val, obj);
        
    }


}

