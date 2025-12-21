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
        return root;
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