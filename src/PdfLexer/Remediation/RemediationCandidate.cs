using System.Collections.ObjectModel;
using System.Numerics;
using PdfLexer.Content;
using PdfLexer.Content.Model;

namespace PdfLexer.Remediation;

/// <summary>
/// Structured-text/content candidate considered by remediation rules.
/// </summary>
public sealed record RemediationCandidate
{
    internal RemediationCandidate(
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
        bool? isGrayish = null)
    {
        Granularity = granularity;
        Text = text;
        BoundingBox = boundingBox;
        RelativeBoundingBox = relativeBoundingBox;
        Characters = characters;
        SourceReferences = sourceReferences;
        SequenceIndex = sequenceIndex;
        FontSize = fontSize;
        FontName = fontName ?? GetCommon(characters.Select(x => x.FontName));
        FontWeight = fontWeight ?? GetCommon(characters.Select(x => x.FontWeight));
        Italic = italic ?? GetCommon(characters.Select(x => x.Italic));
        IsGrayish = isGrayish ?? GetCommon(characters.Select(x => x.IsGrayish));
    }

    /// <summary>Granularity represented by this candidate.</summary>
    public Granularity Granularity { get; }

    /// <summary>Candidate text.</summary>
    public string Text { get; }

    /// <summary>Candidate bounds in page coordinates.</summary>
    public PdfRect<double> BoundingBox { get; }

    /// <summary>Candidate bounds in normalized page-relative coordinates.</summary>
    public PdfRect<double> RelativeBoundingBox { get; }

    /// <summary>Structured characters that compose the candidate.</summary>
    public IReadOnlyList<StructuredCharacter> Characters { get; }

    /// <summary>Source references for content operators that compose the candidate.</summary>
    public IReadOnlyList<StructuredSourceRef> SourceReferences { get; }

    /// <summary>Text ranges selected inside content operators.</summary>
    public IReadOnlyList<RemediationTextRange> TextRanges => BuildTextRanges(Characters);

    /// <summary>Reading-order sequence index.</summary>
    public int SequenceIndex { get; }

    /// <summary>Representative font size.</summary>
    public double FontSize { get; }

    /// <summary>Representative font name, when available.</summary>
    public string? FontName { get; }

    /// <summary>Representative font weight, when available.</summary>
    public int? FontWeight { get; }

    /// <summary>Representative italic style, when available.</summary>
    public bool? Italic { get; }

    /// <summary>Representative grayish color flag, when available.</summary>
    public bool? IsGrayish { get; }

    /// <summary>Creates a candidate from a structured character.</summary>
    public static RemediationCandidate From(StructuredCharacter character) =>
        new(
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
        new(
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
        new(
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
        return new RemediationCandidate(
            Granularity.Paragraph,
            paragraph.Text,
            paragraph.BoundingBox,
            paragraph.RelativeBoundingBox,
            new ReadOnlyCollection<StructuredCharacter>(characters),
            paragraph.SourceReferences,
            paragraph.SequenceIndex,
            characters.Count == 0 ? 0 : characters.Average(x => x.FontSize));
    }

    private static T? GetCommon<T>(IEnumerable<T?> values)
    {
        var found = false;
        T? common = default;
        foreach (var value in values)
        {
            if (value == null)
            {
                continue;
            }

            if (!found)
            {
                found = true;
                common = value;
                continue;
            }

            if (!EqualityComparer<T>.Default.Equals(common, value))
            {
                return default;
            }
        }

        return common;
    }

    private static IReadOnlyList<RemediationTextRange> BuildTextRanges(IReadOnlyList<StructuredCharacter> characters)
    {
        if (characters.Count == 0)
        {
            return Array.Empty<RemediationTextRange>();
        }

        var ranges = new List<RemediationTextRange>();
        foreach (var sourceGroup in characters
            .GroupBy(x => x.SourceReference)
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
                    ranges.Add(new RemediationTextRange(
                        sourceGroup.Key,
                        rangeStart,
                        text.Count,
                        new string(text.ToArray())));
                    rangeStart = character.SourceCharacterIndex;
                    expected = rangeStart;
                    text.Clear();
                }

                text.Add(character.Char);
                expected++;
            }

            ranges.Add(new RemediationTextRange(
                sourceGroup.Key,
                rangeStart,
                text.Count,
                new string(text.ToArray())));
        }

        return new ReadOnlyCollection<RemediationTextRange>(ranges);
    }
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
    public static IReadOnlyList<RemediationCandidate> GetCandidates(
        this StructuredTextPage textPage,
        Granularity granularity)
    {
        return granularity switch
        {
            Granularity.Character => new ReadOnlyCollection<RemediationCandidate>(
                textPage.Characters.Select(RemediationCandidate.From).ToList()),
            Granularity.Word => new ReadOnlyCollection<RemediationCandidate>(
                textPage.Words.Select(RemediationCandidate.From).ToList()),
            Granularity.Line => new ReadOnlyCollection<RemediationCandidate>(
                textPage.Lines.Select(RemediationCandidate.From).ToList()),
            Granularity.Paragraph => new ReadOnlyCollection<RemediationCandidate>(
                textPage.Paragraphs.Select(RemediationCandidate.From).ToList()),
            _ => throw new ArgumentOutOfRangeException(nameof(granularity), granularity, null)
        };
    }

    /// <summary>Finds content-model leaves corresponding to a candidate.</summary>
    public static IReadOnlyList<IContentItem<T>> FindLeaves<T>(
        this RemediationCandidate candidate,
        IEnumerable<IContentNode<T>> content) where T : struct, IFloatingPoint<T>
    {
        return ContentModelBridge.FindItems(content, candidate.SourceReferences);
    }

    /// <summary>Finds claim targets corresponding to a candidate.</summary>
    public static IReadOnlyList<RemediationClaimTarget<T>> FindTargets<T>(
        this RemediationCandidate candidate,
        IEnumerable<IContentNode<T>> content) where T : struct, IFloatingPoint<T>
    {
        if (candidate.Granularity is not (Granularity.Character or Granularity.Word))
        {
            return new ReadOnlyCollection<RemediationClaimTarget<T>>(
                candidate.FindLeaves(content).Select(x => new RemediationClaimTarget<T>(x)).ToList());
        }

        var targets = new List<RemediationClaimTarget<T>>();
        var seenWholeItems = new HashSet<IContentItem<T>>();
        foreach (var range in candidate.TextRanges)
        {
            if (!ContentModelBridge.TryResolveParsedItemId(content, range.SourceReference, out var parsedItemId))
            {
                continue;
            }

            var item = ContentModelBridge.FindItem(content, parsedItemId);
            if (item == null)
            {
                continue;
            }

            if (item is TextContent<T> textContent &&
                (range.StartCharacterIndex != 0 || range.CharacterCount != textContent.Text.Length))
            {
                targets.Add(new RemediationClaimTarget<T>(item, range));
                continue;
            }

            if (seenWholeItems.Add(item))
            {
                targets.Add(new RemediationClaimTarget<T>(item));
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
                    range.StartCharacterIndex,
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
                    range.StartCharacterIndex,
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
