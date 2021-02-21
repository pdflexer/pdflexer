using System;
using System.Buffers;
using System.Buffers.Text;
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
    public class StringParser : Parser<PdfString>, IStreamedParser<PdfString>
    {
        private static Encoding Iso88591 = Encoding.GetEncoding("ISO-8859-1"); // StandardEncoding
        private static Encoding Win1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252); // WinAnsiEncoding
        private static Encoding MacRoman = CodePagesEncodingProvider.Instance.GetEncoding("macintosh"); // WinAnsiEncoding
        // PdfEncoding : 162-255 same, 20-126 same, 127 undefined, 0-21 same
        // MaxExpert??

        private StringStatus Status = StringStatus.None;
        private readonly ParsingContext _ctx;
        private readonly Decoder _decoder;
        private int stringDepth = 0;
        private static byte[] stringLiteralTerms = new byte[]
        {
            (byte) '(', (byte) ')', (byte) '\\'
        };

        public StringParser(ParsingContext ctx)
        {
            _ctx = ctx;
            _decoder = Iso88591.GetDecoder();
        }

        public override PdfString Parse(ReadOnlySpan<byte> buffer)
        {
            var b = buffer[0];
            if (b == '(')
            {
                return ParseStringLiteral(buffer, out _);
            } else if (b == (byte)'<')
            {
                return ParseStringHex(buffer, out _);
            }
            throw new ApplicationException("Invalid string, first char not ( or <.");
        }

        private byte[] hexBuffer = new byte[2];
        private PdfString ParseStringHex(ReadOnlySpan<byte> buffer, out int length)
        {

            length = 0;
            bool completed = true;
            var data = ArrayPool<byte>.Shared.Rent((buffer.Length-2)/2);
            var di = 0;
            bool isLow = true;
            for (var i = 1; i < buffer.Length; i++)
            {
                var b = buffer[i];
                if (b == 0x00
                   || b == 0x09
                   || b == 0x0A
                   || b == 0x0C
                   || b == 0x0D
                   || b == 0x20)
                {
                    continue;
                }

                if (b == (byte)'>')
                {
                    length = i;
                    completed = true;
                    break;
                }

                if (isLow)
                {
                    hexBuffer[0] = b;
                } else
                {
                    hexBuffer[1] = b;
                    
                    if (!Utf8Parser.TryParse(hexBuffer, out byte value, out int consumed, 'x'))
                    {
                        throw CommonUtil.DisplayDataErrorException(buffer, i, "Bad hex string data");
                    }
                    Debug.Assert(consumed == 2);
                    data[di++] = value;
                }
                isLow = !isLow;
            }

            if (!completed)
            {
                throw CommonUtil.DisplayDataErrorException(buffer, buffer.Length-1, "Unexpected hex string end");
            }

            if (!isLow)
            {
                hexBuffer[1] = (byte)'0';
                if (!Utf8Parser.TryParse(hexBuffer, out byte v, out int c, 'x'))
                {
                    throw CommonUtil.DisplayDataErrorException(buffer, buffer.Length-1, "Bad hex string data");
                }
                Debug.Assert(c == 2);
                data[di++] = v;
            }

            var span = new Span<byte>(data).Slice(0, di);
            return new PdfString(Encoding.UTF8.GetString(span)); // TODO bytes?
        }

        public override PdfString Parse(in ReadOnlySequence<byte> sequence)
        {
            var reader = new SequenceReader<byte>(sequence);
            if (!TryReadString(ref reader))
            {
                throw CommonUtil.DisplayDataErrorException(ref reader, "End of string not found");
            }
            return GetCurrentString();
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

            throw new ApplicationException("Invalid string, first char not ( or <.");
        }

        private PdfString value;
        internal StringBuilder builder = new StringBuilder();
        internal PdfString GetCurrentString()
        {
            if (value == null)
            {
                var str = builder.ToString();
                builder.Clear();
                Status = StringStatus.None;
                return new PdfString(str);
            } else
            {
                var val = value;
                value = null;
                return val;
            }

        }
        private char[] charResults = new char[1];
        private byte[] charBuff = new byte[1];
        private PdfString ParseStringLiteral(ReadOnlySpan<byte> buffer, out int pos)
        {
            builder.Clear();
            var copyStart = 0;
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
                                    if (buffer.Length < pos+3)
                                    {
                                        AddToBuilder(buffer.Slice(copyStart, pos));
                                        break;
                                    }
                                    {
                                        b2 = buffer[pos+1];
                                        var b3 = buffer[pos+2];
                                        if (b2 < 48 || b2 > 55 || b3 < 48 || b3 > 55)
                                        {
                                            AddToBuilder(buffer.Slice(copyStart, pos));
                                            break;
                                        }

                                        if (buffer.Length > pos + 3)
                                        {
                                            byte b4 = buffer[pos+3];
                                            if (b4 < 48 || b4 > 55)
                                            {
                                                AddToBuilder(buffer.Slice(copyStart, pos));
                                                builder.Append((char)(8*((int)b2-48)+((int)b3-48)));
                                                pos +=2;
                                                break;
                                            }

                                            AddToBuilder(buffer.Slice(copyStart, pos));
                                            var cc = (64*((int)b2-48)+8*((int)b3-48)+((int)b4-48)) & 0xFF; // 256 max allowed
                                            if (cc > 127)
                                            {
                                                charBuff[0]=(byte)cc;
                                                _decoder.GetChars(charBuff, charResults, true);
                                                builder.Append(charResults[0]);
                                            } else
                                            {
                                                builder.Append((char)cc);
                                            }
                                            pos +=3;
                                        } else
                                        {
                                            AddToBuilder(buffer.Slice(copyStart, pos));
                                            builder.Append((char)(8*((int)b2-48)+((int)b3-48)));
                                            pos +=2;
                                        }
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
                            pos += 2;
                            return GetCurrentString();
                        }
                        AddToBuilder(buffer.Slice(copyStart, pos+1));
                        buffer = buffer.Slice(pos+1);
                        copyStart = 0;
                        continue;
                }
            }
            throw CommonUtil.DisplayDataErrorException(buffer,pos,"String ended incorrectly");
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
                                    charBuff[0]=(byte)cc;
                                    _decoder.GetChars(charBuff, charResults, true);
                                    builder.Append(charResults[0]);
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
        private void AddToBuilder(in ReadOnlySequence<byte> data)
        {
            // TODO optimize.. for now this is easy
            foreach (var seg in data)
            {
                builder.Append(Iso88591.GetString(seg.Span));
            }
        }

        private void AddToBuilder(ReadOnlySpan<byte> data)
        {
            // TODO optimize.. for now this is easy
            builder.Append(Iso88591.GetString(data));
        }

        private bool TryReadStringHex(ref SequenceReader<byte> reader)
        {
            var pos = reader.Position;
            if (!reader.TryAdvanceTo((byte)'>'))
            {
                return false;
            }

            var sequence = reader.Sequence.Slice(pos, reader.Position);
            if (sequence.IsSingleSegment)
            {
                value = Parse(sequence.FirstSpan);
                return true;
            }
            // TODO optimize
            var len = (int)sequence.Length;
            var buffer = ArrayPool<byte>.Shared.Rent(len);
            sequence.CopyTo(buffer);
            Span<byte> buff = buffer;
            value = Parse(buff.Slice(0,len));
            ArrayPool<byte>.Shared.Return(buffer);
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

        public PdfString Parse(ReadOnlySpan<byte> data, int start, out int length)
        {
            // TODO optimize
            var b = data[start];
            if (b == '(')
            {
                return ParseStringLiteral(data.Slice(start), out length);
            } else if (b == (byte)'<')
            {
                return ParseStringHex(data.Slice(start), out length);
            }
            throw new ApplicationException("Invalid string, first char not ( or <.");
        }
    }
}