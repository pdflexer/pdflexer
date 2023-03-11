// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOfOptContentOrders : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfOptContentOrders";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOfOptContentOrders_0, PdfArray>(stack, obj, parent);
        ctx.Run<APM_ArrayOfOptContentOrders_x, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOfOptContentOrders_0 Table 99
/// </summary>
internal partial class APM_ArrayOfOptContentOrders_0 : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfOptContentOrders_0";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        var (utval, wasIR) = ctx.GetOptional<APM_ArrayOfOptContentOrders_0>(obj, 0, IndirectRequirement.Either);
        if (utval == null) { ctx.Fail<APM_ArrayOfOptContentOrders_0>("0 is required"); return; }
        switch (utval.Type) 
        {
            case PdfObjectType.DictionaryObj:
                {
                    var val =  (PdfDictionary)utval;
                    // no indirect obj reqs
                    // no special cases
                    // no value restrictions
                    ctx.Run<APM_OptContentGroup, PdfDictionary>(stack, val, obj);
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
                ctx.Fail<APM_ArrayOfOptContentOrders_0>("0 is required to one of 'dictionary;string-text', was " + utval.Type);
                return;
        }
    }
}

/// <summary>
/// ArrayOfOptContentOrders_* 
/// </summary>
internal partial class APM_ArrayOfOptContentOrders_x : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfOptContentOrders_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.5m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        for (var i = 0; i<obj.Count; i+=1) 
        {
            CheckSingle(i);
        }
        void CheckSingle(int n) 
        {
            var (val, wasIR) = ctx.GetOptional<PdfDictionary, APM_ArrayOfOptContentOrders_x>(obj, n, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            // no value restrictions
            ctx.Run<APM_OptContentGroup, PdfDictionary>(stack, val, obj);
            
        }
    }
}

