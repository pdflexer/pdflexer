using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLexer.Serializers
{
    public interface ISerializer<in T> where T : IPdfObject
    {
        /// <summary>
        /// Writers the given PDF object to the stream.
        /// </summary>
        /// <param name="obj">PDF object to write.</param>
        /// <param name="stream">Stream to write to.</param>
        void WriteToStream(T obj, Stream stream);
        /// <summary>
        /// Write the given PDF object to the buffer.
        /// </summary>
        /// <param name="obj">PDF object to write.</param>
        /// <param name="data">Byte buffer</param>
        /// <returns>Number of bytes written.</returns>
        int GetBytes(T obj, Span<byte> data);
}
}
