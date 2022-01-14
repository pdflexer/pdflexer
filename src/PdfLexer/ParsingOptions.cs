using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer
{
    /// <summary>
    /// Options to customize how PDFs are parsed.
    /// </summary>
    public class ParsingOptions
    {
        /// <summary>
        /// Determines if the page tree should automatically be parsed and loaded
        /// when a PDF document is opened.
        /// Default: true
        /// </summary>
        public bool LoadPageTree { get; set; } = true;
        /// <summary>
        /// Laziness of parsing. If lazy will not parse nested items until they are
        /// requested.
        /// Default: Lazy
        /// </summary>
        public Eagerness Eagerness { get; set; } = Eagerness.Lazy;
        /// <summary>
        /// If numbers should be cached to reduce object allocations
        /// Default: false -> benchmarking shows bad for perf, UTF8Parser perf
        /// is very good for numbers already
        /// </summary>
        public bool CacheNumbers { get; set; } = false;
        /// <summary>
        /// If names should be cached to reduce object allocations and 
        /// parsing overhead.
        /// Default: true
        /// </summary>
        public bool CacheNames { get; set; } = true;

        public bool ThrowOnErrors { get;set;} = false;
        public bool ForceSerialize { get;set;} = false;
        // for perf testing
        public bool LazyStrings { get; set; } = false;
    }

    public enum Eagerness
    {
        Lazy,
        // EagerLazy, TODO
        FullEager
    }
}
