using PdfLexer.Content;
using PdfLexer.Parsers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.PixelFormats;

namespace PdfLexer.Images;

public static class ImageSharpExts
{
    public static Image GetImage<TPixel>(this PdfImage img, ParsingContext ctx)
    {
        var dict = img.Stream.Dictionary;
        if (!dict.TryGetValue(PdfName.Filter, out var filter))
        {
            using var str = img.Stream.Contents.GetDecodedStream(ctx);
            return GetFromDecoded(ctx, img.Stream.Dictionary, str);
        }

        filter = filter.Resolve();
        if (filter.Type == PdfObjectType.ArrayObj)
        {
            var arr = (PdfArray)filter;
            if (arr.Count == 1 && arr[0].Type == PdfObjectType.NameObj)
            {
                return GetFromDecodedSingleFilter(ctx, img, (PdfName)arr[0]);
            }
            using var str = img.Stream.Contents.GetDecodedStream(ctx);
            return GetFromDecoded(ctx, img.Stream.Dictionary, str);
        }
        else if (filter.Type == PdfObjectType.NameObj)
        {
            return GetFromDecodedSingleFilter(ctx, img, (PdfName)filter);
        }
        else
        {
            using var str = img.Stream.Contents.GetDecodedStream(ctx);
            return GetFromDecoded(ctx, img.Stream.Dictionary, str);
        }
    }

    private static (PdfName Cs, PdfName? BaseCs, byte[]? IndexData) GetColorSpace(ParsingContext ctx, PdfDictionary dict, bool isMasked)
    {
        if (isMasked) { return (PdfName.DeviceGray, null,null); }
        if (!dict.TryGetValue(PdfName.ColorSpace, out var cs))
        {
            throw new ApplicationException("Non masked image had no colorspace defined.");
        }
        cs = cs.Resolve();
        switch (cs)
        {
            case PdfName nm:
                return (nm, null, null);
            case PdfArray arr:
                var indexed = arr[0].GetValue<PdfName>();
                var baseCs = arr[1].GetValue<PdfName>();
                var hival = arr[2].GetValue<PdfNumber>();
                var data = arr[3].Resolve();
                byte[] lookup = null;
                if (data.Type == PdfObjectType.StringObj)
                {
                    var str = (PdfString)data;
                    lookup = str.GetRawBytes();
                } else if (data.Type == PdfObjectType.StreamObj)
                {
                    lookup = ((PdfStream)data).Contents.GetDecodedData(ctx);
                } else
                {
                    throw new ApplicationException("Index colorspace had unknown lookup table: " + cs.GetPdfObjType());
                }
                return (indexed, baseCs, lookup); 
            default:
                throw new ApplicationException("Non masked image had unknown colorspace defined: " + cs.GetPdfObjType());
        }
    }

    private static Image GetFromDecodedSingleFilter(ParsingContext ctx, PdfImage img, PdfName filter)
    {
        switch (filter.Value)
        {
            case "/CCITTFaxDecode":
                return GetCCTImage(
                        img.Stream.Dictionary.GetRequiredValue<PdfNumber>(PdfName.Width),
                        img.Stream.Dictionary.GetRequiredValue<PdfNumber>(PdfName.Height),
                        img.Stream.Dictionary.GetRequiredValue<PdfNumber>(PdfName.BitsPerComponent),
                        img.Stream.Contents.GetEncodedData());
            case "/DCTDecode":
            case "/JPXDecode":
                return Image.Load(img.Stream.Contents.GetEncodedData());

        }
        using var str = img.Stream.Contents.GetDecodedStream(ctx);
        return GetFromDecoded(ctx, img.Stream.Dictionary, str);
    }

