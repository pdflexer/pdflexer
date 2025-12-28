using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Filters;
using PdfLexer.IO;
using PdfLexer.Parsers.Structure;

namespace PdfLexer;

/// <summary>
/// Represents a single PDF document.
/// </summary>
public sealed partial class PdfDocument : IDisposable
{
    /// <summary>
    /// Id of PDF, used for tracking indirect references between documents. 
    /// </summary>
    internal int DocumentId { get; set; }
    /// <summary>
    /// Parsing context for this PDF. May be internalized but may provide external access to allow parallel processing at some point.
    /// </summary>
    public ParsingContext Context { get => ParsingContext.Current; }
    /// <summary>
    /// Version of the PDF document.
    /// </summary>
    public decimal PdfVersion { get; set; }
    /// <summary>
    /// PDF trailer dictionary.
    /// Note: The /Root entry pointing to the PDF catalog will be overwritten if PDF is saved.
    /// </summary>
    public PdfDictionary Trailer { get; private set; }
    /// <summary>
    /// PDF catalog dictionary.
    /// Note: The /Pages entry pointing to the page tree will be overwritten if PDF is saved and <see cref="Pages"/> is not null.
    /// </summary>
    public PdfDictionary Catalog { get; set; }
    /// <summary>
    /// List of pages in the document. May be null if <see cref="ParsingOptions.LoadPageTree"/> is false.
    /// </summary>
    public List<PdfPage> Pages { get; set; }

    private BookmarkNode? _outlines;
    private bool _outlinesRead;

    public BookmarkNode? Outlines
    {
        get
        {
            if (_outlinesRead)
            {
                return _outlines;
            }

            _outlines = Parsers.OutlineParser.Parse(this);
            _outlinesRead = true;
            return _outlines;
        }
        set
        {
            _outlines = value;
            _outlinesRead = true;
        }
    }

    private StructuralBuilder? _structure;
    public StructuralBuilder Structure
    {
        get
        {
            if (_structure == null)
            {
                _structure = new StructuralBuilder();
            }
            return _structure;
        }
        set => _structure = value;
    }
    /// <summary>
    /// XRef entries of this document. May be internalized at some point.
    /// Will be null on new documents.
    /// </summary>
    public IReadOnlyDictionary<ulong, XRefEntry> XrefEntries => XRefs;


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal PdfDocument()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        DocumentId = GetNextId();
        CryptFilter = new CryptFilter(this);
    }

#pragma warning disable CS8618
    internal PdfDocument(int id)
#pragma warning restore CS8618
    {
        DocumentId = id;
        CryptFilter = new CryptFilter(this);
    }

#pragma warning disable CS8618
    internal PdfDocument(int id, PdfDictionary catalog, PdfDictionary trailer, List<PdfPage> pages)
