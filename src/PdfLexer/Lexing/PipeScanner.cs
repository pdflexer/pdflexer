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
        public long Offset;

        public PipeScanner(ParsingContext ctx, PipeReader pipe)
        {
            Context = ctx;
            Pipe = pipe;
            IsCompleted = false;
            CurrentTokenType = PdfTokenType.Unknown;
            Reader = default;
            CurrentStart = default;
            CurrentEnd = default;
            Offset = 0;
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
            return Offset + Reader.Consumed;
        }

        private void AdvanceBuffer(SequencePosition pos)
        {
            int count = 0;
            var end = Reader.Sequence.End;
            Offset += Reader.Sequence.Slice(Reader.Sequence.Start, pos).Length; // UGH.. TODO: use GetOffset conditionally for net50
            while (true)
            {
                Pipe.AdvanceTo(pos, Reader.Sequence.End);
                InitReader();
                CurrentStart = Reader.Position;
                if (!Reader.Sequence.End.Equals(end))
                {
                    return;
                }
                count++;
                if (count > 5)
                {
                    throw CommonUtil.DisplayDataErrorException(ref Reader, $"Stream did not advance.");
                }
            }

        }

        public PdfTokenType Peak()
        {
            if (CurrentTokenType != PdfTokenType.Unknown)
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
                CurrentTokenType = PdfTokenType.Unknown;
                AdvanceBuffer(CurrentStart);
                Peak();
            }
            CurrentEnd = Reader.Position;
            return CurrentTokenType;
        }

        public void SkipCurrent()
        {
            Peak();
            ThrowIfAtEndOfData();
            CurrentTokenType = PdfTokenType.Unknown;
        }

        public void SkipExpected(PdfTokenType type)
        {
            Peak();
            ThrowIfAtEndOfData();
            if (type != CurrentTokenType)
            {
                throw CommonUtil.DisplayDataErrorException(ref Reader, $"Mismatched token, found {CurrentTokenType}, expected {type}");
            }
            CurrentTokenType = PdfTokenType.Unknown;
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
            Reader.Advance(cnt);
            Offset += cnt;
            CurrentTokenType = PdfTokenType.Unknown;
        }

        public void SkipObject()
        {
            Peak();
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
            CurrentTokenType = PdfTokenType.Unknown;
        }

        private void SkipDict()
        {
            
            while (true)
            {
                Reader.Advance(2);
                if (!NestedUtil.AdvanceToDictEnd(ref Reader, out _))
                {
                    if (IsCompleted)
                    {
                        Reader.Rewind(10);
                        throw CommonUtil.DisplayDataErrorException(ref Reader, $"Unable to find dictionary end");
                    }
                    AdvanceBuffer(CurrentStart);
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
                Reader.Advance(1);
                if (!NestedUtil.AdvanceToArrayEnd(ref Reader, out _))
                {
                    if (IsCompleted)
                    {
                        Reader.Rewind(10);
                        throw CommonUtil.DisplayDataErrorException(ref Reader, $"Unable to find array end");
                    }
                    AdvanceBuffer(CurrentStart);
                    continue;
                }
                break;
            }
            CurrentEnd = Reader.Position;
        }

        public IPdfObject GetCurrentObject()
        {
            Peak();
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
            CurrentTokenType = PdfTokenType.Unknown;
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

        public bool TrySkipTo(ReadOnlySpan<byte> sequence, int prevBuffer)
        {

            while (true)
            {
                var start = Reader.Consumed;
                if (!Reader.TryAdvanceTo(sequence[0], false) || Reader.Remaining < sequence.Length)
                {
                    if (IsCompleted)
                    {
                        return false;
                    }
                    Reader.Advance(Reader.Remaining - prevBuffer);
                    AdvanceBuffer(Reader.Position);
                    continue;
                }

                if (Reader.IsNext(sequence, false))
                {
                    Reader.Rewind(prevBuffer);
                    Offset += Reader.Consumed - start;
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
    }
}
