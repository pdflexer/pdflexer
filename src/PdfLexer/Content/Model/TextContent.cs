using PdfLexer.Fonts;
using PdfLexer.Writing;
using System.Numerics;
using System.Text;
using PdfLexer.Content;

namespace PdfLexer.Content.Model;


/// <summary>
/// A sequence of write operations beginning with
/// a set initial line matrix.
/// 
/// Can be used to represent paragraphs but when 
/// parsed from existing pdfs a paragraph will 
/// likely be broken into multiple TextContent
/// sections.
/// </summary>
public class TextContent<T> : IContentGroup<T> where T : struct, IFloatingPoint<T>
{
    public ContentType Type { get; } = ContentType.Text;
    public GfxState<T> GraphicsState { get => Segments[0].GraphicsState; }
    public bool CompatibilitySection { get => Segments[0].CompatibilitySection; }
    public required List<TextSegment<T>> Segments { get; set; }
    public required GfxMatrix<T> LineMatrix { get; set; }

    public string Text
    {
        get
        {
            var sb = new StringBuilder();
            foreach (var seg in Segments)
            {
                if (seg.NewLine)
                {
                    sb.Append('\n');
                }
                foreach (var g in seg.Glyphs)
                {
                    if (g.Glyph != null)
                    {
                        if (g.Glyph.MultiChar != null)
                        {
                            sb.Append(g.Glyph.MultiChar);
                        }
                        else
                        {
                            sb.Append(g.Glyph.Char);
                        }
                    }
                }
            }
            return sb.ToString();
        }
    }

    public static TextContent<T> Create(string text, IWritableFont font, T fontSize, PdfPoint<T>? location = null)
    {
        var glyphs = new List<GlyphOrShift<T>>();
        foreach (var g in font.GetGlyphs(text))
        {
             if (g.Glyph != null) 
             {
                 glyphs.Add(new GlyphOrShift<T>(g.Glyph, T.Zero, g.Bytes));
             }
             else
             {
                 glyphs.Add(new GlyphOrShift<T>(FPC<T>.Util.FromDouble<T>(g.Shift)));
             }
        }
        
        var gs = new GfxState<T>
        {
             FontSize = fontSize,
             FontResourceName = new PdfName("F" + Guid.NewGuid().ToString("N").Substring(0, 6)), // Placeholder name?
             // We need to associate the font object. 
             // But GfxState holds IReadableFont/IWritableFont?
             FontObject = font.GetPdfFont(),
             Text = new TxtState<T>
             {
                  TextLineMatrix = location.HasValue ? new GfxMatrix<T>(T.One, T.Zero, T.Zero, T.One, location.Value.X, location.Value.Y) : GfxMatrix<T>.Identity,
                  TextMatrix = location.HasValue ? new GfxMatrix<T>(T.One, T.Zero, T.Zero, T.One, location.Value.X, location.Value.Y) : GfxMatrix<T>.Identity
             }
        };
        gs.UpdateTRM();

        return new TextContent<T>
        {
            LineMatrix = gs.Text.TextLineMatrix,
            Segments = new List<TextSegment<T>>
            {
                new TextSegment<T>
                {
                    GraphicsState = gs,
                    Glyphs = glyphs
                }
            }
        };
    }


    public void Write(ContentWriter<T> writer)
    {
        writer.SetTextAndLinePosition(LineMatrix);
        var lm = LineMatrix;
        for (var i = 0; i < Segments.Count; i++)
        {
            var reset = false;
            var group = Segments[i];
            if (i != 0)
            {
                writer.ReconcileCompatibility(group.CompatibilitySection);

                writer.SetGS(group.GraphicsState);
                if (writer.State != PageState.Text)
                {
                    writer.BeginText();
                    reset = true;
                }
                if (lm != writer.GS.Text.TextMatrix) // in case GFX state reset
                {
                    writer.TextTransform(lm);
                    reset = true;
                }
            }
            group.Write(writer, i == 0 || reset);
            lm = writer.GS.Text.TextMatrix;
        }
    }

