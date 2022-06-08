using PdfLexer.Lexing;
using PdfLexer.Operators;
using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Content
{
    public ref struct TextScanner
    {
        private readonly ParsingContext Context;
        private readonly PageContentScanner Scanner;
        public readonly TextState TextState;
        public readonly GraphicsState GraphicsState;

        public TextScanner(ParsingContext ctx, PdfDictionary page)
        {
            Context = ctx;
            Scanner = new PageContentScanner(ctx, page);
            TextState = new TextState();
            GraphicsState = new GraphicsState();
            CurrentText = null;
            // while ((nxt = scanner.Peek()) != PdfOperatorType.EOC)
            // {
            //     var op = scanner.GetCurrentOperation();
            //     if (op != null)
            //     {
            //         i++;
            //     }
            //     scanner.SkipCurrent();
            // }
        }

        public bool Advance()
        {
            var nxt = Scanner.Peek();
            if (nxt == PdfOperatorType.EOC)
            {
                return false;
            }
            if (nxt == PdfOperatorType.q || nxt == PdfOperatorType.Q || nxt == PdfOperatorType.cm)
            {
                var gso = Scanner.GetCurrentOperation();
                gso.Apply(GraphicsState);
            }
            var b = Scanner.Scanner.Data[Scanner.Scanner.Position];
            if (b == (byte)'T' || b == (byte)'\'' || b == (byte)'"')
            {
                var tso = Scanner.GetCurrentOperation();
                tso.Apply(TextState);
            } else
            {
                // non text
            }

            // text showing
            // Tj ' " TJ

            return true;
        }

        private bool ReadNextFromCurrent()
        {
            // don't think we want to do it this way
            switch (CurrentText.Type)
            {
                case PdfOperatorType.singlequote:
                case PdfOperatorType.doublequote:
                case PdfOperatorType.Tj:
                case PdfOperatorType.TJ:
                    var tj = (TJ_Op)CurrentText;
                    foreach (var item in tj.info)
                    {
                        // item.
                    }
                    break;
            }
            return false;
        }

        IPdfOperation CurrentText; // Tj ' " TJ


    }
}
