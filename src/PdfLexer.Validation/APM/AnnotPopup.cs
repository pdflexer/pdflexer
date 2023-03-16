// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_AnnotPopup : APM_AnnotPopup__Base
{
}

internal partial class APM_AnnotPopup__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "AnnotPopup";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_AnnotPopup_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotPopup_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotPopup_Rect, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotPopup_Contents, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotPopup_P, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotPopup_NM, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotPopup_M, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotPopup_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotPopup_AP, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotPopup_AS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotPopup_Border, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotPopup_C, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotPopup_StructParent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotPopup_OC, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotPopup_AF, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotPopup_ca, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotPopup_CA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotPopup_BM, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotPopup_Lang, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotPopup_Parent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotPopup_Open, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_AnnotPopup>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_AnnotPopup>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            case 1.5m:
            case 1.6m:
            case 1.7m:
            case 1.8m:
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15_16_17_18_19.Contains(x)))
                {
                    ctx.Fail<APM_AnnotPopup>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_AnnotPopup>($"Unknown field {extra} for version {ctx.Version}");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_AnnotPopup_Type, PdfDictionary>(new CallStack(), obj, null);
        c.Run<APM_AnnotPopup_Subtype, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "AP", "AS", "Border", "C", "Contents", "F", "M", "Open", "P", "Parent", "Rect", "StructParent", "Subtype", "Type"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "AP", "AS", "Border", "C", "Contents", "F", "M", "NM", "Open", "P", "Parent", "Rect", "StructParent", "Subtype", "Type"
    };
    public static HashSet<string> AllowedFields_15_16_17_18_19 { get; } = new HashSet<string> 
    {
        "AP", "AS", "Border", "C", "Contents", "F", "M", "NM", "OC", "Open", "P", "Parent", "Rect", "StructParent", "Subtype", "Type"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "AF", "AP", "AS", "BM", "Border", "C", "ca", "CA", "Contents", "F", "Lang", "M", "NM", "OC", "Open", "P", "Parent", "Rect", "StructParent", "Subtype", "Type"
    };
    


}

/// <summary>
/// AnnotPopup_Type Table 166 and Table 186 (NOT markup annot)
/// </summary>
internal partial class APM_AnnotPopup_Type : APM_AnnotPopup_Type__Base
{
}


