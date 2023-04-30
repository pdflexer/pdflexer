using PdfLexer.Content;
using PdfLexer.Fonts;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PdfLexer.Operators;

public partial class q_Op<T>
{

    public void Apply(ref GfxState<T> state)
    {
        state = state with { Prev = state };
    }
}

public partial class Q_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        if (state.Prev == null)
        {
            // err
            state = new GfxState<T>();
            return;
        }
        state = state.Prev with { Text = state.Text };
        state.UpdateTRM();
    }
}

public partial class CS_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        throw new NotSupportedException();
    }
}


public partial class SC_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { ColorStroking = this };
    }
}


public interface IPatternableColor
{
    IPdfObject? Pattern { get; }
}

public partial class SCN_Op<T> : IPatternableColor
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { ColorStroking = this };
    }
    public IPdfObject? Pattern { get; set; } // hack for resource until adding better color handling
}

public partial class sc_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { Color = this };
    }
}

public partial class scn_Op<T> : IPatternableColor
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { Color = this };
    }
    public IPdfObject? Pattern { get; set; } // hack for resource until adding better color handling
}

public partial class G_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { ColorSpaceStroking = PdfName.DeviceGray, ColorStroking = new SC_Op<T>(new List<T> { this.gray }) }; // TODO revisit how this is handled
    }
}

public partial class g_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { ColorSpace = PdfName.DeviceGray, Color = new sc_Op<T>(new List<T> { this.gray }) };  // TODO revisit how this is handled
    }
}

public partial class RG_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { ColorSpaceStroking = PdfName.DeviceRGB, ColorStroking = new SC_Op<T>(new List<T> { r, g, b }) };
    }
}

public partial class rg_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { ColorSpace = PdfName.DeviceRGB, Color = new sc_Op<T>(new List<T> { r, g, b }) };   // TODO revisit how this is handled
    }
}


public partial class K_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { ColorSpaceStroking = PdfName.DeviceCMYK, ColorStroking = new SC_Op<T>(new List<T> { c, m, y, k }) }; // TODO revisit how this is handled
    }
}

public partial class k_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { ColorSpace = PdfName.DeviceCMYK, Color = new sc_Op<T>(new List<T> { c, m, y, k }) }; // TODO revisit how this is handled
    }
}

public partial class w_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { LineWidth = lineWidth };
    }
}

public partial class J_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { LineCap = lineCap };
    }
}

public partial class j_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { LineJoin = lineJoin };
    }
}

public partial class M_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { MiterLimit = miterLimit };
    }
}

public partial class i_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { Flatness = flatness };
    }
}

public partial class d_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { Dashing = this };
    }
}

public partial class ri_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { RenderingIntent = intent };
    }
}

public partial class cm_Op<T>
{

    public void Apply(ref GfxState<T> state)
    {

        // new = cm x ctm
        var val = new GfxMatrix<T>(a, b, c, d, e, f);
        state = state with { CTM = val * state.CTM };
        state.UpdateTRM();
    }

    public static void Apply(ref GfxState<T> state, GfxMatrix<T> val)
    {
        state = state with { CTM = val * state.CTM };
        state.UpdateTRM();
    }

   
    public static void WriteLn(GfxMatrix<T> ctm, Stream stream)
    {
        WriteLn(
            ctm.A, ctm.B,
            ctm.C, ctm.D,
            ctm.E, ctm.F, stream);
    }
}

public partial class gs_Op<T>
{

    public void Apply(ref GfxState<T> state,
        PdfDictionary dict,
        PdfDictionary resources,
        ParsingContext ctx,
        Dictionary<PdfDictionary, PdfDictionary> cache
        )
    {
        var orig = dict;
        dict = dict.CloneShallow();
        if (dict.TryGetValue<PdfNumber>("LS", out var lsobj, false))
        {
            dict.Remove("LS");
        }

        if (dict.TryGetValue<PdfNumber>("LC", out var lcobj, false))
        {
            dict.Remove("LC");
        }

        if (dict.TryGetValue<PdfNumber>("LJ", out var ljobj, false))
        {
            dict.Remove("LJ");
        }

        if (dict.TryGetValue<PdfNumber>("ML", out var mlobj, false))
        {
            dict.Remove("ML");
        }

        if (dict.TryGetValue<PdfName>("RI", out var riobj, false))
        {
            dict.Remove("RI");
        }

        if (dict.TryGetValue<PdfNumber>("FL", out var flobj, false))
        {
            dict.Remove("FL");
        }

        d_Op<T>? dop = null;
        if (dict.TryGetValue<PdfArray>("D", out var dobj, false))
        {
            dict.Remove("D");
            if (dobj.Count > 1 && dobj[0] is PdfArray dashes && dobj[1] is PdfNumber dp)
            {
                dop = new d_Op<T>(dashes, FPC<T>.Util.FromPdfNumber<T>(dp));
            }
        }

        T? fsize = null;
        PdfDictionary? fdict = null;
        IReadableFont? fread = null;
        if (dict.TryGetValue<PdfArray>(PdfName.Font, out var fobj, false))
        {
            dict.Remove(PdfName.Font);
            if (fobj.Count > 0 && fobj[0].Resolve() is PdfDictionary fdv)
            {
                fdict = fdv;
                fread = ctx.GetFont(fdv);
            }
            if (fobj.Count > 1 && fobj[1].Resolve() is PdfNumber fz)
            {
                fsize = FPC<T>.Util.FromPdfNumber<T>(fz);
            }
        }
        if (state.ExtDict != null)
        {
            // TODO -> dedup these or change extdict to list
            var existing = state.ExtDict.Dict.CloneShallow();
            foreach (var kvp in dict)
            {
                existing[kvp.Key] = kvp.Value;
            }
            dict = existing;
        }
        else
        {
            if (!cache.TryGetValue(orig, out var cached))
            {
                cache[orig] = dict;
            }
            else
            {
                dict = cached;
            }
        }



        state = state with
        {
            LineWidth = lsobj == null ? state.LineWidth : FPC<T>.Util.FromPdfNumber<T>(lsobj),
            LineCap = lcobj == null ? state.LineCap : (int)lcobj,
            LineJoin = ljobj == null ? state.LineJoin : (int)ljobj,
            MiterLimit = mlobj == null ? state.MiterLimit : FPC<T>.Util.FromPdfNumber<T>(mlobj),
            RenderingIntent = riobj == null ? state.RenderingIntent : riobj,
            Flatness = flobj == null ? state.Flatness : FPC<T>.Util.FromPdfNumber<T>(flobj),
            Dashing = dop == null ? state.Dashing : dop,
            Font = fread == null ? state.Font : fread,
            FontObject = fdict == null ? state.FontObject : fdict,
            FontSize = fsize == null ? state.FontSize : fsize.Value,
            ExtDict = dict.Count > 0 ? new ExtGraphicsDict<T> { Dict = dict, CTM = state.CTM } : null
        };
        state.UpdateTRM();
    }
}