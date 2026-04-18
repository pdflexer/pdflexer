namespace PdfLexer.TextLayout;

public sealed record TextBoxFitResult(
    TextBoxLayoutResult FittingLayout,
    TextBoxLayoutRequest? RemainderRequest,
    double ConsumedHeight,
    double ConsumedWidth,
    TextBreakKind BreakKind,
    bool HasRemainder)
{
    public TextFragmentBreak FragmentBreak { get; init; } = TextFragmentBreak.None;
    public TextBoxLayoutRequest? FittedRequest { get; init; }

    public TextBoxLayoutResult FittedLayout => FittingLayout;

    public TextBoxLayoutRequest? RemainderContent => RemainderRequest;

    public TextLayoutSize ConsumedSize => new(ConsumedWidth, ConsumedHeight);

    public TextFragmentResult<TextBoxLayoutRequest> Fragment
        => new(
            FittingLayout,
            FittedRequest ?? throw new InvalidOperationException("This fit result does not retain its fitted request."),
            RemainderRequest,
            FragmentBreak,
            HasRemainder);
}

public sealed record TextSegmentStyle(
    string FamilyName,
    int Weight,
    double FontSize,
    bool Underline = false,
    double CharacterSpacing = 0,
    double WordSpacing = 0,
    double? LineSpacing = null,
    TextLineBoxSizing LineBoxSizing = TextLineBoxSizing.AtLeastLineHeight,
    bool Italic = false,
    TextColor? ForegroundColor = null,
    TextColor? BackgroundColor = null,
    bool StrikeThrough = false);

public sealed record TextSegment(
    string Text,
    TextSegmentStyle Style,
    int? SourceStart = null,
    int? SourceLength = null,
    string? SourcePath = null,
    string? SourceNodeId = null);

public sealed record TextBoxLayoutRequest(
    double Width,
    double Height,
    TextFontLibrary FontLibrary,
    IReadOnlyList<TextSegment> Segments)
{
    public TextHorizontalAlignment HorizontalAlignment { get; init; } = TextHorizontalAlignment.Left;
    public TextVerticalAlignment VerticalAlignment { get; init; } = TextVerticalAlignment.Top;
    public TextOverflowMode OverflowMode { get; init; } = TextOverflowMode.Clip;
    public TextResolutionBehavior MissingFontBehavior { get; init; } = TextResolutionBehavior.Error;
    public TextResolutionBehavior MissingGlyphBehavior { get; init; } = TextResolutionBehavior.Error;
    public IReadOnlyList<string> FallbackFamilyNames { get; init; } = Array.Empty<string>();
    public bool PreserveTrailingWhitespaceInWidth { get; init; }
    public TextBoxStyle BoxStyle { get; init; } = new();
    public TextFontMetricSource MetricPreference { get; init; } = TextFontMetricSource.None;
    public TextFlowBreakBefore BreakBefore { get; init; } = TextFlowBreakBefore.Auto;
    public TextFlowBreakAfter BreakAfter { get; init; } = TextFlowBreakAfter.Auto;
    public TextFlowBreakInside BreakInside { get; init; } = TextFlowBreakInside.Auto;
}

public sealed record TextLayoutIssue(
    TextLayoutIssueKind Kind,
    string Message,
    int? SegmentIndex = null,
    string? FamilyName = null,
    int? Weight = null,
    string? FaceId = null);

public sealed record TextLayoutGlyph(
    uint GlyphId,
    uint Cluster,
    double X,
    double Y,
    double Advance,
    double OffsetX,
    double OffsetY);

public sealed record TextLayoutRun(
    int SegmentIndex,
    string Text,
    string FaceId,
    string FamilyName,
    int Weight,
    double FontSize,
    bool Italic,
    bool Underline,
    double CharacterSpacing,
    double WordSpacing,
    TextColor? ForegroundColor,
    TextColor? BackgroundColor,
    double X,
    double BaselineY,
    double Width,
    double MeasuredWidth,
    double LineHeight,
    IReadOnlyList<TextLayoutGlyph> Glyphs,
    bool DrawAsVectorBullet = false,
    bool StrikeThrough = false,
    int? SourceStart = null,
    int? SourceLength = null,
    string? SourcePath = null,
    string? SourceNodeId = null,
    double? RequestedLineHeight = null);

