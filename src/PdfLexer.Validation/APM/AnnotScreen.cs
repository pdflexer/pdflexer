// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_AnnotScreen : APM_AnnotScreen__Base
{
}

internal partial class APM_AnnotScreen__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "AnnotScreen";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_AnnotScreen_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_Rect, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_Contents, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_P, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_NM, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_M, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_AP, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_AS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_Border, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_C, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_StructParent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_OC, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_AF, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_ca, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_CA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_BM, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_Lang, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_T, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_MK, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_A, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotScreen_AA, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_AnnotScreen>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_AnnotScreen>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_AnnotScreen>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_AnnotScreen>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_AnnotScreen>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_AnnotScreen>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_AnnotScreen_Type, PdfDictionary>(new CallStack(), obj, null);
        c.Run<APM_AnnotScreen_Subtype, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Rect", "Contents", "P", "NM", "M", "F", "AP", "AS", "Border", "C", "StructParent", "OC", "T", "MK", "A", "AA"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Rect", "Contents", "P", "NM", "M", "F", "AP", "AS", "Border", "C", "StructParent", "OC", "T", "MK", "A", "AA"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Rect", "Contents", "P", "NM", "M", "F", "AP", "AS", "Border", "C", "StructParent", "OC", "T", "MK", "A", "AA"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Rect", "Contents", "P", "NM", "M", "F", "AP", "AS", "Border", "C", "StructParent", "OC", "T", "MK", "A", "AA"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Rect", "Contents", "P", "NM", "M", "F", "AP", "AS", "Border", "C", "StructParent", "OC", "T", "MK", "A", "AA"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Rect", "Contents", "P", "NM", "M", "F", "AP", "AS", "Border", "C", "StructParent", "OC", "AF", "ca", "CA", "BM", "Lang", "T", "MK", "A", "AA"
    };
    


}

/// <summary>
/// AnnotScreen_Type Table 166 and Table 190 (NOT markup annot)
/// </summary>
internal partial class APM_AnnotScreen_Type : APM_AnnotScreen_Type__Base
{
}


