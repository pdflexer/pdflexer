using PdfLexer.Content;
using PdfLexer.Lexing;
using PdfLexer.Parsers;
using System.Buffers.Text;
using System.Text;

namespace PdfLexer.Operators;

public interface IPdfOperation
{
    public PdfOperatorType Type { get; }
    public void Serialize(Stream stream);
    public void Apply(ref GraphicsState state) { }
    public void Apply(TextState state) { }
}

public class PdfOperator
{
    public static void GetParser(ReadOnlySpan<byte> data)
    {

    }
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
    public delegate IPdfOperation? ParseOp(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands);
    internal static IPdfOperation NotImplementedParseOp(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        throw new NotImplementedException();
    }

    internal static IPdfOperation ParseBDC(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        if (operands.Count == 0)
        {
            ctx.Error("BDC had no operands, falling back to BMC w/ unknown.");
            return new BMC_Op(new PdfName("/Unknown"));
        }
        if (operands.Count == 1)
        {
            if (operands[0].Type == PdfTokenType.NameObj)
            {
                ctx.Error("BDC only had single operands.");
                return new BDC_Op(ParsePdfName(ctx, data, operands[0]), new PdfDictionary());
            }

            throw new PdfLexerException("DP only had single operand and was not name.");
        }
        if (operands.Count > 2 && (operands[1].Type != PdfTokenType.ArrayStart && operands[1].Type != PdfTokenType.DictionaryStart))
        {
            ctx.Error("BDC had more than two operands, using first two.");
        }
        var di = operands[1];
        ctx.CurrentSource = null; // force no lazy
        if (di.Type != PdfTokenType.ArrayStart || di.Type != PdfTokenType.DictionaryStart)
        {
            return new BDC_Op(ParsePdfName(ctx, data, operands[0]), ctx.GetPdfItem(data, di.StartAt, out _));
        }
        return new BDC_Op(ParsePdfName(ctx, data, operands[0]), ctx.GetKnownPdfItem((PdfObjectType)di.Type, data, di.StartAt, di.Length));
    }

    internal static DP_Op ParseDP(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        if (operands.Count == 0)
        {
            throw new PdfLexerException("DP had no operands.");
        }
        if (operands.Count == 1)
        {
            if (operands[0].Type == PdfTokenType.NameObj)
            {
                ctx.Error("DP only had single operands.");
                return new DP_Op(ParsePdfName(ctx, data, operands[0]), new PdfDictionary());
            }
            throw new PdfLexerException("DP only had single operand and was not name.");
        }
        var di = operands[1];
        ctx.CurrentSource = null; // force no lazy
        return new DP_Op(ParsePdfName(ctx, data, operands[0]), ctx.GetKnownPdfItem((PdfObjectType)di.Type, data, di.StartAt, di.Length));
    }

    internal static SC_Op ParseSC(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        var colors = new List<decimal>();
        foreach (var val in operands)
        {
            colors.Add(ParseDecimal(ctx, data, val));
        }
        return new SC_Op(colors);
    }

    internal static sc_Op Parsesc(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        var colors = new List<decimal>();
        foreach (var val in operands)
        {
            colors.Add(ParseDecimal(ctx, data, val));
        }
        return new sc_Op(colors);
    }

    internal static scn_Op Parsescn(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        PdfName? name = null;
        var colors = new List<decimal>();
        for (var i = 0; i < operands.Count; i++)
        {
            if (i == operands.Count - 1 && operands[i].Type == PdfTokenType.NameObj)
            {
                name = ParsePdfName(ctx, data, operands[i]);
            }
            else
            {
                colors.Add(ParseDecimal(ctx, data, operands[i]));
            }
        }
        return new scn_Op(colors, name ?? "Unknown");
    }

    internal static SCN_Op ParseSCN(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        PdfName? name = null;
        var colors = new List<decimal>();
        for (var i = 0; i < operands.Count; i++)
        {
            if (i == operands.Count - 1 && operands[i].Type == PdfTokenType.NameObj)
            {
                name = ParsePdfName(ctx, data, operands[i]);
            }
            else
            {
                colors.Add(ParseDecimal(ctx, data, operands[i]));
            }
        }
        return new SCN_Op(colors, name ?? "Unknown");
    }

    internal static Tj_Op ParseTj(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        var op = operands[0];
        var text = ctx.StringParser.ParseRaw(data.Slice(op.StartAt, op.Length));
        return new Tj_Op(text);
    }

    internal static TJ_Op ParseTJ(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        var items = new List<TJ_Item>(operands.Count - 2);
        foreach (var op in operands)
        {
            if (op.Type == PdfTokenType.StringStart)
            {
                items.Add(
                    new TJ_Item
                    {
                        Data = ctx.StringParser.ParseRaw(data.Slice(op.StartAt, op.Length)) // TODO don't allocate arrays here, need to 
                                                                                            // find solution for Writing since it won't 
                                                                                            // have access to source data span if we just 
                                                                                            // track refs
                    });
            }
            else if (op.Type == PdfTokenType.NumericObj || op.Type == PdfTokenType.DecimalObj)
            {
                items.Add(
                    new TJ_Item
                    {
                        Shift = ParseDecimal(ctx, data, op)
                    });
            }
        }
        return new TJ_Op(items);
    }

    internal static void ParseTJLazy(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands, List<TJ_Lazy_Item> items)
    {
        int i = 0;
        foreach (var op in operands)
        {
            if (op.Type == PdfTokenType.StringStart)
            {
                items.Add(
                    new TJ_Lazy_Item
                    {
                        OpNum = i
                    });
            }
            else if (op.Type == PdfTokenType.NumericObj || op.Type == PdfTokenType.DecimalObj)
            {
                items.Add(
                    new TJ_Lazy_Item
                    {
                        Shift = ParseDecimal(ctx, data, op),
                        OpNum = -1
                    });
            }
            i++;
        }
    }

    internal static singlequote_Op Parsesinglequote(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        var op = operands[0];
        var text = ctx.StringParser.ParseRaw(data.Slice(op.StartAt, op.Length));
        return new singlequote_Op(text);
    }

    internal static doublequote_Op Parsedoublequote(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        var aw = ParseDecimal(ctx, data, operands[0]);
        var ac = ParseDecimal(ctx, data, operands[1]);
        var op = operands[2];
        var text = ctx.StringParser.ParseRaw(data.Slice(op.StartAt, op.Length));
        return new doublequote_Op(aw, ac, text);
    }

    internal static d_Op Parsed(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
    {
        var da = ParsePdfArray(ctx, data, operands[0]);
        decimal dp = 0;
        bool end = false;
        for (var i = 1; i < operands.Count; i++)
        {
            if (end) { dp = ParseDecimal(ctx, data, operands[i]); break; }
            if (operands[i].Type == PdfTokenType.ArrayEnd) { end = true; }
        }
        return new d_Op(da, dp);
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
        var obj = ctx.NestedParser.ParseNestedItem(data, op.StartAt, out length);
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

    public static void Writedecimal(decimal val, Stream stream)
    {
        Span<byte> buffer = stackalloc byte[35];
        if (!Utf8Formatter.TryFormat(val, buffer, out int bytes))
        {
            throw new ApplicationException("TODO: Unable to write decimal: " + val.ToString());
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


