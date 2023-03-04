// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_3DMeasurePD3 : APM_3DMeasurePD3_Base
{
}

internal partial class APM_3DMeasurePD3_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "3DMeasurePD3";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_3DMeasurePD3_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasurePD3_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasurePD3_TRL, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasurePD3_AP, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasurePD3_A1, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasurePD3_N1, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasurePD3_A2, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasurePD3_N2, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasurePD3_D1, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasurePD3_TP, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasurePD3_TY, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasurePD3_TS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasurePD3_C, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasurePD3_V, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasurePD3_U, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasurePD3_P, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasurePD3_UT, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_3DMeasurePD3_S, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
        
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_3DMeasurePD3_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    


}

/// <summary>
/// 3DMeasurePD3_Type Table 326 and Table 328 perpendicular measurement
/// </summary>
internal partial class APM_3DMeasurePD3_Type : APM_3DMeasurePD3_Type_Base
{
}


internal partial class APM_3DMeasurePD3_Type_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasurePD3_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_3DMeasurePD3_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "3DMeasure")) 
        {
            ctx.Fail<APM_3DMeasurePD3_Type>($"Invalid value {val}, allowed are: [3DMeasure]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// 3DMeasurePD3_Subtype 
/// </summary>
internal partial class APM_3DMeasurePD3_Subtype : APM_3DMeasurePD3_Subtype_Base
{
}


internal partial class APM_3DMeasurePD3_Subtype_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasurePD3_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_3DMeasurePD3_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "PD3")) 
        {
            ctx.Fail<APM_3DMeasurePD3_Subtype>($"Invalid value {val}, allowed are: [PD3]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// 3DMeasurePD3_TRL 
/// </summary>
internal partial class APM_3DMeasurePD3_TRL : APM_3DMeasurePD3_TRL_Base
{
}


internal partial class APM_3DMeasurePD3_TRL_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasurePD3_TRL";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_3DMeasurePD3_TRL>(obj, "TRL", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// 3DMeasurePD3_AP 
/// </summary>
internal partial class APM_3DMeasurePD3_AP : APM_3DMeasurePD3_AP_Base
{
}


internal partial class APM_3DMeasurePD3_AP_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasurePD3_AP";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_3DMeasurePD3_AP>(obj, "AP", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_3Numbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DMeasurePD3_A1 
/// </summary>
internal partial class APM_3DMeasurePD3_A1 : APM_3DMeasurePD3_A1_Base
{
}


internal partial class APM_3DMeasurePD3_A1_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasurePD3_A1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_3DMeasurePD3_A1>(obj, "A1", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_3Numbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DMeasurePD3_N1 
/// </summary>
internal partial class APM_3DMeasurePD3_N1 : APM_3DMeasurePD3_N1_Base
{
}


internal partial class APM_3DMeasurePD3_N1_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasurePD3_N1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_3DMeasurePD3_N1>(obj, "N1", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// 3DMeasurePD3_A2 
/// </summary>
internal partial class APM_3DMeasurePD3_A2 : APM_3DMeasurePD3_A2_Base
{
}


internal partial class APM_3DMeasurePD3_A2_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasurePD3_A2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_3DMeasurePD3_A2>(obj, "A2", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_3Numbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DMeasurePD3_N2 
/// </summary>
internal partial class APM_3DMeasurePD3_N2 : APM_3DMeasurePD3_N2_Base
{
}


internal partial class APM_3DMeasurePD3_N2_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasurePD3_N2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_3DMeasurePD3_N2>(obj, "N2", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// 3DMeasurePD3_D1 
/// </summary>
internal partial class APM_3DMeasurePD3_D1 : APM_3DMeasurePD3_D1_Base
{
}


internal partial class APM_3DMeasurePD3_D1_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasurePD3_D1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_3DMeasurePD3_D1>(obj, "D1", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_3Numbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DMeasurePD3_TP 
/// </summary>
internal partial class APM_3DMeasurePD3_TP : APM_3DMeasurePD3_TP_Base
{
}


internal partial class APM_3DMeasurePD3_TP_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasurePD3_TP";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_3DMeasurePD3_TP>(obj, "TP", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_3Numbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DMeasurePD3_TY 
/// </summary>
internal partial class APM_3DMeasurePD3_TY : APM_3DMeasurePD3_TY_Base
{
}


internal partial class APM_3DMeasurePD3_TY_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasurePD3_TY";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_3DMeasurePD3_TY>(obj, "TY", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_3Numbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DMeasurePD3_TS text point size
/// </summary>
internal partial class APM_3DMeasurePD3_TS : APM_3DMeasurePD3_TS_Base
{
}


internal partial class APM_3DMeasurePD3_TS_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasurePD3_TS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_3DMeasurePD3_TS>(obj, "TS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// 3DMeasurePD3_C 
/// </summary>
internal partial class APM_3DMeasurePD3_C : APM_3DMeasurePD3_C_Base
{
}


internal partial class APM_3DMeasurePD3_C_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasurePD3_C";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_3DMeasurePD3_C>(obj, "C", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_3RGBNumbers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// 3DMeasurePD3_V 
/// </summary>
internal partial class APM_3DMeasurePD3_V : APM_3DMeasurePD3_V_Base
{
}


internal partial class APM_3DMeasurePD3_V_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasurePD3_V";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_3DMeasurePD3_V>(obj, "V", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// 3DMeasurePD3_U 
/// </summary>
internal partial class APM_3DMeasurePD3_U : APM_3DMeasurePD3_U_Base
{
}


internal partial class APM_3DMeasurePD3_U_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasurePD3_U";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfString, APM_3DMeasurePD3_U>(obj, "U", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// 3DMeasurePD3_P precision digits
/// </summary>
internal partial class APM_3DMeasurePD3_P : APM_3DMeasurePD3_P_Base
{
}


internal partial class APM_3DMeasurePD3_P_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasurePD3_P";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_3DMeasurePD3_P>(obj, "P", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        IPdfObject @P = val;
        if (!(gte(@P,0))) 
        {
            ctx.Fail<APM_3DMeasurePD3_P>($"Invalid value {val}, allowed are: [fn:Eval(@P>=0)]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// 3DMeasurePD3_UT 
/// </summary>
internal partial class APM_3DMeasurePD3_UT : APM_3DMeasurePD3_UT_Base
{
}


internal partial class APM_3DMeasurePD3_UT_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasurePD3_UT";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_3DMeasurePD3_UT>(obj, "UT", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// 3DMeasurePD3_S 
/// </summary>
internal partial class APM_3DMeasurePD3_S : APM_3DMeasurePD3_S_Base
{
}


internal partial class APM_3DMeasurePD3_S_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "3DMeasurePD3_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_3DMeasurePD3_S>(obj, "S", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_AnnotProjection, PdfDictionary>(stack, val, obj);
        
    }


}