#pragma warning restore CS8618
    {
        DocumentId = id;
        Trailer = trailer;
        Pages = pages;
        Catalog = catalog;
        CryptFilter = new CryptFilter(this);
    }

    public void Dispose()
    {
        if (Trailer == null) { return; } // disposed
        foreach (var item in disposables)
        {
            item.Dispose();
        }
        Pages = null!;
        Catalog = null!;
        Trailer = null!;
    }

    public byte[] Save()
    {
        using var ms = new MemoryStream();
        SaveTo(ms);
        return ms.ToArray();
    }

    public PdfPage AddPage(PageSize size = PageSize.LETTER)
    {
        var pg = new PdfPage();
        pg.MediaBox = PageSizeHelpers.GetMediaBox(size);
        Pages.Add(pg);
        return pg;
    }

    public PdfPage AddPage(double width, double height)
    {
        var pg = new PdfPage();
        pg.MediaBox = new PdfArray { PdfCommonNumbers.Zero, PdfCommonNumbers.Zero, new PdfDoubleNumber(width), new PdfDoubleNumber(height) };
        Pages.Add(pg);
        return pg;
    }

    public void DeduplicateResources()
    {
        var dedup = new Deduplication(this);
        dedup.RunDeduplication();
    }



    /// <summary>
    /// Create a new empty PDF document.
    /// </summary>
    /// <returns>PdfDocument</returns>
    public static PdfDocument Create()
    {
        var doc = new PdfDocument(GetNextId(), new PdfDictionary { [PdfName.TypeName] = PdfName.Catalog }, new PdfDictionary(),
            new List<PdfPage>());
        return doc;
    }


    /// <summary>
    /// Opens a PDF document from the provided seekable stream.
    /// </summary>
    /// <param name="data">PDF data</param>
    /// <param name="options">Optional parsing options</param>
    /// <returns>PdfDocument</returns>
    public static PdfDocument Open(Stream data, DocumentOptions? options = null)
    {
        var ctx = ParsingContext.Current;
        var doc = new PdfDocument();
        doc.UserPass = options?.UserPass;
        doc.OwnerPass = options?.OwnerPass;
        var source = new StreamDataSource(doc, data);
        doc.Initialize(ctx, source);
        return doc;
    }

    /// <summary>
    /// Opens a PDF document from the provided seekable stream.
    /// </summary>
    /// <param name="data">PDF data</param>
    /// <param name="options">Optional parsing options</param>
    /// <returns>PdfDocument</returns>
    [Obsolete]
    public static PdfDocument Open(Stream data, ParsingOptions options)
    {
        var ctx = new ParsingContext(options);
        var doc = new PdfDocument();
        var source = new StreamDataSource(doc, data);
        doc.Initialize(ctx, source);
        return doc;
    }

    /// <summary>
    /// Opens a PDF document from the provided seekable stream.
    /// </summary>
    /// <param name="data">PDF data</param>
    /// <param name="options">Optional parsing options</param>
    /// <returns>PdfDocument</returns>
    [Obsolete]
    public static PdfDocument OpenLowMemory(Stream data, DocumentOptions? options = null)
    {
        var ctx = ParsingContext.CreateLowMemory();
        var doc = new PdfDocument();
        doc.UserPass = options?.UserPass;
        doc.OwnerPass = options?.OwnerPass;

        var source = new StreamDataSource(doc, data);
        doc.Initialize(ctx, source);
        return doc;
    }

    /// <summary>
    /// Opens a PDF document from the provided byte array.
    /// </summary>
    /// <param name="data">PDF data</param>
    /// <param name="options">Optional parsing options</param>
    /// <returns>PdfDocument</returns>
    public static PdfDocument Open(byte[] data, DocumentOptions? options = null)
    {
        var ctx = ParsingContext.Current;
        var doc = new PdfDocument();
        doc.UserPass = options?.UserPass;
        doc.OwnerPass = options?.OwnerPass;
        var source = new InMemoryDataSource(doc, data);
        doc.Initialize(ctx, source);
        return doc;
    }

    /// <summary>
    /// Opens a PDF document from the provided byte array.
    /// </summary>
    /// <param name="data">PDF data</param>
    /// <param name="options">Optional parsing options</param>
    /// <returns>PdfDocument</returns>
    [Obsolete]
    public static PdfDocument Open(byte[] data, ParsingOptions options)
    {
        var ctx = new ParsingContext(options);
        var doc = new PdfDocument();
        var source = new InMemoryDataSource(doc, data);
        doc.Initialize(ctx, source);
        return doc;
    }

    /// <summary>
    /// Opens a PDF document from the provided file path.
    /// </summary>
    /// <param name="data">PDF data</param>
    /// <param name="options">Optional parsing options</param>
    /// <returns>PdfDocument</returns>
    [Obsolete]
    public static PdfDocument Open(string file, ParsingOptions options)
    {
        _ = new ParsingContext(options);
        var doc = Open(file, (DocumentOptions?)null);
        return doc;
    }

    /// <summary>
    /// Opens a PDF document from the provided file path.
    /// </summary>
    /// <param name="data">PDF data</param>
    /// <param name="options">Optional parsing options</param>
    /// <returns>PdfDocument</returns>
    public static PdfDocument Open(string file, DocumentOptions? options = null)
    {
        IPdfDataSource source;
        var ctx = ParsingContext.Current;
        var doc = new PdfDocument();
        doc.UserPass = options?.UserPass;
        doc.OwnerPass = options?.OwnerPass;

        try
        {
            source = new MemoryMappedDataSource(doc, file);
        }
        catch (NotSupportedException)
        {
            var fs = File.OpenRead(file);
            source = new StreamDataSource(doc, fs, false);
        }

        doc.Initialize(ctx, source);
        return doc;
    }


    [Obsolete()]
    public static PdfDocument OpenMapped(string file, DocumentOptions? options = null) => Open(file, options);



    private static int docId = 0;
    internal static int GetNextId() => Interlocked.Increment(ref docId);
}
