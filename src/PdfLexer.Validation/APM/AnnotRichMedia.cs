// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_AnnotRichMedia : APM_AnnotRichMedia_Base
{
}

internal partial class APM_AnnotRichMedia_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "AnnotRichMedia";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_AnnotRichMedia_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotRichMedia_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotRichMedia_Rect, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotRichMedia_Contents, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotRichMedia_P, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotRichMedia_NM, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotRichMedia_M, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotRichMedia_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotRichMedia_AP, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotRichMedia_AS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotRichMedia_Border, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotRichMedia_C, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotRichMedia_StructParent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotRichMedia_OC, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotRichMedia_AF, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotRichMedia_ca, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotRichMedia_CA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotRichMedia_BM, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotRichMedia_Lang, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotRichMedia_RichMediaContent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotRichMedia_RichMediaSettings, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_AnnotRichMedia>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_AnnotRichMedia_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "AF", "ca", "CA", "BM", "Lang"
    };
    


}

/// <summary>
/// AnnotRichMedia_Type Table 166 and Table 333 (NOT markup annot)
/// </summary>
internal partial class APM_AnnotRichMedia_Type : APM_AnnotRichMedia_Type_Base
{
}


internal partial class APM_AnnotRichMedia_Type_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_AnnotRichMedia_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "Annot")) 
        {
            ctx.Fail<APM_AnnotRichMedia_Type>($"Invalid value {val}, allowed are: [Annot]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotRichMedia_Subtype 
/// </summary>
internal partial class APM_AnnotRichMedia_Subtype : APM_AnnotRichMedia_Subtype_Base
{
}


internal partial class APM_AnnotRichMedia_Subtype_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_AnnotRichMedia_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "RichMedia")) 
        {
            ctx.Fail<APM_AnnotRichMedia_Subtype>($"Invalid value {val}, allowed are: [RichMedia]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotRichMedia_Rect 
/// </summary>
internal partial class APM_AnnotRichMedia_Rect : APM_AnnotRichMedia_Rect_Base
{
}


internal partial class APM_AnnotRichMedia_Rect_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_Rect";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_AnnotRichMedia_Rect>(obj, "Rect", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotRichMedia_Contents 
/// </summary>
internal partial class APM_AnnotRichMedia_Contents : APM_AnnotRichMedia_Contents_Base
{
}


internal partial class APM_AnnotRichMedia_Contents_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_Contents";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_AnnotRichMedia_Contents>(obj, "Contents", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotRichMedia_P 
/// </summary>
internal partial class APM_AnnotRichMedia_P : APM_AnnotRichMedia_P_Base
{
}


internal partial class APM_AnnotRichMedia_P_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_P";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_AnnotRichMedia_P>(obj, "P", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_PageObject, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotRichMedia_NM 
/// </summary>
internal partial class APM_AnnotRichMedia_NM : APM_AnnotRichMedia_NM_Base
{
}


internal partial class APM_AnnotRichMedia_NM_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_NM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_AnnotRichMedia_NM>(obj, "NM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotRichMedia_M 
/// </summary>
internal partial class APM_AnnotRichMedia_M : APM_AnnotRichMedia_M_Base
{
}


internal partial class APM_AnnotRichMedia_M_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_M";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_AnnotRichMedia_M>(obj, "M", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            
            // TODO MC date;string-text
            
            default:
                ctx.Fail<APM_AnnotRichMedia_M>("M is required to one of 'date;string-text', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// AnnotRichMedia_F Table 167
/// </summary>
internal partial class APM_AnnotRichMedia_F : APM_AnnotRichMedia_F_Base
{
}


internal partial class APM_AnnotRichMedia_F_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_AnnotRichMedia_F>(obj, "F", IndirectRequirement.Either);
        if (val == null) { return; }
        // TODO special case
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotRichMedia_AP 
/// </summary>
internal partial class APM_AnnotRichMedia_AP : APM_AnnotRichMedia_AP_Base
{
}


internal partial class APM_AnnotRichMedia_AP_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_AP";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        PdfDictionary? val;
        {
            var Rect = obj.Get("Rect");
            if (ctx.Version >= 2.0m && (gt(RectWidth(obj),0)||gt(RectHeight(Rect),0))) {
                val = ctx.GetRequired<PdfDictionary, APM_AnnotRichMedia_AP>(obj, "AP", IndirectRequirement.Either);
            } else {
                val = ctx.GetOptional<PdfDictionary, APM_AnnotRichMedia_AP>(obj, "AP", IndirectRequirement.Either);
            }
            if (val == null) { return; }
        }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Appearance, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotRichMedia_AS 
/// </summary>
internal partial class APM_AnnotRichMedia_AS : APM_AnnotRichMedia_AS_Base
{
}


internal partial class APM_AnnotRichMedia_AS_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_AS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        PdfName? val;
        {
            var APN = obj.Get("AP")?.Get("N");
            var APR = obj.Get("AP")?.Get("R");
            var APD = obj.Get("AP")?.Get("D");
            if (obj.ContainsKey(APN)||obj.ContainsKey(APR)||obj.ContainsKey(APD)) {
                val = ctx.GetRequired<PdfName, APM_AnnotRichMedia_AS>(obj, "AS", IndirectRequirement.Either);
            } else {
                val = ctx.GetOptional<PdfName, APM_AnnotRichMedia_AS>(obj, "AS", IndirectRequirement.Either);
            }
            if (val == null) { return; }
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotRichMedia_Border 
/// </summary>
internal partial class APM_AnnotRichMedia_Border : APM_AnnotRichMedia_Border_Base
{
}


internal partial class APM_AnnotRichMedia_Border_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_Border";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_AnnotRichMedia_Border>(obj, "Border", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_4AnnotBorderCharacteristics, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotRichMedia_C 
/// </summary>
internal partial class APM_AnnotRichMedia_C : APM_AnnotRichMedia_C_Base
{
}


internal partial class APM_AnnotRichMedia_C_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_C";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_AnnotRichMedia_C>(obj, "C", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_4NumbersColorAnnotation, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotRichMedia_StructParent Table 359
/// </summary>
internal partial class APM_AnnotRichMedia_StructParent : APM_AnnotRichMedia_StructParent_Base
{
}


internal partial class APM_AnnotRichMedia_StructParent_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_StructParent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_AnnotRichMedia_StructParent>(obj, "StructParent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotRichMedia_OC 
/// </summary>
internal partial class APM_AnnotRichMedia_OC : APM_AnnotRichMedia_OC_Base
{
}


internal partial class APM_AnnotRichMedia_OC_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_OC";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_AnnotRichMedia_OC>(obj, "OC", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_OptContentGroup.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_OptContentGroup, PdfDictionary>(stack, val, obj);
        } else if (APM_OptContentMembership.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_OptContentMembership, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_AnnotRichMedia_OC>("OC did not match any allowable types: '[OptContentGroup,OptContentMembership]'");
        }
        
    }


}

/// <summary>
/// AnnotRichMedia_AF 
/// </summary>
internal partial class APM_AnnotRichMedia_AF : APM_AnnotRichMedia_AF_Base
{
}


internal partial class APM_AnnotRichMedia_AF_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_AF";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_AnnotRichMedia_AF>(obj, "AF", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_ArrayOfFileSpecifications, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_FileSpecification, PdfDictionary>(stack, val, obj);
                    return;
                }
            
            default:
                ctx.Fail<APM_AnnotRichMedia_AF>("AF is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// AnnotRichMedia_ca 
/// </summary>
internal partial class APM_AnnotRichMedia_ca : APM_AnnotRichMedia_ca_Base
{
}


internal partial class APM_AnnotRichMedia_ca_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_ca";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_AnnotRichMedia_ca>(obj, "ca", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        IPdfObject @ca = val;
        if (!((gte(@ca,0.0m)&&lte(@ca,1.0m)))) 
        {
            ctx.Fail<APM_AnnotRichMedia_ca>($"Invalid value {val}, allowed are: [fn:Eval((@ca>=0.0) && (@ca<=1.0))]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotRichMedia_CA 
/// </summary>
internal partial class APM_AnnotRichMedia_CA : APM_AnnotRichMedia_CA_Base
{
}


internal partial class APM_AnnotRichMedia_CA_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_CA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_AnnotRichMedia_CA>(obj, "CA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        IPdfObject @CA = val;
        if (!((gte(@CA,0.0m)&&lte(@CA,1.0m)))) 
        {
            ctx.Fail<APM_AnnotRichMedia_CA>($"Invalid value {val}, allowed are: [fn:Eval((@CA>=0.0) && (@CA<=1.0))]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotRichMedia_BM Table 134 and Table 135
/// </summary>
internal partial class APM_AnnotRichMedia_BM : APM_AnnotRichMedia_BM_Base
{
}


internal partial class APM_AnnotRichMedia_BM_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_BM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_AnnotRichMedia_BM>(obj, "BM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!((ctx.Version <= 1.4m && val == "Compatible") || val == "Normal" || val == "Multiply" || val == "Screen" || val == "Difference" || val == "Darken" || val == "Lighten" || val == "ColorDodge" || val == "ColorBurn" || val == "Exclusion" || val == "HardLight" || val == "Overlay" || val == "SoftLight" || val == "Luminosity" || val == "Hue" || val == "Saturation" || val == "Color")) 
        {
            ctx.Fail<APM_AnnotRichMedia_BM>($"Invalid value {val}, allowed are: [fn:Deprecated(1.4,Compatible),Normal,Multiply,Screen,Difference,Darken,Lighten,ColorDodge,ColorBurn,Exclusion,HardLight,Overlay,SoftLight,Luminosity,Hue,Saturation,Color]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotRichMedia_Lang 
/// </summary>
internal partial class APM_AnnotRichMedia_Lang : APM_AnnotRichMedia_Lang_Base
{
}


internal partial class APM_AnnotRichMedia_Lang_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_Lang";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_AnnotRichMedia_Lang>(obj, "Lang", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotRichMedia_RichMediaContent 
/// </summary>
internal partial class APM_AnnotRichMedia_RichMediaContent : APM_AnnotRichMedia_RichMediaContent_Base
{
}


internal partial class APM_AnnotRichMedia_RichMediaContent_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_RichMediaContent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_AnnotRichMedia_RichMediaContent>(obj, "RichMediaContent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_RichMediaContent, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotRichMedia_RichMediaSettings 
/// </summary>
internal partial class APM_AnnotRichMedia_RichMediaSettings : APM_AnnotRichMedia_RichMediaSettings_Base
{
}


internal partial class APM_AnnotRichMedia_RichMediaSettings_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotRichMedia_RichMediaSettings";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_AnnotRichMedia_RichMediaSettings>(obj, "RichMediaSettings", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_RichMediaSettings, PdfDictionary>(stack, val, obj);
        
    }


}

