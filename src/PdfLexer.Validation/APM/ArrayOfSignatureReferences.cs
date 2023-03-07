// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOfSignatureReferences : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfSignatureReferences";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOfSignatureReferences_x, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOfSignatureReferences_* Table 255, Reference cell - ISO 32000 does not mention Identity, but Adobe PDF 1.7 (2006) did, therefore noted as deprecated in 1.7
/// </summary>
internal partial class APM_ArrayOfSignatureReferences_x : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfSignatureReferences_*";
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
            var val = ctx.GetOptional<PdfDictionary, APM_ArrayOfSignatureReferences_x>(obj, n, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            // no value restrictions
            if (APM_SignatureReferenceDocMDP.MatchesType(ctx, val)) 
            {
                ctx.Run<APM_SignatureReferenceDocMDP, PdfDictionary>(stack, val, obj);
            } else if (APM_SignatureReferenceFieldMDP.MatchesType(ctx, val)) 
            {
                ctx.Run<APM_SignatureReferenceFieldMDP, PdfDictionary>(stack, val, obj);
            } else if (APM_SignatureReferenceIdentity.MatchesType(ctx, val)) 
            {
                ctx.Run<APM_SignatureReferenceIdentity, PdfDictionary>(stack, val, obj);
            } else if (APM_SignatureReferenceUR.MatchesType(ctx, val)) 
            {
                ctx.Run<APM_SignatureReferenceUR, PdfDictionary>(stack, val, obj);
            }else 
            {
                ctx.Fail<APM_ArrayOfSignatureReferences_x>("n did not match any allowable types: '[SignatureReferenceDocMDP,SignatureReferenceFieldMDP,SignatureReferenceIdentity,SignatureReferenceUR]'");
            }
            
        }
    }
}
