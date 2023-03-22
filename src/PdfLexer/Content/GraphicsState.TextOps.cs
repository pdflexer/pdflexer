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
        state = state with { CharSpacing = (float)charSpace };
    }
}

public partial class Tw_Op
{
    public void Apply(ref GfxState state)
    {
        state = state with { WordSpacing = (float)wordSpace };
    }
}

public partial class Tz_Op
{
    public void Apply(ref GfxState state)
    {
        state = state with { TextHScale = (float)scale / 100.0F };
        state.UpdateTRM();
    }
}

public partial class TL_Op
{
    public void Apply(ref GfxState state)
    {
        state = state with { TextLeading = (float)leading };
    }
}

public partial class Tf_Op
{
    public void Apply(ref GfxState state)
    {
        throw new NotSupportedException();
    }

    public static void Apply(ref GfxState state, PdfDictionary font, IReadableFont readable, float size)
    {
        state = state with { FontSize = size, FontObject = font,  Font = readable };
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
        state = state with { TextRise = (float)rise };
        state.UpdateTRM();
    }
}

public partial class Td_Op
{
    public void Apply(ref GfxState state)
    {
        state.ShiftTextAndLineMatrix((float)tx, (float)ty);
    }

    public static void Apply(ref GfxState state, float tx, float ty)
    {
        state.ShiftTextAndLineMatrix((float)tx, (float)ty);
    }
}
public partial class TD_Op
{
    public void Apply(ref GfxState state)
    {
        state = state with { TextLeading = (float)-ty };
        state.ShiftTextAndLineMatrix((float)tx, (float)ty);
    }
}

public partial class Tm_Op
{
    public void Apply(ref GfxState state)
    {
        state.Text.TextLineMatrix = new Matrix4x4(
                      (float)a, (float)b, 0f, 0f,
                      (float)c, (float)d, 0f, 0f,
                      (float)e, (float)f, 1f, 0f,
                      0f, 0f, 0f, 1f);

        state.Text.TextMatrix = state.Text.TextLineMatrix;
    }

    public static void WriteLn(Matrix4x4 tm, Stream stream)
    {
        Write((decimal)tm.M11, (decimal)tm.M12, (decimal)tm.M21, (decimal)tm.M22, (decimal)tm.M31, (decimal)tm.M32, stream);
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
        state = state with { WordSpacing = (float)aw, CharSpacing = (float)ac };
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
                state.ApplyTj((float)item.Shift);
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
        state.Text.TextLineMatrix = Matrix4x4.Identity;
        state.Text.TextMatrix = Matrix4x4.Identity;
        state.UpdateTRM();
    }
}
