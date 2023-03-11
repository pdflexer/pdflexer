// AUTOGENERATED DO NOT MODIFY

using PdfLexer;
using static PdfLexer.Validation.MathUtil;

namespace PdfLexer.Validation;

internal partial class APM_ArrayOfDigestMethod : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfDigestMethod";
    public static bool RuleGroup() { return true; }
    public static bool AppliesTo(decimal version, List<string> extensions) => true; // managed by links
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        ctx.Run<APM_ArrayOfDigestMethod_x, PdfArray>(stack, obj, parent);

    }

    public static bool MatchesType(PdfValidator ctx, PdfArray obj) 
    {
        return false;
    }
}

/// <summary>
/// ArrayOfDigestMethod_* Table 237, DigestMethod entry
/// </summary>
internal partial class APM_ArrayOfDigestMethod_x : ISpecification<PdfArray>
{
    public static string Name { get; } = "ArrayOfDigestMethod_*";
    public static bool RuleGroup() { return false; }
    public static bool MatchesType(PdfValidator ctx, PdfArray obj) { return false; } // not used for children
    public static bool AppliesTo(decimal version, List<string> extensions) { return version >= 1.7m; }
    public static void Validate(PdfValidator ctx, CallStack stack, PdfArray obj, IPdfObject? parent)
    {
        for (var i = 0; i<obj.Count; i+=1) 
        {
            CheckSingle(i);
        }
        void CheckSingle(int n) 
        {
            var (val, wasIR) = ctx.GetOptional<PdfName, APM_ArrayOfDigestMethod_x>(obj, n, IndirectRequirement.Either);
            if (val == null) { return; }
            // no special cases
            
            
            if (!((ctx.Version < 2.0m && val == PdfName.SHA1) || val == PdfName.SHA256 || val == PdfName.SHA384 || val == PdfName.SHA512 || val == PdfName.RIPEMD160 || (ctx.Extensions.Contains(PdfName.ISO_TS_32001) && val == PdfName.SHA3256) || (ctx.Extensions.Contains(PdfName.ISO_TS_32001) && val == PdfName.SHA3384) || (ctx.Extensions.Contains(PdfName.ISO_TS_32001) && val == PdfName.SHA3512) || (ctx.Extensions.Contains(PdfName.ISO_TS_32001) && val == PdfName.SHAKE256))) 
            {
                ctx.Fail<APM_ArrayOfDigestMethod_x>($"Invalid value {val}, allowed are: [fn:Deprecated(2.0,SHA1),SHA256,SHA384,SHA512,RIPEMD160,fn:Extension(ISO_TS_32001,SHA3-256),fn:Extension(ISO_TS_32001,SHA3-384),fn:Extension(ISO_TS_32001,SHA3-512),fn:Extension(ISO_TS_32001,SHAKE256)]");
            }
            // no linked objects
            
        }
    }
}

