// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_RichMediaInstance : APM_RichMediaInstance_Base
{
}

internal partial class APM_RichMediaInstance_Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "RichMediaInstance";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_RichMediaInstance_Type, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaInstance_Subtype, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaInstance_Asset, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaInstance_Params, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_RichMediaInstance_Scene, PdfDictionary>(stack, obj, parent);
        switch (ctx.Version) {
        
            default:
                break;
        }
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        var c = ctx.Clone();
        c.Run<APM_RichMediaInstance_Type, PdfDictionary>(new CallStack(), obj, null);
        if (c.Errors.Any())
        {
            return false;
        }
        return true;
    }

    


}

/// <summary>
/// RichMediaInstance_Type Table 343
/// </summary>
internal partial class APM_RichMediaInstance_Type : APM_RichMediaInstance_Type_Base
{
}


internal partial class APM_RichMediaInstance_Type_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaInstance_Type";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_RichMediaInstance_Type>(obj, "Type", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "RichMediaInstance")) 
        {
            ctx.Fail<APM_RichMediaInstance_Type>($"Invalid value {val}, allowed are: [RichMediaInstance]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// RichMediaInstance_Subtype 
/// </summary>
internal partial class APM_RichMediaInstance_Subtype : APM_RichMediaInstance_Subtype_Base
{
}


internal partial class APM_RichMediaInstance_Subtype_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaInstance_Subtype";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfName, APM_RichMediaInstance_Subtype>(obj, "Subtype", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        
        if (!(val == "3D" || val == "Sound" || val == "Video" || (ctx.Version == 1.7m && (ctx.Extensions.Contains("ADBE_Extn3") && val == "Flash")))) 
        {
            ctx.Fail<APM_RichMediaInstance_Subtype>($"Invalid value {val}, allowed are: [3D,Sound,Video,fn:IsPDFVersion(1.7,fn:Extension(ADBE_Extn3,Flash))]");
        }
        }
        // no linked objects
        
    }


}

/// <summary>
/// RichMediaInstance_Asset 
/// </summary>
internal partial class APM_RichMediaInstance_Asset : APM_RichMediaInstance_Asset_Base
{
}


internal partial class APM_RichMediaInstance_Asset_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaInstance_Asset";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetRequired<PdfDictionary, APM_RichMediaInstance_Asset>(obj, "Asset", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        ctx.Run<APM_FileSpecification, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// RichMediaInstance_Params Adobe Extension Level 3, Table 9.51b
/// </summary>
internal partial class APM_RichMediaInstance_Params : APM_RichMediaInstance_Params_Base
{
}


internal partial class APM_RichMediaInstance_Params_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaInstance_Params";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfDictionary, APM_RichMediaInstance_Params>(obj, "Params", IndirectRequirement.Either);
        if (val == null) { return; }
        // TODO special case
        // no value restrictions
        ctx.Run<APM_RichMediaParams, PdfDictionary>(stack, val, obj);
        
    }


}

/// <summary>
/// RichMediaInstance_Scene ISO/TS 32007
/// </summary>
internal partial class APM_RichMediaInstance_Scene : APM_RichMediaInstance_Scene_Base
{
}


internal partial class APM_RichMediaInstance_Scene_Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "RichMediaInstance_Scene";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfIntNumber, APM_RichMediaInstance_Scene>(obj, "Scene", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        {
        
        IPdfObject @Scene = val;
        if (!(gte(@Scene,0))) 
        {
            ctx.Fail<APM_RichMediaInstance_Scene>($"Invalid value {val}, allowed are: [fn:Eval(@Scene>=0)]");
        }
        }
        // no linked objects
        
    }


}

