using System.Collections.Generic;

namespace PdfLexer.DOM;

public class PdfOutline
{
    public string Title { get; set; } = string.Empty;
    public List<string>? Section { get; set; }
    public int? Order { get; set; }
    public string? Style { get; set; }
    public double[]? Color { get; set; }
}
