// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_FileSpecification : APM_FileSpecification__Base
{
}

internal partial class APM_FileSpecification__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "FileSpecification";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_FileSpecification_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FileSpecification_FS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FileSpecification_F, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FileSpecification_UF, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FileSpecification_DOS, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FileSpecification_Mac, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FileSpecification_Unix, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FileSpecification_ID, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FileSpecification_V, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FileSpecification_EF, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FileSpecification_RF, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FileSpecification_Desc, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FileSpecification_CI, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FileSpecification_Thumb, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FileSpecification_AFRelationship, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_FileSpecification_EP, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.1m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_11.Contains(x)))
                {
                    ctx.Fail<APM_FileSpecification>($"Unknown field {extra} for version 1.1");
                }
                break;
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_FileSpecification>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_FileSpecification>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_FileSpecification>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_FileSpecification>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_FileSpecification>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_FileSpecification>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_FileSpecification>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_FileSpecification>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_FileSpecification>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_FileSpecification_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_11 { get; } = new HashSet<string> 
    {
        "Type", "FS", "F", "DOS", "Mac", "Unix", "ID"
    };
    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Type", "FS", "F", "DOS", "Mac", "Unix", "ID", "V"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Type", "FS", "F", "DOS", "Mac", "Unix", "ID", "V", "EF", "RF"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Type", "FS", "F", "DOS", "Mac", "Unix", "ID", "V", "EF", "RF"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "FS", "F", "DOS", "Mac", "Unix", "ID", "V", "EF", "RF"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "FS", "F", "DOS", "Mac", "Unix", "ID", "V", "EF", "RF", "Desc"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "FS", "F", "UF", "DOS", "Mac", "Unix", "ID", "V", "EF", "RF", "Desc", "CI", "Thumb"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "FS", "F", "UF", "DOS", "Mac", "Unix", "ID", "V", "EF", "RF", "Desc", "CI", "Thumb"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "FS", "F", "UF", "DOS", "Mac", "Unix", "ID", "V", "EF", "RF", "Desc", "CI", "Thumb"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "FS", "F", "UF", "ID", "V", "EF", "RF", "Desc", "CI", "Thumb", "AFRelationship", "EP"
    };
    


}

/// <summary>
/// FileSpecification_Type Table 43
/// </summary>
internal partial class APM_FileSpecification_Type : APM_FileSpecification_Type__Base
{
}


