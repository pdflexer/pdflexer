// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_RichMediaParams : APM_RichMediaParams__Base
{
}

internal partial class APM_RichMediaParams__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "RichMediaParams";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_RichMediaParams_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaParams_FlashVars, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaParams_Binding, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaParams_BindingMaterial, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaParams_CuePoints, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaParams_Seetings, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.7m:
            case 1.8m:
            case 1.9m:
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17_18_19_20.Contains(x)))
                {
                    ctx.Fail<APM_RichMediaParams>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_RichMediaParams_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_17_18_19_20 { get; } = new HashSet<string> 
    {
        "Binding", "BindingMaterial", "CuePoints", "FlashVars", "Seetings", "Type"
    };
    


}

/// <summary>
/// RichMediaParams_Type Adobe Extension Level 3, Table 9.51c
/// </summary>
internal partial class APM_RichMediaParams_Type : APM_RichMediaParams_Type__Base
{
}


internal partial class APM_RichMediaParams_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaParams_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_RichMediaParams_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.RichMediaParams)) 
        {
            ctx.Fail<APM_RichMediaParams_Type>($"Invalid value {val}, allowed are: [RichMediaParams]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// RichMediaParams_FlashVars 
/// </summary>
internal partial class APM_RichMediaParams_FlashVars : APM_RichMediaParams_FlashVars__Base
{
}


internal partial class APM_RichMediaParams_FlashVars__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaParams_FlashVars";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_RichMediaParams_FlashVars>(obj, "FlashVars", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_RichMediaParams_FlashVars>("FlashVars is required to be indirect when a stream"); return; }
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
                ctx.Fail<APM_RichMediaParams_FlashVars>("FlashVars is required to one of 'stream;string-text', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// RichMediaParams_Binding 
/// </summary>
internal partial class APM_RichMediaParams_Binding : APM_RichMediaParams_Binding__Base
{
}


internal partial class APM_RichMediaParams_Binding__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaParams_Binding";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_RichMediaParams_Binding>(obj, "Binding", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.None || val == PdfName.Foreground || val == PdfName.Background || val == PdfName.Material)) 
        {
            ctx.Fail<APM_RichMediaParams_Binding>($"Invalid value {val}, allowed are: [None,Foreground,Background,Material]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// RichMediaParams_BindingMaterial 
/// </summary>
internal partial class APM_RichMediaParams_BindingMaterial : APM_RichMediaParams_BindingMaterial__Base
{
}


internal partial class APM_RichMediaParams_BindingMaterial__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaParams_BindingMaterial";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var Binding = obj.Get("Binding");
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_RichMediaParams_BindingMaterial>(obj, "BindingMaterial", IndirectRequirement.Either);
        if ((eq(Binding,PdfName.Material)) && val == null) {
            ctx.Fail<APM_RichMediaParams_BindingMaterial>("BindingMaterial is required when 'fn:IsRequired(@Binding==Material)"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// RichMediaParams_CuePoints 
/// </summary>
internal partial class APM_RichMediaParams_CuePoints : APM_RichMediaParams_CuePoints__Base
{
}


internal partial class APM_RichMediaParams_CuePoints__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaParams_CuePoints";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_RichMediaParams_CuePoints>(obj, "CuePoints", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfRichMediaCuePoints, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// RichMediaParams_Seetings 
/// </summary>
internal partial class APM_RichMediaParams_Seetings : APM_RichMediaParams_Seetings__Base
{
}


internal partial class APM_RichMediaParams_Seetings__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaParams_Seetings";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_RichMediaParams_Seetings>(obj, "Seetings", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_RichMediaParams_Seetings>("Seetings is required to be indirect when a stream"); return; }
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
                ctx.Fail<APM_RichMediaParams_Seetings>("Seetings is required to one of 'stream;string-text', was " + utval.Type);
                return;
        }
    }


}

