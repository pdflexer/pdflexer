using PdfLexer.DOM;

namespace PdfLexer.Content;

internal class ResourceCleaner
{
    private readonly PdfPage _page;

    public ResourceCleaner(PdfPage page)
    {
        _page = page;
    }

    public void CleanUnusedResources()
    {
        var xObjs = new HashSet<PdfName>();
        var usedFonts = new HashSet<PdfName>();
        var scanner = new PageContentScanner(ParsingContext.Current, _page, true, (f, _) => { xObjs.Add(f); });
        while (scanner.Advance())
        {
            switch (scanner.CurrentOperator)
            {
                case PdfOperatorType.Do:
                    if (scanner.TryGetCurrentOperation(out var gso) && gso is Do_Op dOp)
                    {
                        xObjs.Add(dOp.name);
                    }
                    break;
                case PdfOperatorType.Tf:
                    if (scanner.TryGetCurrentOperation(out var tfOp) && tfOp is Tf_Op tf)
                    {
                        usedFonts.Add(tf.font);
                    }
                    break;
            }
        }

        var res = _page.Resources.CloneShallow();
        var xobj = res.GetOrCreateValue<PdfDictionary>(PdfName.XObject).CloneShallow();
        res[PdfName.XObject] = xobj;
        var fonts = res.GetOrCreateValue<PdfDictionary>(PdfName.Font).CloneShallow();
        res[PdfName.Font] = fonts;

        foreach (var key in xobj.Keys)
        {
            if (!xObjs.Contains(key))
            {
                xobj.Remove(key);
            }
        }

        foreach (var key in fonts.Keys)
        {
            if (!usedFonts.Contains(key))
            {
                fonts.Remove(key);
            }
        }

        _page.Resources = res;
    }
}
