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
        public IReadOnlyDictionary<ulong, XRefEntry> XrefEntries { get; }
        

        internal PdfDocument(ParsingContext ctx, PdfDictionary trailer, Dictionary<ulong, XRefEntry> entries)
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
            var nums = XrefEntries?.Values.Select(x=>x.Reference.ObjectNumber).ToList();
            var nextId = 1;
            if (nums.Any())
            {
                nextId = nums.Max()+1;
            }
            var ctx = new WritingContext(stream, nextId, DocumentId);
            ctx.Initialize(PdfVersion);
            if (XrefEntries != null)
            {
                SaveExistingObjects(ctx);
            }
            // create clones of these in case they were
            // copied from another doc, don't want to modify existing
            var catalog = Catalog.CloneShallow();
            catalog.Remove("/Names");
            catalog.Remove("/Outlines");
            catalog.Remove("/StructTreeRoot");
            var trailer = Trailer.CloneShallow();
            var cir =  PdfIndirectRef.Create(catalog);
            trailer[PdfName.Root] = cir;
            trailer.Remove(PdfName.DecodeParms);
            trailer.Remove(PdfName.Filter);
            trailer.Remove(PdfName.Length);
            trailer.Remove(PdfName.Prev);
            trailer.Remove(PdfName.XRefStrm);
            if (Pages != null)
            {
                catalog[PdfName.Pages] = BuildPageTree();
            }
            ctx.Complete(trailer);
        }

        private IPdfObject BuildPageTree()
        {
            // TODO page tree
            var dict = new PdfDictionary();
            var arr = new PdfArray();
            var ir = PdfIndirectRef.Create(dict);
            foreach(var page in Pages)
            {
                var pg = page.Dictionary.CloneShallow();
                pg[PdfName.Parent] = ir;
                arr.Add(PdfIndirectRef.Create(pg));
            }
            dict[PdfName.Kids] = arr;
            dict[PdfName.TypeName] = PdfName.Pages;
            dict[PdfName.Count] = new PdfIntNumber(Pages.Count);
            return PdfIndirectRef.Create(dict);
        }

        private void SaveExistingObjects(WritingContext ctx)
        {
            foreach (var obj in XrefEntries.Values)
            {
                if (obj.IsFree)
                {
                    continue;
                }
                if (obj.Type == XRefType.Normal && obj.Offset == 0)
                {
                    // buggy PDFs
                    continue;
                }
                if (obj.Type == XRefType.Normal && Context.IsDataCopyable(obj.Reference)) // TODO copying of compressed items
                {
                    ctx.WriteExistingData(Context, obj);
                } else
                {
                    ctx.WriteIndirectObject(new ExistingIndirectRef(Context, obj.Reference));
                }
            }
        }

        /// <summary>
        /// Create a new empty PDF document.
        /// </summary>
        /// <returns>PdfDocument</returns>
        public static PdfDocument Create()
        {
            var doc = new PdfDocument(new ParsingContext(), new PdfDictionary(), new Dictionary<ulong, XRefEntry>());
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
        public static PdfDocument Open(byte[] data, ParsingOptions options=null)
        {
            var ctx = new ParsingContext(options);
            
            var source = new InMemoryDataSource(ctx, data);
            var result = ctx.Initialize(source);


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
                foreach (var pg in CommonUtil.EnumeratePageTree(pages))
                {
                    doc.Pages.Add(pg);
                }
            }

            return doc;
        }

        private static int docId = 0;
        internal static int GetNextId() => Interlocked.Increment(ref docId);
    }
}
