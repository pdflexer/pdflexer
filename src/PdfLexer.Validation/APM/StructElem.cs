// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_StructElem : APM_StructElem_Base
{
}

internal partial class APM_StructElem_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "StructElem";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_StructElem_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_StructElem_S, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_StructElem_P, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_StructElem_ID, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_StructElem_Ref, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_StructElem_Pg, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_StructElem_K, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_StructElem_A, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_StructElem_C, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_StructElem_R, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_StructElem_T, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_StructElem_Lang, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_StructElem_Alt, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_StructElem_E, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_StructElem_ActualText, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_StructElem_AF, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_StructElem_NS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_StructElem_PhoneticAlphabet, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_StructElem_Phoneme, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_StructElem>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_StructElem>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_StructElem>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_StructElem>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_StructElem>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_StructElem>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_StructElem>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_StructElem>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_StructElem_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "S", "P", "ID", "Pg", "K", "A", "C", "R", "T", "Alt"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "S", "P", "ID", "Pg", "K", "A", "C", "R", "T", "Lang", "Alt", "ActualText"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "S", "P", "ID", "Pg", "K", "A", "C", "R", "T", "Lang", "Alt", "E", "ActualText"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "S", "P", "ID", "Pg", "K", "A", "C", "R", "T", "Lang", "Alt", "E", "ActualText"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "S", "P", "ID", "Pg", "K", "A", "C", "R", "T", "Lang", "Alt", "E", "ActualText"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "S", "P", "ID", "Pg", "K", "A", "C", "R", "T", "Lang", "Alt", "E", "ActualText"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "S", "P", "ID", "Pg", "K", "A", "C", "R", "T", "Lang", "Alt", "E", "ActualText"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "S", "P", "ID", "Ref", "Pg", "K", "A", "C", "T", "Lang", "Alt", "E", "ActualText", "AF", "NS", "PhoneticAlphabet", "Phoneme"
    };
    


}

/// <summary>
/// StructElem_Type Table 355
/// </summary>
internal partial class APM_StructElem_Type : APM_StructElem_Type_Base
{
}


