using PdfLexer.DOM;
using PdfLexer.IO;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PdfLexer
{
    public class PdfDocument : IDisposable
    {
        internal int DocumentId { get; } = Interlocked.Increment(ref docId);
        public ParsingContext Context { get; }
        public decimal PdfVersion { get; set; }
        public PdfDictionary Trailer { get; }
        public PdfDictionary Catalog { get; internal set; }
        public List<PdfPage> Pages { get; internal set; }

        public IReadOnlyDictionary<XRef, XRefEntry> XrefEntries {get;}
        internal int NextObjectNumber {get; set;}

        internal PdfDocument(ParsingContext ctx, PdfDictionary trailer, Dictionary<XRef, XRefEntry> entries)
        {
            Context = ctx;
            ctx.SourceId = DocumentId;
            ctx.Document = this;
            XrefEntries = entries;
            Trailer = trailer;
            NextObjectNumber = entries.Max(x=>x.Key.ObjectNumber) + 1;
        }



        public void Dispose()
        {
        }

        public static PdfDocument Create()
        {
            var doc = new PdfDocument(new ParsingContext(), new PdfDictionary(), new Dictionary<XRef, XRefEntry>());
            doc.Catalog = new PdfDictionary();
            doc.Catalog[PdfName.TypeName] = PdfName.Catalog;
            doc.Pages = new List<PdfPage>();
            return doc;
        }

        public static async ValueTask<PdfDocument> Open(byte[] data, ParsingOptions options=null)
        {
            var ctx = new ParsingContext(options);
            
            var source = new InMemoryDataSource(ctx, data);
            var result = await ctx.Initialize(source);
            var doc = new PdfDocument(ctx, result.Item2, result.Item1);
            doc.Catalog = doc.Trailer.GetRequiredValue<PdfDictionary>(PdfName.Root);
            var pages = doc.Catalog.GetRequiredValue<PdfDictionary>(PdfName.Pages);

            if (ctx.Options.LoadPageTree)
            {
                doc.Pages = EnumeratePages(pages, null, null, null, null).Select(x=>(PdfPage)x).ToList();
            }

            return doc;

            // TODO move somewhere else
            IEnumerable<PdfDictionary> EnumeratePages(PdfDictionary dict, PdfDictionary resources, PdfArray mediabox, PdfArray cropbox, PdfNumber rotate)
            {
                var type = dict.GetRequiredValue<PdfName>(PdfName.TypeName);
                switch (type.Value)
                {
                    case "/Pages":
                        if (dict.TryGetValue<PdfDictionary>(PdfName.Resources, out var next))
                        {
                            resources = next;
                        }
                        if (dict.TryGetValue<PdfArray>(PdfName.MediaBox, out var thisMediaBox))
                        {
                            mediabox = thisMediaBox;
                        }
                        if (dict.TryGetValue<PdfArray>(PdfName.CropBox, out var thisCropBox))
                        {
                            cropbox = thisCropBox;
                        }
                        if (dict.TryGetValue<PdfNumber>(PdfName.Rotate, out var thisRotate))
                        {
                            rotate = thisRotate;
                        }


                        var kids = dict.GetRequiredValue<PdfArray>(PdfName.Kids);
                        foreach (var child in kids)
                        {
                            foreach (var pg in EnumeratePages(child.GetValue<PdfDictionary>(), resources, mediabox, cropbox, rotate)) 
                            {
                                yield return pg;
                            }
                        }
                        break;
                    case "/Page":
                        if (!dict.ContainsKey(PdfName.Resources) && resources != null)
                        {
                            dict[PdfName.Resources] = resources;
                        }
                        if (!dict.ContainsKey(PdfName.MediaBox) && mediabox != null)
                        {
                            dict[PdfName.MediaBox] = mediabox;
                        }
                        if (!dict.ContainsKey(PdfName.CropBox) && cropbox != null)
                        {
                            dict[PdfName.CropBox] = cropbox;
                        }
                        if (!dict.ContainsKey(PdfName.Rotate) && rotate != null)
                        {
                            dict[PdfName.Rotate] = rotate;
                        }
                        yield return dict;
                        break;
                }
            }

            // inheritable
            // Resources required (dictionary)
            // MediaBox required (rectangle)
            // CropBox => default to MediaBox (rectangle)
            // Rotate (integer)
        }

        private static int docId = 0;
        internal static int GetNextId() => Interlocked.Increment(ref docId);
    }
}
