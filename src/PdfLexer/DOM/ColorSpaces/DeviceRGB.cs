using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM.ColorSpaces;

internal class DeviceRGB : IColorSpace
{
    public static readonly DeviceRGB Instance = new ();
    public int Components => 3;
    public PdfName Name => PdfName.DeviceRGB;
    public bool IsPredefined => true;

    public (ushort r, ushort g, ushort b) GetRGB16(ushort[] components, int offset)
    {
        return (components[offset], components[offset + 1], components[offset + 2]);

    }
    public (byte r, byte g, byte b) GetRGB8(ushort[] components, int offset)
    {
        return ((byte)(components[offset] >> 8), (byte)(components[offset + 1] >> 8), (byte)(components[offset + 2] >> 8));
    }

    public void CopyRowToRBGA8Span(ReadOnlySpan<byte> row, Span<byte> output)
    {
        for (var p = 0; p < row.Length / 3; p++)
        {
            var ios = p * 3;
            var oos = p * 4;
            output[oos] = row[ios];
            output[oos + 1] = row[ios + 1];
            output[oos + 2] = row[ios + 2];
            output[oos + 3] = 255;
        }
    }

    public void CopyRowToRBGA16Span(ReadOnlySpan<ushort> row, Span<ushort> output)
    {
        for (var p = 0; p < row.Length / 3; p++)
        {
            var ios = p * 3;
            var oos = p * 4;
            output[oos] = row[ios];
            output[oos + 1] = row[ios + 1];
            output[oos + 2] = row[ios + 2];
            output[oos + 3] = ushort.MaxValue;
        }
    }
}
