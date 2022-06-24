using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Parsers
{
    internal class NameCache
    {
        internal Dictionary<ulong, PdfName> Cache = new Dictionary<ulong, PdfName>();
        public NameCache()
        {
                
        }

        public bool TryGetName(ReadOnlySpan<byte> data, out ulong key, out PdfName value)
        {
            key = 0;
            if (data.Length > 9)
            {
                value = null;
                return false;
            }

            for(int i=1;i<data.Length;i++)
            {
                key = key | ((ulong)data[i] << 8*(i-1));
            }

            if (Cache.TryGetValue(key, out var item))
            {
                value = item;
                return true;
            }
            value = null;
            return false;
        }
        public void AddValue(ulong key, PdfName value)
        {
            value.CacheValue = key;
            Cache[key] = value;
        }
    }
}