    private static Image GetFromDecoded(ParsingContext ctx, PdfDictionary dict, Stream data)
    {
        var isMasked = dict.ContainsKey(PdfName.ImageMask);
        var (cs, baseCs, lookup) = GetColorSpace(ctx, dict, isMasked);
        List<float> decode = null;
        if (dict.TryGetValue<PdfArray>(PdfName.Decode, out var dc, false))
        {
            var cpp = GetCpp(cs);
            decode = new List<float>();
            bool start = true;
            foreach (var item in dc) { 
                if (item.Type == PdfObjectType.NumericObj)
                {
                    decode.Add((PdfNumber)item);
                } else
                {
                    decode.Add(start ? 0 : 1);
                }
                start = !start;
            }
            if (decode.Count < cpp * 2)
            {
                var diff = decode.Count - cpp * 2;
                var first = diff % 2 == 0;
                for (var i=0; i<diff; i++)
                {
                    decode.Add(first ? 0 : 1);
                }
            }
        }

        
        return GetFromDecoded(
            dict.GetRequiredValue<PdfNumber>(PdfName.Width),
            dict.GetRequiredValue<PdfNumber>(PdfName.Height),
            isMasked ? (dict.GetOptionalValue<PdfNumber>(PdfName.BitsPerComponent) ?? 1) : dict.GetRequiredValue<PdfNumber>(PdfName.BitsPerComponent),
            cs,
            baseCs,
            lookup,
            decode,
            data);
    }

    private static int GetCpp(PdfName cs) => cs.Value switch
    {
        "/DeviceGray" => 1,
        "/CalGray" => 1,
        "/DeviceRGB" => 3,
        "/CalRGB" => 3,
        "/DeviceCMYK" => 4,
        "/Indexed" => 1,
        _ => 3 // todo -> how to handle DeviceN
    };

    private static byte GetIndexed11(byte val, byte[] lookup)
    {
        if (val > lookup.Length) { return val; }
        return lookup[val];
    }

    private static (byte r, byte g, byte b) GetIndexed13(int val, byte[] lookup)
    {
        var loc = val * 3;
        if (loc+2 > lookup.Length) { return (0, 0, 0); }
        var r = lookup[loc];
        var g = lookup[loc+1];
        var b = lookup[loc+2];
        return (r, g, b);
    }

