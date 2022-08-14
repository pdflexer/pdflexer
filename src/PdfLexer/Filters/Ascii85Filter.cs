namespace PdfLexer.Filters;

public class Ascii85Filter : IDecoder
{
    public static Ascii85Filter Instance { get; } = new Ascii85Filter();

    public Stream Decode(Stream stream, PdfDictionary? filterParams) => new Ascii85Stream(stream);
}

// Decode FUNCTION PORTED FROM PDF.JS, PDF.JS is licensed as follows:
/* Copyright 2012 Mozilla Foundation
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
internal class Ascii85Stream : MinBufferStream
{
    private const int TILDA_CHAR = 0x7e;  // '~'
    private const int Z_LOWER_CHAR = 0x7a; // 'z'
    private const int EOF = -1;

    private byte[] input = new byte[5];
    private byte[] rev = new byte[4];

    public Ascii85Stream(Stream inner) : base(inner, 4) { }

    protected override int FillBuffer(byte[] buffer, int offset, int count)
    {
        int i;
        int read = 0;
        bool eof = false;
        while (read < count - 3 && !eof && (i = inner.ReadByte()) != -1)
        {
            var c = (byte)i;
            if (CommonUtil.IsWhiteSpace(c))
            {
                continue;
            }

            if (c == TILDA_CHAR)
            {
                return read;
            }

            if (c == Z_LOWER_CHAR)
            {

                for (i = 0; i < 4; ++i)
                {
                    buffer[offset + read] = 0;
                    read++;
                }
                continue;
            }

            input[0] = c;
            for (i = 1; i < 5; ++i)
            {
                var r = inner.ReadByte();
                while (CommonUtil.IsWhiteSpace(c))
                {
                    r = inner.ReadByte();
                }

                input[i] = (byte)r;

                if (r == EOF || r == TILDA_CHAR)
                {
                    eof = true;
                    break;
                }
            }

            // partial ending;
            int added = 0;
            if (i < 5)
            {
                for (; i < 5; ++i)
                {
                    input[i] = 0x21 + 84;
                    added++;
                }
            }

            var t = 0;
            for (i = 0; i < 5; ++i)
            {
                t = t * 85 + (input[i] - 0x21);
            }

            for (i = 3; i >= 0; --i)
            {
                rev[i] = (byte)(t & 0xff);
                t >>= 8;
            }
            for (i = 0; i < 4 - added; i++)
            {
                buffer[offset + read] = rev[i];
                read++;
            }
        }
        return read;
    }
}
