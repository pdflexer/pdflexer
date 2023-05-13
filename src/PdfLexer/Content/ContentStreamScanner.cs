using PdfLexer.Lexing;
using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace PdfLexer.Content;

public class RentedStreamLexInfo : IDisposable
{
    public RentedStreamLexInfo(byte[] data, int count)
    {
        Data = data;
        Count = count;
    }
    public byte[] Data { get; set; }

    public int Count { get; set; }

    public Span<ContentItem> GetItems()
    {
        var items = MemoryMarshal.Cast<byte, ContentItem>(Data);
        return items.Slice(0, Count);
    }

    public void Dispose()
    {
        try
        {
            ArrayPool<byte>.Shared.Return(Data);
        }
        catch { }
        Data = null!;
    }

    public static RentedStreamLexInfo Create(ParsingContext ctx, ReadOnlySpan<byte> data)
    {
        var items = ContentStreamScanner.FillListWithLexedItems(ctx, data, out var array);
        return new RentedStreamLexInfo(array, items.Length);
    }
}

public struct ContentItem
{
    /// <summary>
    /// Start of token
    /// </summary>
    public int StartAt;
    /// <summary>
    /// If <see cref="Type"/> < 30
    ///     Length of operand object
    /// else
    ///     Number of operands for operator
    /// </summary>
    public int Length;
    /// <summary>
    /// Token type. if < 30 -> <see cref="PdfTokenType"/>
    ///             else -> <see cref="PdfOperatorType"/>
    /// </summary>
    public int Type;
}

/// <summary>
/// 
/// </summary>
public ref struct ContentStreamScanner

