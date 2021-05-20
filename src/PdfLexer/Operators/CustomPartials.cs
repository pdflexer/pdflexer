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
            PdfOperator.Shared.SerializeObject(stream, tag, x=>x);
            stream.WriteByte((byte)' ');
            PdfOperator.Shared.SerializeObject(stream, props, x=>x);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }
    }
    public partial class DP_Op
    {
        public void Serialize(Stream stream)
        {
            PdfOperator.Shared.SerializeObject(stream, tag, x=>x);
            stream.WriteByte((byte)' ');
            PdfOperator.Shared.SerializeObject(stream, props, x=>x);
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
    
    
}
