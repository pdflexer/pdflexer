using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.Serializers
{
    public class HexSerializer : ISerializer<PdfHex>
    {
        private static byte[] hexVals = { (byte) '0',(byte) '1',(byte) '2', (byte) '3',(byte) '4', (byte)'5',(byte) '6',(byte) '7', (byte)'8',(byte) '9',
                            (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F' };
        public int GetBytes(PdfHex obj, Span<byte> data)
        {
            var di = 0;
            data[di++] = (byte)'<';
            for (var i = 0; i < obj.Value.Length; i++)
            {
                var b = obj.Value[i];
                int high = ((b & 0xf0) >> 4);
                int low = (b & 0x0f);
                data[di++] = hexVals[high];
                data[di++] = hexVals[low];
            }
            data[di++] = (byte)'>';
            return di;
        }

        public void WriteToStream(PdfHex obj, Stream stream)
        {
            stream.WriteByte((byte)'<');
            for (var i = 0; i < obj.Value.Length; i++)
            {
                var b = obj.Value[i];
                int high = ((b & 0xf0) >> 4);
                int low = (b & 0x0f);
                stream.WriteByte(hexVals[high]);
                stream.WriteByte(hexVals[low]);
            }
            stream.WriteByte((byte)'>');
        }
    }
}
