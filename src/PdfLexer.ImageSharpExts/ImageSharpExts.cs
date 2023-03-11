using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.DOM.ColorSpaces;
using PdfLexer.Filters;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;

namespace PdfLexer.Images;

public static class ImageSharpExts
{
    public static XObjImage CreatePdfImage(this Image img)
    {
        // TODO
        // var info = Image.Identify(new byte[1]);
        // overloads for JPEG / JPEG200 -> copy raw data
        // overloads for PNG -> copy raw data where possible

        var xi = new XObjImage();
        xi.Width = img.Width;
        xi.Height = img.Height;
        xi.BitsPerComponent = 8;
        xi.ColorSpace = PdfName.DeviceRGB;

        if (img.PixelType.AlphaRepresentation.HasValue && img.PixelType.AlphaRepresentation.Value != PixelAlphaRepresentation.None)
        {
            byte[] buff = new byte[3];
            var rgb = img.CloneAs<Rgba32>();
            var writer = new ZLibLexerStream();
            var mask = new ZLibLexerStream();
            rgb.ProcessPixelRows(pa =>
            {
                for (var i = 0; i < pa.Height; i++)
                {
                    var row = MemoryMarshal.Cast<Rgba32, byte>(pa.GetRowSpan(i));
                    for (var x=0;x<pa.Width; x++)
                    {
                        var os = x * 4;
                        if (img.PixelType.AlphaRepresentation.Value == PixelAlphaRepresentation.Unassociated)
                        {
                            writer.Stream.Write(row.Slice(os, 3));
                        } else
                        {
                            var s = (double)row[os + 3];
                            buff[0] = (byte)Math.Min(255, row[os] / s);
                            buff[1] = (byte)Math.Min(255, row[os+1] / s);
                            buff[2] = (byte)Math.Min(255, row[os+2] / s);
                        }
                        mask.Stream.WriteByte(row[os + 3]);
                    }
                }
            });
            xi.Contents = writer.Complete();
            var mi = new XObjImage();
            mi.BitsPerComponent = 8;
            mi.Width = img.Width;
            mi.Height = img.Height;
            mi.ColorSpace = PdfName.DeviceGray;
            mi.Contents = writer.Complete();
            xi.SMask = mi.Stream;

        } else
        {
            var rgb = img.CloneAs<Rgb24>();
            var writer = new ZLibLexerStream();
            rgb.ProcessPixelRows(pa =>
            {
                for (var i = 0; i < pa.Height; i++)
                {
                    var row = MemoryMarshal.Cast<Rgb24, byte>(pa.GetRowSpan(i));
                    writer.Stream.Write(row);
                }
            });
            xi.Contents = writer.Complete();
        }
        return xi;
    }

    [Obsolete("Use GetImageSharp.")]
    public static Image GetImage(this PdfImage img, ParsingContext ctx) => GetImageSharp(img, ctx);
    public static Image GetImageSharp(this PdfImage img, ParsingContext ctx)
    {
        var dict = img.XObj.NativeObject;
        if (!dict.TryGetValue(PdfName.Filter, out var filter))
        {
            using var str = img.XObj.Contents.GetDecodedStream();
            return GetFromDecoded(ctx, img.XObj, str);
        }

        filter = filter.Resolve();
        if (filter.Type == PdfObjectType.ArrayObj)
        {
            var arr = (PdfArray)filter;
            if (arr.Count == 1 && arr[0].Type == PdfObjectType.NameObj)
            {
                return GetFromDecodedSingleFilter(ctx, img.XObj, (PdfName)arr[0]);
            }
            using var str = img.XObj.Contents.GetDecodedStream();
            return GetFromDecoded(ctx, img.XObj, str);
        }
        else if (filter.Type == PdfObjectType.NameObj)
        {
            return GetFromDecodedSingleFilter(ctx, img.XObj, (PdfName)filter);
        }
        else
        {
            using var str = img.XObj.Contents.GetDecodedStream();
            return GetFromDecoded(ctx, img.XObj, str);
        }
    }

