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
    public required List<GlyphOrShift> Glyphs { get; set; }
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
        bool triggered = false;
        var xmin = decimal.MaxValue;
        var xmax = decimal.MinValue;
        var ymin = decimal.MaxValue;
        var ymax = decimal.MinValue;
        var gfx = GraphicsState with { Text = new TxtState {  TextLineMatrix = LineMatrix, TextMatrix = LineMatrix } };
        foreach (var seg in Segments)
        {
            gfx = seg.GraphicsState with { Text = gfx.Text };
            if (seg.NewLine)
            {
                T_Star_Op.Value.Apply(ref gfx);
            } else
            {
                gfx.UpdateTRM();
            }
            GlyphOrShift prev = default;
            foreach (var glyph in seg.Glyphs)
            {
                if (prev.Glyph != null)
                {
                    gfx.ApplyCharShift(prev);
                } else if (prev.Shift != 0)
                {
                    gfx.ApplyTj(prev.Shift);
                }
                prev = glyph;
                if (glyph.Glyph != null)
                {
                    var (x,y,x2,y2) = gfx.GetBoundingBox(glyph.Glyph);
                    xmin = Math.Min(xmin, x);
                    ymin = Math.Min(ymin, y);
                    xmax = Math.Max(xmax, x2);
                    ymax = Math.Max(ymax, y2);
                    triggered = true;
                }
            }
        }
        if (!triggered)
        {
            return new PdfRect { LLx = 0, LLy = 0, URx = 0, URy = 0 };
        }
        return GraphicsState.CTM.GetTransformedBoundingBox(new PdfRect { LLx = xmin, LLy = ymin, URx = xmax, URy = ymax });
    }
}