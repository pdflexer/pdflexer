using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Operators;
using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Content
{
    /// <summary>
    /// Lowest level text reader for PdfLexer
    /// Gives access to raw 
    /// </summary>
    public ref struct TextScanner
    {
        private ParsingContext Context;
        private PageContentScanner Scanner;
        private PdfDictionary? PgRes;
        private readonly List<TJ_Lazy_Item> TJCache;
        private int CurrentTextPos;
        private TextReadState ReadState;
        private readonly List<UnappliedGlyph> CurrentGlyphs;
        private UnappliedGlyph CurrentGlyph;
        private PdfOperatorType LastOp;


        /// <summary>
        /// Current text state
        /// </summary>
        public TextState TextState;
        /// <summary>
        /// Current graphics state
        /// </summary>
        public GraphicsState GraphicsState;
        /// <summary>
        /// Current glyph.
        /// Note may be null before first Advance() call
        /// or when Advance() returns false
        /// </summary>
        public Glyph? Glyph;
        /// <summary>
        /// Set true if previous statement resulted in line shift
        /// Note: does not track manual text cursor repositioning.
        /// </summary>
        public bool WasNewLine;
        /// <summary>
        /// Set true current glyph is part of a new set of glyphs
        /// from a single text showing operation
        /// </summary>
        public bool WasNewStatement;

        public TextScanner(ParsingContext ctx, PdfDictionary page)
        {
            Context = ctx;
            Scanner = new PageContentScanner(ctx, page, true);
            PgRes = page.Get<PdfDictionary>(PdfName.Resources);
            TextState = new TextState(ctx, PgRes);
            GraphicsState = new GraphicsState();
            TextState.GS = GraphicsState;
            TextState.stack = Scanner.stack;
            TextState.UpdateTRM();
            CurrentTextPos = 0;
            ReadState = TextReadState.Normal;
            CurrentGlyphs = new List<UnappliedGlyph>(50);
            CurrentGlyph = default;
            Glyph = default;
            TJCache = new List<TJ_Lazy_Item>(10);
            WasNewLine = false;
            WasNewStatement = false;
            LastOp = PdfOperatorType.Unknown;
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
                        // todo reset text state ?
                        TextState.Apply(BT_Op.Value);
                        TextState.FormResources = Scanner.CurrentForm?.Get<PdfDictionary>(PdfName.Resources);
                        Scanner.SkipCurrent();
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
                        TextState.GS = GraphicsState;
                        TextState.UpdateTRM();
                        Scanner.SkipCurrent();
                        continue;
                }

                var b = Scanner.Scanner.Data[Scanner.Scanner.Position];
                if (b == (byte)'T' || b == (byte)'\'' || b == (byte)'"')
                {
                    // need to brainstorm if way to deduplicate the parsing logic here
                    try
                    {
                        switch (nxt)
                        {
                            case PdfOperatorType.singlequote:
                                {
                                    CurrentGlyphs.Clear();
                                    TextState.Apply(T_Star_Op.Value);
                                    var op = Scanner.Scanner.Operands[0];
                                    var slice = Scanner.Scanner.Data.Slice(op.StartAt, op.Length);
                                    TextState.FillGlyphsFromRawString(slice, CurrentGlyphs);
                                    CurrentTextPos = 0;
                                    ReadState = TextReadState.ReadingOp;
                                    break;
                                }
                            case PdfOperatorType.doublequote:
                                {
                                    CurrentGlyphs.Clear();
                                    var aw = PdfOperator.ParseFloat(Context, Scanner.Scanner.Data, Scanner.Scanner.Operands[0]);
                                    var ac = PdfOperator.ParseFloat(Context, Scanner.Scanner.Data, Scanner.Scanner.Operands[1]);
                                    var op = Scanner.Scanner.Operands[2];
                                    var slice = Scanner.Scanner.Data.Slice(op.StartAt, op.Length);
                                    GraphicsState.WordSpacing = aw;
                                    GraphicsState.CharSpacing = ac;
                                    TextState.Apply(T_Star_Op.Value);
                                    TextState.FillGlyphsFromRawString(slice, CurrentGlyphs);
                                    CurrentTextPos = 0;
                                    ReadState = TextReadState.ReadingOp;
                                    break;
                                }
                            case PdfOperatorType.Tj:
                                {
                                    CurrentGlyphs.Clear();
                                    var op = Scanner.Scanner.Operands[0];
                                    var slice = Scanner.Scanner.Data.Slice(op.StartAt, op.Length);
                                    TextState.FillGlyphsFromRawString(slice, CurrentGlyphs);
                                    CurrentTextPos = 0;
                                    ReadState = TextReadState.ReadingOp;
                                    break;
                                }
                            case PdfOperatorType.TJ:
                                {
                                    TJCache.Clear();
                                    CurrentGlyphs.Clear();
                                    PdfOperator.ParseTJLazy(Context, Scanner.Scanner.Data, Scanner.Scanner.Operands, TJCache);
                                    foreach (var item in TJCache)
                                    {
                                        if (item.OpNum == -1)
                                        {
                                            CurrentGlyphs.Add(new UnappliedGlyph(null, (float)item.Shift));
                                        }
                                        else
                                        {
                                            var op = Scanner.Scanner.Operands[item.OpNum];
                                            var slice = Scanner.Scanner.Data.Slice(op.StartAt, op.Length);
                                            TextState.FillGlyphsFromRawString(slice, CurrentGlyphs);
                                        }
                                    }
                                    CurrentTextPos = 0;
                                    ReadState = TextReadState.ReadingOp;
                                    break;
                                }
                            default:
                                if (Scanner.TryGetCurrentOperation(out var tao))
                                {
                                    tao.Apply(TextState);
                                }
                                if (LastOp == PdfOperatorType.T_Star) { WasNewLine = true; }
                                Scanner.SkipCurrent();
                                continue;
                        }

                        if (LastOp == PdfOperatorType.singlequote || LastOp == PdfOperatorType.doublequote)
                        {
                            WasNewLine = true;
                        }
                        WasNewStatement = true;

                        LastOp = nxt;

                        // text creating ops (default breaks through)
                        Scanner.SkipCurrent();
                        var result = ReadCurrent();
                        if (!result) { continue; }
                        return true;

                    } catch (Exception e)
                    {
                        // since we are manually parsing text ops (not using TryGetCurrentOperation)
                        // we have to handle errors manually here
                        var data = Encoding.ASCII.GetString(Scanner.Scanner.GetCurrentData());
                        Context.Error($"error while parsing text op ({nxt.ToString()} -> '{data}'): " + e.Message);
                        Scanner.SkipCurrent();
                        continue;
                    }
                }

                // non-text affecting op
                Scanner.SkipCurrent();
            }
        }


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
            } else
            {
                TextState.UpdateTRM();
            }

            bool trigger = CurrentTextPos == 0;
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
                            TextState.ApplyCharShift(CurrentGlyph);
                            continue;
                        }
                    }

                    // delay setting false until user can read it once
                    if (trigger)
                    {
                        WasNewLine = false;
                        WasNewStatement = false;
                    }

                    Glyph = CurrentGlyph.Glyph;
                    return true;
                }
            }
        }
    }
}
