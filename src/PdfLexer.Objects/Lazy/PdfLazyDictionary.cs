using System.Collections.Generic;

namespace PdfLexer.Objects.Lazy
{
    public class PdfLazyDictionary
    {
        public PdfLazyDictionary()
        {
            Values = new Dictionary<string, PdfLazyObject>();
        }
        public Dictionary<string, PdfLazyObject> Values { get; set; }
        /// <summary>
        /// Signifies if this PDF dictionary includes a content stream
        /// </summary>
        /// <value></value>
        public bool HasStream { get; set; }
        /// <summary>
        /// Byte offset to start of dictionary stream
        /// </summary>
        /// <value></value>
        public ulong StreamOffset { get; set; }
    }
}