    private static Image GetFromDecodedSingleFilter(ParsingContext ctx, XObjImage img, PdfName filter)
    {
        switch (filter.Value)
        {
            case "DCTDecode":
            case "JPXDecode":
                return Image.Load(img.Contents.GetEncodedData());

        }
        using var str = img.Contents.GetDecodedStream();
        return GetFromDecoded(ctx, img, str);
    }

    private static Image GetFromDecoded(ParsingContext ctx, XObjImage image, Stream data)
    {
        // /Mask -> another image w/ /ImageMask true
        var isMasked = image.ImageMask?.Value ?? false;
        var colorSpace = isMasked ? DeviceGray.Instance : image.ColorSpace == null ? DeviceRGB.Instance : ColorSpace.Get(ctx, image.ColorSpace);
        var mask = image.Mask;
        List<float>? colourMask = null;
        if (!isMasked && mask != null && mask.Type == PdfObjectType.ArrayObj)
        {
            var arr = mask.GetValue<PdfArray>();
            colourMask = arr.Select(x => (float)x.GetValue<PdfNumber>()).ToList();
        }

        List<float>? decode = null;
        var dc = image.Decode;
        if (dc != null)
        {
            var cpp = colorSpace.Components;
            decode = new List<float>();
            bool start = true;
            foreach (var item in dc)
            {
                if (item.Type == PdfObjectType.NumericObj)
                {
                    decode.Add((PdfNumber)item);
                }
                else
                {
                    decode.Add(start ? 0 : 1);
                }
                start = !start;
            }
            if (decode.Count < cpp * 2)
            {
                var diff = decode.Count - cpp * 2;
                var first = diff % 2 == 0;
                for (var i = 0; i < diff; i++)
                {
                    decode.Add(first ? 0 : 1);
                }
            }
        }

        if (!isMasked && image.BitsPerComponent == null)
        {
            ctx.Error("Non-masked image missing bits per component, defaulting to 8.");
        }

        if (image.Width == null)
        {
            ctx.Error("Attempted to extract image but was missing width");
            return new Image<Rgba32>(1, 1);
        }
        if (image.Height == null)
        {
            ctx.Error("Attempted to extract image but was missing height");
            return new Image<Rgba32>(1, 1);
        }

        return GetFromDecoded(
            ctx,
            image.Width,
            image.Height,
            isMasked ? (image.BitsPerComponent ?? 1) : (image.BitsPerComponent ?? 8),
            colorSpace,
            decode,
            data,
            isMasked,
            colourMask);
    }

