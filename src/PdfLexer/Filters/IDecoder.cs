using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.Filters
{
    public interface IDecoder
    {
        /// <summary>
        /// For now use streams... look into if there are better ways.
        /// Would like to use ReadOnlySpan input and provided a buffer to write to
        /// but with unknown content length after decoding may be difficult.
        /// TODO: some perf testing / research to determine if worth it
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="filterParams"></param>
        /// <returns></returns>
        Stream Decode(Stream stream, PdfDictionary filterParams);
    }
}