internal partial class APM_AnnotPopup_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_AnnotPopup_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Annot)) 
        {
            ctx.Fail<APM_AnnotPopup_Type>($"Invalid value {val}, allowed are: [Annot]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotPopup_Subtype 
/// </summary>
internal partial class APM_AnnotPopup_Subtype : APM_AnnotPopup_Subtype__Base
{
}


internal partial class APM_AnnotPopup_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfName, APM_AnnotPopup_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Popup)) 
        {
            ctx.Fail<APM_AnnotPopup_Subtype>($"Invalid value {val}, allowed are: [Popup]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotPopup_Rect 
/// </summary>
internal partial class APM_AnnotPopup_Rect : APM_AnnotPopup_Rect__Base
{
}


internal partial class APM_AnnotPopup_Rect__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_Rect";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetRequired<PdfArray, APM_AnnotPopup_Rect>(obj, "Rect", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotPopup_Contents 
/// </summary>
internal partial class APM_AnnotPopup_Contents : APM_AnnotPopup_Contents__Base
{
}


internal partial class APM_AnnotPopup_Contents__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_Contents";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_AnnotPopup_Contents>(obj, "Contents", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotPopup_P 
/// </summary>
internal partial class APM_AnnotPopup_P : APM_AnnotPopup_P__Base
{
}


internal partial class APM_AnnotPopup_P__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_P";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_AnnotPopup_P>(obj, "P", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_PageObject, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotPopup_NM 
/// </summary>
internal partial class APM_AnnotPopup_NM : APM_AnnotPopup_NM__Base
{
}


internal partial class APM_AnnotPopup_NM__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_NM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_AnnotPopup_NM>(obj, "NM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotPopup_M 
/// </summary>
internal partial class APM_AnnotPopup_M : APM_AnnotPopup_M__Base
{
}


internal partial class APM_AnnotPopup_M__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_M";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_AnnotPopup_M>(obj, "M", IndirectRequirement.Either);
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
                ctx.Fail<APM_AnnotPopup_M>("M is required to one of 'date;string-text', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// AnnotPopup_F Table 167
/// </summary>
internal partial class APM_AnnotPopup_F : APM_AnnotPopup_F__Base
{
}


internal partial class APM_AnnotPopup_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfIntNumber, APM_AnnotPopup_F>(obj, "F", IndirectRequirement.Either);
        if (val == null) { return; }
        
        if (!((ctx.Version >= 1.4m || BitsClear(val,0b11111111111111111111111110000000))&&(ctx.Version >= 1.5m || BitsClear(val,0b11111111111111111111111100000000))&&(ctx.Version >= 1.6m || BitsClear(val,0b11111111111111111111111000000000))&&(ctx.Version >= 1.7m && BitsClear(val,0b11111111111111111111110000000000)))) 
        {
            ctx.Fail<APM_AnnotPopup_F>($"Value failed special case check: fn:Eval(fn:BeforeVersion(1.4,fn:BitsClear(8,32)) && fn:BeforeVersion(1.5,fn:BitsClear(9,32)) && fn:BeforeVersion(1.6,fn:BitsClear(10,32)) && fn:SinceVersion(1.7,fn:BitsClear(11,32)))");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotPopup_AP 
/// </summary>
internal partial class APM_AnnotPopup_AP : APM_AnnotPopup_AP__Base
{
}


internal partial class APM_AnnotPopup_AP__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_AP";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_AnnotPopup_AP>(obj, "AP", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Appearance, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotPopup_AS 
/// </summary>
internal partial class APM_AnnotPopup_AS : APM_AnnotPopup_AS__Base
{
}


internal partial class APM_AnnotPopup_AS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_AS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var APN = obj.Get("AP")?.Get("N");
        var APR = obj.Get("AP")?.Get("R");
        var APD = obj.Get("AP")?.Get("D");
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_AnnotPopup_AS>(obj, "AS", IndirectRequirement.Either);
        if (((APN != null)||(APR != null)||(APD != null)) && val == null) {
            ctx.Fail<APM_AnnotPopup_AS>("AS is required when 'fn:IsRequired(fn:IsPresent(AP::N::*) || fn:IsPresent(AP::R::*) || fn:IsPresent(AP::D::*))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotPopup_Border 
/// </summary>
internal partial class APM_AnnotPopup_Border : APM_AnnotPopup_Border__Base
{
}


internal partial class APM_AnnotPopup_Border__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_Border";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_AnnotPopup_Border>(obj, "Border", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_4AnnotBorderCharacteristics, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotPopup_C 
/// </summary>
internal partial class APM_AnnotPopup_C : APM_AnnotPopup_C__Base
{
}


internal partial class APM_AnnotPopup_C__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_C";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfArray, APM_AnnotPopup_C>(obj, "C", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_4NumbersColorAnnotation, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotPopup_StructParent Table 359
/// </summary>
internal partial class APM_AnnotPopup_StructParent : APM_AnnotPopup_StructParent__Base
{
}


internal partial class APM_AnnotPopup_StructParent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_StructParent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfIntNumber, APM_AnnotPopup_StructParent>(obj, "StructParent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotPopup_OC 
/// </summary>
internal partial class APM_AnnotPopup_OC : APM_AnnotPopup_OC__Base
{
}


internal partial class APM_AnnotPopup_OC__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_OC";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_AnnotPopup_OC>(obj, "OC", IndirectRequirement.Either);
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
            ctx.Fail<APM_AnnotPopup_OC>("OC did not match any allowable types: '[OptContentGroup,OptContentMembership]'");
        }
        
    }


}

/// <summary>
/// AnnotPopup_AF 
/// </summary>
internal partial class APM_AnnotPopup_AF : APM_AnnotPopup_AF__Base
{
}


internal partial class APM_AnnotPopup_AF__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_AF";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_AnnotPopup_AF>(obj, "AF", IndirectRequirement.Either);
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
                ctx.Fail<APM_AnnotPopup_AF>("AF is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// AnnotPopup_ca 
/// </summary>
internal partial class APM_AnnotPopup_ca : APM_AnnotPopup_ca__Base
{
}


internal partial class APM_AnnotPopup_ca__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_ca";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfNumber, APM_AnnotPopup_ca>(obj, "ca", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var ca = obj.Get("ca");
        if (!((gte(ca,0.0m)&&lte(ca,1.0m)))) 
        {
            ctx.Fail<APM_AnnotPopup_ca>($"Invalid value {val}, allowed are: [fn:Eval((@ca>=0.0) && (@ca<=1.0))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotPopup_CA 
/// </summary>
internal partial class APM_AnnotPopup_CA : APM_AnnotPopup_CA__Base
{
}


internal partial class APM_AnnotPopup_CA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_CA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfNumber, APM_AnnotPopup_CA>(obj, "CA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var CA = obj.Get("CA");
        if (!((gte(CA,0.0m)&&lte(CA,1.0m)))) 
        {
            ctx.Fail<APM_AnnotPopup_CA>($"Invalid value {val}, allowed are: [fn:Eval((@CA>=0.0) && (@CA<=1.0))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotPopup_BM Table 134 and Table 135
/// </summary>
internal partial class APM_AnnotPopup_BM : APM_AnnotPopup_BM__Base
{
}


internal partial class APM_AnnotPopup_BM__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_BM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfName, APM_AnnotPopup_BM>(obj, "BM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!((ctx.Version < 1.4m && val == PdfName.Compatible) || val == PdfName.Normal || val == PdfName.Multiply || val == PdfName.Screen || val == PdfName.Difference || val == PdfName.Darken || val == PdfName.Lighten || val == PdfName.ColorDodge || val == PdfName.ColorBurn || val == PdfName.Exclusion || val == PdfName.HardLight || val == PdfName.Overlay || val == PdfName.SoftLight || val == PdfName.Luminosity || val == PdfName.Hue || val == PdfName.Saturation || val == PdfName.Color)) 
        {
            ctx.Fail<APM_AnnotPopup_BM>($"Invalid value {val}, allowed are: [fn:Deprecated(1.4,Compatible),Normal,Multiply,Screen,Difference,Darken,Lighten,ColorDodge,ColorBurn,Exclusion,HardLight,Overlay,SoftLight,Luminosity,Hue,Saturation,Color]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotPopup_Lang 
/// </summary>
internal partial class APM_AnnotPopup_Lang : APM_AnnotPopup_Lang__Base
{
}


internal partial class APM_AnnotPopup_Lang__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_Lang";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfString, APM_AnnotPopup_Lang>(obj, "Lang", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotPopup_Parent 
/// </summary>
internal partial class APM_AnnotPopup_Parent : APM_AnnotPopup_Parent__Base
{
}


internal partial class APM_AnnotPopup_Parent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_Parent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_AnnotPopup_Parent>(obj, "Parent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_AnnotText.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_AnnotText, PdfDictionary>(stack, val, obj);
        } else if (APM_AnnotLink.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_AnnotLink, PdfDictionary>(stack, val, obj);
        } else if (APM_AnnotFreeText.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_AnnotFreeText, PdfDictionary>(stack, val, obj);
        } else if (APM_AnnotLine.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_AnnotLine, PdfDictionary>(stack, val, obj);
        } else if (APM_AnnotSquare.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_AnnotSquare, PdfDictionary>(stack, val, obj);
        } else if (APM_AnnotCircle.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_AnnotCircle, PdfDictionary>(stack, val, obj);
        } else if (APM_AnnotHighlight.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_AnnotHighlight, PdfDictionary>(stack, val, obj);
        } else if (APM_AnnotUnderline.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_AnnotUnderline, PdfDictionary>(stack, val, obj);
        } else if (APM_AnnotStrikeOut.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_AnnotStrikeOut, PdfDictionary>(stack, val, obj);
        } else if (APM_AnnotStamp.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_AnnotStamp, PdfDictionary>(stack, val, obj);
        } else if (APM_AnnotInk.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_AnnotInk, PdfDictionary>(stack, val, obj);
        } else if (APM_AnnotPopup.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_AnnotPopup, PdfDictionary>(stack, val, obj);
        } else if (APM_AnnotFileAttachment.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_AnnotFileAttachment, PdfDictionary>(stack, val, obj);
        } else if (APM_AnnotSound.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_AnnotSound, PdfDictionary>(stack, val, obj);
        } else if (APM_AnnotMovie.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_AnnotMovie, PdfDictionary>(stack, val, obj);
        } else if (APM_AnnotWidget.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_AnnotWidget, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 1.5m && APM_AnnotPolygon.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_AnnotPolygon, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 1.4m && APM_AnnotSquiggly.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_AnnotSquiggly, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 1.5m && APM_AnnotCaret.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_AnnotCaret, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 1.5m && APM_AnnotScreen.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_AnnotScreen, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 1.4m && APM_AnnotPrinterMark.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_AnnotPrinterMark, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 1.6m && APM_AnnotWatermark.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_AnnotWatermark, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 1.6m && APM_Annot3D.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_Annot3D, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 1.7m && APM_AnnotRedact.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_AnnotRedact, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 2.0m && APM_AnnotProjection.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_AnnotProjection, PdfDictionary>(stack, val, obj);
        } else if ((ctx.Version >= 2.0m && APM_AnnotRichMedia.MatchesType(ctx, val))) 
        {
            ctx.Run<APM_AnnotRichMedia, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_AnnotPopup_Parent>("Parent did not match any allowable types: '[AnnotText,AnnotLink,AnnotFreeText,AnnotLine,AnnotSquare,AnnotCircle,fn:SinceVersion(1.5,AnnotPolygon),AnnotHighlight,AnnotUnderline,fn:SinceVersion(1.4,AnnotSquiggly),AnnotStrikeOut,fn:SinceVersion(1.5,AnnotCaret),AnnotStamp,AnnotInk,AnnotPopup,AnnotFileAttachment,AnnotSound,AnnotMovie,fn:SinceVersion(1.5,AnnotScreen),AnnotWidget,fn:SinceVersion(1.4,AnnotPrinterMark),fn:SinceVersion(1.6,AnnotWatermark),fn:SinceVersion(1.6,Annot3D),fn:SinceVersion(1.7,AnnotRedact),fn:SinceVersion(2.0,AnnotProjection),fn:SinceVersion(2.0,AnnotRichMedia)]'");
        }
        
    }


}

/// <summary>
/// AnnotPopup_Open 
/// </summary>
internal partial class APM_AnnotPopup_Open : APM_AnnotPopup_Open__Base
{
}


internal partial class APM_AnnotPopup_Open__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotPopup_Open";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (val, wasIR) = ctx.GetOptional<PdfBoolean, APM_AnnotPopup_Open>(obj, "Open", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

