using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.Fonts;
using PdfLexer.Serializers;
using System.IO;
using System.Numerics;

namespace PdfLexer.Writing;

public partial class ContentWriter
{
    internal ContentWriter SetGS(GfxState state, bool wrapExtDicts = true)
    {
        var stream = StreamWriter.Stream;
        if (Object.ReferenceEquals(GfxState, state))
        {
            return this;
        }

        if (wrapExtDicts && state.ExtDict != GfxState.ExtDict) // actual changes done at end
        {
            while (GfxState.ExtDict != null)
            {
                Restore();
            }
        }

        // want to reset to default using gfx restore
        var ctmIdentity = (state.CTM != GfxState.CTM && state.CTM.IsIdentity)
            || (state.Clipping != GfxState.Clipping);
        var clippingReset = state.Clipping != GfxState.Clipping;
        if (ctmIdentity || clippingReset)
        {
            EnsureInPageState();
            if (clippingReset && GfxState.Clipping != null)
            {
                var gfx = GfxState;
                var cnt = 0;
                bool found = false;
                while (gfx.Prev != null)
                {
                    cnt++;
                    if (gfx.Prev.Clipping == null)
                    {
                        found = true;
                        break;
                    }
                    gfx = gfx.Prev;
                }
                if (found)
                {
                    for (var i = 0; i < cnt; i++)
                    {
                        Restore();
                    }
                }
                else
                {
                    throw new PdfLexerException("Unable to reset clipping to default.");
                }
            }
            if (ctmIdentity && !GfxState.CTM.IsIdentity)
            {
                var gfx = GfxState;
                var cnt = 0;
                bool found = false;
                while (gfx.Prev != null)
                {
                    cnt++;
                    if (gfx.Prev.CTM.IsIdentity)
                    {
                        found = true;
                        break;
                    }
                    gfx = gfx.Prev;
                }
                if (found)
                {
                    for (var i = 0; i < cnt; i++)
                    {
                        Restore();
                    }
                }
                else
                {
                    throw new PdfLexerException("Unable to reset CTM to default.");
                }
            }
        }

        // set fonts here in case clipping is based on text
        if (state.FontObject != null && state.FontObject != GfxState.FontObject)
        {
            writableFont = null;
            var nm = AddFont(state.FontObject);
            Tf_Op.WriteLn(nm, (decimal)state.FontSize, stream);
        }
        else if (state.FontObject != null && state.FontSize != GfxState.FontSize)
        {
            writableFont = null;
            var nm = AddFont(state.FontObject);
            Tf_Op.WriteLn(nm, (decimal)state.FontSize, stream);
        }

        if (state.Clipping != GfxState.Clipping)
        {
            EnsureInPageState();
            Save();
            foreach (var clip in state.Clipping)
            {
                if (clip.TM != GfxState.CTM)
                {
                    var cm = GfxState.GetTranslation(clip.TM);
                    cm_Op.WriteLn(cm, stream);
                    GfxState = GfxState with { CTM = clip.TM };
                }

                clip.Apply(this);
            }
            
        }

        if (state.CTM != GfxState.CTM)
        {
            EnsureInPageState();
            EnsureRestorable();
            var cm = GfxState.GetTranslation(state.CTM);
            cm_Op.WriteLn(cm, stream);
            GfxState = GfxState with { CTM = state.CTM };
        }




        if (state.CharSpacing != GfxState.CharSpacing)
        {
            Tc_Op.WriteLn((decimal)state.CharSpacing, stream);
        }

        if (state.TextHScale != GfxState.TextHScale)
        {
            Tz_Op.WriteLn((decimal)(state.TextHScale * 100.0), stream);
        }

        if (state.TextLeading != GfxState.TextLeading)
        {
            TL_Op.WriteLn((decimal)(state.TextLeading), stream);
        }

        if (state.TextMode != GfxState.TextMode)
        {
            Tr_Op.WriteLn(state.TextMode, stream);
        }

        if (state.TextRise != GfxState.TextRise)
        {
            Ts_Op.WriteLn((decimal)state.TextRise, stream);
        }

        if (state.WordSpacing != GfxState.WordSpacing)
        {
            Tw_Op.WriteLn((decimal)state.WordSpacing, stream);
        }

        if (state.ColorSpace != GfxState.ColorSpace)
        {
            if (state.ColorSpace == null)
            {
                g_Op.WriteLn(0, stream);
            }
            else
            {
                Writecs(state.ColorSpace);
            }

            GfxState = GfxState with { Color = null };
        }

        if (state.ColorSpaceStroking != GfxState.ColorSpaceStroking)
        {
            if (state.ColorSpaceStroking == null)
            {
                G_Op.WriteLn(0, stream);
            }
            else
            {
                WriteCS(state.ColorSpaceStroking);
            }
            GfxState = GfxState with { ColorStroking = null };
        }

        if (!ColorsEqual(state.ColorStroking, GfxState.ColorStroking))
        {
            if (state.ColorStroking == null)
            {
                if (state.ColorSpaceStroking == null)
                {
                    G_Op.WriteLn(0, stream);
                }
                else
                {
                    WriteCS(state.ColorSpaceStroking);
                }
            }
            else
            {
                if (state.ColorStroking is IPatternableColor pc && pc.Pattern != null)
                {
                    var nm = AddPattern(pc.Pattern);
                    NameSerializer.WriteToStreamInternal(nm, stream);
                    stream.WriteByte((byte)' ');
                    stream.Write(SCN_Op.OpData);
                    stream.WriteByte((byte)'\n');
                } else
                {
                    state.ColorStroking.Serialize(stream);
                    stream.WriteByte((byte)'\n');
                }
                
            }
        }

        if (!ColorsEqual(state.Color, GfxState.Color))
        {
            if (state.Color == null)
            {
                if (state.ColorSpace == null)
                {
                    g_Op.WriteLn(0, stream);
                } else
                {
                    Writecs(state.ColorSpace);
                }
            }
            else
            {
                if (state.Color is IPatternableColor pc && pc.Pattern != null)
                {
                    var nm = AddPattern(pc.Pattern);
                    NameSerializer.WriteToStreamInternal(nm, stream);
                    stream.WriteByte((byte)' ');
                    stream.Write(scn_Op.OpData);
                    stream.WriteByte((byte)'\n');
                }
                else
                {
                    state.Color.Serialize(stream);
                    stream.WriteByte((byte)'\n');
                }
            }
        }

        if (state.Dashing != GfxState.Dashing)
        {
            if (state.Dashing == null)
            {
                d_Op.Default.Serialize(stream);
            }
            else
            {
                state.Dashing.Serialize(stream);
            }
            stream.WriteByte((byte)'\n');
        }

        if (state.Flatness != GfxState.Flatness)
        {
            i_Op.WriteLn((decimal)state.Flatness, stream);
        }

        if (state.LineWidth != GfxState.LineWidth)
        {
            w_Op.WriteLn((decimal)state.LineWidth, stream);
        }

        if (state.LineCap != GfxState.LineCap)
        {
            J_Op.WriteLn(state.LineCap, stream);
        }
        if (state.LineJoin != GfxState.LineJoin)
        {
            j_Op.WriteLn(state.LineJoin, stream);
        }
        if (state.MiterLimit != GfxState.MiterLimit)
        {
            M_Op.WriteLn((decimal)state.MiterLimit, stream);
        }

        if (state.RenderingIntent != GfxState.RenderingIntent)
        {
            if (state.RenderingIntent == null)
            {
                ri_Op.WriteLn(PdfName.RelativeColorimetric, stream);
            } else
            {
                ri_Op.WriteLn(state.RenderingIntent, stream);
            }
            
        }

        //MiterLimit

        GfxState = state with { Prev = GfxState.Prev, Text = GfxState.Text, ExtDict = GfxState.ExtDict };

        if (state.ExtDict != GfxState.ExtDict && state.ExtDict != null)
        {
            if (wrapExtDicts)
            {
                // do this after rest of gfx state ops so we dont reset non GS specific ones over and over
                EnsureRestorable();
            }


            var nm = AddExtGS(state.ExtDict.Dict);
            if (state.ExtDict.Dict.ContainsKey(PdfName.SMask))
            {
                // smask dependent on CTM
                var prev = GfxState.CTM;
                var cm = GfxState.GetTranslation(state.ExtDict.CTM);
                cm_Op.WriteLn(cm, stream);
                gs_Op.WriteLn(nm, stream);
                Matrix4x4.Invert(state.ExtDict.CTM, out var iv);
                var toPrev = prev * iv;
                cm_Op.WriteLn(toPrev, stream);
            } else
            {
                gs_Op.WriteLn(nm, stream);
            }
            
            

            GfxState = GfxState with { ExtDict = state.ExtDict };
        }


        return this;
    }
    private Dictionary<PdfDictionary, PdfName> propertyLists = new Dictionary<PdfDictionary, PdfName>();

