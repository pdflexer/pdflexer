using PdfLexer.Content;
using PdfLexer.Content.Model;

namespace PdfLexer.Remediation;

internal readonly record struct SourceTextSpan(
    StructuredSourceRef SourceReference,
    int StartCharacterIndex,
    int CharacterCount)
{
    public int EndCharacterIndex => StartCharacterIndex + CharacterCount;

    public bool Overlaps(SourceTextSpan other) =>
        SourceReference == other.SourceReference &&
        StartCharacterIndex < other.EndCharacterIndex &&
        other.StartCharacterIndex < EndCharacterIndex;
}

internal sealed record OwnedTextSpan(SourceTextSpan Span, RemediationClaim Claim);

internal sealed class TextOwnershipIndex
{
    private readonly List<OwnedTextSpan> _owned = new();

    public IReadOnlyList<OwnedTextSpan> FindOverlaps(IEnumerable<SourceTextSpan> spans)
    {
        var requested = spans.ToList();
        return _owned
            .Where(x => requested.Any(y => x.Span.Overlaps(y)))
            .ToList();
    }

    public void Add(RemediationClaim claim, IEnumerable<SourceTextSpan> spans)
    {
        foreach (var span in spans.Where(x => x.CharacterCount > 0))
        {
            _owned.Add(new OwnedTextSpan(span, claim));
        }
    }

    public void Remove(RemediationClaim claim)
    {
        _owned.RemoveAll(x => x.Claim.ClaimId == claim.ClaimId);
    }

    public IReadOnlyList<SourceTextSpan> GetUnowned(SourceTextSpan source)
    {
        var covered = _owned
            .Select(x => x.Span)
            .Where(x => x.SourceReference == source.SourceReference && x.Overlaps(source))
            .Select(x => new SourceTextSpan(
                source.SourceReference,
                Math.Max(source.StartCharacterIndex, x.StartCharacterIndex),
                Math.Min(source.EndCharacterIndex, x.EndCharacterIndex) -
                Math.Max(source.StartCharacterIndex, x.StartCharacterIndex)))
            .OrderBy(x => x.StartCharacterIndex)
            .ToList();

        if (covered.Count == 0)
        {
            return new[] { source };
        }

        var gaps = new List<SourceTextSpan>();
        var cursor = source.StartCharacterIndex;
        foreach (var span in covered)
        {
            if (span.StartCharacterIndex > cursor)
            {
                gaps.Add(new SourceTextSpan(source.SourceReference, cursor, span.StartCharacterIndex - cursor));
            }

            cursor = Math.Max(cursor, span.EndCharacterIndex);
        }

        if (cursor < source.EndCharacterIndex)
        {
            gaps.Add(new SourceTextSpan(source.SourceReference, cursor, source.EndCharacterIndex - cursor));
        }

        return gaps;
    }

    public bool IsFullyOwned(SourceTextSpan source) => GetUnowned(source).Count == 0;
}