public static class TextLayoutBuiltInFaces
{
    public const string UnorderedListMarkerFaceId = "__pdflexer_builtin_unordered_list_marker";
}

public abstract record TextLayoutDecoration;

public sealed record TextLayoutFillRectDecoration(
    double X,
    double Y,
    double Width,
    double Height,
    TextColor Color,
    double Radius = 0,
    string? SourcePath = null) : TextLayoutDecoration;

public sealed record TextLayoutStrokeRectDecoration(
    double X,
    double Y,
    double Width,
    double Height,
    TextColor Color,
    double Thickness,
    double Radius = 0,
    string? SourcePath = null) : TextLayoutDecoration;

public sealed record TextLayoutLineDecoration(
    double X1,
    double Y1,
    double X2,
    double Y2,
    TextColor Color,
    double Thickness,
    string? SourcePath = null) : TextLayoutDecoration;

public sealed record TextLayoutLine(
    int Index,
    double X,
    double BaselineY,
    double Width,
    double MeasuredWidth,
    double Height,
    double BaselineOffset,
    IReadOnlyList<TextLayoutRun> Runs);

public readonly record struct TextLayoutRunDescriptor(
    int SegmentIndex,
    int SourceTextStart,
    int SourceLength,
    string FaceId,
    string FamilyName,
    int Weight,
    double FontSize,
    bool Italic,
    bool Underline,
    double CharacterSpacing,
    double WordSpacing,
    TextColor? ForegroundColor,
    TextColor? BackgroundColor,
    double X,
    double BaselineY,
    double Width,
    double MeasuredWidth,
    double LineHeight,
    bool DrawAsVectorBullet = false,
    bool StrikeThrough = false,
    int? SourceStart = null,
    string? SourcePath = null,
    string? SourceNodeId = null,
    double? RequestedLineHeight = null,
    int GlyphStart = 0,
    int GlyphCount = 0);

public readonly record struct TextLayoutLineDescriptor(
    int Index,
    double X,
    double BaselineY,
    double Width,
    double MeasuredWidth,
    double Height,
    double BaselineOffset,
    int RunStart,
    int RunCount);

public readonly struct TextLayoutGlyphCollection : IReadOnlyList<TextLayoutGlyph>
{
    private readonly IReadOnlyList<TextLayoutGlyph>? _materialized;
    private readonly TextLayoutGlyph[]? _buffer;
    private readonly int _start;

    internal TextLayoutGlyphCollection(IReadOnlyList<TextLayoutGlyph> materialized)
    {
        _materialized = materialized;
        _buffer = null;
        _start = 0;
        Count = materialized.Count;
    }

    internal TextLayoutGlyphCollection(TextLayoutGlyph[] buffer, int start, int count)
    {
        _materialized = null;
        _buffer = buffer;
        _start = start;
        Count = count;
    }

    public int Count { get; }

    public TextLayoutGlyph this[int index]
        => _materialized is not null ? _materialized[index] : _buffer![_start + index];

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<TextLayoutGlyph> IEnumerable<TextLayoutGlyph>.GetEnumerator()
        => GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();

    public struct Enumerator : IEnumerator<TextLayoutGlyph>
    {
        private readonly TextLayoutGlyphCollection _collection;
        private int _index;

        internal Enumerator(TextLayoutGlyphCollection collection)
        {
            _collection = collection;
            _index = -1;
        }

        public TextLayoutGlyph Current => _collection[_index];
        object System.Collections.IEnumerator.Current => Current;
        public bool MoveNext() => ++_index < _collection.Count;
        public void Reset() => _index = -1;
        public void Dispose() { }
    }
}

