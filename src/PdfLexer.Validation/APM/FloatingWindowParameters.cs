// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_FloatingWindowParameters : APM_FloatingWindowParameters_Base
{
}

internal partial class APM_FloatingWindowParameters_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "FloatingWindowParameters";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_FloatingWindowParameters_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FloatingWindowParameters_D, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FloatingWindowParameters_RT, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FloatingWindowParameters_P, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FloatingWindowParameters_O, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FloatingWindowParameters_T, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FloatingWindowParameters_UC, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FloatingWindowParameters_R, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FloatingWindowParameters_TT, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_FloatingWindowParameters>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_FloatingWindowParameters>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_FloatingWindowParameters>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_FloatingWindowParameters>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_FloatingWindowParameters>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_FloatingWindowParameters>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_FloatingWindowParameters_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "D", "RT", "P", "O", "T", "UC", "R", "TT"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "D", "RT", "P", "O", "T", "UC", "R", "TT"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "D", "RT", "P", "O", "T", "UC", "R", "TT"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "D", "RT", "P", "O", "T", "UC", "R", "TT"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "D", "RT", "P", "O", "T", "UC", "R", "TT"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "D", "RT", "P", "O", "T", "UC", "R", "TT"
    };
    


}

/// <summary>
/// FloatingWindowParameters_Type Table 295
/// </summary>
internal partial class APM_FloatingWindowParameters_Type : APM_FloatingWindowParameters_Type_Base
{
}


internal partial class APM_FloatingWindowParameters_Type_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FloatingWindowParameters_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_FloatingWindowParameters_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "FWParams")) 
        {
            ctx.Fail<APM_FloatingWindowParameters_Type>($"Invalid value {val}, allowed are: [FWParams]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// FloatingWindowParameters_D 
/// </summary>
internal partial class APM_FloatingWindowParameters_D : APM_FloatingWindowParameters_D_Base
{
}


internal partial class APM_FloatingWindowParameters_D_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FloatingWindowParameters_D";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_FloatingWindowParameters_D>(obj, "D", IndirectRequirement.Either);
        if (val == null) { return; }
        // TODO special case
        // no value restrictions
        ctx.Run<APM_ArrayOf_2Integers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// FloatingWindowParameters_RT 
/// </summary>
internal partial class APM_FloatingWindowParameters_RT : APM_FloatingWindowParameters_RT_Base
{
}


internal partial class APM_FloatingWindowParameters_RT_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FloatingWindowParameters_RT";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FloatingWindowParameters_RT>(obj, "RT", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == 0 || val == 1 || val == 2 || val == 3)) 
        {
            ctx.Fail<APM_FloatingWindowParameters_RT>($"Invalid value {val}, allowed are: [0,1,2,3]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// FloatingWindowParameters_P 
/// </summary>
internal partial class APM_FloatingWindowParameters_P : APM_FloatingWindowParameters_P_Base
{
}


internal partial class APM_FloatingWindowParameters_P_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FloatingWindowParameters_P";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FloatingWindowParameters_P>(obj, "P", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == 0 || val == 1 || val == 2 || val == 3 || val == 4 || val == 5 || val == 6 || val == 7 || val == 8)) 
        {
            ctx.Fail<APM_FloatingWindowParameters_P>($"Invalid value {val}, allowed are: [0,1,2,3,4,5,6,7,8]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// FloatingWindowParameters_O 
/// </summary>
internal partial class APM_FloatingWindowParameters_O : APM_FloatingWindowParameters_O_Base
{
}


internal partial class APM_FloatingWindowParameters_O_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FloatingWindowParameters_O";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FloatingWindowParameters_O>(obj, "O", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == 0 || val == 1 || val == 2)) 
        {
            ctx.Fail<APM_FloatingWindowParameters_O>($"Invalid value {val}, allowed are: [0,1,2]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// FloatingWindowParameters_T 
/// </summary>
internal partial class APM_FloatingWindowParameters_T : APM_FloatingWindowParameters_T_Base
{
}


internal partial class APM_FloatingWindowParameters_T_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FloatingWindowParameters_T";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_FloatingWindowParameters_T>(obj, "T", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FloatingWindowParameters_UC 
/// </summary>
internal partial class APM_FloatingWindowParameters_UC : APM_FloatingWindowParameters_UC_Base
{
}


internal partial class APM_FloatingWindowParameters_UC_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FloatingWindowParameters_UC";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_FloatingWindowParameters_UC>(obj, "UC", IndirectRequirement.Either);
        if (val == null) { return; }
        // TODO special case
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FloatingWindowParameters_R 
/// </summary>
internal partial class APM_FloatingWindowParameters_R : APM_FloatingWindowParameters_R_Base
{
}


internal partial class APM_FloatingWindowParameters_R_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FloatingWindowParameters_R";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_FloatingWindowParameters_R>(obj, "R", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == 0 || val == 1 || val == 2)) 
        {
            ctx.Fail<APM_FloatingWindowParameters_R>($"Invalid value {val}, allowed are: [0,1,2]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// FloatingWindowParameters_TT 
/// </summary>
internal partial class APM_FloatingWindowParameters_TT : APM_FloatingWindowParameters_TT_Base
{
}


internal partial class APM_FloatingWindowParameters_TT_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FloatingWindowParameters_TT";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_FloatingWindowParameters_TT>(obj, "TT", IndirectRequirement.Either);
        if (val == null) { return; }
        // TODO special case
        // no value restrictions
        ctx.Run<APM_ArrayOfStringsText, PdfArray>(stack, val, obj);
        
    }


}

