using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.Serializers
{
    // public class HexSerializer : ISerializer<>
    // {

    // 
    //     public void WriteToStream(PdfHex obj, Stream stream)
    //     {
    //         stream.WriteByte((byte)'<');
    //         for (var i = 0; i < obj.Value.Length; i++)
    //         {
    //             var b = obj.Value[i];
    //             int high = ((b & 0xf0) >> 4);
    //             int low = (b & 0x0f);
    //             stream.WriteByte(hexVals[high]);
    //             stream.WriteByte(hexVals[low]);
    //         }
    //         stream.WriteByte((byte)'>');
    //     }
    // }
}