public readonly struct TextLayoutRunView
{
    private readonly TextBoxLayoutResult _owner;
    private readonly TextLayoutRunDescriptor _descriptor;
    private readonly TextLayoutRun? _legacy;

    internal TextLayoutRunView(TextBoxLayoutResult owner, TextLayoutRunDescriptor descriptor)
    {
        _owner = owner;
        _descriptor = descriptor;
        _legacy = null;
    }

    internal TextLayoutRunView(TextBoxLayoutResult owner, TextLayoutRun legacy)
    {
        _owner = owner;
        _descriptor = default;
        _legacy = legacy;
    }

    public int SegmentIndex => _legacy?.SegmentIndex ?? _descriptor.SegmentIndex;
    public string FaceId => _legacy?.FaceId ?? _descriptor.FaceId;
    public string FamilyName => _legacy?.FamilyName ?? _descriptor.FamilyName;
    public int Weight => _legacy?.Weight ?? _descriptor.Weight;
    public double FontSize => _legacy?.FontSize ?? _descriptor.FontSize;
    public bool Italic => _legacy?.Italic ?? _descriptor.Italic;
    public bool Underline => _legacy?.Underline ?? _descriptor.Underline;
    public double CharacterSpacing => _legacy?.CharacterSpacing ?? _descriptor.CharacterSpacing;
    public double WordSpacing => _legacy?.WordSpacing ?? _descriptor.WordSpacing;
    public TextColor? ForegroundColor => _legacy?.ForegroundColor ?? _descriptor.ForegroundColor;
    public TextColor? BackgroundColor => _legacy?.BackgroundColor ?? _descriptor.BackgroundColor;
    public double X => _legacy?.X ?? _descriptor.X;
    public double BaselineY => _legacy?.BaselineY ?? _descriptor.BaselineY;
    public double Width => _legacy?.Width ?? _descriptor.Width;
    public double MeasuredWidth => _legacy?.MeasuredWidth ?? _descriptor.MeasuredWidth;
    public double LineHeight => _legacy?.LineHeight ?? _descriptor.LineHeight;
    public bool DrawAsVectorBullet => _legacy?.DrawAsVectorBullet ?? _descriptor.DrawAsVectorBullet;
    public bool StrikeThrough => _legacy?.StrikeThrough ?? _descriptor.StrikeThrough;
    public int? SourceStart => _legacy?.SourceStart ?? _descriptor.SourceStart;
    public int? SourceLength => _legacy?.SourceLength ?? _descriptor.SourceLength;
    public string? SourcePath => _legacy?.SourcePath ?? _descriptor.SourcePath;
    public string? SourceNodeId => _legacy?.SourceNodeId ?? _descriptor.SourceNodeId;
    public double? RequestedLineHeight => _legacy?.RequestedLineHeight ?? _descriptor.RequestedLineHeight;

    public ReadOnlySpan<char> TextSpan
        => _legacy is not null
            ? _legacy.Text.AsSpan()
            : _owner.GetRunTextSpan(_descriptor);

    public string Text
        => _legacy?.Text ?? _owner.GetRunText(_descriptor);

    public TextLayoutGlyphCollection Glyphs
        => _legacy is not null
            ? new TextLayoutGlyphCollection(_legacy.Glyphs)
            : _owner.GetRunGlyphs(_descriptor);
}

public readonly struct TextLayoutRunViewCollection : IReadOnlyList<TextLayoutRunView>
{
    private readonly TextBoxLayoutResult _owner;
    private readonly int _start;
    private readonly IReadOnlyList<TextLayoutRun>? _legacyRuns;

    internal TextLayoutRunViewCollection(TextBoxLayoutResult owner, int start, int count)
    {
        _owner = owner;
        _start = start;
        _legacyRuns = null;
        Count = count;
    }

    internal TextLayoutRunViewCollection(TextBoxLayoutResult owner, IReadOnlyList<TextLayoutRun> legacyRuns)
    {
        _owner = owner;
        _start = 0;
        _legacyRuns = legacyRuns;
        Count = legacyRuns.Count;
    }

    public int Count { get; }

    public TextLayoutRunView this[int index]
        => _legacyRuns is not null
            ? new TextLayoutRunView(_owner, _legacyRuns[index])
            : new TextLayoutRunView(_owner, _owner.GetRunDescriptor(_start + index));

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<TextLayoutRunView> IEnumerable<TextLayoutRunView>.GetEnumerator()
        => GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();

    public struct Enumerator : IEnumerator<TextLayoutRunView>
    {
        private readonly TextLayoutRunViewCollection _collection;
        private int _index;

        internal Enumerator(TextLayoutRunViewCollection collection)
        {
            _collection = collection;
            _index = -1;
        }

        public TextLayoutRunView Current => _collection[_index];
        object System.Collections.IEnumerator.Current => Current;
        public bool MoveNext() => ++_index < _collection.Count;
        public void Reset() => _index = -1;
        public void Dispose() { }
    }
}

