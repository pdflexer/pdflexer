using System.Collections.Generic;
using PdfLexer.DOM;

namespace PdfLexer.DOM;

public class BookmarkNode
{
    public string Title { get; set; } = string.Empty;
    public IPdfObject? Destination { get; set; }
    public double[]? Color { get; set; }
    public int? Style { get; set; } // 1 = Italic, 2 = Bold
    public bool IsOpen { get; set; } = true;
    public List<BookmarkNode> Children { get; } = new List<BookmarkNode>();
}