using PdfLexer.DOM;
using System.Collections.Generic;
using System.Linq;

namespace PdfLexer.Writing;

public class OutlineBuilder
{
    private readonly PdfDocument _doc;

    public OutlineBuilder(PdfDocument doc)
    {
        _doc = doc;
    }

    public List<AggregatedOutline> Aggregate()
    {
        var result = new List<AggregatedOutline>();
        for (int i = 0; i < _doc.Pages.Count; i++)
        {
            var page = _doc.Pages[i];
            foreach (var outline in page.Outlines)
            {
                result.Add(new AggregatedOutline
                {
                    Outline = outline,
                    PageIndex = i
                });
            }
        }
        return result;
    }

    public OutlineNode BuildTree(List<AggregatedOutline> aggregated)
    {
        var root = new OutlineNode { Title = "ROOT" };
        foreach (var item in aggregated)
        {
            var current = root;
            if (item.Outline.Section != null)
            {
                foreach (var part in item.Outline.Section)
                {
                    var next = current.Children.FirstOrDefault(x => x.Title == part);
                    if (next == null)
                    {
                        next = new OutlineNode { Title = part };
                        current.Children.Add(next);
                    }
                    current = next;
                }
            }
            
            var existing = current.Children.FirstOrDefault(x => x.Title == item.Outline.Title);
            if (existing != null)
            {
                existing.Data = item;
            }
            else
            {
                current.Children.Add(new OutlineNode { Title = item.Outline.Title, Data = item });
            }
        }
        
        SortNodes(root);
        return root;
    }

    private void SortNodes(OutlineNode node)
    {
        if (node.Children.Count == 0) return;
        
        node.Children.Sort((a, b) => {
            int aOrder = a.Data?.Outline.Order ?? int.MaxValue;
            int bOrder = b.Data?.Outline.Order ?? int.MaxValue;
            
            if (aOrder != bOrder) return aOrder.CompareTo(bOrder);
            
            int aPage = a.Data?.PageIndex ?? GetMinPageIndex(a);
            int bPage = b.Data?.PageIndex ?? GetMinPageIndex(b);
            
            return aPage.CompareTo(bPage);
        });
        
        foreach (var child in node.Children)
        {
            SortNodes(child);
        }
    }

    private int GetMinPageIndex(OutlineNode node)
    {
        if (node.Data != null) return node.Data.PageIndex;
        if (node.Children.Count == 0) return int.MaxValue;
        return node.Children.Min(GetMinPageIndex);
    }

    public PdfOutlineRoot ConvertToPdf(OutlineNode rootNode, List<PdfIndirectRef>? pageRefs = null)
    {
        var rootDict = new PdfDictionary();
        rootDict[PdfName.TYPE] = PdfName.Outlines;
        var rootRef = PdfIndirectRef.Create(rootDict);
        
        var items = new List<(OutlineNode Node, PdfOutlineItem Item, PdfIndirectRef Ref)>();
        
        CollectItems(rootNode, items);

        foreach (var entry in items)
        {
            var item = entry.Item;
            var node = entry.Node;
            
            item.Title = node.Title;
            if (node.Data != null)
            {
                IPdfObject pageObj;
                if (pageRefs != null && node.Data.PageIndex < pageRefs.Count)
                {
                    pageObj = pageRefs[node.Data.PageIndex];
                }
                else
                {
                    pageObj = _doc.Pages[node.Data.PageIndex].NativeObject;
                }

                item.GetPdfObject()[PdfName.Dest] = new PdfArray 
                { 
                    pageObj, 
                    PdfName.XYZ, 
                    new PdfNull(), 
                    new PdfNull(), 
                    new PdfNull() 
                };
            }
        }

        // Link them
        var map = items.ToDictionary(x => x.Node, x => (x.Item, x.Ref));
        LinkNodes(rootNode, rootDict, rootRef, map);

        return new PdfOutlineRoot(rootDict);
    }

    private void CollectItems(OutlineNode parent, List<(OutlineNode Node, PdfOutlineItem Item, PdfIndirectRef Ref)> collected)
    {
        foreach (var child in parent.Children)
        {
            var dict = new PdfDictionary();
            collected.Add((child, new PdfOutlineItem(dict), PdfIndirectRef.Create(dict)));
            CollectItems(child, collected);
        }
    }

    private void LinkNodes(OutlineNode node, PdfDictionary pdfParent, PdfIndirectRef parentRef, Dictionary<OutlineNode, (PdfOutlineItem Item, PdfIndirectRef Ref)> map)
    {
        if (node.Children.Count == 0) return;

        pdfParent[PdfName.First] = map[node.Children.First()].Ref;
        pdfParent[PdfName.Last] = map[node.Children.Last()].Ref;

        PdfIndirectRef? prevRef = null;
        for (int i = 0; i < node.Children.Count; i++)
        {
            var childNode = node.Children[i];
            var (item, ir) = map[childNode];
            var dict = item.GetPdfObject();
            
            dict[PdfName.Parent] = parentRef;
            if (prevRef != null)
            {
                dict[PdfName.Prev] = prevRef;
                var prevNode = node.Children[i - 1];
                map[prevNode].Item.GetPdfObject()[PdfName.Next] = ir;
            }
            
            LinkNodes(childNode, dict, ir, map);
            prevRef = ir;
        }

        int openCount = GetOpenCount(node);
        if (openCount > 0 && node.Title != "ROOT")
        {
            pdfParent[PdfName.Count] = new PdfIntNumber(openCount);
        } else if (node.Title == "ROOT")
        {
            pdfParent[PdfName.Count] = new PdfIntNumber(openCount);
        }
    }

    private int GetOpenCount(OutlineNode node)
    {
        int count = node.Children.Count;
        foreach (var child in node.Children)
        {
            count += GetOpenCount(child);
        }
        return count;
    }
}

public class AggregatedOutline
{
    public required PdfOutline Outline { get; init; }
    public int PageIndex { get; init; }
}

public class OutlineNode
{
    public string Title { get; set; } = string.Empty;
    public List<OutlineNode> Children { get; } = new();
    public AggregatedOutline? Data { get; set; }
}