internal partial class APM_AnnotScreen_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_AnnotScreen_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Annot)) 
        {
            ctx.Fail<APM_AnnotScreen_Type>($"Invalid value {val}, allowed are: [Annot]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotScreen_Subtype 
/// </summary>
internal partial class APM_AnnotScreen_Subtype : APM_AnnotScreen_Subtype__Base
{
}


internal partial class APM_AnnotScreen_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfName, APM_AnnotScreen_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Screen)) 
        {
            ctx.Fail<APM_AnnotScreen_Subtype>($"Invalid value {val}, allowed are: [Screen]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotScreen_Rect 
/// </summary>
internal partial class APM_AnnotScreen_Rect : APM_AnnotScreen_Rect__Base
{
}


internal partial class APM_AnnotScreen_Rect__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_Rect";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfArray, APM_AnnotScreen_Rect>(obj, "Rect", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotScreen_Contents 
/// </summary>
internal partial class APM_AnnotScreen_Contents : APM_AnnotScreen_Contents__Base
{
}


internal partial class APM_AnnotScreen_Contents__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_Contents";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_AnnotScreen_Contents>(obj, "Contents", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotScreen_P 
/// </summary>
internal partial class APM_AnnotScreen_P : APM_AnnotScreen_P__Base
{
}


internal partial class APM_AnnotScreen_P__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_P";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_AnnotScreen_P>(obj, "P", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_PageObject, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotScreen_NM 
/// </summary>
internal partial class APM_AnnotScreen_NM : APM_AnnotScreen_NM__Base
{
}


internal partial class APM_AnnotScreen_NM__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_NM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_AnnotScreen_NM>(obj, "NM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotScreen_M 
/// </summary>
internal partial class APM_AnnotScreen_M : APM_AnnotScreen_M__Base
{
}


internal partial class APM_AnnotScreen_M__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_M";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_AnnotScreen_M>(obj, "M", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.StringObj:
                {
            
                    // TODO MC date;string-text
            
                    var val =  (PdfString)utval;
                    if (IsDate(val)) 
                    {
                        // date
                        // no indirect obj reqs
                        // no special cases
                        // no value restrictions
                        // no linked objects
                    } else if (true) 
                    {
                        // string-text
                        // no indirect obj reqs
                        // no special cases
                        // no value restrictions
                        // no linked objects
                    }
                    return;
                }
            
            default:
                ctx.Fail<APM_AnnotScreen_M>("M is required to one of 'date;string-text', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// AnnotScreen_F Table 167
/// </summary>
internal partial class APM_AnnotScreen_F : APM_AnnotScreen_F__Base
{
}


internal partial class APM_AnnotScreen_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfIntNumber, APM_AnnotScreen_F>(obj, "F", IndirectRequirement.Either);
        if (val == null) { return; }
        
        if (!(((ctx.Version >= 1.7m || BitsClear(val,0b11111111111111111111111000000000))&&(ctx.Version < 1.7m || BitsClear(val,0b11111111111111111111110000000000))))) 
        {
            ctx.Fail<APM_AnnotScreen_F>($"Value failed special case check: fn:Eval(fn:BeforeVersion(1.7,fn:BitsClear(10,32)) && fn:SinceVersion(1.7,fn:BitsClear(11,32)))");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotScreen_AP 
/// </summary>
internal partial class APM_AnnotScreen_AP : APM_AnnotScreen_AP__Base
{
}


internal partial class APM_AnnotScreen_AP__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_AP";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var Rect = obj.Get("Rect");
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_AnnotScreen_AP>(obj, "AP", IndirectRequirement.Either);
        if (((ctx.Version < 2.0m || (gt(RectWidth(Rect),0)||gt(RectHeight(Rect),0)))) && val == null) {
            ctx.Fail<APM_AnnotScreen_AP>("AP is required when 'fn:IsRequired(fn:SinceVersion(2.0,(fn:RectWidth(Rect)>0) || (fn:RectHeight(Rect)>0)))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Appearance, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotScreen_AS 
/// </summary>
internal partial class APM_AnnotScreen_AS : APM_AnnotScreen_AS__Base
{
}


internal partial class APM_AnnotScreen_AS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_AS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var APN = obj.Get("AP")?.Get("N");
        var APR = obj.Get("AP")?.Get("R");
        var APD = obj.Get("AP")?.Get("D");
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_AnnotScreen_AS>(obj, "AS", IndirectRequirement.Either);
        if (((APN != null)||(APR != null)||(APD != null)) && val == null) {
            ctx.Fail<APM_AnnotScreen_AS>("AS is required when 'fn:IsRequired(fn:IsPresent(AP::N::*) || fn:IsPresent(AP::R::*) || fn:IsPresent(AP::D::*))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotScreen_Border 
/// </summary>
internal partial class APM_AnnotScreen_Border : APM_AnnotScreen_Border__Base
{
}


internal partial class APM_AnnotScreen_Border__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_Border";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_AnnotScreen_Border>(obj, "Border", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_4AnnotBorderCharacteristics, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotScreen_C 
/// </summary>
internal partial class APM_AnnotScreen_C : APM_AnnotScreen_C__Base
{
}


internal partial class APM_AnnotScreen_C__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_C";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_AnnotScreen_C>(obj, "C", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_4NumbersColorAnnotation, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotScreen_StructParent Table 359
/// </summary>
internal partial class APM_AnnotScreen_StructParent : APM_AnnotScreen_StructParent__Base
{
}


internal partial class APM_AnnotScreen_StructParent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_StructParent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfIntNumber, APM_AnnotScreen_StructParent>(obj, "StructParent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotScreen_OC 
/// </summary>
internal partial class APM_AnnotScreen_OC : APM_AnnotScreen_OC__Base
{
}


internal partial class APM_AnnotScreen_OC__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_OC";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_AnnotScreen_OC>(obj, "OC", IndirectRequirement.Either);
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
            ctx.Fail<APM_AnnotScreen_OC>("OC did not match any allowable types: '[OptContentGroup,OptContentMembership]'");
        }
        
    }


}

/// <summary>
/// AnnotScreen_AF 
/// </summary>
internal partial class APM_AnnotScreen_AF : APM_AnnotScreen_AF__Base
{
}


internal partial class APM_AnnotScreen_AF__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_AF";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_AnnotScreen_AF>(obj, "AF", IndirectRequirement.Either);
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
                ctx.Fail<APM_AnnotScreen_AF>("AF is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// AnnotScreen_ca 
/// </summary>
internal partial class APM_AnnotScreen_ca : APM_AnnotScreen_ca__Base
{
}


internal partial class APM_AnnotScreen_ca__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_ca";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfNumber, APM_AnnotScreen_ca>(obj, "ca", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var ca = obj.Get("ca");
        if (!((gte(ca,0.0m)&&lte(ca,1.0m)))) 
        {
            ctx.Fail<APM_AnnotScreen_ca>($"Invalid value {val}, allowed are: [fn:Eval((@ca>=0.0) && (@ca<=1.0))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotScreen_CA 
/// </summary>
internal partial class APM_AnnotScreen_CA : APM_AnnotScreen_CA__Base
{
}


internal partial class APM_AnnotScreen_CA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_CA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfNumber, APM_AnnotScreen_CA>(obj, "CA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var CA = obj.Get("CA");
        if (!((gte(CA,0.0m)&&lte(CA,1.0m)))) 
        {
            ctx.Fail<APM_AnnotScreen_CA>($"Invalid value {val}, allowed are: [fn:Eval((@CA>=0.0) && (@CA<=1.0))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotScreen_BM Table 134 and Table 135
/// </summary>
internal partial class APM_AnnotScreen_BM : APM_AnnotScreen_BM__Base
{
}


internal partial class APM_AnnotScreen_BM__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_BM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_AnnotScreen_BM>(obj, "BM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!((ctx.Version < 1.4m && val == PdfName.Compatible) || val == PdfName.Normal || val == PdfName.Multiply || val == PdfName.Screen || val == PdfName.Difference || val == PdfName.Darken || val == PdfName.Lighten || val == PdfName.ColorDodge || val == PdfName.ColorBurn || val == PdfName.Exclusion || val == PdfName.HardLight || val == PdfName.Overlay || val == PdfName.SoftLight || val == PdfName.Luminosity || val == PdfName.Hue || val == PdfName.Saturation || val == PdfName.Color)) 
        {
            ctx.Fail<APM_AnnotScreen_BM>($"Invalid value {val}, allowed are: [fn:Deprecated(1.4,Compatible),Normal,Multiply,Screen,Difference,Darken,Lighten,ColorDodge,ColorBurn,Exclusion,HardLight,Overlay,SoftLight,Luminosity,Hue,Saturation,Color]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotScreen_Lang 
/// </summary>
internal partial class APM_AnnotScreen_Lang : APM_AnnotScreen_Lang__Base
{
}


internal partial class APM_AnnotScreen_Lang__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_Lang";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_AnnotScreen_Lang>(obj, "Lang", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotScreen_T 
/// </summary>
internal partial class APM_AnnotScreen_T : APM_AnnotScreen_T__Base
{
}


internal partial class APM_AnnotScreen_T__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_T";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_AnnotScreen_T>(obj, "T", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotScreen_MK 
/// </summary>
internal partial class APM_AnnotScreen_MK : APM_AnnotScreen_MK__Base
{
}


internal partial class APM_AnnotScreen_MK__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_MK";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_AnnotScreen_MK>(obj, "MK", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_AppearanceCharacteristics, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotScreen_A 
/// </summary>
internal partial class APM_AnnotScreen_A : APM_AnnotScreen_A__Base
{
}


internal partial class APM_AnnotScreen_A__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_A";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_AnnotScreen_A>(obj, "A", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_ActionGoTo.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_ActionGoTo, PdfDictionary>(stack, val, obj);
        } else if (APM_ActionGoToR.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_ActionGoToR, PdfDictionary>(stack, val, obj);
        } else if (APM_ActionLaunch.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_ActionLaunch, PdfDictionary>(stack, val, obj);
        } else if (APM_ActionThread.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_ActionThread, PdfDictionary>(stack, val, obj);
        } else if (APM_ActionURI.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_ActionURI, PdfDictionary>(stack, val, obj);
        } else if (APM_ActionSound.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_ActionSound, PdfDictionary>(stack, val, obj);
        } else if (APM_ActionMovie.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_ActionMovie, PdfDictionary>(stack, val, obj);
        } else if (APM_ActionHide.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_ActionHide, PdfDictionary>(stack, val, obj);
        } else if (APM_ActionNamed.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_ActionNamed, PdfDictionary>(stack, val, obj);
        } else if (APM_ActionSubmitForm.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_ActionSubmitForm, PdfDictionary>(stack, val, obj);
        } else if (APM_ActionResetForm.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_ActionResetForm, PdfDictionary>(stack, val, obj);
        } else if (APM_ActionImportData.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_ActionImportData, PdfDictionary>(stack, val, obj);
        } else if (APM_ActionSetOCGState.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_ActionSetOCGState, PdfDictionary>(stack, val, obj);
        } else if (APM_ActionRendition.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_ActionRendition, PdfDictionary>(stack, val, obj);
        } else if (APM_ActionTransition.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_ActionTransition, PdfDictionary>(stack, val, obj);
        } else if (APM_ActionECMAScript.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_ActionECMAScript, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 1.6m && APM_ActionGoToE.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_ActionGoToE, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 2.0m && APM_ActionGoToDp.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_ActionGoToDp, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 1.6m && APM_ActionGoTo3DView.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_ActionGoTo3DView, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 2.0m && APM_ActionRichMediaExecute.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_ActionRichMediaExecute, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_AnnotScreen_A>("A did not match any allowable types: '[ActionGoTo,ActionGoToR,fn:SinceVersion(1.6,ActionGoToE),fn:SinceVersion(2.0,ActionGoToDp),ActionLaunch,ActionThread,ActionURI,ActionSound,ActionMovie,ActionHide,ActionNamed,ActionSubmitForm,ActionResetForm,ActionImportData,ActionSetOCGState,ActionRendition,ActionTransition,fn:SinceVersion(1.6,ActionGoTo3DView),ActionECMAScript,fn:SinceVersion(2.0,ActionRichMediaExecute)]'");
        }
        
    }


}

/// <summary>
/// AnnotScreen_AA 
/// </summary>
internal partial class APM_AnnotScreen_AA : APM_AnnotScreen_AA__Base
{
}


internal partial class APM_AnnotScreen_AA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotScreen_AA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_AnnotScreen_AA>(obj, "AA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_AddActionScreenAnnotation, PdfDictionary>(stack, val, obj);
        
    }


}

