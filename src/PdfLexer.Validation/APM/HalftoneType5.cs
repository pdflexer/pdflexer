// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_HalftoneType5 : APM_HalftoneType5__Base
{
}

internal partial class APM_HalftoneType5__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "HalftoneType5";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_HalftoneType5_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType5_HalftoneType, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType5_HalftoneName, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType5_Default, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType5_CatchAll, PdfDictionary>(stack, obj, parent);
        
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_HalftoneType5_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    


}

/// <summary>
/// HalftoneType5_Type Table 132
/// </summary>
internal partial class APM_HalftoneType5_Type : APM_HalftoneType5_Type__Base
{
}


internal partial class APM_HalftoneType5_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType5_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_HalftoneType5_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "Halftone")) 
        {
            ctx.Fail<APM_HalftoneType5_Type>($"Invalid value {val}, allowed are: [Halftone]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// HalftoneType5_HalftoneType 
/// </summary>
internal partial class APM_HalftoneType5_HalftoneType : APM_HalftoneType5_HalftoneType__Base
{
}


internal partial class APM_HalftoneType5_HalftoneType__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType5_HalftoneType";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_HalftoneType5_HalftoneType>(obj, "HalftoneType", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 5)) 
        {
            ctx.Fail<APM_HalftoneType5_HalftoneType>($"Invalid value {val}, allowed are: [5]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// HalftoneType5_HalftoneName 
/// </summary>
internal partial class APM_HalftoneType5_HalftoneName : APM_HalftoneType5_HalftoneName__Base
{
}


internal partial class APM_HalftoneType5_HalftoneName__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType5_HalftoneName";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_HalftoneType5_HalftoneName>(obj, "HalftoneName", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// HalftoneType5_Default 
/// </summary>
internal partial class APM_HalftoneType5_Default : APM_HalftoneType5_Default__Base
{
}


internal partial class APM_HalftoneType5_Default__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType5_Default";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_HalftoneType5_Default>(obj, "Default", IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_HalftoneType5_Default>("Default is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_HalftoneType1, PdfDictionary>(stack, val, obj);
                    return;
                }
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_HalftoneType5_Default>("Default is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    if (APM_HalftoneType6.MatchesType(ctx, val.Dictionary)) 
                    {
                        ctx.Run<APM_HalftoneType6, PdfDictionary>(stack, val.Dictionary, obj);
                    } else if (APM_HalftoneType10.MatchesType(ctx, val.Dictionary)) 
                    {
                        ctx.Run<APM_HalftoneType10, PdfDictionary>(stack, val.Dictionary, obj);
                    } else if ((ctx.Version >= 1.3m && APM_HalftoneType16.MatchesType(ctx, val.Dictionary))) 
                    {
                        ctx.Run<APM_HalftoneType16, PdfDictionary>(stack, val.Dictionary, obj);
                    }else 
                    {
                        ctx.Fail<APM_HalftoneType5_Default>("Default did not match any allowable types: '[HalftoneType6,HalftoneType10,fn:SinceVersion(1.3,HalftoneType16)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_HalftoneType5_Default>("Default is required to one of 'dictionary;stream', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// HalftoneType5_* 
/// </summary>
internal partial class APM_HalftoneType5_CatchAll : APM_HalftoneType5_CatchAll__Base
{
}


internal partial class APM_HalftoneType5_CatchAll__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType5_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        foreach (var key in obj.Keys)
        {
            if (AllVals.Contains(key)) { continue; }
            
            
            var (utval, wasIR) = ctx.GetOptional<APM_HalftoneType5_CatchAll>(obj, key, IndirectRequirement.Either);
            if (utval == null) { return; }
            switch (utval.Type) 
            {
                case PdfObjectType.DictionaryObj:
                    {
                        var val =  (PdfDictionary)utval;
                        // no indirect obj reqs
                        // TODO special case: fn:KeyNameIsColorant()
                        // no value restrictions
                        ctx.Run<APM_HalftoneType1, PdfDictionary>(stack, val, obj);
                        return;
                    }
                case PdfObjectType.StreamObj:
                    {
                        var val =  (PdfStream)utval;
                        if (!wasIR) { ctx.Fail<APM_HalftoneType5_CatchAll>("* is required to be indirect when a stream"); return; }
                        // TODO special case: fn:KeyNameIsColorant()
                        // no value restrictions
                        if (APM_HalftoneType6.MatchesType(ctx, val.Dictionary)) 
                        {
                            ctx.Run<APM_HalftoneType6, PdfDictionary>(stack, val.Dictionary, obj);
                        } else if (APM_HalftoneType10.MatchesType(ctx, val.Dictionary)) 
                        {
                            ctx.Run<APM_HalftoneType10, PdfDictionary>(stack, val.Dictionary, obj);
                        } else if ((ctx.Version >= 1.3m && APM_HalftoneType16.MatchesType(ctx, val.Dictionary))) 
                        {
                            ctx.Run<APM_HalftoneType16, PdfDictionary>(stack, val.Dictionary, obj);
                        }else 
                        {
                            ctx.Fail<APM_HalftoneType5_CatchAll>("CatchAll did not match any allowable types: '[HalftoneType6,HalftoneType10,fn:SinceVersion(1.3,HalftoneType16)]'");
                        }
                        return;
                    }
                
                default:
                    ctx.Fail<APM_HalftoneType5_CatchAll>("* is required to one of 'dictionary;stream', was " + utval.Type);
                    return;
            }
        }
        
    }

    public static HashSet<string> AllVals = new HashSet<string> { "Type", "HalftoneType", "HalftoneName", "Default" };
}

