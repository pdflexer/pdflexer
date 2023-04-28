using PdfLexer.Writing;

namespace PdfLexer.Content.Model;

internal class FormContent : IContentGroup
{
    public ContentType Type { get; } = ContentType.Form;
    public required GfxState GraphicsState { get; set; }
    public required PdfStream Stream { get; set; }
    public List<MarkedContent>? Markings { get; set; }
    public bool CompatibilitySection { get; set; }

    public void Write(ContentWriter writer)
    {
        writer.Form(Stream);
    }

    public List<IContentGroup> Parse(PdfDictionary parentPage)
    {
        var parser = new ContentModelParser(ParsingContext.Current, parentPage, Stream, GraphicsState);
        return parser.Parse();
    }


    // not able to accurately get bounding box for form
    // without fully processing it
}