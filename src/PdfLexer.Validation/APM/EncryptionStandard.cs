// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_EncryptionStandard : APM_EncryptionStandard__Base
{
}

internal partial class APM_EncryptionStandard__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "EncryptionStandard";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_EncryptionStandard_Filter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EncryptionStandard_SubFilter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EncryptionStandard_V, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EncryptionStandard_Length, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EncryptionStandard_CF, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EncryptionStandard_StmF, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EncryptionStandard_StrF, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EncryptionStandard_EFF, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EncryptionStandard_R, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EncryptionStandard_O, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EncryptionStandard_U, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EncryptionStandard_OE, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EncryptionStandard_UE, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EncryptionStandard_P, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EncryptionStandard_Perms, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EncryptionStandard_EncryptMetadata, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_EncryptionStandard_KDFSalt, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.1m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_11.Contains(x)))
                {
                    ctx.Fail<APM_EncryptionStandard>($"Unknown field {extra} for version 1.1");
                }
                break;
            case 1.2m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_12.Contains(x)))
                {
                    ctx.Fail<APM_EncryptionStandard>($"Unknown field {extra} for version 1.2");
                }
                break;
            case 1.3m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_13.Contains(x)))
                {
                    ctx.Fail<APM_EncryptionStandard>($"Unknown field {extra} for version 1.3");
                }
                break;
            case 1.4m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_14.Contains(x)))
                {
                    ctx.Fail<APM_EncryptionStandard>($"Unknown field {extra} for version 1.4");
                }
                break;
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_EncryptionStandard>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_EncryptionStandard>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_EncryptionStandard>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_EncryptionStandard>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_EncryptionStandard>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_EncryptionStandard>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        return false;
    }

    public static HashSet<string> AllowedFields_11 { get; } = new HashSet<string> 
    {
        "Filter", "V", "EFF", "R", "O", "U", "OE", "UE", "P", "Perms"
    };
    public static HashSet<string> AllowedFields_12 { get; } = new HashSet<string> 
    {
        "Filter", "V", "EFF", "R", "O", "U", "OE", "UE", "P", "Perms"
    };
    public static HashSet<string> AllowedFields_13 { get; } = new HashSet<string> 
    {
        "Filter", "SubFilter", "V", "EFF", "R", "O", "U", "OE", "UE", "P", "Perms"
    };
    public static HashSet<string> AllowedFields_14 { get; } = new HashSet<string> 
    {
        "Filter", "SubFilter", "V", "Length", "EFF", "R", "O", "U", "OE", "UE", "P", "Perms"
    };
    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Filter", "SubFilter", "V", "Length", "CF", "StmF", "StrF", "EFF", "R", "O", "U", "OE", "UE", "P", "Perms", "EncryptMetadata"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Filter", "SubFilter", "V", "Length", "CF", "StmF", "StrF", "EFF", "R", "O", "U", "OE", "UE", "P", "Perms", "EncryptMetadata"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Filter", "SubFilter", "V", "Length", "CF", "StmF", "StrF", "EFF", "R", "O", "U", "OE", "UE", "P", "Perms", "EncryptMetadata"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Filter", "SubFilter", "V", "Length", "CF", "StmF", "StrF", "EFF", "R", "O", "U", "OE", "UE", "P", "Perms", "EncryptMetadata"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Filter", "SubFilter", "V", "Length", "CF", "StmF", "StrF", "EFF", "R", "O", "U", "OE", "UE", "P", "Perms", "EncryptMetadata"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Filter", "SubFilter", "V", "CF", "StmF", "StrF", "EFF", "R", "O", "U", "OE", "UE", "P", "Perms", "EncryptMetadata", "KDFSalt"
    };
    


}

/// <summary>
/// EncryptionStandard_Filter Table 20 and Table 21
/// </summary>
internal partial class APM_EncryptionStandard_Filter : APM_EncryptionStandard_Filter__Base
{
}


