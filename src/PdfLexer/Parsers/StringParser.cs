using PdfLexer.IO;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PdfLexer.Parsers
{
    internal enum StringStatus
    {
        None,
        ParsingLiteral,
        ParsingHex
    }
    public class StringParser : IParser<PdfString>, IStreamedParser<PdfString>
    {
        private StringStatus Status = StringStatus.None;
        private readonly ParsingContext _ctx;
        private int stringDepth = 0;
        private static byte[] stringLiteralTerms = new byte[]
        {
            (byte) '(', (byte) ')', (byte) '\\'
        };

        public StringParser(ParsingContext ctx)
        {
            _ctx = ctx;
        }

        public PdfString Parse(ReadOnlySpan<byte> buffer)
        {
            builder.Clear();
            var copyStart = 0;
            int pos = -1;
            while ((pos = buffer.IndexOfAny(stringLiteralTerms)) > -1)
            {
                var b = buffer[pos];
                switch (b)
                {
                    case (byte)'\\':
                        if (buffer.Length > pos+1)
                        {
                            var b2 = buffer[pos+1];
                            switch (b2)
                            {
                                case (byte)'n':
                                    AddToBuilder(buffer.Slice(copyStart, pos));
                                    builder.Append('\n');
                                    pos+=1;
                                    break;
                                case (byte)'r':
                                    AddToBuilder(buffer.Slice(copyStart, pos));
                                    builder.Append('\r');
                                    pos+=1;
                                    break;
                                case (byte)'t':
                                    AddToBuilder(buffer.Slice(copyStart, pos));
                                    builder.Append('\t');
                                    pos+=1;
                                    break;
                                case (byte)'b':
                                    AddToBuilder(buffer.Slice(copyStart, pos));
                                    builder.Append('\b');
                                    pos+=1;
                                    break;
                                case (byte)'f':
                                    AddToBuilder(buffer.Slice(copyStart, pos));
                                    builder.Append('\f');
                                    pos+=1;
                                    break;
                                case (byte)'(':
                                    AddToBuilder(buffer.Slice(copyStart, pos));
                                    builder.Append('(');
                                    pos+=1;
                                    break;
                                case (byte)')':
                                    AddToBuilder(buffer.Slice(copyStart, pos));
                                    builder.Append(')');
                                    pos+=1;
                                    break;
                                case (byte)'\\':
                                    AddToBuilder(buffer.Slice(copyStart, pos));
                                    builder.Append('\\');
                                    pos+=1;
                                    break;
                                case (byte)'\r':
                                    if (buffer.Length > pos+2)
                                    {
                                        var b3 = buffer[pos+2];
                                        if (b3 == (byte)'\n')
                                        {
                                            AddToBuilder(buffer.Slice(copyStart, pos));
                                            pos+=2;
                                            break;
                                        }
                                        AddToBuilder(buffer.Slice(copyStart, pos));
                                        pos+=1;
                                        break;
                                    }
                                    throw CommonUtil.DisplayDataErrorException(buffer,pos,"String ended incorrectly");
                                case (byte)'\n':
                                    AddToBuilder(buffer.Slice(copyStart, pos));
                                    pos+=1;
                                    break;
                                default:
                                    if (buffer.Length < pos+4)
                                    {
                                        AddToBuilder(buffer.Slice(copyStart, pos));
                                        break;
                                    }
                                    {
                                        b2 = buffer[pos+1];
                                        var b3 = buffer[pos+2];
                                        var b4 = buffer[pos+3];

                                        if (b2 < 48 || b2 > 55 || b3 < 48 || b3 > 55 || b4 < 48 || b4 > 55)
                                        {
                                            AddToBuilder(buffer.Slice(copyStart, pos));
                                            break;
                                        }


                                        AddToBuilder(buffer.Slice(copyStart, pos));
                                        builder.Append((char)(64*((int)b2-48)+8*((int)b3-48)+((int)b4-48)));
                                        pos += 3;
                                        break;
                                    }
                            }
                        } else
                        {
                            throw CommonUtil.DisplayDataErrorException(buffer,pos,"String ended incorrectly");
                        }
                        buffer = buffer.Slice(pos+1);
                        copyStart = 0;
                        break;
                    case (byte)'(':
                        stringDepth++;
                        if (stringDepth > 1)
                        {
                            AddToBuilder(buffer.Slice(copyStart, pos+1));
                            copyStart = 0;
                        }
                        buffer = buffer.Slice(pos+1);
                        continue;
                    case (byte)')':
                        stringDepth--;
                        if (stringDepth == 0)
                        {
                            AddToBuilder(buffer.Slice(copyStart, pos));
                            return new PdfString(GetCurrentString());
                        }
                        AddToBuilder(buffer.Slice(copyStart, pos+1));
                        buffer = buffer.Slice(pos+1);
                        copyStart = 0;
                        continue;
                }
            }
            throw CommonUtil.DisplayDataErrorException(buffer,pos,"String ended incorrectly");
        }

        public PdfString Parse(ReadOnlySpan<byte> buffer, int start, int length)
        {
            return Parse(buffer.Slice(start, length));
        }

        public PdfString Parse(in ReadOnlySequence<byte> sequence)
        {
            var reader = new SequenceReader<byte>(sequence);
            if (!TryReadString(ref reader))
            {
                throw CommonUtil.DisplayDataErrorException(ref reader, "End of string not found");
            }
            return new PdfString(GetCurrentString());
        }

        public PdfString Parse(in ReadOnlySequence<byte> sequence, long start, int length)
        {
            var slice = sequence.Slice(start, length);
            return Parse(in slice);
        }

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
                result = new PdfString(GetCurrentString());
            } else
            {
                result = null;
            }
            pos = reader.Position;
            return success;
        }
        
        internal bool TryReadString(ref SequenceReader<byte> reader)
        {
            switch (Status)
            {
                case StringStatus.None:
                    builder.Clear();
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

            throw new ApplicationException("Invalid string, first char not ( or <.");
        }

        internal StringBuilder builder = new StringBuilder();
        internal string GetCurrentString()
        {
            var value = builder.ToString();
            builder.Clear();
            Status = StringStatus.None;
            return value;
        }

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
                                    reader.Rewind(3);
                                    return false;
                                }

                                if (b4 < 48 || b4 > 55)
                                {
                                    Ignore(ref reader, 3);
                                    break;
                                }

                                AddToBuilder(reader.Sequence.Slice(start,reader.Consumed-initial-4));
                                builder.Append((char)(64*((int)b2-48)+8*((int)b3-48)+((int)b4-48)));
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
        private void AddToBuilder(in ReadOnlySequence<byte> data)
        {
            // TODO optimize.. for now this is easy
            foreach (var seg in data)
            {
                builder.Append(Encoding.ASCII.GetString(seg.Span));
            }
        }

        private void AddToBuilder(ReadOnlySpan<byte> data)
        {
            // TODO optimize.. for now this is easy
            builder.Append(Encoding.ASCII.GetString(data));
        }

        private bool TryReadStringHex(ref SequenceReader<byte> reader)
        {
            // Utf8Parser.TryParse(data, out byte value, out int consumed, 'x');
            // TODO
            return true;
        }

        public static bool TryAdvancePastString(ref SequenceReader<byte> reader)
        {
            if (!reader.TryPeek(out byte b))
            {
                return false;
            }

            if (b == (byte)'(')
            {
                return AdvancePastStringLiteral(ref reader);
            } else if (b == (byte)'<')
            {
                return reader.TryAdvanceTo((byte) '>', true);
            }

            throw new ApplicationException("Invalid string, first char not ( or <.");
        }

        public static bool AdvancePastString(ReadOnlySpan<byte> data, ref int i)
        {
            var b = data[i];

            if (b == (byte)'(')
            {
                return  AdvancePastStringLiteral(data, ref i);
            } else if (b == (byte)'<')
            {
                var end = data.IndexOf((byte) '>');
                if (end == -1)
                {
                    return false;
                }

                i = end+1;
                return true;
            }

            throw new ApplicationException("Invalid string, first char not ( or <.");
        }

        internal static bool AdvancePastStringLiteral(ReadOnlySpan<byte> data, ref int i)
        {
            // TODO data.IndexOfAny
            ReadOnlySpan<byte> local = data;
            var depth = 0;
            for (; i < local.Length; i++)
            {
                byte b = local[i];
                if (b == '\\')
                {
                    if (local.Length !> i + 1)
                    {
                        return false;
                    }

                    continue;
                } else if (b == '(')
                {
                    depth++;
                }
                else if (b == ')')
                {
                    depth--;
                }

                if (depth == 0)
                {
                    i += 1;
                    return true;
                }
            }
            return false;
        }

        internal static bool AdvancePastStringLiteral(ref SequenceReader<byte> reader)
        {
            // TODO allow rentrancy?
            var orig = reader.Consumed;
            var stringDepth = 0;
            while (reader.TryAdvanceToAny(stringLiteralTerms, false))
            {
                if (!reader.TryRead(out byte b))
                {
                    return false;
                }
                switch (b)
                {
                    case (byte)'\\':
                        if (!reader.TryRead(out _))
                        {
                            reader.Rewind(1);
                            return false;
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
                            return true;
                        }
                        continue;
                }
            }
            reader.Rewind(reader.Consumed-orig);
            return false;
        }
    }
}