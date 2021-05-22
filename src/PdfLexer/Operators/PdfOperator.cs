using PdfLexer.Lexing;
using PdfLexer.Parsers;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PdfLexer.Operators
{
    public class Unkown_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.Unknown;

        public string op { get; }
        public byte[] allData { get; }
        public Unkown_Op(string op, byte[] allData)
        {
            this.op = op;
            this.allData = allData;
        }
        public void Serialize(Stream stream)
        {
            stream.Write(allData);
        }
    }

    public class InlineImage_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.Unknown;

        public string op { get; }
        public PdfArray header { get; }
        public byte[] allData { get; }
        public InlineImage_Op(PdfArray header, byte[] allData)
        {
            this.op = op;
            this.header = header;
            this.allData = allData;
        }
        public void Serialize(Stream stream)
        {
            stream.Write(BI_Op.OpData);
            stream.WriteByte((byte)'\n');
            foreach (var item in header)
            {
                PdfOperator.Shared.SerializeObject(stream, item, x => x);
                stream.WriteByte((byte)' ');
            }
            stream.Write(ID_Op.OpData);
            stream.WriteByte((byte)'\n');
            stream.Write(allData);
            stream.Write(EI_Op.OpData);
        }
    }

    public class PdfOperator
    {
        public static PdfOperatorType GetType(ReadOnlySpan<byte> data, int startAt, int length)
        {
            int key = 0;
            var end = startAt + length;
            var pos = 1;
            CommonUtil.SkipWhiteSpace(data, ref startAt);
            for (int i = startAt; i < end; i++)
            {
                key = key | (data[i] << 8 * (pos - 1));
                pos++;
            }
            return (PdfOperatorType)key;
        }
        public delegate IPdfOperation ParseOp(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands);
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
                ctx.Error("BDC only had single operands.");
                if (operands[0].Type == PdfTokenType.NameObj)
                {
                    return new BDC_Op(ParsePdfName(ctx, data, operands[0]), new PdfDictionary());
                }
                return null;
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
                ctx.Error("DP had no operands.");
                return null;
            }
            if (operands.Count == 1)
            {
                ctx.Error("DP only had single operands.");
                if (operands[0].Type == PdfTokenType.NameObj)
                {
                    return new DP_Op(ParsePdfName(ctx, data, operands[0]), new PdfDictionary());
                }
                return null;
            }
            if (operands.Count > 2)
            {
                ctx.Error("DP only had more than two operands, using first two.");
            }
            var di = operands[1];
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
            PdfName name = null;
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
            return new scn_Op(colors, name);
        }

        internal static SCN_Op ParseSCN(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
        {
            PdfName name = null;
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
            return new SCN_Op(colors, name);
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
                            Text = ctx.StringParser.Parse(data.Slice(op.StartAt, op.Length))
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

        public static decimal ParseDecimal(ParsingContext ctx, ReadOnlySpan<byte> data, OperandInfo op)
        {
            if (!Utf8Parser.TryParse(data.Slice(op.StartAt, op.Length), out decimal val, out int consumed))
            {
                ctx.Error("Bad decimal found in content stream: " + Encoding.ASCII.GetString(data.Slice(op.StartAt, op.Length)));
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

        public static PdfArray ParsePdfArray(ParsingContext ctx, ReadOnlySpan<byte> data, OperandInfo op)
        {
            var obj = ctx.NestedParser.ParseNestedItem(data, op.StartAt, out _);
            if (obj.Type != PdfObjectType.ArrayObj)
            {
                ctx.Error("Bad array found in content stream: " + Encoding.ASCII.GetString(data.Slice(op.StartAt, op.Length)));
                return new PdfArray();
            }
            return (PdfArray)obj;
        }

        public static int Parseint(ParsingContext ctx, ReadOnlySpan<byte> data, OperandInfo op)
        {
            if (!Utf8Parser.TryParse(data.Slice(op.StartAt, op.Length), out int val, out int consumed))
            {
                ctx.Error("Bad decimal found in content stream: " + Encoding.ASCII.GetString(data.Slice(op.StartAt, op.Length)));
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
            stream.Write(buffer.Slice(0, bytes));
        }

        public static void Writeint(int val, Stream stream)
        {
            Span<byte> buffer = stackalloc byte[35];
            if (!Utf8Formatter.TryFormat(val, buffer, out int bytes))
            {
                throw new ApplicationException("TODO: Unable to write int: " + val.ToString());
            }
            stream.Write(buffer.Slice(0, bytes));
        }

        internal static Serializers.Serializers Shared => new Serializers.Serializers();

        public static void WritePdfName(PdfName val, Stream stream)
        {
            Shared.NameSerializer.WriteToStream(val, stream);
        }

        public static void WritePdfArray(PdfArray val, Stream stream)
        {
            Shared.ArraySerializer.WriteToStream(val, stream, x => x);
        }

        public static void WritePdfString(PdfString val, Stream stream)
        {
            Shared.StringSerializer.WriteToStream(val, stream);
        }
    }

    public interface IPdfOperation
    {
        public PdfOperatorType Type { get; }
        public void Serialize(Stream stream);
    }

}
