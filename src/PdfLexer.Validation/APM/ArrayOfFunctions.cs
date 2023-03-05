// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOfFunctions : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfFunctions";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOfFunctions_x, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOfFunctions_* 
/// </summary>
internal partial class APM_ArrayOfFunctions_x : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfFunctions_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        for (var i = 0; i<obj.Count; i+=1) 
        {
            CheckSingle(i);
        }
        void CheckSingle(int n) 
        {
            var (utval, wasIR) = ctx.GetOptional<APM_ArrayOfFunctions_x>(obj, n, IndirectRequirement.Either);
            if (utval == null) { return; }
            switch (utval.Type) 
            {
                // TODO funcs: fn:SinceVersion(1.3,dictionary)
                case PdfObjectType.StreamObj:
                    {
                        var val =  (PdfStream)utval;
                        if (!wasIR) { ctx.Fail<APM_ArrayOfFunctions_x>("* is required to be indirect when a stream"); return; }
                        // no special cases
                        // no value restrictions
                        if (APM_FunctionType0.MatchesType(ctx, val.Dictionary)) 
                        {
                            ctx.Run<APM_FunctionType0, PdfDictionary>(stack, val.Dictionary, obj);
                        } else if ((ctx.Version < 1.3m || (ctx.Version >= 1.3m && APM_FunctionType4.MatchesType(ctx, val.Dictionary)))) 
                        {
                            ctx.Run<APM_FunctionType4, PdfDictionary>(stack, val.Dictionary, obj);
                        }else 
                        {
                            ctx.Fail<APM_ArrayOfFunctions_x>("x did not match any allowable types: '[FunctionType0,fn:SinceVersion(1.3,FunctionType4)]'");
                        }
                        return;
                    }
                
                default:
                    ctx.Fail<APM_ArrayOfFunctions_x>("* is required to one of 'fn:SinceVersion(1.3,dictionary);stream', was " + utval.Type);
                    return;
            }
        }
    }
}

