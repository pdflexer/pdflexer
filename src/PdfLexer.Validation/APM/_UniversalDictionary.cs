// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

using System.Linq;

internal partial class APM__UniversalDictionary : APM__UniversalDictionary__Base
{
}

internal partial class APM__UniversalDictionary__Base : ISpecification<PdfDictionary>
{
    public static bool RuleGroup() { return true; }
    public static string Name { get; } = "_UniversalDictionary";
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        ctx.Run<APM__UniversalDictionary_CatchAll, PdfDictionary>(stack, obj, parent);
        
    }

    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) 
    {
        return false;
    }

    


}

/// <summary>
/// _UniversalDictionary_* 
/// </summary>
internal partial class APM__UniversalDictionary_CatchAll : APM__UniversalDictionary_CatchAll__Base
{
}


internal partial class APM__UniversalDictionary_CatchAll__Base : ISpecification<PdfDictionary>
{
    public static string Name { get; } = "_UniversalDictionary_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfDictionary obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.2m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfDictionary obj, IPdfObject? parent)
    {
        
        foreach (var key in obj.Keys)
        {
            if (AllVals.Contains(key)) { continue; }
            
            
            var (utval, wasIR) = ctx.GetOptional<APM__UniversalDictionary_CatchAll>(obj, key, IndirectRequirement.Either);
            if (utval == null) { return; }
            switch (utval.Type) 
            {
                case PdfObjectType.ArrayObj:
                    {
                        var val =  (PdfArray)utval;
                        // no indirect obj reqs
                        // no special cases
                        // no value restrictions
                        ctx.Run<APM__UniversalArray, PdfArray>(stack, val, obj);
                        return;
                    }
                case PdfObjectType.BooleanObj:
                    {
                        var val =  (PdfBoolean)utval;
                        // no indirect obj reqs
                        // no special cases
                        // no value restrictions
                        // no linked objects
                        return;
                    }
                case PdfObjectType.DictionaryObj:
                    {
                        var val =  (PdfDictionary)utval;
                        // no indirect obj reqs
                        // no special cases
                        // no value restrictions
                        ctx.Run<APM__UniversalDictionary, PdfDictionary>(stack, val, obj);
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
                case PdfObjectType.NullObj:
                    {
                        var val =  (PdfNull)utval;
                        // no indirect obj reqs
                        // no special cases
                        // no value restrictions
                        // no linked objects
                        return;
                    }
                case PdfObjectType.NumericObj:
                    {
                        var val =  (PdfNumber)utval;
                        // no indirect obj reqs
                        // no special cases
                        // no value restrictions
                        // no linked objects
                        return;
                    }
                case PdfObjectType.StreamObj:
                    {
                        var val =  (PdfStream)utval;
                        if (!wasIR) { ctx.Fail<APM__UniversalDictionary_CatchAll>("* is required to be indirect when a stream"); return; }
                        // no special cases
                        // no value restrictions
                        ctx.Run<APM_Stream, PdfDictionary>(stack, val.Dictionary, obj);
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
                    ctx.Fail<APM__UniversalDictionary_CatchAll>("* is required to one of 'array;boolean;dictionary;name;null;number;stream;string', was " + utval.Type);
                    return;
            }
        }
        
    }

    public static HashSet<string> AllVals = new HashSet<string> {  };
}
