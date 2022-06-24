using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.Filters
{
    public interface IDecoder
    {
        /// <summary>
        /// For now use streams... look into if there are better ways
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="filterParams"></param>
        /// <returns></returns>
        Stream Decode(Stream stream, PdfDictionary filterParams);
    }
}
