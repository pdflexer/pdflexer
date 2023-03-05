// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_HalftoneType1 : APM_HalftoneType1__Base
{
}

internal partial class APM_HalftoneType1__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "HalftoneType1";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_HalftoneType1_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType1_HalftoneType, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType1_HalftoneName, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType1_Frequency, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType1_Angle, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType1_SpotFunction, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType1_AccurateScreens, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_HalftoneType1_TransferFunction, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_HalftoneType1>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_HalftoneType1>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_HalftoneType1>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_HalftoneType1>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_HalftoneType1>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_HalftoneType1>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_HalftoneType1>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_HalftoneType1>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_HalftoneType1>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_HalftoneType1_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "HalftoneType", "HalftoneName", "Frequency", "Angle", "SpotFunction", "AccurateScreens", "TransferFunction"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "HalftoneType", "HalftoneName", "Frequency", "Angle", "SpotFunction", "AccurateScreens", "TransferFunction"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "HalftoneType", "HalftoneName", "Frequency", "Angle", "SpotFunction", "AccurateScreens", "TransferFunction"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "HalftoneType", "HalftoneName", "Frequency", "Angle", "SpotFunction", "AccurateScreens", "TransferFunction"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "HalftoneType", "HalftoneName", "Frequency", "Angle", "SpotFunction", "AccurateScreens", "TransferFunction"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "HalftoneType", "HalftoneName", "Frequency", "Angle", "SpotFunction", "AccurateScreens", "TransferFunction"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "HalftoneType", "HalftoneName", "Frequency", "Angle", "SpotFunction", "AccurateScreens", "TransferFunction"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "HalftoneType", "HalftoneName", "Frequency", "Angle", "SpotFunction", "AccurateScreens", "TransferFunction"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "HalftoneType", "HalftoneName", "Frequency", "Angle", "SpotFunction", "AccurateScreens", "TransferFunction"
    };
    


}

/// <summary>
/// HalftoneType1_Type Table 128
/// </summary>
internal partial class APM_HalftoneType1_Type : APM_HalftoneType1_Type__Base
{
}


