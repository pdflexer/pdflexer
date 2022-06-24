using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.Filters
{
    public class AsciiHexFilter : IDecoder
    {
        private readonly ParsingContext _ctx;

        public AsciiHexFilter(ParsingContext ctx)
        {
            _ctx = ctx;
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
        public Stream Decode(Stream stream, PdfDictionary filterParams)
        {
            var eof = false;
            var firstDigit = -1;
            int ch;

            // TODO create real stream implementation
            var output = _ctx.GetTemporaryStream();
            while ((ch = stream.ReadByte()) != -1)
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
                    output.WriteByte((byte) ((firstDigit << 4) | digit));
                    firstDigit = -1;
                }
            }

            if (firstDigit >= 0 && eof)
            {
                // incomplete byte
                output.WriteByte((byte) (firstDigit << 4));
            }
            output.Position = 0;
            return output;
        }
    }
}