    private static Image GetFromDecoded(ParsingContext ctx, 
        int width, int height, int bpc, IColorSpace cs, List<float>? decode, Stream data, bool isMask, List<float>? mask)
    {
        var cpp = cs.Components;
        var bpp = cpp * bpc;
        var cc = width * cpp;
        var bits = cc * bpc;
        var bytes = bits / 8;
        if (bits % 8 > 0 || bytes == 0) { bytes++; }
        var roundedBits = bytes * 8;
        var roundedComps = roundedBits / bpc;
        var comps = new ushort[roundedComps];

        var buffer = new byte[bytes];

        bool[]? masking = mask != null ? new bool[width] : null;

        bool runDecode = false;
        if (decode != null && !cs.IsDefaultDecode(bpc, decode))
        {
            runDecode = true;
        }


        if (bpc <= 8)
        {
            double existingMax = Math.Pow(2, bpc) - 1;
            int decodeOutputScale = 1;
            int scale = 1;
            if (cs.Name != PdfName.Indexed)
            {
                scale = bpc switch { 1 => 255, 2 => 85, 4 => 17, _ => 1 };
                decodeOutputScale = 255;
            }

            ConvertToBytes convert = bpc switch
            {
                1 => ReadLine1bpc,
                2 => ReadLine2bpc,
                4 => ReadLine4bpc,
                8 => null!,
                _ => throw new ApplicationException("Unsupported bit size: " + bpc)
            };
            var compBuffer = new byte[roundedComps];
            var img = new Image<Rgba32>(width, height);
            img.ProcessPixelRows(ra =>
            {
                Span<byte> components = compBuffer;
                if (compBuffer.Length > cc)
                {
                    components = components.Slice(0, cc);
                }
                for (int y = 0; y < height; y++)
                {
                    int total = 0;
                    int read = 0;
                    while ((read = data.Read(buffer, total, buffer.Length - total)) > 0)
                    {
                        total += read;
                    }
                    if (total < buffer.Length)
                    {
                        ctx.Error($"Image data ended before filling image, row {y+1}, {total} bytes read out of {buffer.Length} expected.");
                        // see LrUpVnZ0SQZWkawizVTIwQ
                        // may be normal for ccitt
                    }

                    if (bpc == 8) // shortcut as 8b values already in correct byte
                    {
                        compBuffer = buffer;
                        components = buffer;
                    }
                    else
                    {
                        convert(buffer, compBuffer); // bit math to extract byte values
                    }
                    if (mask != null) // component matched masking
                    {
                        CalculateMasking(cpp, compBuffer, mask, masking!);
                    }
                    if (runDecode)
                    {
                        Decode(cpp, compBuffer, decode!, existingMax, decodeOutputScale);
                    } else if (scale > 1)
                    {
                        for (var i=0;i< compBuffer.Length;i++)
                        {
                            compBuffer[i] = (byte)(scale*compBuffer[i]);
                        }
                    }
                    var row = MemoryMarshal.Cast<Rgba32, byte>(ra.GetRowSpan(y));
                    cs.CopyRowToRBGA8Span(components, row);
                    if (masking != null) // component matched masking
                    {
                        ApplyMasking(row, masking);
                    }
                    if (isMask) // 1b masking
                    {
                        ApplyMasking(row);
                    }
                }
            });
            return img;
        } else
        {
            var compBuffer = new ushort[roundedComps];
            var img = new Image<Rgba64>(width, height);

            int scale = (int)((Math.Pow(2, 16) - 1) / (Math.Pow(2, bpc) - 1));

            img.ProcessPixelRows(ra =>
            {
                Span<ushort> components = compBuffer;
                if (compBuffer.Length > cc)
                {
                    components = components.Slice(0, cc);
                }
                for (int y = 0; y < height; y++)
                {
                    int total = 0;
                    int read = 0;
                    while ((read = data.Read(buffer, total, buffer.Length - total)) > 0)
                    {
                        total += read;
                    }
                    if (total < buffer.Length)
                    {
                        ctx.Error($"Image data ended before filling image, row {y + 1}, {total} bytes read out of {buffer.Length} expected.");
                    }

                    ReadLine16bpc(buffer, compBuffer);
                    if (mask != null)
                    {
                        CalculateMasking(cpp, compBuffer, mask, masking!);
                    }
                    if (runDecode)
                    {
                        Decode(cpp, compBuffer, decode!, 65535);
                    }
                    else if (scale > 1)
                    {
                        for (var i = 0; i < compBuffer.Length; i++)
                        {
                            compBuffer[i] = (byte)(scale * compBuffer[i]);
                        }
                    }
                    var row = MemoryMarshal.Cast<Rgba64, ushort>(ra.GetRowSpan(y));
                    cs.CopyRowToRBGA16Span(components, row);
                    if (masking != null)
                    {
                        ApplyMasking(row, masking);
                    }
                }
            });
            return img;
        }
    }

    private static void CalculateMasking(int cpp, Span<byte> comps, List<float> mask, bool[] maskedPixels)
    {
        for (var p=0; p< comps.Length/cpp; p++)
        {
            bool match = true;
            var os = p * cpp;
            for (int i = 0; i < cpp; i++)
            {
                var val = comps[os + i];
                if (val < mask[i * 2] || val > mask[i * 2 + 1])
                {
                    match = false;
                    break;
                }
            }
            maskedPixels[p] = match;
        }
    }
    private static void ApplyMasking(Span<byte> comps, bool[] maskedPixels)
    {
        for (var p = 0; p < comps.Length / 4; p++) // bgra
        {
            if (maskedPixels[p])
            {
                comps[p * 4 + 3] = 0;
            }
        }
    }
    private static void ApplyMasking(Span<byte> comps)
    {
        for (var p = 0; p < comps.Length / 4; p++) // bgra
        {
            var os = p * 4;
            if (comps[os] == 255)
            {
                comps[os] = 0;
                comps[os + 1] = 0;
                comps[os + 2] = 0;
                comps[os + 3] = 0;
            }
        }
    }

