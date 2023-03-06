// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_SigFieldSeedValue : APM_SigFieldSeedValue__Base
{
}

internal partial class APM_SigFieldSeedValue__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "SigFieldSeedValue";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_SigFieldSeedValue_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SigFieldSeedValue_Ff, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SigFieldSeedValue_Filter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SigFieldSeedValue_SubFilter, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SigFieldSeedValue_DigestMethod, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SigFieldSeedValue_V, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SigFieldSeedValue_Cert, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SigFieldSeedValue_Reasons, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SigFieldSeedValue_MDP, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SigFieldSeedValue_TimeStamp, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SigFieldSeedValue_LegalAttestation, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SigFieldSeedValue_AddRevInfo, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SigFieldSeedValue_LockDocument, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_SigFieldSeedValue_AppearanceFilter, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_SigFieldSeedValue>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_SigFieldSeedValue>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_SigFieldSeedValue>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_SigFieldSeedValue>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_SigFieldSeedValue>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_SigFieldSeedValue>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_SigFieldSeedValue_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_15 { get; } = new HashSet<string> 
    {
        "Type", "Filter", "SubFilter", "V", "Cert", "Reasons"
    };
    public static HashSet<string> AllowedFields_16 { get; } = new HashSet<string> 
    {
        "Type", "Filter", "SubFilter", "V", "Cert", "Reasons", "MDP", "TimeStamp", "LegalAttestation"
    };
    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "Ff", "Filter", "SubFilter", "DigestMethod", "V", "Cert", "Reasons", "MDP", "TimeStamp", "LegalAttestation", "AddRevInfo"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "Ff", "Filter", "SubFilter", "DigestMethod", "V", "Cert", "Reasons", "MDP", "TimeStamp", "LegalAttestation", "AddRevInfo"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "Ff", "Filter", "SubFilter", "DigestMethod", "V", "Cert", "Reasons", "MDP", "TimeStamp", "LegalAttestation", "AddRevInfo"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "Ff", "Filter", "SubFilter", "DigestMethod", "V", "Cert", "Reasons", "MDP", "TimeStamp", "LegalAttestation", "AddRevInfo"
    };
    


}

/// <summary>
/// SigFieldSeedValue_Type Table 237
/// </summary>
internal partial class APM_SigFieldSeedValue_Type : APM_SigFieldSeedValue_Type__Base
{
}


