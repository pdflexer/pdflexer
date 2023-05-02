using PdfLexer.Content;
using PdfLexer.Serializers;
using System.IO;
using System.Numerics;

namespace PdfLexer.Operators;



public partial class BDC_Op<T>
{
    public void Serialize(Stream stream)
    {
        PdfOperator.Shared.SerializeObject(stream, tag, x => x);
        stream.WriteByte((byte)' ');
        PdfOperator.Shared.SerializeObject(stream, props, x => x);
        stream.WriteByte((byte)' ');
        stream.Write(OpData);
    }
}

public partial class d_Op<T>
{
    public static d_Op<T> Default { get; } = new d_Op<T>(new PdfArray(), T.Zero);
    public void Serialize(Stream stream)
    {
        PdfOperator.Shared.SerializeObject(stream, dashArray, x => x);
        stream.WriteByte((byte)' ');
        FPC<T>.Util.Write(dashPhase, stream);
        stream.WriteByte((byte)' ');
        stream.Write(OpData);
    }
}

public partial class DP_Op<T>

{
    public void Serialize(Stream stream)
    {
        PdfOperator.Shared.SerializeObject(stream, tag, x => x);
        stream.WriteByte((byte)' ');
        PdfOperator.Shared.SerializeObject(stream, props, x => x);
        stream.WriteByte((byte)' ');
        stream.Write(OpData);
    }
}


public partial class SC_Op<T>
{
    public void Serialize(Stream stream)
    {
        foreach (var val in colorInfo)
        {
            FPC<T>.Util.Write(val, stream);
            stream.WriteByte((byte)' ');
        }
        stream.Write(OpData);
    }
}



public partial class sc_Op<T>
{
    public void Serialize(Stream stream)
    {
        foreach (var val in colorInfo)
        {
            FPC<T>.Util.Write(val, stream);
            stream.WriteByte((byte)' ');
        }
        stream.Write(OpData);
    }
}


public partial class SCN_Op<T>
{
    public void Serialize(Stream stream)
    {
        foreach (var val in colorInfo)
        {
            FPC<T>.Util.Write(val, stream);
            stream.WriteByte((byte)' ');
        }
        if (name != null)
        {
            PdfOperator.WritePdfName(name, stream);
            stream.WriteByte((byte)' ');
        }
        stream.Write(OpData);
    }
}


public partial class scn_Op<T>
{
    public void Serialize(Stream stream)
    {
        foreach (var val in colorInfo)
        {
            FPC<T>.Util.Write(val, stream);
            stream.WriteByte((byte)' ');
        }
        if (name != null)
        {
            PdfOperator.WritePdfName(name, stream);
            stream.WriteByte((byte)' ');
        }
        stream.Write(OpData);
    }
}

public partial class TJ_Op<T>
{
    public void Serialize(Stream stream)
    {
        stream.WriteByte((byte)'[');
        foreach (var item in info)
        {
            if (item.Data != null)
            {
                StringSerializer.WriteToStream(item.Data, stream);
            }
            else
            {
                FPC<T>.Util.Write(item.Shift, stream);
            }
            stream.WriteByte((byte)' ');
        }
        stream.WriteByte((byte)']');
        stream.Write(OpData);
    }
}

public partial class Tj_Op<T>
{
    public void Serialize(Stream stream)
    {
        StringSerializer.WriteToStream(text, stream);
        stream.WriteByte((byte)' ');
        stream.Write(OpData);
    }
}


public partial class singlequote_Op<T>
{
    public void Serialize(Stream stream)
    {
        StringSerializer.WriteToStream(text, stream);
        stream.WriteByte((byte)' ');
        stream.Write(OpData);
    }
}


public partial class doublequote_Op<T>
{
    public void Serialize(Stream stream)
    {
        FPC<T>.Util.Write(aw, stream);
        stream.WriteByte((byte)' ');
        FPC<T>.Util.Write(ac, stream);
        stream.WriteByte((byte)' ');
        StringSerializer.WriteToStream(text, stream);
        stream.WriteByte((byte)' ');
        stream.Write(OpData);
    }
}





public struct TJ_Item<T> where T : struct, IFloatingPoint<T>
{
    public T Shift;
    public byte[] Data;
}
internal struct TJ_Lazy_Item<T> where T : struct, IFloatingPoint<T>
{
    public T Shift;
    public int OpNum;
}



#if false

public partial class Tc_Op
{
    public void Apply(TextState state)
    {
        state.Apply(this);
    }
}

public partial class Tw_Op
{
    public void Apply(TextState state)
    {
        state.Apply(this);
    }
}

public partial class Tz_Op
{
    public void Apply(TextState state)
    {
        state.Apply(this);
    }
}

public partial class TL_Op
{
    public void Apply(TextState state)
    {
        state.Apply(this);
    }
}

public partial class Tf_Op
{
    public void Apply(TextState state)
    {
        state.Apply(this);
    }
}

public partial class Tr_Op
{
    public void Apply(TextState state)
    {
        state.Apply(this);
    }
}

public partial class Ts_Op
{
    public void Apply(TextState state)
    {
        state.Apply(this);
    }
}

public partial class Td_Op
{
    public void Apply(TextState state)
    {
        state.Apply(this);
    }
}
public partial class TD_Op
{
    public void Apply(TextState state)
    {
        state.Apply(this);
    }
}

public partial class Tm_Op
{
    public void Apply(TextState state)
    {
        state.Apply(this);
    }
}

public partial class T_Star_Op
{
    public void Apply(TextState state)
    {
        state.Apply(this);
    }
}

public partial class Tj_Op
{
    public void Apply(TextState state)
    {
        state.Apply(this);
    }
}

public partial class doublequote_Op
{
    public void Apply(TextState state)
    {
        state.Apply(this);
    }
}

public partial class singlequote_Op
{
    public void Apply(TextState state)
    {
        state.Apply(this);
    }
}

public partial class TJ_Op
{
    public void Apply(TextState state)
    {
        state.Apply(this);
    }
}

public partial class gs_Op
{
    public void Apply(TextState state)
    {
        state.Apply(this);
    }
}

public partial class Q_Op
{
    public void Apply(ref GraphicsState state)
    {
        if (state.Prev == null)
        {
            // err
            state = new GraphicsState();
            return;
        }
        state = state.Prev;
    }

}



public partial class q_Op
{
    public void Apply(ref GraphicsState state)
    {
        var prev = state;
        state = state.CloneNoPrev();
        state.Prev = prev;
    }
}



public partial class cm_Op
{
    public void Apply(ref GraphicsState state)
    {
        // new = cm x ctm
        var val = new Matrix4x4(
                      (float)a, (float)b, 0f, 0f,
                      (float)c, (float)d, 0f, 0f,
                      (float)e, (float)f, 1f, 0f,
                      0f, 0f, 0f, 1f);
        state.CTM = val * state.CTM;
    }

    public static void Apply(ref GraphicsState state, Matrix4x4 val)
    {
        state.CTM = val * state.CTM;
    }

    public static void WriteLn(Matrix4x4 ctm, Stream stream)
    {
        WriteLn(
            (double)ctm.M11, (double)ctm.M12,
            (double)ctm.M21, (double)ctm.M22,
            (double)ctm.M31, (double)ctm.M32, stream);
    }

}

#endif
