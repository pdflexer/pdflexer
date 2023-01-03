using PdfLexer.DOM;
using PdfLexer.Filters;
using PdfLexer.Fonts;
using PdfLexer.Parsers;
using System.Resources;

namespace PdfLexer.Content;

public ref struct SinglePassRedactor
{
    private bool Randomize;
    private Random RNG;
    private PdfPage Page;
    private ParsingContext Ctx;

    public SinglePassRedactor(ParsingContext ctx, PdfPage page, bool randomize = false)
    {
        Ctx = ctx;
        Page = page.NativeObject.CloneShallow();
        Randomize = randomize;
        RNG = randomize ? new Random() : null!;
    }
    public PdfPage RedactContent(Func<CharInfo, bool> shouldRedact)
    {
        Page.Resources = Page.Resources.CloneShallow();
        var xobj = Page.Resources.GetOrCreateValue<PdfDictionary>(PdfName.XObject).CloneShallow();
        Page.Resources[PdfName.XObject] = xobj;

        var main = new TextScanner(Ctx, Page, false, false);
        var mc = RunSingleStream(main, shouldRedact);
        Page.NativeObject[PdfName.Contents] = PdfIndirectRef.Create(new PdfStream(mc));
        if (main.FormsRead == null) { return Page; }

        foreach (var form in main.FormsRead)
        {
            if (!main.Scanner.TryGetForm(form.Key, out var str))
            {
                continue;
            }
            foreach (var item in form.Value)
            {
                str = RunForm(str, item, shouldRedact, 1);
            }
            xobj[form.Key] = PdfIndirectRef.Create(str);
        }
        return Page;
    }

    private PdfStream RunForm(PdfStream str, GraphicsState state, Func<CharInfo, bool> shouldRedact, int depth)
    {
        if (depth > Ctx.Options.MaxFormDepth)
        {
            throw new ApplicationException($"Max form scan depth of {Ctx.Options.MaxFormDepth} exceeded");
        }
        str = str.CloneShallow();
        var res = str.Dictionary.GetOrCreateValue<PdfDictionary>(PdfName.Resources);
        res = res.CloneShallow();
        str.Dictionary[PdfName.Resources] = res;
        var xobj = res.GetOrCreateValue<PdfDictionary>(PdfName.XObject).CloneShallow();
        res[PdfName.XObject] = xobj;

        var fr = new TextScanner(Ctx, Page, str, state, false);
        str.Contents = RunSingleStream(fr, shouldRedact);
        if (fr.FormsRead == null) { return str; }

        foreach (var form in fr.FormsRead)
        {
            if (!fr.Scanner.TryGetForm(form.Key, out var currentForm))
            {
                continue;
            }
            var cf = currentForm.CloneShallow();
            foreach (var item in form.Value)
            {
                cf = RunForm(cf, item, shouldRedact, depth + 1);
            }
            xobj[form.Key] = PdfIndirectRef.Create(cf);
        }
        return str;
    }

    private PdfStreamContents RunSingleStream(TextScanner text, Func<CharInfo, bool> shouldRedact)
    {
        // info for last statement, only set if a character triggered
        // redaction during it
        PdfOperatorType type = default;
        var glyphs = new List<UnappliedGlyph>(50);
        float ws = 0;
        float cs = 0;

        // redaction info for last statement
        var seqs = new List<int>();
        var widths = new List<float>();
        int pos = 0;

        // info for each statement regardless of redaction
        var start = 0; var length = 0;
        var writeEnd = 0;

        var writers = new List<FlateWriter>() { new FlateWriter() };

        while (true)
        {
            var content = text.Advance();
            if (!content)
            {
                CompleteStatement(text.Scanner.Scanner.Data);
                return Finish(text.Scanner.Scanner.Data);
            }

            if (text.WasNewStatement)
            {
                CompleteStatement(text.Scanner.Scanner.Data);
                start = text.TxtOpStart;
                length = text.TxtOpLength;
                seqs.Clear();
                widths.Clear();
                pos = 0;
            }


            var (x, y) = text.GetCurrentTextPos();
            bool redactCurrent = false;
            if (text.Glyph!.MultiChar != null)
            {
                foreach (var c in text.Glyph.MultiChar)
                {
                    if (shouldRedact(
                        new CharInfo
                        {
                            Char = c,
                            X = x,
                            Y = y,
                            OpPos = text.CurrentTextPos,
                            Pos = text.Scanner.Scanner.Position,
                            Stream = text.Scanner.CurrentStreamId
                        }))
                    {
                        SetStatementForRedaction(text);
                        break;
                    }
                }
            }
            else if (shouldRedact(new CharInfo
            {
                Char = text.Glyph.Char,
                X = x,
                Y = y,
                OpPos = text.CurrentTextPos,
                Pos = text.Scanner.Scanner.Position,
                Stream = text.Scanner.CurrentStreamId
            }))
            {
                SetStatementForRedaction(text);
            }

            if (!redactCurrent)
            {
                pos++;
                continue;
            }

            seqs.Add(pos);
            if (!(text.GraphicsState.Font?.IsVertical ?? false))
            {
                widths.Add(Randomize ? RandomizeVal(text.Glyph.w0) : text.Glyph.w0);
            }
            else
            {
                widths.Add(Randomize ? RandomizeVal(text.Glyph.w1) : text.Glyph.w1);
            }

            pos++;

            PdfStreamContents Finish(ReadOnlySpan<byte> data)
            {
                var str = writers[0];
                if (writeEnd < data.Length)
                {
                    str.Stream.Write(data.Slice(writeEnd));
                }
                return str.Complete();
            }

            void SetStatementForRedaction(TextScanner txt)
            {
                if (!redactCurrent)
                {
                    type = txt.LastOp;
                    glyphs.Clear();
                    glyphs.AddRange(txt.CurrentGlyphs);
                    if (type == PdfOperatorType.doublequote)
                    {
                        ws = txt.GraphicsState.WordSpacing;
                        cs = txt.GraphicsState.CharSpacing;
                    }
                }
                redactCurrent = true;
            }

            void CompleteStatement(ReadOnlySpan<byte> data)
            {
                var str = writers[0].Stream;
                if (writeEnd != start)
                {
                    str.Write(data.Slice(writeEnd, start - writeEnd));
                }
                if (seqs.Count == 0) // no redacts
                {
                    if (length == 0) { return; } // first statement

                    str.Write(data.Slice(start, length));
                    str.WriteByte((byte)'\n');
                }
                else
                {
                    switch (type)
                    {
                        case PdfOperatorType.doublequote:
                            {
                                Tc_Op.WriteLn((decimal)cs, str);
                                Tw_Op.WriteLn((decimal)ws, str);
                                T_Star_Op.WriteLn(str);
                                WriteRedacted(str);
                                break;
                            }
                        case PdfOperatorType.singlequote:
                            {
                                T_Star_Op.WriteLn(str);
                                WriteRedacted(str);
                                break;
                            }
                        case PdfOperatorType.TJ:
                        case PdfOperatorType.Tj:
                            {
                                WriteRedacted(str);
                                break;
                            }
                    }
                }
                writeEnd = start + length;
            }

            void WriteRedacted(Stream str)
            {
                int wp = 0;
                var tj = new TJ_Op(new List<TJ_Item> { });
                int bp = 0;
                decimal shift = 0;
                Span<byte> charData = stackalloc byte[4];
                Span<byte> buffer = stackalloc byte[100];
                int i = 0;
                foreach (var g in glyphs)
                {
                    if (g.Shift == 0 && seqs.Contains(i))
                    {
                        if (bp > 0)
                        {
                            CompleteBuffer(buffer);
                        }
                        shift -= 1000 * (decimal)widths[wp];
                        wp++;
                    }
                    else
                    {
                        if (shift != 0)
                        {
                            tj.info.Add(new TJ_Item { Shift = shift });
                            shift = 0;
                        }
                        if (bp > 96)
                        {
                            CompleteBuffer(buffer);
                        }
                        if (g.Shift != 0)
                        {
                            if (bp > 0)
                            {
                                CompleteBuffer(buffer);
                            }
                            tj.info.Add(new TJ_Item { Shift = (decimal)g.Shift });
                            // TJ spacer not counter in seqs to redact
                            continue;
                        }
                        var v = g.Glyph?.CodePoint ?? (uint)' ';
                        if (g.Glyph?.Undefined ?? false)
                        {
                            // TODO -> need way to copy raw data
                            // could just make sure to clone notdef on each occurence and set codepoint
                        }
                        switch (g.Bytes)
                        {
                            case 1:
                                {
                                    WriteChar(buffer, v);
                                }
                                break;
                            case 2:
                                {
                                    WriteChar(buffer, (v >> 8) & 0xFF);
                                    WriteChar(buffer, v & 0xFF);
                                }
                                break;
                            case 3:
                                {
                                    WriteChar(buffer, (v >> 16) & 0xFF);
                                    WriteChar(buffer, (v >> 8) & 0xFF);
                                    WriteChar(buffer, v & 0xFF);
                                }
                                break;
                            default:
                                {
                                    WriteChar(buffer, (v >> 24) & 0xFF);
                                    WriteChar(buffer, (v >> 16) & 0xFF);
                                    WriteChar(buffer, (v >> 8) & 0xFF);
                                    WriteChar(buffer, v & 0xFF);
                                }
                                break;
                        }
                    }
                    i++;
                }
                if (shift != 0)
                {
                    tj.info.Add(new TJ_Item { Shift = shift });
                }
                if (bp > 0)
                {
                    CompleteBuffer(buffer);
                }
                tj.Serialize(str);
                str.WriteByte((byte)'\n');

                void CompleteBuffer(Span<byte> buff)
                {
                    tj.info.Add(new TJ_Item { Data = buff.Slice(0, bp).ToArray() });
                    bp = 0;
                }

                void WriteChar(Span<byte> buff, uint c)
                {
                    buff[bp++] = (byte)c;
                }
            }
        }
    }

    private float RandomizeVal(float val)
    {
        // +/- 10%
        var sf = 1 + (0.2 * RNG.NextDouble() - 0.1);
        return val * (float)sf;
    }
}

public struct CharInfo
{
    public char Char;
    public float X;
    public float Y;
    internal ulong Stream;
    internal int Pos;
    internal int OpPos;
}