internal partial class APM_HalftoneType1_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType1_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_HalftoneType1_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "Halftone")) 
        {
            ctx.Fail<APM_HalftoneType1_Type>($"Invalid value {val}, allowed are: [Halftone]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// HalftoneType1_HalftoneType 
/// </summary>
internal partial class APM_HalftoneType1_HalftoneType : APM_HalftoneType1_HalftoneType__Base
{
}


internal partial class APM_HalftoneType1_HalftoneType__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType1_HalftoneType";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_HalftoneType1_HalftoneType>(obj, "HalftoneType", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 1)) 
        {
            ctx.Fail<APM_HalftoneType1_HalftoneType>($"Invalid value {val}, allowed are: [1]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// HalftoneType1_HalftoneName 
/// </summary>
internal partial class APM_HalftoneType1_HalftoneName : APM_HalftoneType1_HalftoneName__Base
{
}


internal partial class APM_HalftoneType1_HalftoneName__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType1_HalftoneName";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_HalftoneType1_HalftoneName>(obj, "HalftoneName", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// HalftoneType1_Frequency 
/// </summary>
internal partial class APM_HalftoneType1_Frequency : APM_HalftoneType1_Frequency__Base
{
}


internal partial class APM_HalftoneType1_Frequency__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType1_Frequency";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_HalftoneType1_Frequency>(obj, "Frequency", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var Frequency = obj.Get("Frequency");
        if (!(gte(Frequency,0))) 
        {
            ctx.Fail<APM_HalftoneType1_Frequency>($"Invalid value {val}, allowed are: [fn:Eval(@Frequency>=0)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// HalftoneType1_Angle 
/// </summary>
internal partial class APM_HalftoneType1_Angle : APM_HalftoneType1_Angle__Base
{
}


internal partial class APM_HalftoneType1_Angle__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType1_Angle";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfNumber, APM_HalftoneType1_Angle>(obj, "Angle", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// HalftoneType1_SpotFunction 
/// </summary>
internal partial class APM_HalftoneType1_SpotFunction : APM_HalftoneType1_SpotFunction__Base
{
}


internal partial class APM_HalftoneType1_SpotFunction__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType1_SpotFunction";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_HalftoneType1_SpotFunction>(obj, "SpotFunction", IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_HalftoneType1_SpotFunction>("SpotFunction is required"); return; }
        switch (utval.Type) 
        {
            // TODO funcs: fn:SinceVersion(1.3,dictionary)
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_ArrayOfFunctions, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    
                    
                    if (!(val == "SimpleDot" || val == "InvertedSimpleDot" || val == "DoubleDot" || val == "InvertedDoubleDot" || val == "CosineDot" || val == "Double" || val == "InvertedDouble" || val == "Line" || val == "LineX" || val == "LineY" || val == "Round" || val == "Ellipse" || val == "EllipseA" || val == "InvertedEllipseA" || val == "EllipseB" || val == "EllipseC" || val == "InvertedEllipseC" || val == "Square" || val == "Cross" || val == "Rhomboid" || val == "Diamond")) 
                    {
                        ctx.Fail<APM_HalftoneType1_SpotFunction>($"Invalid value {val}, allowed are: [SimpleDot,InvertedSimpleDot,DoubleDot,InvertedDoubleDot,CosineDot,Double,InvertedDouble,Line,LineX,LineY,Round,Ellipse,EllipseA,InvertedEllipseA,EllipseB,EllipseC,InvertedEllipseC,Square,Cross,Rhomboid,Diamond]");
                    }
                    // no linked objects
                    return;
                }
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_HalftoneType1_SpotFunction>("SpotFunction is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    if (APM_FunctionType0.MatchesType(ctx, val.Dictionary)) 
                    {
                        ctx.Run<APM_FunctionType0, PdfDictionary>(stack, val.Dictionary, obj);
                    } else if ((ctx.Version >= 1.3m && APM_FunctionType4.MatchesType(ctx, val.Dictionary))) 
                    {
                        ctx.Run<APM_FunctionType4, PdfDictionary>(stack, val.Dictionary, obj);
                    }else 
                    {
                        ctx.Fail<APM_HalftoneType1_SpotFunction>("SpotFunction did not match any allowable types: '[FunctionType0,fn:SinceVersion(1.3,FunctionType4)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_HalftoneType1_SpotFunction>("SpotFunction is required to one of 'array;fn:SinceVersion(1.3,dictionary);name;stream', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// HalftoneType1_AccurateScreens 
/// </summary>
internal partial class APM_HalftoneType1_AccurateScreens : APM_HalftoneType1_AccurateScreens__Base
{
}


internal partial class APM_HalftoneType1_AccurateScreens__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType1_AccurateScreens";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_HalftoneType1_AccurateScreens>(obj, "AccurateScreens", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// HalftoneType1_TransferFunction 
/// </summary>
internal partial class APM_HalftoneType1_TransferFunction : APM_HalftoneType1_TransferFunction__Base
{
}


internal partial class APM_HalftoneType1_TransferFunction__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "HalftoneType1_TransferFunction";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_HalftoneType1_TransferFunction>(obj, "TransferFunction", IndirectRequirement.Either);
        
        var parentHalftoneType = parent?.Get("HalftoneType");
        if ((eq(parentHalftoneType,5)) && utval == null) {
            ctx.Fail<APM_HalftoneType1_TransferFunction>("TransferFunction is required"); return;
        } else if (utval == null) {
            return;
        }
        
        switch (utval.Type) 
        {
            // TODO funcs: fn:SinceVersion(1.3,dictionary)
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    
                    
                    if (!(val == "Identity")) 
                    {
                        ctx.Fail<APM_HalftoneType1_TransferFunction>($"Invalid value {val}, allowed are: [Identity]");
                    }
                    // no linked objects
                    return;
                }
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_HalftoneType1_TransferFunction>("TransferFunction is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    if (APM_FunctionType0.MatchesType(ctx, val.Dictionary)) 
                    {
                        ctx.Run<APM_FunctionType0, PdfDictionary>(stack, val.Dictionary, obj);
                    } else if ((ctx.Version >= 1.3m && APM_FunctionType4.MatchesType(ctx, val.Dictionary))) 
                    {
                        ctx.Run<APM_FunctionType4, PdfDictionary>(stack, val.Dictionary, obj);
                    }else 
                    {
                        ctx.Fail<APM_HalftoneType1_TransferFunction>("TransferFunction did not match any allowable types: '[FunctionType0,fn:SinceVersion(1.3,FunctionType4)]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_HalftoneType1_TransferFunction>("TransferFunction is required to one of 'fn:SinceVersion(1.3,dictionary);name;stream', was " + utval.Type);
                return;
        }
    }


}

