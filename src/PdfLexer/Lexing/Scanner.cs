﻿using PdfLexer.Parsers;
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
            CurrentTokenType = PdfTokenType.TBD;
            CurrentLength = 0;
        }

        public PdfTokenType CurrentTokenType;
        public int CurrentLength;
        public int Position;

        public PdfTokenType Peek()
        {
            if (CurrentTokenType == PdfTokenType.TBD)
            {
                Position = PdfSpanLexer.TryReadNextToken(Data, out CurrentTokenType, Position, out CurrentLength);
            }
            return CurrentTokenType;
        }

        public void SkipCurrent()
        {
            Peek();
            ThrowIfAtEndOfData();
            Position += CurrentLength;
            CurrentTokenType = PdfTokenType.TBD;
        }

        public void SkipExpected(PdfTokenType type)
        {
            Peek();
            ThrowIfAtEndOfData();
            if (type != CurrentTokenType)
            {
                var info = CommonUtil.GetDataErrorInfo(Data, Position);
                throw new PdfLexerTokenMismatchException($"Mismatched token, found {CurrentTokenType}, expected {type}: " + info);
            }
            Position += CurrentLength;
            CurrentTokenType = PdfTokenType.TBD;
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
            CurrentTokenType = PdfTokenType.TBD;
        }

        public int SkipObject()
        {
            Peek();
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
            CurrentTokenType = PdfTokenType.TBD;
            return Position - start;
        }

        public void ScanToToken(ReadOnlySpan<byte> token)
        {
            var loc = Data.Slice(Position).IndexOf(token);
            if (loc == -1)
            {
                CurrentTokenType = PdfTokenType.EOS;
            }
            else
            {
                Position = Position+loc;
                CurrentTokenType = PdfTokenType.TBD;
            }
        }

        public IPdfObject GetCurrentObject()
        {
            Peek();
            ThrowIfAtEndOfData();
            if ((int)CurrentTokenType > 7)
            {
                throw CommonUtil.DisplayDataErrorException(Data, Position, $"No object found at offset, found token of type {CurrentTokenType.ToString()}");
            }
            var obj = Context.GetPdfItem(Data, Position, out var length);
            Position += length;
            CurrentTokenType = PdfTokenType.TBD;
            return obj;
        }

        public bool TryFindEndStream()
        {
            var pos = Data.Slice(Position).IndexOf(IndirectSequences.endstream);
            if (pos != -1)
            {
                Position += pos;
                if (Data[Position - 1] == (byte)'\n')
                {
                    Position--;
                }
                return true;
            }

            var original = Position;
            Position = 0;
            pos = 0;
            var lastPos = 0;

            while (pos < original - Position && (pos = Data.Slice(Position).IndexOf(IndirectSequences.endstream)) != -1)
            {
                lastPos = Position + pos;
                Position += pos + 1;
            }

            if (lastPos == 0)
            {
                Position = original;
                return false;
            }

            Position = lastPos;
            if (Data[Position - 1] == (byte)'\n')
            {
                Position--;
            }
            return true;
        }

        public bool TryScanBackTokens(int count, int maxScan)
        {
            CurrentTokenType = PdfTokenType.TBD;
            count += 1;
            var total = 0;
            var cnt = 0;
            var isWhite = false;
            while (total < maxScan && Position > 0)
            {
                Position--;
                total++;
                var b = Data[Position];
                if (!CommonUtil.IsNonBinary(b))
                {
                    break;
                }
                if (CommonUtil.IsWhiteSpace(b))
                {
                    if (!isWhite)
                    {
                        cnt++;
                    }
                    isWhite = true;
                }
                else
                {
                    isWhite = false;
                }

                if (cnt >= count)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
