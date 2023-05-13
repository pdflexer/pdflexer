using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.Serializers;
using System.Numerics;

namespace PdfLexer.Writing;

public partial class ContentWriter<T> where T : struct, IFloatingPoint<T>
{
    internal ContentWriter<T> SetGS(GfxState<T> state, bool wrapExtDicts = true)
    {
        var stream = Writer.Stream;
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
        var clippingReset = ((state.Clipping == null || GfxState.Clipping == null) && state.Clipping != GfxState.Clipping)
            || (state.Clipping != null && GfxState.Clipping != null && !state.Clipping.SequenceEqual(GfxState.Clipping));
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
            Tf_Op<T>.WriteLn(nm, state.FontSize, stream);
            GfxState = GfxState with { FontObject = state.FontObject, FontSize = state.FontSize };
        }
        else if (state.FontObject != null && state.FontSize != GfxState.FontSize)
        {
            writableFont = null;
            var nm = AddFont(state.FontObject);
            Tf_Op<T>.WriteLn(nm, state.FontSize, stream);
            GfxState = GfxState with { FontSize = state.FontSize };
        }

        if (state.Clipping != null && !(GfxState.Clipping?.SequenceEqual(state.Clipping) ?? false))
        {
            EnsureInPageState();
            Save();
            var prev = GS.TextMode;
            bool reset = false;

            if (state.Clipping.Any(x => x is TextClippingInfo<T>))
            {
                if (State == PageState.Text)
                {
                    EndText();
                    BeginText();
                }
                Tr_Op<T>.WriteLn(7, Writer.Stream);
                reset = true;
            }
            foreach (var clip in state.Clipping)
            {
                if (clip.TM != GfxState.CTM)
                {
                    var cm = GfxState.GetTranslation(clip.TM);
                    cm_Op<T>.WriteLn(cm.Round(), stream);
                    GfxState = GfxState with { CTM = clip.TM };
                }

                clip.Apply(this);
            }

            if (reset)
            {
                EndText();
                Tr_Op<T>.WriteLn(prev, Writer.Stream);
            }

        }

        if (state.CTM != GfxState.CTM)
        {
            EnsureInPageState();
            EnsureRestorable();
            var cm = GfxState.GetTranslation(state.CTM);
            cm_Op<T>.WriteLn(cm.Round(), stream);
            GfxState = GfxState with { CTM = state.CTM };
        }




        if (state.CharSpacing != GfxState.CharSpacing)
        {
            Tc_Op<T>.WriteLn(state.CharSpacing, stream);
        }

        if (state.TextHScale != GfxState.TextHScale)
        {
            Tz_Op<T>.WriteLn((state.TextHScale * FPC<T>.V100), stream);
        }

        if (state.TextLeading != GfxState.TextLeading)
        {
            TL_Op<T>.WriteLn((state.TextLeading), stream);
        }

        if (state.TextMode != GfxState.TextMode)
        {
            Tr_Op<T>.WriteLn(state.TextMode, stream);
        }

        if (state.TextRise != GfxState.TextRise)
        {
            Ts_Op<T>.WriteLn(state.TextRise, stream);
        }

        if (state.WordSpacing != GfxState.WordSpacing)
        {
            Tw_Op<T>.WriteLn(state.WordSpacing, stream);
        }

