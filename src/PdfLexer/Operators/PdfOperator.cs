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

        public PdfArray header { get; }
        public byte[] allData { get; }
        public InlineImage_Op(PdfArray header, byte[] allData)
        {
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

    public interface IPdfOperationHandler
    {
        IPdfOperation ParseOp(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands);
        void Apply(ParsingContext ctx, TextState txt, IGraphicsState gfx, ReadOnlySpan<byte> data, List<OperandInfo> operands);
    }

    public class Glyph
    {
        public char C { get; internal set; }
        public float w0 { get; internal set; }
        public float w1 { get; internal set; }
        public bool IsWordSpace { get; internal set; }

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
        public void Apply(ParsingContext ctx, TextState txt, IGraphicsState gfx, ReadOnlySpan<byte> data, List<OperandInfo> operands)
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
        public void Apply(IGraphicsState state) { }
        public void Apply(ITextState state) { }
    }

    public interface IGraphicsState
    {

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
    public class TextState
    {
        int Mode { get; set; }
        float FontSize { get; set; }
        float HorizontalScaling { get; set; }
        float CharSpacing { get; set; }
        float WordSpacing { get; set; }
        float TextLeading { get; set; }
        Matrix4x4 TextMatrix { get; set; }
        Matrix4x4 TextRenderingMatrix { get; set; } // todo
                                                    // Tm = Tlm = [ T_fs*T_h  0       0 ] x Tm x CTM
                                                    //              0         T_fs    0
                                                    //              0         T_rise  1
        Matrix4x4 TextLineMatrix { get; set; }

        public TextState()
        {
            TextMatrix = Matrix4x4.Identity;
            TextLineMatrix = Matrix4x4.Identity;
        }

        public int GetGlyph(ReadOnlySpan<byte> data, int pos, out Glyph info)
        {
            info = default;
            return 0;
        }

        public void ApplyTj(float tj)
        {
            if (tj == 0f) { return; }
            float tx = 0f;
            float ty = 0f;
            if (Mode == 0)
            {
                tx = (-tj / 1000) * FontSize * HorizontalScaling;
            }
            else
            {
                var s = (-tj / 1000) * FontSize;
                ty = s;
            }

            ShiftTextMatrix(tx, ty);
        }

        public void Apply(UnappliedGlyph glyph)
        {
            if (glyph.Shift != 0) { ApplyTj(glyph.Shift); }
            Apply(glyph.Glyph);
        }

        public void Apply(Glyph info)
        {
            // shift
            float tx = 0f;
            float ty = 0f;
            if (Mode == 0)
            {
                // tx = ((w0-Tj/1000) * T_fs + T_c + T_w?) * Th
                // var s = (info.w0 - tj / 1000) * FontSize + CharSpacing; // Tj pre applied
                var s = (info.w0) * FontSize + CharSpacing;
                if (info.IsWordSpace) { s += WordSpacing; }
                tx = s * HorizontalScaling;
            }
            else
            {
                // ty = (w1-Tj/1000) * T_fs + T_c + T_w?)
                var s = (info.w1) * FontSize + CharSpacing;
                if (info.IsWordSpace) { s += WordSpacing; }
                ty = s;
            }

            ShiftTextMatrix(tx, ty);
        }

        private void ShiftTextMatrix(float tx, float ty)
        {
            // Tm = [ 1  0  0 ] x Tm
            //        0  1  0
            //        tx ty 1

            TextMatrix = new Matrix4x4(
              1f, 0f, 0f, 0f,
              0f, 1f, 0f, 0f,
              tx, ty, 1f, 0f,
              0f, 0f, 0f, 1f) * TextMatrix;
        }

        public void Apply(Td_Op op)
        {
            // Tm = Tlm = [ 1  0  0 ] x Tlm
            //              0  1  0
            //              tx ty 1

            TextLineMatrix = new Matrix4x4(
                          1f, 0f, 0f, 0f,
                          0f, 1f, 0f, 0f,
                          (float)op.tx, (float)op.ty, 1f, 0f,
                          0f, 0f, 0f, 1f) * TextLineMatrix;

            TextMatrix = TextLineMatrix;
        }

        public void Apply(TD_Op op)
        {
            // -ty TL
            // tx, ty Td

            TextLineMatrix = new Matrix4x4(
                          1f, 0f, 0f, 0f,
                          0f, 1f, 0f, 0f,
                          (float)op.tx, (float)op.ty, 1f, 0f,
                          0f, 0f, 0f, 1f) * TextLineMatrix;

            TextMatrix = TextLineMatrix;
        }

        public void Apply(Tm_Op op)
        {
            TextLineMatrix = new Matrix4x4(
                          (float)op.a, (float)op.b, 0f, 0f,
                          (float)op.c, (float)op.d, 0f, 0f,
                          (float)op.e, (float)op.f, 1f, 0f,
                          0f, 0f, 0f, 1f);

            TextMatrix = TextLineMatrix;
        }

        public void Apply(T_Star_Op op)
        {
            TextLineMatrix = new Matrix4x4(
                          1f, 0f, 0f, 0f,
                          0f, 1f, 0f, 0f,
                          0f, -TextLeading, 1f, 0f,
                          0f, 0f, 0f, 1f) * TextLineMatrix;

            TextMatrix = TextLineMatrix;
        }


        public void Apply(Tj_Op op)
        {
            // TODO
        }

        public void Apply(doublequote_Op op)
        {
            WordSpacing = (float)op.aw;
            CharSpacing = (float)op.ac;
            Apply(new singlequote_Op(op.text));
        }

        public void Apply(singlequote_Op op)
        {
            // TODO
        }

        public void Apply(TJ_Op op)
        {
            foreach (var item in op.info)
            {
                if (item.Shift != 0m)
                {
                    // shift
                }
                else
                {
                    // string
                }
            }
        }

    }
}
