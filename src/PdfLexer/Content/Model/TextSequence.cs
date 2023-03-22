using PdfLexer.Fonts;
using PdfLexer.Serializers;
using PdfLexer.Writing;
using System.Buffers.Text;
using System.Numerics;

namespace PdfLexer.Content.Model;

/// <summary>
/// A sequence of glyphs
/// </summary>
internal class TextSequence : IContentGroup
{
    public ContentType Type { get; } = ContentType.Text;
    public required GfxState GraphicsState { get; set; }
    public required Matrix4x4 LineMatrix { get; set; }
    public required List<UnappliedGlyph> Glyphs { get; set; }
    public List<MarkedContent>? Markings { get; set; }
    public bool CompatibilitySection { get; set; }

    public void Write(ContentWriter writer)
    {
        writer.SetGS(GraphicsState);
        writer.SetLinePosition(LineMatrix);
        writer.WriteGlyphs(Glyphs);
    }
}