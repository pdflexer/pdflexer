// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_CollectionField : APM_CollectionField__Base
{
}

internal partial class APM_CollectionField__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "CollectionField";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_CollectionField_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CollectionField_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CollectionField_N, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CollectionField_O, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CollectionField_V, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_CollectionField_E, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_CollectionField>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_CollectionField>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_CollectionField>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_CollectionField>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_CollectionField_Type, PdfDictionary>(new CallStack(), obj, null);
        c.Run<APM_CollectionField_Subtype, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static HashSet<string> AllowedFields_17 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "N", "O", "V", "E"
    };
    public static HashSet<string> AllowedFields_18 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "N", "O", "V", "E"
    };
    public static HashSet<string> AllowedFields_19 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "N", "O", "V", "E"
    };
    public static HashSet<string> AllowedFields_20 { get; } = new HashSet<string> 
    {
        "Type", "Subtype", "N", "O", "V", "E"
    };
    


}

/// <summary>
/// CollectionField_Type Table 155
/// </summary>
internal partial class APM_CollectionField_Type : APM_CollectionField_Type__Base
{
}


internal partial class APM_CollectionField_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionField_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_CollectionField_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.CollectionField)) 
        {
            ctx.Fail<APM_CollectionField_Type>($"Invalid value {val}, allowed are: [CollectionField]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// CollectionField_Subtype 
/// </summary>
internal partial class APM_CollectionField_Subtype : APM_CollectionField_Subtype__Base
{
}


internal partial class APM_CollectionField_Subtype__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionField_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_CollectionField_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.S || val == PdfName.D || val == PdfName.N || val == PdfName.F || val == PdfName.Desc || val == PdfName.ModDate || val == PdfName.CreationDate || val == PdfName.Size || (ctx.Version == 1.7m && (ctx.Extensions.Contains(PdfName.ADBE_Extn3) && val == PdfName.CompressedSize)) || (ctx.Version >= 2.0m && val == PdfName.CompressedSize))) 
        {
            ctx.Fail<APM_CollectionField_Subtype>($"Invalid value {val}, allowed are: [S,D,N,F,Desc,ModDate,CreationDate,Size,fn:IsPDFVersion(1.7,fn:Extension(ADBE_Extn3,CompressedSize)),fn:SinceVersion(2.0,CompressedSize)]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// CollectionField_N 
/// </summary>
internal partial class APM_CollectionField_N : APM_CollectionField_N__Base
{
}


internal partial class APM_CollectionField_N__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionField_N";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfString, APM_CollectionField_N>(obj, "N", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// CollectionField_O 
/// </summary>
internal partial class APM_CollectionField_O : APM_CollectionField_O__Base
{
}


internal partial class APM_CollectionField_O__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionField_O";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_CollectionField_O>(obj, "O", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// CollectionField_V 
/// </summary>
internal partial class APM_CollectionField_V : APM_CollectionField_V__Base
{
}


internal partial class APM_CollectionField_V__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionField_V";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_CollectionField_V>(obj, "V", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// CollectionField_E 
/// </summary>
internal partial class APM_CollectionField_E : APM_CollectionField_E__Base
{
}


internal partial class APM_CollectionField_E__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "CollectionField_E";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfBoolean, APM_CollectionField_E>(obj, "E", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

