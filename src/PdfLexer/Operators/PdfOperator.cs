using PdfLexer.Content;
using PdfLexer.Lexing;
using PdfLexer.Parsers;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace PdfLexer.Operators
{



    public interface IPdfOperationHandler
    {
        IPdfOperation ParseOp(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands);
        void Apply(ParsingContext ctx, TextState txt, GraphicsState gfx, ReadOnlySpan<byte> data, List<OperandInfo> operands);
    }

    public class Glyph
    {
        public char Char { get; internal set; }
        public float w0 { get; internal set; }
        public float w1 { get; internal set; }
        public bool IsWordSpace { get; internal set; } // single byte character code 32 when simple font
                                                       // composite font if 32 is single byte code


        // originalCharCode,
        // fontChar,
        // unicode,
        // accent,
        // width,
        // vmetric,
        // operatorListId,
        // isSpace,
        // isInFont
    }
    public readonly struct UnappliedGlyph
    {
        public UnappliedGlyph(Glyph glyph, float shift)
        {
            Glyph = glyph;
            Shift = shift;

        }
        public readonly Glyph Glyph;
        public readonly float Shift;
    }

    public class TJ_OpHandler : IPdfOperationHandler
    {
        private void FillGlyphs(ParsingContext ctx, TextState txt, ReadOnlySpan<byte> data, List<OperandInfo> operands, List<UnappliedGlyph> glyphs)
        {
            var max = operands.Max(x => x.Length);
            if (max > 200)
            {
                var rented = ArrayPool<byte>.Shared.Rent(max);
                FillGlyphsWithBuffer(ctx, txt, data, operands, glyphs, rented);
                ArrayPool<byte>.Shared.Return(rented);
            } else
            {
                Span<byte> buffer = stackalloc byte[max];
                FillGlyphsWithBuffer(ctx, txt, data, operands, glyphs, buffer);
            }
        }

        private void FillGlyphsWithBuffer(ParsingContext ctx, TextState txt, ReadOnlySpan<byte> data, List<OperandInfo> operands, List<UnappliedGlyph> glyphs, Span<byte> buffer)
        {
            float offset = 0f;
            glyphs.Clear();
            foreach (var op in operands)
            {
                if (op.Type == PdfTokenType.StringStart)
                {
                    var used = ctx.StringParser.ConvertBytes(data.Slice(op.StartAt, op.Length), buffer);
                    buffer = buffer.Slice(0, used);
                    int i = 0;
                    int u = 0;
                    while ((u = txt.GetGlyph(buffer, i, out var info)) > 0)
                    {
                        glyphs.Add(new UnappliedGlyph(info, offset));
                        offset = 0f;
                        i += u;
                    }
                }
                else if (op.Type == PdfTokenType.NumericObj || op.Type == PdfTokenType.DecimalObj)
                {
                    if (!Utf8Parser.TryParse(data.Slice(op.StartAt, op.Length), out float val, out int consumed))
                    {
                        ctx.Error("Bad TJ found in content stream: " + Encoding.ASCII.GetString(data.Slice(op.StartAt, op.Length)));
                        val = 0f;
                    }
                    offset += val;
                }
            }
        }
        public void Apply(ParsingContext ctx, TextState txt, GraphicsState gfx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
        {

        }

        public IPdfOperation ParseOp(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) =>
            PdfOperator.ParseTJ(ctx, data, operands);
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
            return new doublequote_Op(aw,ac,text);
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

        public static float Parsefloat(ParsingContext ctx, ReadOnlySpan<byte> data, OperandInfo op)
        {
            if (!Utf8Parser.TryParse(data.Slice(op.StartAt, op.Length), out float val, out int consumed))
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
            stream.Write(buffer.Slice(0, bytes));
        }

        public static void Writefloat(float val, Stream stream)
        {
            Span<byte> buffer = stackalloc byte[35];
            if (!Utf8Formatter.TryFormat(val, buffer, out int bytes))
            {
                throw new ApplicationException("TODO: Unable to write float: " + val.ToString());
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

    public interface IPdfOperation
    {
        public PdfOperatorType Type { get; }
        public void Serialize(Stream stream);
        public void Apply(GraphicsState state) { }
        public void Apply(TextState state) { }
    }

    public interface ITextState
    {
        Matrix4x4 TextMatrix { get; set; }
        Matrix4x4 TextLineMatrix { get; set; }
        // character spacing - Tc = 0, used by Tj, TJ and ' (unscaled by Tfs), added to glyph displacement
        // word spacing - Tw = 0, used by Tj, TJ and ' (unscaled by Tfs), only applied to char code 32 if simple font or composite with 32 as single byte code
        // horizontal scaling - Th = 100,
        // leading - Tl = 0, used by T*, ', " (unscaled by Tfs) - vertical coordinate space
        // Text font - Tt
        // Text font size Tfs
        // Text rise - Trise set by Ts = 0 (unscaled by Tfs) - 
        // Text knockout - Tk -> graphics state, default true, set for whole BT ET

        // mode, default = 0 set by Tr

        // text matrix
        // text line matrix
        // text rendering matrix = text state params + text matrix + CTM
        // current font
        // user unit (PDF 1.6)

        // CTM
        // string byte (0-255) -> character code

        // horizontal 0, vertical 1 writing mode


    }
    
}
