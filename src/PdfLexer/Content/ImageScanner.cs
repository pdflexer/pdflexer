using PdfLexer.DOM;

namespace PdfLexer.Content;

public ref struct ImageScanner
{
    private readonly ParsingContext Context;
    private readonly PdfDictionary Page;
    private PageContentScanner Scanner;

    public GfxState<double> GraphicsState;


    public ImageScanner(ParsingContext ctx, PdfDictionary page)
    {
        Context = ctx;
        Scanner = new PageContentScanner(ctx, page, true);
        GraphicsState = new GfxState<double>();
        Page = page;
        lastDoImg = null;
    }

    public PdfRect<double> GetCurrentSize()
    {
        if (Scanner.CurrentOperator == PdfOperatorType.BI || Scanner.CurrentOperator == PdfOperatorType.Do)
        {
            return GraphicsState.CTM.GetCurrentSize();
        }
        return new PdfRect<double> { LLx = 0, LLy = 0, URx = 0, URy = 0 };
    }

    public bool Advance()
    {
        lastDoImg = null;
        while (Scanner.Advance())
        {
            var op = Scanner.CurrentOperator;
            switch (op)
            {
                case PdfOperatorType.EOC:
                    return false;
                case PdfOperatorType.q:
                case PdfOperatorType.Q:
                case PdfOperatorType.cm:
                    if (Scanner.TryGetCurrentOperation(out var gso))
                    {
                        gso.Apply(ref GraphicsState);
                    }
                    continue;
                case PdfOperatorType.BI:
                case PdfOperatorType.EI:
                    return true;
                case PdfOperatorType.Do:
                    if (TryGetXObjImage(out lastDoImg))
                    {
                        return true;
                    }
                    break;
            }
        }
        return false;
    }
    private PdfStream? lastDoImg;

    public bool TryGetImage([NotNullWhen(true)] out PdfImage image)
    {
        if (Scanner.CurrentOperator == PdfOperatorType.BI || Scanner.CurrentOperator == PdfOperatorType.EI)
        {
            if (!Scanner.TryGetCurrentOperation(out var gso))
            {
                image = null!;
                return false;
            }
            var iim = (InlineImage_Op)gso;
            image = GetImage(iim.ConvertToStream(Scanner.Resources));
            return true;
        }
        else if (Scanner.CurrentOperator == PdfOperatorType.Do)
        {
            if (lastDoImg == null)
            {
                throw new PdfLexerException("No last image with Do operator.");
            }
            image = GetImage(lastDoImg);
            return true;
        }
        else
        {
            image = null!;
            return false;
        }
    }

    private bool TryGetXObjImage([NotNullWhen(true)] out PdfStream img)
    {
        if (!Scanner.TryGetCurrentOperation(out var op))
        {
            img = null!;
            return false;
        }
        var doOp = (Do_Op)op;
        if (Scanner.CurrentForm != null && TryGetFromDict(Scanner.CurrentForm, doOp.name, out var str))
        {
            if (str == null)
            {
                img = null!;
                return false;
            }
            img = str;
            return true;
        }

        if (TryGetFromDict(Page, doOp.name, out var pgStr))
        {
            if (pgStr == null) { img = null!; return false; }
            img = pgStr;
            return true;
        }

        Context.Error($"{doOp.name} Do called but not found in resources.");
        img = null!;
        return false;

        static bool TryGetFromDict(PdfDictionary dict, PdfName name, out PdfStream? image)
        {
            image = null;
            if (dict.TryGetValue<PdfDictionary>(PdfName.Resources, out var res, false)
                    && res.TryGetValue<PdfDictionary>(PdfName.XObject, out var xobjs, false)
                    && xobjs.TryGetValue(name, out var xobj)
                    )
            {
                xobj = xobj.Resolve();
                if (xobj.Type == PdfObjectType.StreamObj)
                {
                    var str = (PdfStream)xobj;
                    if (str.Dictionary.TryGetValue<PdfName>(PdfName.Subtype, out var st, false)
                    && st == PdfName.Image)
                    {
                        image = str;
                    }
                }
                return true;
            }
            return false;
        }
    }

    private PdfImage GetImage(PdfStream stream)
    {
        var rect = GetCurrentSize();
        return new PdfImage
        {
            X = rect.LLx,
            Y = rect.LLy,
            W = rect.URx - rect.LLx,
            H = rect.URy - rect.LLy,
            XObj = stream
        };
    }
}

public sealed class PdfImage
{
    public double X { get; set; }
    public double Y { get; set; }
    public double W { get; set; }
    public double H { get; set; }
    public XObjImage XObj { get; set; } = null!;

    // TODO need to pass through access default colorspaces if 
    // they exist in Resources dict for Device* overrides

    internal PdfImage()
    {

    }
}
