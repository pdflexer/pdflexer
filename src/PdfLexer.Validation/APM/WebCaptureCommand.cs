// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_WebCaptureCommand : APM_WebCaptureCommand__Base
{
}

internal partial class APM_WebCaptureCommand__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "WebCaptureCommand";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_WebCaptureCommand_URL, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_WebCaptureCommand_L, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_WebCaptureCommand_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_WebCaptureCommand_P, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_WebCaptureCommand_CT, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_WebCaptureCommand_H, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_WebCaptureCommand_S, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_WebCaptureCommand>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_WebCaptureCommand>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_WebCaptureCommand>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_WebCaptureCommand>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_WebCaptureCommand>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_WebCaptureCommand>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_WebCaptureCommand>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_WebCaptureCommand>($"Unknown field {extra} for version 2.0");
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

    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "URL", "L", "F", "P", "CT", "H", "S"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "URL", "L", "F", "P", "CT", "H", "S"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "URL", "L", "F", "P", "CT", "H", "S"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "URL", "L", "F", "P", "CT", "H", "S"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "URL", "L", "F", "P", "CT", "H", "S"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "URL", "L", "F", "P", "CT", "H", "S"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "URL", "L", "F", "P", "CT", "H", "S"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "URL", "L", "F", "P", "CT", "H", "S"
    };
    


}

/// <summary>
/// WebCaptureCommand_URL Table 393
/// </summary>
internal partial class APM_WebCaptureCommand_URL : APM_WebCaptureCommand_URL__Base
{
}


internal partial class APM_WebCaptureCommand_URL__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "WebCaptureCommand_URL";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfString, APM_WebCaptureCommand_URL>(obj, "URL", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// WebCaptureCommand_L 
/// </summary>
internal partial class APM_WebCaptureCommand_L : APM_WebCaptureCommand_L__Base
{
}


internal partial class APM_WebCaptureCommand_L__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "WebCaptureCommand_L";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_WebCaptureCommand_L>(obj, "L", IndirectRequirement.Either);
        if (val == null) { return; }
        var L = obj.Get("L");
        if (!(gte(L,1))) 
        {
            ctx.Fail<APM_WebCaptureCommand_L>($"Value failed special case check: fn:Eval(@L>=1)");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// WebCaptureCommand_F Table 394
/// </summary>
internal partial class APM_WebCaptureCommand_F : APM_WebCaptureCommand_F__Base
{
}


internal partial class APM_WebCaptureCommand_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "WebCaptureCommand_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_WebCaptureCommand_F>(obj, "F", IndirectRequirement.Either);
        if (val == null) { return; }
        
        if (!(BitsClear(obj))) 
        {
            ctx.Fail<APM_WebCaptureCommand_F>($"Value failed special case check: fn:Eval(fn:BitsClear(4,32))");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// WebCaptureCommand_P 
/// </summary>
internal partial class APM_WebCaptureCommand_P : APM_WebCaptureCommand_P__Base
{
}


internal partial class APM_WebCaptureCommand_P__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "WebCaptureCommand_P";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_WebCaptureCommand_P>(obj, "P", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_WebCaptureCommand_P>("P is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
                    return;
                }
            case PdfObjectType.StringObj:
                {
                    var val =  (PdfString)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_WebCaptureCommand_P>("P is required to one of 'stream;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// WebCaptureCommand_CT shall describe the content type of the posted data as described in Internet RFC 2045
/// </summary>
internal partial class APM_WebCaptureCommand_CT : APM_WebCaptureCommand_CT__Base
{
}


internal partial class APM_WebCaptureCommand_CT__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "WebCaptureCommand_CT";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_WebCaptureCommand_CT>(obj, "CT", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// WebCaptureCommand_H 
/// </summary>
internal partial class APM_WebCaptureCommand_H : APM_WebCaptureCommand_H__Base
{
}


internal partial class APM_WebCaptureCommand_H__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "WebCaptureCommand_H";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_WebCaptureCommand_H>(obj, "H", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// WebCaptureCommand_S 
/// </summary>
internal partial class APM_WebCaptureCommand_S : APM_WebCaptureCommand_S__Base
{
}


internal partial class APM_WebCaptureCommand_S__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "WebCaptureCommand_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_WebCaptureCommand_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_WebCaptureCommandSettings, PdfDictionary>(stack, val, obj);
        
    }


}

