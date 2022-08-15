using System.Diagnostics.CodeAnalysis;

namespace PdfLexer.Parsers;

internal class NumberCache
{
    private Dictionary<ulong, PdfNumber> Cache = new Dictionary<ulong, PdfNumber>();
    public NumberCache()
    {

    }

    public bool TryGetNumber(ReadOnlySpan<byte> data, out ulong key, [NotNullWhen(true)]out PdfNumber value)
    {
        key = 0;
        if (data.Length > 8)
        {
            value = null!;
            return false;
        }

        for (int i = 0; i < data.Length; i++)
        {
            key = key | ((ulong)data[i] << 8 * (i - 1));
        }

        if (Cache.TryGetValue(key, out var item))
        {
            value = item;
            return true;
        }
        value = null!;
        return false;
    }
    public void AddValue(ulong key, PdfNumber value)
    {
        Cache[key] = value;
    }
}
