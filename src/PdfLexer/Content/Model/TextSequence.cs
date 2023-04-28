using PdfLexer.Fonts;
using PdfLexer.Writing;
using System.Text.RegularExpressions;

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
internal class TextContent : IContentGroup
{
    public ContentType Type { get; } = ContentType.Text;
    public GfxState GraphicsState { get => Segments[0].GraphicsState; }
    public List<MarkedContent>? Markings { get => Segments[0].Markings; }
    public bool CompatibilitySection { get => Segments[0].CompatibilitySection; }
    public required List<TextSegment> Segments { get; set; }
    public required GfxMatrix LineMatrix { get; set; }

    public void Write(ContentWriter writer)
    {
        writer.SetTextAndLinePosition(LineMatrix);
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

        foreach (var rect in GetGlyphBoundingBoxes())
        {
            triggered = true;
            xmin = Math.Min(xmin, rect.LLx);
            ymin = Math.Min(ymin, rect.LLy);
            xmax = Math.Max(xmax, rect.URx);
            ymax = Math.Max(ymax, rect.URy);
        }
        if (!triggered)
        {
            return new PdfRect { LLx = 0, LLy = 0, URx = 0, URy = 0 };
        }
        return GraphicsState.CTM.GetTransformedBoundingBox(new PdfRect { LLx = xmin, LLy = ymin, URx = xmax, URy = ymax });
    }

    public IEnumerable<PdfRect> GetGlyphBoundingBoxes()
    {
        var gfx = GraphicsState with { Text = new TxtState { TextLineMatrix = LineMatrix, TextMatrix = LineMatrix } };
        foreach (var seg in Segments)
        {
            gfx = seg.GraphicsState with { Text = gfx.Text };
            if (seg.NewLine)
            {
                T_Star_Op.Value.Apply(ref gfx);
            }
            else
            {
                gfx.UpdateTRM();
            }
            GlyphOrShift prev = default;
            foreach (var glyph in seg.Glyphs)
            {
                if (prev.Glyph != null)
                {
                    gfx.ApplyCharShift(prev);
                }
                else if (prev.Shift != 0)
                {
                    gfx.ApplyTj(prev.Shift);
                }
                prev = glyph;
                if (glyph.Glyph != null)
                {
                    yield return gfx.GetGlyphBoundingBox(glyph.Glyph);
                }
            }
        }
    }

    public (TextContent? Inside, TextContent? Outside) Split(PdfRect rect)
    {
        var gfx = GraphicsState with { Text = new TxtState { TextLineMatrix = LineMatrix, TextMatrix = LineMatrix } };

        var inside = new TextContent
        {
            LineMatrix = LineMatrix,
            Segments = new List<TextSegment> { }
        };
        var outside = new TextContent
        {
            LineMatrix = LineMatrix,
            Segments = new List<TextSegment> { }
        };

        foreach (var seg in Segments)
        {
            gfx = seg.GraphicsState with { Text = gfx.Text };
            if (seg.NewLine)
            {
                T_Star_Op.Value.Apply(ref gfx);
            }
            else
            {
                gfx.UpdateTRM();
            }

            var ci = GetInside(seg, gfx.Text);

            var co = GetOutside(seg, gfx.Text);

            GlyphOrShift prev = default;
            bool skippedInside = false;
            bool skippedOutside = false;
            foreach (var glyph in seg.Glyphs)
            {
                gfx.Apply(prev);
                if (!skippedInside)
                {
                    ci.GraphicsState.Apply(prev);
                }
                prev = glyph;
                if (glyph.Glyph != null)
                {
                    var bb = gfx.GetGlyphBoundingBox(glyph.Glyph);
                    var info = rect.CheckEnclosure(bb);

                    if (info == EncloseType.Full || info == EncloseType.Partial)
                    {
                        if (skippedInside)
                        {
                            if (ci.Glyphs.Count > 0)
                            {
                                inside.Segments.Add(ci);
                            }
                            ci = GetInside(seg, gfx.Text);
                        }
                        ci.Glyphs.Add(glyph);
                    }
                    else
                    {
                        skippedInside = true;
                    }
                    if (info == EncloseType.None || info == EncloseType.Partial)
                    {
                        if (skippedOutside)
                        {
                            if (co.Glyphs.Count > 0)
                            {
                                outside.Segments.Add(co);
                            }
                            co = GetOutside(seg, gfx.Text);
                        }
                        co.Glyphs.Add(glyph);
                    }
                    else
                    {
                        skippedOutside = true;
                    }
                }
                else if (glyph.Shift != 0)
                {
                    if (!skippedInside)
                    {
                        ci.Glyphs.Add(glyph);
                    }
                    if (!skippedOutside)
                    {
                        co.Glyphs.Add(glyph);
                    }
                }
            }
            if (ci.Glyphs.Count > 0)
            {
                inside.Segments.Add(ci);
            }
            if (co.Glyphs.Count > 0)
            {
                outside.Segments.Add(co);
            }
        }
        return (inside.Segments.Count > 0 ? inside : null, outside.Segments.Count > 0 ? outside : null);


        TextSegment GetInside(TextSegment seg, TxtState current)
        {
            var ci = new TextSegment
            {
                GraphicsState = seg.GraphicsState with
                {
                    Text = new TxtState
                    {
                        TextLineMatrix = current.TextMatrix,
                        TextMatrix = current.TextMatrix
                    },
                    Clipping = GraphicsState.Clipping == null ? new List<IClippingSection>() : GraphicsState.Clipping.ToList()
                },
                Glyphs = new List<GlyphOrShift> { }
            };
            ci.GraphicsState.Clipping.Add(new ClippingInfo(GfxMatrix.Identity, new List<SubPath> {
                    new SubPath
                    {
                        XPos = 0,
                        YPos = 0,
                        Operations = new List<IPathCreatingOp>
                            {
                                new re_Op(rect.LLx, rect.LLy, rect.URx-rect.LLx, rect.URy - rect.LLy),
                                new re_Op(rect.LLx-50, rect.LLy-50, rect.URx-rect.LLx+50, rect.URy - rect.LLy+50)

                            }
                    }
            }, false));
            return ci;
        }

        TextSegment GetOutside(TextSegment seg, TxtState current)
        {
            var co = new TextSegment
            {
                GraphicsState = seg.GraphicsState with
                {
                    Text = new TxtState
                    {
                        TextLineMatrix = current.TextMatrix,
                        TextMatrix = current.TextMatrix
                    },
                    Clipping = GraphicsState.Clipping == null ? new List<IClippingSection>() : GraphicsState.Clipping.ToList()
                },
                Glyphs = new List<GlyphOrShift> { }
            };
            // co.GraphicsState.Clipping.Add(new ClippingInfo(GfxMatrix.Identity, new List<SubPath> {
            //         new SubPath
            //         {
            //             XPos = rect.LLx,
            //             YPos = rect.LLy,
            //             Operations = new List<IPathCreatingOp>
            //                 {
            //                     new re_Op(rect.LLx, rect.LLy, rect.URx-rect.LLx, rect.URy - rect.LLy)
            //                 }
            //         }
            // }, false));
            return co;
        }
    }
}

