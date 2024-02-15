using PdfLexer.Content;
using PdfLexer.Fonts;
using PdfLexer.Serializers;
using System.Buffers;
using System.Numerics;

namespace PdfLexer.Writing.TextLayout;

internal class TextBox<T> where T : struct, IFloatingPoint<T>
{
    public T Width { get; }
    public T Height { get; }

    public T XPos { get; private set; }
    public T YPos { get; private set; }
    public TextAlign Alignment { get; set; }
    public GfxState<T> CurrentState { get; set; }
    public IWritableFont CurrentFont { get; set; }


    public TextBox(GfxState<T> state, IWritableFont font, T width, T height = default)
    {
        Width = width;
        Height = height;
        CurrentState = state;
        CurrentFont = font;
        FirstLineSize = state.TextLeading;
    }


    private List<Func<GfxState<T>, GfxState<T>>> stateMutations { get; } = new List<Func<GfxState<T>, GfxState<T>>>();
    private List<List<TextBlock<T>>> Contents { get; } = new List<List<TextBlock<T>>>();

    public void Apply(ContentWriter<T> writer)
    {
        writer.Writer.Stream.WriteByte((byte)'\n');
        for (var i = 0; i < Contents.Count; i++)
        {
            WriteBlocks(writer.Writer.Stream, Contents[i], i + 1 < Contents.Count);
        }

        void WriteBlocks(Stream stream, List<TextBlock<T>> blocks, bool newLine)
        {
            if (blocks.Count == 0)
            {
                if (newLine)
                {
                    T_Star_Op<T>.WriteLn(stream);
                }
                return;
            }

            T width = T.Zero;
            blocks.ForEach(x => width += x.Width);
            var first = blocks.First().State;

            T dx;
            switch (Alignment)
            {
                case TextAlign.Center:
                    {
                        var dw = Width - width;
                        dx = dw / (T.One + T.One);
                        dx = GetTJValue(first, dx);
                    }
                    break;
                case TextAlign.Right:
                    {
                        dx = Width - width;
                        dx = GetTJValue(first, dx);
                    }
                    break;
                default:
                case TextAlign.Left:
                    dx = T.Zero;
                    break;
            }



            for (var i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                // writer.Font(block.Font, block.State.FontSize); // should move this to work with SetGS
                writer.SetGS(block.State);
                stream.WriteByte((byte)'[');
                if (i == 0 && dx != T.Zero)
                {
                    FPC<T>.Util.Write(dx, stream);
                }
                block.Data.Position = 0;
                block.Data.CopyTo(stream);
                stream.WriteByte((byte)']');
                stream.WriteByte((byte)' ');
                stream.Write(TJ_Op.OpData);
                stream.WriteByte((byte)'\n');
            }

            var last = blocks.Last().State;

            blocks.Clear();

            if (newLine)
            {
                T_Star_Op.WriteLn(stream);
            }
        }

        T GetTJValue(GfxState<T> state, T userSpace)
        {
            var dx = -userSpace * FPC<T>.V1000 / state.FontSize;
            if (!(state.Font?.IsVertical ?? false))
            {
                dx /= state.TextHScale;
            }
            return dx;
        }
    }

    public void NewLine()
    {
        var last = Contents.Where(x => x.Count > 0).LastOrDefault()?.LastOrDefault()?.State;
        if (last == null)
        {
            throw new NotSupportedException("NewLine may not be called on text box before any content added.");
        }
        Contents.Add(new List<TextBlock<T>>());
        YPos += last.TextLeading;
        XPos = T.Zero;
    }

    internal T? FirstLineSize;

