using PdfLexer.Content;
using PdfLexer.Fonts;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PdfLexer.Operators;


public partial class Tc_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { CharSpacing = charSpace };
    }
}

public partial class Tw_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { WordSpacing = wordSpace };
    }
}

public partial class Tz_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { TextHScale =   scale / FPC<T>.V100 };
        state.UpdateTRM();
    }
}

public partial class TL_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { TextLeading = leading };
    }
}

public partial class Tf_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        throw new NotSupportedException();
    }

    public static void Apply(ref GfxState<T> state, PdfName name,  PdfDictionary font, IReadableFont? readable, T size)
    {
        state = state with { FontSize = size, FontObject = font, Font = readable, FontResourceName = name };
        state.UpdateTRM();
    }
}

public partial class Tr_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { TextMode = render };
    }
}

public partial class Ts_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { TextRise = rise };
        state.UpdateTRM();
    }
}

public partial class Td_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state.ShiftTextAndLineMatrix(tx, ty);
    }

    public static void Apply(ref GfxState<T> state, T tx, T ty)
    {
        state.ShiftTextAndLineMatrix(tx, ty);
    }
}
public partial class TD_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { TextLeading = -ty };
        state.ShiftTextAndLineMatrix(tx, ty);
    }
}

public partial class Tm_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state.Text.TextLineMatrix = new GfxMatrix<T>(a, b, c, d, e, f);
        state.Text.TextMatrix = state.Text.TextLineMatrix;
        state.UpdateTRM();
    }

    public static void WriteLn(GfxMatrix<T> tm, Stream stream)
    {
        Write(tm.A, tm.B, tm.C, tm.D, tm.E, tm.F, stream);
        stream.WriteByte((byte)'\n');
    }

}

public partial class T_Star_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state.ShiftTextAndLineMatrix(T.Zero, -state.TextLeading);
    }
}

public partial class Tj_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state.ApplyData(text);
    }
}

public partial class doublequote_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state = state with { WordSpacing = aw, CharSpacing = ac };
        T_Star_Op<T>.Value.Apply(ref state);
        state.ApplyData(text);
    }
}

public partial class singlequote_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        T_Star_Op<T>.Value.Apply(ref state);
        state.ApplyData(text);
    }
}

public partial class TJ_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        foreach (var item in info)
        {
            if (item.Shift != T.Zero)
            {
                state.ApplyTj(item.Shift);
            }
            else
            {
                state.ApplyData(item.Data);
            }
        }
    }
}

public partial class BT_Op<T>
{
    public void Apply(ref GfxState<T> state)
    {
        state.Text.TextLineMatrix = GfxMatrix<T>.Identity;
        state.Text.TextMatrix = GfxMatrix<T>.Identity;
        state.UpdateTRM();
    }
}
