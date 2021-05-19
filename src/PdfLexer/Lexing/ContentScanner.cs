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
            while ((nxt = Scanner.Peek()) != PdfTokenType.Unknown)
            {
                Operands.Add(new OperandInfo { Type = nxt, StartAt = Scanner.Position, Length = Scanner.CurrentLength });
                Scanner.SkipCurrent();
            }

            CurrentOperator = PdfOperator.GetType(Data, Scanner.Position, Scanner.CurrentLength);
            
            return CurrentOperator;
        }

        public void SkipCurrent()
        {
            CurrentOperator = PdfOperatorType.Unknown;
            Scanner.SkipCurrent();
            Operands.Clear();
        }

        public IPdfOperation GetCurrentOperation()
        {
            Peek();
            var oi = (int)CurrentOperator;
            if (!ParseOpMapping.Parsers.ContainsKey(oi))
            {
                _ctx.Error("Unkown operator found: " + Encoding.ASCII.GetString(Data.Slice(Scanner.Position, Scanner.CurrentLength)));
                Scanner.SkipCurrent();
                return GetCurrentOperation();
            }
            var parser = ParseOpMapping.Parsers[oi];
            return parser(_ctx, Data, Operands);
        }

        public ReadOnlySpan<byte> GetCurrentData()
        {
            Peek();
            if (Operands.Count > 0)
            {
                var sp = Operands[0].StartAt;
                return Data.Slice(sp, Scanner.Position-sp+Scanner.CurrentLength);
            }
            return Data.Slice(Scanner.Position, Scanner.CurrentLength);
            
        }
    }
}
