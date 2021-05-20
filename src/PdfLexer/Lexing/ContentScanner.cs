using PdfLexer.Operators;
using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Lexing
{
    public struct OperandInfo
    {
        public int StartAt;
        public int Length;
        public PdfTokenType Type;
    }
    public ref struct ContentScanner
    {
        public ReadOnlySpan<byte> Data;
        private Scanner Scanner;
        private ParsingContext _ctx;
        public ContentScanner(ParsingContext ctx, ReadOnlySpan<byte> data)
        {
            Data = data;
            _ctx = ctx;
            Scanner = new Scanner(ctx, data);
            CurrentOperator = PdfOperatorType.Unknown;
            Operands = new List<OperandInfo>(6);
        }

        public PdfOperatorType CurrentOperator;
        public List<OperandInfo> Operands;

        public PdfOperatorType Peek()
        {
            if (CurrentOperator != PdfOperatorType.Unknown)
            {
                return CurrentOperator;
            }

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
            CurrentOperator = PdfOperatorType.Unknown;
            Scanner.SkipCurrent();
            Operands.Clear();
        }

        public IPdfOperation GetCurrentOperation()
        {
            Peek();

            if (CurrentOperator == PdfOperatorType.BI)
            {

            }

            var oi = (int)CurrentOperator;
            if (!ParseOpMapping.Parsers.ContainsKey(oi))
            {
                var op = Encoding.ASCII.GetString(Data.Slice(Scanner.Position, Scanner.CurrentLength));
                _ctx.Error("Unkown operator found: " + op);
                byte[] data;
                if (Operands.Count > 0)
                {
                    data = Data.Slice(Operands[0].StartAt, Scanner.Position - Operands[0].StartAt + Scanner.CurrentLength).ToArray();
                }
                else
                {
                    data = Data.Slice(Scanner.Position, Scanner.CurrentLength).ToArray();
                }
                return new Unkown_Op(op, data);
            }
            var parser = ParseOpMapping.Parsers[oi];
            return parser(_ctx, Data, Operands);
        }

        private IPdfOperation GetImage()
        {
            Scanner.SkipCurrent();

            // skip dict info
            PdfTokenType nxt = PdfTokenType.TBD;
            while ((nxt = Scanner.Peek()) != PdfTokenType.Unknown && nxt != PdfTokenType.EOS)
            {
                Operands.Add(new OperandInfo { Type = nxt, StartAt = Scanner.Position, Length = Scanner.CurrentLength });
                Scanner.SkipCurrent();
            }

            var sp = Scanner.Position;
            var id = PdfOperator.GetType(Data, Scanner.Position, Scanner.CurrentLength);
            if (id != PdfOperatorType.ID)
            {
                _ctx.Error("Inline image did not contain ID op.");
            }

            while (true)
            {
                var start = Scanner.Position + Scanner.CurrentLength;
                var i = Data.Slice(start).IndexOf(EI);
                if (i == -1)
                {
                    _ctx.Error("End of image not found, assuming rest of content is data.");
                    return new InlineImage_Op(null, Data.Slice(start).ToArray());
                }

                if (IsEndOfToken(Data, i+1) && NoBinaryData(Data, i + 2, 5))
                {
                    CurrentOperator = PdfOperatorType.EI;
                    Scanner.Position = i;
                    Scanner.CurrentLength = 2;
                    return new InlineImage_Op(null, Data.Slice(start, i-start).ToArray());
                }
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
        }

        public ReadOnlySpan<byte> GetCurrentData()
        {
            Peek();
            if (Operands.Count > 0)
            {
                var sp = Operands[0].StartAt;
                return Data.Slice(sp, Scanner.Position - sp + Scanner.CurrentLength);
            }
            return Data.Slice(Scanner.Position, Scanner.CurrentLength);

        }
    }
}
