using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.Lexing;
using System.Buffers.Text;
using System.Numerics;
using System.Text;

namespace PdfLexer.Operators;

public partial interface IPdfOperation
{
    public PdfOperatorType Type { get; }
    public void Serialize(Stream stream);
}


public partial interface IPdfOperation<T> : IPdfOperation where T : struct, IFloatingPoint<T>
{
    public void Apply(ref GfxState<T> state) 
    { }
}


public interface IPathCreatingOp<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
{
    public PdfRect<T> GetApproximateBoundingBox(T xpos, T ypos);
    public (T, T) GetFinishingPoint();
}


public partial class PdfOperator
{
    public static bool TryRepair(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> info, List<string> types,
        out List<OperandInfo> fixedOps)
    {
        fixedOps = new List<OperandInfo>();
        var reversed = info.ToList();
        reversed.Reverse();
        types = types.ToList();
        types.Reverse();
        var i = 0;
        foreach (var type in types)
        {
            bool matched = false;
            while (i < reversed.Count)
            {
                var current = reversed[i];
                switch (type)
                {
                    case "int":
                        if (current.Type == PdfTokenType.NumericObj)
                        {
                            matched = true;
                        }
                        break;
                    case "decimal":
                    case "T":
                        if (current.Type == PdfTokenType.NumericObj || current.Type == PdfTokenType.DecimalObj)
                        {
                            matched = true;
                        }
                        break;
                    case "PdfName":
                        matched = current.Type == PdfTokenType.NameObj;
                        break;
                    default:
                        break;
                }
                i++;
                if (matched)
                {
                    fixedOps.Add(current);
                    break;
                }
            }
        }

        if (fixedOps.Count == types.Count)
        {
            fixedOps.Reverse();
            return true;
        }
        fixedOps.Clear();
        return false;
    }

    public static PdfOperatorType GetType(ReadOnlySpan<byte> data, int startAt, int length)
    {
        int key = 0;
        var end = startAt + length;
        var pos = 1;
        CommonUtil.SkipWhiteSpace(data, ref startAt);
        for (int i = startAt; i < end; i++)
        {
            key |= (data[i] << 8 * (pos - 1));
            pos++;
        }
        return (PdfOperatorType)key;
    }


    internal static IPdfOperation NotImplementedParseOp(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        throw new NotImplementedException();
    }


    public static double ParseDouble(ParsingContext ctx, ReadOnlySpan<byte> data, OperandInfo op)
    {
        if (!Utf8Parser.TryParse(data.Slice(op.StartAt, op.Length), out double val, out _))
        {
            ctx.Error("Bad double found in content stream: " + Encoding.ASCII.GetString(data.Slice(op.StartAt, op.Length)));
        }
        return val;
    }

    public static decimal ParseDecimal(ParsingContext ctx, ReadOnlySpan<byte> data, OperandInfo op)
    {
        if (!Utf8Parser.TryParse(data.Slice(op.StartAt, op.Length), out decimal val, out _))
        {
            ctx.Error("Bad decimal found in content stream: " + Encoding.ASCII.GetString(data.Slice(op.StartAt, op.Length)));
        }
        return val;
    }

    public static float ParseFloat(ParsingContext ctx, ReadOnlySpan<byte> data, OperandInfo op)
    {
        if (!Utf8Parser.TryParse(data.Slice(op.StartAt, op.Length), out float val, out _))
        {
            ctx.Error("Bad float found in content stream: " + Encoding.ASCII.GetString(data.Slice(op.StartAt, op.Length)));
        }
        return val;
    }

    public static PdfName ParsePdfName(ParsingContext ctx, ReadOnlySpan<byte> data, OperandInfo op)
    {
        return ctx.NameParser.Parse(data.Slice(op.StartAt, op.Length));
    }

    public static PdfString ParsePdfString(ParsingContext ctx, ReadOnlySpan<byte> data, OperandInfo op)
    {
        return ctx.StringParser.Parse(data.Slice(op.StartAt, op.Length));
    }

