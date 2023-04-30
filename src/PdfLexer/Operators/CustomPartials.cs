using PdfLexer.Content;
using PdfLexer.Serializers;
using System.IO;
using System.Numerics;

namespace PdfLexer.Operators;



#if NET7_0_OR_GREATER
public partial class BDC_Op<T>
#else
public partial class BDC_Op
#endif
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

#if NET7_0_OR_GREATER
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
#else

public partial class d_Op
{
    public static d_Op Default { get; } = new d_Op(new PdfArray(), 0);
    public void Serialize(Stream stream)
    {
        PdfOperator.Shared.SerializeObject(stream, dashArray, x => x);
        stream.WriteByte((byte)' ');
        PdfOperator.Writedecimal((decimal)dashPhase, stream);
        stream.WriteByte((byte)' ');
        stream.Write(OpData);
    }
}
#endif

#if NET7_0_OR_GREATER
public partial class DP_Op<T>
#else
public partial class DP_Op
#endif
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


#if NET7_0_OR_GREATER
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
#else

public partial class SC_Op
{
    public void Serialize(Stream stream)
    {
        foreach (var dec in colorInfo)
        {
            PdfOperator.Writedecimal((decimal)dec, stream);
            stream.WriteByte((byte)' ');
        }
        stream.Write(OpData);
    }
}
#endif


#if NET7_0_OR_GREATER
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
#else

public partial class sc_Op
{
    public void Serialize(Stream stream)
    {
        foreach (var dec in colorInfo)
        {
            PdfOperator.Writedecimal((decimal)dec, stream);
            stream.WriteByte((byte)' ');
        }
        stream.Write(OpData);
    }
}
#endif



#if NET7_0_OR_GREATER
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
#else

public partial class SCN_Op
{
    public void Serialize(Stream stream)
    {
        foreach (var dec in colorInfo)
        {
            PdfOperator.Writedecimal((decimal)dec, stream);
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
#endif



#if NET7_0_OR_GREATER
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
#else

public partial class scn_Op
{
    public void Serialize(Stream stream)
    {
        foreach (var dec in colorInfo)
        {
            PdfOperator.Writedecimal((decimal)dec, stream);
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
#endif



#if NET7_0_OR_GREATER
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
#else
public partial class TJ_Op
{
    public void Serialize(Stream stream)
    {
        stream.WriteByte((byte)'[');
        foreach (var item in info)
        {
            if (item.Data != null)
            {
                StringSerializer.WriteToStream(item.Data, stream);
                // PdfOperator.Shared.StringSerializer.WriteToStream(item.Text, stream);
            }
            else
            {
                PdfOperator.Writedouble(item.Shift, stream);
            }
            stream.WriteByte((byte)' ');
        }
        stream.WriteByte((byte)']');
        stream.Write(OpData);
    }
}
#endif


#if NET7_0_OR_GREATER
public partial class Tj_Op<T>
#else
public partial class Tj_Op
#endif
{
    public void Serialize(Stream stream)
    {
        StringSerializer.WriteToStream(text, stream);
        stream.WriteByte((byte)' ');
        stream.Write(OpData);
    }
}

#if NET7_0_OR_GREATER
public partial class singlequote_Op<T>
#else
public partial class singlequote_Op
#endif
{
    public void Serialize(Stream stream)
    {
        StringSerializer.WriteToStream(text, stream);
        stream.WriteByte((byte)' ');
        stream.Write(OpData);
    }
}

#if NET7_0_OR_GREATER
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
#else

public partial class doublequote_Op
{
    public void Serialize(Stream stream)
    {
        PdfOperator.Writedecimal((decimal)aw, stream);
        stream.WriteByte((byte)' ');
        PdfOperator.Writedecimal((decimal)ac, stream);
        stream.WriteByte((byte)' ');
        StringSerializer.WriteToStream(text, stream);
        stream.WriteByte((byte)' ');
        stream.Write(OpData);
    }
}
#endif

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

public struct TJ_Item
{
    public double Shift;
    public byte[] Data;
}
internal struct TJ_Lazy_Item
{
    public double Shift;
    public int OpNum;
}

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

public partial class NoOp_Op : IPdfOperation
{
    public NoOp_Op Value { get; } = new NoOp_Op();
    public PdfOperatorType Type => PdfOperatorType.NoOp;
    public NoOp_Op()
    {
    }

    public void Serialize(Stream stream)
    {
    }
}
