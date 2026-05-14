using System.Collections.ObjectModel;
using System.Numerics;

namespace PdfLexer.Content.Model;

public static class ContentWrapExtensions
{
    public static MarkedContentGroup<T> Wrap<T>(
        this List<IContentGroup<T>> content,
        IEnumerable<IContentItem<T>> leaves,
        MarkedContent tag) where T : struct, IFloatingPoint<T>
    {
        ArgumentNullException.ThrowIfNull(content);

        var nodes = content.Cast<IContentNode<T>>().ToList();
        var wrapper = nodes.Wrap(leaves, tag);

        content.Clear();
        content.AddRange(nodes.Cast<IContentGroup<T>>());
        return wrapper;
    }

    public static MarkedContentGroup<T> Wrap<T>(
        this List<IContentNode<T>> content,
        IEnumerable<IContentItem<T>> leaves,
        MarkedContent tag) where T : struct, IFloatingPoint<T>
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(leaves);

        var selectedLeaves = leaves.ToList();
        if (selectedLeaves.Count == 0)
        {
            throw new InvalidOperationException("Wrap requires at least one leaf.");
        }

        if (selectedLeaves.Any(x => x is IContentContainer<T>))
        {
            throw new InvalidOperationException("Wrap only accepts leaf content items.");
        }

        var selectedSet = new HashSet<IContentItem<T>>(selectedLeaves);
        if (selectedSet.Count != selectedLeaves.Count)
        {
            throw new InvalidOperationException("Wrap leaves must be unique.");
        }

        var entries = Flatten(content, new List<MarkedContentGroup<T>>()).ToList();
        var selectedEntries = entries.Where(x => selectedSet.Contains(x.Leaf)).ToList();
        if (selectedEntries.Count != selectedSet.Count)
        {
            throw new InvalidOperationException("All wrap leaves must belong to the target content tree.");
        }

        var firstIndex = entries.FindIndex(x => ReferenceEquals(x.Leaf, selectedEntries[0].Leaf));
        var lastIndex = entries.FindLastIndex(x => ReferenceEquals(x.Leaf, selectedEntries[^1].Leaf));
        if (lastIndex - firstIndex + 1 != selectedEntries.Count)
        {
            throw new InvalidOperationException("Wrap leaves must form a contiguous leaf range.");
        }

        var commonAncestors = selectedEntries[0].Ancestors.ToList();
        foreach (var entry in selectedEntries.Skip(1))
        {
            var count = Math.Min(commonAncestors.Count, entry.Ancestors.Count);
            var shared = 0;
            while (shared < count && ReferenceEquals(commonAncestors[shared], entry.Ancestors[shared]))
            {
                shared++;
            }

            commonAncestors = commonAncestors.Take(shared).ToList();
        }

        var targetContainer = commonAncestors.Count == 0 ? content : commonAncestors[^1].Children;
        var exploded = ExplodeSelection(targetContainer, selectedSet);
        var selectedIndexes = exploded
            .Select((x, index) => (x, index))
            .Where(x => x.x.Selected)
            .Select(x => x.index)
            .ToList();
        if (selectedIndexes.Count != selectedSet.Count)
        {
            throw new InvalidOperationException("Wrap failed to resolve the selected leaves into a single container.");
        }

        if (selectedIndexes[^1] - selectedIndexes[0] + 1 != selectedIndexes.Count)
        {
            throw new InvalidOperationException("Wrap leaves did not remain contiguous after splitting ancestor groups.");
        }

        var before = exploded.Take(selectedIndexes[0]).Select(x => x.Node).ToList();
        var wrappedChildren = exploded
            .Skip(selectedIndexes[0])
            .Take(selectedIndexes.Count)
            .Select(x => x.Node)
            .ToList();
        var after = exploded.Skip(selectedIndexes[^1] + 1).Select(x => x.Node).ToList();

        var firstWrappedItem = wrappedChildren.OfType<IContentItem<T>>().FirstOrDefault();
        var wrapper = new MarkedContentGroup<T>(tag)
        {
            GraphicsState = firstWrappedItem?.GraphicsState ?? new GfxState<T>(),
            CompatibilitySection = firstWrappedItem?.CompatibilitySection ?? false
        };
        wrapper.Children.AddRange(wrappedChildren);

        targetContainer.Clear();
        targetContainer.AddRange(before);
        targetContainer.Add(wrapper);
        targetContainer.AddRange(after);

        return wrapper;
    }

    private static List<ExplodedNode<T>> ExplodeSelection<T>(
        List<IContentNode<T>> children,
        HashSet<IContentItem<T>> selectedSet) where T : struct, IFloatingPoint<T>
    {
        var exploded = new List<ExplodedNode<T>>();
        foreach (var child in children)
        {
            if (child is MarkedContentGroup<T> group)
            {
                var nested = ExplodeSelection(group.Children, selectedSet);
                if (nested.All(x => !x.Selected))
                {
                    exploded.Add(new ExplodedNode<T>(group, false));
                    continue;
                }

                if (nested.All(x => x.Selected))
                {
                    exploded.AddRange(nested);
                    continue;
                }

                var unselectedBuffer = new List<IContentNode<T>>();
                foreach (var explodedNode in nested)
                {
                    if (explodedNode.Selected)
                    {
                        if (unselectedBuffer.Count > 0)
                        {
                            exploded.Add(new ExplodedNode<T>(CreateGroupCopy(group, unselectedBuffer), false));
                            unselectedBuffer = new List<IContentNode<T>>();
                        }

                        exploded.Add(explodedNode);
                    }
                    else
                    {
                        unselectedBuffer.Add(explodedNode.Node);
                    }
                }

                if (unselectedBuffer.Count > 0)
                {
                    exploded.Add(new ExplodedNode<T>(CreateGroupCopy(group, unselectedBuffer), false));
                }

                continue;
            }

            if (child is IContentItem<T> item)
            {
                exploded.Add(new ExplodedNode<T>(child, selectedSet.Contains(item)));
            }
            else
            {
                exploded.Add(new ExplodedNode<T>(child, false));
            }
        }

        return exploded;
    }

    private static MarkedContentGroup<T> CreateGroupCopy<T>(
        MarkedContentGroup<T> group,
        List<IContentNode<T>> children) where T : struct, IFloatingPoint<T>
    {
        var copy = new MarkedContentGroup<T>(group.Tag)
        {
            GraphicsState = group.GraphicsState,
            CompatibilitySection = group.CompatibilitySection
        };
        copy.Children.AddRange(children);
        return copy;
    }

    private static IEnumerable<LeafEntry<T>> Flatten<T>(
        IEnumerable<IContentNode<T>> content,
        List<MarkedContentGroup<T>> ancestors) where T : struct, IFloatingPoint<T>
    {
        foreach (var node in content)
        {
            if (node is MarkedContentGroup<T> group)
            {
                ancestors.Add(group);
                foreach (var entry in Flatten(group.Children, ancestors))
                {
                    yield return entry;
                }

                ancestors.RemoveAt(ancestors.Count - 1);
                continue;
            }

            if (node is IContentItem<T> item)
            {
                yield return new LeafEntry<T>(item, new ReadOnlyCollection<MarkedContentGroup<T>>(ancestors.ToList()));
            }
        }
    }

    private sealed record LeafEntry<T>(
        IContentItem<T> Leaf,
        IReadOnlyList<MarkedContentGroup<T>> Ancestors) where T : struct, IFloatingPoint<T>;

    private sealed record ExplodedNode<T>(IContentNode<T> Node, bool Selected) where T : struct, IFloatingPoint<T>;
}
