﻿using System.Buffers;

namespace PdfLexer.Parsers.Nested;

internal static class NestedUtil
{
    internal static byte[] dictScanTerms = new byte[4]
    {
        (byte) '<', (byte) '>', (byte) '(', (byte) 'R'
    };

    internal static byte[] arrayScanTerms = new byte[5]
    {
        (byte) '<', (byte) '(', (byte) '[', (byte) ']', (byte) 'R'
    };

    public static bool AdvanceToDictEnd(ReadOnlySpan<byte> buffer, ref int i, out bool hadIndirectObjects)
    {
        hadIndirectObjects = false;
        var slice = buffer.Slice(i);
        var pos = 0;
        while ((pos = slice.IndexOfAny(dictScanTerms)) != -1)
        {
            var b = slice[pos];
            switch (b)
            {
                case (byte)'R':
                    {
                        if (pos > 0 && CommonUtil.IsWhiteSpace(slice[pos-1]))
                        {
                            hadIndirectObjects = true;
                        }
                        i += pos + 1;
                        slice = buffer.Slice(i);
                        continue;
                    }
                case (byte)'<':
                    {
                        if (slice.Length < pos + 1)
                        {
                            return false;
                        }

                        if (slice[pos + 1] == (byte)'<')
                        {
                            // new dict
                            i += pos + 2;
                            if (!AdvanceToDictEnd(buffer, ref i, out bool hadSubIOs))
                            {
                                return false;
                            }

                            if (hadSubIOs)
                            {
                                hadIndirectObjects = true;
                            }
                            slice = buffer.Slice(i);
                            continue;
                        }

                        // just hex string
                        StringParser.AdvancePastHexString(slice, ref pos);
                        i += pos;
                        slice = buffer.Slice(i);
                        continue;
                    }
                case (byte)'>':
                    {
                        if (slice.Length < pos + 1)
                        {
                            return false;
                        }

                        if (slice[pos + 1] == (byte)'>')
                        {
                            // ended!
                            i += pos + 2;
                            return true;
                        }
                        // just hex end
                        i += pos + 1;
                        slice = buffer.Slice(i);
                        continue;
                    }
                case (byte)'(':
                    {
                        i += pos;
                        if (!StringParser.AdvancePastStringLiteral(buffer, ref i))
                        {
                            return false;
                        }

                        slice = buffer.Slice(i);
                        continue;
                    }
            }
        }

        return false;
    }

    public static bool AdvanceToArrayEnd(ReadOnlySpan<byte> buffer, ref int i, out bool hadIndirectObjects)
    {
        hadIndirectObjects = false;
        var slice = buffer.Slice(i);
        var pos = 0;
        while ((pos = slice.IndexOfAny(arrayScanTerms)) != -1)
        {
            var b = slice[pos];
            switch (b)
            {
                case (byte)'R':
                    {
                        if (pos > 0 && CommonUtil.IsWhiteSpace(slice[pos-1]))
                        {
                            hadIndirectObjects = true;
                        }
                        i += pos + 1;
                        slice = buffer.Slice(i);
                        continue;
                    }
                case (byte)'<':
                    {
                        if (slice.Length < pos + 1)
                        {
                            return false;
                        }

                        if (slice[pos + 1] == (byte)'<')
                        {
                            // new dict
                            i += pos + 2;
                            if (!AdvanceToDictEnd(buffer, ref i, out bool hadSubIOs))
                            {
                                return false;
                            }
                            if (hadSubIOs)
                            {
                                hadIndirectObjects = true;
                            }
                        }
                        else
                        {
                            // just hex string
                            i += pos + 1;
                        }


                        slice = buffer.Slice(i);
                        continue;
                    }
                case (byte)'(':
                    {
                        i += pos;
                        if (!StringParser.AdvancePastStringLiteral(buffer, ref i))
                        {
                            return false;
                        }

                        slice = buffer.Slice(i);
                        continue;
                    }
                case (byte)'[':
                    {
                        i += pos + 1;
                        if (!AdvanceToArrayEnd(buffer, ref i, out bool hadSubIOs))
                        {
                            return false;
                        }

                        if (hadSubIOs)
                        {
                            hadIndirectObjects = true;
                        }

                        slice = buffer.Slice(i);
                        continue;
                    }
                case (byte)']':
                    i += pos + 1;
                    return true;
            }
        }
        return false;
    }

