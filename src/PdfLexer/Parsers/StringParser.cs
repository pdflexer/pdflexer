using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Parsers
{
    internal enum StringStatus
    {
        None,
        ParsingLiteral,
        ParsingHex
    }
    public class StringParser
    {
        private StringStatus Status = StringStatus.None;
        public StringParser(ParsingContext ctx)
        {
            _ctx = ctx;
        }

        
        public bool TryReadString(ref SequenceReader<byte> reader)
        {
            switch (Status)
            {
                case StringStatus.None:
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

        private int stringDepth = 0;
        private static byte[] stringLiteralTerms = new byte[]
        {
            (byte) '(', (byte) ')', (byte) '\\'
        };
        private StringBuilder builder = new StringBuilder();
        private bool TryReadStringLiteral(ref SequenceReader<byte> reader)
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
                            default:
                                // TODO check for octals
                                break;
                        }
                        break;
                    case (byte)'(':
                        
                        stringDepth++;
                        if (stringDepth != 1)
                        {
                            AddToBuilder(reader.Sequence.Slice(start, reader.Position));
                        }
                        break;
                    case (byte)')':
                        AddToBuilder(reader.Sequence.Slice(start,reader.Consumed-initial-2));
                        stringDepth--;
                        if (stringDepth == 0)
                        {
                            AddToBuilder(reader.Sequence.Slice(start, reader.Consumed-initial-1));
                            return true;
                        } else
                        {
                            AddToBuilder(reader.Sequence.Slice(start, reader.Position));
                        }
                        break;
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
        private bool TryReadStringHex(ref SequenceReader<byte> reader)
        {
            return true;
        }

        // TODO string literal octal (\0053) = \005, (\053) = \053, (\53) = \053
        // TODO ignore escape \ if not followed by allowable chars
        // TODO escape before end of line for line splitting
        // TODO end of line conversion from what's in string to just \n
        public static bool AdvancePastString(ref SequenceReader<byte> reader)
        {
            if (!reader.TryPeek(out byte b))
            {
                return false;
            }

            if (b == (byte)'(')
            {
                return  AdvancePastStringLiteral(ref reader);
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

        public static bool GetString(ReadOnlySpan<byte> data, out ReadOnlySpan<byte> results)
        {
            if (data[0] == (byte)'(')
            {
                return GetStringLiteral(data, out results);
            }
            else if (data[0] == (byte)'<')
            {
                return GetStringHex(data, out results);
            }
            else
            {
                throw new ApplicationException("Invalid string, first char not ( or <.");
            }
        }

        internal static bool AdvancePastStringLiteral(ReadOnlySpan<byte> data, ref int i)
        {
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

        internal static bool AdvancePastStringLiteral(ref SequenceReader<byte> data)
        {
            var orig = data.Consumed;
            var depth = 0;
            while (data.TryRead(out var b))
            {
                if (b == '\\')
                {
                    if (!data.TryRead(out b))
                    {
                        data.Rewind(data.Consumed-orig);
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
                    return true;
                }
            }
            data.Rewind(data.Consumed-orig);
            return false;
        }

        private static bool GetStringLiteral(ReadOnlySpan<byte> data, out ReadOnlySpan<byte> results)
        {

            var result = Convert.ToInt16("", 8);
            int depth = 1;
            int end = -1;
            for (var i = 1; i < data.Length; i++)
            {
                var b = data[i];
                if (b == '\\')
                {
                    i++; // skip next
                } else 
                if (b == '(')
                {
                    depth++;
                }
                else if (data[i] == ')')
                {
                    depth--;
                }
                if (depth == 0)
                {
                    end = i + 1;
                    break;
                }
            }
            if (end == -1)
            {
                if (depth != 0)
                {
                    results = null;
                    return false;
                }
                end = data.Length;
            }
            results = data.Slice(0, end);
            return true;
        }
        private static bool GetStringHex(ReadOnlySpan<byte> data, out ReadOnlySpan<byte> results)
        {
            Utf8Parser.TryParse(data, out byte value, out int consumed, 'x');
            var loc = data.IndexOf((byte)'>');
            if (loc == -1)
            {
                results = null;
                return false;
            }
            results = data.Slice(0, loc + 1);
            return true;
        }

        public static int ParseString(ReadOnlySpan<byte> data, out Span<char> results)
        {
            if (data[0] == (byte)'(')
            {
                return ParseStringLiteral(data, out results);
            }
            else if (data[0] == (byte)'<')
            {
                // hex bytes
                throw new NotImplementedException("Hex string todo");
            }
            else
            {
                throw new ApplicationException("Invalid string, first char not ( or <.");
            }
        }

        private static int ParseStringLiteral(ReadOnlySpan<byte> data, out Span<char> results)
        {
            int depth = 1;
            int end = -1;
            List<int> escapes = null;
            for (var i = 1; i < data.Length; i++)
            {
                if (data[i] == '(' && data[i - 1] != '\\')
                {
                    depth++;
                }
                else if (data[i] == ')' && data[i - 1] != '\\')
                {
                    depth--;
                }
                else if (data[i] == '\\' && data[i - 1] != '\\')
                {
                    if (escapes == null)
                    {
                        escapes = new List<int>();
                    }
                    escapes.Add(i);
                }
                if (depth == 0)
                {
                    end = i;
                    break;
                }
            }
            if (end == -1)
            {
                if (depth != 0)
                {
                    throw new ApplicationException("Unbalanced string literal.");
                }
                end = data.Length - 1;
            }
            results = new Span<char>(new char[end - 1]);
            Encoding.ASCII.GetChars(data.Slice(1, end - 1), results);
            // ReplaceEspacedChars(results, out results);
            return end + 1;
        }
        private static char[] numeric = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private readonly ParsingContext _ctx;
        // private static void ReplaceEspacedChars(Span<char> chars, out Span<char> results)
        // {
        //     var espace = chars.IndexOf('\\');
        //     if (espace == -1)
        //     {
        //         results = chars;
        //     } else
        //     {
        //         int pos = 0;
        //         var search = chars;
        //         switch (search[pos+1])
        //         {
        //             case 'n':
        //                 search[pos] = '\n';
        //                 break;
        //             case 'r':
        //                 search[pos] = '\r';
        //                 break;
        //             case 't':
        //                 search[pos] = '\t';
        //                 break;
        //             case 'b':
        //                 search[pos] = '\b';
        //                 break;
        //             case 'f':
        //                 search[pos] = '\f';
        //                 break;
        //             case '(':
        //                 search[pos] = '(';
        //                 break;
        //             case ')':
        //                 search[pos] = ')';
        //                 break;
        //             case '\\':
        //                 search[pos] = '\\';
        //                 break;
        //             default:
        // 
        //         }
        //         search = chars.Slice(pos)
        //         results = null;
        //     }
        // }
    }
}