    public string? AddText(string text)
    {
        if (Height != default && YPos > Height)
        {
            return text;
        }

        Span<byte> space = stackalloc byte[1];
        space[0] = 32;

        var rented = ArrayPool<byte>.Shared.Rent(text.Length);
        Span<byte> buf = rented;
        byte[] sb = new byte[4];
        Span<byte> spaceBuffer = sb;


        byte[] letterBuffer = new byte[4];

        var ms = new MemoryStream();

        double fs = FPC<T>.Util.ToDouble(CurrentState.FontSize);
        var ws = FPC<T>.Util.ToDouble(CurrentState.WordSpacing);

        var (spaceWidth, spaceBytes) = GetSpaceWidth(CurrentFont, CurrentState, sb);
        spaceBuffer = spaceBuffer.Slice(0, spaceBytes);


        TextBlock<T> currentText;
        List<TextBlock<T>>? currentLine = Contents.LastOrDefault();
        if (currentLine == null)
        {
            currentLine = new List<TextBlock<T>>();
            Contents.Add(currentLine);
        }
        var lc = currentLine.LastOrDefault();
        if (lc != null)
        {
            currentText = lc;
            if (currentText.State != CurrentState || currentText.Font != CurrentFont)
            {
                currentText = new TextBlock<T> { State = CurrentState, Data = new MemoryStream(), Font = CurrentFont, Width = T.Zero };
                currentLine.Add(currentText);
            }
        }
        else
        {
            currentText = new TextBlock<T> { State = CurrentState, Data = new MemoryStream(), Font = CurrentFont, Width = T.Zero };
            currentLine.Add(currentText);
        }

        var lines = text.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var txt = line.Trim();
            var words = txt.Split(" ");
            for (var j = 0; j < words.Length; j++)
            {
                var word = words[j];
                if (string.IsNullOrWhiteSpace(word)) continue;
                var w = word.Trim();

                int os = 0;
                double total = 0;
                foreach (var c in CurrentFont.ConvertFromUnicode(w, 0, w.Length, letterBuffer))
                {
                    if (c.PrevKern != 0)
                    {
                        StringSerializer.WriteToStream(buf.Slice(0, os), ms);
                        PdfOperator.Writedecimal((decimal)c.PrevKern, ms);
                        os = 0;
                        total -= c.PrevKern / 1000;
                    }

                    for (var t = 0; t < c.ByteCount; t++)
                    {
                        buf[os++] = letterBuffer[t];
                    }

                    total += c.Width;
                    if (c.ByteCount == 1 && letterBuffer[0] == 32)
                    {
                        total += ws / fs;
                    }

                    if (c.AddWordSpace && ws != 0)
                    {
                        StringSerializer.WriteToStream(buf.Slice(0, os), ms);
                        PdfOperator.Writedecimal((decimal)(ws / fs), ms);
                        os = 0;
                        total += ws / fs;
                    }
                }

                StringSerializer.WriteToStream(buf.Slice(0, os), ms);

                var (x, y) = CurrentState.CalculateCharShift(FPC<T>.Util.FromDouble<T>(total));
                var cw = y == T.Zero ? x : y;
                if (currentText.Data.Length > 0 || currentLine.Count > 1)
                {
                    cw += spaceWidth;
                }

                if (XPos + cw > Width)
                {

                    if (i == 0 && j == 0 && XPos == T.Zero)
                    {
                        // first word but over limit... write anyway
                    }
                    else
                    {
                        // add to next line
                        NewLine();
                        currentLine = Contents.Last();
                        currentText = new TextBlock<T> { State = CurrentState, Data = new MemoryStream(), Font = CurrentFont, Width = T.Zero };
                        currentLine.Add(currentText);
                    }
                }

                if (currentText.Data.Length > 0 || currentLine.Count > 1)
                {
                    StringSerializer.WriteToStream(spaceBuffer, currentText.Data);
                }
                ms.Position = 0;
                ms.CopyTo(currentText.Data);
                // StringSerializer.WriteToStream(ms.ToArray(), currentText.Data);
                currentText.Width += cw;
                ms.Position = 0;
                ms.SetLength(0);

                if (Height != default && YPos > Height)
                {
                    var thisLine = string.Join(' ', words[(j - 1)..]);
                    if (i + 1 < lines.Length)
                    {
                        return thisLine + "\n" + string.Join('\n', lines[i..]);
                    }
                    return thisLine;
                }

                XPos += cw;
            }

            if (i < lines.Length - 1)
            {
                // create next line
                NewLine();
                currentLine = Contents.Last();
                currentText = new TextBlock<T> { State = CurrentState, Data = new MemoryStream(), Font = CurrentFont, Width = T.Zero };
                currentLine.Add(currentText);
            }

            if (Height != default && YPos > Height)
            {
                if (i + 1 < lines.Length)
                {
                    return string.Join('\n', lines[i..]);
                }
                return null;
            }
        }

        ArrayPool<byte>.Shared.Return(rented);
        return null;


    }

    private static (T Width, int Bytes) GetSpaceWidth(IWritableFont font, GfxState<T> state, byte[] buff)
    {
        Span<byte> sb = stackalloc byte[1];
        sb[0] = 32;
        T spaceWidth = T.Zero;
        int cc = 0;
        foreach (var c in font.ConvertFromUnicode(" ", 0, 1, buff))
        {
            var x = FPC<T>.Util.FromDouble<T>(c.Width);
            var (sx, sy) = state.CalculateCharShift(x);
            spaceWidth = sx == T.Zero ? sy : sx;
            cc = c.ByteCount;
        }

        var (dx, dy) = state.CalculateCharShift(state.WordSpacing / state.FontSize);
        var extraSpacing = dy == T.Zero ? dx : dy;
        return (spaceWidth + extraSpacing, cc);
    }
}

internal class TextBlock<T> where T : struct, IFloatingPoint<T>
{
    public required MemoryStream Data { get; set; }
    public required IWritableFont Font { get; set; }
    public required GfxState<T> State { get; set; }
    public required T Width { get; set; }
}
