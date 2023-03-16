// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_AnnotMovie : APM_AnnotMovie__Base
{
}

internal partial class APM_AnnotMovie__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "AnnotMovie";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_AnnotMovie_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_Rect, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_Contents, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_P, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_NM, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_M, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_AP, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_AS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_Border, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_C, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_StructParent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_OC, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_AF, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_ca, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_CA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_BM, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_Lang, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_T, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_Movie, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotMovie_A, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_AnnotMovie>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_AnnotMovie>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_AnnotMovie>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            case 1.5m:
            case 1.6m:
            case 1.7m:
            case 1.8m:
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15_16_17_18_19.Contains(x)))
                {
                    ctx.Fail<APM_AnnotMovie>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_AnnotMovie_Type, PdfDictionary>(new CallStack(), obj, null);
        c.Run<APM_AnnotMovie_Subtype, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "A", "AP", "AS", "Border", "C", "Contents", "F", "M", "Movie", "Rect", "Subtype", "T", "Type"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "A", "AP", "AS", "Border", "C", "Contents", "F", "M", "Movie", "P", "Rect", "StructParent", "Subtype", "T", "Type"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "A", "AP", "AS", "Border", "C", "Contents", "F", "M", "Movie", "NM", "P", "Rect", "StructParent", "Subtype", "T", "Type"
    };
    public static HashSet<string> AllowedFields_15_16_17_18_19 { get; } = new HashSet<string> 
    {
        "A", "AP", "AS", "Border", "C", "Contents", "F", "M", "Movie", "NM", "OC", "P", "Rect", "StructParent", "Subtype", "T", "Type"
    };
    


}

/// <summary>
/// AnnotMovie_Type Table 166 and Table 189 (NOT markup annot)
/// </summary>
internal partial class APM_AnnotMovie_Type : APM_AnnotMovie_Type__Base
{
}