    private void WriteCS(IPdfObject CS)
    {
        if (CS is PdfName nm)
        {
            CS_Op.WriteLn(nm, StreamWriter.Stream);
        }
        else
        {
            nm = AddColorSpace(CS);
            CS_Op.WriteLn(nm, StreamWriter.Stream);
        }
    }

    private void Writecs(IPdfObject cs)
    {
        if (cs is PdfName nm)
        {
            cs_Op.WriteLn(nm, StreamWriter.Stream);
        }
        else
        {
            nm = AddColorSpace(cs);
            cs_Op.WriteLn(nm, StreamWriter.Stream);
        }
    }

    private static bool ColorsEqual(IPdfOperation? a, IPdfOperation? b)
    {
        if (a == null && b == null)
        {
            return true;
        }
        if (a == null || b == null) { return false; }

        if (a.Type != b.Type) { return false; }
        switch (a)
        {
            case scn_Op ac:
                {
                    var bc = (scn_Op)b;
                    return bc.name == ac.name && bc.colorInfo.SequenceEqual(ac.colorInfo);
                }
            case sc_Op ac:
                {
                    var bc = (sc_Op)b;
                    return bc.colorInfo.SequenceEqual(ac.colorInfo);
                }
            case SCN_Op ac:
                {
                    var bc = (SCN_Op)b;
                    return bc.name == ac.name && bc.colorInfo.SequenceEqual(ac.colorInfo);
                }
            case SC_Op ac:
                {
                    var bc = (SC_Op)b;
                    return bc.colorInfo.SequenceEqual(ac.colorInfo);
                }
        }
        return false;
    }
    internal void SubPath(SubPath subPath)
    {
        EnsureInPageState();
        if (subPath.Operations.Count > 0 && subPath.Operations[0].Type != PdfOperatorType.re)
        {
            MoveTo((decimal)subPath.XPos, (decimal)subPath.YPos);
        }
        foreach (var op in subPath.Operations)
        {
            Op(op);
        }
        if (subPath.Closed)
        {
            Op(h_Op.Value);
        }
    }

