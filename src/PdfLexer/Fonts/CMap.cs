using System.Runtime.CompilerServices;

namespace PdfLexer.Fonts;

public class CMapData
{
    public CMapData(List<CRange> ranges, Dictionary<uint, CResult> mapping, bool vertical)
    {
        Ranges = ranges;
        Mapping = mapping;
        Vertical = vertical;
    }
    public List<CRange> Ranges { get; set; }
    public Dictionary<uint, CResult> Mapping { get; set; }
    public bool Vertical { get; set; }
}

public struct CResult
{
    public uint Code;
    public string MultiChar;
}

public struct CRange
{
    public uint Start;
    public uint End;
    public int Bytes;

    public static int EstimateByteSize(List<CRange> ranges, uint cid)
    {
        var bytes = ranges.Select(x => x.Bytes).ToHashSet();
        foreach (var rng in ranges)
        {
            if (cid >= rng.Start && cid <= rng.End)
            {
                return rng.Bytes;
            }
        }

        if (bytes.Contains(1) && cid < 256)
        {
            return 1;
        }
        else if (bytes.Contains(2) && cid <= 0xFFFF)
        {
            return 2;
        }
        else if (bytes.Contains(3) && cid <= 0xFFFFFF)
        {
            return 3;
        }
        else if (bytes.Contains(4))
        {
            return 4;
        }
        else
        {
            return cid < 256 ? 1 : 2; // fallback
        }
    }
}


internal class CMap
{
    private List<CRange>[] rangeSets = new List<CRange>[4];
    private Dictionary<uint, CResult>? mapping;
    internal List<CRange> Ranges;

    public CMap(List<CRange> ranges, Dictionary<uint, CResult>? mapping = null)
    {
        foreach (var range in ranges)
        {
            var b = range.Bytes - 1;
            if (rangeSets[b] == null)
            {
                rangeSets[b] = new List<CRange>();
            }
            rangeSets[b].Add(range);
        }
        this.mapping = mapping;
        Ranges = ranges;
    }

    public bool HasMapping { get => mapping != null; }


    public bool TryGetMapping([NotNullWhen(true)]out Dictionary<uint, CResult>? val)
    {
        val = null;
        if (mapping == null) { return false; }
        val = mapping;
        return true;
    }

    public bool TryGetFallback(uint cp, out CResult val)
    {
        if (mapping == null) { val = default; return false; }
        return mapping.TryGetValue(cp, out val);
    }

    public uint GetCID(uint c)
    {
        if (mapping != null && mapping.TryGetValue(c, out var cr))
        {
            c = cr.Code;
        }
        return c;
    }

    public uint GetCodePoint(ReadOnlySpan<byte> data, int os, out int l)
    {
        uint c = 0;
        var imax = 4;
        var cl = data.Length - os;
        if (cl < 4) { imax = cl; }
        for (var i = 0; i < imax; i++)
        {
            c = (uint)(c << 8) | data[os + i];
            var rangeSet = rangeSets[i];
            if (rangeSet == null) { continue; }
            foreach (var range in rangeSet)
            {
                if (range.Start <= c && c <= range.End)
                {
                    l = i + 1;
                    return c;
                }
            }
        }
        l = 0;
        return 0;
    }

}

public class GlobalCMapProvider : ICMapProvider
{
    private static Dictionary<string, ICMapProvider> providers = new();

    public static ICMapProvider Instance { get; } = new GlobalCMapProvider();
    public GlobalCMapProvider()
    {

    }
    public static void AddProvider(ICMapProvider provider)
    {
        lock(providers)
        {
            foreach (var cmap in provider.GetProvidedNames())
            {
                providers[cmap.ToLower()] = provider;
            }
        }
    }

    private static ConditionalWeakTable<string, CMapData>
        globalCache = new ConditionalWeakTable<string, CMapData>();

    public CMapData? GetCMapData(string name)
    {
        name = name.ToLower();
        lock (globalCache)
        {
            if (globalCache.TryGetValue(name, out var data))
            {
                return data;
            }
        }
        ICMapProvider? provider = null;
        lock (providers)
        {
            if (!providers.TryGetValue(name, out provider))
            {
                return null;
            }
        }
        var result =  provider.GetCMapData(name);
        if (result == null) { return null; }
        lock (globalCache)
        {
            globalCache.AddOrUpdate(name, result);
        }
        return result;
    }

    public IEnumerable<string> GetProvidedNames()
    {
        lock (providers)
        {
            return providers.Keys.ToList();
        }
    }
}

public interface ICMapProvider
{
    IEnumerable<string> GetProvidedNames();
    CMapData? GetCMapData(string name);
}