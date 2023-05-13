using PdfLexer.Writing;
using System.Numerics;

namespace PdfLexer.DOM.ColorSpaces;

internal class DeviceGray : IColorSpace
{
    public static readonly DeviceGray Instance = new ();
    public int Components => 1;
    public PdfName Name => PdfName.DeviceGray;
    public bool IsPredefined => true;

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

    public static IColor<double> Black { get; } = new DeviceGrayColor<double>(0);
    public static IColor<T> GetBlack<T>() where T : struct, IFloatingPoint<T> { return new DeviceGrayColor<T>(T.Zero); }
}

internal class DeviceGrayColor<T> : IColor<T> where T : struct, IFloatingPoint<T>
{
    public IColorSpace Colorspace => DeviceGray.Instance;

    public bool IsRGBConvertable => true;

    public T Value { get; }

    public DeviceGrayColor(T value)
    {
        Value = T.Clamp(value, T.Zero, T.One);
    }

    public (ushort r, ushort g, ushort b) GetRBG()
    {
        throw new NotImplementedException();
    }

    public void Set(ContentWriter<T> writer)
    {
        writer.SetFillGray(Value);
    }

    public void SetStroking(ContentWriter<T> writer)
    {
        writer.SetStrokingGray(Value);
    }
}
