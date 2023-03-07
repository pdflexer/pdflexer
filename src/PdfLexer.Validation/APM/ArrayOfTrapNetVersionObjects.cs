// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOfTrapNetVersionObjects : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfTrapNetVersionObjects";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOfTrapNetVersionObjects_x, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOfTrapNetVersionObjects_* Table 403, Version cell
/// </summary>
internal partial class APM_ArrayOfTrapNetVersionObjects_x : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfTrapNetVersionObjects_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        for (var i = 0; i<obj.Count; i+=1) 
        {
            CheckSingle(i);
        }
        void CheckSingle(int n) 
        {
            var (utval, wasIR) = ctx.GetOptional<APM_ArrayOfTrapNetVersionObjects_x>(obj, n, IndirectRequirement.Either);
            if (utval == null) { return; }
            switch (utval.Type) 
            {
                case PdfObjectType.ArrayObj:
                    {
                        var val =  (PdfArray)utval;
                        if (!wasIR) { ctx.Fail<APM_ArrayOfTrapNetVersionObjects_x>("* is required to be indirect when a array"); return; }
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
                        } else if (APM_IndexedColorSpace.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_IndexedColorSpace, PdfArray>(stack, val, obj);
                        } else if (APM_SeparationColorSpace.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_SeparationColorSpace, PdfArray>(stack, val, obj);
                        } else if (APM_DeviceNColorSpace.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_DeviceNColorSpace, PdfArray>(stack, val, obj);
                        } else if (APM_PatternColorSpace.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_PatternColorSpace, PdfArray>(stack, val, obj);
                        }else 
                        {
                            ctx.Fail<APM_ArrayOfTrapNetVersionObjects_x>("x did not match any allowable types: '[CalGrayColorSpace,CalRGBColorSpace,LabColorSpace,ICCBasedColorSpace,IndexedColorSpace,SeparationColorSpace,DeviceNColorSpace,PatternColorSpace]'");
                        }
                        return;
                    }
                case PdfObjectType.DictionaryObj:
                    {
                        var val =  (PdfDictionary)utval;
                        if (!wasIR) { ctx.Fail<APM_ArrayOfTrapNetVersionObjects_x>("* is required to be indirect when a dictionary"); return; }
                        // no special cases
                        // no value restrictions
                        if (APM_OPIVersion13Dict.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_OPIVersion13Dict, PdfDictionary>(stack, val, obj);
                        } else if (APM_OPIVersion20Dict.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_OPIVersion20Dict, PdfDictionary>(stack, val, obj);
                        } else if (APM_GraphicsStateParameter.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_GraphicsStateParameter, PdfDictionary>(stack, val, obj);
                        } else if (APM_PatternType2.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_PatternType2, PdfDictionary>(stack, val, obj);
                        } else if (APM_ShadingType1.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_ShadingType1, PdfDictionary>(stack, val, obj);
                        } else if (APM_ShadingType2.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_ShadingType2, PdfDictionary>(stack, val, obj);
                        } else if (APM_ShadingType3.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_ShadingType3, PdfDictionary>(stack, val, obj);
                        } else if (APM_XObjectFormType1.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_XObjectFormType1, PdfDictionary>(stack, val, obj);
                        } else if (APM_XObjectImage.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_XObjectImage, PdfDictionary>(stack, val, obj);
                        } else if (APM_XObjectFormPS.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_XObjectFormPS, PdfDictionary>(stack, val, obj);
                        } else if (APM_XObjectFormPSpassthrough.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_XObjectFormPSpassthrough, PdfDictionary>(stack, val, obj);
                        } else if (APM_FontType1.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_FontType1, PdfDictionary>(stack, val, obj);
                        } else if (APM_FontTrueType.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_FontTrueType, PdfDictionary>(stack, val, obj);
                        } else if (APM_FontMultipleMaster.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_FontMultipleMaster, PdfDictionary>(stack, val, obj);
                        } else if (APM_FontType3.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_FontType3, PdfDictionary>(stack, val, obj);
                        } else if (APM_FontType0.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_FontType0, PdfDictionary>(stack, val, obj);
                        } else if (APM_FontCIDType0.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_FontCIDType0, PdfDictionary>(stack, val, obj);
                        } else if (APM_FontCIDType2.MatchesType(ctx, val)) 
                        {
                            ctx.Run<APM_FontCIDType2, PdfDictionary>(stack, val, obj);
                        }else 
                        {
                            ctx.Fail<APM_ArrayOfTrapNetVersionObjects_x>("x did not match any allowable types: '[OPIVersion13Dict,OPIVersion20Dict,GraphicsStateParameter,PatternType2,ShadingType1,ShadingType2,ShadingType3,XObjectFormType1,XObjectImage,XObjectFormPS,XObjectFormPSpassthrough,FontType1,FontTrueType,FontMultipleMaster,FontType3,FontType0,FontCIDType0,FontCIDType2]'");
                        }
                        return;
                    }
                case PdfObjectType.StreamObj:
                    {
                        var val =  (PdfStream)utval;
                        if (!wasIR) { ctx.Fail<APM_ArrayOfTrapNetVersionObjects_x>("* is required to be indirect when a stream"); return; }
                        // no special cases
                        // no value restrictions
                        if (APM_PatternType1.MatchesType(ctx, val.Dictionary)) 
                        {
                            ctx.Run<APM_PatternType1, PdfDictionary>(stack, val.Dictionary, obj);
                        } else if (APM_ShadingType4.MatchesType(ctx, val.Dictionary)) 
                        {
                            ctx.Run<APM_ShadingType4, PdfDictionary>(stack, val.Dictionary, obj);
                        } else if (APM_ShadingType5.MatchesType(ctx, val.Dictionary)) 
                        {
                            ctx.Run<APM_ShadingType5, PdfDictionary>(stack, val.Dictionary, obj);
                        } else if (APM_ShadingType6.MatchesType(ctx, val.Dictionary)) 
                        {
                            ctx.Run<APM_ShadingType6, PdfDictionary>(stack, val.Dictionary, obj);
                        } else if (APM_ShadingType7.MatchesType(ctx, val.Dictionary)) 
                        {
                            ctx.Run<APM_ShadingType7, PdfDictionary>(stack, val.Dictionary, obj);
                        } else if (APM_Stream.MatchesType(ctx, val.Dictionary)) 
                        {
                            ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
                        }else 
                        {
                            ctx.Fail<APM_ArrayOfTrapNetVersionObjects_x>("x did not match any allowable types: '[PatternType1,ShadingType4,ShadingType5,ShadingType6,ShadingType7,Stream]'");
                        }
                        return;
                    }
                
                default:
                    ctx.Fail<APM_ArrayOfTrapNetVersionObjects_x>("* is required to one of 'array;dictionary;stream', was " + utval.Type);
                    return;
            }
        }
    }
}