    private List<MarkedContent>? mcState;
    internal void ReconcileMC(List<MarkedContent>? desired)
    {
        if (mcState != desired)
        {
            if (mcState == null)
            {
                for (var i = 0; i < desired.Count; i++)
                {
                    MarkedContent(desired[i]);
                }

            }
            else if (desired == null)
            {
                var mcc = mcState.Count;
                for (var i = 0; i < mcc; i++)
                {
                    EndMarkedContent();
                }
            }
            else
            {
                var min = Math.Min(mcState.Count, mcState.Count);
                var i = 0;
                for (; i < min; i++)
                {
                    if (mcState[i] == mcState[i])
                    {
                        continue;
                    }
                }
                if (i < mcState.Count)
                {
                    var mcc = mcState.Count;
                    for (var x = i; x < mcc; x++)
                    {
                        EndMarkedContent();
                    }
                }
                if (i < mcState.Count)
                {
                    for (var x = i; x < mcState.Count; x++)
                    {
                        MarkedContent(mcState[i]);
                    }
                }
            }
        }
    }

    bool isCompatSection;
    internal void ReconcileCompatibility(bool compat)
    {
        if (compat != isCompatSection)
        {
            if (compat)
            {
                BX_Op.WriteLn(StreamWriter.Stream);
            }
            else
            {
                EX_Op.WriteLn(StreamWriter.Stream);
            }
            isCompatSection = compat;
        }
    }

