using PdfLexer.Parsers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PdfLexer.Legacy
{
    [Obsolete]
    internal class StringStreams
    {
        private StringStatus Status = StringStatus.None;
        public PdfString Parse(PipeReader reader)
        {
            return ParseAsync(reader).GetAwaiter().GetResult();
        }

        public async ValueTask<PdfString> ParseAsync(PipeReader reader, CancellationToken cancellationToken = default)
        {
            Debug.Assert(Status == StringStatus.None);
            SequencePosition lastPos = default;
            int count = 0;
            if (reader.TryRead(out var result) && TryReadString(result, out var value, out var pos))
            {
                reader.AdvanceTo(pos);
                return value;
            }
            while (true)
            {
                result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                if (TryReadString(result, out value, out pos))
                {
                    reader.AdvanceTo(pos);
                    return value;
                }
               
                if (result.IsCompleted)
                {
                    throw CommonUtil.DisplayDataErrorException(result.Buffer, pos, "Unable to read string, ended at");
                }
                if (result.IsCanceled)
                {
                    throw new OperationCanceledException();
                }

                if (pos.Equals(lastPos))
                {
                    count++;
                    if (count > 5)
                    {
                        throw CommonUtil.DisplayDataErrorException(result.Buffer, pos, "Sequence position did not advance, buffer likely too small.");
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

        private bool TryReadString(ReadResult read, out PdfString result, out SequencePosition pos)
        {
            var reader = new SequenceReader<byte>(read.Buffer);
            var success = TryReadString(ref reader);
            if (success)
            {
                result = GetCurrentString();
            } else
            {
                result = null;
            }
            pos = reader.Position;
            return success;
        }

        internal PdfString GetCurrentString()
        {
            if (value == null)
            {
                var str = builder.ToString();
                builder.Clear();
                Status = StringStatus.None;
                return null;
                // return new PdfString(str);
            } else
            {
                var val = value;
                value = null;
                return val;
            }

        }
        
        internal bool TryReadString(ref SequenceReader<byte> reader)
        {
            switch (Status)
            {
                case StringStatus.None:
                    builder.Clear();
                    value = null;
                    break;
                case StringStatus.ParsingLiteral:
                    if (TryReadStringLiteral(ref reader))
                    {
                        Status = StringStatus.None;
                        return true;
                    }
                    return false;
                case StringStatus.ParsingHex:
                    if (TryReadStringHex(ref reader))
                    {
                        Status = StringStatus.None;
                        return true;
                    }
                    return false;
            }

            if (!reader.TryPeek(out byte b))
            {
                return false;
            }

            if (b == (byte)'(')
            {
                if (!TryReadStringLiteral(ref reader))
                {
                    Status = StringStatus.ParsingLiteral;
                    return false;
                }
                return true;
            } else if (b == (byte)'<')
            {
                if (!TryReadStringHex(ref reader))
                {
                    Status = StringStatus.ParsingHex;
                    return false;
                }
                return true;
            }

            throw CommonUtil.DisplayDataErrorException(ref reader, "Invalid string, first char not ( or <.");
        }

        private void AddToBuilder(in ReadOnlySequence<byte> data)
        {
            // TODO optimize.. for now this is easy
            foreach (var seg in data)
            {
                builder.Append(Iso88591.GetString(seg.Span));
            }
        }
        private PdfString value;
        internal StringBuilder builder = new StringBuilder();
        private static Encoding Iso88591 = Encoding.GetEncoding("ISO-8859-1"); // StandardEncoding
        private bool TryReadStringHex(ref SequenceReader<byte> reader)
        {
            throw new NotImplementedException();
            // var pos = reader.Position;
            // if (!reader.TryAdvanceTo((byte)'>'))
            // {
            //     return false;
            // }
            // 
            // var sequence = reader.Sequence.Slice(pos, reader.Position);
            // if (sequence.IsSingleSegment)
            // {
            //     value = Parse(sequence.FirstSpan);
            //     return true;
            // }
            // // TODO optimize
            // var len = (int)sequence.Length;
            // var buffer = ArrayPool<byte>.Shared.Rent(len);
            // sequence.CopyTo(buffer);
            // Span<byte> buff = buffer;
            // value = Parse(buff.Slice(0,len));
            // ArrayPool<byte>.Shared.Return(buffer);
            // return true;
        }

        // public PdfString Parse(ReadOnlySpan<byte> data, int start, out int length)
        // {
        //     // TODO optimize
        //     var b = data[start];
        //     if (b == '(')
        //     {
        //         return ParseStringLiteral(data.Slice(start), out length);
        //     } else if (b == (byte)'<')
        //     {
        //         return ParseStringHex(data.Slice(start), out length);
        //     }
        //     throw new ApplicationException("Invalid string, first char not ( or <.");
        // }

        private static byte[] stringLiteralTerms = new byte[]
        {
            (byte) '(', (byte) ')', (byte) '\\'
        };

        private int stringDepth = 0;
        private byte[] stringBuff = new byte[2];
        internal bool TryReadStringLiteral(ref SequenceReader<byte> reader)
        {
            var start = reader.Position;
            var initial = reader.Consumed;
            while (reader.TryAdvanceToAny(stringLiteralTerms, false))
            {
                if (!reader.TryRead(out byte b))
                {
                    return false;
                }
                switch (b)
                {
                    case (byte)'\\':
                        if (!reader.TryRead(out var b2))
                        {
                            AddToBuilder(reader.Sequence.Slice(start, reader.Consumed-initial-1));
                            reader.Rewind(1);
                            return false;
                        }
                        switch (b2)
                        {
                            case (byte)'n':
                                AddToBuilder(reader.Sequence.Slice(start,reader.Consumed-initial-2));
                                builder.Append('\n');
                                break;
                            case (byte)'r':
                                AddToBuilder(reader.Sequence.Slice(start,reader.Consumed-initial-2));
                                builder.Append('\r');
                                break;
                            case (byte)'t':
                                AddToBuilder(reader.Sequence.Slice(start,reader.Consumed-initial-2));
                                builder.Append('\t');
                                break;
                            case (byte)'b':
                                AddToBuilder(reader.Sequence.Slice(start,reader.Consumed-initial-2));
                                builder.Append('\b');
                                break;
                            case (byte)'f':
                                AddToBuilder(reader.Sequence.Slice(start,reader.Consumed-initial-2));
                                builder.Append('\f');
                                break;
                            case (byte)'(':
                                AddToBuilder(reader.Sequence.Slice(start,reader.Consumed-initial-2));
                                builder.Append('(');
                                break;
                            case (byte)')':
                                AddToBuilder(reader.Sequence.Slice(start,reader.Consumed-initial-2));
                                builder.Append(')');
                                break;
                            case (byte)'\\':
                                AddToBuilder(reader.Sequence.Slice(start,reader.Consumed-initial-2));
                                builder.Append('\\');
                                break;
                            case (byte)'\r':
                                if (reader.TryPeek(out var b3))
                                {
                                    if (b3 == (byte)'\n')
                                    {
                                        AddToBuilder(reader.Sequence.Slice(start,reader.Consumed-initial-2));
                                        reader.Advance(1);
                                        break;
                                    }
                                    AddToBuilder(reader.Sequence.Slice(start,reader.Consumed-initial-2));
                                    break;
                                }
                                return false;
                            case (byte)'\n':
                                AddToBuilder(reader.Sequence.Slice(start,reader.Consumed-initial-2));
                                break;
                            default:
                                if (b2 < 48 || b2 > 55)
                                {
                                    Ignore(ref reader, 1);
                                    break;
                                }

                                if (!reader.TryRead(out b3))
                                {
                                    reader.Rewind(2);
                                    return false;
                                }

                                if (b3 < 48 || b3 > 55)
                                {
                                    Ignore(ref reader, 2);
                                    break;
                                }

                                if (!reader.TryRead(out var b4))
                                {
                                    AddToBuilder(reader.Sequence.Slice(start,reader.Consumed-initial-3));
                                    builder.Append((char)(8*((int)b2-48)+((int)b3-48)));
                                    break;;
                                }

                                if (b4 < 48 || b4 > 55)
                                {
                                    AddToBuilder(reader.Sequence.Slice(start,reader.Consumed-initial-4));
                                    builder.Append((char)(8*((int)b2-48)+((int)b3-48)));
                                    reader.Rewind(1);
                                    break;
                                }

                                AddToBuilder(reader.Sequence.Slice(start,reader.Consumed-initial-4));
                                var cc = (64*((int)b2-48)+8*((int)b3-48)+((int)b4-48)) & 0xFF; // 256 max allowed
                                if (cc > 127)
                                {
                                    throw new NotImplementedException();
                                    // charBuff[0]=(byte)cc;
                                    // _decoder.GetChars(charBuff, charResults, true);
                                    // builder.Append(charResults[0]);
                                } else
                                {
                                    builder.Append((char)cc);
                                }

                                break;

                                void Ignore(ref SequenceReader<byte> rdr, int cnt)
                                {
                                    AddToBuilder(rdr.Sequence.Slice(start,rdr.Consumed-initial-cnt-1));
                                    rdr.Rewind(cnt);
                                }
                        }
                        break;
                    case (byte)'(':
                        stringDepth++;
                        if (stringDepth == 1)
                        {
                            break;
                        }
                        continue;
                    case (byte)')':
                        stringDepth--;
                        if (stringDepth == 0)
                        {
                            AddToBuilder(reader.Sequence.Slice(start, reader.Consumed-initial-1));
                            return true;
                        }
                        continue;
                }
                start = reader.Position;
                initial = reader.Consumed;
            }
            reader.Advance(reader.Remaining);
            AddToBuilder(reader.Sequence.Slice(start, reader.Position));
            return false;
        }
    }
}