    public static bool AdvanceToArrayEnd(this ref SequenceReader<byte> reader, out bool hadIndirectObjects)
    {
        hadIndirectObjects = false;
        while (reader.TryAdvanceToAny(arrayScanTerms, false))
        {
            if (!reader.TryRead(out byte b))
            {
                return false;
            }

            switch (b)
            {
                case (byte)'R':
                    {
                        if (reader.Consumed > 1)
                        {
                            reader.Rewind(2);
                            if (!reader.TryRead(out var pb))
                            {
                                reader.Advance(2);
                                continue;
                                
                            }
                            if (CommonUtil.IsWhiteSpace(pb))
                            {
                                hadIndirectObjects = true;
                            }
                            reader.Advance(1);
                        }
                        continue;
                    }
                case (byte)'<':
                    {
                        if (!reader.TryPeek(out byte b2))
                        {
                            return false;
                        }

                        if (b2 == (byte)'<')
                        {
                            // new dict
                            reader.TryRead(out byte _);
                            if (!reader.AdvanceToDictEnd(out bool hadSubIOs))
                            {
                                return false;
                            }
                            if (hadSubIOs)
                            {
                                hadIndirectObjects = true;
                            }
                        }

                        // just hex string
                    }
                    continue;
                case (byte)'(':
                    {
                        reader.Rewind(1);
                        if (!StringParser.AdvancePastStringLiteral(ref reader))
                        {
                            return false;
                        }

                        continue;
                    }
                case (byte)'[':
                    {
                        if (!reader.AdvanceToArrayEnd(out bool hadSubIOs))
                        {
                            return false;
                        }

                        if (hadSubIOs)
                        {
                            hadIndirectObjects = true;
                        }

                        continue;
                    }
                case (byte)']':
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Advances to the end of dictionary. Note: Current dictionary tokens must already have been read.
    /// TODO: switch to not have token be read
    /// </summary>
    /// <param name="reader"></param>
    /// <returns>If completed successfully. False designates incomplete data.</returns>
    public static bool AdvanceToDictEnd(this ref SequenceReader<byte> reader, out bool hadIndirectObjects)
    {
        hadIndirectObjects = false;
        while (reader.TryAdvanceToAny(dictScanTerms, false))
        {
            if (!reader.TryRead(out byte b))
            {
                return false;
            }

            switch (b)
            {
                case (byte)'R':
                    {
                        if (reader.Consumed > 1)
                        {
                            reader.Rewind(2);
                            if (!reader.TryRead(out var pb))
                            {
                                reader.Advance(2);
                                continue;
                                
                            }
                            if (CommonUtil.IsWhiteSpace(pb))
                            {
                                hadIndirectObjects = true;
                            }
                            reader.Advance(1);
                        }
                        continue;
                    }
                case (byte)'<':
                    {
                        if (!reader.TryPeek(out byte b2))
                        {
                            return false;
                        }

                        if (b2 == (byte)'<')
                        {
                            // new dict
                            reader.TryRead(out byte _);
                            if (!reader.AdvanceToDictEnd(out bool hadSubIOs))
                            {
                                return false;
                            }

                            if (hadSubIOs)
                            {
                                hadIndirectObjects = true;
                            }
                        }
                        else
                        {
                            // hex string
                            if (!reader.TryAdvanceTo((byte)'>', true))
                            {
                                return false;
                            }
                        }


                    }
                    continue;
                case (byte)'>':
                    {
                        if (!reader.TryPeek(out byte b2))
                        {
                            return false;
                        }

                        if (b2 == (byte)'>')
                        {
                            // ended!
                            return reader.TryRead(out _);
                        }
                        // just hex end
                    }
                    continue;
                case (byte)'(':
                    {
                        reader.Rewind(1);
                        if (!StringParser.AdvancePastStringLiteral(ref reader))
                        {
                            return false;
                        }

                        continue;
                    }
            }
        }

        return false;
    }
}
