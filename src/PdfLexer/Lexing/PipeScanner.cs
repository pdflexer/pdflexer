using PdfLexer.Parsers;
using PdfLexer.Parsers.Nested;
using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;

namespace PdfLexer.Lexing
{
    public ref struct PipeScanner
    {
        private ParsingContext Context;
        private PipeReader Pipe;
        private SequenceReader<byte> Reader;
        public long CurrentOffset;

        public PipeScanner(ParsingContext ctx, PipeReader pipe)
        {
            Context = ctx;
            Pipe = pipe;
            IsCompleted = false;
            CurrentTokenType = PdfTokenType.TBD;
            Reader = default;
            CurrentStart = default;
            CurrentEnd = default;
            CurrentOffset = 0;
            InitReader();
        }

        public PdfTokenType CurrentTokenType;
        private SequencePosition CurrentStart;
        private SequencePosition CurrentEnd;
        private bool IsCompleted;

        private void InitReader()
        {
            // why no sync version? review this
            var result = Pipe.ReadAsync().GetAwaiter().GetResult();
            IsCompleted = result.IsCompleted;
            Reader = new SequenceReader<byte>(result.Buffer);
        }

        public long GetOffset()
        {
            return CurrentOffset + Reader.Consumed;
        }

        public long GetStartOffset()
        {
            Peek();
            return CurrentOffset + Reader.Sequence.Slice(Reader.Sequence.Start, CurrentStart).Length; // :(
        }

        private void AdvanceBuffer(SequencePosition pos)
        {
            // UGH.. better way? Sequence.GetOffset for net5.0 but not sure how that behaves
            // since we are recreating sequence reader... data is from same pipereader so probably is
            // what we want
            CurrentOffset += Reader.Sequence.Slice(Reader.Sequence.Start, pos).Length;
            Pipe.AdvanceTo(pos, Reader.Sequence.End);
            InitReader();
            CurrentStart = Reader.Position;
            return;
        }

        public PdfTokenType Peek()
        {
            if (CurrentTokenType != PdfTokenType.TBD)
            {
                return CurrentTokenType;
            }
            if (!PdfSequenceLexer.TryReadNextToken(ref Reader, false, out CurrentTokenType, out CurrentStart))
            {
                if (IsCompleted)
                {
                    CurrentTokenType = PdfTokenType.EOS;
                    return CurrentTokenType;
                }
                CurrentTokenType = PdfTokenType.TBD;
                AdvanceBuffer(CurrentStart);
                Peek();
            }
            CurrentEnd = Reader.Position;
            return CurrentTokenType;
        }

        public void SkipCurrent()
        {
            Peek();
            ThrowIfAtEndOfData();
            CurrentTokenType = PdfTokenType.TBD;
            CurrentStart = CurrentEnd;
        }

        public void SkipExpected(PdfTokenType type)
        {
            Peek();
            ThrowIfAtEndOfData();
            if (type != CurrentTokenType)
            {
                throw CommonUtil.DisplayDataErrorException(ref Reader, $"Mismatched token, found {CurrentTokenType}, expected {type}");
            }
            CurrentTokenType = PdfTokenType.TBD;
        }

        private void ThrowIfAtEndOfData()
        {
            if (CurrentTokenType == PdfTokenType.EOS)
            {
                Reader.Rewind(10);
                throw CommonUtil.DisplayDataErrorException(ref Reader, $"End of data reached");
            }
        }

        public void Advance(int cnt)
        {
            
            while (Reader.Remaining < cnt)
            {
                cnt -= (int)Reader.Remaining;
                AdvanceBuffer(Reader.Sequence.End);
            }
            Reader.Advance(cnt);
            CurrentTokenType = PdfTokenType.TBD;
        }

        public void SkipObject()
        {
            Peek();
            ThrowIfAtEndOfData();
            if ((int)CurrentTokenType > 7)
            {
                throw CommonUtil.DisplayDataErrorException(ref Reader, $"No object found at offset, found token of type {CurrentTokenType.ToString()}");
            }
            if (CurrentTokenType == PdfTokenType.ArrayStart)
            {
                SkipArray();
            }
            else if (CurrentTokenType == PdfTokenType.DictionaryStart)
            {
                SkipDict();
            }
            CurrentTokenType = PdfTokenType.TBD;
        }

        private void SkipDict()
        {

            while (true)
            {
                if (!NestedUtil.AdvanceToDictEnd(ref Reader, out _))
                {
                    if (IsCompleted)
                    {
                        Context.Error("Found end of data before dictionary end" + CommonUtil.GetDataErrorInfo(ref Reader));
                        CurrentEnd = Reader.Sequence.End;
                        return;
                    }
                    AdvanceBuffer(CurrentStart);
                    Reader.Advance(2); // <<
                    continue;
                }
                break;
            }
            CurrentEnd = Reader.Position;
        }
        private void SkipArray()
        {
            while (true)
            {

                if (!NestedUtil.AdvanceToArrayEnd(ref Reader, out _))
                {
                    if (IsCompleted)
                    {
                        Context.Error("Found end of data before array end" + CommonUtil.GetDataErrorInfo(ref Reader));
                        CurrentEnd = Reader.Sequence.End;
                        return;
                    }
                    AdvanceBuffer(CurrentStart);
                    Reader.Advance(1); // [
                    continue;
                }
                break;
            }
            CurrentEnd = Reader.Position;
        }

        public IPdfObject GetCurrentObject()
        {
            Peek();
            ThrowIfAtEndOfData();
            if ((int)CurrentTokenType > 7)
            {
                throw CommonUtil.DisplayDataErrorException(ref Reader, $"No object found at offset, found token of type {CurrentTokenType.ToString()}");
            }
            if (CurrentTokenType == PdfTokenType.ArrayStart)
            {
                SkipArray();
            }
            else if (CurrentTokenType == PdfTokenType.DictionaryStart)
            {
                SkipDict();
            }

            var obj = Context.GetPdfItem((PdfObjectType)CurrentTokenType, Reader.Sequence, CurrentStart, CurrentEnd);
            CurrentTokenType = PdfTokenType.TBD;
            return obj;
        }

        public ReadOnlySequence<byte> ReadTo(ReadOnlySpan<byte> sequence)
        {
            var start = Reader.Position;
            while (true)
            {
                if (!Reader.TryAdvanceTo(sequence[0], false) || Reader.Remaining < sequence.Length)
                {
                    if (IsCompleted)
                    {
                        throw CommonUtil.DisplayDataErrorException(ref Reader, "Sequence not found to read to " + System.Text.Encoding.ASCII.GetString(sequence));
                    }
                    AdvanceBuffer(start);
                    continue;
                }

                if (!Reader.IsNext(sequence, false))
                {
                    Reader.Advance(1);
                    continue;
                }

                return Reader.Sequence.Slice(start, Reader.Position);
            }

        }

        public ReadOnlySequence<byte> Read(int total)
        {
            var start = Reader.Position;
            while (Reader.Remaining < total)
            {
                AdvanceBuffer(CurrentStart);
            }
            Reader.Advance(total);
            var data = Reader.Sequence.Slice(CurrentStart, Reader.Position);
            CurrentStart = Reader.Position;
            CurrentTokenType = PdfTokenType.TBD;
            return data;
        }

        public bool TrySkipToWhiteSpace()
        {
            while (true)
            {
                if (!Reader.TryAdvanceToAny(CommonUtil.WhiteSpaces, true))
                {
                    if (IsCompleted)
                    {
                        return false;
                    }
                    AdvanceBuffer(Reader.Sequence.End);
                    continue;
                }
                CurrentTokenType = PdfTokenType.TBD;
                return true;
            }
        }

        public bool TrySkipToToken(ReadOnlySpan<byte> sequence, int prevBuffer)
        {
            if (prevBuffer < 1)
            {
                prevBuffer = 1;
            }
            CurrentTokenType = PdfTokenType.TBD;
            while (true)
            {
                if (!Reader.TryAdvanceTo(sequence[0], false) || Reader.Remaining < sequence.Length)
                {
                    if (IsCompleted)
                    {
                        return false;
                    }
                    var e = Reader.Remaining - prevBuffer;
                    if (e > 0)
                    {
                        Reader.Advance(Reader.Remaining - prevBuffer);
                        AdvanceBuffer(Reader.Position);
                    }
                    else
                    {
                        // need to keep enough in prevbuffer
                        Reader.Rewind(-e);
                        AdvanceBuffer(Reader.Position);
                        Reader.Advance(-e);
                    }
                    continue;
                }

                if (Reader.IsNext(sequence, false))
                {
                    if (Reader.Consumed == 0) // special handling for match at start of sequence
                    {
                        return true;
                    }
                    Reader.Rewind(1);
                    if (!Reader.TryPeek(out var b))
                    {
                        Reader.Advance(1);
                        return false;
                    }

                    if (!CommonUtil.IsWhiteSpace(b))
                    {
                        Reader.Advance(2);
                        continue;
                    }
                    Reader.Advance(1);
                    return true;
                }

                if (IsCompleted)
                {
                    return false;
                }

                Reader.Advance(1);
                continue;
            }
        }

        public int ScanBackTokens(int count, int maxScan)
        {
            CurrentTokenType = PdfTokenType.TBD;
            count += 1;
            var total = 0;
            var cnt = 0;
            var isWhite = false;
            while (total < maxScan && Reader.Consumed > 0)
            {
                Reader.Rewind(1);
                total++;
                if (!Reader.TryPeek(out var b))
                {
                    break;
                }
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
                    return total;
                }
            }
            return -1;
        }
    }
}
