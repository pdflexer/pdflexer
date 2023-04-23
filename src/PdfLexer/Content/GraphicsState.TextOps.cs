using PdfLexer.Content;
using PdfLexer.Fonts;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PdfLexer.Operators;


public partial class Tc_Op
{
    public void Apply(ref GfxState state)
    {
        state = state with { CharSpacing = charSpace };
    }
}

public partial class Tw_Op
{
    public void Apply(ref GfxState state)
    {
        state = state with { WordSpacing = wordSpace };
    }
}

public partial class Tz_Op
{
    public void Apply(ref GfxState state)
    {
        state = state with { TextHScale =   scale / 100.0m };
        state.UpdateTRM();
    }
}

public partial class TL_Op
{
    public void Apply(ref GfxState state)
    {
        state = state with { TextLeading = leading };
    }
}

public partial class Tf_Op
{
    public void Apply(ref GfxState state)
    {
        throw new NotSupportedException();
    }

    public static void Apply(ref GfxState state, PdfDictionary font, IReadableFont readable, decimal size)
    {
        state = state with { FontSize = size, FontObject = font, Font = readable };
        state.UpdateTRM();
    }
}

public partial class Tr_Op
{
    public void Apply(ref GfxState state)
    {
        state = state with { TextMode = render };
    }
}

public partial class Ts_Op
{
    public void Apply(ref GfxState state)
    {
        state = state with { TextRise = rise };
        state.UpdateTRM();
    }
}

public partial class Td_Op
{
    public void Apply(ref GfxState state)
    {
        state.ShiftTextAndLineMatrix(tx, ty);
    }

    public static void Apply(ref GfxState state, decimal tx, decimal ty)
    {
        state.ShiftTextAndLineMatrix(tx, ty);
    }
}
public partial class TD_Op
{
    public void Apply(ref GfxState state)
    {
        state = state with { TextLeading = -ty };
        state.ShiftTextAndLineMatrix(tx, ty);
    }
}

public partial class Tm_Op
{
    public void Apply(ref GfxState state)
    {
        state.Text.TextLineMatrix = new GfxMatrix(a, b, c, d, e, f);
        state.Text.TextMatrix = state.Text.TextLineMatrix;
    }

    public static void WriteLn(GfxMatrix tm, Stream stream)
    {
        Write(tm.A, tm.B, tm.C, tm.D, tm.E, tm.F, stream);
        stream.WriteByte((byte)'\n');
    }

}

public partial class T_Star_Op
{
    public void Apply(ref GfxState state)
    {
        state.ShiftTextAndLineMatrix(0, -state.TextLeading);
    }
}

public partial class Tj_Op
{
    public void Apply(ref GfxState state)
    {
        state.ApplyData(text);
    }
}

public partial class doublequote_Op
{
    public void Apply(ref GfxState state)
    {
        state = state with { WordSpacing = aw, CharSpacing = ac };
        T_Star_Op.Value.Apply(ref state);
        state.ApplyData(text);
    }
}

public partial class singlequote_Op
{
    public void Apply(ref GfxState state)
    {
        T_Star_Op.Value.Apply(ref state);
        state.ApplyData(text);
    }
}

public partial class TJ_Op
{
    public void Apply(ref GfxState state)
    {
        foreach (var item in info)
        {
            if (item.Shift != 0m)
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

public partial class BT_Op
{
    public void Apply(ref GfxState state)
    {
        state.Text.TextLineMatrix = GfxMatrix.Identity;
        state.Text.TextMatrix = GfxMatrix.Identity;
        state.UpdateTRM();
    }
}
