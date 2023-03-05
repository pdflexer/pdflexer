// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOfCompressionFilterNames : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfCompressionFilterNames";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOfCompressionFilterNames_x, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOfCompressionFilterNames_* Table 5, Filter cell and Table 6
/// </summary>
internal partial class APM_ArrayOfCompressionFilterNames_x : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfCompressionFilterNames_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.0m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        for (var i = 0; i<obj.Count; i+=1) 
        {
            CheckSingle(i);
        }
        void CheckSingle(int n) 
        {
            var val = ctx.GetOptional<PdfName, APM_ArrayOfCompressionFilterNames_x>(obj, n, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            
            
            if (!(val == "ASCIIHexDecode" || val == "ASCII85Decode" || val == "LZWDecode" || (ctx.Version < 1.2m || (ctx.Version >= 1.2m && val == "FlateDecode")) || val == "RunLengthDecode" || (ctx.Version < 1.5m || (ctx.Version >= 1.5m && val == "Crypt")))) 
            {
                ctx.Fail<APM_ArrayOfCompressionFilterNames_x>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,fn:SinceVersion(1.2,FlateDecode),RunLengthDecode,fn:SinceVersion(1.5,Crypt)]");
            }
            // no linked objects
            
        }
    }
}

