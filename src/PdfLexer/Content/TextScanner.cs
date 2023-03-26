﻿using PdfLexer.Fonts;
using PdfLexer.Lexing;
using System.Text;

namespace PdfLexer.Content;


internal class CharPosition
{
    public float x { get; set; }
    public float y { get; set; }
    public char c { get; set; }
    public string? s { get; set; }
    public ulong Stream { get; set; }
    public int Pos { get; set; }
    public int OpPos { get; set; }
}

/// <summary>
/// Lowest level text reader for PdfLexer
/// Gives access to raw 
/// </summary>
public ref struct TextScanner
{
    private ParsingContext Context;
    private PdfDictionary? PgRes;
    private readonly List<TJ_Lazy_Item> TJCache;
    
    private TextReadState ReadState;
    private UnappliedGlyph CurrentGlyph;

    private bool DataRead;
    private bool IgnoreUndefined;


    internal int CurrentTextPos;
    internal readonly List<UnappliedGlyph> CurrentGlyphs;
    internal PageContentScanner2 Scanner;
    internal int TxtOpStart;
    internal int TxtOpLength;
    internal PdfOperatorType LastOp;

    /// <summary>
    /// Graphics state at the point form Do operations were encountered
    /// Only populated if (readForms = false) or TextScanner is 
    /// created using a form.
    /// </summary>
    public Dictionary<PdfName, List<GraphicsState>>? FormsRead;
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
    public Glyph Glyph;
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

    public TextScanner(ParsingContext ctx, PdfDictionary page, bool readForms = true, bool ignoreUndefined = true)
    {
        Context = ctx;
        FormsRead = readForms ? null : new Dictionary<PdfName, List<GraphicsState>>();
        Scanner = new PageContentScanner2(ctx, page, readForms);
        PgRes = page.Get<PdfDictionary>(PdfName.Resources);
        GraphicsState = new GraphicsState();
        TextState = new TextState(ctx, GraphicsState, PgRes);
        TextState.stack = Scanner.stack;
        TextState.UpdateTRM();
        CurrentTextPos = 0;
        ReadState = TextReadState.Normal;
        CurrentGlyphs = new List<UnappliedGlyph>(50);
        CurrentGlyph = default;
        Glyph = default!;
        TJCache = new List<TJ_Lazy_Item>(10);
        WasNewLine = false;
        WasNewStatement = false;
        LastOp = PdfOperatorType.Unknown;
        TxtOpStart = 0;
        TxtOpLength = 0;
        DataRead = false;
        IgnoreUndefined = ignoreUndefined;
    }

    public TextScanner(ParsingContext ctx, PdfDictionary page, PdfStream form, GraphicsState state, bool ignoreUndefined = true)
    {
        FormsRead = new Dictionary<PdfName, List<GraphicsState>>();
        Context = ctx;
        Scanner = new PageContentScanner2(ctx, page, form);
        PgRes = page.Get<PdfDictionary>(PdfName.Resources);
        GraphicsState = state;
        TextState = new TextState(ctx, GraphicsState, PgRes);
        TextState.stack = Scanner.stack;
        TextState.UpdateTRM();
        CurrentTextPos = 0;
        ReadState = TextReadState.Normal;
        CurrentGlyphs = new List<UnappliedGlyph>(50);
        CurrentGlyph = default;
        Glyph = default!;
        TJCache = new List<TJ_Lazy_Item>(10);
        WasNewLine = false;
        WasNewStatement = false;
        LastOp = PdfOperatorType.Unknown;
        TxtOpStart = 0;
        TxtOpLength = 0;
        DataRead = false;
        IgnoreUndefined = ignoreUndefined;
    }

    public bool Advance()
    {
        if (ReadState == TextReadState.ReadingOp)
        {
            var result = ReadCurrent();
            if (result) return true;
        }

        while (Scanner.Advance())
        {
            var nxt = Scanner.CurrentOperator;

            switch (nxt)
            {
                case PdfOperatorType.Do:
                    if (FormsRead == null)
                    {
                        break;
                    }
                    if (!Scanner.TryGetCurrentOperation(out var doOpI))
                    {
                        break;
                    }
                    var doOp = (Do_Op)doOpI;
                    if (!Scanner.TryGetForm(doOp.name, out _))
                    {
                        break;
                    }
                    if (!FormsRead.TryGetValue(doOp.name, out var states))
                    {
                        states = new List<GraphicsState>();
                        FormsRead[doOp.name] = states;
                    }
                    states.Add(GraphicsState.CloneNoPrev());
                    break;
                case PdfOperatorType.EOC:
                    // Glyph = null;
                    return false;
                case PdfOperatorType.BT:
                    ReadState = TextReadState.ReadingText;
                    // todo reset text state ?
                    TextState.Apply(BT_Op.Value);
                    TextState.FormResources = Scanner.CurrentForm?.Get<PdfDictionary>(PdfName.Resources);

                    continue;
                case PdfOperatorType.ET:
                    ReadState = TextReadState.Normal;
                    continue;
                case PdfOperatorType.gs:
                    if (Scanner.TryGetCurrentOperation(out var gs))
                    {
                        gs.Apply(TextState);
                    }
                    continue;
                case PdfOperatorType.q:
                case PdfOperatorType.Q:
                case PdfOperatorType.cm:
                    if (Scanner.TryGetCurrentOperation(out var gso))
                    {
                        gso.Apply(ref GraphicsState);
                    }
                    TextState.GS = GraphicsState;
                    TextState.FormResources = Scanner.CurrentForm?.Get<PdfDictionary>(PdfName.Resources); // Q from pop form
                    TextState.UpdateTRM();
                    continue;
            }

            
            if (nxt == PdfOperatorType.EI) { continue; } // EI can spill outside bounds if data is corrupt
            var b = Scanner.Scanner.Data[Scanner.Scanner.CurrentInfo.StartAt];
            if (b == (byte)'T' || b == (byte)'\'' || b == (byte)'"')
            {
                (TxtOpStart, TxtOpLength) = Scanner.Scanner.GetCurrentLength();
                try
                {
                    switch (nxt)
                    {
                        case PdfOperatorType.singlequote:
                            {
                                var ops = Scanner.Scanner.GetOperands();
                                var op = ops[0];
                                CurrentGlyphs.Clear();
                                TextState.Apply(T_Star_Op.Value);
                                var slice = Scanner.Scanner.Data.Slice(op.StartAt, op.Length);
                                TextState.FillGlyphsFromRawString(slice, CurrentGlyphs);
                                CurrentTextPos = 0;
                                ReadState = TextReadState.ReadingOp;
                                break;
                            }
                        case PdfOperatorType.doublequote:
                            {
                                CurrentGlyphs.Clear();
                                var ops = Scanner.Scanner.GetOperands();
                                var aw = PdfOperator.ParseFloat(Context, Scanner.Scanner.Data, ops[0]);
                                var ac = PdfOperator.ParseFloat(Context, Scanner.Scanner.Data, ops[1]);
                                var op = ops[2];
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
                                var ops = Scanner.Scanner.GetOperands(); 
                                var op = ops[0];
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
                                var ops = Scanner.Scanner.GetOperands();
                                PdfOperator.ParseTJLazy(Context, Scanner.Scanner.Data, ops, TJCache);
                                foreach (var item in TJCache)
                                {
                                    if (item.OpNum == -1)
                                    {
                                        CurrentGlyphs.Add(new UnappliedGlyph(null, (float)item.Shift));
                                    }
                                    else
                                    {
                                        var op = ops[item.OpNum];
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
                            if (LastOp == PdfOperatorType.singlequote
                                || LastOp == PdfOperatorType.doublequote
                                || LastOp == PdfOperatorType.T_Star)
                            {
                                WasNewLine = true;
                                DataRead = false;
                            }
                            LastOp = nxt;
                            continue;
                    }

                    if (LastOp == PdfOperatorType.singlequote || LastOp == PdfOperatorType.doublequote || LastOp == PdfOperatorType.T_Star)
                    {
                        WasNewLine = true;
                    }
                    WasNewStatement = true;
                    DataRead = false;

                    LastOp = nxt;

                    // text creating ops (default breaks through)
                    var result = ReadCurrent();
                    if (!result) { continue; }
                    return true;

                }
                catch (Exception e)
                {
                    // since we are manually parsing text ops (not using TryGetCurrentOperation)
                    // we have to handle errors manually here
                    var data = Encoding.ASCII.GetString(Scanner.Scanner.GetDataForCurrent());
                    Context.Error($"error while parsing text op ({nxt.ToString()} -> '{data}'): " + e.Message);
                    continue;
                }
            }

            // non-text affecting op
        }
        return false;
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

    internal CharPosition GetCurrentInfo()
    {
        return new CharPosition
        {
            c = Glyph.Char,
            s = Glyph.MultiChar,
            x = TextState.TextRenderingMatrix.M31,
            y = TextState.TextRenderingMatrix.M32,
            Pos = Scanner.Scanner.Position,
            OpPos = CurrentTextPos
        };
    }

    public (float llx, float lly, float urx, float ury) GetCurrentBoundingBox()
    {
        if (Glyph == null)
        {
            throw new NotSupportedException("GetCurrentBoundingBox called with no current glyph in scanner state.");
        }
        return TextState.GetBoundingBox(Glyph);
    }

    private bool ReadCurrent()
    {
        if (CurrentGlyph.Glyph != null)
        {
            TextState.ApplyCharShift(CurrentGlyph); // apply previous glyph to shift char size
        }
        else
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
            }
            else if (CurrentGlyph.Glyph != null)
            {
                // special not def handling if char is wordspace
                if (IgnoreUndefined && CurrentGlyph.Glyph.Undefined)
                {
                    if (CurrentGlyph.Glyph.IsWordSpace)
                    {
                        CurrentGlyph.Glyph.Char = ' ';
                    }
                    else
                    {
                        TextState.ApplyCharShift(CurrentGlyph);
                        continue;
                    }
                }

                // delay setting false until user can read it once
                if (DataRead)
                {
                    WasNewLine = false;
                    WasNewStatement = false;
                }

                DataRead = true;
                Glyph = CurrentGlyph.Glyph;
                return true;
            }
        }
    }
}
