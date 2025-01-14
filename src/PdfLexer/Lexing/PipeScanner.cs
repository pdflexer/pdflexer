﻿using PdfLexer.Parsers.Nested;
using System.Buffers;
using System.IO.Pipelines;

namespace PdfLexer.Lexing;

internal ref struct PipeScanner
{
    private ParsingContext Context;
    private PipeReader Pipe;
    public SequenceReader<byte> Reader;
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
    private long CurrentStart;
    private long CurrentEnd;
    private bool IsCompleted;

    private ReadOnlySequence<byte> LastRead;

    private void InitReader()
    {
        // why no sync version? review this
        var result = Pipe.ReadAsync().AsTask().GetAwaiter().GetResult();
        IsCompleted = result.IsCompleted;
        LastRead = result.Buffer;
        Reader = new SequenceReader<byte>(result.Buffer);
        
    }
    private void InitReader(SequencePosition pos)
    {
        var result = Pipe.ReadAsync().AsTask().GetAwaiter().GetResult();
        IsCompleted = result.IsCompleted;
        LastRead = result.Buffer;
        Reader = new SequenceReader<byte>(result.Buffer.Slice(pos));
    }

    public long GetOffset()
    {
        return CurrentOffset + Reader.Consumed;
    }

    public long GetStartOffset()
    {
        Peek();
        return CurrentOffset + CurrentStart;
    }

    private void AdvanceBuffer(SequencePosition pos)
    {
        CurrentOffset += Reader.Sequence.Slice(Reader.Sequence.Start, pos).Length;
        Pipe.AdvanceTo(pos, Reader.Sequence.End);
        InitReader();
        CurrentStart = 0;
        return;
    }

    // read more data and advance reader to start of new data but don't update positioning
    private void AdvanceBufferSkipReaderToEnd(SequencePosition pos)
    {
        var l = LastRead.Length;
        Pipe.AdvanceTo(pos, Reader.Sequence.End);
        InitReader(LastRead.End);
        CurrentStart = l;
        return;
    }

    private void AdvanceBuffer(long count)
    {
        CurrentOffset += count;
        Pipe.AdvanceTo(Reader.Sequence.GetPosition(count), Reader.Sequence.End);
        InitReader();
        CurrentStart = 0;
        return;
    }

    public PdfTokenType Peek()
    {
        if (CurrentTokenType != PdfTokenType.TBD)
        {
            return CurrentTokenType;
        }
        if (!PdfSequenceLexer.TryReadNextToken(ref Reader, IsCompleted, out CurrentTokenType, out CurrentStart))
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
        CurrentEnd = Reader.Consumed;
        return CurrentTokenType;
    }

    public void SkipCurrent()
    {
        Peek();
        ThrowIfAtEndOfData();
        CurrentTokenType = PdfTokenType.TBD;
        CurrentStart = CurrentEnd;
    }

    public ReadOnlySequence<byte> GetAndSkipCurrentData()
    {
        Peek();
        ThrowIfAtEndOfData();
        if (CurrentTokenType == PdfTokenType.ArrayStart)
        {
            SkipArray();
        }
        else if (CurrentTokenType == PdfTokenType.DictionaryStart)
        {
            SkipDict();
        }
        CurrentTokenType = PdfTokenType.TBD;
        return Reader.Sequence.Slice(CurrentStart, CurrentEnd - CurrentStart);
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

    public bool Advance(int cnt)
    {
        while (Reader.Remaining < cnt && !IsCompleted)
        {
            cnt -= (int)Reader.Remaining;
            AdvanceBuffer(Reader.Sequence.End);
        }
        if (IsCompleted == true && cnt > Reader.Remaining)
        {
            return false;
        }
        Reader.Advance(cnt);
        CurrentTokenType = PdfTokenType.TBD;
        return true;
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
                    CurrentEnd = Reader.Sequence.Length;
                    return;
                }
                AdvanceBuffer(CurrentStart);
                Reader.Advance(2); // <<
                continue;
            }
            break;
        }
        CurrentEnd = Reader.Consumed;
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
                    CurrentEnd = Reader.Sequence.Length;
                    return;
                }
                AdvanceBuffer(CurrentStart);
                Reader.Advance(1); // [
                continue;
            }
            break;
        }
        CurrentEnd = Reader.Consumed;
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
        var obj = Context.GetPdfItem((PdfObjectType)CurrentTokenType, Reader.Sequence.Slice(CurrentStart, CurrentEnd - CurrentStart));
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
                AdvanceBufferSkipReaderToEnd(start);
                continue;
            }

            if (!Reader.IsNext(sequence, false))
            {
                Reader.Advance(1);
                continue;
            }
            
            return LastRead.Slice(start, Reader.Position);
        }
    }

    public bool ScanToToken(ReadOnlySpan<byte> token)
    {
        var start = Reader.Position;
        while (true)
        {
            if (!Reader.TryAdvanceTo(token[0], false) || Reader.Remaining < token.Length)
            {
                if (IsCompleted)
                {
                    return false;
                }
                AdvanceBuffer(start);
                continue;
            }

            if (!Reader.IsNext(token, false))
            {
                Reader.Advance(1);
                continue;
            }

            return true;
        }
    }

    public ReadOnlySequence<byte> Read(int total)
    {
        while (Reader.Remaining < total)
        {
            AdvanceBuffer(CurrentStart);
        }
        Reader.Advance(total);
        var data = Reader.Sequence.Slice(CurrentStart, Reader.Position);
        CurrentStart = Reader.Consumed;
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
                var e = Reader.Remaining - prevBuffer - sequence.Length;
                if (e > 0)
                {
                    Reader.Advance(e);
                    AdvanceBuffer(Reader.Position);
                }
                else
                {
                    if (-e > Reader.Consumed) { e = -Reader.Consumed; }
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
                return total - 1;
            }
        }
        return -1;
    }
}
