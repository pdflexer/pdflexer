// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_DeviceNColorSpace : ISpecification<PdfArray>
{
    public static string Name { get; } = "DeviceNColorSpace";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_DeviceNColorSpace_0, PdfArray>(stack, obj, parent);
        ctx.Run<APM_DeviceNColorSpace_1, PdfArray>(stack, obj, parent);
        ctx.Run<APM_DeviceNColorSpace_2, PdfArray>(stack, obj, parent);
        ctx.Run<APM_DeviceNColorSpace_3, PdfArray>(stack, obj, parent);
        ctx.Run<APM_DeviceNColorSpace_4, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// DeviceNColorSpace_0 Clause 8.6.6.5
/// </summary>
internal partial class APM_DeviceNColorSpace_0 : ISpecification<PdfArray>
{
    public static string Name { get; } = "DeviceNColorSpace_0";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_DeviceNColorSpace_0>(obj, 0, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.DeviceN)) 
        {
            ctx.Fail<APM_DeviceNColorSpace_0>($"Invalid value {val}, allowed are: [DeviceN]");
        }
        // no linked objects
        
    }
}

/// <summary>
/// DeviceNColorSpace_1 
/// </summary>
internal partial class APM_DeviceNColorSpace_1 : ISpecification<PdfArray>
{
    public static string Name { get; } = "DeviceNColorSpace_1";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_DeviceNColorSpace_1>(obj, 1, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfNamesGeneral, PdfArray>(stack, val, obj);
        
    }
}

/// <summary>
/// DeviceNColorSpace_2 cannot be Pattern,Indexed,Separation,DeviceN
/// </summary>
internal partial class APM_DeviceNColorSpace_2 : ISpecification<PdfArray>
{
    public static string Name { get; } = "DeviceNColorSpace_2";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_DeviceNColorSpace_2>(obj, 2, IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_DeviceNColorSpace_2>("2 is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    if (APM_CalGrayColorSpace.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_CalGrayColorSpace, PdfArray>(stack, val, obj);
                    } else if (APM_CalRGBColorSpace.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_CalRGBColorSpace, PdfArray>(stack, val, obj);
                    } else if (APM_LabColorSpace.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_LabColorSpace, PdfArray>(stack, val, obj);
                    } else if (APM_ICCBasedColorSpace.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ICCBasedColorSpace, PdfArray>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_DeviceNColorSpace_2>("2 did not match any allowable types: '[CalGrayColorSpace,CalRGBColorSpace,LabColorSpace,ICCBasedColorSpace]'");
                    }
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    
                    
                    if (!(val == PdfName.DeviceCMYK || val == PdfName.DeviceRGB || val == PdfName.DeviceGray)) 
                    {
                        ctx.Fail<APM_DeviceNColorSpace_2>($"Invalid value {val}, allowed are: [DeviceCMYK,DeviceRGB,DeviceGray]");
                    }
                    // no linked objects
                    return;
                }
            
            default:
                ctx.Fail<APM_DeviceNColorSpace_2>("2 is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }
}

/// <summary>
/// DeviceNColorSpace_3 
/// </summary>
internal partial class APM_DeviceNColorSpace_3 : ISpecification<PdfArray>
{
    public static string Name { get; } = "DeviceNColorSpace_3";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_DeviceNColorSpace_3>(obj, 3, IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_DeviceNColorSpace_3>("3 is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    if (APM_FunctionType2.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_FunctionType2, PdfDictionary>(stack, val, obj);
                    } else if (APM_FunctionType3.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_FunctionType3, PdfDictionary>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_DeviceNColorSpace_3>("3 did not match any allowable types: '[FunctionType2,FunctionType3]'");
                    }
                    return;
                }
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_DeviceNColorSpace_3>("3 is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    if (APM_FunctionType0.MatchesType(ctx, val.Dictionary)) 
                    {
                        ctx.Run<APM_FunctionType0, PdfDictionary>(stack, val.Dictionary, obj);
                    } else if (APM_FunctionType4.MatchesType(ctx, val.Dictionary)) 
                    {
                        ctx.Run<APM_FunctionType4, PdfDictionary>(stack, val.Dictionary, obj);
                    }else 
                    {
                        ctx.Fail<APM_DeviceNColorSpace_3>("3 did not match any allowable types: '[FunctionType0,FunctionType4]'");
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_DeviceNColorSpace_3>("3 is required to one of 'dictionary;stream', was " + utval.Type);
                return;
        }
    }
}

/// <summary>
/// DeviceNColorSpace_4 
/// </summary>
internal partial class APM_DeviceNColorSpace_4 : ISpecification<PdfArray>
{
    public static string Name { get; } = "DeviceNColorSpace_4";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_DeviceNColorSpace_4>(obj, 4, IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_DeviceNDict, PdfDictionary>(stack, val, obj);
        
    }
}
