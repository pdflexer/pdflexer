// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_OptContentUser : APM_OptContentUser__Base
{
}

internal partial class APM_OptContentUser__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "OptContentUser";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_OptContentUser_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_OptContentUser_Name, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
            case 1.5m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_15.Contains(x)))
                {
                    ctx.Fail<APM_OptContentUser>($"Unknown field {extra} for version 1.5");
                }
                break;
            case 1.6m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_16.Contains(x)))
                {
                    ctx.Fail<APM_OptContentUser>($"Unknown field {extra} for version 1.6");
                }
                break;
            case 1.7m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_17.Contains(x)))
                {
                    ctx.Fail<APM_OptContentUser>($"Unknown field {extra} for version 1.7");
                }
                break;
            case 1.8m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_18.Contains(x)))
                {
                    ctx.Fail<APM_OptContentUser>($"Unknown field {extra} for version 1.8");
                }
                break;
            case 1.9m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_19.Contains(x)))
                {
                    ctx.Fail<APM_OptContentUser>($"Unknown field {extra} for version 1.9");
                }
                break;
            case 2.0m:
                foreach (var extra in obj.Keys.Where(x=> !AllowedFields_20.Contains(x)))
                {
                    ctx.Fail<APM_OptContentUser>($"Unknown field {extra} for version 2.0");
                }
                break;
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_OptContentUser_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    public static List<string> AllowedFields_15 { get; } = new List<string> 
    {
        "Type", "Name"
    };
    public static List<string> AllowedFields_16 { get; } = new List<string> 
    {
        "Type", "Name"
    };
    public static List<string> AllowedFields_17 { get; } = new List<string> 
    {
        "Type", "Name"
    };
    public static List<string> AllowedFields_18 { get; } = new List<string> 
    {
        "Type", "Name"
    };
    public static List<string> AllowedFields_19 { get; } = new List<string> 
    {
        "Type", "Name"
    };
    public static List<string> AllowedFields_20 { get; } = new List<string> 
    {
        "Type", "Name"
    };
    


}

/// <summary>
/// OptContentUser_Type Table 100, User cell
/// </summary>
internal partial class APM_OptContentUser_Type : APM_OptContentUser_Type__Base
{
}


internal partial class APM_OptContentUser_Type__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OptContentUser_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_OptContentUser_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.Ind || val == PdfName.Ttl || val == PdfName.Org)) 
        {
            ctx.Fail<APM_OptContentUser_Type>($"Invalid value {val}, allowed are: [Ind,Ttl,Org]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// OptContentUser_Name 
/// </summary>
internal partial class APM_OptContentUser_Name : APM_OptContentUser_Name__Base
{
}


internal partial class APM_OptContentUser_Name__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "OptContentUser_Name";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_OptContentUser_Name>(obj, "Name", IndirectRequirement.Either);
        if (utval == null) { return; }
        switch (utval.Type) 
        {
            case PdfObjectType.ArrayObj:
                {
                    var val =  (PdfArray)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_ArrayOfStringsText, PdfArray>(stack, val, obj);
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
                ctx.Fail<APM_OptContentUser_Name>("Name is required to one of 'array;string-text', was " + utval.Type);
                return;
        }
    }


}
