using PdfLexer.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Content.Model;

public record class MarkedContent
{
    public MarkedContent(PdfName name)
    {
        Name = name;
    }
    public PdfName Name { get; set; }
    public PdfDictionary? InlineProps { get; set; }
    public PdfDictionary? PropList { get; set; }
    // TODO proper handling for created marked content
    internal bool? OCGDefault { get; set; }
}