internal partial class APM_FileSpecification_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FileSpecification_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfName, APM_FileSpecification_Type>(obj, "Type", IndirectRequirement.Either);
        if ((obj.ContainsKey(PdfName.EF)||obj.ContainsKey(PdfName.EP)||obj.ContainsKey(PdfName.RF)) && val == null) {
            ctx.Fail<APM_FileSpecification_Type>("Type is required when 'fn:IsRequired(fn:IsPresent(EF) || fn:IsPresent(EP) || fn:IsPresent(RF))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        
        
        if (!(val == PdfName.Filespec)) 
        {
            ctx.Fail<APM_FileSpecification_Type>($"Invalid value {val}, allowed are: [Filespec]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FileSpecification_FS 
/// </summary>
internal partial class APM_FileSpecification_FS : APM_FileSpecification_FS__Base
{
}


internal partial class APM_FileSpecification_FS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FileSpecification_FS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_FileSpecification_FS>(obj, "FS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restictions
        // no linked objects
        
    }


}

/// <summary>
/// FileSpecification_F 
/// </summary>
internal partial class APM_FileSpecification_F : APM_FileSpecification_F__Base
{
}


internal partial class APM_FileSpecification_F__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FileSpecification_F";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfString, APM_FileSpecification_F>(obj, "F", IndirectRequirement.Either);
        if ((!obj.ContainsKey(PdfName.DOS)&&!obj.ContainsKey(PdfName.Mac)&&!obj.ContainsKey(PdfName.Unix)) && val == null) {
            ctx.Fail<APM_FileSpecification_F>("F is required when 'fn:IsRequired(fn:Not(fn:IsPresent(DOS)) && fn:Not(fn:IsPresent(Mac)) && fn:Not(fn:IsPresent(Unix)))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FileSpecification_UF 
/// </summary>
internal partial class APM_FileSpecification_UF : APM_FileSpecification_UF__Base
{
}


internal partial class APM_FileSpecification_UF__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FileSpecification_UF";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FileSpecification_UF>(obj, "UF", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FileSpecification_DOS 
/// </summary>
internal partial class APM_FileSpecification_DOS : APM_FileSpecification_DOS__Base
{
}


internal partial class APM_FileSpecification_DOS__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FileSpecification_DOS";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FileSpecification_DOS>(obj, "DOS", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FileSpecification_Mac 
/// </summary>
internal partial class APM_FileSpecification_Mac : APM_FileSpecification_Mac__Base
{
}


internal partial class APM_FileSpecification_Mac__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FileSpecification_Mac";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FileSpecification_Mac>(obj, "Mac", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FileSpecification_Unix 
/// </summary>
internal partial class APM_FileSpecification_Unix : APM_FileSpecification_Unix__Base
{
}


internal partial class APM_FileSpecification_Unix__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FileSpecification_Unix";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FileSpecification_Unix>(obj, "Unix", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FileSpecification_ID 
/// </summary>
internal partial class APM_FileSpecification_ID : APM_FileSpecification_ID__Base
{
}


internal partial class APM_FileSpecification_ID__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FileSpecification_ID";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_FileSpecification_ID>(obj, "ID", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOf_2StringsByte, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// FileSpecification_V 
/// </summary>
internal partial class APM_FileSpecification_V : APM_FileSpecification_V__Base
{
}


internal partial class APM_FileSpecification_V__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FileSpecification_V";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_FileSpecification_V>(obj, "V", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FileSpecification_EF 
/// </summary>
internal partial class APM_FileSpecification_EF : APM_FileSpecification_EF__Base
{
}


internal partial class APM_FileSpecification_EF__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FileSpecification_EF";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        var val = ctx.GetOptional<PdfDictionary, APM_FileSpecification_EF>(obj, "EF", IndirectRequirement.Either);
        if ((obj.ContainsKey(PdfName.RF)) && val == null) {
            ctx.Fail<APM_FileSpecification_EF>("EF is required when 'fn:IsRequired(fn:IsPresent(RF))"); return;
        } else if (val == null) {
            return;
        }
        // no special cases
        // no value restrictions
        ctx.Run<APM_FileSpecEF, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// FileSpecification_RF 
/// </summary>
internal partial class APM_FileSpecification_RF : APM_FileSpecification_RF__Base
{
}


internal partial class APM_FileSpecification_RF__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FileSpecification_RF";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_FileSpecification_RF>(obj, "RF", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_FileSpecRF, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// FileSpecification_Desc 
/// </summary>
internal partial class APM_FileSpecification_Desc : APM_FileSpecification_Desc__Base
{
}


internal partial class APM_FileSpecification_Desc__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FileSpecification_Desc";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_FileSpecification_Desc>(obj, "Desc", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// FileSpecification_CI 
/// </summary>
internal partial class APM_FileSpecification_CI : APM_FileSpecification_CI__Base
{
}


internal partial class APM_FileSpecification_CI__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FileSpecification_CI";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_FileSpecification_CI>(obj, "CI", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_CollectionItem, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// FileSpecification_Thumb 
/// </summary>
internal partial class APM_FileSpecification_Thumb : APM_FileSpecification_Thumb__Base
{
}


internal partial class APM_FileSpecification_Thumb__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FileSpecification_Thumb";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfStream, APM_FileSpecification_Thumb>(obj, "Thumb", IndirectRequirement.MustBeIndirect);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_Thumbnail, PdfDictionary>(stack, val.Dictionary, obj);
        
    }


}

/// <summary>
/// FileSpecification_AFRelationship 
/// </summary>
internal partial class APM_FileSpecification_AFRelationship : APM_FileSpecification_AFRelationship__Base
{
}


internal partial class APM_FileSpecification_AFRelationship__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FileSpecification_AFRelationship";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_FileSpecification_AFRelationship>(obj, "AFRelationship", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Source || val == PdfName.Data || val == PdfName.Alternative || val == PdfName.Supplement || val == PdfName.EncryptedPayload || val == PdfName.FormData || val == PdfName.Schema || val == PdfName.Unspecified)) 
        {
            ctx.Fail<APM_FileSpecification_AFRelationship>($"Invalid value {val}, allowed are: [Source,Data,Alternative,Supplement,EncryptedPayload,FormData,Schema,Unspecified]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// FileSpecification_EP 
/// </summary>
internal partial class APM_FileSpecification_EP : APM_FileSpecification_EP__Base
{
}


internal partial class APM_FileSpecification_EP__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "FileSpecification_EP";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_FileSpecification_EP>(obj, "EP", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_EncryptedPayload, PdfDictionary>(stack, val, obj);
        
    }


}