internal partial class APM_SigFieldSeedValue_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SigFieldSeedValue_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_SigFieldSeedValue_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "SV")) 
        {
            ctx.Fail<APM_SigFieldSeedValue_Type>($"Invalid value {val}, allowed are: [SV]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// SigFieldSeedValue_Ff 
/// </summary>
internal partial class APM_SigFieldSeedValue_Ff : APM_SigFieldSeedValue_Ff__Base
{
}


internal partial class APM_SigFieldSeedValue_Ff__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SigFieldSeedValue_Ff";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_SigFieldSeedValue_Ff>(obj, "Ff", IndirectRequirement.Either);
        if (val == null) { return; }
        
        if (!(((ctx.Version >= 2.0m || BitsClear(val,0b11111111111111111111111110000000))&&BitsClear(val,0b11111111111111111111111000000000)))) 
        {
            ctx.Fail<APM_SigFieldSeedValue_Ff>($"Value failed special case check: fn:Eval(fn:BeforeVersion(2.0,fn:BitsClear(8,32)) && fn:BitsClear(10,32))");
        }
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// SigFieldSeedValue_Filter 
/// </summary>
internal partial class APM_SigFieldSeedValue_Filter : APM_SigFieldSeedValue_Filter__Base
{
}


internal partial class APM_SigFieldSeedValue_Filter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SigFieldSeedValue_Filter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_SigFieldSeedValue_Filter>(obj, "Filter", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// SigFieldSeedValue_SubFilter 
/// </summary>
internal partial class APM_SigFieldSeedValue_SubFilter : APM_SigFieldSeedValue_SubFilter__Base
{
}


internal partial class APM_SigFieldSeedValue_SubFilter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SigFieldSeedValue_SubFilter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_SigFieldSeedValue_SubFilter>(obj, "SubFilter", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfSignatureSubFilterNames, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// SigFieldSeedValue_DigestMethod https://github.com/pdf-association/pdf-issues/issues/159
/// </summary>
internal partial class APM_SigFieldSeedValue_DigestMethod : APM_SigFieldSeedValue_DigestMethod__Base
{
}


internal partial class APM_SigFieldSeedValue_DigestMethod__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SigFieldSeedValue_DigestMethod";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_SigFieldSeedValue_DigestMethod>(obj, "DigestMethod", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfDigestMethod, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// SigFieldSeedValue_V 
/// </summary>
internal partial class APM_SigFieldSeedValue_V : APM_SigFieldSeedValue_V__Base
{
}


internal partial class APM_SigFieldSeedValue_V__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SigFieldSeedValue_V";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_SigFieldSeedValue_V>(obj, "V", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == 0 || val == 1 || val == 2 || val == 3)) 
        {
            ctx.Fail<APM_SigFieldSeedValue_V>($"Invalid value {val}, allowed are: [0,1,2,3]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// SigFieldSeedValue_Cert 
/// </summary>
internal partial class APM_SigFieldSeedValue_Cert : APM_SigFieldSeedValue_Cert__Base
{
}


internal partial class APM_SigFieldSeedValue_Cert__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SigFieldSeedValue_Cert";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_SigFieldSeedValue_Cert>(obj, "Cert", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_CertSeedValue, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// SigFieldSeedValue_Reasons 
/// </summary>
internal partial class APM_SigFieldSeedValue_Reasons : APM_SigFieldSeedValue_Reasons__Base
{
}


internal partial class APM_SigFieldSeedValue_Reasons__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SigFieldSeedValue_Reasons";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_SigFieldSeedValue_Reasons>(obj, "Reasons", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfStringsText, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// SigFieldSeedValue_MDP 
/// </summary>
internal partial class APM_SigFieldSeedValue_MDP : APM_SigFieldSeedValue_MDP__Base
{
}


internal partial class APM_SigFieldSeedValue_MDP__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SigFieldSeedValue_MDP";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_SigFieldSeedValue_MDP>(obj, "MDP", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_MDPDict, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// SigFieldSeedValue_TimeStamp 
/// </summary>
internal partial class APM_SigFieldSeedValue_TimeStamp : APM_SigFieldSeedValue_TimeStamp__Base
{
}


internal partial class APM_SigFieldSeedValue_TimeStamp__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SigFieldSeedValue_TimeStamp";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_SigFieldSeedValue_TimeStamp>(obj, "TimeStamp", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_TimeStampDict, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// SigFieldSeedValue_LegalAttestation 
/// </summary>
internal partial class APM_SigFieldSeedValue_LegalAttestation : APM_SigFieldSeedValue_LegalAttestation__Base
{
}


internal partial class APM_SigFieldSeedValue_LegalAttestation__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SigFieldSeedValue_LegalAttestation";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.6m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfArray, APM_SigFieldSeedValue_LegalAttestation>(obj, "LegalAttestation", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_ArrayOfStringsText, PdfArray>(stack, val, obj);
        
    }


}

/// <summary>
/// SigFieldSeedValue_AddRevInfo 
/// </summary>
internal partial class APM_SigFieldSeedValue_AddRevInfo : APM_SigFieldSeedValue_AddRevInfo__Base
{
}


internal partial class APM_SigFieldSeedValue_AddRevInfo__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SigFieldSeedValue_AddRevInfo";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_SigFieldSeedValue_AddRevInfo>(obj, "AddRevInfo", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// SigFieldSeedValue_LockDocument 
/// </summary>
internal partial class APM_SigFieldSeedValue_LockDocument : APM_SigFieldSeedValue_LockDocument__Base
{
}


internal partial class APM_SigFieldSeedValue_LockDocument__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SigFieldSeedValue_LockDocument";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_SigFieldSeedValue_LockDocument>(obj, "LockDocument", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == "true" || val == "false" || val == "auto")) 
        {
            ctx.Fail<APM_SigFieldSeedValue_LockDocument>($"Invalid value {val}, allowed are: [true,false,auto]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// SigFieldSeedValue_AppearanceFilter 
/// </summary>
internal partial class APM_SigFieldSeedValue_AppearanceFilter : APM_SigFieldSeedValue_AppearanceFilter__Base
{
}


internal partial class APM_SigFieldSeedValue_AppearanceFilter__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "SigFieldSeedValue_AppearanceFilter";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_SigFieldSeedValue_AppearanceFilter>(obj, "AppearanceFilter", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

