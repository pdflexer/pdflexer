using System.Collections.ObjectModel;
using System.Numerics;

namespace PdfLexer.Content.Model;

public static class ContentModelBridge
{
    public static bool TryResolveParsedItemId<T>(
        IEnumerable<IContentNode<T>> content,
        StructuredSourceRef sourceReference,
        out ParsedContentId parsedItemId) where T : struct, IFloatingPoint<T>
    {
        foreach (var item in EnumerateItems(content))
        {
            if (item.SourceReference is not { } itemSourceReference)
            {
                continue;
            }

            if (itemSourceReference.StreamId != sourceReference.StreamId)
            {
                continue;
            }

            var itemStart = itemSourceReference.OperatorStart;
            var itemEnd = itemStart + itemSourceReference.OperatorLength;
            var sourceStart = sourceReference.OperatorStart;
            var sourceEnd = sourceStart + sourceReference.OperatorLength;
            if (itemStart <= sourceStart && itemEnd >= sourceEnd && item.ParsedItemId is { } id)
            {
                parsedItemId = id;
                return true;
            }
        }

        parsedItemId = default;
        return false;
    }

    public static IContentItem<T>? FindItem<T>(
        IEnumerable<IContentNode<T>> content,
        ParsedContentId parsedItemId) where T : struct, IFloatingPoint<T>
    {
        return EnumerateItems(content).FirstOrDefault(x => x.ParsedItemId == parsedItemId);
    }

    public static IReadOnlyList<IContentItem<T>> FindItems<T>(
        IEnumerable<IContentNode<T>> content,
        StructuredCharacter character) where T : struct, IFloatingPoint<T>
    {
        return FindItems(content, new[] { character.SourceReference });
    }

    public static IReadOnlyList<IContentItem<T>> FindItems<T>(
        IEnumerable<IContentNode<T>> content,
        StructuredWord word) where T : struct, IFloatingPoint<T>
    {
        return FindItems(content, word.SourceReferences);
    }

    public static IReadOnlyList<IContentItem<T>> FindItems<T>(
        IEnumerable<IContentNode<T>> content,
        StructuredLine line) where T : struct, IFloatingPoint<T>
    {
        return FindItems(content, line.SourceReferences);
    }

    public static IReadOnlyList<IContentItem<T>> FindItems<T>(
        IEnumerable<IContentNode<T>> content,
        StructuredParagraph paragraph) where T : struct, IFloatingPoint<T>
    {
        return FindItems(content, paragraph.SourceReferences);
    }

    public static IReadOnlyList<IContentItem<T>> FindItems<T>(
        IEnumerable<IContentNode<T>> content,
        IEnumerable<StructuredSourceRef> sourceReferences) where T : struct, IFloatingPoint<T>
    {
        var ordered = new List<IContentItem<T>>();
        var seen = new HashSet<ParsedContentId>();
        foreach (var sourceReference in sourceReferences)
        {
            if (!TryResolveParsedItemId(content, sourceReference, out var parsedItemId))
            {
                continue;
            }

            if (!seen.Add(parsedItemId))
            {
                continue;
            }

            var item = FindItem(content, parsedItemId);
            if (item != null)
            {
                ordered.Add(item);
            }
        }

        return new ReadOnlyCollection<IContentItem<T>>(ordered);
    }

    private static IEnumerable<IContentItem<T>> EnumerateItems<T>(
        IEnumerable<IContentNode<T>> content) where T : struct, IFloatingPoint<T>
    {
        foreach (var node in content)
        {
            if (node is MarkedContentGroup<T> marked)
            {
                foreach (var child in EnumerateItems(marked.Children))
                {
                    yield return child;
                }

                yield return marked;
            }
            else if (node is FormContent<T> form)
            {
                foreach (var child in EnumerateItems(form.Parse()))
                {
                    yield return child;
                }

                yield return form;
            }
            else if (node is IContentItem<T> item)
            {
                yield return item;
            }
        }
    }
}