    private static Image GetFromDecoded(int width, int height, int bpc, PdfName cs, PdfName? baseCs, byte[]? lookup, List<float>? decode, Stream data)
    {
        var cpp = GetCpp(cs);
        Action<Stream, int, int[]> readLine = bpc switch
        {
            1 => ReadLine1bpc,
            2 => ReadLine2bpc,
            4 => ReadLine4bpc,
            8 => ReadLine8bpc,
            16 => ReadLine16bpc,
            _ => throw new ApplicationException("Unsupported bit size: " + bpc)
        };
        var bpp = cpp * bpc;
        var cc = width * cpp;
        var bits = cc * bpc;
        var bytes = bits / 8;
        if (bytes % 8 > 1 || bytes == 0) { bytes++; }
        var roundedBits = bytes * 8;
        var roundedComps = roundedBits / bpc;
        var comps = new int[roundedComps];


        var basecpp = cpp;
        if (baseCs != null)
        {
            basecpp = GetCpp(baseCs);
        }


        switch (basecpp)
        {
            case 1:
                {

                    var scaling = Math.Pow(2, 8 / bpc) - 1;
                    var dScale = Math.Pow(2, bpc) - 1;
                    var img = new Image<Rgb24>(width, height);
                    for (int y = 0; y < height; y++)
                    {
                        readLine(data, cc, comps);
                        for (int x = 0; x < width; x++)
                        {
                            var os = x * cpp;
                            var c = comps[os];
                            var val = (byte)(c);
                            if (decode != null)
                            {
                                val = (byte)(decode[0] + (val * ((decode[1]-decode[0])/dScale)));
                            }
                            if (lookup != null)
                            {
                                val = GetIndexed11(val, lookup);
                            }
                            val = (byte)(c * scaling);
                            img[x, y] = new Rgb24 { B = val, R = val, G = val };
                        }
                    }
                    return img;
                }
            case 3:
                {
                    if (bpc == 16)
                    {
                        int scaling = (int)(Math.Pow(2, 16 / bpc) - 1);
                        var dScale = Math.Pow(2, bpc) - 1;
                        var img = new Image<Rgba64>(width, height);
                        for (int y = 0; y < height; y++)
                        {
                            readLine(data, cc, comps);
                            for (int x = 0; x < width; x++)
                            {
                                var os = x * cpp;
                                int r, g, b;
                                if (lookup != null)
                                {
                                    if (decode != null)
                                    {
                                        os = (byte)(decode[0] + (os * ((decode[1] - decode[0]) / dScale)));
                                    }
                                    (r,g,b) = GetIndexed13(os, lookup);
                                } else
                                {
                                    r = comps[os];
                                    g = comps[os + 1];
                                    b = comps[os + 2];
                                    if (decode != null)
                                    {
                                        r = (int)(decode[0] + (r * ((decode[1] - decode[0]) / dScale)));
                                        g = (int)(decode[2] + (r * ((decode[3] - decode[2]) / dScale)));
                                        b = (int)(decode[4] + (r * ((decode[5] - decode[4]) / dScale)));
                                    }
                                }

                                r = r * scaling;
                                g = g * scaling;
                                b = b * scaling;

                                img[x, y] = new Rgba64
                                {
                                    R = (ushort)r,
                                    G = (ushort)g,
                                    B = (ushort)b,
                                    A = ushort.MaxValue
                                };
                            }
                        }
                        return img;
                    }
                    else
                    {
                        int scaling = (int)(Math.Pow(2, 8 / bpc) - 1);
                        var dScale = Math.Pow(2, bpc) - 1;
                        var img = new Image<Rgba32>(width, height);
                        for (int y = 0; y < height; y++)
                        {
                            readLine(data, cc, comps);
                            for (int x = 0; x < width; x++)
                            {
                                var os = x * cpp;
                                int r, g, b;
                                if (lookup != null)
                                {
                                    var sb = comps[os];
                                    if (decode != null)
                                    {
                                        sb = (byte)(decode[0] + (sb * ((decode[1] - decode[0]) / dScale)));
                                    }
                                    (r, g, b) = GetIndexed13(sb, lookup);
                                }
                                else
                                {
                                    r = comps[os];
                                    g = comps[os + 1];
                                    b = comps[os + 2];
                                    if (decode != null)
                                    {
                                        r = (int)(decode[0] + (r * ((decode[1] - decode[0]) / dScale)));
                                        g = (int)(decode[2] + (r * ((decode[3] - decode[2]) / dScale)));
                                        b = (int)(decode[4] + (r * ((decode[5] - decode[4]) / dScale)));
                                    }
                                }

                                r = r * scaling;
                                g = g * scaling;
                                b = b * scaling;

                                img[x, y] = new Rgba32
                                {
                                    R = (byte)r,
                                    G = (byte)g,
                                    B = (byte)b,
                                    A = 255
                                };
                            }
                        }
                        return img;
                    }

                }
            case 4:
                break;
        }

        throw new NotImplementedException("Type not implemented");


    }

    private static void ReadLine1bpc(Stream stream, int width, int[] components)
    {
        int read = 0;
        while (read < width)
        {
            var b = stream.ReadByte();
            components[read + 0] = (b & 0b10000000) >> 7;
            components[read + 1] = (b & 0b01000000) >> 6;
            components[read + 2] = (b & 0b00100000) >> 5;
            components[read + 3] = (b & 0b00010000) >> 4;
            components[read + 4] = (b & 0b00001000) >> 3;
            components[read + 5] = (b & 0b00000100) >> 2;
            components[read + 6] = (b & 0b00000010) >> 1;
            components[read + 7] = (b & 0b00000001);
            read += 8;
        }
    }
    private static void ReadLine2bpc(Stream stream, int width, int[] components)
    {
        int read = 0;
        while (read < width)
        {
            var b = stream.ReadByte();
            components[read + 0] = (b & 0b11000000) >> 6;
            components[read + 1] = (b & 0b00110000) >> 4;
            components[read + 2] = (b & 0b00001100) >> 2;
            components[read + 3] = (b & 0b00000011);
            read += 4;
        }
    }

