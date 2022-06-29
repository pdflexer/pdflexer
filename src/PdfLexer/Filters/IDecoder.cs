using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.Filters
{
    public interface IDecoder
    {
        /// <summary>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="filterParams"></param>
        /// <returns></returns>
        Stream Decode(Stream stream, PdfDictionary filterParams);
    }

    public interface IEncoder
    {
        /// <summary>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="filterParams"></param>
        /// <returns></returns>
        (Stream Data, PdfName Filter, PdfDictionary? Params) Encode(Stream source);
    }
}
