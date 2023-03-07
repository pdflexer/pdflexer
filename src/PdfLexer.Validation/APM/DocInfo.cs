// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM_DocInfo : APM_DocInfo__Base
{
}

internal partial class APM_DocInfo__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "DocInfo";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM_DocInfo_Title, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocInfo_Author, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocInfo_Subject, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocInfo_Keywords, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocInfo_Creator, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocInfo_Producer, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocInfo_CreationDate, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocInfo_ModDate, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocInfo_Trapped, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocInfo_GTS_PDFXVersion, PdfDictionary>(stack, obj, parent);
        ctx.Run<APM_DocInfo_CatchAll, PdfDictionary>(stack, obj, parent);
        
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        return false;
    }

    


}

/// <summary>
/// DocInfo_Title Table 349
/// </summary>
internal partial class APM_DocInfo_Title : APM_DocInfo_Title__Base
{
}


internal partial class APM_DocInfo_Title__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocInfo_Title";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_DocInfo_Title>(obj, "Title", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DocInfo_Author 
/// </summary>
internal partial class APM_DocInfo_Author : APM_DocInfo_Author__Base
{
}


internal partial class APM_DocInfo_Author__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocInfo_Author";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_DocInfo_Author>(obj, "Author", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DocInfo_Subject 
/// </summary>
internal partial class APM_DocInfo_Subject : APM_DocInfo_Subject__Base
{
}


internal partial class APM_DocInfo_Subject__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocInfo_Subject";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_DocInfo_Subject>(obj, "Subject", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DocInfo_Keywords 
/// </summary>
internal partial class APM_DocInfo_Keywords : APM_DocInfo_Keywords__Base
{
}


internal partial class APM_DocInfo_Keywords__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocInfo_Keywords";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_DocInfo_Keywords>(obj, "Keywords", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DocInfo_Creator 
/// </summary>
internal partial class APM_DocInfo_Creator : APM_DocInfo_Creator__Base
{
}


internal partial class APM_DocInfo_Creator__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocInfo_Creator";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_DocInfo_Creator>(obj, "Creator", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DocInfo_Producer 
/// </summary>
internal partial class APM_DocInfo_Producer : APM_DocInfo_Producer__Base
{
}


internal partial class APM_DocInfo_Producer__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocInfo_Producer";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_DocInfo_Producer>(obj, "Producer", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DocInfo_CreationDate 
/// </summary>
internal partial class APM_DocInfo_CreationDate : APM_DocInfo_CreationDate__Base
{
}


internal partial class APM_DocInfo_CreationDate__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocInfo_CreationDate";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_DocInfo_CreationDate>(obj, "CreationDate", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DocInfo_ModDate 
/// </summary>
internal partial class APM_DocInfo_ModDate : APM_DocInfo_ModDate__Base
{
}


internal partial class APM_DocInfo_ModDate__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocInfo_ModDate";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_DocInfo_ModDate>(obj, "ModDate", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DocInfo_Trapped 
/// </summary>
internal partial class APM_DocInfo_Trapped : APM_DocInfo_Trapped__Base
{
}


internal partial class APM_DocInfo_Trapped__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocInfo_Trapped";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.3m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfName, APM_DocInfo_Trapped>(obj, "Trapped", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        
        
        if (!(val == PdfName.True || val == PdfName.False || val == PdfName.Unknown)) 
        {
            ctx.Fail<APM_DocInfo_Trapped>($"Invalid value {val}, allowed are: [True,False,Unknown]");
        }
        // no linked objects
        
    }


}

/// <summary>
/// DocInfo_GTS_PDFXVersion 
/// </summary>
internal partial class APM_DocInfo_GTS_PDFXVersion : APM_DocInfo_GTS_PDFXVersion__Base
{
}


internal partial class APM_DocInfo_GTS_PDFXVersion__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocInfo_GTS_PDFXVersion";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return false; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        var val = ctx.GetOptional<PdfString, APM_DocInfo_GTS_PDFXVersion>(obj, "GTS_PDFXVersion", IndirectRequirement.Either);
        if (val == null) { return; }
        // no special cases
        // no value restrictions
        // no linked objects
        
    }


}

/// <summary>
/// DocInfo_* Annex E, 2nd class names
/// </summary>
internal partial class APM_DocInfo_CatchAll : APM_DocInfo_CatchAll__Base
{
}


internal partial class APM_DocInfo_CatchAll__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "DocInfo_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.1m && version < 2.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        foreach (var key in obj.Keys)
        {
            if (AllVals.Contains(key)) { continue; }
            
            
            var val = ctx.GetOptional<PdfString, APM_DocInfo_CatchAll>(obj, key, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            // no value restrictions
            // no linked objects
            
        }
        
    }

    public static HashSet<string> AllVals = new HashSet<string> { "Title", "Author", "Subject", "Keywords", "Creator", "Producer", "CreationDate", "ModDate", "Trapped", "AAPL:Keywords", "GTS_PDFXVersion" };
}
