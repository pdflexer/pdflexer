namespace PdfLexer.Validation;

internal partial class APM_PageTreeNode // : APM_PageTreeNode__Base
{
    public static new bool MatchesType(PdfValidator ctx, PdfDictionary obj)
    {
        var type = obj.Get<PdfName>("Type");
        if (type == null) { return false; }

        if (type != PdfName.Pages)
        {
            return false;
        }

        if (obj.ContainsKey(PdfName.Parent)) { return true; }
        return false;
    }
}
