using System.Collections.ObjectModel;
using System.Numerics;
using PdfLexer.Content;
using PdfLexer.Content.Model;

namespace PdfLexer.Remediation;

/// <summary>
/// Structured-text/content candidate considered by remediation rules.
/// </summary>
public abstract record RemediationCandidate
{
    /// <summary>Candidate family.</summary>
    public abstract RemediationCandidateKind Kind { get; }

    /// <summary>Stable identity within the parsed page candidate index.</summary>
    public abstract string CandidateId { get; }

    /// <summary>Zero-based page index in the document candidate index.</summary>
    public int PageIndex { get; internal init; } = -1;
    protected RemediationCandidate(
        PdfRect<double> boundingBox,
        PdfRect<double> relativeBoundingBox,
        IReadOnlyList<StructuredSourceRef> sourceReferences,
        int sequenceIndex)
    {
        BoundingBox = boundingBox;
        RelativeBoundingBox = relativeBoundingBox;
        SourceReferences = sourceReferences;
        ContentOrderIndex = sequenceIndex;
    }

    /// <summary>Candidate bounds in page coordinates.</summary>
    public PdfRect<double> BoundingBox { get; }

    /// <summary>Candidate bounds in normalized page-relative coordinates.</summary>
    public PdfRect<double> RelativeBoundingBox { get; }

    /// <summary>Source references for content operators that compose the candidate.</summary>
    public IReadOnlyList<StructuredSourceRef> SourceReferences { get; }

    /// <summary>Unified page content-order index shared by text and non-text candidates.</summary>
    public int ContentOrderIndex { get; }

    internal virtual int SequenceIndex => ContentOrderIndex;

    /// <summary>Whether geometry predicates can safely use this candidate's bounds.</summary>
    public virtual bool HasUsableGeometry => true;

    /// <summary>Creates a candidate from a structured character.</summary>
    public static RemediationCandidate From(StructuredCharacter character) =>
        new TextRemediationCandidate(
            Granularity.Character,
            character.Char.ToString(),
            character.BoundingBox,
            character.RelativeBoundingBox,
            new ReadOnlyCollection<StructuredCharacter>(new[] { character }),
            new ReadOnlyCollection<StructuredSourceRef>(new[] { character.SourceReference }),
            character.SequenceIndex,
            character.FontSize);

    /// <summary>Creates a candidate from a structured word.</summary>
    public static RemediationCandidate From(StructuredWord word) =>
        new TextRemediationCandidate(
            Granularity.Word,
            word.Text,
            word.BoundingBox,
            word.RelativeBoundingBox,
            word.Characters,
            word.SourceReferences,
            word.SequenceIndex,
            word.FontSize);

    /// <summary>Creates a candidate from a structured line.</summary>
    public static RemediationCandidate From(StructuredLine line) =>
        new TextRemediationCandidate(
            Granularity.Line,
            line.Text,
            line.BoundingBox,
            line.RelativeBoundingBox,
            new ReadOnlyCollection<StructuredCharacter>(line.Words.SelectMany(x => x.Characters).ToList()),
            line.SourceReferences,
            line.SequenceIndex,
            line.FontSize);

    /// <summary>Creates a candidate from a structured paragraph.</summary>
    public static RemediationCandidate From(StructuredParagraph paragraph)
    {
        var words = paragraph.Lines.SelectMany(x => x.Words).ToList();
        var characters = words.SelectMany(x => x.Characters).ToList();
        return new TextRemediationCandidate(
            Granularity.Paragraph,
            paragraph.Text,
            paragraph.BoundingBox,
            paragraph.RelativeBoundingBox,
            new ReadOnlyCollection<StructuredCharacter>(characters),
            paragraph.SourceReferences,
            paragraph.SequenceIndex,
            characters.Count == 0 ? 0 : characters.Average(x => x.FontSize));
    }

