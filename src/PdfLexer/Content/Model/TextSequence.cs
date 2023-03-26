using PdfLexer.Fonts;
using PdfLexer.Serializers;
using PdfLexer.Writing;
using System.Buffers.Text;
using System.Numerics;

namespace PdfLexer.Content.Model;

/// <summary>
/// A sequence of glyphs
/// </summary>
internal class TextLineSequence : IContentGroup
{
    public ContentType Type { get; } = ContentType.Text;
    public required GfxState GraphicsState { get; set; }
    public List<MarkedContent>? Markings { get; set; }
    public bool CompatibilitySection { get; set; }

    public required List<UnappliedGlyph> Glyphs { get; set; }
    public Matrix4x4? LineMatrix { get; set; }

    public void Write(ContentWriter writer)
    {
        if (LineMatrix.HasValue)
        {
            writer.SetLinePosition(LineMatrix.Value);
        }
        writer.WriteGlyphs(Glyphs);
    }
}
