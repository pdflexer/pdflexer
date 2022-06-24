using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Nested;

namespace PdfLexer.Lexing
{
    public static class SequenceExtensions
    {
        // internal static long GetExtraReadCount(this PipeReader reader)
        // {
        //     if (!reader.TryRead(out var result))
        //     {
        //         return 0;
        //     }
        // 
        //     return result.Buffer.Length; 
        // }
        // 
        // internal static byte[] GetExtraReadData(this PipeReader reader, int length)
        // {
        //     if (!reader.TryRead(out var result))
        //     {
        //         throw new NotSupportedException("Tried reading remaining data out of reader that did not have data.");
        //     }
        //     return result.Buffer.Slice(0, length).ToArray();
        // }
        // 
        // internal static async ValueTask<(int length, bool WasStream)> GetIndirectObjectData(this PipeReader reader, ParsingContext ctx)
        // {
        //     SequencePosition lastPos = default;
        //     int count = 0;
        //     int stage = 0;
        //     while (true)
        //     {
        //         var result = await reader.ReadAsync().ConfigureAwait(false);
        //         if (result.TryReadToIndirectEnd(ctx, result.IsCompleted, ref stage, out var pos))
        //         {
        //             reader.AdvanceTo(pos);
        //             return ((int)result.Buffer.Slice(result.Buffer.Start, pos).Length, stage == 2);
        //         }
        // 
        //         if (result.IsCompleted || result.IsCanceled)
        //         {
        //             throw CommonUtil.DisplayDataErrorException(result.Buffer, pos, "Unable to read token sequence");
        //         }
        //         if (pos.Equals(lastPos))
        //         {
        //             count++;
        //             if (count > 5)
        //             {
        //                 throw CommonUtil.DisplayDataErrorException(result.Buffer, pos, "Sequence positions did not advance, buffer likely too small.");
        //             }
        //         }
        //         else
        //         {
        //             count = 0;
        //             lastPos = pos;
        //         }
        // 
        //         reader.AdvanceTo(result.Buffer.Start, result.Buffer.End);
        //     }
        // }
        // 
        // private static bool TryReadToIndirectEnd(this ReadResult read, ParsingContext ctx, bool isCompleted, ref int stage, out SequencePosition readTo)
        // {
        //     var reader = new SequenceReader<byte>(read.Buffer);
        //     readTo = reader.Position;
        //     if (stage == 0)
        //     {
        //         if (!reader.TryReadNextToken(read.IsCompleted, out var type, out var pos))
        //         {
        //             return false;
        //         }
        //         if (type != PdfTokenType.NumericObj)
        //         {
        //             throw CommonUtil.DisplayDataErrorException(ref reader, "Unexpected token when trying to read indirect object");
        //         }
        //         if (!reader.TryReadNextToken(read.IsCompleted, out type, out pos))
        //         {
        //             return false;
        //         }
        //         if (type != PdfTokenType.NumericObj)
        //         {
        //             throw CommonUtil.DisplayDataErrorException(ref reader, "Unexpected token when trying to read indirect object");
        //         }
        //         if (!reader.TryReadNextToken(read.IsCompleted, out type, out pos))
        //         {
        //             return false;
        //         }
        //         if (type != PdfTokenType.StartObj)
        //         {
        //             throw CommonUtil.DisplayDataErrorException(ref reader, "Unexpected token when trying to read indirect object");
        //         }
        //         stage++;
        //     }
        //     readTo = reader.Position;
        //     if (stage == 1)
        //     {
        //         if (!reader.TryReadNextToken(read.IsCompleted, out var type, out var pos))
        //         {
        //             return false;
        //         }
        //         if (type == PdfTokenType.DictionaryStart )
        //         {
        //             if (!reader.AdvanceToDictEnd(out _))
        //             {
        //                 return false;
        //             }
        //             if (!reader.TryReadNextToken(read.IsCompleted, out type, out pos))
        //             {
        //                 return false;
        //             }
        //             if (type == PdfTokenType.StartStream)
        //             {
        //                 stage++;
        //                 return true;
        //                 
        //             } else if (type == PdfTokenType.EndObj)
        //             {
        //                 readTo = reader.Position;
        //                 return true;
        //             } else
        //             {
        //                 throw CommonUtil.DisplayDataErrorException(ref reader, "Unexpected token when trying to read indirect object");
        //             }
        //         }
        //         else
        //         {
        //             if (type == PdfTokenType.ArrayStart && !reader.AdvanceToArrayEnd(out _))
        //             {
        //                 return false;
        //             }
        //             if (!reader.TryReadNextToken(read.IsCompleted, out type, out pos))
        //             {
        //                 return false;
        //             }
        //             if (type != PdfTokenType.EndObj)
        //             {
        //                 throw CommonUtil.DisplayDataErrorException(ref reader, "Unexpected token when trying to read indirect object");
        //             }
        //             readTo = reader.Position;
        //             return true;
        //         }
        //     }
        //     throw new ApplicationException("Unknown parse stage: " + stage);
        // }


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
            SequencePosition lastPos = default;
            int count = 0;
            while (true)
            {
                var result = await reader.ReadAsync().ConfigureAwait(false);
                if (result.TryReadTokenSequence(ctx, ops, results, out var pos))
                {
                    reader.AdvanceTo(pos);
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
            }

            while (reader.TryReadNextToken(read.IsCompleted, out var type, out var start))
            {
                var expected = op.Token;
                if (expected != PdfTokenType.WildCard && type != expected)
                {
                    throw CommonUtil.DisplayDataErrorException(ref reader, $"Unexpected token type, got {type} instead of {expected}");
                }

                if (type == PdfTokenType.ArrayStart)
                {
                    if (!reader.AdvanceToArrayEnd(out _))
                    {
                        pos = start;
                        return false;
                    }
                }
                else if (type == PdfTokenType.DictionaryStart)
                {

                    if (!reader.AdvanceToDictEnd(out _))
                    {
                        pos = start;
                        return false;
                    }
                }

                if ((int)type < 9) // objects to parse
                {
                    results.Add(ctx.GetPdfItem((PdfObjectType)type, read.Buffer, start, reader.Position));
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

            if (!reader.TryReadNextToken(read.IsCompleted, out type, out var start))
            {
                obj = null;
                pos = start;
                return false;
            }


            if (type == PdfTokenType.ArrayStart)
            {
                if (!reader.AdvanceToArrayEnd(out _))
                {
                    pos = start;
                    obj = null;
                    return false;
                }
            }
            else if (type == PdfTokenType.DictionaryStart)
            {

                if (!reader.AdvanceToDictEnd(out _))
                {
                    pos = start;
                    obj = null;
                    return false;
                }
            }

            pos = reader.Position;

            if ((int)type < 9) // objects to parse
            {
                obj = ctx.GetPdfItem((PdfObjectType)type, reader.Sequence, start, reader.Position);
            }
            else
            {
                obj = null;
            }
            return true;
        }
    }
}
