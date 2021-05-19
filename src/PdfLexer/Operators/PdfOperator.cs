using PdfLexer.Lexing;
using PdfLexer.Parsers;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Operators
{
    public class PdfOperator
    {
        public static PdfOperatorType GetType(ReadOnlySpan<byte> data, int startAt, int length)
        {
            int key = 0;
            var end = startAt+length;
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
        internal static IPdfOperation NotImplementedParseOp(ParsingContext ctx,  ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            throw new NotImplementedException();
        }

        internal static BDC_Op ParseBDC(ParsingContext ctx,  ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            throw new NotImplementedException();
        }

        internal static DP_Op ParseDP(ParsingContext ctx,  ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            throw new NotImplementedException();
        }

        internal static SC_Op ParseSC(ParsingContext ctx,  ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            throw new NotImplementedException();
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
    }

    public interface IPdfOperation
    {
        public PdfOperatorType Type {get;}
    }

}
