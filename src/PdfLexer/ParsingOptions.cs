using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer
{
    public class ParsingOptions
    {
        public bool LoadPageTree { get; set; } = true;
        public Eagerness Eagerness { get; set; } = Eagerness.FullEager;
        public bool CacheObjects { get; set; } = true;
        public bool CacheNumbers { get; set; } = true;
        public bool CacheNames { get; set; } = true;
    }

    public enum Eagerness
    {
        Lazy,
        // EagerLazy, TODO
        FullEager
    }
}