{
    public ReadOnlySpan<byte> Data;
    public ReadOnlySpan<ContentItem> Items;
    //internal RentedStreamLexInfo LexInfo;

    private byte[] BackingArray;



    private ParsingContext Ctx;

    public ContentItem CurrentInfo;
    public PdfOperatorType CurrentOperator;
    public int Position;

    public int LastOpPosition;

    private List<OperandInfo> Operands;
    internal static int Size { get; } = Marshal.SizeOf<ContentItem>();


    public ContentStreamScanner(ParsingContext ctx, ReadOnlySpan<byte> data)
    {
        Data = data;
        Items = FillListWithLexedItems(ctx, Data, out BackingArray);
        CurrentOperator = PdfOperatorType.Unknown;
        CurrentInfo = default;
        Operands = new List<OperandInfo>(6);
        Ctx = ctx;
        Position = -1;
        LastOpPosition = -1;
    }

    public ContentStreamScanner(ParsingContext ctx, ReadOnlySpan<byte> data, RentedStreamLexInfo lexInfo)
    {
        Data = data;
        BackingArray = lexInfo.Data;
        Items = lexInfo.GetItems();
        CurrentOperator = PdfOperatorType.Unknown;
        CurrentInfo = default;
        Operands = new List<OperandInfo>(6);
        Ctx = ctx;
        LastOpPosition = -1;
        Position = -1;
    }

    public void SetPosition(int position)
    {
        Position = position - 1;
        Advance();
    }

    public bool Advance()
    {
        Position++;
        for (; Position < Items.Length; Position++)
        {
            CurrentInfo = Items[Position];
            if (CurrentInfo.Type < 30)
            {
                continue;
            }
            CurrentOperator = (PdfOperatorType)CurrentInfo.Type;
            LastOpPosition = Position;
            return true;

        }
        CurrentOperator = PdfOperatorType.EOC;

        return false;
    }

    /// <summary>
    /// Returns the number of operands that were seen at the end of a constent stream
    /// without a corresponding operator.
    /// </summary>
    /// <returns>Operand count</returns>
    /// <exception cref="NotSupportedException">Thrown if not called at end of content</exception>
    public int GetDanglingOperandCount()
    {
        // for cases where there were operands at end of content but no operator
        if (CurrentOperator != PdfOperatorType.EOC)
        {
            throw new NotSupportedException("Get dangling called not at end of content");
        }

        if (LastOpPosition == -1)
        {
            return Items.Length;
        }

        return Items.Length - LastOpPosition - 1;

    }

    public ReadOnlySpan<byte> GetDataForCurrent()
    {
        var first = Position - CurrentInfo.Length;
        var start = Items[first].StartAt;
        var oplen = CurrentInfo.Type switch
        {
            < 256 => 1,
            < 65536 => 2,
            _ => 3
        };

        if (CurrentInfo.StartAt + oplen >= Data.Length)
        {
            // bad EI or other issue
            return Data.Slice(start);
        }
        return Data.Slice(start, CurrentInfo.StartAt - start + oplen);
    }

    public (int start, int length) GetCurrentLength()
    {
        var first = Position - CurrentInfo.Length;
        var start = Items[first].StartAt;
        var oplen = CurrentInfo.Type switch
        {
            < 256 => 1,
            < 6553 => 2,
            _ => 3
        };

        if (CurrentInfo.StartAt + oplen >= Data.Length)
        {
            // bad EI or other issue
            return (start, Data.Length - start);
        }

        return (start, CurrentInfo.StartAt - start + oplen);
    }

    internal List<OperandInfo> GetOperands()
    {
        Operands.Clear();
        for (var i = Position - CurrentInfo.Length; i < Position; i++)
        {
            var op = Items[i];
            Operands.Add(new OperandInfo
            {
                StartAt = op.StartAt,
                Length = op.Length,
                Type = (PdfTokenType)op.Type
            });
        }
        return Operands;
    }

    public bool TryGetCurrentOperation<T>([NotNullWhen(true)] out IPdfOperation<T>? op) where T : struct, IFloatingPoint<T>
    {
        if (CurrentOperator == PdfOperatorType.EI)
        {

            op = GetImage<T>();
            return true;
        }

        var ops = GetOperands();

        var oi = (int)CurrentOperator;



        PdfOperator.ParseOp<T>? parser = null;
        if (oi > 0 && oi < 256)
        {
            parser = ParseOpMapping<T>.SingleByteParsers[oi];
        }
        if (parser == null && !ParseOpMapping<T>.Parsers.TryGetValue(oi, out parser))
        {
            var uk = GetUnknown(CurrentInfo, Data, GetDataForCurrent());
            op = uk;
            Ctx.Error("Unkown operator found: " + uk.op);
            return false;
        }

        try
        {
            op = parser(Ctx, Data, Operands);
            if (op == null)
            {
                op = GetUnknown(CurrentInfo, Data, GetDataForCurrent());
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            var opData = GetDataForCurrent();
            var opTxt = Encoding.ASCII.GetString(opData);
            GetUnknown(CurrentInfo, Data, opData);
            Ctx.Error($"Failure parsing op ({e.Message}): " + opTxt);
            op = GetUnknown(CurrentInfo, Data, opData);
            return false;
        }


        static Unkown_Op<T> GetUnknown(ContentItem current, ReadOnlySpan<byte> allData, ReadOnlySpan<byte> thisOp)
        {
            var l = current.Type switch
            {
                < 256 => 1,
                < 6553 => 2,
                _ => 3
            };
            return new Unkown_Op<T>(Encoding.ASCII.GetString(allData.Slice(current.StartAt, l)), thisOp.ToArray());
        }


    }

    private InlineImage_Op<T> GetImage<T>() where T : struct, IFloatingPoint<T>
    {
        var header = new PdfArray();
        if (Position < 2)
        {
            Ctx.Error("Found EI in stream but did not have proper format");

            return Create(new PdfArray(), new byte[0]);
        }
        var h = Items[Position - 2];
        var d = Items[Position - 1];
        if (h.Type != 21 || d.Type != 21)
        {
            Ctx.Error("Found EI in stream but did not have proper format");
            return Create(new PdfArray(), new byte[0]);
        }
        var pos = h.StartAt + 2; // skip BI
        var he = h.StartAt + h.Length;
        while ((pos = PdfSpanLexer.TryReadNextToken(Data, out var current, pos, out var length)) != -1)
        {
            if (current == PdfTokenType.ArrayStart || current == PdfTokenType.DictionaryStart)
            {
                header.Add(Ctx.GetPdfItem(Data, pos, out var len, null));
                pos += len;
            }
            else if (current == PdfTokenType.Unknown)
            {
                // ID
                pos += length;
            }
            else
            {
                header.Add(Ctx.GetKnownPdfItem((PdfObjectType)current, Data, pos, length, null));
                pos += length;
            }

            if (pos >= he)
            {
                break;
            }
        }

        // adjust for start of binary data
        var start = 0;
        var data = Data.Slice(d.StartAt, d.Length);
        if (data.Length > 0 && data[0] == '\n') { start++; }
        else if (data.Length > 1 && data[0] == '\r' && data[1] == '\n')
        {
            start += 2;
        }
        else if (data.Length > 0 && CommonUtil.IsWhiteSpace(data[0]))
        {
            // this isn't beginstream allowed but seen in inline images
            start++;
        }

        // adjust for end of binary data
        var wsCount = 0;
        if (data[data.Length - 1] == '\n') { wsCount++; }
        if (data[data.Length - 2] == '\r') { wsCount++; }

        return Create(header, data.Slice(start, data.Length - start - wsCount).ToArray());

        InlineImage_Op<T> Create(PdfArray header, byte[] allData) => new InlineImage_Op<T>(header, allData);
    }

    public static Span<ContentItem> FillListWithLexedItems(ParsingContext ctx, ReadOnlySpan<byte> data, out byte[] arr)
    {
        var est = (int)Math.Max(10, data.Length / 2.75 + 10) * Size; // TODO determine best estimate
        arr = ArrayPool<byte>.Shared.Rent(est);
        var max = arr.Length / Size;
        var items = MemoryMarshal.Cast<byte, ContentItem>(arr);
        items = items.Slice(0, max);
        int i = 0;

        int position = 0;
        int length = 0;
        var count = 0;
        var lastOp = -1;
        while ((position = PdfSpanLexer.TryReadNextToken(data, out var current, position, out length)) != -1)
        {
            if (max <= i + 3) // 3 in case image
            {
                items = ExpandCapacity(arr, out arr);
            }
            if (current == PdfTokenType.Unknown)
            {
                // operator
                var type = PdfOperator.GetType(data, position, length);
                if (type == PdfOperatorType.BI)
                {
                    // special handling for image
                    position = AddTwoImageItems(ctx, data, position, items.Slice(i));
                    i += 3; // two op / one operator
                    if (position == -1) { return items.Slice(0, i); }
                    count += 2; // adjust for two for BI
                }
                else
                {
                    items[i++] = new ContentItem
                    {
                        StartAt = position,
                        Length = count - lastOp - 1,
                        Type = (int)type,
                    };
                }
                lastOp = count;
            }
            else
            {
                // operands
                items[i++] = new ContentItem
                {
                    StartAt = position,
                    Length = length,
                    Type = (int)current,
                };
            }
            count++;
            position += length;
        }

        return items.Slice(0, i);

        Span<ContentItem> ExpandCapacity(byte[] arr, out byte[] na)
        {
            var next = arr.Length * 2;
            na = ArrayPool<byte>.Shared.Rent(next);
            arr.CopyTo(na, 0);
            ArrayPool<byte>.Shared.Return(arr);
            max = na.Length / Size;
            return MemoryMarshal.Cast<byte, ContentItem>(na);
        }
    }

    private const byte lastPlainText = 127;
    private static byte[] EI = new byte[] { (byte)'E', (byte)'I' };
    private static int AddTwoImageItems(ParsingContext ctx, ReadOnlySpan<byte> data,
        int startAt, Span<ContentItem> items)
    {
        // to follow the ContentItem semantics we treat an inline image as one single EI operation with two operands
        // - first operand: starts at BI (including BI) and ends after ID (if it exists)
        // - second operand: starts after ID and ends right before EI
        // both operands use type 21 (unknown) to be treated as operand not an operation
        // this setup allows data copying techniques to work properly
        // [    op 1     ][   op 2 ][]
        // BI <objects> ID <binary> EI

        var scanner = new Scanner(ctx, data, startAt + 2); // skip bi
        var headerStart = startAt;
        // skip dict info
        PdfTokenType nxt;
        while ((nxt = scanner.Peek()) != PdfTokenType.Unknown && nxt != PdfTokenType.EOS)
        {
            scanner.SkipCurrent();
        }
        items[0] = new ContentItem
        {
            StartAt = headerStart,
            Length = scanner.Position - headerStart,
            Type = 21,
        };

        var id = PdfOperator.GetType(data, scanner.Position, scanner.CurrentLength);
        if (id != PdfOperatorType.ID)
        {
            ctx.Error("Inline image did not contain ID op.");
        }

        var start = scanner.Position + scanner.CurrentLength;
        var afterID = start;
        // follow beginstream semantics
        // if (data.Length > start + 1 && data[start] == '\n') { start++; }
        // else if (data.Length > start + 2 && data[start] == '\r' && data[start + 1] == '\n')
        // {
        //     start += 2;
        // }
        // else if (data.Length > start + 1 && CommonUtil.IsWhiteSpace(data[start]))
        // {
        //     // this isn't beginstream allowed but seen in inline images
        //     start++;
        // }
        bool requireStartBreak = true;
    FINDEND:
        var current = start;
        while (true)
        {
            var i = data[current..].IndexOf(EI);
            if (i == -1)
            {
                if (requireStartBreak)
                {
                    ctx.Error("End of image not found, trying with relaxed rules.");
                    requireStartBreak = false;
                    goto FINDEND;
                }
                ctx.Error("End of image not found, assuming rest of content is data.");

                items[1] = new ContentItem
                {
                    StartAt = afterID,
                    Length = data.Length - afterID,
                    Type = 21,
                };
                items[2] = new ContentItem
                {
                    StartAt = data.Length,
                    Length = 2,
                    Type = (int)PdfOperatorType.EI,
                };
                return -1;
            }
            i += current; // correct for slice offset

            if ((!requireStartBreak || IsStartOfToken(data, i)) && IsEndOfToken(data, i + 1) && NoBinaryData(data, i + 2, 5))
            {
                // var wsCount = 0;
                // if (data[i - 1] == '\n') { wsCount++; }
                // if (data[i - 2] == '\r') { wsCount++; }
                items[1] = new ContentItem
                {
                    StartAt = afterID,
                    Length = i - afterID,
                    Type = 21,
                };
                items[2] = new ContentItem
                {
                    StartAt = i,
                    Length = 2,
                    Type = (int)PdfOperatorType.EI,
                };
                var e = i + 2;
                if (e >= data.Length) { return -1; }
                return i;
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
}
