using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using PdfLexer.Parsers;

namespace PdfLexer.Lexing
{
    public static class SequenceExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ctx"></param>
        /// <param name="results"></param>
        /// <param name="tokens"></param>
        /// <returns></returns>
        internal static async Task<SequencePosition> ReadTokenSequence(this PipeReader reader, ParsingContext ctx, List<IPdfObject> results, params PdfTokenType[] tokens)
        {
            Debug.Assert(ctx.ParseState == ParseState.None);
            SequencePosition lastPos = default;
            int count = 0;
            while (true)
            {
                var result = await reader.ReadAsync();
                if (result.TryReadTokenSequence(ctx, tokens, results, out var pos))
                {
                    return pos;
                }
               
                if (result.IsCompleted || result.IsCanceled)
                {
                    throw CommonUtil.DisplayDataErrorException(result.Buffer, pos, "Unable to read token sequence.");
                }
                if (pos.Equals(lastPos))
                {
                    count++;
                    if (count > 5)
                    {
                        throw CommonUtil.DisplayDataErrorException(result.Buffer, pos, "Sequence positions did not advance, buffer likely too small.");
                    }
                }
                else
                {
                    count = 0;
                    lastPos = pos;
                }

                reader.AdvanceTo(pos, result.Buffer.End);
            }
        }

        private static bool TryReadTokenSequence(this ReadResult read, ParsingContext ctx, PdfTokenType[] tokens, List<IPdfObject> results, out SequencePosition pos)
        {
            var reader = new SequenceReader<byte>(read.Buffer);
            switch (ctx.ParseState)
            {
                case ParseState.None:
                    break;
                case ParseState.Nested:
                    while (ctx.NestedSeqParser.ParseNestedItem(ref reader, read.IsCompleted)) { }
                    if (ctx.NestedSeqParser.completed)
                    {
                        results.Add(ctx.NestedSeqParser.GetCompletedObject());
                        ctx.ParseState = ParseState.None;
                    } else
                    {
                        pos = reader.Position;
                        ctx.ParseState = ParseState.Nested;
                        return false;
                    }

                    if (results.Count >= tokens.Length)
                    {
                        pos = reader.Position;
                        return true;
                    }
                    break;
            }
            
            while (reader.TryReadNextToken(read.IsCompleted, out var type, out var start))
            {
                var expected = tokens[results.Count];
                if (expected != PdfTokenType.WildCard && type != expected)
                {
                    throw CommonUtil.DisplayDataErrorException(ref reader, $"Unexpected token type, got {type} instead of {expected}");
                }

                if (type == PdfTokenType.DictionaryStart || type == PdfTokenType.ArrayStart)
                {
                    // special case for nested
                    reader.Rewind(type == PdfTokenType.DictionaryStart ? 2 : 1);
                    while (ctx.NestedSeqParser.ParseNestedItem(ref reader, read.IsCompleted)) { }

                    if (ctx.NestedSeqParser.completed)
                    {
                        results.Add(ctx.NestedSeqParser.GetCompletedObject());
                        continue;
                    } else
                    {
                        ctx.ParseState = ParseState.Nested;
                        pos = reader.Position;
                        return false;
                    }
                }

                if ((int)type < 7) // objects to parse
                {
                    results.Add(ctx.GetPdfItem((PdfObjectType) type, read.Buffer, start, reader.Position));
                }
                else
                {
                    results.Add(null);
                }
                if (results.Count >= tokens.Length)
                {
                    pos = reader.Position;
                    return true;
                }
            }
            pos = reader.Position;
            return false;

        }

        internal static async Task<ReadOnlySequence<byte>> LocateXrefTableAndTrailer(this PipeReader reader)
        {
            SequencePosition firstStart = default;
            int stage = 0;
            while (true)
            {
                var result = await reader.ReadAsync();
                var before = stage;
                if (result.TryReadXrefData(ref stage, out var thisStart, out var end))
                {
                    if (before == 0)
                    {
                        return result.Buffer.Slice(thisStart, end);
                    }
                    else
                    {
                        return result.Buffer.Slice(firstStart, end);
                    }
                    
                }

                if (stage == 1 && before == 0)
                {
                    firstStart = thisStart;
                }
               
                if (result.IsCompleted || result.IsCanceled)
                {
                    throw CommonUtil.DisplayDataErrorException(result.Buffer, thisStart, "Unable to read xref table.");
                }
                reader.AdvanceTo(thisStart);
            }
        }

        private static bool TryReadXrefData(this ReadResult read, ref int stage, out SequencePosition start,
            out SequencePosition end)
        {
            var reader = new SequenceReader<byte>(read.Buffer);
            if (stage == 0)
            {
                if (!reader.TryReadNextToken(read.IsCompleted, out var type, out start))
                {
                    end = reader.Position;
                    return false;
                }
                if (type != PdfTokenType.Xref)
                {
                    throw CommonUtil.DisplayDataErrorException(ref reader, "Xref statement not found");
                }
                stage++;
            }
            else
            {
                start = reader.Position;
            }
            if (stage == 1)
            {
                if (!reader.TryAdvanceTo((byte) 't', false))
                {
                    end = reader.Position;
                    return false;
                }
                if (reader.Remaining < 6)
                {
                    end = reader.Position;
                    return false;
                }

                if (reader.IsNext(XRefParser.trailer, false))
                {
                    end = reader.Position;
                    return true;
                }

                throw CommonUtil.DisplayDataErrorException(ref reader, "Trailer");
            }

            throw new ApplicationException("Unknown stage for Xref parsing.");
        }

        

        internal static async ValueTask<IPdfObject> ReadNextObject(this PipeReader reader, ParsingContext ctx)
        {
            while (true)
            {
                var result = await reader.ReadAsync();
                if (result.TryReadNextToken(out var type, out var start, out var end ))
                {
                    if (type == PdfTokenType.NumericObj)
                    {

                    }
                    var buffer = result.Buffer;
                    var item = ctx.GetPdfItem((PdfObjectType)type, in buffer, start, end);
                    reader.AdvanceTo(end);
                    return item;
                }
                if (result.IsCompleted || result.IsCanceled)
                {
                    throw CommonUtil.DisplayDataErrorException(result.Buffer, start, "Unable to read object");
                }
                reader.AdvanceTo(start);
            }
        }

        internal static bool TryReadNextToken(this ReadResult read, out PdfTokenType type,
            out SequencePosition start, out SequencePosition end)
        {
            var reader = new SequenceReader<byte>(read.Buffer);
            var result = reader.TryReadNextToken(read.IsCompleted, out type, out start);
            end = reader.Position;
            if (!result || type != PdfTokenType.NumericObj)
            {
                return result;
            }

            result = reader.TryReadNextToken(read.IsCompleted, out var secondType, out _);
            if (!result || secondType != PdfTokenType.NumericObj)
            {
                return result;
            }

            result = reader.TryReadNextToken(read.IsCompleted, out var thirdType, out _);
            if (!result || thirdType != PdfTokenType.IndirectRef)
            {
                return result;
            }
            end = reader.Position;
            type = PdfTokenType.IndirectRef;
            return true;
        }
    }
}