    public static PdfArray ParsePdfArrayAndLength(ParsingContext ctx, ReadOnlySpan<byte> data, OperandInfo op, out int length)
    {
        var obj = ctx.NestedParser.ParseNestedItem(ctx.CurrentSource?.Document, data, op.StartAt, out length);
        if (obj.Type != PdfObjectType.ArrayObj)
        {
            ctx.Error("Bad array found in content stream: " + Encoding.ASCII.GetString(data.Slice(op.StartAt, op.Length)));
            return new PdfArray();
        }
        return (PdfArray)obj;
    }
    public static PdfArray ParsePdfArray(ParsingContext ctx, ReadOnlySpan<byte> data, OperandInfo op)
    {
        return ParsePdfArrayAndLength(ctx, data, op, out var _);
    }

    public static int Parseint(ParsingContext ctx, ReadOnlySpan<byte> data, OperandInfo op)
    {
        if (!Utf8Parser.TryParse(data.Slice(op.StartAt, op.Length), out int val, out _))
        {
            ctx.Error("Bad decimal found in content stream: " + Encoding.ASCII.GetString(data.Slice(op.StartAt, op.Length)));
        }
        return val;
    }

    public static float Parsefloat(ParsingContext ctx, ReadOnlySpan<byte> data, OperandInfo op)
    {
        if (!Utf8Parser.TryParse(data.Slice(op.StartAt, op.Length), out float val, out _))
        {
            ctx.Error("Bad float found in content stream: " + Encoding.ASCII.GetString(data.Slice(op.StartAt, op.Length)));
        }
        return val;
    }

    public static double Parsedouble(ParsingContext ctx, ReadOnlySpan<byte> data, OperandInfo op)
    {
        if (!Utf8Parser.TryParse(data.Slice(op.StartAt, op.Length), out double val, out _))
        {
            ctx.Error("Bad double found in content stream: " + Encoding.ASCII.GetString(data.Slice(op.StartAt, op.Length)));
        }
        return val;
    }

    public static void Writedecimal(decimal val, Stream stream)
    {
        Span<byte> buffer = stackalloc byte[35];
        if (!Utf8Formatter.TryFormat(val, buffer, out int bytes))
        {
            throw new ApplicationException("TODO: Unable to write decimal: " + val.ToString());
        }
        if (buffer.IndexOf((byte)'.') > -1)
        {
            for (; bytes > 0; bytes--)
            {
                var b = buffer[bytes - 1];
                if (b != (byte)'0')
                {
                    if (b == (byte)'.')
                    {
                        bytes--;
                    }
                    break;
                }
            }
        }

        
        stream.Write(buffer[..bytes]);
    }

    public static void Writedouble(double val, Stream stream)
    {
        Span<byte> buffer = stackalloc byte[35];
        if (!Utf8Formatter.TryFormat(val, buffer, out int bytes))
        {
            throw new ApplicationException("TODO: Unable to write double: " + val.ToString());
        }
        if (buffer.IndexOf((byte)'.') > -1)
        {
            for (; bytes > 0; bytes--)
            {
                var b = buffer[bytes - 1];
                if (b != (byte)'0')
                {
                    if (b == (byte)'.')
                    {
                        bytes--;
                    }
                    break;
                }
            }
        }


        stream.Write(buffer[..bytes]);
    }

    public static void Writefloat(float val, Stream stream)
    {
        Span<byte> buffer = stackalloc byte[35];
        if (!Utf8Formatter.TryFormat(val, buffer, out int bytes))
        {
            throw new ApplicationException("TODO: Unable to write float: " + val.ToString());
        }
        stream.Write(buffer[..bytes]);
    }


    public static void Writeint(int val, Stream stream)
    {
        Span<byte> buffer = stackalloc byte[35];
        if (!Utf8Formatter.TryFormat(val, buffer, out int bytes))
        {
            throw new ApplicationException("TODO: Unable to write int: " + val.ToString());
        }
        stream.Write(buffer[..bytes]);
    }

    internal static Serializers.Serializers Shared { get; } = new Serializers.Serializers();

    public static void WritePdfName(PdfName val, Stream stream)
    {
        Shared.NameSerializer.WriteToStream(val, stream);
    }

    public static void WritePdfArray(PdfArray val, Stream stream)
    {
        Shared.ArraySerializer.WriteToStream(val, stream);
    }

    public static void WritePdfString(PdfString val, Stream stream)
    {
        Shared.StringSerializer.WriteToStream(val, stream);
    }
}


