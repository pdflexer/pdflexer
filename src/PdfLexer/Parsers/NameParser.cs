using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;


namespace PdfLexer.Parsers
{
    public class NameParser : Parser<PdfName>
    {
        internal static byte[] NameTerminators = new byte[16] { 0x00, 0x09, 0x0A, 0x0C, 0x0D, 0x20,
            (byte)'(', (byte)')', (byte)'<', (byte)'>', (byte)'[', (byte)']', (byte)'{', (byte)'}', (byte)'/', (byte)'%' };


        public override PdfName Parse(ReadOnlySpan<byte> buffer)
        {
            if (buffer.IndexOf((byte) '#') == -1)
            {
                return ParseFastNoHex(buffer);
            }

            return ParseWithHex(buffer, 0, buffer.Length);
        }

        private PdfName ParseFastNoHex(ReadOnlySpan<byte> buffer)
        {
            // TODO lookup for commonly used names
            var chars = ArrayPool<char>.Shared.Rent(buffer.Length);
            var i = Encoding.ASCII.GetChars(buffer, chars);
            var str = new PdfName(new String(chars, 0, i));
            ArrayPool<char>.Shared.Return(chars);
            return str;
        }

        private PdfName ParseWithHex(ReadOnlySpan<byte> buffer, int start, int length)
        {
            var ci = 0;
            var rented = ArrayPool<char>.Shared.Rent(buffer.Length);
            Span<char> chars = rented;
            for (var i = start; i < start + length; i++)
            {
                var c = buffer[i];
                if (c == (byte) '#')
                {
                    if (!Utf8Parser.TryParse(buffer.Slice(i+1, 2), out byte v, out int ic, 'x'))
                    {
                        throw CommonUtil.DisplayDataErrorException(buffer, i, "Invalid hex found");
                    }
                    Debug.Assert(ic == 2);
                    i += 2;
                    chars[ci++] = (char)v;
                }
                else
                {
                    chars[ci++] = (char)c;
                }
            }
            var name = new PdfName(new String(chars.Slice(0,ci)));
            ArrayPool<char>.Shared.Return(rented);
            return name;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SkipName(ReadOnlySpan<byte> bytes, ref int i)
        {
            ReadOnlySpan<byte> local = bytes;
            ReadOnlySpan<byte> terms = NameTerminators;
            for (; i < local.Length; i++)
            {
                byte b = local[i];
                if (terms.IndexOf(b) == -1)
                {
                    continue;
                }

                return;
            }
        }
        
    }
}