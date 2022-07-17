using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM.ColorSpaces
{
    public class DeviceGray : IColorSpace
    {
        public static DeviceGray Instance = new DeviceGray();
        public int Components => 1;
        public PdfName Name => PdfName.DeviceGray;

        public void CopyRowToRBGA8Span(ReadOnlySpan<byte> row, Span<byte> output)
        {
            for (var p = 0; p < row.Length; p++)
            {
                var oos = p * 4;
                var v = row[p];
                output[oos] = v;
                output[oos + 1] = v;
                output[oos + 2] = v;
                output[oos + 3] = 255;
            }
        }

        public void CopyRowToRBGA16Span(ReadOnlySpan<ushort> row, Span<ushort> output)
        {
            for (var p = 0; p < row.Length; p++)
            {
                var oos = p * 4;
                var v = row[p];
                output[oos] = v;
                output[oos + 1] = v;
                output[oos + 2] = v;
                output[oos + 3] = ushort.MaxValue;
            }
        }
    }
}
