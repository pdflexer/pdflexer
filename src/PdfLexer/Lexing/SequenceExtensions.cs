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
        internal static async Task<SequencePosition> ReadTokenSequence(this PipeReader reader, ParsingContext ctx, IReadOnlyList<ParseOp> ops, List<IPdfObject> results)
        {
            Debug.Assert(ctx.ParseState == ParseState.None);
            SequencePosition lastPos = default;
            int count = 0;
            while (true)
            {
                var result = await reader.ReadAsync();
                if (result.TryReadTokenSequence(ctx, ops, results, out var pos))
                {
                    return pos;
                }
               
                if (result.IsCompleted || result.IsCanceled)
                {
                    throw CommonUtil.DisplayDataErrorException(result.Buffer, pos, "Unable to read token sequence");
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

        private static bool TryReadTokenSequence(this ReadResult read, ParsingContext ctx, IReadOnlyList<ParseOp> ops, List<IPdfObject> results, out SequencePosition pos)
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

                    if (results.Count >= ops.Count)
                    {
                        pos = reader.Position;
                        return true;
                    }
                    break;
            }
            var op = ops[results.Count];
            switch (op.Type)
            {
                case ParseOpType.ReadToken:
                    break;
                case ParseOpType.ScanToAndSkip:
                    while (true)
                    {
                        if (!reader.TryAdvanceTo(op.ScanSequence[0], false))
                        {
                            reader.Advance(reader.Remaining);
                            pos = reader.Position;
                            return false;
                        }

                        if (reader.Remaining < op.ScanSequence.Length)
                        {
                            reader.Advance(reader.Remaining);
                            pos = reader.Position;
                            return false;
                        }

                        if (reader.IsNext(op.ScanSequence, true))
                        {
                            results.Add(null);
                            pos = reader.Position;
                            if (results.Count >= ops.Count)
                            {
                                return true;
                            }
                            return false;
                        }
                        reader.Advance(1);
                    }

                    //pos = reader.Position;
                    //return TryReadTokenSequence(read, ctx, ops, results, out pos);
            }

            while (reader.TryReadNextToken(read.IsCompleted, out var type, out var start))
            {
                var expected = op.Token;
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

                if ((int)type < 8) // objects to parse
                {
                    results.Add(ctx.GetPdfItem((PdfObjectType) type, read.Buffer, start, reader.Position));
                }
                else
                {
                    results.Add(null);
                }
                if (results.Count >= ops.Count)
                {
                    pos = reader.Position;
                    return true;
                }
                op = ops[results.Count];
            }
            pos = reader.Position;
            return false;

        }

        internal static async ValueTask<(PdfTokenType Type, IPdfObject Obj)> ReadNextObjectOrToken(this PipeReader pipe, ParsingContext ctx)
        {
            while (true)
            {
                var result = await pipe.ReadAsync();
                if (TryReadNextObject(result, ctx, out var pos, out var type, out var obj))
                {
                    pipe.AdvanceTo(pos);
                    return (type, obj);
                }

                if (result.IsCompleted || result.IsCanceled)
                {
                    throw CommonUtil.DisplayDataErrorException(result.Buffer, pos, "Unable to read object");
                }

                pipe.AdvanceTo(pos, result.Buffer.End);
            }
        }

        internal static bool TryReadNextObject(this ReadResult read, ParsingContext ctx, out SequencePosition pos, out PdfTokenType type, out IPdfObject obj)
        {
            var reader = new SequenceReader<byte>(read.Buffer);
            switch (ctx.ParseState)
            {
                case ParseState.None:
                    break;
                case ParseState.Nested:
                    while (ctx.NestedSeqParser.ParseNestedItem(ref reader, read.IsCompleted)) { }
                    pos = reader.Position;
                    if (ctx.NestedSeqParser.completed)
                    {
                        obj = ctx.NestedSeqParser.GetCompletedObject();
                        type = (PdfTokenType)obj.Type;
                        ctx.ParseState = ParseState.None;
                        return true;
                    } else {
                        ctx.ParseState = ParseState.Nested;
                        obj = null;
                        type = default;
                        return false;
                    }
            }

            if (!reader.TryReadNextToken(read.IsCompleted, out type, out var start)) {
                obj = null;
                pos = start;
                return false;
            }


            if (type == PdfTokenType.DictionaryStart || type == PdfTokenType.ArrayStart)
            {
                // special case for nested
                reader.Rewind(type == PdfTokenType.DictionaryStart ? 2 : 1);
                while (ctx.NestedSeqParser.ParseNestedItem(ref reader, read.IsCompleted)) { }

                pos = reader.Position;
                if (ctx.NestedSeqParser.completed)
                {
                    obj = ctx.NestedSeqParser.GetCompletedObject();
                    return true;
                } else
                {
                    obj = null;
                    ctx.ParseState = ParseState.Nested;
                    return false;
                }
            } else { 
                pos = reader.Position;
                
                if ((int)type < 8) // objects to parse
                {
                    obj = ctx.GetPdfItem((PdfObjectType) type, reader.Sequence, start, reader.Position);
                } else
                {
                    obj = null;
                }
                return true;
            } 
        }
    }
}