    internal ContentWriter MarkedContent(MarkedContent mc)
    {
        EnsureInPageState(); // make sure in page state to simplify making sure we don't have
                             // uneven operators eg. BT BMC ET EMC
        if (mc.PropList != null)
        {
            if (!propertyLists.TryGetValue(mc.PropList, out var name))
            {
                name = $"PL{objCnt++}";
                while (ExtGS.ContainsKey(name))
                {
                    name = $"PL{objCnt++}";
                }

                propertyLists[mc.PropList] = name;
                Properties[name] = mc.PropList;
            }
            var op = new BDC_Op(mc.Name, name);
            op.Serialize(StreamWriter.Stream);
            StreamWriter.Stream.WriteByte((byte)'\n');
        }
        else if (mc.InlineProps != null)
        {
            var op = new BDC_Op(mc.Name, mc.InlineProps);
            op.Serialize(StreamWriter.Stream);
            StreamWriter.Stream.WriteByte((byte)'\n');
        }
        else
        {
            BMC_Op.WriteLn(mc.Name, StreamWriter.Stream);
        }
        mcState ??= new List<MarkedContent>();
        mcState.Add(mc);
        mcDepth++;
        return this;
    }

    internal ContentWriter EndMarkedContent()
    {
        EnsureInPageState();
        if (mcState == null || mcState.Count == 0)
        {
            throw new PdfLexerException("EMC called without MC state");
        }

        mcState.RemoveAt(mcState.Count - 1);
        if (mcState.Count == 0)
        {
            mcState = null;
        }

        mcDepth--;
        EMC_Op.Value.Serialize(StreamWriter.Stream);
        StreamWriter.Stream.WriteByte((byte)'\n');
        return this;
    }

    private PdfName? emptyGS;
    private Dictionary<PdfDictionary, PdfName> extGraphics = new Dictionary<PdfDictionary, PdfName>();
    internal PdfName AddExtGS(PdfDictionary gs)
    {
        if (extGraphics.TryGetValue(gs, out var name))
        {
            return name;
        }

        name = $"GS{objCnt++}";
        while (ExtGS.ContainsKey(name))
        {
            name = $"GS{objCnt++}";
        }

        extGraphics[gs] = name;
        ExtGS[name] = gs;
        return name;
    }


    internal void Shading(IPdfObject sh)
    {
        EnsureInPageState();
        var nm = AddShading(sh);
        sh_Op.WriteLn(nm, StreamWriter.Stream);
    }
    private Dictionary<IPdfObject, PdfName> shadings = new Dictionary<IPdfObject, PdfName>();
    internal PdfName AddShading(IPdfObject sh)
    {
        if (shadings.TryGetValue(sh, out var name))
        {
            return name;
        }

        name = $"SH{objCnt++}";
        while (Shadings.ContainsKey(name))
        {
            name = $"SH{objCnt++}";
        }

        shadings[sh] = name;
        Shadings[name] = sh.Indirect();
        return name;
    }

    private Dictionary<IPdfObject, PdfName> patterns = new Dictionary<IPdfObject, PdfName>();
    internal PdfName AddPattern(IPdfObject sh)
    {
        if (patterns.TryGetValue(sh, out var name))
        {
            return name;
        }

        name = $"PA{objCnt++}";
        while (Patterns.ContainsKey(name))
        {
            name = $"PA{objCnt++}";
        }

        patterns[sh] = name;
        Patterns[name] = sh.Indirect();
        return name;
    }
}