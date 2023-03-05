// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_NumberFormat : APM_NumberFormat__Base
{
}

internal partial class APM_NumberFormat__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "NumberFormat";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_NumberFormat_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_NumberFormat_U, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_NumberFormat_C, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_NumberFormat_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_NumberFormat_D, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_NumberFormat_FD, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_NumberFormat_RT, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_NumberFormat_RD, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_NumberFormat_PS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_NumberFormat_SS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_NumberFormat_O, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_NumberFormat>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_NumberFormat>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_NumberFormat>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_NumberFormat>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_NumberFormat>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_NumberFormat_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "U", "C", "F", "D", "FD", "RT", "RD", "PS", "SS", "O"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "U", "C", "F", "D", "FD", "RT", "RD", "PS", "SS", "O"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "U", "C", "F", "D", "FD", "RT", "RD", "PS", "SS", "O"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "U", "C", "F", "D", "FD", "RT", "RD", "PS", "SS", "O"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "U", "C", "F", "D", "FD", "RT", "RD", "PS", "SS", "O"
    };
    


}

/// <summary>
/// NumberFormat_Type Table 268
/// </summary>
internal partial class APM_NumberFormat_Type : APM_NumberFormat_Type__Base
{
}


internal partial class APM_NumberFormat_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "NumberFormat_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_NumberFormat_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "NumberFormat")) 
        {
            ctx.Fail<APM_NumberFormat_Type>($"Invalid value {val}, allowed are: [NumberFormat]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// NumberFormat_U 
/// </summary>
internal partial class APM_NumberFormat_U : APM_NumberFormat_U__Base
{
}


internal partial class APM_NumberFormat_U__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "NumberFormat_U";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfString, APM_NumberFormat_U>(obj, "U", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// NumberFormat_C 
/// </summary>
internal partial class APM_NumberFormat_C : APM_NumberFormat_C__Base
{
}


internal partial class APM_NumberFormat_C__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "NumberFormat_C";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_NumberFormat_C>(obj, "C", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// NumberFormat_F 
/// </summary>
internal partial class APM_NumberFormat_F : APM_NumberFormat_F__Base
{
}


internal partial class APM_NumberFormat_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "NumberFormat_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_NumberFormat_F>(obj, "F", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:IsMeaningful, not pertinent to validation
        
        
        if (!(val == "D" || val == "F" || val == "R" || val == "T")) 
        {
            ctx.Fail<APM_NumberFormat_F>($"Invalid value {val}, allowed are: [D,F,R,T]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// NumberFormat_D 
/// </summary>
internal partial class APM_NumberFormat_D : APM_NumberFormat_D__Base
{
}


internal partial class APM_NumberFormat_D__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "NumberFormat_D";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_NumberFormat_D>(obj, "D", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:IsMeaningful, not pertinent to validation
        
        var D = obj.Get("D");
        if (!(gt(D,0))) 
        {
            ctx.Fail<APM_NumberFormat_D>($"Invalid value {val}, allowed are: [fn:Eval(@D>0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// NumberFormat_FD 
/// </summary>
internal partial class APM_NumberFormat_FD : APM_NumberFormat_FD__Base
{
}


internal partial class APM_NumberFormat_FD__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "NumberFormat_FD";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_NumberFormat_FD>(obj, "FD", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:IsMeaningful, not pertinent to validation
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// NumberFormat_RT 
/// </summary>
internal partial class APM_NumberFormat_RT : APM_NumberFormat_RT__Base
{
}


internal partial class APM_NumberFormat_RT__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "NumberFormat_RT";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_NumberFormat_RT>(obj, "RT", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// NumberFormat_RD 
/// </summary>
internal partial class APM_NumberFormat_RD : APM_NumberFormat_RD__Base
{
}


internal partial class APM_NumberFormat_RD__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "NumberFormat_RD";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_NumberFormat_RD>(obj, "RD", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// NumberFormat_PS 
/// </summary>
internal partial class APM_NumberFormat_PS : APM_NumberFormat_PS__Base
{
}


internal partial class APM_NumberFormat_PS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "NumberFormat_PS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_NumberFormat_PS>(obj, "PS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// NumberFormat_SS 
/// </summary>
internal partial class APM_NumberFormat_SS : APM_NumberFormat_SS__Base
{
}


internal partial class APM_NumberFormat_SS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "NumberFormat_SS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_NumberFormat_SS>(obj, "SS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// NumberFormat_O 
/// </summary>
internal partial class APM_NumberFormat_O : APM_NumberFormat_O__Base
{
}


internal partial class APM_NumberFormat_O__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "NumberFormat_O";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_NumberFormat_O>(obj, "O", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "S" || val == "P")) 
        {
            ctx.Fail<APM_NumberFormat_O>($"Invalid value {val}, allowed are: [S,P]");
        }
        // no linked objects
        
    }


}