        if (state.ColorSpace != GfxState.ColorSpace)
        {
            if (state.ColorSpace == null)
            {
                g_Op<T>.WriteLn(T.Zero, stream);
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
                G_Op<T>.WriteLn(T.Zero, stream);
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
                    G_Op<T>.WriteLn(T.Zero, stream);
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
                }
                else
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
                    g_Op<T>.WriteLn(T.Zero, stream);
                }
                else
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
            i_Op<T>.WriteLn(state.Flatness, stream);
        }

        if (state.LineWidth != GfxState.LineWidth)
        {
            w_Op<T>.WriteLn(state.LineWidth, stream);
        }

        if (state.LineCap != GfxState.LineCap)
        {
            J_Op<T>.WriteLn(state.LineCap, stream);
        }
        if (state.LineJoin != GfxState.LineJoin)
        {
            j_Op<T>.WriteLn(state.LineJoin, stream);
        }
        if (state.MiterLimit != GfxState.MiterLimit)
        {
            M_Op<T>.WriteLn(state.MiterLimit, stream);
        }

        if (state.RenderingIntent != GfxState.RenderingIntent)
        {
            if (state.RenderingIntent == null)
            {
                ri_Op.WriteLn(PdfName.RelativeColorimetric, stream);
            }
            else
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
                cm_Op<T>.WriteLn(cm.Round(), stream);
                gs_Op.WriteLn(nm, stream);
                state.ExtDict.CTM.Invert(out var iv);
                var toPrev = prev * iv;
                cm_Op<T>.WriteLn(toPrev.Round(), stream);
            }
            else
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
            CS_Op.WriteLn(nm, Writer.Stream);
        }
        else
        {
            nm = AddColorSpace(CS);
            CS_Op.WriteLn(nm, Writer.Stream);
        }
    }

    private void Writecs(IPdfObject cs)
    {
        if (cs is PdfName nm)
        {
            cs_Op.WriteLn(nm, Writer.Stream);
        }
        else
        {
            nm = AddColorSpace(cs);
            cs_Op.WriteLn(nm, Writer.Stream);
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
            case scn_Op<T> ac:
                {
                    var bc = (scn_Op<T>)b;
                    return bc.name == ac.name && bc.colorInfo.SequenceEqual(ac.colorInfo);
                }
            case sc_Op<T> ac:
                {
                    var bc = (sc_Op<T>)b;
                    return bc.colorInfo.SequenceEqual(ac.colorInfo);
                }
            case SCN_Op<T> ac:
                {
                    var bc = (SCN_Op<T>)b;
                    return bc.name == ac.name && bc.colorInfo.SequenceEqual(ac.colorInfo);
                }
            case SC_Op<T> ac:
                {
                    var bc = (SC_Op<T>)b;
                    return bc.colorInfo.SequenceEqual(ac.colorInfo);
                }
        }
        return false;
    }
    internal void SubPath(SubPath<T> subPath)
    {
        EnsureInPageState();
        if (subPath.Operations.Count > 0 && subPath.Operations[0].Type != PdfOperatorType.re)
        {
            MoveTo(subPath.XPos, subPath.YPos);
        }
        foreach (var op in subPath.Operations)
        {
            Op(op);
        }
        if (subPath.Closed)
        {
            Op(h_Op<T>.Value);
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
            else if (!mcState.SequenceEqual(desired))
            {
                var min = Math.Min(mcState.Count, desired.Count);
                var i = 0;
                for (; i < min; i++)
                {
                    if (mcState[i] == desired[i])
                    {
                        continue;
                    }
                    break;
                }
                if (i < mcState.Count)
                {
                    var mcc = mcState.Count;
                    for (var x = i; x < mcc; x++)
                    {
                        EndMarkedContent();
                    }
                    for (var x = i; x < desired.Count; x++)
                    {
                        MarkedContent(desired[i]);
                    }
                }
            }
        }
    }


    internal void ReconcileCompatibility(bool compat)
    {
        if (compat != isCompatSection)
        {
            if (compat)
            {
                BX_Op.WriteLn(Writer.Stream);
            }
            else
            {
                EX_Op.WriteLn(Writer.Stream);
            }
            isCompatSection = compat;
        }
    }
    
    internal ContentWriter<T> MarkedContent(MarkedContent mc)
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
                var ir = mc.PropList.Indirect();
                if (mc.OCGDefault != null)
                {
                    OCDefaults[ir] = mc.OCGDefault.Value;
                }
                Properties[name] = ir;
            }
            var op = new BDC_Op(mc.Name, name);
            op.Serialize(Writer.Stream);
            Writer.Stream.WriteByte((byte)'\n');
        }
        else if (mc.InlineProps != null)
        {
            var op = new BDC_Op(mc.Name, mc.InlineProps);
            op.Serialize(Writer.Stream);
            Writer.Stream.WriteByte((byte)'\n');
        }
        else
        {
            BMC_Op.WriteLn(mc.Name, Writer.Stream);
        }
        mcState ??= new List<MarkedContent>();
        mcState.Add(mc);
        mcDepth++;
        return this;
    }

    internal ContentWriter<T> EndMarkedContent()
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
        EMC_Op.Value.Serialize(Writer.Stream);
        Writer.Stream.WriteByte((byte)'\n');
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
        sh_Op.WriteLn(nm, Writer.Stream);
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
