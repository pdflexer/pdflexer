using PdfLexer.IO;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLexer
{
    public class PdfDocument : IDisposable
    {
        private Stream _stream;
        private bool _disposeStream;

        public ParsingContext Context { get; }
        public PdfDictionary Trailer {get;}
        public IReadOnlyDictionary<XRef, XRefEntry> XrefEntries {get;}
        internal int NextObjectNumber {get; set;}

        public PdfDocument(ParsingContext ctx, PdfDictionary trailer, Dictionary<XRef, XRefEntry> entries, int nextObj)
        {
            Context = ctx;
            XrefEntries = entries;
            Trailer = trailer;
            NextObjectNumber = entries.Max(x=>x.Key.ObjectNumber) + 1;
        }

        public void Dispose()
        {
            if (_disposeStream)
            {
                _stream?.Dispose();
            }
        }

        public static async ValueTask<PdfDocument> Open(byte[] data)
        {
            var ctx = new ParsingContext();
            var source = new InMemoryDataSource(ctx, data);
            var result = await ctx.Initialize(source);
            var doc = new PdfDocument(ctx, result.Item2, result.Item1, 0);
            ctx.Document = doc;
            return doc;
        }
    }
}