    internal static RemediationCandidate CreateExactRange(
        RemediationCandidate template,
        RemediationTextRange range,
        IReadOnlyList<StructuredCharacter> characters)
    {
        var textTemplate = (TextRemediationCandidate)template;
        var bounds = characters.Count == 0 ? template.BoundingBox : Union(characters.Select(x => x.BoundingBox));
        var relativeBounds = characters.Count == 0
            ? template.RelativeBoundingBox
            : Union(characters.Select(x => x.RelativeBoundingBox));
        var text = characters.Count == 0
            ? range.Text
            : new string(characters.OrderBy(x => x.SourceCharacterIndex).Select(x => x.Char).ToArray());

        return new TextRemediationCandidate(
            textTemplate.Granularity,
            text,
            bounds,
            relativeBounds,
            new ReadOnlyCollection<StructuredCharacter>(characters.ToList()),
            new ReadOnlyCollection<StructuredSourceRef>(new[] { range.SourceReference }),
            characters.Count == 0 ? template.SequenceIndex : characters.Min(x => x.SequenceIndex),
            characters.Count == 0 ? textTemplate.FontSize : characters.Average(x => x.FontSize),
            textTemplate.FontName,
            textTemplate.FontWeight,
            textTemplate.Italic,
            textTemplate.IsGrayish,
            new ReadOnlyCollection<RemediationTextRange>(new[] { range }))
        {
            PageIndex = template.PageIndex
        };
    }

    private static PdfRect<double> Union(IEnumerable<PdfRect<double>> rects)
    {
        using var enumerator = rects.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return new PdfRect<double>(0, 0, 0, 0);
        }

        var result = enumerator.Current;
        while (enumerator.MoveNext())
        {
            var rect = enumerator.Current;
            result = new PdfRect<double>(
                Math.Min(result.LLx, rect.LLx),
                Math.Min(result.LLy, rect.LLy),
                Math.Max(result.URx, rect.URx),
                Math.Max(result.URy, rect.URy));
        }

        return result;
    }
}

/// <summary>A structured-text remediation candidate with exact source ranges and font properties.</summary>
public sealed record TextRemediationCandidate : RemediationCandidate
{
    public override RemediationCandidateKind Kind => RemediationCandidateKind.Text;
    internal TextRemediationCandidate(
        Granularity granularity,
        string text,
        PdfRect<double> boundingBox,
        PdfRect<double> relativeBoundingBox,
        IReadOnlyList<StructuredCharacter> characters,
        IReadOnlyList<StructuredSourceRef> sourceReferences,
        int sequenceIndex,
        double fontSize,
        string? fontName = null,
        int? fontWeight = null,
        bool? italic = null,
        bool? isGrayish = null,
        IReadOnlyList<RemediationTextRange>? exactTextRanges = null,
        int? contentOrderIndex = null)
        : base(boundingBox, relativeBoundingBox, sourceReferences, contentOrderIndex ?? sequenceIndex)
    {
        Granularity = granularity;
        Text = text;
        Characters = characters;
        FontSize = fontSize;
        FontName = fontName ?? GetCommon(characters.Select(x => x.FontName));
        FontWeight = fontWeight ?? GetCommon(characters.Select(x => x.FontWeight));
        Italic = italic ?? GetCommon(characters.Select(x => x.Italic));
        IsGrayish = isGrayish ?? GetCommon(characters.Select(x => x.IsGrayish));
        ExactTextRanges = exactTextRanges;
        SequenceIndex = sequenceIndex;
    }

    public override string CandidateId =>
        $"Text:{ContentOrderIndex}:{Granularity}:{SourceReferences.FirstOrDefault()}:{TextRanges.FirstOrDefault()?.StartCharacterIndex ?? 0}";
    public Granularity Granularity { get; }
    public string Text { get; }
    public IReadOnlyList<StructuredCharacter> Characters { get; }
    public IReadOnlyList<RemediationTextRange> TextRanges => ExactTextRanges ?? BuildTextRanges(Characters);
    internal IReadOnlyList<RemediationTextRange>? ExactTextRanges { get; }
    internal bool RequiresExactMaterialization =>
        Granularity is Granularity.Character or Granularity.Word || ExactTextRanges != null;
    internal override int SequenceIndex { get; }
    public double FontSize { get; }
    public string? FontName { get; }
    public int? FontWeight { get; }
    public bool? Italic { get; }
    public bool? IsGrayish { get; }

    internal TextRemediationCandidate WithContentOrderIndex(int contentOrderIndex) =>
        new(
            Granularity,
            Text,
            BoundingBox,
            RelativeBoundingBox,
            Characters,
            SourceReferences,
            SequenceIndex,
            FontSize,
            FontName,
            FontWeight,
            Italic,
            IsGrayish,
            ExactTextRanges,
            contentOrderIndex);

    private static T? GetCommon<T>(IEnumerable<T?> values)
    {
        var valuesWithData = values.Where(x => x != null).Distinct().Take(2).ToArray();
        return valuesWithData.Length == 1 ? valuesWithData[0] : default;
    }

