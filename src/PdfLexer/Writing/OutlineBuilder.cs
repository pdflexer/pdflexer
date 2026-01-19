using PdfLexer.DOM;
using System.Collections.Generic;
using System.Linq;

namespace PdfLexer.Writing;

internal class OutlineBuilder
{
    private readonly PdfDocument _doc;

    public OutlineBuilder(PdfDocument doc)
    {
        _doc = doc;
    }

    public PdfDictionary ConvertToPdf(BookmarkNode rootNode, Dictionary<StructureNode, PdfIndirectRef>? structureMap = null)
    {
        var rootDict = new PdfDictionary();
        rootDict[PdfName.TypeName] = PdfName.Outlines;

        var rootRef = PdfIndirectRef.Create(rootDict);

        // Flatten the tree for processing but keep hierarchy references
        var items = new List<(BookmarkNode Node, PdfDictionary Dict, PdfIndirectRef Ref)>();

        // Note: rootNode itself isn't a bookmark, it's the container. We process its children.
        foreach (var child in rootNode.Children)
        {
            var dict = new PdfDictionary();
            var ir = PdfIndirectRef.Create(dict);
            items.Add((child, dict, ir));
            CollectItems(child, items);
        }

        // Create a map for easy lookup
        var map = items.ToDictionary(x => x.Node, x => (x.Dict, x.Ref));

        // Link root to first/last
        if (rootNode.Children.Count > 0)
        {
            rootDict[PdfName.First] = map[rootNode.Children.First()].Ref;
            rootDict[PdfName.Last] = map[rootNode.Children.Last()].Ref;

            // Calculate Count for Root (total open items)
            // Assuming all open for now
            int totalOpen = 0;
            foreach (var child in rootNode.Children)
            {
                totalOpen += GetOpenCount(child);
            }
            rootDict[PdfName.Count] = new PdfIntNumber(totalOpen);
        }

        // We need to iterate the top level separately to link to root parent
        LinkSiblings(rootNode.Children, rootRef, map);

        foreach (var (node, dict, ir) in items)
        {
            // Set properties
            dict[PdfName.Title] = new PdfString(node.Title);

            if (node.Destination != null)
            {
                if (node.Destination is PdfDictionary pageDict)
                {
                    dict[PdfName.Dest] = new PdfArray
                {
                        PdfIndirectRef.Create(pageDict),
                        PdfName.XYZ,
                        new PdfNull(),
                        new PdfNull(),
                        new PdfNull()
                    };
                }
                else if (node.Destination is PdfIndirectRef pageRef)
                {
                    dict[PdfName.Dest] = new PdfArray
                    {
                        pageRef,
                        PdfName.XYZ,
                        new PdfNull(),
                        new PdfNull(),
                        new PdfNull()
                    };
                }
                else
                {
                    dict[PdfName.Dest] = node.Destination;
                }
            }

            if (node.StructureElement != null && structureMap != null)
            {
                if (structureMap.TryGetValue(node.StructureElement, out var seRef))
                {
                    dict[PdfName.SE] = seRef;
                }
            }

            if (node.Color != null && node.Color.Length == 3)
            {
                dict[PdfName.C] = new PdfArray(node.Color.Select(c => (IPdfObject)new PdfDoubleNumber(c)).ToList());
            }

            if (node.Style.HasValue)
            {
                dict[PdfName.F] = new PdfIntNumber(node.Style.Value);
            }

            // Link children if any
            if (node.Children.Count > 0)
            {
                dict[PdfName.First] = map[node.Children.First()].Ref;
                dict[PdfName.Last] = map[node.Children.Last()].Ref;
                LinkSiblings(node.Children, ir, map);

                // Calculate count
                int count = 0;
                foreach (var child in node.Children)
                {
                    count += GetOpenCount(child);
                }

                if (!node.IsOpen)
                {
                    count = -count; // Negative count means closed
                }
                dict[PdfName.Count] = new PdfIntNumber(count);
            }
        }

        return rootDict;
    }

    private void LinkSiblings(List<BookmarkNode> siblings, PdfIndirectRef parentRef, Dictionary<BookmarkNode, (PdfDictionary Dict, PdfIndirectRef Ref)> map)
    {
        for (int i = 0; i < siblings.Count; i++)
        {
            var node = siblings[i];
            var (dict, ir) = map[node];

            dict[PdfName.Parent] = parentRef;

            if (i > 0)
            {
                var prev = siblings[i - 1];
                dict[PdfName.Prev] = map[prev].Ref;
            }

            if (i < siblings.Count - 1)
            {
                var next = siblings[i + 1];
                dict[PdfName.Next] = map[next].Ref;
            }
        }
    }

    private void CollectItems(BookmarkNode parent, List<(BookmarkNode Node, PdfDictionary Dict, PdfIndirectRef Ref)> collected)
    {
        foreach (var child in parent.Children)
        {
            var dict = new PdfDictionary();
            var ir = PdfIndirectRef.Create(dict);
            collected.Add((child, dict, ir));
            CollectItems(child, collected);
        }
    }

    private int GetOpenCount(BookmarkNode node)
    {
        int count = 1; // Self
        if (node.IsOpen)
        {
            foreach (var child in node.Children)
            {
                count += GetOpenCount(child);
            }
        }
        return count;
    }
}
