using PdfLexer.DOM;
using PdfLexer.IO;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PdfLexer
{
    /// <summary>
    /// Represents a single PDF document.
    /// </summary>
    public class PdfDocument : IDisposable
    {
        /// <summary>
        /// Id of PDF, used for tracking indirect references between documents. 
        /// </summary>
        internal int DocumentId { get; } = Interlocked.Increment(ref docId);
        /// <summary>
        /// Parsing context for this PDF. May be internalized but may provide external access to allow parallel processing at some point.
        /// </summary>
        public ParsingContext Context { get; }
        /// <summary>
        /// Version of the PDF document.
        /// </summary>
        public decimal PdfVersion { get; set; } = 1.7m; // TODO
        /// <summary>
        /// PDF trailer dictionary.
        /// Note: The /Root entry pointing to the PDF catalog will be overwritten if PDF is saved.
        /// </summary>
        public PdfDictionary Trailer { get; }
        /// <summary>
        /// PDF catalog dictionary.
        /// Note: The /Pages entry pointing to the page tree will be overwritten if PDF is saved and <see cref="Pages"/> is not null.
        /// </summary>
        public PdfDictionary Catalog { get; set; }
        /// <summary>
        /// List of pages in the document. May be null if <see cref="ParsingOptions.LoadPageTree"/> is false.
        /// </summary>
        public List<PdfPage> Pages { get; set; }
        /// <summary>
        /// XRef entries of this document. May be internalized at some point.
        /// Will be null on new documents.
        /// </summary>
        public IReadOnlyDictionary<XRef, XRefEntry> XrefEntries { get; }
        

        internal PdfDocument(ParsingContext ctx, PdfDictionary trailer, Dictionary<XRef, XRefEntry> entries)
        {
            Context = ctx;
            ctx.SourceId = DocumentId;
            ctx.Document = this;
            XrefEntries = entries;
            Trailer = trailer;
        }

        public void Dispose()
        {
            // TODO
            // context disposing -> currently in memory only
        }

        /// <summary>
        /// Saves the document to the provided stream.
        /// </summary>
        /// <param name="stream"></param>
        public void SaveTo(Stream stream)
        {
            var nextId =  XrefEntries.Keys.Select(x=>x.ObjectNumber).Max() + 1;
            var ctx = new WritingContext(stream, nextId, DocumentId);
            ctx.Initialize(PdfVersion);
            foreach (var obj in XrefEntries.Values)
            {
                if (obj.IsFree)
                {
                    continue;
                }
                if (IsDataCopyable(obj.Reference))
                {
                    ctx.WriteExistingData(Context, obj);
                } else
                {
                    ctx.WriteIndirectObject(new ExistingIndirectRef(Context, obj.Reference));
                }
            }
            ctx.Complete(Trailer);
        }

        // TODO move elsewhere
        private bool IsDataCopyable(XRef entry)
        {
                ulong id = ((ulong)entry.ObjectNumber << 16) | ((uint)entry.Generation & 0xFFFF);
                if (Context.IndirectCache.TryGetValue(id, out var value))
                {
                    switch (value.Type)
                    {
                        case PdfObjectType.ArrayObj:
                            var arr = (PdfArray)value;
                            return !arr.IsModified;
                        case PdfObjectType.DictionaryObj:
                            var dict = (PdfDictionary)value;
                            return !dict.IsModified;
                    }
                    return true;
                }
                return true;
        }

        /// <summary>
        /// Create a new empty PDF document.
        /// </summary>
        /// <returns>PdfDocument</returns>
        public static PdfDocument Create()
        {
            var doc = new PdfDocument(new ParsingContext(), new PdfDictionary(), new Dictionary<XRef, XRefEntry>());
            doc.Catalog = new PdfDictionary();
            doc.Catalog[PdfName.TypeName] = PdfName.Catalog;
            doc.Pages = new List<PdfPage>();
            return doc;
        }

        /// <summary>
        /// Opens a PDF document from the provided byte array.
        /// TODO turn this into sync code.
        /// </summary>
        /// <param name="data">PDF data</param>
        /// <param name="options">Optional parsing options</param>
        /// <returns>PdfDocument</returns>
        public static async ValueTask<PdfDocument> Open(byte[] data, ParsingOptions options=null)
        {
            var ctx = new ParsingContext(options);
            
            var source = new InMemoryDataSource(ctx, data);
            var result = await ctx.Initialize(source);


            var doc = new PdfDocument(ctx, result.Item2, result.Item1);
            // TODO: clean the existing ref ID up
            foreach (var item in result.Item2.Values)
            {
                if (item.Type == PdfObjectType.IndirectRefObj)
                {
                    var eir = (ExistingIndirectRef)item;
                    eir.SourceId = doc.DocumentId;
                }
            }
            doc.Catalog = doc.Trailer.GetRequiredValue<PdfDictionary>(PdfName.Root);
            var pages = doc.Catalog.GetRequiredValue<PdfDictionary>(PdfName.Pages);

            if (ctx.Options.LoadPageTree)
            {
                doc.Pages = new List<PdfPage>();
                foreach (var pg in EnumeratePages(pages, null, null, null, null))
                {
                    doc.Pages.Add(pg);
                }
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
