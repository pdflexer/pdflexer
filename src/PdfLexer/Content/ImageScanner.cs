using PdfLexer.DOM;
using PdfLexer.Parsers;
using System.Diagnostics.CodeAnalysis;

namespace PdfLexer.Content;

public ref struct ImageScanner
{
    private readonly ParsingContext Context;
    private readonly PageContentScanner Scanner;
    private readonly PdfDictionary Page;
    
    public GraphicsState GraphicsState;


    public ImageScanner(ParsingContext ctx, PdfDictionary page)
    {
        Context = ctx;
        Scanner = new PageContentScanner(ctx, page, true);
        GraphicsState = new GraphicsState();
        Page = page;
        lastDoImg = null;
    }

    public (float x, float y, float w, float h) GetCurrentSize()
    {
        if (Scanner.CurrentOperator == PdfOperatorType.BI || Scanner.CurrentOperator == PdfOperatorType.Do)
        {
            return GraphicsState.GetCurrentSize();
        }
        return (0, 0, 0, 0);
    }

    public bool Advance()
    {
        lastDoImg = null;
        if (Scanner.CurrentOperator != PdfOperatorType.Unknown) { Scanner.SkipCurrent(); }
        while (true)
        {
            var op = Scanner.Peek();
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
                    Scanner.SkipCurrent();
                    continue;
                case PdfOperatorType.BI:
                    return true;
                case PdfOperatorType.Do:
                    if (TryGetXObjImage(out lastDoImg)) 
                    {
                        return true;
                    }
                    break;
            }
            Scanner.SkipCurrent();
        }
    }
    private PdfStream? lastDoImg;

    public bool TryGetImage([NotNullWhen(true)]out PdfImage image)
    {
        if (Scanner.CurrentOperator == PdfOperatorType.BI)
        {
            if (!Scanner.TryGetCurrentOperation(out var gso))
            {
                image = null!;
                return false;
            }
            var iim = (InlineImage_Op)gso;
            image = GetImage(iim.ConvertToStream());
            return true;
        } else if (Scanner.CurrentOperator == PdfOperatorType.Do)
        {
            if (lastDoImg == null)
            {
                throw new PdfLexerException("No last image with Do operator.");
            }
            image = GetImage(lastDoImg);
            return true;
        } else
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
        var (x, y, w, h) = GetCurrentSize();
        return new PdfImage
        {
            X = x,
            Y = y,
            W = w,
            H = h,
            XObj = stream
        };
    }
}

public class PdfImage
{
    public float X { get; set; }
    public float Y { get; set; }
    public float W { get; set; }
    public float H { get; set; }
    public XObjImage XObj { get; set; }
}
