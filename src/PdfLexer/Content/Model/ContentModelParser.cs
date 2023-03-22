﻿using PdfLexer.Fonts;
using System.Text;

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
    private PdfDictionary? PgRes;
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
        PgRes = page.Get<PdfDictionary>(PdfName.Resources);
        GraphicsState = new GfxState();
        TJCache = new List<TJ_Lazy_Item>(10);
    }

    public ContentModelParser(ParsingContext ctx, PdfDictionary page, PdfStream form, GfxState state)
    {
        Context = ctx;
        Scanner = new PageContentScanner2(ctx, page, form);
        PgRes = page.Get<PdfDictionary>(PdfName.Resources);
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

        SubPath? currentSubPath = null;
        PdfOperatorType? clipping = null;

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
                            } else
                            {
                                mc.Add(new MarkedContent(bdcOp.tag) { PropList = found });
                            }
                        } else if (bdcOp.props is PdfDictionary dict)
                        {
                            mc.Add(new MarkedContent(bdcOp.tag) { InlineProps = dict });
                        } else
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
                    } else
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
                case PdfOperatorType.BI:
                    {
                        if (!Scanner.TryGetCurrentOperation(out var iiop))
                        {
                            continue;
                        }
                        content.Add(new InlineImage
                        {
                            Img = (InlineImage_Op)iiop,
                            GraphicsState = GraphicsState,
                            CompatibilitySection = bx,
                            Markings = mc.Count > 0 ? mc.ToList() : null
                        });
                        continue;
                    }
                case PdfOperatorType.EOC:
                    return content;
                case PdfOperatorType.BT:
                    state = ParseState.Text;
                    BT_Op.Value.Apply(ref GraphicsState);
                    continue;
                case PdfOperatorType.ET:
                    CompleteCurrent(GraphicsState);
                    state = ParseState.Page;
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
                            } else
                            {
                                rf = Context.GetFont(font);
                            }
                            Tf_Op.Apply(ref GraphicsState, font, rf, (float)tfOp.size);
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
                        gso.Apply(ref GraphicsState, gsd, Scanner.Resources, Context);

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
                            } else
                            {
                                GraphicsState = GraphicsState with { ColorSpaceStroking = nm };
                            }
                        } else
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
                case PdfOperatorType.SCN:
                case PdfOperatorType.scn:
                case PdfOperatorType.w:
                    {
                        CompleteCurrent(GraphicsState, false);

                        if (Scanner.TryGetCurrentOperation(out var gso))
                        {
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
                case PdfOperatorType.sh:
                    {
                        if (currentPath == null)
                        {
                            continue; // warning
                        }
                        if (Scanner.TryGetCurrentOperation(out var pdo))
                        {
                            currentPath.Closing = pdo;
                        } else
                        {
                            currentPath.Closing = n_Op.Value;
                        }
                        
                        CompleteCurrent(GraphicsState);

                        if (clipping != null)
                        {
                            // TODO
                        }
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
                        continue;
                    }
            }


            if (nxt == PdfOperatorType.EI) { continue; } // EI can spill outside bounds if data is corrupt
            var b = Scanner.Scanner.Data[Scanner.Scanner.CurrentInfo.StartAt];
            if (b == (byte)'T' || b == (byte)'\'' || b == (byte)'"')
            {
                try
                {
                    switch (nxt)
                    {
                        case PdfOperatorType.singlequote:
                            {
                                var ops = Scanner.Scanner.GetOperands();
                                var op = ops[0];
                                T_Star_Op.Value.Apply(ref GraphicsState);
                                var slice = Scanner.Scanner.Data.Slice(op.StartAt, op.Length);
                                var seq = CreateTextSequence(slice);
                                seq.CompatibilitySection = bx;
                                seq.Markings = mc.Count > 0 ? mc.ToList() : null;
                                content.Add(seq);
                                continue;
                            }
                        case PdfOperatorType.doublequote:
                            {
                                var ops = Scanner.Scanner.GetOperands();
                                var aw = PdfOperator.ParseFloat(Context, Scanner.Scanner.Data, ops[0]);
                                var ac = PdfOperator.ParseFloat(Context, Scanner.Scanner.Data, ops[1]);
                                var op = ops[2];
                                var slice = Scanner.Scanner.Data.Slice(op.StartAt, op.Length);
                                GraphicsState = GraphicsState with { CharSpacing = ac, WordSpacing = aw };
                                T_Star_Op.Value.Apply(ref GraphicsState);
                                var seq = CreateTextSequence(slice);
                                seq.CompatibilitySection = bx;
                                seq.Markings = mc.Count > 0 ? mc.ToList() : null;
                                content.Add(seq);
                                continue;
                            }
                        case PdfOperatorType.Tj:
                            {
                                var ops = Scanner.Scanner.GetOperands();
                                var op = ops[0];
                                var slice = Scanner.Scanner.Data.Slice(op.StartAt, op.Length);
                                var seq = CreateTextSequence(slice);
                                seq.CompatibilitySection = bx;
                                seq.Markings = mc.Count > 0 ? mc.ToList() : null;
                                content.Add(seq);
                                break;
                            }
                        case PdfOperatorType.TJ:
                            {
                                TJCache.Clear();
                                var ops = Scanner.Scanner.GetOperands();
                                var (x, y) = GetCurrentTextPos();
                                var seq = new TextSequence { 
                                    Glyphs = new List<UnappliedGlyph>(),
                                    GraphicsState = GraphicsState,
                                    CompatibilitySection = bx,
                                    LineMatrix = GraphicsState.Text.TextLineMatrix,
                                    Markings = mc.Count > 0 ? mc.ToList() : null
                                };
                                PdfOperator.ParseTJLazy(Context, Scanner.Scanner.Data, ops, TJCache);
                                foreach (var item in TJCache)
                                {
                                    if (item.OpNum == -1)
                                    {
                                        seq.Glyphs.Add(new UnappliedGlyph(null, (float)item.Shift));
                                    }
                                    else
                                    {
                                        var op = ops[item.OpNum];
                                        var slice = Scanner.Scanner.Data.Slice(op.StartAt, op.Length);
                                        Context.FillGlyphsFromRawString(GraphicsState, slice, seq.Glyphs);
                                    }
                                }
                                ApplyAll(seq.Glyphs);
                                content.Add(seq);
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
                catch (Exception e)
                {
                    // since we are manually parsing text ops (not using TryGetCurrentOperation)
                    // we have to handle errors manually here
                    var data = Encoding.ASCII.GetString(Scanner.Scanner.GetDataForCurrent());
                    Context.Error($"error while parsing text op ({nxt.ToString()} -> '{data}'): " + e.Message);
                    continue;
                }
            }

            // non-text affecting op
        }
        return content;


        void CompleteCurrent(GfxState gs, bool reset=true)
        {
            if (state == ParseState.Text)
            {
                if (reset)
                {
                    state = ParseState.Page;
                }
            } else if (state == ParseState.Paths)
            {
                content.Add(currentPath!);
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
            } else
            {
                currentPath.Paths.Add(nsp);
            }

            return nsp;
        }
    }


    private TextSequence CreateTextSequence(ReadOnlySpan<byte> slice)
    {
        var seq = new TextSequence { LineMatrix = GraphicsState.Text.TextLineMatrix, Glyphs = new List<UnappliedGlyph>(), GraphicsState = GraphicsState };
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

    public (float x, float y) GetCurrentTextPos()
    {
        return (GraphicsState.Text.TextRenderingMatrix.M31, GraphicsState.Text.TextRenderingMatrix.M32);
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