    private static IReadOnlyList<RemediationTextRange> BuildTextRanges(IReadOnlyList<StructuredCharacter> characters)
    {
        var ranges = new List<RemediationTextRange>();
        foreach (var sourceGroup in characters.GroupBy(x => x.SourceReference)
                     .OrderBy(x => x.Min(y => y.SourceCharacterIndex)))
        {
            var ordered = sourceGroup.OrderBy(x => x.SourceCharacterIndex).ToList();
            var rangeStart = ordered[0].SourceCharacterIndex;
            var expected = rangeStart;
            var text = new List<char>();
            foreach (var character in ordered)
            {
                if (character.SourceCharacterIndex != expected)
                {
                    ranges.Add(new RemediationTextRange(sourceGroup.Key, rangeStart, text.Count, new string(text.ToArray())));
                    rangeStart = character.SourceCharacterIndex;
                    expected = rangeStart;
                    text.Clear();
                }
                text.Add(character.Char);
                expected++;
            }
            ranges.Add(new RemediationTextRange(sourceGroup.Key, rangeStart, text.Count, new string(text.ToArray())));
        }
        return new ReadOnlyCollection<RemediationTextRange>(ranges);
    }
}

/// <summary>An atomic non-text painting candidate from the parsed content model.</summary>
public sealed record ContentRemediationCandidate : RemediationCandidate
{
    internal ContentRemediationCandidate(
        RemediationCandidateKind kind,
        IContentItem<double> item,
        PdfRect<double> bounds,
        PdfRect<double> relativeBounds,
        int sequenceIndex,
        int resourceUseCount,
        string? resourceIdentity = null,
        string? resourceName = null)
        : base(bounds, relativeBounds,
            item.SourceReference is { } source ? new[] { source } : Array.Empty<StructuredSourceRef>(),
            sequenceIndex)
    {
        ContentKind = kind;
        Item = item;
        ParsedContentIdentity = item.ParsedItemId;
        ResourceIdentity = resourceIdentity;
        ResourceName = resourceName;
        ResourceUseCount = resourceUseCount;
    }

    public override RemediationCandidateKind Kind => ContentKind;
    public override string CandidateId =>
        $"{Kind}:{ContentOrderIndex}:{ParsedContentIdentity}:{SourceReferences.FirstOrDefault()}";
    public RemediationCandidateKind ContentKind { get; }
    public ParsedContentId? ParsedContentIdentity { get; }
    public string? ResourceIdentity { get; }
    public string? ResourceName { get; }
    public int ResourceUseCount { get; }
    internal IContentItem<double> Item { get; }
}


/// <summary>Normalized destination category for an existing annotation.</summary>
public enum AnnotationDestinationKind
{
    None,
    Internal,
    Uri,
    Remote,
    Other
}

/// <summary>An annotation already present in a page /Annots array.</summary>
public sealed record AnnotationRemediationCandidate : RemediationCandidate
{
    internal AnnotationRemediationCandidate(
        PdfDictionary annotation,
        int annotationIndex,
        string subtype,
        PdfRect<double>? bounds,
        PdfRect<double>? relativeBounds,
        int flags,
        bool hidden,
        bool offPage,
        bool hasStructParent,
        string? contents,
        AnnotationDestinationKind destinationKind,
        string? destinationValue,
        int sequenceIndex)
        : base(bounds ?? new PdfRect<double>(0, 0, 0, 0),
            relativeBounds ?? new PdfRect<double>(0, 0, 0, 0),
            Array.Empty<StructuredSourceRef>(), sequenceIndex)
    {
        Annotation = annotation;
        AnnotationIndex = annotationIndex;
        Subtype = subtype;
        Bounds = bounds;
        RelativeBounds = relativeBounds;
        Flags = flags;
        Hidden = hidden;
        OffPage = offPage;
        HasStructParent = hasStructParent;
        Contents = contents;
        DestinationKind = destinationKind;
        DestinationValue = destinationValue;
    }

    public override RemediationCandidateKind Kind => RemediationCandidateKind.Annotation;
    public override string CandidateId => $"Annotation:{PageIndex}:{AnnotationIndex}:{Subtype}";
    public override bool HasUsableGeometry => Bounds != null && RelativeBounds != null;
    public int AnnotationIndex { get; }
    public string Subtype { get; }
    public PdfRect<double>? Bounds { get; }
    public PdfRect<double>? RelativeBounds { get; }
    public int Flags { get; }
    public bool Hidden { get; }
    public bool OffPage { get; }
    public bool HasStructParent { get; }
    public string? Contents { get; }
    public AnnotationDestinationKind DestinationKind { get; }
    public string? DestinationValue { get; }
    internal PdfDictionary Annotation { get; }
}