public readonly struct TextLayoutLineView
{
    private readonly TextBoxLayoutResult _owner;
    private readonly TextLayoutLineDescriptor _descriptor;
    private readonly TextLayoutLine? _legacy;

    internal TextLayoutLineView(TextBoxLayoutResult owner, TextLayoutLineDescriptor descriptor)
    {
        _owner = owner;
        _descriptor = descriptor;
        _legacy = null;
    }

    internal TextLayoutLineView(TextBoxLayoutResult owner, TextLayoutLine legacy)
    {
        _owner = owner;
        _descriptor = default;
        _legacy = legacy;
    }

    public int Index => _legacy?.Index ?? _descriptor.Index;
    public double X => _legacy?.X ?? _descriptor.X;
    public double BaselineY => _legacy?.BaselineY ?? _descriptor.BaselineY;
    public double Width => _legacy?.Width ?? _descriptor.Width;
    public double MeasuredWidth => _legacy?.MeasuredWidth ?? _descriptor.MeasuredWidth;
    public double Height => _legacy?.Height ?? _descriptor.Height;
    public double BaselineOffset => _legacy?.BaselineOffset ?? _descriptor.BaselineOffset;
    public TextLayoutRunViewCollection Runs
        => _legacy is not null
            ? new TextLayoutRunViewCollection(_owner, _legacy.Runs)
            : new TextLayoutRunViewCollection(_owner, _descriptor.RunStart, _descriptor.RunCount);
}

public readonly struct TextLayoutLineViewCollection : IReadOnlyList<TextLayoutLineView>
{
    private readonly TextBoxLayoutResult _owner;

    internal TextLayoutLineViewCollection(TextBoxLayoutResult owner)
    {
        _owner = owner;
    }

    public int Count => _owner.GetLineCount();

    public TextLayoutLineView this[int index] => _owner.GetLineView(index);

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<TextLayoutLineView> IEnumerable<TextLayoutLineView>.GetEnumerator()
        => GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();

    public struct Enumerator : IEnumerator<TextLayoutLineView>
    {
        private readonly TextLayoutLineViewCollection _collection;
        private int _index;

        internal Enumerator(TextLayoutLineViewCollection collection)
        {
            _collection = collection;
            _index = -1;
        }

        public TextLayoutLineView Current => _collection[_index];
        object System.Collections.IEnumerator.Current => Current;
        public bool MoveNext() => ++_index < _collection.Count;
        public void Reset() => _index = -1;
        public void Dispose() { }
    }
}

public sealed class TextBoxLayoutResult
{
    private IReadOnlyList<TextLayoutLine>? _lines;

    internal TextSegment[]? Segments { get; init; }
    internal TextLayoutGlyph[]? GlyphBuffer { get; init; }
    internal TextLayoutRunDescriptor[]? RunBuffer { get; init; }
    internal TextLayoutLineDescriptor[]? LineBuffer { get; init; }

    public required TextLayoutStatus Status { get; init; }
    public required bool FitsWidth { get; init; }
    public required bool FitsHeight { get; init; }
    public required double MeasuredWidth { get; init; }
    public required double MeasuredHeight { get; init; }
    public required double RenderedWidth { get; init; }
    public required double RenderedHeight { get; init; }
    public required TextBoxStyle BoxStyle { get; init; }
    public IReadOnlyList<TextLayoutLine> Lines
    {
        get
        {
            if (_lines is null)
            {
                _lines = LineBuffer is not null
                    ? new DeferredTextLayoutLineList(this)
                    : Array.Empty<TextLayoutLine>();
            }

            return _lines;
        }
        init => _lines = value;
    }
    public TextLayoutLineViewCollection LineViews => new(this);
    public int LineCount => GetLineCount();
    public required IReadOnlyList<TextLayoutIssue> Issues { get; init; }
    public IReadOnlyList<TextLayoutDecoration> Decorations { get; init; } = Array.Empty<TextLayoutDecoration>();

    public bool Success => Status == TextLayoutStatus.Success;

    public double NaturalWidth => MeasuredWidth;

    public double NaturalHeight => MeasuredHeight;

    public double VisibleWidth => RenderedWidth;

    public double VisibleHeight => RenderedHeight;

    public TextLayoutSize NaturalSize => new(NaturalWidth, NaturalHeight);

    public TextLayoutSize VisibleSize => new(VisibleWidth, VisibleHeight);

    public TextLayoutLineView GetLine(int index) => GetLineView(index);

