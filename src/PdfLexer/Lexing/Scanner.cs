using PdfLexer.Parsers;
using PdfLexer.Parsers.Nested;
using System;

namespace PdfLexer.Lexing
{
    public ref struct Scanner
    {
        private ParsingContext Context;
        private ReadOnlySpan<byte> Data;

        public Scanner(ParsingContext ctx, ReadOnlySpan<byte> data, int startAt = 0)
        {
            Context = ctx;
            Data = data;
            Position = startAt;
            CurrentTokenType = PdfTokenType.Unknown;
            CurrentLength = 0;
        }

        public PdfTokenType CurrentTokenType;
        public int CurrentLength;
        public int Position;

        public PdfTokenType Peek()
        {
            if (CurrentLength == 0)
            {
                Position = PdfSpanLexer.TryReadNextToken(Data, out CurrentTokenType, Position, out CurrentLength);
            }
            return CurrentTokenType;
        }

        public void SkipCurrent()
        {
            if (CurrentLength == 0)
            {
                Peek();
            }
            ThrowIfAtEndOfData();
            Position += CurrentLength;
            CurrentLength = 0;
        }

        public void SkipExpected(PdfTokenType type)
        {
            if (CurrentLength == 0)
            {
                Peek();
            }
            ThrowIfAtEndOfData();
            if (type != CurrentTokenType)
            {
                throw CommonUtil.DisplayDataErrorException(Data, Position, $"Mismatched token, found {CurrentTokenType}, expected {type}");
            }
            Position += CurrentLength;
            CurrentLength = 0;
        }

        private void ThrowIfAtEndOfData()
        {
            if (Position == -1)
            {
                var pos = Data.Length - 10;
                pos = Math.Max(0, pos);
                throw CommonUtil.DisplayDataErrorException(Data, pos, $"End of data reached");
            }
        }

        public void Advance(int cnt)
        {
            Position += cnt;
            CurrentLength = 0;
        }

        public int SkipObject()
        {
            if (CurrentLength == 0)
            {
                Peek();
            }
            ThrowIfAtEndOfData();
            if ((int)CurrentTokenType > 7)
            {
                throw CommonUtil.DisplayDataErrorException(Data, Position, $"No object found at offset, found token of type {CurrentTokenType.ToString()}");
            }
            var start = Position;
            Position += CurrentLength;
            if (CurrentTokenType == PdfTokenType.ArrayStart)
            {
                if (!NestedUtil.AdvanceToArrayEnd(Data, ref Position, out _))
                {
                    var pos = Data.Length - 10;
                    pos = Math.Max(0, pos);
                    throw CommonUtil.DisplayDataErrorException(Data, pos, $"Unable to find array end");
                }
            }
            else if (CurrentTokenType == PdfTokenType.DictionaryStart)
            {
                if (!NestedUtil.AdvanceToDictEnd(Data, ref Position, out _))
                {
                    var pos = Data.Length - 10;
                    pos = Math.Max(0, pos);
                    throw CommonUtil.DisplayDataErrorException(Data, pos, $"Unable to find dictionary end");
                }
            }
            CurrentLength = 0;
            return Position - start;
        }

        public IPdfObject GetCurrentObject()
        {
            if (CurrentLength == 0)
            {
                Peek();
            }
            ThrowIfAtEndOfData();
            if ((int)CurrentTokenType > 7)
            {
                throw CommonUtil.DisplayDataErrorException(Data, Position, $"No object found at offset, found token of type {CurrentTokenType.ToString()}");
            }
            var obj = Context.GetPdfItem(Data, Position, out var length);
            Position += length;
            CurrentTokenType = PdfTokenType.Unknown;
            CurrentLength = 0;
            return obj;
        }
    }
}
