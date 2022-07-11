using PdfLexer.Operators;
using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Content
{
    public ref struct ImageScanner
    {
        private ParsingContext Context;
        private PageContentScanner Scanner;
        private PdfDictionary Page;
        
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
        private PdfStream lastDoImg;

        public bool TryGetImage(out PdfImage image)
        {
            image = null;
            if (Scanner.CurrentOperator == PdfOperatorType.BI)
            {
                if (!Scanner.TryGetCurrentOperation(out var gso))
                {
                    return false;
                }
                var iim = (InlineImage_Op)gso;
                image = GetImage(iim.ConvertToStream());
                return true;
            } else if (Scanner.CurrentOperator == PdfOperatorType.Do)
            {
                image = GetImage(lastDoImg);
                return true;
            } else
            {
                return false;
            }
        }

        private bool TryGetXObjImage(out PdfStream img)
        {
            img = null;
            if (!Scanner.TryGetCurrentOperation(out var op))
            {
                return false;
            }
            var doOp = (Do_Op)op;
            if (Scanner.CurrentForm != null && TryGetFromDict(Scanner.CurrentForm, doOp.name, out var str, out var wasImage))
            {
                if (!wasImage)
                {
                    return false;
                }
                img = str;
                return true;
            }

            if (TryGetFromDict(Page, doOp.name, out var pgStr, out var wasImg))
            {
                if (!wasImg) { return false; }
                img = pgStr;
                return true;
            }

            Context.Error($"{doOp.name} Do called but not found in resources.");
            return false;

            bool TryGetFromDict(PdfDictionary dict, PdfName name, out PdfStream image, out bool wasImg)
            {
                image = null;
                wasImg = false;
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
                            wasImg = true;
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
                Stream = stream
            };
        }
    }

    public class PdfImage
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float W { get; set; }
        public float H { get; set; }
        public PdfStream Stream { get; set; }
    }
}