    public IReadOnlyList<TextLayoutLine> MaterializeLegacyLines()
    {
        var lines = new TextLayoutLine[GetLineCount()];
        for (var i = 0; i < lines.Length; i++)
        {
            lines[i] = MaterializeLine(i);
        }

        return lines;
    }

    public TextBoxLayoutResult MaterializeLegacyLayout()
        => new()
        {
            Status = Status,
            FitsWidth = FitsWidth,
            FitsHeight = FitsHeight,
            MeasuredWidth = MeasuredWidth,
            MeasuredHeight = MeasuredHeight,
            RenderedWidth = RenderedWidth,
            RenderedHeight = RenderedHeight,
            BoxStyle = BoxStyle,
            Lines = MaterializeLegacyLines(),
            Issues = Issues,
            Decorations = Decorations
        };

    internal int GetLineCount() => LineBuffer?.Length ?? Lines.Count;

    internal TextLayoutLineView GetLineView(int index)
        => LineBuffer is not null
            ? new TextLayoutLineView(this, LineBuffer[index])
            : new TextLayoutLineView(this, Lines[index]);

    internal TextLayoutRunDescriptor GetRunDescriptor(int index)
        => RunBuffer![index];

    internal ReadOnlySpan<char> GetRunTextSpan(TextLayoutRunDescriptor descriptor)
        => Segments![descriptor.SegmentIndex].Text.AsSpan(descriptor.SourceTextStart, descriptor.SourceLength);

    internal string GetRunText(TextLayoutRunDescriptor descriptor)
        => Segments![descriptor.SegmentIndex].Text.Substring(descriptor.SourceTextStart, descriptor.SourceLength);

    internal TextLayoutGlyphCollection GetRunGlyphs(TextLayoutRunDescriptor descriptor)
        => descriptor.GlyphCount == 0
            ? new TextLayoutGlyphCollection(Array.Empty<TextLayoutGlyph>())
            : new TextLayoutGlyphCollection(GlyphBuffer!, descriptor.GlyphStart, descriptor.GlyphCount);

    internal TextLayoutLine MaterializeLine(int index)
    {
        if (LineBuffer is null || RunBuffer is null || Segments is null || GlyphBuffer is null)
        {
            return ((IReadOnlyList<TextLayoutLine>)Lines)[index];
        }

        var line = LineBuffer[index];
        var runs = new TextLayoutRun[line.RunCount];
        for (var runIndex = 0; runIndex < line.RunCount; runIndex++)
        {
            var run = RunBuffer[line.RunStart + runIndex];
            runs[runIndex] = new TextLayoutRun(
                run.SegmentIndex,
                Segments[run.SegmentIndex].Text.Substring(run.SourceTextStart, run.SourceLength),
                run.FaceId,
                run.FamilyName,
                run.Weight,
                run.FontSize,
                run.Italic,
                run.Underline,
                run.CharacterSpacing,
                run.WordSpacing,
                run.ForegroundColor,
                run.BackgroundColor,
                run.X,
                run.BaselineY,
                run.Width,
                run.MeasuredWidth,
                run.LineHeight,
                run.GlyphCount == 0 ? Array.Empty<TextLayoutGlyph>() : new TextLayoutGlyphCollection(GlyphBuffer, run.GlyphStart, run.GlyphCount),
                run.DrawAsVectorBullet,
                run.StrikeThrough,
                run.SourceStart,
                run.SourceLength,
                run.SourcePath,
                run.SourceNodeId,
                run.RequestedLineHeight);
        }

        return new TextLayoutLine(
            line.Index,
            line.X,
            line.BaselineY,
            line.Width,
            line.MeasuredWidth,
            line.Height,
            line.BaselineOffset,
            runs);
    }

    private sealed class DeferredTextLayoutLineList : IReadOnlyList<TextLayoutLine>
    {
        private readonly TextBoxLayoutResult _owner;

        public DeferredTextLayoutLineList(TextBoxLayoutResult owner)
        {
            _owner = owner;
        }

        public int Count => _owner.LineBuffer?.Length ?? 0;

        public TextLayoutLine this[int index] => _owner.MaterializeLine(index);

        public IEnumerator<TextLayoutLine> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return _owner.MaterializeLine(i);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}

public enum TextBreakKind
{
    None,
    Line,
    Paragraph,
    ListItem,
    TableRow,
    ContainerChild
}
