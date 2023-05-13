namespace PdfLexer.DOM.ColorSpaces;

internal class CalGray : IColorSpace
{
    public int Components => 1;
    private float XW;
    private float YW;
    private float ZW;
    private float XB;
    private float YB;
    private float ZB;
    private float G;

    public PdfName Name => PdfName.CalGray;

    public bool IsPredefined => false;

    public CalGray(float xw, float yw, float zw, float xb, float yb, float zb, float gamma)
    {
        XW = xw;
        YW = yw;
        ZW = zw;
        XB = xb;
        YB = yb;
        ZB = zb;
        G = gamma;
    }

    private double Get8BitVal(double A)
    {
        var AG = Math.Pow(A, G);
        // Computes L as per spec. ( = cs.YW * AG )
        // Except if other than default BlackPoint values are used.
        var L = YW * AG;
        // http://www.poynton.com/notes/colour_and_gamma/ColorFAQ.html, Ch 4.
        // Convert values to rgb range [0, 255].
        return Math.Max(295.8 * Math.Pow(L, 0.3333333333333333) - 40.8, 0);
    }

    public void CopyRowToRBGA16Span(ReadOnlySpan<ushort> row, Span<ushort> output)
    {
        for (var p = 0; p < row.Length; p++)
        {
            var oos = p * 4;
            var v = (ushort)(Get8BitVal(row[p] / 65535.0)*257);
            output[oos] = v;
            output[oos + 1] = v;
            output[oos + 2] = v;
            output[oos + 3] = ushort.MaxValue;
        }
    }

    public void CopyRowToRBGA8Span(ReadOnlySpan<byte> row, Span<byte> output)
    {
        for (var p = 0; p < row.Length; p++)
        {
            var oos = p * 4;
            var v = (byte)Get8BitVal(row[p] / 255.0);
            output[oos] = v;
            output[oos + 1] = v;
            output[oos + 2] = v;
            output[oos + 3] = 255;
        }
    }

    public static CalGray FromObject(PdfDictionary dict)
    {
        var wp = dict.Get<PdfArray>("WhitePoint");
        if (wp == null)
        {
            throw new PdfLexerException("No whitepoint entry for calgray colorspace");
        }
        var wps = wp.Select(x => x.GetValueOrNull<PdfNumber>()).Where(x => x != null).Select(x => (float)x!).ToList();
        if (wps.Count != 3)
        {
            throw new PdfLexerException($"Invalid WhitePoint entry for CalGray:" + wp.ToString());
        }
        var (xw, yw, zw) = (wps[0], wps[1], wps[2]);
        if (xw < 0 || zw < 0 || yw != 1)
        {
            throw new PdfLexerException($"Invalid WhitePoint entry for CalGray:" + wp.ToString());
        }

        var (xb, yb, zb) = (0f, 0f, 0f);
        var bp = dict.Get<PdfArray>("BlackPoint");
        if (bp != null)
        {
            var bps = bp.Select(x => x.GetValueOrNull<PdfNumber>()).Where(x => x != null).Select(x => (float)x!).ToList();
            if (wps.Count == 3)
            {
                (xb, yb, zb) = (bps[0], bps[1], bps[2]);
            }
            if (xb < 0 || yb < 0 || zb < 0)
            {
                (xb, yb, zb) = (0f, 0f, 0f);
            }
        }

        var gamma = 1f;
        
        var g = dict.Get<PdfNumber>("Gamma");
        if (g != null && (double)g > 1)
        {
            gamma = g;
        }

        return new CalGray(xw, yw, zw, xb, yb, zb, gamma);
    }
}