internal partial class APM_EncryptionStandard_Filter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EncryptionStandard_Filter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_EncryptionStandard_Filter>(obj, "Filter", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "Standard")) 
        {
            ctx.Fail<APM_EncryptionStandard_Filter>($"Invalid value {val}, allowed are: [Standard]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// EncryptionStandard_SubFilter 
/// </summary>
internal partial class APM_EncryptionStandard_SubFilter : APM_EncryptionStandard_SubFilter__Base
{
}


internal partial class APM_EncryptionStandard_SubFilter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EncryptionStandard_SubFilter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_EncryptionStandard_SubFilter>(obj, "SubFilter", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// EncryptionStandard_V 
/// </summary>
internal partial class APM_EncryptionStandard_V : APM_EncryptionStandard_V__Base
{
}


internal partial class APM_EncryptionStandard_V__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EncryptionStandard_V";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_EncryptionStandard_V>(obj, "V", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!((ctx.Version < 1.1m && val == 0) || (ctx.Version < 2.0m && val == 1) || (ctx.Version < 2.0m && (ctx.Version >= 1.4m && val == 2)) || (ctx.Version < 2.0m && (ctx.Version >= 1.4m && val == 3)) || (ctx.Version < 2.0m && (ctx.Version >= 1.5m && val == 4)) || (ctx.Version == 1.7m && (ctx.Extensions.Contains("ADBE_Extn3") && val == 5)) || (ctx.Version >= 2.0m && val == 5) || (ctx.Version < 2.0m || (ctx.Extensions.Contains("ISO_TS_32003") && val == 6)))) 
        {
            ctx.Fail<APM_EncryptionStandard_V>($"Invalid value {val}, allowed are: [fn:Deprecated(1.1,0),fn:Deprecated(2.0,1),fn:Deprecated(2.0,fn:SinceVersion(1.4,2)),fn:Deprecated(2.0,fn:SinceVersion(1.4,3)),fn:Deprecated(2.0,fn:SinceVersion(1.5,4)),fn:IsPDFVersion(1.7,fn:Extension(ADBE_Extn3,5)),fn:SinceVersion(2.0,5),fn:SinceVersion(2.0,fn:Extension(ISO_TS_32003,6))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// EncryptionStandard_Length 
/// </summary>
internal partial class APM_EncryptionStandard_Length : APM_EncryptionStandard_Length__Base
{
}


internal partial class APM_EncryptionStandard_Length__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EncryptionStandard_Length";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.4m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_EncryptionStandard_Length>(obj, "Length", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:IsMeaningful, not pertinent to validation
        
        var Length = obj.Get("Length");
        if (!(gte(Length,40)&&(lte(Length,128)||(ctx.Extensions.Contains("ADBE_Extn3") && lte(Length,256)))&&eq(mod(Length,8),0))) 
        {
            ctx.Fail<APM_EncryptionStandard_Length>($"Invalid value {val}, allowed are: [fn:Eval((@Length>=40) && ((@Length<=128) || fn:Extension(ADBE_Extn3,(@Length<=256))) && ((@Length mod 8)==0))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// EncryptionStandard_CF 
/// </summary>
internal partial class APM_EncryptionStandard_CF : APM_EncryptionStandard_CF__Base
{
}


internal partial class APM_EncryptionStandard_CF__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EncryptionStandard_CF";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_EncryptionStandard_CF>(obj, "CF", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:IsMeaningful, not pertinent to validation
        // no value restrictions
        ctx.Run<APM_CryptFilterMap, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// EncryptionStandard_StmF 
/// </summary>
internal partial class APM_EncryptionStandard_StmF : APM_EncryptionStandard_StmF__Base
{
}


internal partial class APM_EncryptionStandard_StmF__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EncryptionStandard_StmF";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_EncryptionStandard_StmF>(obj, "StmF", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:IsMeaningful, not pertinent to validation
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// EncryptionStandard_StrF 
/// </summary>
internal partial class APM_EncryptionStandard_StrF : APM_EncryptionStandard_StrF__Base
{
}


internal partial class APM_EncryptionStandard_StrF__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EncryptionStandard_StrF";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_EncryptionStandard_StrF>(obj, "StrF", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:IsMeaningful, not pertinent to validation
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// EncryptionStandard_EFF 
/// </summary>
internal partial class APM_EncryptionStandard_EFF : APM_EncryptionStandard_EFF__Base
{
}


internal partial class APM_EncryptionStandard_EFF__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EncryptionStandard_EFF";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_EncryptionStandard_EFF>(obj, "EFF", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:IsMeaningful, not pertinent to validation
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// EncryptionStandard_R 
/// </summary>
internal partial class APM_EncryptionStandard_R : APM_EncryptionStandard_R__Base
{
}


internal partial class APM_EncryptionStandard_R__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EncryptionStandard_R";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_EncryptionStandard_R>(obj, "R", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        var V = obj.Get("V");
        // TODO required value checks
        if (!((ctx.Version < 2.0m && (ctx.Version == 1.7m && (ctx.Extensions.Contains("ADBE_Extn3") && val == 5))) || (ctx.Version < 2.0m && (ctx.Version >= 2.0m && val == 5)) || (ctx.Version < 2.0m || (ctx.Extensions.Contains("ISO_TS_32003") && val == 7)))) 
        {
            ctx.Fail<APM_EncryptionStandard_R>($"Invalid value {val}, allowed are: [fn:Deprecated(2.0,fn:RequiredValue(@V<2,2)),fn:Deprecated(2.0,fn:RequiredValue((@V==2) || (@V==3),3)),fn:Deprecated(2.0,fn:RequiredValue(@V==4,4)),fn:Deprecated(2.0,fn:IsPDFVersion(1.7,fn:Extension(ADBE_Extn3,5))),fn:Deprecated(2.0,fn:SinceVersion(2.0,5)),fn:SinceVersion(2.0,fn:RequiredValue(@V==5,6)),fn:SinceVersion(2.0,fn:Extension(ISO_TS_32003,7))]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// EncryptionStandard_O 32 bytes long if the value of R is 4 or less and 48 bytes long if the value of R is 6
/// </summary>
internal partial class APM_EncryptionStandard_O : APM_EncryptionStandard_O__Base
{
}


internal partial class APM_EncryptionStandard_O__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EncryptionStandard_O";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfString, APM_EncryptionStandard_O>(obj, "O", IndirectRequirement.Either);
        if (val == null) { return; }
        var R = obj.Get("R");
        if (!((AlwaysUnencrypted(obj)&&((lte(R,4)&&eq(StringLength(obj),32))||((eq(R,5)||eq(R,6))&&eq(StringLength(obj),48)))))) 
        {
            ctx.Fail<APM_EncryptionStandard_O>($"Value failed special case check: fn:Eval(fn:AlwaysUnencrypted() && (((@R<=4) && (fn:StringLength(O)==32)) || (((@R==5) || (@R==6)) && (fn:StringLength(O)==48))))");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// EncryptionStandard_U 32 bytes long if the value of R is 4 or less and 48 bytes long if the value of R is 6
/// </summary>
internal partial class APM_EncryptionStandard_U : APM_EncryptionStandard_U__Base
{
}


internal partial class APM_EncryptionStandard_U__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EncryptionStandard_U";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfString, APM_EncryptionStandard_U>(obj, "U", IndirectRequirement.Either);
        if (val == null) { return; }
        var R = obj.Get("R");
        if (!((AlwaysUnencrypted(obj)&&((lte(R,4)&&eq(StringLength(obj),32))||((eq(R,5)||eq(R,6))&&eq(StringLength(obj),48)))))) 
        {
            ctx.Fail<APM_EncryptionStandard_U>($"Value failed special case check: fn:Eval(fn:AlwaysUnencrypted() && (((@R<=4) && (fn:StringLength(U)==32)) || (((@R==5) || (@R==6)) && (fn:StringLength(U)==48))))");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// EncryptionStandard_OE 32 byte string
/// </summary>
internal partial class APM_EncryptionStandard_OE : APM_EncryptionStandard_OE__Base
{
}


internal partial class APM_EncryptionStandard_OE__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EncryptionStandard_OE";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var R = obj.Get("R");
        var val = ctx.GetOptional<PdfString, APM_EncryptionStandard_OE>(obj, "OE", IndirectRequirement.Either);
        if (((eq(R,5)||eq(R,6))) && val == null) {
            ctx.Fail<APM_EncryptionStandard_OE>("OE is required when 'fn:IsRequired((@R==5) || (@R==6))"); return;
        } else if (val == null) {
            return;
        }
        
        if (!((AlwaysUnencrypted(obj)&&eq(StringLength(obj),32)))) 
        {
            ctx.Fail<APM_EncryptionStandard_OE>($"Value failed special case check: fn:Eval(fn:AlwaysUnencrypted() && (fn:StringLength(OE)==32))");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// EncryptionStandard_UE 
/// </summary>
internal partial class APM_EncryptionStandard_UE : APM_EncryptionStandard_UE__Base
{
}


internal partial class APM_EncryptionStandard_UE__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EncryptionStandard_UE";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var R = obj.Get("R");
        var val = ctx.GetOptional<PdfString, APM_EncryptionStandard_UE>(obj, "UE", IndirectRequirement.Either);
        if (((eq(R,5)||eq(R,6))) && val == null) {
            ctx.Fail<APM_EncryptionStandard_UE>("UE is required when 'fn:IsRequired((@R==5) || (@R==6))"); return;
        } else if (val == null) {
            return;
        }
        
        if (!(AlwaysUnencrypted(obj))) 
        {
            ctx.Fail<APM_EncryptionStandard_UE>($"Value failed special case check: fn:Eval(fn:AlwaysUnencrypted())");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// EncryptionStandard_P Table 22
/// </summary>
internal partial class APM_EncryptionStandard_P : APM_EncryptionStandard_P__Base
{
}


internal partial class APM_EncryptionStandard_P__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EncryptionStandard_P";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfIntNumber, APM_EncryptionStandard_P>(obj, "P", IndirectRequirement.Either);
        if (val == null) { return; }
        
        if (!(BitsClear(val,0b00000000000000000000000000000011)&&BitsSet(val,0b00000000000000000000000011000000)&&(ctx.Version < 2.0m || BitsSet(val,0b00000000000000000000001000000000))&&((ctx.Version < 2.0m || (ctx.Extensions.Contains("ISO_TS_32004") && BitsSet(val,0b11111111111111111110000000000000)))||BitsSet(val,0b11111111111111111111000000000000)))) 
        {
            ctx.Fail<APM_EncryptionStandard_P>($"Value failed special case check: fn:Eval(fn:BitsClear(1,2) && fn:BitsSet(7,8) && fn:SinceVersion(2.0,fn:BitSet(10)) && (fn:SinceVersion(2.0,fn:Extension(ISO_TS_32004,fn:BitsSet(14,32))) || fn:BitsSet(13,32)))");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// EncryptionStandard_Perms 
/// </summary>
internal partial class APM_EncryptionStandard_Perms : APM_EncryptionStandard_Perms__Base
{
}


internal partial class APM_EncryptionStandard_Perms__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EncryptionStandard_Perms";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var R = obj.Get("R");
        var val = ctx.GetOptional<PdfString, APM_EncryptionStandard_Perms>(obj, "Perms", IndirectRequirement.Either);
        if (((eq(R,5)||eq(R,6))) && val == null) {
            ctx.Fail<APM_EncryptionStandard_Perms>("Perms is required when 'fn:IsRequired((@R==5) || (@R==6))"); return;
        } else if (val == null) {
            return;
        }
        
        if (!(AlwaysUnencrypted(obj))) 
        {
            ctx.Fail<APM_EncryptionStandard_Perms>($"Value failed special case check: fn:Eval(fn:AlwaysUnencrypted())");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// EncryptionStandard_EncryptMetadata 
/// </summary>
internal partial class APM_EncryptionStandard_EncryptMetadata : APM_EncryptionStandard_EncryptMetadata__Base
{
}


internal partial class APM_EncryptionStandard_EncryptMetadata__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EncryptionStandard_EncryptMetadata";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_EncryptionStandard_EncryptMetadata>(obj, "EncryptMetadata", IndirectRequirement.Either);
        if (val == null) { return; }
        // special case is an fn:IsMeaningful, not pertinent to validation
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// EncryptionStandard_KDFSalt ISO/TS 32004 integrity protection
/// </summary>
internal partial class APM_EncryptionStandard_KDFSalt : APM_EncryptionStandard_KDFSalt__Base
{
}


internal partial class APM_EncryptionStandard_KDFSalt__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "EncryptionStandard_KDFSalt";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_EncryptionStandard_KDFSalt>(obj, "KDFSalt", IndirectRequirement.MustBeDirect);
        if (val == null) { return; }
        
        if (!(AlwaysUnencrypted(obj))) 
        {
            ctx.Fail<APM_EncryptionStandard_KDFSalt>($"Value failed special case check: fn:Eval(fn:AlwaysUnencrypted())");
        }
        // no value restrictions
        // no linked objects
        
    }


}

