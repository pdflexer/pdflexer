using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.Filters
{
    public class Ascii85Filter : IDecoder
    {
        private byte[] input = new byte[5];
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
            var TILDA_CHAR = 0x7e; // '~'
            var Z_LOWER_CHAR = 0x7a; // 'z'
            var EOF = -1;

            var ms = new MemoryStream();
            int i;
            while ((i = stream.ReadByte()) != -1)
            {
                var c = (byte)i;
                if (CommonUtil.IsWhiteSpace(c))
                {
                    continue;
                }

                if (c == TILDA_CHAR)
                {
                    ms.Position = 0;
                    return ms;
                }

                if (c == Z_LOWER_CHAR)
                {
                    for (i = 0; i < 4; ++i)
                    {
                        ms.WriteByte((byte)0);
                    }
                    continue;
                }

                input[0] = c;
                for (i = 1; i < 5; ++i)
                {
                    c = (byte)stream.ReadByte();
                    while (CommonUtil.IsWhiteSpace(c))
                    {
                        c = (byte)stream.ReadByte();
                    }

                    input[i] = c;

                    if (c == EOF || c == TILDA_CHAR)
                    {
                        break;
                    }
                }

                // partial ending;
                if (i < 5)
                {
                    for (; i < 5; ++i)
                    {
                        input[i] = 0x21 + 84;
                    }
                }

                var t = 0;
                for (i = 0; i < 5; ++i)
                {
                    t = t * 85 + (input[i] - 0x21);
                }

                for (i = 3; i >= 0; --i)
                {
                    ms.WriteByte((byte)(t & 0xff));
                    t >>= 8;
                }
            }

            ms.Position = 0;
            return ms;
        }
    }
}
