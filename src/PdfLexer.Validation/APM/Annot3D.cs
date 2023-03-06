// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_Annot3D : APM_Annot3D__Base
{
}

internal partial class APM_Annot3D__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "Annot3D";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_Annot3D_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_Rect, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_Contents, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_P, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_NM, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_M, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_AP, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_AS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_Border, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_C, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_StructParent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_OC, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_AF, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_ca, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_CA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_BM, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_Lang, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_3DD, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_3DV, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_3DA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_3DI, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_3DB, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_3DU, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_Annot3D_GEO, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_Annot3D>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_Annot3D>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_Annot3D>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_Annot3D>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_Annot3D>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_Annot3D_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Rect", "Contents", "P", "NM", "M", "F", "AP", "AS", "Border", "C", "StructParent", "OC", "3DD", "3DV", "3DA", "3DI", "3DB"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Rect", "Contents", "P", "NM", "M", "F", "AP", "AS", "Border", "C", "StructParent", "OC", "3DD", "3DV", "3DA", "3DI", "3DB"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Rect", "Contents", "P", "NM", "M", "F", "AP", "AS", "Border", "C", "StructParent", "OC", "3DD", "3DV", "3DA", "3DI", "3DB"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Rect", "Contents", "P", "NM", "M", "F", "AP", "AS", "Border", "C", "StructParent", "OC", "3DD", "3DV", "3DA", "3DI", "3DB"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Rect", "Contents", "P", "NM", "M", "F", "AP", "AS", "Border", "C", "StructParent", "OC", "AF", "ca", "CA", "BM", "Lang", "3DD", "3DV", "3DA", "3DI", "3DB", "GEO"
    };
    


}

/// <summary>
/// Annot3D_Type Table 166 and Table 309 (NOT markup annot)
/// </summary>
internal partial class APM_Annot3D_Type : APM_Annot3D_Type__Base
{
}


