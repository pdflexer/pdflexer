using PdfLexer.Parsers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PdfLexer.Lexing;

public struct OperandInfo
{
    public int StartAt;
    public int Length;
    public PdfTokenType Type;
}
public ref struct ContentScanner
{
    public readonly ReadOnlySpan<byte> Data;
    public PdfOperatorType CurrentOperator;
    public List<OperandInfo> Operands;
    public int Position => Scanner.Position;
    public int CurrentLength => Scanner.CurrentLength;

    private readonly ParsingContext _ctx;
    private Scanner Scanner;
    private bool ImageScanned;

    public ContentScanner(ParsingContext ctx, ReadOnlySpan<byte> data, int position = 0)
    {
        Data = data;
        _ctx = ctx;
        Scanner = new Scanner(ctx, data, position);
        CurrentOperator = PdfOperatorType.Unknown;
        Operands = new List<OperandInfo>(6);
        ImageScanned = false;
    }

    public PdfOperatorType Peek()
    {
        if (CurrentOperator != PdfOperatorType.Unknown)
        {
            return CurrentOperator;
        }

        ImageScanned = false;
        PdfTokenType nxt = PdfTokenType.TBD;
        while ((nxt = Scanner.Peek()) != PdfTokenType.Unknown && nxt != PdfTokenType.EOS)
        {
            Operands.Add(new OperandInfo { Type = nxt, StartAt = Scanner.Position, Length = Scanner.CurrentLength });
            Scanner.SkipCurrent();
        }

        if (nxt == PdfTokenType.EOS)
        {
            CurrentOperator = PdfOperatorType.EOC;
        }
        else
        {
            CurrentOperator = PdfOperator.GetType(Data, Scanner.Position, Scanner.CurrentLength);
        }

        return CurrentOperator;
    }
    private const byte lastPlainText = 127;
    private static byte[] EI = new byte[] { (byte)'E', (byte)'I' };

    public void SkipCurrent()
    {
        if (CurrentOperator == PdfOperatorType.BI && !ImageScanned ) { GetImage(true); }
        CurrentOperator = PdfOperatorType.Unknown;
        Scanner.SkipCurrent();
        Operands.Clear();
    }

    public bool TryGetCurrentOperation([NotNullWhen(true)] out IPdfOperation? op)
    {
        Peek();

        if (CurrentOperator == PdfOperatorType.BI)
        {
            op = GetImage();
            return true;
        }

        var oi = (int)CurrentOperator;
        PdfOperator.ParseOp? parser = null;
        if (oi > 0 && oi < 256)
        {
            parser = ParseOpMapping.SingleByteParsers[oi];
        } 
        if (parser == null && !ParseOpMapping.Parsers.TryGetValue(oi, out parser))
        {
            var uk = GetUnknown(Scanner, Operands, Data);
            op = uk;
            _ctx.Error("Unkown operator found: " + uk.op);
            return false;
        }

        try
        {
            op = parser(_ctx, Data, Operands);
            if (op == null)
            {
                op = GetUnknown(Scanner, Operands, Data);
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            var st = Operands.Count > 0 ? Operands[0].StartAt : Scanner.Position;
            var len = Scanner.Position + Scanner.CurrentLength - st;
            var opTxt = Encoding.ASCII.GetString(Data.Slice(st, len));
            _ctx.Error($"Failure parsing op ({e.Message}): " + opTxt);
            op = GetUnknown(Scanner, Operands, Data);
            return false;
        }

        static Unkown_Op GetUnknown(Scanner scanner, List<OperandInfo> ops, ReadOnlySpan<byte> data)
        {
            byte[] opData;
            var op = Encoding.ASCII.GetString(data.Slice(scanner.Position, scanner.CurrentLength));
            if (ops.Count > 0)
            {
                opData = data.Slice(ops[0].StartAt, scanner.Position - ops[0].StartAt + scanner.CurrentLength).ToArray();
            }
            else
            {
                opData = data.Slice(scanner.Position, scanner.CurrentLength).ToArray();
            }
            return new Unkown_Op(op, opData);
        }
    }

    public IPdfOperation? GetCurrentOperation()
    {
        _ = TryGetCurrentOperation(out var op);
        return op;
    }

    private IPdfOperation GetImage(bool justSkip = false)
    {
        if (ImageScanned)
        {
            throw new NotSupportedException("Image unable to be scanned multiple times. TODO, simplify this");
        }
        ImageScanned = true;
        var sp = Scanner.Position;
        Scanner.SkipCurrent();

        // skip dict info
        PdfTokenType nxt;
        while ((nxt = Scanner.Peek()) != PdfTokenType.Unknown && nxt != PdfTokenType.EOS)
        {
            if (!justSkip)
            {
                Operands.Add(new OperandInfo { Type = nxt, StartAt = Scanner.Position, Length = Scanner.CurrentLength });
            }
            Scanner.SkipCurrent();
        }

        var id = PdfOperator.GetType(Data, Scanner.Position, Scanner.CurrentLength);
        if (id != PdfOperatorType.ID)
        {
            _ctx.Error("Inline image did not contain ID op.");
        }
        var header = new PdfArray();
        if (!justSkip)
        {
            for (var i = 0; i < Operands.Count; i++)
            {
                var op = Operands[i];
                if (op.Type == PdfTokenType.ArrayStart || op.Type == PdfTokenType.DictionaryStart)
                {
                    header.Add(_ctx.GetPdfItem(Data, op.StartAt, out var len));
                    var end = op.StartAt + len;
                    for (; i < Operands.Count; i++)
                    {
                        if (Operands[i].StartAt >= end)
                        {
                            i--;
                            break;
                        }
                    }
                }
                else
                {
                    header.Add(_ctx.GetKnownPdfItem((PdfObjectType)op.Type, Data, op.StartAt, op.Length));
                }
            }
        }

        var start = Scanner.Position + Scanner.CurrentLength;
        // follow beginstream semantics
        if (Data.Length > start + 1 && Data[start] == '\n') { start++; }
        else if (Data.Length > start + 2 && Data[start] == '\r' && Data[start + 1] == '\n')
        {
            start += 2;
        } else if (Data.Length > start + 1 && CommonUtil.IsWhiteSpace(Data[start])) { 
            // this isn't beginstream allowed but seen in inline images
            start++;
        }
        var current = start;
        while (true)
        {
            var i = Data[current..].IndexOf(EI);
            if (i == -1)
            {
                _ctx.Error("End of image not found, assuming rest of content is data.");
                // to allow GetCurrentData() to work
                Operands.Clear();
                Operands.Add(new OperandInfo { Type = PdfTokenType.Unknown, StartAt = sp, Length = Data.Length - sp });
                // CurrentOperator = PdfOperatorType.EI;
                // get skipCurrent to work
                Scanner.Position = Data.Length;
                Scanner.CurrentLength = 0;
                return justSkip ? null! : new InlineImage_Op(header, Data.Slice(start).ToArray());
            }
            i += current; // correct for slice offset

            if (IsStartOfToken(Data, i) && IsEndOfToken(Data, i + 1) && NoBinaryData(Data, i + 2, 5))
            {
                var wsCount = 0;
                if (Data[i - 1] == '\n') { wsCount++; }
                if (Data[i - 2] == '\r') { wsCount++; }
                // to allow GetCurrentData() to work
                Operands.Clear();
                Operands.Add(new OperandInfo { Type = PdfTokenType.Unknown, StartAt = sp, Length = i - sp + 2 }); //+2 for EI
                // CurrentOperator = PdfOperatorType.EI;
                // get skipCurrent to work
                Scanner.Position = i;
                Scanner.CurrentLength = 2;
                return justSkip ? null! : new InlineImage_Op(header, Data.Slice(start, i - start - wsCount).ToArray());
            }
            current = i + 2;
        }

        bool NoBinaryData(ReadOnlySpan<byte> input, int pos, int length)
        {
            var end = pos + length;
            if (end > input.Length)
            {
                end = input.Length;
            }
            for (var i = pos; i < end; i++)
            {
                if (input[i] > lastPlainText)
                {
                    return false;
                }
            }

            return true;
        }
        bool IsEndOfToken(ReadOnlySpan<byte> input, int pos)
        {
            var next = pos + 1;
            return next >= input.Length || CommonUtil.IsWhiteSpace(input[next]);
        }

        bool IsStartOfToken(ReadOnlySpan<byte> input, int pos)
        {
            var prev = pos - 1;
            return prev < 0 || CommonUtil.IsWhiteSpace(input[prev]);
        }
    }

    public ReadOnlySpan<byte> GetCurrentData()
    {
        Peek();
        if (CurrentOperator == PdfOperatorType.BI && !ImageScanned) { GetImage(true); }
        if (Operands.Count > 0)
        {
            var sp = Operands[0].StartAt;
            return Data.Slice(sp, Scanner.Position - sp + Scanner.CurrentLength);
        }
        return Data.Slice(Scanner.Position, Scanner.CurrentLength);
    }

    public (int start, int lenth) GetCurrentSize()
    {
        if (Operands.Count > 0)
        {
            var sp = Operands[0].StartAt;
            return (sp, Scanner.Position - sp + Scanner.CurrentLength);
        }
        return (Scanner.Position, Scanner.CurrentLength);
    }
}
