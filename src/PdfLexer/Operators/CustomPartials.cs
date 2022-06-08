﻿using PdfLexer.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.Operators
{
    public partial class BDC_Op
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
    public partial class DP_Op
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

    public partial class SC_Op
    {
        public void Serialize(Stream stream)
        {
            foreach (var dec in colorInfo)
            {
                PdfOperator.Writedecimal(dec, stream);
                stream.WriteByte((byte)' ');
            }
            stream.Write(OpData);
        }
    }

    public partial class sc_Op
    {
        public void Serialize(Stream stream)
        {
            foreach (var dec in colorInfo)
            {
                PdfOperator.Writedecimal(dec, stream);
                stream.WriteByte((byte)' ');
            }
            stream.Write(OpData);
        }
    }

    public partial class SCN_Op
    {
        public void Serialize(Stream stream)
        {
            foreach (var dec in colorInfo)
            {
                PdfOperator.Writedecimal(dec, stream);
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

    public partial class scn_Op
    {
        public void Serialize(Stream stream)
        {
            foreach (var dec in colorInfo)
            {
                PdfOperator.Writedecimal(dec, stream);
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

    public partial class TJ_Op
    {
        public void Serialize(Stream stream)
        {
            stream.WriteByte((byte)'[');
            foreach (var item in info)
            {
                if (item.Data != null)
                {
                    PdfOperator.Shared.StringSerializer.WriteToStream(item.Data, stream);
                    // PdfOperator.Shared.StringSerializer.WriteToStream(item.Text, stream);
                }
                else
                {
                    PdfOperator.Writedecimal(item.Shift, stream);
                }
                stream.WriteByte((byte)' ');
            }
            stream.WriteByte((byte)']');
            stream.Write(OpData);
        }
    }

    public struct TJ_Item
    {
        public decimal Shift;
        public byte[] Data;
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
}