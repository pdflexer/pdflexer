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

    public PdfOutlineRoot ConvertToPdf(OutlineNode rootNode)
    {
        var root = new PdfOutlineRoot();
        foreach (var childNode in rootNode.Children)
        {
            var item = CreateItem(childNode);
            root.Add(item);
        }
        return root;
    }

    private PdfOutlineItem CreateItem(OutlineNode node)
    {
        var item = new PdfOutlineItem();
        item.Title = node.Title;
        
        if (node.Data != null)
        {
            var page = _doc.Pages[node.Data.PageIndex];
            item.GetPdfObject()[PdfName.Dest] = new PdfArray 
            { 
                page.NativeObject, 
                PdfName.XYZ, 
                new PdfNull(), 
                new PdfNull(), 
                new PdfNull() 
            };
        }
        
        foreach (var childNode in node.Children)
        {
            var childItem = CreateItem(childNode);
            item.Add(childItem);
        }
        
        return item;
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