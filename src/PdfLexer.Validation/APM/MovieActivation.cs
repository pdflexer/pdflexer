// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_MovieActivation : APM_MovieActivation__Base
{
}

internal partial class APM_MovieActivation__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "MovieActivation";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_MovieActivation_Start, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MovieActivation_Duration, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MovieActivation_Rate, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MovieActivation_Volume, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MovieActivation_ShowControls, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MovieActivation_Mode, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MovieActivation_Synchronous, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MovieActivation_FWScale, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_MovieActivation_FWPosition, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_MovieActivation>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_MovieActivation>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_MovieActivation>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_MovieActivation>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_MovieActivation>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_MovieActivation>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_MovieActivation>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_MovieActivation>($"Unknown field {extra} for version 1.9");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        return false;
    }

    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Start", "Duration", "Rate", "Volume", "ShowControls", "Mode", "Synchronous", "FWScale", "FWPosition"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Start", "Duration", "Rate", "Volume", "ShowControls", "Mode", "Synchronous", "FWScale", "FWPosition"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Start", "Duration", "Rate", "Volume", "ShowControls", "Mode", "Synchronous", "FWScale", "FWPosition"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Start", "Duration", "Rate", "Volume", "ShowControls", "Mode", "Synchronous", "FWScale", "FWPosition"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Start", "Duration", "Rate", "Volume", "ShowControls", "Mode", "Synchronous", "FWScale", "FWPosition"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Start", "Duration", "Rate", "Volume", "ShowControls", "Mode", "Synchronous", "FWScale", "FWPosition"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Start", "Duration", "Rate", "Volume", "ShowControls", "Mode", "Synchronous", "FWScale", "FWPosition"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Start", "Duration", "Rate", "Volume", "ShowControls", "Mode", "Synchronous", "FWScale", "FWPosition"
    };
    


}

/// <summary>
/// MovieActivation_Start Table 307
/// </summary>
internal partial class APM_MovieActivation_Start : APM_MovieActivation_Start__Base
{
}


