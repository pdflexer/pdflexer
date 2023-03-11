// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOfFilterNames : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfFilterNames";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOfFilterNames_x, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOfFilterNames_* 
/// </summary>
internal partial class APM_ArrayOfFilterNames_x : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfFilterNames_*";
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
            var (val, wasIR) = ctx.GetOptional<PdfName, APM_ArrayOfFilterNames_x>(obj, n, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            
            
            if (!(val == PdfName.ASCIIHexDecode || val == PdfName.ASCII85Decode || val == PdfName.LZWDecode || (ctx.Version >= 1.2m && val == PdfName.FlateDecode) || val == PdfName.RunLengthDecode || val == PdfName.CCITTFaxDecode || (ctx.Version >= 1.4m && val == PdfName.JBIG2Decode) || val == PdfName.DCTDecode || (ctx.Version >= 1.5m && val == PdfName.JPXDecode) || (ctx.Version >= 1.5m && val == PdfName.Crypt))) 
            {
                ctx.Fail<APM_ArrayOfFilterNames_x>($"Invalid value {val}, allowed are: [ASCIIHexDecode,ASCII85Decode,LZWDecode,fn:SinceVersion(1.2,FlateDecode),RunLengthDecode,CCITTFaxDecode,fn:SinceVersion(1.4,JBIG2Decode),DCTDecode,fn:SinceVersion(1.5,JPXDecode),fn:SinceVersion(1.5,Crypt)]");
            }
            // no linked objects
            
        }
    }
}

