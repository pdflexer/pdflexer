using PdfLexer.Fonts;
using PdfLexer.Writing;

namespace PdfLexer.Content.Model;


/// <summary>
/// A sequence of glyphs
/// </summary>
internal record class TextSegment : IContentGroup
{
    public ContentType Type { get; } = ContentType.Text;
    public required GfxState GraphicsState { get; set; }
    public List<MarkedContent>? Markings { get; set; }
    public bool CompatibilitySection { get; set; }
    public required List<UnappliedGlyph> Glyphs { get; set; }
    public bool NewLine { get; set; }

    public void Write(ContentWriter writer)
    {
        if (NewLine)
        {
            writer.Op(T_Star_Op.Value);
        }
        writer.WriteGlyphs(Glyphs);
    }

    public PdfRect GetBoundingBox()
    {
        throw new NotSupportedException();
    }
}


/// <summary>
/// A sequence of glyphs
/// </summary>
internal class TextSequence : IContentGroup
{
    public ContentType Type { get; } = ContentType.Text;
    public GfxState GraphicsState { get => Segments[0].GraphicsState; }
    public List<MarkedContent>? Markings { get => Segments[0].Markings; }
    public bool CompatibilitySection { get => Segments[0].CompatibilitySection; }
    public required List<TextSegment> Segments { get; set; }
    public required GfxMatrix LineMatrix { get; set; }

    public void Write(ContentWriter writer)
    {
        writer.SetLinePosition(LineMatrix);
        var lm = LineMatrix;
        for (var i = 0; i < Segments.Count; i++)
        {
            var group = Segments[i];
            if (i != 0)
            {
                writer.ReconcileCompatibility(group.CompatibilitySection);
                writer.ReconcileMC(group.Markings);
                writer.SetGS(group.GraphicsState);
                if (writer.State != PageState.Text)
                {
                    writer.BeginText();
                }
                if (lm != writer.GS.Text.TextMatrix) // in case GFX state reset
                {
                    writer.TextTransform(lm);
                }
            }
            group.Write(writer);
            lm = writer.GS.Text.TextMatrix;
        }
    }

    public PdfRect GetBoundingBox()
    {
        return new PdfRect { LLx = 0, LLy = 0, URx = 0, URy = 0, };
    }
}