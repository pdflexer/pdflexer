using PdfLexer.Parsers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace PdfLexer.Filters
{
    public class FlateFilter : IDecoder
    {
        private PdfNumber DefaultPredictor = PdfCommonNumbers.One;
        private PdfNumber DefaultColors = PdfCommonNumbers.One;
        private PdfNumber DefaultColumns = PdfCommonNumbers.One;
        private PdfNumber DefaultEarlyChange = PdfCommonNumbers.One;
        private PdfNumber DefaultBPC = PdfCommonNumbers.Eight;
        private readonly ParsingContext _ctx;

        public FlateFilter(ParsingContext ctx)
        {
            _ctx = ctx;
        }

        public Stream Encode(Stream source)
        {
            return source;
        }

        public Stream Decode(Stream source, PdfDictionary decodeParms)
        {
            // remove header
            source.ReadByte();
            source.ReadByte();
            var deflated = new DeflateStream(source, CompressionMode.Decompress, true);
            if (decodeParms == null) { return deflated; }

            int predictor = decodeParms.GetOptionalValue<PdfNumber>(PdfName.Predictor) ?? DefaultPredictor;
            if (predictor == 1)
            {
                return deflated;
            }

            int bpc = decodeParms.GetOptionalValue<PdfNumber>(PdfName.BitsPerComponent) ?? DefaultBPC;
            int colors = decodeParms.GetOptionalValue<PdfNumber>(PdfName.Colors) ?? DefaultColors;
            int columns = decodeParms.GetOptionalValue<PdfNumber>(PdfName.Columns) ?? DefaultColumns;

            if (predictor == 2)
            {
                return TiffDecode(deflated, bpc, colors, columns);
            }
            else
            {
                return PngDecode(deflated, bpc, colors, columns);
            }
        }

        internal Stream TiffDecode(Stream stream, int bpc, int colors, int columns)
        {
            throw new NotImplementedException("Flate TiffDecode has not been implemented");
        }

        // PngDecode FUNCTION PORTED FROM PDF.JS, PDF.JS is licensed as follows:
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
        internal Stream PngDecode(Stream stream, int bpc, int colors, int columns)
        {
            var ms = ParsingContext.StreamManager.GetStream();
            var pixBytes = (colors * bpc + 7) >> 3;
            var rowBytes = (columns * colors * bpc + 7) >> 3;

            var prevRow = new byte[rowBytes];
            var buffer = new byte[rowBytes];
            var rawBytes = new byte[rowBytes];

            var predictor = (int)stream.ReadByte();

            int i;
            var j = 0;
            byte up;
            byte c;
            while (stream.Read(rawBytes) == rowBytes)
            {
                switch (predictor)
                {
                    case 0:
                        ms.Write(rawBytes);
                        break;
                    case 1:
                        j = 0;
                        i = 0;
                        for (; i < pixBytes; i++)
                        {
                            buffer[j++] = rawBytes[i];
                        }
                        for (; i < rowBytes; i++)
                        {
                            buffer[j] = (byte)((buffer[j - pixBytes] + rawBytes[i]) & 0xff);
                            j++;
                        }
                        ms.Write(buffer, 0, j);
                        break;
                    case 2:
                        j = 0;
                        for (i = 0; i < rowBytes; ++i)
                        {
                            var b = (prevRow[i] + rawBytes[i]);
                            var bs = b & 0xFF;
                            var bf = (byte)bs;
                            buffer[j++] = (byte)((prevRow[i] + rawBytes[i]) & 0xff);
                        }
                        ms.Write(buffer, 0, j);
                        break;
                    case 3:
                        j = 0;
                        for (i = 0; i < pixBytes; ++i)
                        {
                            buffer[j++] = (byte)((prevRow[i] >> 1) + rawBytes[i]);
                        }
                        for (; i < rowBytes; ++i)
                        {
                            buffer[j] = (byte)
                              ((((prevRow[i] + buffer[j - pixBytes]) >> 1) + rawBytes[i]) & 0xff);
                            j++;
                        }
                        ms.Write(buffer, 0, j);
                        break;
                    case 4:
                        j = 0;
                        for (i = 0; i < pixBytes; ++i)
                        {
                            up = prevRow[i];
                            c = rawBytes[i];
                            buffer[j++] = (byte)(up + c);
                        }
                        for (; i < rowBytes; ++i)
                        {
                            up = prevRow[i];
                            var upLeft = prevRow[i - pixBytes];
                            var left = buffer[j - pixBytes];
                            var p = left + up - upLeft;

                            var pa = p - left;
                            if (pa < 0)
                            {
                                pa = -pa;
                            }
                            var pb = p - up;
                            if (pb < 0)
                            {
                                pb = -pb;
                            }
                            var pc = p - upLeft;
                            if (pc < 0)
                            {
                                pc = -pc;
                            }

                            c = rawBytes[i];
                            if (pa <= pb && pa <= pc)
                            {
                                buffer[j++] = (byte)(left + c);
                            }
                            else if (pb <= pc)
                            {
                                buffer[j++] = (byte)(up + c);
                            }
                            else
                            {
                                buffer[j++] = (byte)(upLeft + c);
                            }
                        }
                        break;
                    default:
                        throw new NotSupportedException($"FlateDecode Predictor with value {predictor} is not supported.");
                }
                var pr = buffer;
                buffer = prevRow;
                prevRow = pr;
                predictor = (int)stream.ReadByte();
            }
            ms.Position = 0;
            return ms;
        }


    }
}
