// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_AnnotTrapNetwork : APM_AnnotTrapNetwork__Base
{
}

internal partial class APM_AnnotTrapNetwork__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "AnnotTrapNetwork";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_AnnotTrapNetwork_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_Rect, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_Contents, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_P, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_NM, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_M, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_AP, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_AS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_Border, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_C, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_StructParent, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_OC, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_AF, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_ca, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_CA, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_BM, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_Lang, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_LastModified, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_Version, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_AnnotStates, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_AnnotTrapNetwork_FontFauxing, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_AnnotTrapNetwork>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_AnnotTrapNetwork>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_AnnotTrapNetwork>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_AnnotTrapNetwork>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_AnnotTrapNetwork>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_AnnotTrapNetwork>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_AnnotTrapNetwork>($"Unknown field {extra} for version 1.9");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_AnnotTrapNetwork_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Rect", "Contents", "P", "M", "F", "AP", "AS", "Border", "C", "StructParent", "Version", "AnnotStates", "FontFauxing"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Rect", "Contents", "P", "NM", "M", "F", "AP", "AS", "Border", "C", "StructParent", "LastModified", "Version", "AnnotStates", "FontFauxing"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Rect", "Contents", "P", "NM", "M", "F", "AP", "AS", "Border", "C", "StructParent", "OC", "LastModified", "Version", "AnnotStates", "FontFauxing"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Rect", "Contents", "P", "NM", "M", "F", "AP", "AS", "Border", "C", "StructParent", "OC", "LastModified", "Version", "AnnotStates", "FontFauxing"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Rect", "Contents", "P", "NM", "M", "F", "AP", "AS", "Border", "C", "StructParent", "OC", "LastModified", "Version", "AnnotStates", "FontFauxing"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Rect", "Contents", "P", "NM", "M", "F", "AP", "AS", "Border", "C", "StructParent", "OC", "LastModified", "Version", "AnnotStates", "FontFauxing"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "Rect", "Contents", "P", "NM", "M", "F", "AP", "AS", "Border", "C", "StructParent", "OC", "LastModified", "Version", "AnnotStates", "FontFauxing"
    };
    


}

/// <summary>
/// AnnotTrapNetwork_Type Clause 12.5.6.21 and Table 403. NOT markup annot. 
/// </summary>
internal partial class APM_AnnotTrapNetwork_Type : APM_AnnotTrapNetwork_Type__Base
{
}


