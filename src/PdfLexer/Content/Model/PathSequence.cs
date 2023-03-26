using PdfLexer.Writing;

namespace PdfLexer.Content.Model;

internal enum ContentType
{
    Text,
    Paths,
    InlineImage,
    XImage,
    XForm,
    Shading,
    MarkedPoint
}
internal interface IContentGroup
{
    public GfxState GraphicsState { get; }
    public ContentType Type { get; }
    public List<MarkedContent>? Markings { get; }
    public bool CompatibilitySection { get; }
    public void Write(ContentWriter writer);
}

internal class PathSequence : IContentGroup
{
    public ContentType Type { get; } = ContentType.Paths;
    public List<MarkedContent>? Markings { get; set; }
    public bool CompatibilitySection { get; set; }
    public required GfxState GraphicsState { get; set; }
    public required List<SubPath> Paths { get; set; }
    public IPdfOperation Closing { get; set; }

    public void Write(ContentWriter writer)
    {
        foreach (var subPath in Paths)
        {
            writer.SubPath(subPath);
        }
        writer.Op(Closing);
    }
}

internal class SubPath
{
    public required float XPos { get; set; }
    public required float YPos { get; set; }
    public required List<IPdfOperation> Operations { get; set; }
    public bool Closed { get; set; }
}
