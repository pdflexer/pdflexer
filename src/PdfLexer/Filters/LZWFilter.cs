using System;
using System.IO;

namespace PdfLexer.Filters
{
    public class LZWFilter : IDecoder
    {
        public static LZWFilter Instance { get; } = new LZWFilter();

        public LZWFilter()
        {
        }

        public Stream Decode(Stream stream, PdfDictionary filterParams)
        {
            var ec = filterParams?.Get<PdfNumber>("/EarlyChange");
            if (ec != null)
            {
                return FlateFilter.Instance.Decode(
                    new LZWStream(ec, stream),
                    filterParams
                    );
            }
            return new LZWStream(1, stream);
        }
    }

    // LZWStream decode stream MODIFIED / PORTED FROM PDF.JS, PDF.JS is licensed as follows:
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

    internal class LZWStream : MinBufferStream
    {
        int cachedData = 0;
        int bitsCached = 0;
        int earlyChange;
        int nextCode = 258;
        int[] dictionaryValues;
        int[] dictionaryLengths;
        int[] dictionaryPrevCodes;
        int[] currentSequence;
        int currentSequenceLength;
        int codeLength;
        int prevCode;

        public LZWStream(int earlyChange, Stream inner) : base(inner, 4096)
        {
            this.earlyChange = earlyChange;
            codeLength = 9;
            nextCode = 258;
            dictionaryValues = new int[4096];
            dictionaryLengths = new int[4096];
            dictionaryPrevCodes = new int[4096];
            currentSequence = new int[4096];
            currentSequenceLength = 0;
            for (var i=0;i<256;i++)
            {
                dictionaryValues[i] = i;
                dictionaryLengths[i] = 1;
            }
        }

        protected override int FillBuffer(byte[] outgoing, int offset, int count)
        {
            var blockSize = 512;
            int i, j, q;

            if (dictionaryValues == null)
            {
                return 0; // eof was found
            }

            var decodedLength = 0;
            for (i = 0; i < blockSize; i++)
            {
                var cr = this.readBits(codeLength);
                var hasPrev = currentSequenceLength > 0;

                if (cr == null || cr.Value == 257)
                {
                    dictionaryValues = null;
                    return decodedLength;
                }
                var code = cr.Value;
                if (code < 256)
                {
                    currentSequence[0] = code;
                    currentSequenceLength = 1;
                }
                else if (code >= 258)
                {
                    if (code < nextCode)
                    {
                        currentSequenceLength = dictionaryLengths[code];
                        for (j = currentSequenceLength - 1, q = code; j >= 0; j--)
                        {
                            currentSequence[j] = dictionaryValues[q];
                            q = dictionaryPrevCodes[q];
                        }
                    }
                    else
                    {
                        currentSequence[currentSequenceLength++] = currentSequence[0];
                    }
                }
                else if (code == 256)
                {
                    codeLength = 9;
                    nextCode = 258;
                    currentSequenceLength = 0;
                    continue;
                }

                if (hasPrev)
                {
                    dictionaryPrevCodes[nextCode] = prevCode;
                    dictionaryLengths[nextCode] = dictionaryLengths[prevCode] + 1;
                    dictionaryValues[nextCode] = currentSequence[0];
                    nextCode++;
                    codeLength =
                      ((nextCode + earlyChange) & (nextCode + earlyChange - 1)) != 0
                        ? codeLength
                        : (int)Math.Floor(Math.Min(
                            Math.Log(nextCode + earlyChange) / 0.6931471805599453 + 1,
                            12
                          )); // | 0;
                }
                prevCode = code;

                decodedLength += currentSequenceLength;
                if (count < decodedLength)
                {
                    throw new PdfLexerException("LZW string produced more data than 4096");
                }
                for (j = 0; j < currentSequenceLength; j++)
                {
                    outgoing[offset++] = (byte)currentSequence[j];
                }
            }
            return decodedLength;
        }

        int? readBits(int n)
        {
            while (bitsCached < n)
            {
                var c = inner.ReadByte();
                if (c == -1)
                {
                    return null;
                }
                cachedData = ((cachedData << 8) | c);
                bitsCached += 8;
            }
            bitsCached -= n;
            return (int)((uint)cachedData >> bitsCached) & ((1 << n) - 1);
        }
    }
}
