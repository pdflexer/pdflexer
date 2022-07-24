using PdfLexer.DOM;
using PdfLexer.Fonts;
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
        private ParsingContext Context;
        private PageContentScanner Scanner;
        public TextState TextState;
        public GraphicsState GraphicsState;
        public Glyph Glyph;

        public TextScanner(ParsingContext ctx, PdfDictionary page)
        {
            Context = ctx;
            Scanner = new PageContentScanner(ctx, page, true);
            TextState = new TextState(ctx, page.Get<PdfDictionary>(PdfName.Resources));
            GraphicsState = new GraphicsState();
            CurrentTextPos = 0;
            ReadState = TextReadState.Normal;
            CurrentGlyphs = new List<UnappliedGlyph>();
            CurrentGlyph = default;
            Glyph = default;
            Position = null;
        }

        public bool Advance()
        {
            if (ReadState == TextReadState.ReadingOp)
            {
                var result = ReadCurrent();
                if (result) return true;
            }

            while (true)
            {
                var nxt = Scanner.Peek();

                switch (nxt)
                {
                    case PdfOperatorType.EOC:
                        Glyph = null;
                        return false;
                    case PdfOperatorType.BT:
                        ReadState = TextReadState.ReadingText;
                        TextState.Apply(BT_Op.Value);
                        TextState.FormResources = Scanner.CurrentForm?.Get<PdfDictionary>(PdfName.Resources);
                        Scanner.SkipCurrent();
                        // todo reset text state
                        continue;
                    case PdfOperatorType.ET:
                        ReadState = TextReadState.Normal;
                        Scanner.SkipCurrent();
                        continue;
                    case PdfOperatorType.q:
                    case PdfOperatorType.Q:
                    case PdfOperatorType.cm:
                        if (Scanner.TryGetCurrentOperation(out var gso))
                        {
                            gso.Apply(ref GraphicsState);
                        }
                        TextState.CTM = GraphicsState.CTM;
                        Scanner.SkipCurrent();
                        continue;
                }

                var b = Scanner.Scanner.Data[Scanner.Scanner.Position];
                if (b == (byte)'T' || b == (byte)'\'' || b == (byte)'"')
                {
                    if (Scanner.TryGetCurrentOperation(out var to))
                    {
                        switch (nxt)
                        {
                            case PdfOperatorType.singlequote:
                                {
                                    var op = (singlequote_Op)to;
                                    TextState.Apply(T_Star_Op.Value);
                                    TextState.FillGlyphs(op.text, CurrentGlyphs);
                                    CurrentTextPos = 0;
                                    ReadState = TextReadState.ReadingOp;
                                    break;
                                }
                            case PdfOperatorType.doublequote:
                                {
                                    var op = (doublequote_Op)to;
                                    TextState.WordSpacing = (float)op.aw;
                                    TextState.CharSpacing = (float)op.ac;
                                    TextState.Apply(T_Star_Op.Value);
                                    TextState.FillGlyphs(op.text, CurrentGlyphs);
                                    CurrentTextPos = 0;
                                    ReadState = TextReadState.ReadingOp;
                                    break;
                                }
                            case PdfOperatorType.Tj:
                                {
                                    var op = (Tj_Op)to;
                                    TextState.FillGlyphs(op, CurrentGlyphs);
                                    CurrentTextPos = 0;
                                    ReadState = TextReadState.ReadingOp;
                                    break;
                                }
                            case PdfOperatorType.TJ:
                                {
                                    var op = (TJ_Op)to;
                                    TextState.FillGlyphs(op, CurrentGlyphs);
                                    CurrentTextPos = 0;
                                    ReadState = TextReadState.ReadingOp;
                                    break;
                                }
                            default:
                                to.Apply(TextState);
                                Scanner.SkipCurrent();
                                continue;
                        }
                    }
                    

                    // text creating ops (default breaks through)
                    Scanner.SkipCurrent();
                    var result = ReadCurrent();
                    if (!result) { continue; }
                    return true;
                }

                // non-text affecting op
                Scanner.SkipCurrent();
            }
        }

        int CurrentTextPos;
        TextReadState ReadState;
        List<UnappliedGlyph> CurrentGlyphs;
        UnappliedGlyph CurrentGlyph;
        PdfRectangle Position;
        public enum TextReadState
        {
            Normal,
            ReadingText,
            ReadingOp
        }

        public (float x, float y) GetCurrentTextPos()
        {
            return (TextState.TextRenderingMatrix.M31, TextState.TextRenderingMatrix.M32);
        }

        public (float llx, float lly, float urx, float ury) GetCurrentBoundingBox()
        {
            return TextState.GetBoundingBox(Glyph);
        }

        private bool ReadCurrent()
        {
            if (CurrentGlyph.Glyph != null)
            {
                TextState.ApplyCharShift(CurrentGlyph); // apply previous glyph to shift char size
            }

            while (true)
            {
                if (CurrentTextPos >= CurrentGlyphs.Count)
                {
                    CurrentGlyph = default;
                    ReadState = TextReadState.ReadingText;
                    return false;
                }

                CurrentGlyph = CurrentGlyphs[CurrentTextPos];
                CurrentTextPos++;

                if (CurrentGlyph.Shift != 0)
                {
                    TextState.ApplyShift(CurrentGlyph);
                } else if (CurrentGlyph.Glyph != null)
                {
                    // special not def handling if char is wordspace
                    if (CurrentGlyph.Glyph.Undefined)
                    {
                        if (CurrentGlyph.Glyph.IsWordSpace)
                        {
                            CurrentGlyph.Glyph.Char = ' ';
                        } else
                        {
                            continue;
                        }
                    }
                    Glyph = CurrentGlyph.Glyph;
                    return true;
                }
            }
        }
    }
}