internal partial class APM_Annot3D_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_Annot3D_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "Annot")) 
        {
            ctx.Fail<APM_Annot3D_Type>($"Invalid value {val}, allowed are: [Annot]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// Annot3D_Subtype 
/// </summary>
internal partial class APM_Annot3D_Subtype : APM_Annot3D_Subtype__Base
{
}


internal partial class APM_Annot3D_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_Annot3D_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "3D")) 
        {
            ctx.Fail<APM_Annot3D_Subtype>($"Invalid value {val}, allowed are: [3D]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// Annot3D_Rect 
/// </summary>
internal partial class APM_Annot3D_Rect : APM_Annot3D_Rect__Base
{
}


internal partial class APM_Annot3D_Rect__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_Rect";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_Annot3D_Rect>(obj, "Rect", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Annot3D_Contents 
/// </summary>
internal partial class APM_Annot3D_Contents : APM_Annot3D_Contents__Base
{
}


internal partial class APM_Annot3D_Contents__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_Contents";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_Annot3D_Contents>(obj, "Contents", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Annot3D_P 
/// </summary>
internal partial class APM_Annot3D_P : APM_Annot3D_P__Base
{
}


internal partial class APM_Annot3D_P__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_P";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Annot3D_P>(obj, "P", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_PageObject, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// Annot3D_NM 
/// </summary>
internal partial class APM_Annot3D_NM : APM_Annot3D_NM__Base
{
}


internal partial class APM_Annot3D_NM__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_NM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_Annot3D_NM>(obj, "NM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Annot3D_M 
/// </summary>
internal partial class APM_Annot3D_M : APM_Annot3D_M__Base
{
}


internal partial class APM_Annot3D_M__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_M";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_Annot3D_M>(obj, "M", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            
            // TODO MC date;string-text
            
            default:
                ctx.Fail<APM_Annot3D_M>("M is required to one of 'date;string-text', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// Annot3D_F Table 167
/// </summary>
internal partial class APM_Annot3D_F : APM_Annot3D_F__Base
{
}


internal partial class APM_Annot3D_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_Annot3D_F>(obj, "F", IndirectRequirement.Either);
        if (val == null) { return; }
        
        if (!(((ctx.Version >= 1.7m || BitsClear(val,0b11111111111111111111111000000000))&&BitsClear(val,0b11111111111111111111110000000000)))) 
        {
            ctx.Fail<APM_Annot3D_F>($"Value failed special case check: fn:Eval(fn:BeforeVersion(1.7,fn:BitsClear(10,32)) && fn:BitsClear(11,32))");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Annot3D_AP 
/// </summary>
internal partial class APM_Annot3D_AP : APM_Annot3D_AP__Base
{
}


internal partial class APM_Annot3D_AP__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_AP";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var Rect = obj.Get("Rect");
        var val = ctx.GetOptional<PdfDictionary, APM_Annot3D_AP>(obj, "AP", IndirectRequirement.Either);
        if (((ctx.Version < 2.0m || (gt(RectWidth(obj),0)||gt(RectHeight(Rect),0)))) && val == null) {
            ctx.Fail<APM_Annot3D_AP>("AP is required when 'fn:IsRequired(fn:SinceVersion(2.0,(fn:RectWidth(Rect)>0) || (fn:RectHeight(Rect)>0)))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Appearance, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// Annot3D_AS 
/// </summary>
internal partial class APM_Annot3D_AS : APM_Annot3D_AS__Base
{
}


internal partial class APM_Annot3D_AS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_AS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var APN = obj.Get("AP")?.Get("N");
        var APR = obj.Get("AP")?.Get("R");
        var APD = obj.Get("AP")?.Get("D");
        var val = ctx.GetOptional<PdfName, APM_Annot3D_AS>(obj, "AS", IndirectRequirement.Either);
        if (((APN != null)||(APR != null)||(APD != null)) && val == null) {
            ctx.Fail<APM_Annot3D_AS>("AS is required when 'fn:IsRequired(fn:IsPresent(AP::N::*) || fn:IsPresent(AP::R::*) || fn:IsPresent(AP::D::*))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Annot3D_Border 
/// </summary>
internal partial class APM_Annot3D_Border : APM_Annot3D_Border__Base
{
}


internal partial class APM_Annot3D_Border__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_Border";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_Annot3D_Border>(obj, "Border", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_4AnnotBorderCharacteristics, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// Annot3D_C 
/// </summary>
internal partial class APM_Annot3D_C : APM_Annot3D_C__Base
{
}


internal partial class APM_Annot3D_C__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_C";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_Annot3D_C>(obj, "C", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_4NumbersColorAnnotation, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// Annot3D_StructParent Table 359
/// </summary>
internal partial class APM_Annot3D_StructParent : APM_Annot3D_StructParent__Base
{
}


internal partial class APM_Annot3D_StructParent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_StructParent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_Annot3D_StructParent>(obj, "StructParent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Annot3D_OC 
/// </summary>
internal partial class APM_Annot3D_OC : APM_Annot3D_OC__Base
{
}


internal partial class APM_Annot3D_OC__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_OC";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Annot3D_OC>(obj, "OC", IndirectRequirement.Either);
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
            ctx.Fail<APM_Annot3D_OC>("OC did not match any allowable types: '[OptContentGroup,OptContentMembership]'");
        }
        
    }


}

/// <summary>
/// Annot3D_AF 
/// </summary>
internal partial class APM_Annot3D_AF : APM_Annot3D_AF__Base
{
}


internal partial class APM_Annot3D_AF__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_AF";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_Annot3D_AF>(obj, "AF", IndirectRequirement.Either);
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
                ctx.Fail<APM_Annot3D_AF>("AF is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// Annot3D_ca 
/// </summary>
internal partial class APM_Annot3D_ca : APM_Annot3D_ca__Base
{
}


internal partial class APM_Annot3D_ca__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_ca";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_Annot3D_ca>(obj, "ca", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var ca = obj.Get("ca");
        if (!((gte(ca,0.0m)&&lte(ca,1.0m)))) 
        {
            ctx.Fail<APM_Annot3D_ca>($"Invalid value {val}, allowed are: [fn:Eval((@ca>=0.0) && (@ca<=1.0))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// Annot3D_CA 
/// </summary>
internal partial class APM_Annot3D_CA : APM_Annot3D_CA__Base
{
}


internal partial class APM_Annot3D_CA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_CA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_Annot3D_CA>(obj, "CA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var CA = obj.Get("CA");
        if (!((gte(CA,0.0m)&&lte(CA,1.0m)))) 
        {
            ctx.Fail<APM_Annot3D_CA>($"Invalid value {val}, allowed are: [fn:Eval((@CA>=0.0) && (@CA<=1.0))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// Annot3D_BM Table 134 and Table 135
/// </summary>
internal partial class APM_Annot3D_BM : APM_Annot3D_BM__Base
{
}


internal partial class APM_Annot3D_BM__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_BM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_Annot3D_BM>(obj, "BM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!((ctx.Version < 1.4m && val == "Compatible") || val == "Normal" || val == "Multiply" || val == "Screen" || val == "Difference" || val == "Darken" || val == "Lighten" || val == "ColorDodge" || val == "ColorBurn" || val == "Exclusion" || val == "HardLight" || val == "Overlay" || val == "SoftLight" || val == "Luminosity" || val == "Hue" || val == "Saturation" || val == "Color")) 
        {
            ctx.Fail<APM_Annot3D_BM>($"Invalid value {val}, allowed are: [fn:Deprecated(1.4,Compatible),Normal,Multiply,Screen,Difference,Darken,Lighten,ColorDodge,ColorBurn,Exclusion,HardLight,Overlay,SoftLight,Luminosity,Hue,Saturation,Color]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// Annot3D_Lang 
/// </summary>
internal partial class APM_Annot3D_Lang : APM_Annot3D_Lang__Base
{
}


internal partial class APM_Annot3D_Lang__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_Lang";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_Annot3D_Lang>(obj, "Lang", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Annot3D_3DD 
/// </summary>
internal partial class APM_Annot3D_3DD : APM_Annot3D_3DD__Base
{
}


internal partial class APM_Annot3D_3DD__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_3DD";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_Annot3D_3DD>(obj, "3DD", IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_Annot3D_3DD>("3DD is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_3DReference, PdfDictionary>(stack, val, obj);
                    return;
                }
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_Annot3D_3DD>("3DD is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_3DStream, PdfDictionary>(stack, val.Dictionary, obj);
                    return;
                }
            
            default:
                ctx.Fail<APM_Annot3D_3DD>("3DD is required to one of 'dictionary;stream', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// Annot3D_3DV 
/// </summary>
internal partial class APM_Annot3D_3DV : APM_Annot3D_3DV__Base
{
}


internal partial class APM_Annot3D_3DV__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_3DV";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_Annot3D_3DV>(obj, "3DV", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_3DView, PdfDictionary>(stack, val, obj);
                    return;
                }
            case PdfObjectType.NumericObj:
                {
                    var val =  (PdfIntNumber)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
                    return;
                }
            case PdfObjectType.NameObj:
                {
                    var val =  (PdfName)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    // no linked objects
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
                ctx.Fail<APM_Annot3D_3DV>("3DV is required to one of 'dictionary;integer;name;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// Annot3D_3DA 
/// </summary>
internal partial class APM_Annot3D_3DA : APM_Annot3D_3DA__Base
{
}


internal partial class APM_Annot3D_3DA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_3DA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Annot3D_3DA>(obj, "3DA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_3DActivation, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// Annot3D_3DI 
/// </summary>
internal partial class APM_Annot3D_3DI : APM_Annot3D_3DI__Base
{
}


internal partial class APM_Annot3D_3DI__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_3DI";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_Annot3D_3DI>(obj, "3DI", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Annot3D_3DB 
/// </summary>
internal partial class APM_Annot3D_3DB : APM_Annot3D_3DB__Base
{
}


internal partial class APM_Annot3D_3DB__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_3DB";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_Annot3D_3DB>(obj, "3DB", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// Annot3D_3DU 
/// </summary>
internal partial class APM_Annot3D_3DU : APM_Annot3D_3DU__Base
{
}


internal partial class APM_Annot3D_3DU__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_3DU";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Annot3D_3DU>(obj, "3DU", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_3DUnits, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// Annot3D_GEO 
/// </summary>
internal partial class APM_Annot3D_GEO : APM_Annot3D_GEO__Base
{
}


internal partial class APM_Annot3D_GEO__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "Annot3D_GEO";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_Annot3D_GEO>(obj, "GEO", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_MeasureGEO, PdfDictionary>(stack, val, obj);
        
    }


}

