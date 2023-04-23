using PdfLexer.DOM;
using PdfLexer.Fonts;

namespace PdfLexer.Content.Model;

internal enum ParseState
{
    Page,
    Text,
    Paths
}
internal ref struct ContentModelParser
{
    private ParsingContext Context;
    private readonly List<TJ_Lazy_Item> TJCache;

    internal PageContentScanner2 Scanner;

    /// <summary>
    /// Current graphics state
    /// </summary>
    public GfxState GraphicsState;


    public ContentModelParser(ParsingContext ctx, PdfDictionary page, bool flattenForms = false)
    {
        Context = ctx;
        Scanner = new PageContentScanner2(ctx, page, flattenForms);
        GraphicsState = new GfxState();
        TJCache = new List<TJ_Lazy_Item>(10);
    }

    public ContentModelParser(ParsingContext ctx, PdfDictionary page, PdfStream form, GfxState state)
    {
        Context = ctx;
        Scanner = new PageContentScanner2(ctx, page, form);
        GraphicsState = state;
        TJCache = new List<TJ_Lazy_Item>(10);
    }

    public List<IContentGroup> Parse()
    {
        var mc = new List<MarkedContent>();
        var bx = false;
        var content = new List<IContentGroup> { };

        // BX -> EX compatibility section
        // BDC / BMC -> EMC marked-content
        // MP marked-content point

        ParseState state = ParseState.Page;
        PathSequence? currentPath = null;


        // we trim some data from ext graphics but want to keep
        // incoming objects that are the same to have the same outgoing object
        // for GS deduping
        var extGraphics = new Dictionary<PdfDictionary, PdfDictionary>();

        SubPath? currentSubPath = null;
        PdfOperatorType? clipping = null;
        List<TextSequence>? textClipping = null;
        TextSequence? currentText = null;

        while (Scanner.Advance())
        {
            var nxt = Scanner.CurrentOperator;

            switch (nxt)
            {
                case PdfOperatorType.MP: // marked content points TODO
                case PdfOperatorType.DP:
                    {
                        continue;
                    }
                case PdfOperatorType.BMC:
                    {
                        CompleteCurrent(GraphicsState, false);
                        continue;
                    }
                case PdfOperatorType.BDC:
                    {
                        CompleteCurrent(GraphicsState);

                        if (!Scanner.TryGetCurrentOperation(out var bdc))
                        {
                            continue;
                        }
                        var bdcOp = (BDC_Op)bdc;
                        if (bdcOp.props is PdfName nm)
                        {
                            if (!Scanner.TryGetPropertyList(nm, out var found))
                            {
                                // turn into BMC
                                mc.Add(new MarkedContent(bdcOp.tag));
                            }
                            else
                            {
                                mc.Add(new MarkedContent(bdcOp.tag) { PropList = found });
                            }
                        }
                        else if (bdcOp.props is PdfDictionary dict)
                        {
                            mc.Add(new MarkedContent(bdcOp.tag) { InlineProps = dict });
                        }
                        else
                        {
                            mc.Add(new MarkedContent(bdcOp.tag));
                        }
                        continue;
                    }
                case PdfOperatorType.EMC:
                    {
                        CompleteCurrent(GraphicsState);
                        if (mc.Count > 0)
                        {
                            mc.RemoveAt(mc.Count - 1);
                        }
                        continue;
                    }
                case PdfOperatorType.BX:
                    {
                        CompleteCurrent(GraphicsState);
                        bx = true;
                        continue;
                    }
                case PdfOperatorType.EX:
                    {
                        CompleteCurrent(GraphicsState);
                        bx = false;
                        continue;
                    }
                case PdfOperatorType.sh:
                    {
                        if (!Scanner.TryGetCurrentOperation(out var shOpI))
                        {
                            continue;
                        }
                        var shOp = (sh_Op)shOpI;
                        if (!Scanner.TryGetShading(shOp.name, out var shObj))
                        {
                            continue;
                        }
                        content.Add(new ShadingContent
                        {
                            Shading = shObj,
                            GraphicsState = GraphicsState,
                            CompatibilitySection = bx,
                            Markings = mc.Count > 0 ? mc.ToList() : null
                        });
                        break;
                    }
                case PdfOperatorType.Do:
                    if (!Scanner.TryGetCurrentOperation(out var doOpI))
                    {
                        continue;
                    }
                    var doOp = (Do_Op)doOpI;
                    if (!Scanner.TryGetXObject(doOp.name, out var obj, out var isForm))
                    {
                        continue;
                    }
                    if (isForm)
                    {
                        content.Add(new XFormContent
                        {
                            Stream = obj,
                            GraphicsState = GraphicsState,
                            CompatibilitySection = bx,
                            Markings = mc.Count > 0 ? mc.ToList() : null
                        });
                    }
                    else
                    {
                        content.Add(new XImgContent
                        {
                            Stream = obj,
                            GraphicsState = GraphicsState,
                            CompatibilitySection = bx,
                            Markings = mc.Count > 0 ? mc.ToList() : null
                        });
                    }
                    continue;
                case PdfOperatorType.EI:
                    {
                        if (!Scanner.TryGetCurrentOperation(out var iiop))
                        {
                            continue;
                        }
                        var img = (InlineImage_Op)iiop;
                        var ximg = img.ConvertToStream(Scanner.Resources);
                        content.Add(new XImgContent
                        {
                            Stream = ximg,
                            GraphicsState = GraphicsState,
                            CompatibilitySection = bx,
                            Markings = mc.Count > 0 ? mc.ToList() : null
                        });
                        continue;
                    }
                case PdfOperatorType.EOC:
                    return content;
                case PdfOperatorType.BT:
                    {
                        state = ParseState.Text;
                        BT_Op.Value.Apply(ref GraphicsState);
                        currentText = null;
                        var clip = GraphicsState.TextMode == 4
                                    || GraphicsState.TextMode == 5
                                    || GraphicsState.TextMode == 6
                                    || GraphicsState.TextMode == 7;
                        if (clip)
                        {
                            textClipping = new List<TextSequence>();
                        }
                        else
                        {
                            textClipping = null;
                        }
                    }
                    continue;
                case PdfOperatorType.Tr:
                    {
                        if (Scanner.TryGetCurrentOperation(out var trOp))
                        {
                            var prev = GraphicsState.TextMode;
                            trOp.Apply(ref GraphicsState);
                            var clip = GraphicsState.TextMode == 4
                                || GraphicsState.TextMode == 5
                                || GraphicsState.TextMode == 6
                                || GraphicsState.TextMode == 7;
                            if (clip)
                            {
                                textClipping = new List<TextSequence>();
                            }
                            else
                            {
                                textClipping = null;
                            }
                            if ((prev == 7 || GraphicsState.TextMode == 7) && prev != GraphicsState.TextMode)
                            {
                                currentText = null;
                            }
                        }
                    }
                    continue;
                case PdfOperatorType.ET:
                    CompleteCurrent(GraphicsState);
                    state = ParseState.Page;
                    currentText = null;
                    if (textClipping != null)
                    {
                        var clip = GraphicsState.Clipping;
                        if (clip == null)
                        {
                            clip = new List<IClippingSection>();
                        }
                        else
                        {
                            clip = clip.ToList();
                        }
                        clip.AddRange(textClipping.Select(x => new TextClippingInfo
                        {
                            Glyphs = x.Segments[0].Glyphs.ToList(),
                            TM = x.GraphicsState.CTM,
                            LineMatrix = x.LineMatrix,
                        }));
                        GraphicsState = GraphicsState with
                        {
                            Clipping = clip
                        };
                    }
                    continue;
                case PdfOperatorType.Tf:
                    {
                        if (Scanner.TryGetCurrentOperation(out var op))
                        {
                            var tfOp = (Tf_Op)op;
                            IReadableFont? rf;
                            if (!Scanner.TryGetFont(tfOp.font, out var font))
                            {
                                rf = SingleByteFont.Fallback;
                                font = Standard14Font.GetHelvetica().GetPdfFont();
                            }
                            else
                            {
                                // resources may not be included
                                var ft = font.Get(PdfName.Subtype);
                                if (ft != null && (ft as PdfName) == PdfName.Type3 && !font.ContainsKey(PdfName.Resources))
                                {
                                    font = font.CloneShallow();
                                    font[PdfName.Resources] = Scanner.Resources.CloneShallow();
                                }
                                rf = Context.GetFont(font);

                            }
                            Tf_Op.Apply(ref GraphicsState, font, rf, tfOp.size);
                        }
                    }
                    continue;
                case PdfOperatorType.gs:
                    {
                        if (!Scanner.TryGetCurrentOperation(out var gs))
                        {
                            // TODO warn
                            continue;
                        }

                        CompleteCurrent(GraphicsState);

                        var gso = (gs_Op)gs;
                        if (!Scanner.TryGetGraphicsState(gso.name, out var gsd))
                        {
                            continue;
                        }
                        gso.Apply(ref GraphicsState, gsd, Scanner.Resources, Context, extGraphics);

                        continue;
                    }
                case PdfOperatorType.CS:
                case PdfOperatorType.cs:
                    {
                        if (!Scanner.TryGetCurrentOperation(out var gso))
                        {
                            // TODO Warn
                            continue;
                        }

                        CompleteCurrent(GraphicsState);

                        if (nxt == PdfOperatorType.CS)
                        {
                            var nm = ((CS_Op)gso).name;
                            if (Scanner.TryGetColorSpace(nm, out var cs))
                            {
                                GraphicsState = GraphicsState with { ColorSpaceStroking = cs };
                            }
                            else
                            {
                                GraphicsState = GraphicsState with { ColorSpaceStroking = nm };
                            }
                        }
                        else
                        {
                            var nm = ((cs_Op)gso).name;
                            if (Scanner.TryGetColorSpace(nm, out var cs))
                            {
                                GraphicsState = GraphicsState with { ColorSpace = cs };
                            }
                            else
                            {
                                GraphicsState = GraphicsState with { ColorSpace = nm };
                            }
                        }

                        continue;
                    }
                case PdfOperatorType.d: // non-text gs ops
                case PdfOperatorType.G:
                case PdfOperatorType.g:
                case PdfOperatorType.i:
                case PdfOperatorType.j:
                case PdfOperatorType.K:
                case PdfOperatorType.k:
                case PdfOperatorType.M:
                case PdfOperatorType.RG:
                case PdfOperatorType.rg:
                case PdfOperatorType.ri:
                case PdfOperatorType.SC:
                case PdfOperatorType.sc:
                case PdfOperatorType.w:
                    {
                        CompleteCurrent(GraphicsState, false);

                        if (Scanner.TryGetCurrentOperation(out var gso))
                        {
                            gso.Apply(ref GraphicsState);
                        }

                        continue;
                    }
                case PdfOperatorType.SCN: // may need resources
                case PdfOperatorType.scn:
                    {
                        CompleteCurrent(GraphicsState, false);

                        if (Scanner.TryGetCurrentOperation(out var gso))
                        {
                            if (nxt == PdfOperatorType.SCN)
                            {
                                var scn = (SCN_Op)gso;
                                if (scn.name != null && Scanner.TryGetPattern(scn.name, out var pattern))
                                {
                                    scn.Pattern = pattern;
                                }
                            }
                            else
                            {
                                var scn = (scn_Op)gso;
                                if (scn.name != null && Scanner.TryGetPattern(scn.name, out var pattern))
                                {
                                    scn.Pattern = pattern;
                                }
                            }
                            gso.Apply(ref GraphicsState);
                        }

                        continue;
                    }
                // path creating ops
                case PdfOperatorType.c:
                case PdfOperatorType.l:
                case PdfOperatorType.v:
                case PdfOperatorType.y:
                    {
                        if (Scanner.TryGetCurrentOperation(out var pco))
                        {
                            currentSubPath ??= NewSubPath(0f, 0f, GraphicsState);
                            currentSubPath.Operations.Add(pco);
                        }
                        continue;
                    }
                case PdfOperatorType.h:
                    {
                        if (currentSubPath != null)
                        {
                            currentSubPath.Closed = true;
                        }
                        continue;
                    }
                // subpath starting ops
                case PdfOperatorType.re:
                    {
                        if (state != ParseState.Paths)
                        {
                            state = ParseState.Paths;
                            clipping = null;
                        }
                        if (Scanner.TryGetCurrentOperation(out var gso))
                        {
                            var re = (re_Op)gso;
                            currentSubPath = NewSubPath((float)re.x, (float)re.y, GraphicsState);
                            currentSubPath.Operations.Add(re);
                        }
                        continue;
                    }
                case PdfOperatorType.m:
                    {
                        if (state != ParseState.Paths)
                        {
                            state = ParseState.Paths;
                            clipping = null;
                        }
                        if (Scanner.TryGetCurrentOperation(out var gso))
                        {
                            var m = (m_Op)gso;
                            currentSubPath = NewSubPath((float)m.x, (float)m.y, GraphicsState);
                        }
                        continue;
                    }
                // path drawing ops
                case PdfOperatorType.b:
                case PdfOperatorType.B:
                case PdfOperatorType.b_Star:
                case PdfOperatorType.B_Star:
                case PdfOperatorType.f:
                case PdfOperatorType.F:
                case PdfOperatorType.f_Star:
                case PdfOperatorType.n:
                case PdfOperatorType.s:
                case PdfOperatorType.S:
                    {
                        if (currentPath == null)
                        {
                            continue; // warning
                        }
                        if (Scanner.TryGetCurrentOperation(out var pdo))
                        {
                            currentPath.Closing = pdo;
                        }
                        else
                        {
                            currentPath.Closing = n_Op.Value;
                        }

                        var clip = clipping != null ? new ClippingInfo(GraphicsState.CTM, currentPath.Paths, clipping == PdfOperatorType.W_Star) : null;

                        CompleteCurrent(GraphicsState);

                        if (clip != null)
                        {
                            if (GraphicsState.Clipping != null)
                            {
                                var old = GraphicsState.Clipping.ToList();
                                old.Add(clip);
                                GraphicsState = GraphicsState with { Clipping = old };
                            }
                            else
                            {
                                GraphicsState = GraphicsState with { Clipping = new List<IClippingSection> { clip } };
                            }

                        }

                        currentSubPath = null;
                        clipping = null;
                        continue;
                    }
                case PdfOperatorType.W:
                case PdfOperatorType.W_Star:
                    {
                        if (state != ParseState.Paths)
                        {
                            continue; // warning
                        }
                        clipping = nxt;
                        continue;
                    }
                case PdfOperatorType.Td:
                case PdfOperatorType.TD:
                case PdfOperatorType.Tm:
                case PdfOperatorType.T_Star:
                    {
                        if (Scanner.TryGetCurrentOperation(out var gso))
                        {
                            gso.Apply(ref GraphicsState);
                        }
                        currentText = null;
                        continue;
                    }
                case PdfOperatorType.d0:
                case PdfOperatorType.d1:
                    // should never occur, this doesn't parse t3 font streams
                    continue;
                case PdfOperatorType.q:
                case PdfOperatorType.Q:
                case PdfOperatorType.cm:
                    {
                        if (Scanner.TryGetCurrentOperation(out var gso))
                        {
                            gso.Apply(ref GraphicsState);
                        }
                        currentText = null;
                        continue;
                    }
                case PdfOperatorType.singlequote:
                    {
                        // TODO error handling
                        var ops = Scanner.Scanner.GetOperands();
                        var op = ops[0];
                        T_Star_Op.Value.Apply(ref GraphicsState);
                        var slice = Scanner.Scanner.Data.Slice(op.StartAt, op.Length);
                        var seg = CreateTextSegment(slice);
                        seg.CompatibilitySection = bx;
                        seg.Markings = mc.Count > 0 ? mc.ToList() : null;
                        if (currentText == null)
                        {
                            currentText = new TextSequence
                            {
                                Segments = new List<TextSegment> { seg },
                                LineMatrix = GraphicsState.Text.TextLineMatrix
                            };
                            if (GraphicsState.TextMode != 7)
                            {
                                content.Add(currentText);
                            }
                        }
                        else
                        {
                            seg.NewLine = true;
                            currentText.Segments.Add(seg);
                        }

                        textClipping?.Add(new TextSequence
                        {
                            Segments = new List<TextSegment> { seg with { NewLine = false } },
                            LineMatrix = GraphicsState.Text.TextLineMatrix
                        });
                        continue;
                    }
                case PdfOperatorType.doublequote:
                    {
                        // TODO error handling
                        var ops = Scanner.Scanner.GetOperands();
                        var aw = PdfOperator.ParseDecimal(Context, Scanner.Scanner.Data, ops[0]);
                        var ac = PdfOperator.ParseDecimal(Context, Scanner.Scanner.Data, ops[1]);
                        var op = ops[2];
                        var slice = Scanner.Scanner.Data.Slice(op.StartAt, op.Length);
                        GraphicsState = GraphicsState with { CharSpacing = ac, WordSpacing = aw };
                        T_Star_Op.Value.Apply(ref GraphicsState);
                        var seg = CreateTextSegment(slice);
                        seg.CompatibilitySection = bx;
                        seg.Markings = mc.Count > 0 ? mc.ToList() : null;
                        if (currentText == null)
                        {
                            currentText = new TextSequence
                            {
                                Segments = new List<TextSegment> { seg },
                                LineMatrix = GraphicsState.Text.TextLineMatrix
                            };
                            if (GraphicsState.TextMode != 7)
                            {
                                content.Add(currentText);
                            }
                        }
                        else
                        {
                            seg.NewLine = true;
                            currentText.Segments.Add(seg);
                        }
                        textClipping?.Add(new TextSequence
                        {
                            Segments = new List<TextSegment> { seg with { NewLine = false } },
                            LineMatrix = GraphicsState.Text.TextLineMatrix
                        });
                        continue;
                    }
                case PdfOperatorType.Tj:
                    {
                        // TODO error handling
                        var ops = Scanner.Scanner.GetOperands();
                        var op = ops[0];
                        var slice = Scanner.Scanner.Data.Slice(op.StartAt, op.Length);
                        var seg = CreateTextSegment(slice);
                        seg.CompatibilitySection = bx;
                        seg.Markings = mc.Count > 0 ? mc.ToList() : null;
                        if (currentText == null)
                        {
                            currentText = new TextSequence
                            {
                                Segments = new List<TextSegment> { seg },
                                LineMatrix = GraphicsState.Text.TextLineMatrix
                            };
                            if (GraphicsState.TextMode != 7)
                            {
                                content.Add(currentText);
                            }
                        }
                        else
                        {
                            currentText.Segments.Add(seg);
                        }
                        textClipping?.Add(new TextSequence
                        {
                            Segments = new List<TextSegment> { seg with { } },
                            LineMatrix = GraphicsState.Text.TextLineMatrix
                        });
                        break;
                    }
                case PdfOperatorType.TJ:
                    {
                        // TODO error handling
                        TJCache.Clear();
                        var ops = Scanner.Scanner.GetOperands();
                        var seg = new TextSegment
                        {
                            Glyphs = new List<UnappliedGlyph>(),
                            GraphicsState = GraphicsState.TextMode > 3 ? GraphicsState with { TextMode = GraphicsState.TextMode - 4 } : GraphicsState,
                            CompatibilitySection = bx,
                            Markings = mc.Count > 0 ? mc.ToList() : null
                        };
                        PdfOperator.ParseTJLazy(Context, Scanner.Scanner.Data, ops, TJCache);
                        foreach (var item in TJCache)
                        {
                            if (item.OpNum == -1)
                            {
                                seg.Glyphs.Add(new UnappliedGlyph(null, item.Shift));
                            }
                            else
                            {
                                var op = ops[item.OpNum];
                                var slice = Scanner.Scanner.Data.Slice(op.StartAt, op.Length);
                                Context.FillGlyphsFromRawString(GraphicsState, slice, seg.Glyphs);
                            }
                        }
                        ApplyAll(seg.Glyphs);

                        if (currentText == null)
                        {
                            currentText = new TextSequence
                            {
                                Segments = new List<TextSegment> { seg },
                                LineMatrix = GraphicsState.Text.TextLineMatrix
                            };
                            if (GraphicsState.TextMode != 7)
                            {
                                content.Add(currentText);
                            }
                        }
                        else
                        {
                            currentText.Segments.Add(seg);
                        }
                        textClipping?.Add(new TextSequence
                        {
                            Segments = new List<TextSegment> { seg with { } },
                            LineMatrix = GraphicsState.Text.TextLineMatrix
                        });
                        continue;
                    }
                default:
                    if (Scanner.TryGetCurrentOperation(out var tao))
                    {
                        tao.Apply(ref GraphicsState);
                    }
                    continue;
            }
        }

        return content;

        void CompleteCurrent(GfxState gs, bool reset = true)
        {
            if (state == ParseState.Text)
            {
                if (reset)
                {
                    state = ParseState.Page;
                }
            }
            else if (state == ParseState.Paths)
            {
                if (currentPath!.Closing != n_Op.Value)
                {
                    content.Add(currentPath!);
                }
                if (reset)
                {
                    currentPath = null;
                    state = ParseState.Page;
                }
                else
                {
                    currentPath = new PathSequence
                    {
                        GraphicsState = gs,
                        CompatibilitySection = bx,
                        Paths = new List<SubPath>()
                    };
                }
            }
        }

        SubPath NewSubPath(float x, float y, GfxState gs)
        {
            var nsp = new SubPath { XPos = x, YPos = y, Operations = new List<IPdfOperation>() };
            if (currentPath == null)
            {
                currentPath = new PathSequence
                {
                    GraphicsState = gs,
                    CompatibilitySection = bx,
                    Paths = new List<SubPath>
                {
                    nsp
                }
                };
            }
            else
            {
                currentPath.Paths.Add(nsp);
            }

            return nsp;
        }
    }


    private TextSegment CreateTextSegment(ReadOnlySpan<byte> slice)
    {
        var seq = new TextSegment
        {
            Glyphs = new List<UnappliedGlyph>(),
            GraphicsState = GraphicsState.TextMode > 3 ? GraphicsState with { TextMode = GraphicsState.TextMode - 4 } : GraphicsState
        };
        Context.FillGlyphsFromRawString(GraphicsState, slice, seq.Glyphs);
        ApplyAll(seq.Glyphs);
        return seq;
    }

    public enum TextReadState
    {
        Normal,
        ReadingText,
        ReadingOp
    }

    public (decimal x, decimal y) GetCurrentTextPos()
    {
        return (GraphicsState.Text.TextRenderingMatrix.E, GraphicsState.Text.TextRenderingMatrix.F);
    }

    private void ApplyAll(List<UnappliedGlyph> glyphs)
    {
        foreach (var glyph in glyphs)
        {
            if (glyph.Shift != 0)
            {
                GraphicsState.ApplyShift(glyph);
            }
            else if (glyph.Glyph != null)
            {
                GraphicsState.ApplyCharShift(glyph);
            }
        }
    }

}