/// <summary>High-level family of content represented by a remediation candidate.</summary>
public enum RemediationCandidateKind
{
    Text,
    Image,
    Path,
    Form,
    Shading,
    Annotation
}

/// <summary>
/// Text range inside a content operator selected by character or word remediation.
/// </summary>
public sealed record RemediationTextRange(
    /// <summary>Source operator reference for the text range.</summary>
    StructuredSourceRef SourceReference,
    /// <summary>Zero-based start character index inside the source operator.</summary>
    int StartCharacterIndex,
    /// <summary>Number of characters in the selected range.</summary>
    int CharacterCount,
    /// <summary>Selected text.</summary>
    string Text);

/// <summary>
/// Content-model item targeted by a remediation claim, optionally narrowed to a text range.
/// </summary>
public sealed record RemediationClaimTarget<T>(
    IContentItem<T> Item,
    RemediationTextRange? TextRange = null) where T : struct, IFloatingPoint<T>
{
    /// <summary>True when the target covers the whole content item.</summary>
    public bool IsWholeItem => TextRange == null;
}

/// <summary>
/// Helpers that map structured text candidates to content-model leaves.
/// </summary>
public static class RemediationLeafSelection
{
    /// <summary>Returns structured-text candidates at the requested granularity.</summary>
    public static IReadOnlyList<TextRemediationCandidate> GetCandidates(
        this StructuredTextPage textPage,
        Granularity granularity)
    {
        return granularity switch
        {
            Granularity.Character => new ReadOnlyCollection<TextRemediationCandidate>(
                textPage.Characters.Select(x => (TextRemediationCandidate)RemediationCandidate.From(x)).ToList()),
            Granularity.Word => new ReadOnlyCollection<TextRemediationCandidate>(
                textPage.Words.Select(x => (TextRemediationCandidate)RemediationCandidate.From(x)).ToList()),
            Granularity.Line => new ReadOnlyCollection<TextRemediationCandidate>(
                textPage.Lines.Select(x => (TextRemediationCandidate)RemediationCandidate.From(x)).ToList()),
            Granularity.Paragraph => new ReadOnlyCollection<TextRemediationCandidate>(
                textPage.Paragraphs.Select(x => (TextRemediationCandidate)RemediationCandidate.From(x)).ToList()),
            _ => throw new ArgumentOutOfRangeException(nameof(granularity), granularity, null)
        };
    }

    /// <summary>Finds content-model leaves corresponding to a candidate.</summary>
    public static IReadOnlyList<IContentItem<T>> FindLeaves<T>(
        this RemediationCandidate candidate,
        IEnumerable<IContentNode<T>> content) where T : struct, IFloatingPoint<T>
    {
        if (candidate is ContentRemediationCandidate graphical)
        {
            if (typeof(T) != typeof(double))
            {
                throw new NotSupportedException("Graphical remediation candidates currently support double-precision content models only.");
            }
            return new[] { (IContentItem<T>)(object)graphical.Item };
        }
        return ContentModelBridge.FindItems(content, candidate.SourceReferences);
    }

    /// <summary>Finds claim targets corresponding to a candidate.</summary>
    public static IReadOnlyList<RemediationClaimTarget<T>> FindTargets<T>(
        this RemediationCandidate candidate,
        IEnumerable<IContentNode<T>> content) where T : struct, IFloatingPoint<T>
    {
        if (candidate is ContentRemediationCandidate graphical)
        {
            if (typeof(T) != typeof(double))
            {
                throw new NotSupportedException("Graphical remediation candidates currently support double-precision content models only.");
            }
            return new[] { new RemediationClaimTarget<T>((IContentItem<T>)(object)graphical.Item) };
        }
        var textCandidate = (TextRemediationCandidate)candidate;
        if (!textCandidate.RequiresExactMaterialization)
        {
            return new ReadOnlyCollection<RemediationClaimTarget<T>>(
                candidate.FindLeaves(content).Select(x => new RemediationClaimTarget<T>(x)).ToList());
        }

        var targets = new List<RemediationClaimTarget<T>>();
        var seenWholeItems = new HashSet<IContentItem<T>>();
        foreach (var range in textCandidate.TextRanges)
        {
            foreach (var textContent in ContentModelBridge.FindTextFragments(content, range.SourceReference))
            {
                var fragmentStart = textContent.SourceCharacterOffset;
                var fragmentEnd = fragmentStart + textContent.Text.Length;
                var rangeStart = range.StartCharacterIndex;
                var rangeEnd = rangeStart + range.CharacterCount;
                var intersectionStart = Math.Max(fragmentStart, rangeStart);
                var intersectionEnd = Math.Min(fragmentEnd, rangeEnd);
                if (intersectionStart >= intersectionEnd)
                {
                    continue;
                }

                if (intersectionStart == fragmentStart &&
                    intersectionEnd == fragmentEnd)
                {
                    if (seenWholeItems.Add(textContent))
                    {
                        targets.Add(new RemediationClaimTarget<T>(textContent));
                    }
                }
                else
                {
                    targets.Add(new RemediationClaimTarget<T>(
                        textContent,
                        new RemediationTextRange(
                            range.SourceReference,
                            intersectionStart,
                            intersectionEnd - intersectionStart,
                            range.Text)));
                }
            }
        }

        return new ReadOnlyCollection<RemediationClaimTarget<T>>(targets);
    }