    private static void ApplyMasking(Span<ushort> comps, bool[] maskedPixels)
    {
        for (var p = 0; p < comps.Length / 4; p++) // bgra
        {
            if (maskedPixels[p])
            {
                comps[p * 4 + 3] = 0;
            }
        }
    }
    private static void CalculateMasking(int cpp, Span<ushort> comps, List<float> mask, bool[] maskedPixels)
    {
        for (var p = 0; p < comps.Length / cpp; p++)
        {
            bool match = true;
            var os = p * cpp;
            for (int i = 0; i < cpp; i++)
            {
                var val = comps[os + i];
                if (val < mask[i * 2] || val > mask[i * 2 + 1])
                {
                    match = false;
                    break;
                }
            }
            maskedPixels[p] = match;
        }
    }

    private static void Decode(int cpp, Span<byte> comps, List<float> decode, double decodeScale, int outputScale)
    {
        for (var p = 0; p < comps.Length / cpp; p++)
        {
            var os = p * cpp;
            for (var i = 0; i < cpp; i++)
            {
                comps[os + i] = (byte)((decode[0 + 2 * i] + (comps[os + i] * ((double)(decode[1 + 2 * i] - decode[0 + 2 * i]) / decodeScale)))*outputScale);
            }
        }
    }
    private static void Decode(int cpp, Span<ushort> comps, List<float> decode, double scale)
    {
        for (var p = 0; p < comps.Length / cpp; p++)
        {
            var os = p * cpp;
            for (var i = 0; i < cpp; i++)
            {
                comps[os + i] = (byte)((decode[0 + 2 * i] + (comps[os + i] * ((double)(decode[1 + 2 * i] - decode[0 + 2 * i]) / scale))) * scale);
            }
        }
    }

    private delegate void ConvertToBytes(ReadOnlySpan<byte> buffer, Span<byte> components);


    private static void ReadLine1bpc(ReadOnlySpan<byte> buffer, Span<byte> components)
    {
        int pos = 0;
        for (var i = 0; i < buffer.Length; i++)
        {
            var b = buffer[i];
            components[pos++] = (byte)((b & 0b10000000) >> 7);
            components[pos++] = (byte)((b & 0b01000000) >> 6);
            components[pos++] = (byte)((b & 0b00100000) >> 5);
            components[pos++] = (byte)((b & 0b00010000) >> 4);
            components[pos++] = (byte)((b & 0b00001000) >> 3);
            components[pos++] = (byte)((b & 0b00000100) >> 2);
            components[pos++] = (byte)((b & 0b00000010) >> 1);
            components[pos++] = (byte)(b & 0b00000001);
        }
    }

    private static void ReadLine2bpc(ReadOnlySpan<byte> buffer, Span<byte> components)
    {
        int pos = 0;
        for (var i = 0; i < buffer.Length; i++)
        {
            var b = buffer[i];
            components[pos++] = (byte)((b & 0b11000000) >> 6);
            components[pos++] = (byte)((b & 0b00110000) >> 4);
            components[pos++] = (byte)((b & 0b00001100) >> 2);
            components[pos++] = (byte)(b & 0b00000011);
        }
    }

    private static void ReadLine4bpc(ReadOnlySpan<byte> buffer, Span<byte> components)
    {
        int pos = 0;
        for (var i = 0; i < buffer.Length; i++)
        {
            var b = buffer[i];
            components[pos++] = (byte)(b >> 4);
            components[pos++] = (byte)(b & 0b00001111);
        }
    }


    private static void ReadLine16bpc(ReadOnlySpan<byte> buffer, Span<ushort> components)
    {
        int pos = 0;
        for (var i = 0; i < buffer.Length /2; i++)
        {
            var b1 = buffer[2*i];
            var b2 = buffer[2 * i+1];
            components[pos++] = (ushort)((b1 << 8) | b2);
        }
    }
}