internal partial class APM_AnnotMovie_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_AnnotMovie_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Annot)) 
        {
            ctx.Fail<APM_AnnotMovie_Type>($"Invalid value {val}, allowed are: [Annot]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotMovie_Subtype 
/// </summary>
internal partial class APM_AnnotMovie_Subtype : APM_AnnotMovie_Subtype__Base
{
}


internal partial class APM_AnnotMovie_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfName, APM_AnnotMovie_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Movie)) 
        {
            ctx.Fail<APM_AnnotMovie_Subtype>($"Invalid value {val}, allowed are: [Movie]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotMovie_Rect 
/// </summary>
internal partial class APM_AnnotMovie_Rect : APM_AnnotMovie_Rect__Base
{
}


internal partial class APM_AnnotMovie_Rect__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_Rect";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfArray, APM_AnnotMovie_Rect>(obj, "Rect", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotMovie_Contents 
/// </summary>
internal partial class APM_AnnotMovie_Contents : APM_AnnotMovie_Contents__Base
{
}


internal partial class APM_AnnotMovie_Contents__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_Contents";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_AnnotMovie_Contents>(obj, "Contents", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotMovie_P 
/// </summary>
internal partial class APM_AnnotMovie_P : APM_AnnotMovie_P__Base
{
}


internal partial class APM_AnnotMovie_P__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_P";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_AnnotMovie_P>(obj, "P", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_PageObject, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotMovie_NM 
/// </summary>
internal partial class APM_AnnotMovie_NM : APM_AnnotMovie_NM__Base
{
}


internal partial class APM_AnnotMovie_NM__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_NM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_AnnotMovie_NM>(obj, "NM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotMovie_M 
/// </summary>
internal partial class APM_AnnotMovie_M : APM_AnnotMovie_M__Base
{
}


internal partial class APM_AnnotMovie_M__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_M";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_AnnotMovie_M>(obj, "M", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.StringObj:
                {
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
                ctx.Fail<APM_AnnotMovie_M>("M is required to one of 'date;string-text', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// AnnotMovie_F Table 167
/// </summary>
internal partial class APM_AnnotMovie_F : APM_AnnotMovie_F__Base
{
}


internal partial class APM_AnnotMovie_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfIntNumber, APM_AnnotMovie_F>(obj, "F", IndirectRequirement.Either);
        if (val == null) { return; }
        
        if (!((ctx.Version >= 1.3m || BitsClear(val,0b11111111111111111111111111111000))&&(ctx.Version >= 1.4m || BitsClear(val,0b11111111111111111111111110000000))&&(ctx.Version >= 1.5m || BitsClear(val,0b11111111111111111111111100000000))&&(ctx.Version >= 1.6m || BitsClear(val,0b11111111111111111111111000000000))&&(ctx.Version >= 1.7m && BitsClear(val,0b11111111111111111111110000000000)))) 
        {
            ctx.Fail<APM_AnnotMovie_F>($"Value failed special case check: fn:Eval(fn:BeforeVersion(1.3,fn:BitsClear(4,32)) && fn:BeforeVersion(1.4,fn:BitsClear(8,32)) && fn:BeforeVersion(1.5,fn:BitsClear(9,32)) && fn:BeforeVersion(1.6,fn:BitsClear(10,32)) && fn:SinceVersion(1.7,fn:BitsClear(11,32)))");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotMovie_AP 
/// </summary>
internal partial class APM_AnnotMovie_AP : APM_AnnotMovie_AP__Base
{
}


internal partial class APM_AnnotMovie_AP__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_AP";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var Rect = obj.Get("Rect");
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_AnnotMovie_AP>(obj, "AP", IndirectRequirement.Either);
        if (((ctx.Version >= 2.0m && (gt(RectWidth(Rect),0)||gt(RectHeight(Rect),0)))) && val == null) {
            ctx.Fail<APM_AnnotMovie_AP>("AP is required when 'fn:IsRequired(fn:SinceVersion(2.0,(fn:RectWidth(Rect)>0) || (fn:RectHeight(Rect)>0)))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Appearance, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotMovie_AS 
/// </summary>
internal partial class APM_AnnotMovie_AS : APM_AnnotMovie_AS__Base
{
}


internal partial class APM_AnnotMovie_AS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_AS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var APN = obj.Get("AP")?.Get("N");
        var APR = obj.Get("AP")?.Get("R");
        var APD = obj.Get("AP")?.Get("D");
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_AnnotMovie_AS>(obj, "AS", IndirectRequirement.Either);
        if (((APN != null)||(APR != null)||(APD != null)) && val == null) {
            ctx.Fail<APM_AnnotMovie_AS>("AS is required when 'fn:IsRequired(fn:IsPresent(AP::N::*) || fn:IsPresent(AP::R::*) || fn:IsPresent(AP::D::*))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotMovie_Border 
/// </summary>
internal partial class APM_AnnotMovie_Border : APM_AnnotMovie_Border__Base
{
}


internal partial class APM_AnnotMovie_Border__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_Border";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_AnnotMovie_Border>(obj, "Border", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_4AnnotBorderCharacteristics, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotMovie_C 
/// </summary>
internal partial class APM_AnnotMovie_C : APM_AnnotMovie_C__Base
{
}


internal partial class APM_AnnotMovie_C__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_C";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_AnnotMovie_C>(obj, "C", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_4NumbersColorAnnotation, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotMovie_StructParent Table 359
/// </summary>
internal partial class APM_AnnotMovie_StructParent : APM_AnnotMovie_StructParent__Base
{
}


internal partial class APM_AnnotMovie_StructParent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_StructParent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfIntNumber, APM_AnnotMovie_StructParent>(obj, "StructParent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotMovie_OC 
/// </summary>
internal partial class APM_AnnotMovie_OC : APM_AnnotMovie_OC__Base
{
}


internal partial class APM_AnnotMovie_OC__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_OC";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_AnnotMovie_OC>(obj, "OC", IndirectRequirement.Either);
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
            ctx.Fail<APM_AnnotMovie_OC>("OC did not match any allowable types: '[OptContentGroup,OptContentMembership]'");
        }
        
    }


}

/// <summary>
/// AnnotMovie_AF 
/// </summary>
internal partial class APM_AnnotMovie_AF : APM_AnnotMovie_AF__Base
{
}


internal partial class APM_AnnotMovie_AF__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_AF";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_AnnotMovie_AF>(obj, "AF", IndirectRequirement.Either);
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
                ctx.Fail<APM_AnnotMovie_AF>("AF is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// AnnotMovie_ca 
/// </summary>
internal partial class APM_AnnotMovie_ca : APM_AnnotMovie_ca__Base
{
}


internal partial class APM_AnnotMovie_ca__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_ca";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfNumber, APM_AnnotMovie_ca>(obj, "ca", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var ca = obj.Get("ca");
        if (!((gte(ca,0.0m)&&lte(ca,1.0m)))) 
        {
            ctx.Fail<APM_AnnotMovie_ca>($"Invalid value {val}, allowed are: [fn:Eval((@ca>=0.0) && (@ca<=1.0))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotMovie_CA 
/// </summary>
internal partial class APM_AnnotMovie_CA : APM_AnnotMovie_CA__Base
{
}


internal partial class APM_AnnotMovie_CA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_CA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfNumber, APM_AnnotMovie_CA>(obj, "CA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var CA = obj.Get("CA");
        if (!((gte(CA,0.0m)&&lte(CA,1.0m)))) 
        {
            ctx.Fail<APM_AnnotMovie_CA>($"Invalid value {val}, allowed are: [fn:Eval((@CA>=0.0) && (@CA<=1.0))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotMovie_BM Table 134 and Table 135
/// </summary>
internal partial class APM_AnnotMovie_BM : APM_AnnotMovie_BM__Base
{
}


internal partial class APM_AnnotMovie_BM__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_BM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_AnnotMovie_BM>(obj, "BM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!((ctx.Version < 1.4m && val == PdfName.Compatible) || val == PdfName.Normal || val == PdfName.Multiply || val == PdfName.Screen || val == PdfName.Difference || val == PdfName.Darken || val == PdfName.Lighten || val == PdfName.ColorDodge || val == PdfName.ColorBurn || val == PdfName.Exclusion || val == PdfName.HardLight || val == PdfName.Overlay || val == PdfName.SoftLight || val == PdfName.Luminosity || val == PdfName.Hue || val == PdfName.Saturation || val == PdfName.Color)) 
        {
            ctx.Fail<APM_AnnotMovie_BM>($"Invalid value {val}, allowed are: [fn:Deprecated(1.4,Compatible),Normal,Multiply,Screen,Difference,Darken,Lighten,ColorDodge,ColorBurn,Exclusion,HardLight,Overlay,SoftLight,Luminosity,Hue,Saturation,Color]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotMovie_Lang 
/// </summary>
internal partial class APM_AnnotMovie_Lang : APM_AnnotMovie_Lang__Base
{
}


internal partial class APM_AnnotMovie_Lang__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_Lang";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_AnnotMovie_Lang>(obj, "Lang", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotMovie_T 
/// </summary>
internal partial class APM_AnnotMovie_T : APM_AnnotMovie_T__Base
{
}


internal partial class APM_AnnotMovie_T__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_T";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_AnnotMovie_T>(obj, "T", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotMovie_Movie 
/// </summary>
internal partial class APM_AnnotMovie_Movie : APM_AnnotMovie_Movie__Base
{
}


internal partial class APM_AnnotMovie_Movie__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_Movie";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfDictionary, APM_AnnotMovie_Movie>(obj, "Movie", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Movie, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotMovie_A 
/// </summary>
internal partial class APM_AnnotMovie_A : APM_AnnotMovie_A__Base
{
}


internal partial class APM_AnnotMovie_A__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotMovie_A";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_AnnotMovie_A>(obj, "A", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.BooleanObj:
                {
                    var val =  (PdfBoolean)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
                    return;
                }
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_MovieActivation, PdfDictionary>(stack, val, obj);
                    return;
                }
            
            default:
                ctx.Fail<APM_AnnotMovie_A>("A is required to one of 'boolean;dictionary', was " + utval.Type);
                return;
        }
    }


}

