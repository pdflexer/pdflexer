// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_Transition : APM_Transition__Base
{
}

internal partial class APM_Transition__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "Transition";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_Transition_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Transition_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Transition_D, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Transition_Dm, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Transition_M, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Transition_Di, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Transition_SS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Transition_B, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.1m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_11.Contains(x)))
                {
                    ctx.Fail<APM_Transition>($"Unknown field {extra} for version 1.1");
                }
                break;
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_Transition>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_Transition>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_Transition>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_Transition>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_Transition>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_Transition>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_Transition>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_Transition>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_Transition>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_Transition_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_11 { get; } = new HashSet<string> 
    {
        "Type", "S", "D", "Dm", "M", "Di"
    };
    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "S", "D", "Dm", "M", "Di"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "S", "D", "Dm", "M", "Di"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "S", "D", "Dm", "M", "Di"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "S", "D", "Dm", "M", "Di", "SS", "B"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "S", "D", "Dm", "M", "Di", "SS", "B"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "S", "D", "Dm", "M", "Di", "SS", "B"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "S", "D", "Dm", "M", "Di", "SS", "B"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "S", "D", "Dm", "M", "Di", "SS", "B"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "S", "D", "Dm", "M", "Di", "SS", "B"
    };
    


}

/// <summary>
/// Transition_Type Table 164
/// </summary>
internal partial class APM_Transition_Type : APM_Transition_Type__Base
{
}


internal partial class APM_Transition_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Transition_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_Transition_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "Trans")) 
        {
            ctx.Fail<APM_Transition_Type>($"Invalid value {val}, allowed are: [Trans]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// Transition_S 
/// </summary>
internal partial class APM_Transition_S : APM_Transition_S__Base
{
}


internal partial class APM_Transition_S__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Transition_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_Transition_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "Split" || val == "Blinds" || val == "Box" || val == "Wipe" || val == "Dissolve" || val == "Glitter" || val == "R" || val == "Fly" || val == "Push" || val == "Cover" || val == "Uncover" || val == "Fade")) 
        {
            ctx.Fail<APM_Transition_S>($"Invalid value {val}, allowed are: [Split,Blinds,Box,Wipe,Dissolve,Glitter,R,Fly,Push,Cover,Uncover,Fade]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// Transition_D 
/// </summary>
internal partial class APM_Transition_D : APM_Transition_D__Base
{
}


internal partial class APM_Transition_D__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Transition_D";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_Transition_D>(obj, "D", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var D = obj.Get("D");
        if (!(gte(D,0))) 
        {
            ctx.Fail<APM_Transition_D>($"Invalid value {val}, allowed are: [fn:Eval(@D>=0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// Transition_Dm 
/// </summary>
internal partial class APM_Transition_Dm : APM_Transition_Dm__Base
{
}


internal partial class APM_Transition_Dm__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Transition_Dm";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_Transition_Dm>(obj, "Dm", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:IsMeaningful, not pertinent to validation
        
        
        if (!(val == "H" || val == "V")) 
        {
            ctx.Fail<APM_Transition_Dm>($"Invalid value {val}, allowed are: [H,V]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// Transition_M 
/// </summary>
internal partial class APM_Transition_M : APM_Transition_M__Base
{
}


internal partial class APM_Transition_M__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Transition_M";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_Transition_M>(obj, "M", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:IsMeaningful, not pertinent to validation
        
        
        if (!(val == "I" || val == "O")) 
        {
            ctx.Fail<APM_Transition_M>($"Invalid value {val}, allowed are: [I,O]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// Transition_Di 
/// </summary>
internal partial class APM_Transition_Di : APM_Transition_Di__Base
{
}


internal partial class APM_Transition_Di__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Transition_Di";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_Transition_Di>(obj, "Di", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.NumericObj:
                {
                    var val =  (PdfIntNumber)utval;
                    // no indirect obj reqs
                    var Di = obj.Get("Di");
                    var S = obj.Get("S");
                    if (!((((eq(Di,90)||eq(Di,180))&&eq(S,"Wipe"))||(eq(Di,315)&&eq(S,"Glitter"))))) 
                    {
                        ctx.Fail<APM_Transition_Di>($"Value failed special case check: fn:Eval((((@Di==90) || (@Di==180)) && (@S==Wipe)) || ((@Di==315) && (@S==Glitter)))");
                    }
                    
                    
                    if (!(val == 0 || val == 90 || val == 180 || val == 270 || val == 315)) 
                    {
                        ctx.Fail<APM_Transition_Di>($"Invalid value {val}, allowed are: [0,90,180,270,315]");
                    }
                    // no linked objects
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    var Di = obj.Get("Di");
                    var S = obj.Get("S");
                    var SS = obj.Get("SS");
                    if (!(eq(Di,"None")&&eq(S,"Fly")&&(ctx.Version < 1.5m || !eq(SS,1.0m)))) 
                    {
                        ctx.Fail<APM_Transition_Di>($"Value failed special case check: fn:Eval(((@Di==None) && (@S==Fly) && fn:SinceVersion(1.5,(@SS!=1.0))))");
                    }
                    
                    
                    if (!(val == "None")) 
                    {
                        ctx.Fail<APM_Transition_Di>($"Invalid value {val}, allowed are: [None]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_Transition_Di>("Di is required to one of 'integer;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// Transition_SS 
/// </summary>
internal partial class APM_Transition_SS : APM_Transition_SS__Base
{
}


internal partial class APM_Transition_SS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Transition_SS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_Transition_SS>(obj, "SS", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:IsMeaningful, not pertinent to validation
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Transition_B 
/// </summary>
internal partial class APM_Transition_B : APM_Transition_B__Base
{
}


internal partial class APM_Transition_B__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Transition_B";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_Transition_B>(obj, "B", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:IsMeaningful, not pertinent to validation
        // no value restrictions
        // no linked objects
        
    }


}

