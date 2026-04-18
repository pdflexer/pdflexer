using System.Runtime.CompilerServices;

using HarfRust;

namespace PdfLexer.TextLayout;

public sealed class TextLayoutAnalysisContext : IDisposable
{
    private readonly Dictionary<TextFontLibrary, TextLayoutFontCache> _fontCaches = new(ReferenceIdentityComparer<TextFontLibrary>.Instance);
    private bool _disposed;

    internal IntrinsicMeasurementCache IntrinsicMeasurements { get; } = new();
    internal bool CaptureLineDiagnostics { get; private set; }
    internal bool CaptureDetailedLineDiagnostics { get; private set; }
    internal int CachedFontLibraryCount => _fontCaches.Count;

    public int CachedIntrinsicMeasurementCount => IntrinsicMeasurements.Count;

    public int CacheHitCount => IntrinsicMeasurements.HitCount;

    public int CacheMissCount => IntrinsicMeasurements.MissCount;

    public TextLayoutAnalysisContext EnableLineDiagnostics(bool detailed = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        CaptureLineDiagnostics = true;
        CaptureDetailedLineDiagnostics = detailed;
        return this;
    }

    public void ClearMeasurements()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        IntrinsicMeasurements.Clear();
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        IntrinsicMeasurements.Clear();
        ClearFontCaches();
    }

    internal TextLayoutFontCache GetFontCache(TextFontLibrary library)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(library);

        if (!_fontCaches.TryGetValue(library, out var cache))
        {
            cache = new TextLayoutFontCache(library);
            _fontCaches[library] = cache;
        }

        return cache;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~TextLayoutAnalysisContext()
        => Dispose(disposing: false);

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        IntrinsicMeasurements.Clear();
        ClearFontCaches();
        _disposed = true;
    }

    private void ClearFontCaches()
    {
        foreach (var cache in _fontCaches.Values)
        {
            cache.Dispose();
        }

        _fontCaches.Clear();
    }
}

internal sealed class TextLayoutFontCache : IDisposable
{
    private readonly Dictionary<string, HarfRustFont> _fonts = new(StringComparer.Ordinal);
    private readonly HarfRustShapeSession _session;

    public TextLayoutFontCache(TextFontLibrary library)
    {
        Library = library;
        _session = new HarfRustShapeSession(HarfRustBackend.Current);
    }

    public TextFontLibrary Library { get; }
    public HarfRustShapeSession Session => _session;

    public HarfRustFont Get(TextFontFace face)
    {
        if (!_fonts.TryGetValue(face.FaceId, out var font))
        {
            font = new HarfRustFont(face.FontData, face.FaceIndex, HarfRustBackend.Current);
            _fonts[face.FaceId] = font;
        }

        return font;
    }

    public void Dispose()
    {
        _session.Dispose();
        foreach (var font in _fonts.Values)
        {
            font.Dispose();
        }

        _fonts.Clear();
    }
}

internal sealed class ReferenceIdentityComparer<T> : IEqualityComparer<T>
    where T : class
{
    public static ReferenceIdentityComparer<T> Instance { get; } = new();

    public bool Equals(T? x, T? y)
        => ReferenceEquals(x, y);

    public int GetHashCode(T obj)
        => RuntimeHelpers.GetHashCode(obj);
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
    {
        if (_entries.TryGetValue(key, out var existing))
        {
            DisposeEntry(existing);
        }

        _entries[key] = entry;
    }

    public void Clear()
    {
        foreach (var entry in _entries.Values)
        {
            DisposeEntry(entry);
        }

        _entries.Clear();
        HitCount = 0;
        MissCount = 0;
    }

    private static void DisposeEntry(IntrinsicMeasurementCacheEntry entry)
    {
        if (entry.Value is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
