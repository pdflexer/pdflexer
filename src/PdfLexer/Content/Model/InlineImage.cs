using PdfLexer.Writing;

namespace PdfLexer.Content.Model;

internal class InlineImage : IContentGroup
{
    public ContentType Type { get; } = ContentType.InlineImage;
    public List<MarkedContent>? Markings { get; set; }
    public bool CompatibilitySection { get; set; }
    public required GfxState GraphicsState { get; set; }
    public required InlineImage_Op Img { get; set; }

    public void Write(ContentWriter writer)
    {
        if (writer.GS.CTM != GraphicsState.CTM)
        {
            var cm = writer.GS.GetTranslation(GraphicsState.CTM);
            writer.Transform(cm);
        }
        writer.Op(Img);
    }
}