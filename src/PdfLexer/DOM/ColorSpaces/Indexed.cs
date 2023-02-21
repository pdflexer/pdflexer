namespace PdfLexer.DOM.ColorSpaces;

internal class Indexed : IColorSpace
{
    private readonly IColorSpace _baseCs;
    private readonly byte[] _lookup;
    // private readonly int _hival;
    private readonly byte[] buffer8 = new byte[4];
    public int Components => 1;
    public PdfName Name => PdfName.Indexed;

    public Indexed(IColorSpace baseCs, int hival, byte[] lookup)
    {
        _baseCs = baseCs;
        _lookup = lookup;
        // _hival = hival;
    }

    public void CopyRowToRBGA8Span(ReadOnlySpan<byte> row, Span<byte> output)
    {
        var c = _baseCs.Components;
        Span<byte> buffer = buffer8;
        buffer = buffer.Slice(0, c);

        for (var p = 0; p < row.Length; p++)
        {
            var v = row[p];
            var oos = p * 4;
            for (var i = 0; i < c; i++)
            {
                buffer[i] = _lookup[v * c + i];
            }
            _baseCs.CopyRowToRBGA8Span(buffer, output.Slice(oos));
        }
    }

    public void CopyRowToRBGA16Span(ReadOnlySpan<ushort> row, Span<ushort> output)
    {
        throw new NotSupportedException("Indexed colorspace does not support 16 bit.");
    }

    public static Indexed FromArray(ParsingContext ctx, PdfArray arr)
    {
        if (arr.Count < 4) { throw new PdfLexerException($"Indexed colorspace had less than 4 entries."); }
        var baseCs = ColorSpace.Get(ctx, arr[1]);
        var hival = arr[2].GetValue<PdfNumber>();
        var data = arr[3].Resolve();
        byte[]? lookup;
        if (data.Type == PdfObjectType.StringObj)
        {
            var str = (PdfString)data;
            lookup = str.GetRawBytes();
        }
        else if (data.Type == PdfObjectType.StreamObj)
        {
            lookup = ((PdfStream)data).Contents.GetDecodedData();
        }
        else
        {
            throw new ApplicationException("Index colorspace had unknown lookup table: " + data.GetPdfObjType());
        }

        return new Indexed(baseCs, hival, lookup);
    }
}