    private static void ReadLine4bpc(Stream stream, int width, int[] components)
    {
        int read = 0;
        while (read < width)
        {
            var b = stream.ReadByte();
            components[read + 0] = b >> 4;
            components[read + 1] = (b & 0b00001111);
            read += 2;
        }
    }

    private static void ReadLine8bpc(Stream stream, int width, int[] components)
    {
        int read = 0;
        while (read < width)
        {
            var b = stream.ReadByte();
            components[read] = b;
            read += 1;
        }
    }

    private static void ReadLine16bpc(Stream stream, int width, int[] components)
    {
        int read = 0;
        while (read < width)
        {
            var b1 = stream.ReadByte();
            var b2 = stream.ReadByte();
            components[read] = b1 * (2 ^ 8) + b2;
            read += 1;
        }
    }

    private static Image GetDCTImage(Stream data)
    {
        return Image.Load(data);
    }

    private static Image GetCCTImage(int width, int height, int bpp, Stream data)
    {
        using var str = GetTiffStream(width, height, bpp, data);
        return Image.Load(str, new TiffDecoder());
    }

    private static Stream GetTiffStream(int width, int height, int bpp, Stream image)
    {
        const short TIFF_BIGENDIAN = 0x4d4d;
        const short TIFF_LITTLEENDIAN = 0x4949;

        const int ifd_length = 10;
        const int header_length = 10 + (ifd_length * 12 + 4);
        var buffer = new MemoryStream(header_length + (int)image.Length);
        // TIFF Header
        buffer.Write(BitConverter.GetBytes(BitConverter.IsLittleEndian ? TIFF_LITTLEENDIAN : TIFF_BIGENDIAN), 0, 2); // tiff_magic (big/little endianness)
        buffer.Write(BitConverter.GetBytes((uint)42), 0, 2);         // tiff_version
        buffer.Write(BitConverter.GetBytes((uint)8), 0, 4);          // first_ifd (Image file directory) / offset
        buffer.Write(BitConverter.GetBytes((uint)ifd_length), 0, 2); // ifd_length, number of tags (ifd entries)

        // Dictionary should be in order based on the TiffTag value
        WriteTiffTag(buffer, TiffTag.SUBFILETYPE, TiffType.LONG, 1, 0);
        WriteTiffTag(buffer, TiffTag.IMAGEWIDTH, TiffType.LONG, 1, (uint)width);
        WriteTiffTag(buffer, TiffTag.IMAGELENGTH, TiffType.LONG, 1, (uint)height);
        WriteTiffTag(buffer, TiffTag.BITSPERSAMPLE, TiffType.SHORT, 1, (uint)bpp);
        WriteTiffTag(buffer, TiffTag.COMPRESSION, TiffType.SHORT, 1, (uint)Compression.CCITTFAX4); // CCITT Group 4 fax encoding.
        WriteTiffTag(buffer, TiffTag.PHOTOMETRIC, TiffType.SHORT, 1, 0); // WhiteIsZero
        WriteTiffTag(buffer, TiffTag.STRIPOFFSETS, TiffType.LONG, 1, header_length);
        WriteTiffTag(buffer, TiffTag.SAMPLESPERPIXEL, TiffType.SHORT, 1, 1);
        WriteTiffTag(buffer, TiffTag.ROWSPERSTRIP, TiffType.LONG, 1, (uint)height);
        WriteTiffTag(buffer, TiffTag.STRIPBYTECOUNTS, TiffType.LONG, 1, (uint)image.Length);

        // Next IFD Offset
        buffer.Write(BitConverter.GetBytes((uint)0), 0, 4);
        image.CopyTo(buffer);
        buffer.Seek(0, SeekOrigin.Begin);
        return buffer;
    }

    private static void WriteTiffTag(Stream stream, TiffTag tag, TiffType type, uint count, uint value)
    {
        stream.Write(BitConverter.GetBytes((uint)tag), 0, 2);
        stream.Write(BitConverter.GetBytes((uint)type), 0, 2);
        stream.Write(BitConverter.GetBytes(count), 0, 4);
        stream.Write(BitConverter.GetBytes(value), 0, 4);
    }
}
