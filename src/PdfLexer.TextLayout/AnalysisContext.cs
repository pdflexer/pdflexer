namespace PdfLexer.TextLayout;

public sealed class TextLayoutAnalysisContext
{
    internal IntrinsicMeasurementCache IntrinsicMeasurements { get; } = new();
    internal bool CaptureLineDiagnostics { get; private set; }
    internal bool CaptureDetailedLineDiagnostics { get; private set; }

    public int CachedIntrinsicMeasurementCount => IntrinsicMeasurements.Count;

    public int CacheHitCount => IntrinsicMeasurements.HitCount;

    public int CacheMissCount => IntrinsicMeasurements.MissCount;

    public TextLayoutAnalysisContext EnableLineDiagnostics(bool detailed = false)
    {
        CaptureLineDiagnostics = true;
        CaptureDetailedLineDiagnostics = detailed;
        return this;
    }

    public void Clear()
        => IntrinsicMeasurements.Clear();
}

internal enum IntrinsicMeasurementUnitKind
{
    Paragraph,
    TableCellSubtree,
    LayoutChildSubtree,
    RichBlockSubtree
}

internal readonly record struct IntrinsicMeasurementCacheKey(
    IntrinsicMeasurementUnitKind UnitKind,
    string NodeId,
    int ContentVersion,
    int StyleVersion,
    double Width,
    int LayoutModeFlags);

internal sealed class IntrinsicMeasurementCacheEntry
{
    public required TextLayoutSize NaturalSize { get; init; }

    public required TextLayoutSize VisibleSize { get; init; }

    public object? Value { get; init; }
}

internal sealed class IntrinsicMeasurementCache
{
    private readonly Dictionary<IntrinsicMeasurementCacheKey, IntrinsicMeasurementCacheEntry> _entries = new();

    public int Count => _entries.Count;

    public int HitCount { get; private set; }

    public int MissCount { get; private set; }

    public bool TryGet(in IntrinsicMeasurementCacheKey key, out IntrinsicMeasurementCacheEntry? entry)
    {
        if (_entries.TryGetValue(key, out entry))
        {
            HitCount++;
            return true;
        }

        MissCount++;
        entry = null;
        return false;
    }

    public void Set(in IntrinsicMeasurementCacheKey key, IntrinsicMeasurementCacheEntry entry)
        => _entries[key] = entry;

    public void Clear()
    {
        _entries.Clear();
        HitCount = 0;
        MissCount = 0;
    }
}