    /// <summary>Materializes candidate text ranges into content leaves.</summary>
    public static IReadOnlyList<IContentItem<T>> MaterializeLeaves<T>(
        this RemediationCandidate candidate,
        List<IContentNode<T>> content) where T : struct, IFloatingPoint<T>
    {
        var materialized = new List<IContentItem<T>>();
        foreach (var target in candidate.FindTargets(content))
        {
            if (target.TextRange == null)
            {
                materialized.Add(target.Item);
                continue;
            }

            if (target.Item is not TextContent<T> textContent)
            {
                throw new InvalidOperationException("Text range targets can only be materialized from text content.");
            }

            var replaced = ReplaceTextRange(content, textContent, target.TextRange);
            materialized.Add(replaced);
        }

        return new ReadOnlyCollection<IContentItem<T>>(materialized);
    }

    private static TextContent<T> ReplaceTextRange<T>(
        List<IContentNode<T>> nodes,
        TextContent<T> target,
        RemediationTextRange range) where T : struct, IFloatingPoint<T>
    {
        for (var i = 0; i < nodes.Count; i++)
        {
            if (ReferenceEquals(nodes[i], target))
            {
                if (!target.TrySplitByCharacterRange(
                    range.StartCharacterIndex - target.SourceCharacterOffset,
                    range.CharacterCount,
                    out var before,
                    out var selected,
                    out var after,
                    out var error))
                {
                    throw new InvalidOperationException(error ?? "Text range could not be materialized.");
                }

                var replacement = new List<IContentNode<T>>();
                if (before != null)
                {
                    replacement.Add(before);
                }

                replacement.Add(selected!);
                if (after != null)
                {
                    replacement.Add(after);
                }

                nodes.RemoveAt(i);
                nodes.InsertRange(i, replacement);
                return selected!;
            }

            if (nodes[i] is MarkedContentGroup<T> marked)
            {
                var replaced = TryReplaceTextRange(marked.Children, target, range);
                if (replaced != null)
                {
                    return replaced;
                }
            }
        }

        throw new InvalidOperationException("Text range target does not belong to the target content tree.");
    }

    private static TextContent<T>? TryReplaceTextRange<T>(
        List<IContentNode<T>> nodes,
        TextContent<T> target,
        RemediationTextRange range) where T : struct, IFloatingPoint<T>
    {
        for (var i = 0; i < nodes.Count; i++)
        {
            if (ReferenceEquals(nodes[i], target))
            {
                if (!target.TrySplitByCharacterRange(
                    range.StartCharacterIndex - target.SourceCharacterOffset,
                    range.CharacterCount,
                    out var before,
                    out var selected,
                    out var after,
                    out var error))
                {
                    throw new InvalidOperationException(error ?? "Text range could not be materialized.");
                }

                var replacement = new List<IContentNode<T>>();
                if (before != null)
                {
                    replacement.Add(before);
                }

                replacement.Add(selected!);
                if (after != null)
                {
                    replacement.Add(after);
                }

                nodes.RemoveAt(i);
                nodes.InsertRange(i, replacement);
                return selected;
            }

            if (nodes[i] is MarkedContentGroup<T> marked)
            {
                var replaced = TryReplaceTextRange(marked.Children, target, range);
                if (replaced != null)
                {
                    return replaced;
                }
            }
        }

        return null;
    }
}
