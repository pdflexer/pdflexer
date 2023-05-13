using PdfLexer.Fonts;
using PdfLexer.Lexing;
using System.Numerics;
using System.Text;

namespace PdfLexer.Content;


internal class CharPosition
{
    public double x { get; set; }
    public double y { get; set; }
    public char c { get; set; }
    public string? s { get; set; }
    public ulong Stream { get; set; }
    public int Pos { get; set; }
    public int OpPos { get; set; }
}

public struct CharPos
{
    public double x { get; init; }
    public double y { get; init; }
    public char c { get; init; }
    public override string ToString()
    {
        return $"{c} {x:00} {y:00}";
    }
}


/// <summary>
/// Lowest level text reader for PdfLexer
/// Gives access to raw 
/// </summary>
public ref struct TextScanner
{
    private ParsingContext Context;
    private PdfDictionary? PgRes;
    private readonly List<TJ_Lazy_Item<double>> TJCache;

    private TextReadState ReadState;
    private GlyphOrShift<double> CurrentGlyph;

    private bool DataRead;
    private bool IgnoreUndefined;


    internal int CurrentTextPos;
    internal readonly List<GlyphOrShift<double>> CurrentGlyphs;
    internal PageContentScanner Scanner;
    internal int TxtOpStart;
    internal int TxtOpLength;
    internal PdfOperatorType LastOp;

    /// <summary>
    /// Graphics state at the point form Do operations were encountered
    /// Only populated if (readForms = false) or TextScanner is 
    /// created using a form.
    /// </summary>
    public Dictionary<PdfName, List<GfxState<double>>>? FormsRead;

    /// <summary>
    /// Current graphics state
    /// </summary>
    public GfxState<double> GraphicsState;
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
        FormsRead = readForms ? null : new Dictionary<PdfName, List<GfxState<double>>>();
        Scanner = new PageContentScanner(ctx, page, readForms);
        PgRes = page.Get<PdfDictionary>(PdfName.Resources);
        GraphicsState = new GfxState<double>();
        GraphicsState.UpdateTRM();
        CurrentTextPos = 0;
        ReadState = TextReadState.Normal;
        CurrentGlyphs = new List<GlyphOrShift<double>>(50);
        CurrentGlyph = default;
        Glyph = default!;
        TJCache = new List<TJ_Lazy_Item<double>>(10);
        WasNewLine = false;
        WasNewStatement = false;
        LastOp = PdfOperatorType.Unknown;
        TxtOpStart = 0;
        TxtOpLength = 0;
        DataRead = false;
        IgnoreUndefined = ignoreUndefined;
    }

    public TextScanner(ParsingContext ctx, PdfDictionary page, PdfStream form, GfxState<double> state, bool ignoreUndefined = true)
    {
        FormsRead = new Dictionary<PdfName, List<GfxState<double>>>();
        Context = ctx;
        Scanner = new PageContentScanner(ctx, page, form);
        PgRes = page.Get<PdfDictionary>(PdfName.Resources);
        GraphicsState = state;
        GraphicsState.UpdateTRM();
        CurrentTextPos = 0;
        ReadState = TextReadState.Normal;
        CurrentGlyphs = new List<GlyphOrShift<double>>(50);
        CurrentGlyph = default;
        Glyph = default!;
        TJCache = new List<TJ_Lazy_Item<double>>(10);
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
                        states = new List<GfxState<double>>();
                        FormsRead[doOp.name] = states;
                    }
                    states.Add(GraphicsState with { Prev = null });
                    break;
                case PdfOperatorType.EOC:
                    return false;
                case PdfOperatorType.BT:
                    ReadState = TextReadState.ReadingText;
                    BT_Op<double>.Value.Apply(ref GraphicsState);
                    continue;
                case PdfOperatorType.ET:
                    ReadState = TextReadState.Normal;
                    continue;
                case PdfOperatorType.gs:
                    if (!Scanner.TryGetCurrentOperation(out var gs))
                    {
                        // TODO warn
                        continue;
                    }
                    var extGs = (gs_Op)gs;
                    if (!Scanner.TryGetGraphicsState(extGs.name, out var gsd))
                    {
                        continue;
                    }
                    float? fsize = null;
                    PdfDictionary? fdict = null;
                    IReadableFont? fread = null;
                    if (gsd.TryGetValue<PdfArray>(PdfName.Font, out var fobj, false))
                    {
                        if (fobj.Count > 0 && fobj[0].Resolve() is PdfDictionary fdv)
                        {
                            fdict = fdv;
                            fread = Context.GetFont(fdv);
                        }
                        if (fobj.Count > 1 && fobj[1].Resolve() is PdfNumber fz)
                        {
                            fsize = fz;
                        }
                    }
                    if (fdict != null) 
                    {
                        GraphicsState = GraphicsState with { 
                            Font = fread != null ? fread : GraphicsState.Font,
                            FontObject = fdict != null ? fdict : GraphicsState.FontObject,
                            FontSize = fsize ?? GraphicsState.FontSize };
                    }
                    
                    continue;
                case PdfOperatorType.q:
                case PdfOperatorType.Q:
                case PdfOperatorType.cm:
                    if (Scanner.TryGetCurrentOperation<double>(out var gso))
                    {
                        gso.Apply(ref GraphicsState);
                    }
                    GraphicsState.UpdateTRM();
                    continue;
            }


            if (Scanner.Scanner.CurrentInfo.StartAt >= Scanner.Scanner.Data.Length)
            {
                // EI can spill out
                // re / T_star for form clipping will occur before data filled
                continue;
            }
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
                                T_Star_Op<double>.Value.Apply(ref GraphicsState);
                                var slice = Scanner.Scanner.Data.Slice(op.StartAt, op.Length);
                                Context.FillGlyphsFromRawString(GraphicsState, slice, CurrentGlyphs);
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
                                GraphicsState = GraphicsState with { WordSpacing = aw, CharSpacing = ac };
                                T_Star_Op<double>.Value.Apply(ref GraphicsState);
                                Context.FillGlyphsFromRawString(GraphicsState, slice, CurrentGlyphs);
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
                                Context.FillGlyphsFromRawString(GraphicsState, slice, CurrentGlyphs);
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
                                        CurrentGlyphs.Add(new GlyphOrShift<double>(null, item.Shift));
                                    }
                                    else
                                    {
                                        var op = ops[item.OpNum];
                                        var slice = Scanner.Scanner.Data.Slice(op.StartAt, op.Length);
                                        Context.FillGlyphsFromRawString(GraphicsState, slice, CurrentGlyphs);
                                    }
                                }
                                CurrentTextPos = 0;
                                ReadState = TextReadState.ReadingOp;
                                break;
                            }
                        case PdfOperatorType.Tf:
                            {
                                if (Scanner.TryGetCurrentOperation<double>(out var op))
                                {
                                    var tfOp = (Tf_Op<double>)op;
                                    IReadableFont? rf;
                                    if (!Scanner.TryGetFont(tfOp.font, out var font))
                                    {
                                        rf = SingleByteFont.Fallback;
                                        font = Standard14Font.GetHelvetica().GetPdfFont();
                                    }
                                    else
                                    {
                                        rf = Context.GetFont(font);
                                    }
                                    Tf_Op<double>.Apply(ref GraphicsState, tfOp.font, font, rf, tfOp.size);
                                }
                            }
                            continue;
                        default:
                            if (Scanner.TryGetCurrentOperation<double>(out var tao))
                            {
                                tao.Apply(ref GraphicsState);
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

    public IEnumerable<CharPos> EnumerateCharacters()
    {
        if (Glyph == null)
        {
            throw new NotSupportedException("GetCurrentBoundingBox called with no current glyph in scanner state.");
        }
        var x = GraphicsState.Text.TextRenderingMatrix.E;
        var y = GraphicsState.Text.TextRenderingMatrix.F;
        return GetEnum(Glyph, x, y);
    }

    private static IEnumerable<CharPos> GetEnum(Glyph glyph, double x, double y) 
    {
        if (glyph.MultiChar != null)
        {
            foreach (var c in glyph.MultiChar)
            {
                yield return new CharPos
                {
                    c = c,
                    x = x,
                    y = y
                };
            }
        }
        else
        {
            yield return new CharPos
            {
                c = glyph.Char,
                x = x,
                y = y
            };
        }
    }

    public (double x, double y) GetCurrentTextPos()
    {
        return (GraphicsState.Text.TextRenderingMatrix.E, GraphicsState.Text.TextRenderingMatrix.F);
    }

    internal CharPosition GetCurrentInfo()
    {
        return new CharPosition
        {
            c = Glyph.Char,
            s = Glyph.MultiChar,
            x = GraphicsState.Text.TextRenderingMatrix.E,
            y = GraphicsState.Text.TextRenderingMatrix.F,
            Pos = Scanner.Scanner.Position,
            OpPos = CurrentTextPos
        };
    }

    public PdfRect<double> GetCurrentBoundingBox()
    {
        if (Glyph == null)
        {
            throw new NotSupportedException("GetCurrentBoundingBox called with no current glyph in scanner state.");
        }
        return GraphicsState.GetGlyphBoundingBox(Glyph);
    }

    private bool ReadCurrent()
    {
        if (CurrentGlyph.Glyph != null)
        {
            GraphicsState.ApplyCharShift(CurrentGlyph); // apply previous glyph to shift char size
        }
        else
        {
            GraphicsState.UpdateTRM();
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
                GraphicsState.ApplyShift(CurrentGlyph);
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
                        GraphicsState.ApplyCharShift(CurrentGlyph);
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