internal partial class APM_StructElem_Type_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "StructElem_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_StructElem_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "StructElem")) 
        {
            ctx.Fail<APM_StructElem_Type>($"Invalid value {val}, allowed are: [StructElem]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// StructElem_S 
/// </summary>
internal partial class APM_StructElem_S : APM_StructElem_S_Base
{
}


internal partial class APM_StructElem_S_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "StructElem_S";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_StructElem_S>(obj, "S", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// StructElem_P 
/// </summary>
internal partial class APM_StructElem_P : APM_StructElem_P_Base
{
}


internal partial class APM_StructElem_P_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "StructElem_P";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_StructElem_P>(obj, "P", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        if (APM_StructElem.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_StructElem, PdfDictionary>(stack, val, obj);
        } else if (APM_StructTreeRoot.MatchesType(ctx, val)) 
        {
            ctx.Run<APM_StructTreeRoot, PdfDictionary>(stack, val, obj);
        }else 
        {
            ctx.Fail<APM_StructElem_P>("P did not match any allowable types: '[StructElem,StructTreeRoot]'");
        }
        
    }


}

/// <summary>
/// StructElem_ID 
/// </summary>
internal partial class APM_StructElem_ID : APM_StructElem_ID_Base
{
}


internal partial class APM_StructElem_ID_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "StructElem_ID";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_StructElem_ID>(obj, "ID", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// StructElem_Ref 
/// </summary>
internal partial class APM_StructElem_Ref : APM_StructElem_Ref_Base
{
}


internal partial class APM_StructElem_Ref_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "StructElem_Ref";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_StructElem_Ref>(obj, "Ref", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfStructElem, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// StructElem_Pg 
/// </summary>
internal partial class APM_StructElem_Pg : APM_StructElem_Pg_Base
{
}


internal partial class APM_StructElem_Pg_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "StructElem_Pg";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_StructElem_Pg>(obj, "Pg", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_PageObject, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// StructElem_K 
/// </summary>
internal partial class APM_StructElem_K : APM_StructElem_K_Base
{
}


internal partial class APM_StructElem_K_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "StructElem_K";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_StructElem_K>(obj, "K", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_ArrayOfStructElemKids, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    if (APM_StructElem.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_StructElem, PdfDictionary>(stack, val, obj);
                    } else if (APM_MarkedContentReference.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_MarkedContentReference, PdfDictionary>(stack, val, obj);
                    } else if (APM_ObjectReference.MatchesType(ctx, val)) 
                    {
                        ctx.Run<APM_ObjectReference, PdfDictionary>(stack, val, obj);
                    }else 
                    {
                        ctx.Fail<APM_StructElem_K>("K did not match any allowable types: '[StructElem,MarkedContentReference,ObjectReference]'");
                    }
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
            
            default:
                ctx.Fail<APM_StructElem_K>("K is required to one of 'array;dictionary;integer', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// StructElem_A 
/// </summary>
internal partial class APM_StructElem_A : APM_StructElem_A_Base
{
}


internal partial class APM_StructElem_A_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "StructElem_A";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_StructElem_A>(obj, "A", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // TODO value checks array
                    ctx.Run<APM_ArrayOfAttributeRevisions, PdfArray>(stack, val, obj);
                    return;
                }
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_StructureAttributesDict, PdfDictionary>(stack, val, obj);
                    return;
                }
            case PdfObjectType.StreamObj:
                {
                    var val =  (PdfStream)utval;
                    if (!wasIR) { ctx.Fail<APM_StructElem_A>("A is required to be indirect when a stream"); return; }
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
                    return;
                }
            
            default:
                ctx.Fail<APM_StructElem_A>("A is required to one of 'array;dictionary;stream', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// StructElem_C 
/// </summary>
internal partial class APM_StructElem_C : APM_StructElem_C_Base
{
}


internal partial class APM_StructElem_C_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "StructElem_C";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_StructElem_C>(obj, "C", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_ArrayOfClassNamesRevisions, PdfArray>(stack, val, obj);
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
            
            default:
                ctx.Fail<APM_StructElem_C>("C is required to one of 'array;name', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// StructElem_R see https://github.com/pdf-association/pdf-issues/issues/93
/// </summary>
internal partial class APM_StructElem_R : APM_StructElem_R_Base
{
}


internal partial class APM_StructElem_R_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "StructElem_R";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_StructElem_R>(obj, "R", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        IPdfObject @R = val;
        if (!(gte(@R,0))) 
        {
            ctx.Fail<APM_StructElem_R>($"Invalid value {val}, allowed are: [fn:Eval(@R>=0)]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// StructElem_T 
/// </summary>
internal partial class APM_StructElem_T : APM_StructElem_T_Base
{
}


internal partial class APM_StructElem_T_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "StructElem_T";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_StructElem_T>(obj, "T", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// StructElem_Lang 
/// </summary>
internal partial class APM_StructElem_Lang : APM_StructElem_Lang_Base
{
}


internal partial class APM_StructElem_Lang_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "StructElem_Lang";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_StructElem_Lang>(obj, "Lang", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// StructElem_Alt 
/// </summary>
internal partial class APM_StructElem_Alt : APM_StructElem_Alt_Base
{
}


internal partial class APM_StructElem_Alt_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "StructElem_Alt";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_StructElem_Alt>(obj, "Alt", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// StructElem_E 
/// </summary>
internal partial class APM_StructElem_E : APM_StructElem_E_Base
{
}


internal partial class APM_StructElem_E_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "StructElem_E";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_StructElem_E>(obj, "E", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// StructElem_ActualText 
/// </summary>
internal partial class APM_StructElem_ActualText : APM_StructElem_ActualText_Base
{
}


internal partial class APM_StructElem_ActualText_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "StructElem_ActualText";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_StructElem_ActualText>(obj, "ActualText", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// StructElem_AF 
/// </summary>
internal partial class APM_StructElem_AF : APM_StructElem_AF_Base
{
}


internal partial class APM_StructElem_AF_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "StructElem_AF";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_StructElem_AF>(obj, "AF", IndirectRequirement.Either);
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
                ctx.Fail<APM_StructElem_AF>("AF is required to one of 'array;dictionary', was " + utval.Type);
                return;
        }
    }


}

/// <summary>
/// StructElem_NS 
/// </summary>
internal partial class APM_StructElem_NS : APM_StructElem_NS_Base
{
}


internal partial class APM_StructElem_NS_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "StructElem_NS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_StructElem_NS>(obj, "NS", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Namespace, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// StructElem_PhoneticAlphabet 
/// </summary>
internal partial class APM_StructElem_PhoneticAlphabet : APM_StructElem_PhoneticAlphabet_Base
{
}


internal partial class APM_StructElem_PhoneticAlphabet_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "StructElem_PhoneticAlphabet";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_StructElem_PhoneticAlphabet>(obj, "PhoneticAlphabet", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// StructElem_Phoneme 
/// </summary>
internal partial class APM_StructElem_Phoneme : APM_StructElem_Phoneme_Base
{
}


internal partial class APM_StructElem_Phoneme_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "StructElem_Phoneme";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_StructElem_Phoneme>(obj, "Phoneme", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