internal partial class APM_AnnotTrapNetwork_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_AnnotTrapNetwork_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "Annot")) 
        {
            ctx.Fail<APM_AnnotTrapNetwork_Type>($"Invalid value {val}, allowed are: [Annot]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotTrapNetwork_Subtype Clause 14.11.6
/// </summary>
internal partial class APM_AnnotTrapNetwork_Subtype : APM_AnnotTrapNetwork_Subtype__Base
{
}


internal partial class APM_AnnotTrapNetwork_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_AnnotTrapNetwork_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "TrapNet")) 
        {
            ctx.Fail<APM_AnnotTrapNetwork_Subtype>($"Invalid value {val}, allowed are: [TrapNet]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotTrapNetwork_Rect 
/// </summary>
internal partial class APM_AnnotTrapNetwork_Rect : APM_AnnotTrapNetwork_Rect__Base
{
}


internal partial class APM_AnnotTrapNetwork_Rect__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_Rect";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfArray, APM_AnnotTrapNetwork_Rect>(obj, "Rect", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotTrapNetwork_Contents 
/// </summary>
internal partial class APM_AnnotTrapNetwork_Contents : APM_AnnotTrapNetwork_Contents__Base
{
}


internal partial class APM_AnnotTrapNetwork_Contents__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_Contents";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_AnnotTrapNetwork_Contents>(obj, "Contents", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotTrapNetwork_P 
/// </summary>
internal partial class APM_AnnotTrapNetwork_P : APM_AnnotTrapNetwork_P__Base
{
}


internal partial class APM_AnnotTrapNetwork_P__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_P";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_AnnotTrapNetwork_P>(obj, "P", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_PageObject, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotTrapNetwork_NM 
/// </summary>
internal partial class APM_AnnotTrapNetwork_NM : APM_AnnotTrapNetwork_NM__Base
{
}


internal partial class APM_AnnotTrapNetwork_NM__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_NM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_AnnotTrapNetwork_NM>(obj, "NM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotTrapNetwork_M 
/// </summary>
internal partial class APM_AnnotTrapNetwork_M : APM_AnnotTrapNetwork_M__Base
{
}


internal partial class APM_AnnotTrapNetwork_M__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_M";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_AnnotTrapNetwork_M>(obj, "M", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            
            // TODO MC date;string
            
            default:
                ctx.Fail<APM_AnnotTrapNetwork_M>("M is required to one of 'date;string', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// AnnotTrapNetwork_F Table 167
/// </summary>
internal partial class APM_AnnotTrapNetwork_F : APM_AnnotTrapNetwork_F__Base
{
}


internal partial class APM_AnnotTrapNetwork_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_AnnotTrapNetwork_F>(obj, "F", IndirectRequirement.Either);
        if (val == null) { return; }
        
        if (!(BitsClear(obj)&&BitSet(obj)&&BitsClear(obj)&&BitSet(obj)&&BitsClear(obj))) 
        {
            ctx.Fail<APM_AnnotTrapNetwork_F>($"Value failed special case check: fn:Eval(fn:BitsClear(1,2) && fn:BitSet(3) && fn:BitsClear(4,6) && fn:BitSet(7) && fn:BitsClear(8,32))");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotTrapNetwork_AP 
/// </summary>
internal partial class APM_AnnotTrapNetwork_AP : APM_AnnotTrapNetwork_AP__Base
{
}


internal partial class APM_AnnotTrapNetwork_AP__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_AP";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_AnnotTrapNetwork_AP>(obj, "AP", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_AppearanceTrapNet, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotTrapNetwork_AS 
/// </summary>
internal partial class APM_AnnotTrapNetwork_AS : APM_AnnotTrapNetwork_AS__Base
{
}


internal partial class APM_AnnotTrapNetwork_AS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_AS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_AnnotTrapNetwork_AS>(obj, "AS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotTrapNetwork_Border 
/// </summary>
internal partial class APM_AnnotTrapNetwork_Border : APM_AnnotTrapNetwork_Border__Base
{
}


internal partial class APM_AnnotTrapNetwork_Border__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_Border";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_AnnotTrapNetwork_Border>(obj, "Border", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_4AnnotBorderCharacteristics, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotTrapNetwork_C 
/// </summary>
internal partial class APM_AnnotTrapNetwork_C : APM_AnnotTrapNetwork_C__Base
{
}


internal partial class APM_AnnotTrapNetwork_C__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_C";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_AnnotTrapNetwork_C>(obj, "C", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_4NumbersColorAnnotation, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotTrapNetwork_StructParent Table 359
/// </summary>
internal partial class APM_AnnotTrapNetwork_StructParent : APM_AnnotTrapNetwork_StructParent__Base
{
}


internal partial class APM_AnnotTrapNetwork_StructParent__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_StructParent";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_AnnotTrapNetwork_StructParent>(obj, "StructParent", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotTrapNetwork_OC 
/// </summary>
internal partial class APM_AnnotTrapNetwork_OC : APM_AnnotTrapNetwork_OC__Base
{
}


internal partial class APM_AnnotTrapNetwork_OC__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_OC";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_AnnotTrapNetwork_OC>(obj, "OC", IndirectRequirement.Either);
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
            ctx.Fail<APM_AnnotTrapNetwork_OC>("OC did not match any allowable types: '[OptContentGroup,OptContentMembership]'");
        }
        
    }


}

/// <summary>
/// AnnotTrapNetwork_AF 
/// </summary>
internal partial class APM_AnnotTrapNetwork_AF : APM_AnnotTrapNetwork_AF__Base
{
}


internal partial class APM_AnnotTrapNetwork_AF__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_AF";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_AnnotTrapNetwork_AF>(obj, "AF", IndirectRequirement.Either);
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
                ctx.Fail<APM_AnnotTrapNetwork_AF>("AF is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// AnnotTrapNetwork_ca 
/// </summary>
internal partial class APM_AnnotTrapNetwork_ca : APM_AnnotTrapNetwork_ca__Base
{
}


internal partial class APM_AnnotTrapNetwork_ca__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_ca";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_AnnotTrapNetwork_ca>(obj, "ca", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var ca = obj.Get("ca");
        if (!((gte(ca,0.0m)&&lte(ca,1.0m)))) 
        {
            ctx.Fail<APM_AnnotTrapNetwork_ca>($"Invalid value {val}, allowed are: [fn:Eval((@ca>=0.0) && (@ca<=1.0))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotTrapNetwork_CA 
/// </summary>
internal partial class APM_AnnotTrapNetwork_CA : APM_AnnotTrapNetwork_CA__Base
{
}


internal partial class APM_AnnotTrapNetwork_CA__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_CA";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfNumber, APM_AnnotTrapNetwork_CA>(obj, "CA", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var CA = obj.Get("CA");
        if (!((gte(CA,0.0m)&&lte(CA,1.0m)))) 
        {
            ctx.Fail<APM_AnnotTrapNetwork_CA>($"Invalid value {val}, allowed are: [fn:Eval((@CA>=0.0) && (@CA<=1.0))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotTrapNetwork_BM Table 134 and Table 135
/// </summary>
internal partial class APM_AnnotTrapNetwork_BM : APM_AnnotTrapNetwork_BM__Base
{
}


internal partial class APM_AnnotTrapNetwork_BM__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_BM";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_AnnotTrapNetwork_BM>(obj, "BM", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!((ctx.Version <= 1.4m && val == "Compatible") || val == "Normal" || val == "Multiply" || val == "Screen" || val == "Difference" || val == "Darken" || val == "Lighten" || val == "ColorDodge" || val == "ColorBurn" || val == "Exclusion" || val == "HardLight" || val == "Overlay" || val == "SoftLight" || val == "Luminosity" || val == "Hue" || val == "Saturation" || val == "Color")) 
        {
            ctx.Fail<APM_AnnotTrapNetwork_BM>($"Invalid value {val}, allowed are: [fn:Deprecated(1.4,Compatible),Normal,Multiply,Screen,Difference,Darken,Lighten,ColorDodge,ColorBurn,Exclusion,HardLight,Overlay,SoftLight,Luminosity,Hue,Saturation,Color]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// AnnotTrapNetwork_Lang 
/// </summary>
internal partial class APM_AnnotTrapNetwork_Lang : APM_AnnotTrapNetwork_Lang__Base
{
}


internal partial class APM_AnnotTrapNetwork_Lang__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_Lang";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_AnnotTrapNetwork_Lang>(obj, "Lang", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotTrapNetwork_LastModified 
/// </summary>
internal partial class APM_AnnotTrapNetwork_LastModified : APM_AnnotTrapNetwork_LastModified__Base
{
}


internal partial class APM_AnnotTrapNetwork_LastModified__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_LastModified";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfString, APM_AnnotTrapNetwork_LastModified>(obj, "LastModified", IndirectRequirement.Either);
        if (((!obj.ContainsKey("Version")&&!obj.ContainsKey("AnnotStates"))) && val == null) {
            ctx.Fail<APM_AnnotTrapNetwork_LastModified>("LastModified is required when 'fn:IsRequired(fn:Not(fn:IsPresent(Version)) && fn:Not(fn:IsPresent(AnnotStates)))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// AnnotTrapNetwork_Version 
/// </summary>
internal partial class APM_AnnotTrapNetwork_Version : APM_AnnotTrapNetwork_Version__Base
{
}


internal partial class APM_AnnotTrapNetwork_Version__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_Version";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfArray, APM_AnnotTrapNetwork_Version>(obj, "Version", IndirectRequirement.Either);
        if (((obj.ContainsKey("AnnotStates")&&!obj.ContainsKey(val))) && val == null) {
            ctx.Fail<APM_AnnotTrapNetwork_Version>("Version is required when 'fn:IsRequired(fn:IsPresent(AnnotStates) && fn:Not(fn:IsPresent(LastModified)))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfTrapNetVersionObjects, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotTrapNetwork_AnnotStates 
/// </summary>
internal partial class APM_AnnotTrapNetwork_AnnotStates : APM_AnnotTrapNetwork_AnnotStates__Base
{
}


internal partial class APM_AnnotTrapNetwork_AnnotStates__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_AnnotStates";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfArray, APM_AnnotTrapNetwork_AnnotStates>(obj, "AnnotStates", IndirectRequirement.Either);
        if (((obj.ContainsKey(val)&&!obj.ContainsKey("LastModified"))) && val == null) {
            ctx.Fail<APM_AnnotTrapNetwork_AnnotStates>("AnnotStates is required when 'fn:IsRequired(fn:IsPresent(Version) && fn:Not(fn:IsPresent(LastModified)))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfAnnotStates, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// AnnotTrapNetwork_FontFauxing 
/// </summary>
internal partial class APM_AnnotTrapNetwork_FontFauxing : APM_AnnotTrapNetwork_FontFauxing__Base
{
}


internal partial class APM_AnnotTrapNetwork_FontFauxing__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "AnnotTrapNetwork_FontFauxing";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_AnnotTrapNetwork_FontFauxing>(obj, "FontFauxing", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfFonts, PdfArray>(stack, val, obj);
        
    }


}

