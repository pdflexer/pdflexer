using PdfLexer.Parsers;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace PdfLexer.Filters
{
    public class FlateFilter : IDecoder //, IEncoder
    {
        private PdfNumber DefaultPredictor = PdfCommonNumbers.One;
        private PdfNumber DefaultColors = PdfCommonNumbers.One;
        private PdfNumber DefaultColumns = PdfCommonNumbers.One;
        private PdfNumber DefaultEarlyChange = PdfCommonNumbers.One;
        private PdfNumber DefaultBPC = PdfCommonNumbers.Eight;


        public static FlateFilter Instance { get; } = new FlateFilter();

        public FlateFilter()
        {

        }

        // // TODO migrate to net 6 zlib?
        // public (Stream Data, PdfName Filter, PdfDictionary? Params) Encode(Stream source)
        // {
        //     var output = _ctx.GetTemporaryStream();
        //     output.WriteByte(120);
        //     output.WriteByte(1);
        // 
        //     using (var flate = new DeflateStream(output, CompressionLevel.Fastest, true))
        //     {
        //         source.CopyTo(flate);
        //     }
        // 
        //     output.Seek(0, SeekOrigin.Begin);
        //     var cs = Calculate(output, 65521);
        //     output.Seek(0, SeekOrigin.End);
        //     output.WriteByte((byte)(cs >> 24));
        //     output.WriteByte((byte)(cs >> 16));
        //     output.WriteByte((byte)(cs >> 8));
        //     output.WriteByte((byte)(cs >> 0));
        //     output.Seek(0, SeekOrigin.Begin);
        //     return (output, "/FlateDecode", null);
        // 
        //     static int Calculate(Stream data, int modulus)
        //     {
        //         var s1 = 1;
        //         var s2 = 0;
        // 
        //         int b = 0;
        //         while ((b = data.ReadByte()) != -1)
        //         {
        //             s1 = (s1 + b) % modulus;
        //             s2 = (s1 + s2) % modulus;
        //         }
        //         return s2 * 65536 + s1;
        //     }
        // }

        public Stream Decode(Stream source, PdfDictionary decodeParms)
        {
#if NET6_0
            // var deflated = new ZLibStream(source, CompressionMode.Decompress, false);
            // remove header
            source.ReadByte();
            source.ReadByte();
            var deflated = new DeflateStream(source, CompressionMode.Decompress, false);
#else
            // remove header
            source.ReadByte();
            source.ReadByte();
            var deflated = new DeflateStream(source, CompressionMode.Decompress, false);
#endif


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

        internal Stream TiffDecode(Stream stream, int bpc, int colors, int columns) => new TiffStream(stream, bpc, colors, columns);


        internal Stream PngDecode(Stream stream, int bpc, int colors, int columns) => new PngStream(stream, bpc, colors, columns);
    }

    // PngStream and TiffStream MODIFIED / PORTED FROM PDF.JS, PDF.JS is licensed as follows:
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

    internal class TiffStream : MinBufferStream
    {
        private int bits;
        private int colors;
        private int columns;
        private int rowBytes;
        private byte[] rawBytes;
        private byte[] compArray;

        public TiffStream(Stream stream, int bits, int colors, int columns) : base(stream, (columns * colors * bits + 7) >> 3)
        {
            rowBytes = (columns * colors * bits + 7) >> 3;
            this.bits = bits;
            this.colors = colors;
            this.columns = columns;
            rawBytes = new byte[rowBytes];
            compArray = new byte[colors+1];
        }
        protected override int FillBuffer(byte[] outgoing, int offset, int count)
        {
            if (count < rowBytes)
            {
                throw new NotSupportedException("Stream requires at least " + rowBytes + " read at a time.");
            }

            if (!inner.TryFillArray(rawBytes))
            {
                return 0;
            }

            var inbuf = 0;
            var outbuf = 0;
            var inbits = 0;
            var outbits = 0;
            var pos = offset;
            var i = 0;

            if (bits == 1 && colors == 1)
            {
                // Optimized version of the loop in the "else"-branch
                // for 1 bit-per-component and 1 color TIFF images.
                for (i = 0; i < rowBytes; ++i)
                {
                    var c = rawBytes[i] ^ inbuf;
                    c ^= c >> 1;
                    c ^= c >> 2;
                    c ^= c >> 4;
                    inbuf = (c & 1) << 7;
                    outgoing[pos++] = (byte)c;
                }
            }
            else if (bits == 8)
            {
                for (i = 0; i < colors; ++i)
                {
                    outgoing[pos++] = rawBytes[i];
                }
                for (; i < rowBytes; ++i)
                {
                    outgoing[pos] = (byte)(outgoing[pos - colors] + rawBytes[i]);
                    pos++;
                }
            }
            else if (bits == 16)
            {
                var bytesPerPixel = colors * 2;
                for (i = 0; i < bytesPerPixel; ++i)
                {
                    outgoing[pos++] = rawBytes[i];
                }
                for (; i < rowBytes; i += 2)
                {
                    var sum =
                      ((rawBytes[i] & 0xff) << 8) +
                      (rawBytes[i + 1] & 0xff) +
                      ((outgoing[pos - bytesPerPixel] & 0xff) << 8) +
                      (outgoing[pos - bytesPerPixel + 1] & 0xff);
                    outgoing[pos++] = (byte)((sum >> 8) & 0xff);
                    outgoing[pos++] = (byte)(sum & 0xff);
                }
            }
            else
            {
                var bitMask = (1 << bits) - 1;
                var j = 0;
                var k = pos;
                for (i = 0; i < columns; ++i)
                {
                    for (var kk = 0; kk < colors; ++kk)
                    {
                        if (inbits < bits)
                        {
                            inbuf = (inbuf << 8) | (rawBytes[j++] & 0xff);
                            inbits += 8;
                        }
                        compArray[kk] = (byte)
                          ((compArray[kk] + (inbuf >> (inbits - bits))) & bitMask);
                        inbits -= bits;
                        outbuf = (outbuf << bits) | compArray[kk];
                        outbits += bits;
                        if (outbits >= 8)
                        {
                            outgoing[k++] = (byte)((outbuf >> (outbits - 8)) & 0xff);
                            outbits -= 8;
                        }
                    }
                }
                if (outbits > 0)
                {
                    outgoing[k++] = (byte) (
                      (outbuf << (8 - outbits)) + (inbuf & ((1 << (8 - outbits)) - 1))
                      );
                }
            }
            return rowBytes;
        }
    }

    internal class PngStream : MinBufferStream
    {
        private readonly int pixBytes;
        private readonly int rowBytes;
        private byte[] prevRow;
        private byte[] rawBytes;

        public PngStream(Stream inner, int bpc, int colors, int columns) : base(inner, (columns * colors * bpc + 7) >> 3)
        {
            this.pixBytes = (colors * bpc + 7) >> 3;
            this.rowBytes = (columns * colors * bpc + 7) >> 3;

            this.prevRow = new byte[rowBytes];
            this.rawBytes = new byte[rowBytes];
        }
        protected override int FillBuffer(byte[] outgoing, int offset, int count)
        {
            int i;
            var j = offset;
            byte up;
            byte c;

            var predictor = (int)inner.ReadByte();

            if (!Fill())
            {
                return 0;
            }

            switch (predictor)
            {
                case 0:
                    Array.Copy(rawBytes, 0, outgoing, offset, rawBytes.Length);
                    Array.Copy(rawBytes, prevRow, rawBytes.Length);
                    return rawBytes.Length;
                case 1:
                    j = offset;
                    i = 0;
                    for (; i < pixBytes; i++)
                    {
                        outgoing[j++] = rawBytes[i];
                    }
                    for (; i < rowBytes; i++)
                    {
                        outgoing[j] = (byte)((outgoing[j - pixBytes] + rawBytes[i]) & 0xff);
                        j++;
                    }
                    break;
                case 2:
                    j = offset;
                    for (i = 0; i < rowBytes; ++i)
                    {
                        outgoing[j++] = (byte)((prevRow[i] + rawBytes[i]) & 0xff);
                    }
                    break;
                case 3:
                    j = offset;
                    for (i = 0; i < pixBytes; ++i)
                    {
                        outgoing[j++] = (byte)((prevRow[i] >> 1) + rawBytes[i]);
                    }
                    for (; i < rowBytes; ++i)
                    {
                        outgoing[j] = (byte)
                          ((((prevRow[i] + outgoing[j - pixBytes]) >> 1) + rawBytes[i]) & 0xff);
                        j++;
                    }
                    break;
                case 4:
                    j = offset;
                    for (i = 0; i < pixBytes; ++i)
                    {
                        up = prevRow[i];
                        c = rawBytes[i];
                        outgoing[j++] = (byte)(up + c);
                    }
                    for (; i < rowBytes; ++i)
                    {
                        up = prevRow[i];
                        var upLeft = prevRow[i - pixBytes];
                        var left = outgoing[j - pixBytes];
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
                            outgoing[j++] = (byte)(left + c);
                        }
                        else if (pb <= pc)
                        {
                            outgoing[j++] = (byte)(up + c);
                        }
                        else
                        {
                            outgoing[j++] = (byte)(upLeft + c);
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException($"FlateDecode Predictor with value {predictor} is not supported.");
            }

            Array.Copy(outgoing, offset, prevRow, 0, rawBytes.Length);

            return j - offset;

            bool Fill()
            {
                var os = 0;
                var read = 0;
                while (os < rowBytes && (read = inner.Read(rawBytes, os, rowBytes - os)) > 0)
                {
                    os += read;
                }
                if (os != rowBytes)
                {
                    return false;
                }

                return true;
            }
        }
    }

}
