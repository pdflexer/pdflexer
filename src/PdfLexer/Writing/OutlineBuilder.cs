using PdfLexer.DOM;
using System.Collections.Generic;

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
}

public class AggregatedOutline
{
    public required PdfOutline Outline { get; init; }
    public int PageIndex { get; init; }
}
