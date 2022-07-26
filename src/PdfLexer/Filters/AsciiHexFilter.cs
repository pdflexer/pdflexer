using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.Filters
{
    public class AsciiHexFilter : IDecoder
    {
        public static AsciiHexFilter Instance { get; } = new AsciiHexFilter();

        public Stream Decode(Stream stream, PdfDictionary filterParams)
        {
            return new AsciiHexStream(stream);
        }
    }

    // Decode stream MODIFIED / PORTED FROM PDF.JS, PDF.JS is licensed as follows:
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
    internal class AsciiHexStream : DecodeStream
    {
        public AsciiHexStream(Stream inner) : base(inner) { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var eof = false;
            var firstDigit = -1;
            int ch;

            int read = 0;

            while ((ch = inner.ReadByte()) != -1 && read < count)
            {
                int digit;
                if (ch >= /* '0' = */ 0x30 && ch <= /* '9' = */ 0x39)
                {
                    digit = ch & 0x0f;
                }
                else if (
                (ch >= /* 'A' = */ 0x41 && ch <= /* 'Z' = */ 0x46) ||
                (ch >= /* 'a' = */ 0x61 && ch <= /* 'z' = */ 0x66)
              )
                {
                    digit = (ch & 0x0f) + 9;
                }
                else if (ch == /* '>' = */ 0x3e)
                {
                    eof = true;
                    break;
                }
                else
                {
                    // Probably whitespace, ignoring.
                    continue;
                }
                if (firstDigit < 0)
                {
                    firstDigit = digit;
                }
                else
                {
                    buffer[offset + read] = (byte)((firstDigit << 4) | digit);
                    read++;
                    firstDigit = -1;
                }
            }

            if (firstDigit >= 0 && eof)
            {
                // incomplete byte
                buffer[offset + read] = (byte)(firstDigit << 4);
                read++;
            }

            return read;
        }
    }
}