internal partial class APM_MovieActivation_Start__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MovieActivation_Start";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_MovieActivation_Start>(obj, "Start", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_ArrayOfDuration, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NumericObj:
                {
                    var val =  (PdfIntNumber)utval;
                    // no indirect obj reqs
                    var Start = obj.Get("Start");
                    if (!(gt(Start,0))) 
                    {
                        ctx.Fail<APM_MovieActivation_Start>($"Value failed special case check: fn:Eval(@Start>0)");
                    }
                    // no value restrictions
                    // no linked objects
                    return;
                }
            case PdfObjectType.StringObj:
                {
                    var val =  (PdfString)utval;
                    // no indirect obj reqs
                    var Start = obj.Get("Start");
                    if (!(eq(StringLength(Start),8))) 
                    {
                        ctx.Fail<APM_MovieActivation_Start>($"Value failed special case check: fn:Eval(fn:StringLength(Start)==8)");
                    }
                    // no value restrictions
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_MovieActivation_Start>("Start is required to one of 'array;integer;string-byte', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// MovieActivation_Duration 
/// </summary>
internal partial class APM_MovieActivation_Duration : APM_MovieActivation_Duration__Base
{
}


internal partial class APM_MovieActivation_Duration__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MovieActivation_Duration";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_MovieActivation_Duration>(obj, "Duration", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_ArrayOfDuration, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NumericObj:
                {
                    var val =  (PdfIntNumber)utval;
                    // no indirect obj reqs
                    var Duration = obj.Get("Duration");
                    if (!(gt(Duration,0))) 
                    {
                        ctx.Fail<APM_MovieActivation_Duration>($"Value failed special case check: fn:Eval(@Duration>0)");
                    }
                    // no value restrictions
                    // no linked objects
                    return;
                }
            case PdfObjectType.StringObj:
                {
                    var val =  (PdfString)utval;
                    // no indirect obj reqs
                    var Duration = obj.Get("Duration");
                    if (!(eq(StringLength(Duration),8))) 
                    {
                        ctx.Fail<APM_MovieActivation_Duration>($"Value failed special case check: fn:Eval(fn:StringLength(Duration)==8)");
                    }
                    // no value restrictions
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_MovieActivation_Duration>("Duration is required to one of 'array;integer;string-byte', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// MovieActivation_Rate negative means play backwards
/// </summary>
internal partial class APM_MovieActivation_Rate : APM_MovieActivation_Rate__Base
{
}


internal partial class APM_MovieActivation_Rate__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MovieActivation_Rate";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_MovieActivation_Rate>(obj, "Rate", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// MovieActivation_Volume 
/// </summary>
internal partial class APM_MovieActivation_Volume : APM_MovieActivation_Volume__Base
{
}


internal partial class APM_MovieActivation_Volume__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MovieActivation_Volume";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_MovieActivation_Volume>(obj, "Volume", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var Volume = obj.Get("Volume");
        if (!((gte(Volume,-1)&&lte(Volume,1)))) 
        {
            ctx.Fail<APM_MovieActivation_Volume>($"Invalid value {val}, allowed are: [fn:Eval((@Volume>=-1) && (@Volume<=1))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// MovieActivation_ShowControls 
/// </summary>
internal partial class APM_MovieActivation_ShowControls : APM_MovieActivation_ShowControls__Base
{
}


internal partial class APM_MovieActivation_ShowControls__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MovieActivation_ShowControls";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_MovieActivation_ShowControls>(obj, "ShowControls", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// MovieActivation_Mode 
/// </summary>
internal partial class APM_MovieActivation_Mode : APM_MovieActivation_Mode__Base
{
}


internal partial class APM_MovieActivation_Mode__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MovieActivation_Mode";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_MovieActivation_Mode>(obj, "Mode", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Once || val == PdfName.Open || val == PdfName.Repeat || val == PdfName.Palindrome)) 
        {
            ctx.Fail<APM_MovieActivation_Mode>($"Invalid value {val}, allowed are: [Once,Open,Repeat,Palindrome]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// MovieActivation_Synchronous 
/// </summary>
internal partial class APM_MovieActivation_Synchronous : APM_MovieActivation_Synchronous__Base
{
}


internal partial class APM_MovieActivation_Synchronous__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MovieActivation_Synchronous";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_MovieActivation_Synchronous>(obj, "Synchronous", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// MovieActivation_FWScale 
/// </summary>
internal partial class APM_MovieActivation_FWScale : APM_MovieActivation_FWScale__Base
{
}


internal partial class APM_MovieActivation_FWScale__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MovieActivation_FWScale";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_MovieActivation_FWScale>(obj, "FWScale", IndirectRequirement.Either);
        if (val == null) { return; }
        var FWScale0 = val.Get(0);
        var FWScale1 = val.Get(1);
        if (!((gt(FWScale0,0)&&gt(FWScale1,0)))) 
        {
            ctx.Fail<APM_MovieActivation_FWScale>($"Value failed special case check: fn:Eval((FWScale::@0>0) && (FWScale::@1>0))");
        }
        // no value restrictions
        ctx.Run<APM_ArrayOf_2Integers, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// MovieActivation_FWPosition 
/// </summary>
internal partial class APM_MovieActivation_FWPosition : APM_MovieActivation_FWPosition__Base
{
}


internal partial class APM_MovieActivation_FWPosition__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "MovieActivation_FWPosition";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_MovieActivation_FWPosition>(obj, "FWPosition", IndirectRequirement.Either);
        if (val == null) { return; }
        var FWPosition0 = val.Get(0);
        var FWPosition1 = val.Get(1);
        if (!(gte(FWPosition0,0.0m)&&lte(FWPosition0,1.0m)&&gte(FWPosition1,0.0m)&&lte(FWPosition1,1.0m))) 
        {
            ctx.Fail<APM_MovieActivation_FWPosition>($"Value failed special case check: fn:Eval((FWPosition::@0>=0.0) && (FWPosition::@0<=1.0) && (FWPosition::@1>=0.0) && (FWPosition::@1<=1.0))");
        }
        // no value restrictions
        ctx.Run<APM_ArrayOf_2Numbers, PdfArray>(stack, val, obj);
        
    }


}
