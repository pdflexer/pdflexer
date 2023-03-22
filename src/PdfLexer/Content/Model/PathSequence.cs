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
            if (subPath.Operations.Count > 0 && subPath.Operations[0].Type != PdfOperatorType.re)
            {
                writer.MoveTo((decimal)subPath.XPos, (decimal)subPath.YPos);
            }
            foreach (var op in subPath.Operations)
            {
                writer.Op(op);
            }
            if (subPath.Closed)
            {
                writer.Op(h_Op.Value);
            }
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
