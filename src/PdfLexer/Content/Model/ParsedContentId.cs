using System.Numerics;

namespace PdfLexer.Content.Model;

public readonly record struct ParsedContentId(
    ulong StreamId,
    int StartOperatorIndex,
    int OperatorCount = 1)
{
    public int EndOperatorIndexInclusive => StartOperatorIndex + OperatorCount - 1;

    public bool Contains(ParsedContentId other)
    {
        return StreamId == other.StreamId
            && StartOperatorIndex <= other.StartOperatorIndex
            && EndOperatorIndexInclusive >= other.EndOperatorIndexInclusive;
    }

    public bool ContainsOperator(ulong streamId, int operatorIndex)
    {
        return StreamId == streamId
            && StartOperatorIndex <= operatorIndex
            && EndOperatorIndexInclusive >= operatorIndex;
    }

    public static ParsedContentId FromSingleOperator(ulong streamId, int operatorIndex)
    {
        return new ParsedContentId(streamId, operatorIndex, 1);
    }

    public static ParsedContentId Merge(ParsedContentId first, ParsedContentId last)
    {
        if (first.StreamId != last.StreamId)
        {
            throw new InvalidOperationException("Cannot merge parsed content identities from different streams.");
        }

        var start = Math.Min(first.StartOperatorIndex, last.StartOperatorIndex);
        var end = Math.Max(first.EndOperatorIndexInclusive, last.EndOperatorIndexInclusive);
        return new ParsedContentId(first.StreamId, start, end - start + 1);
    }
}

internal static class ContentIdentityHelpers
{
    public static StructuredSourceRef? Merge(StructuredSourceRef? first, StructuredSourceRef? last)
    {
        if (first == null)
        {
            return last;
        }

        if (last == null)
        {
            return first;
        }

        if (first.Value.StreamId != last.Value.StreamId)
        {
            return null;
        }

        var start = Math.Min(first.Value.OperatorStart, last.Value.OperatorStart);
        var end = Math.Max(
            first.Value.OperatorStart + first.Value.OperatorLength,
            last.Value.OperatorStart + last.Value.OperatorLength);
        return new StructuredSourceRef(first.Value.StreamId, start, end - start);
    }

    public static ParsedContentId? Merge(ParsedContentId? first, ParsedContentId? last)
    {
        if (first == null)
        {
            return last;
        }

        if (last == null)
        {
            return first;
        }

        if (first.Value.StreamId != last.Value.StreamId)
        {
            return null;
        }

        return ParsedContentId.Merge(first.Value, last.Value);
    }

    public static ParsedContentId? Merge<T>(IEnumerable<IContentNode<T>> children) where T : struct, IFloatingPoint<T>
    {
        ParsedContentId? merged = null;
        foreach (var child in children)
        {
            if (child is not IContentItem<T> item)
            {
                continue;
            }

            merged = Merge(merged, item.ParsedItemId);
        }

        return merged;
    }

    public static StructuredSourceRef? MergeSourceRefs<T>(IEnumerable<IContentNode<T>> children) where T : struct, IFloatingPoint<T>
    {
        StructuredSourceRef? merged = null;
        foreach (var child in children)
        {
            if (child is not IContentItem<T> item)
            {
                continue;
            }

            merged = Merge(merged, item.SourceReference);
        }

        return merged;
    }
}