    public PdfRect<T> GetBoundingBox()
    {
        bool triggered = false;
        T xmin = default;
        T xmax = default;
        T ymin = default;
        T ymax = default;

        foreach (var rect in GetGlyphBoundingBoxes())
        {
            if (triggered)
            {
                xmin = T.Min(xmin, rect.LLx);
                ymin = T.Min(ymin, rect.LLy);
                xmax = T.Max(xmax, rect.URx);
                ymax = T.Max(ymax, rect.URy);
            } else
            {
                xmin = rect.LLx;
                ymin = rect.LLy;
                xmax = rect.URx;
                ymax = rect.URy;
            }
            
            triggered = true;
        }
        if (!triggered)
        {
            return new PdfRect<T> { LLx = T.Zero, LLy = T.Zero, URx = T.Zero, URy = T.Zero };
        }
        return new PdfRect<T> { LLx = xmin, LLy = ymin, URx = xmax, URy = ymax };
        // return GraphicsState.CTM.GetTransformedBoundingBox(new PdfRect<T> { LLx = xmin, LLy = ymin, URx = xmax, URy = ymax });
    }

    public IEnumerable<PdfRect<T>> GetGlyphBoundingBoxes()
    {
        var gfx = GraphicsState with { Text = new TxtState<T> { TextLineMatrix = LineMatrix, TextMatrix = LineMatrix } };
        foreach (var seg in Segments)
        {
            gfx = seg.GraphicsState with { Text = gfx.Text };
            if (seg.NewLine)
            {
                T_Star_Op<T>.Value.Apply(ref gfx);
            }
            else
            {
                gfx.UpdateTRM();
            }
            GlyphOrShift<T> prev = default;
            foreach (var glyph in seg.Glyphs)
            {
                if (prev.Glyph != null)
                {
                    gfx.ApplyCharShift(prev);
                }
                else if (prev.Shift != T.Zero)
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

    public IEnumerable<CharPos<T>> EnumerateCharacters()
    {
        var gfx = GraphicsState with { Text = new TxtState<T> { TextLineMatrix = LineMatrix, TextMatrix = LineMatrix } };
        foreach (var seg in Segments)
        {
            gfx = seg.GraphicsState with { Text = gfx.Text };
            if (seg.NewLine)
            {
                T_Star_Op<T>.Value.Apply(ref gfx);
            }
            else
            {
                gfx.UpdateTRM();
            }
            GlyphOrShift<T> prev = default;
            foreach (var glyph in seg.Glyphs)
            {
                if (prev.Glyph != null)
                {
                    gfx.ApplyCharShift(prev);
                }
                else if (prev.Shift != T.Zero)
                {
                    gfx.ApplyTj(prev.Shift);
                }
                prev = glyph;
                if (glyph.Glyph != null)
                {
                    var x = GraphicsState.Text.TextRenderingMatrix.E;
                    var y = GraphicsState.Text.TextRenderingMatrix.F;
                    if (glyph.Glyph.MultiChar != null)
                    {
                        foreach (var c in glyph.Glyph.MultiChar)
                        {
                            yield return new CharPos<T>
                            {
                                Char = c,
                                XPos = x,
                                YPos = y
                            };
                        }
                    }
                    else
                    {
                        yield return new CharPos<T>
                        {
                            Char = glyph.Glyph.Char,
                            XPos = x,
                            YPos = y
                        };
                    }
                }
            }
        }
    }

    public void Transform(GfxMatrix<T> transformation)
    {
        foreach (var item in Segments)
        {
            item.GraphicsState = item.GraphicsState with { 
                CTM = transformation * item.GraphicsState.CTM,
                Clipping = item.GraphicsState.Clipping == null ? null :
                    item.GraphicsState.Clipping.Select(x=> { var c = x.ShallowClone(); c.TM = transformation * c.TM; return c; }).ToList()

            };
        }
    }

    public void TransformInitial(GfxMatrix<T> transformation)
    {
        foreach (var item in Segments)
        {
            item.GraphicsState = item.GraphicsState with
            {
                CTM = item.GraphicsState.CTM * transformation,
                Clipping = item.GraphicsState.Clipping == null ? null :
                     item.GraphicsState.Clipping.Select(x => { var c = x.ShallowClone(); c.TM = c.TM * transformation; return c; }).ToList()

            };
        }

    }

    public void ClipExcept(PdfRect<T> rect)
    {
        foreach (var item in Segments)
        {
            item.GraphicsState = item.GraphicsState.ClipExcept(rect);
        }
    }

    public TextContent<T>? CopyArea(PdfRect<T> rect) => SplitInternal(rect, true, false).Inside;
    public (TextContent<T>? Inside, TextContent<T>? Outside) Split(PdfRect<T> rect) => SplitInternal(rect, true, true);

    public (TextContent<T>? Inside, TextContent<T>? Outside) SplitInternal(PdfRect<T> rect, bool trackInside, bool trackOutside)
    {
        var gfx = GraphicsState with { Text = new TxtState<T> { TextLineMatrix = LineMatrix, TextMatrix = LineMatrix } };

        var inside = trackInside ? new TextContent<T>
        {
            LineMatrix = LineMatrix,
            Segments = new List<TextSegment<T>> { }
        } : null;
        var outside = trackOutside ? new TextContent<T>
        {
            LineMatrix = LineMatrix,
            Segments = new List<TextSegment<T>> { }
        } : null;

        foreach (var seg in Segments)
        {
            gfx = seg.GraphicsState with { Text = gfx.Text };
            if (seg.NewLine)
            {
                T_Star_Op<T>.Value.Apply(ref gfx);
            }
            else
            {
                gfx.UpdateTRM();
            }

            var ci = trackInside ? GetInside(seg, gfx.Text) : null;

            var co = trackOutside ? GetOutside(seg, gfx.Text) : null;

            T bbx1 = default;
            T bbx2 = default;
            T bby1 = default;
            T bby2 = default;

            GlyphOrShift<T> prev = default;
            bool hadInside = false;
            bool hadOutside = false;
            T skippedInside = T.Zero;
            T skippedOutside = T.Zero;
            foreach (var glyph in seg.Glyphs)
            {
                gfx.Apply(prev);
                prev = glyph;
                if (glyph.Glyph != null)
                {
                    var bb = gfx.GetGlyphBoundingBox(glyph.Glyph);
                    var info = rect.CheckEnclosure(bb);

                    if (ci != null)
                    {
                        if (info == EncloseType.Full || info == EncloseType.Partial)
                        {
                            hadInside = true;
                            if (skippedInside != T.Zero)
                            {
                                ci.Glyphs.Add(new GlyphOrShift<T>(-skippedInside));
                                skippedInside = T.Zero;
                            }
                            ci.Glyphs.Add(glyph);
                        }
                        else
                        {

                            skippedInside += FPC<T>.V1000 * 
                                FPC<T>.Util.FromDouble<T>((double)((gfx.Font?.IsVertical ?? false) ? glyph.Glyph.w1 : glyph.Glyph.w0));
                            var cw = gfx.CharSpacing;
                            if (glyph.Glyph.IsWordSpace) { cw += gfx.WordSpacing; }
                            skippedInside += FPC<T>.V1000 * (cw / gfx.FontSize);
                        }
                    }

                    if (co != null)
                    {
                        if (info == EncloseType.None || info == EncloseType.Partial)
                        {
                            if (!hadOutside)
                            {
                                bbx1 = bb.LLx;
                                bbx2 = bb.URx;
                                bby1 = bb.LLy;
                                bby2 = bb.URy;
                            } else
                            {
                                bbx1 = T.Min(bbx1, bb.LLx);
                                bbx2 = T.Max(bbx2, bb.URx);
                                bby1 = T.Min(bby1, bb.LLy);
                                bby2 = T.Max(bby2, bb.URy);
                            }
                            hadOutside = true;
                            if (skippedOutside != T.Zero)
                            {
                                co.Glyphs.Add(new GlyphOrShift<T>(-skippedOutside));
                                skippedOutside = T.Zero;
                            }
                            co.Glyphs.Add(glyph);
                        }
                        else
                        {
                            skippedOutside += FPC<T>.V1000 *
                                FPC<T>.Util.FromDouble<T>((double)((gfx.Font?.IsVertical ?? false) ? glyph.Glyph.w1 : glyph.Glyph.w0));
                            var cw = gfx.CharSpacing;
                            if (glyph.Glyph.IsWordSpace) { cw += gfx.WordSpacing; }
                            skippedOutside += FPC<T>.V1000 * (cw / gfx.FontSize);
                        }
                    }

                }
                else if (glyph.Shift != T.Zero)
                {
                    skippedInside += -glyph.Shift;
                    skippedOutside += -glyph.Shift;
                }
            }
            if (hadInside)
            {
                inside!.Segments.Add(ci!);
            }
            if (hadOutside)
            {
                co!.GraphicsState = co!.GraphicsState.Clip(rect, 
                    new PdfRect<T> { LLx = bbx1 - T.One, LLy = bby1 - T.One, URx = bbx1 + T.One, URy = bby1 + T.One });
                outside!.Segments.Add(co!);
            }
        }
        return (inside?.Segments.Count > 0 ? inside : null, outside?.Segments.Count > 0 ? outside : null);


        TextSegment<T> GetInside(TextSegment<T> seg, TxtState<T> current)
        {
            var ci = new TextSegment<T>
            {
                GraphicsState = seg.GraphicsState with
                {
                    Text = new TxtState<T>
                    {
                        TextLineMatrix = current.TextMatrix,
                        TextMatrix = current.TextMatrix
                    },
                    Clipping = seg.GraphicsState.Clipping.ClipExcept(rect)
                },
                Glyphs = new List<GlyphOrShift<T>> { }
            };
            return ci;
        }

        TextSegment<T> GetOutside(TextSegment<T> seg, TxtState<T> current)
        {
            var co = new TextSegment<T>
            {
                GraphicsState = seg.GraphicsState with
                {
                    Text = new TxtState<T>
                    {
                        TextLineMatrix = current.TextMatrix,
                        TextMatrix = current.TextMatrix
                    }
                },
                Glyphs = new List<GlyphOrShift<T>> { }
            };
            return co;
        }
    }

    IContentGroup<T>? IContentGroup<T>.CopyArea(PdfRect<T> rect) => CopyArea(rect);

    (IContentGroup<T>? Inside, IContentGroup<T>? Outside) IContentGroup<T>.Split(PdfRect<T> rect) => Split(rect);

    public void ClipFrom(GfxState<T> other)
    {
        if (other.Clipping == null) { return; }
        foreach (var seg in Segments)
        {
            if (seg.GraphicsState.Clipping != null)
            {
                seg.GraphicsState = seg.GraphicsState with { Clipping = seg.GraphicsState.Clipping.ToList() };
            }
            else
            {
                seg.GraphicsState = seg.GraphicsState with { Clipping = new List<IClippingSection<T>>() };
            }
            seg.GraphicsState.Clipping.AddRange(other.Clipping);
        }
    }
}



/// <summary>
/// A sequence of glyphs
/// </summary>
public record class TextSegment<T> where T : struct, IFloatingPoint<T> //: IContentGroup<T> 
{
    public ContentType Type { get; } = ContentType.Text;
    public required GfxState<T> GraphicsState { get; set; }

    public bool CompatibilitySection { get; set; }
    public required List<GlyphOrShift<T>> Glyphs { get; set; }
    public bool NewLine { get; set; }

    public void Write(ContentWriter<T> writer, bool lineReset)
    {
        if (!lineReset && NewLine)
        {
            writer.Op(T_Star_Op<T>.Value);
        }
        writer.WriteGlyphs(Glyphs);
    }
}
