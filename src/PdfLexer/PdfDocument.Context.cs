using PdfLexer.DOM;
using PdfLexer.Encryption;
using PdfLexer.Filters;
using PdfLexer.IO;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using System.Runtime.CompilerServices;

namespace PdfLexer;

internal class ObjLocation
{
    public XRef Reference { get; set; }
    public long Location { get; set; }
    public PdfObjectType ObjType { get; set; }
}

public sealed partial class PdfDocument
{
    internal string? OwnerPass { get; set; }
    internal string? UserPass { get; set; }
    internal Dictionary<ulong, WeakReference<IPdfObject>> IndirectCache = new Dictionary<ulong, WeakReference<IPdfObject>>();
    internal ConditionalWeakTable<IPdfObject, XRefEntry> IndirectLookup = new ConditionalWeakTable<IPdfObject, XRefEntry>();
    internal Dictionary<ulong, XRefEntry> XRefs = null!;
    internal bool IsEncrypted { get; set; } = false;
    internal IDecryptionHandler Decryption { get; private set; }
    internal List<IDisposable> disposables = new List<IDisposable>();
    internal List<ObjLocation>? searchedXRefs;

    public IPdfDataSource MainDocSource { get; private set; }

    internal CryptFilter CryptFilter { get; }


    internal void Initialize(ParsingContext ctx, IPdfDataSource pdf)
    {
        MainDocSource = pdf;
        disposables.Add(MainDocSource);
        var (xr, tr) = ctx.XRefParser.LoadCrossReferences(pdf);
        XRefs = xr;
        Trailer = tr ?? new PdfDictionary();
        if (IsEncrypted)
        {
            IsEncrypted = false; // encryption handlers set encrypted true again when setup completed
            ctx.Options.Eagerness = Eagerness.FullEager;
            var enc = tr!.Get<PdfDictionary>(PdfName.Encrypt); // not null if set encrypt true;
            if (enc != null)
            {
                var filter = enc.Get<PdfName>(PdfName.Filter);
                switch (filter?.Value)
                {
                    case "Standard":
                    case null:
                        Decryption = new StandardEncryption(this, tr ?? new PdfDictionary());
                        break;
                    default:
                        throw new PdfLexerException($"Encryption of type {filter.Value} is not supported.");
                }
            }
        }

        // TODO clean doc id during parsing up
        // foreach (var item in Trailer.Values)
        // {
        //     if (item.Type == PdfObjectType.IndirectRefObj)
        //     {
        //         var eir = (ExistingIndirectRef)item;
        //         eir.SourceId = DocumentId;
        //     }
        // }

        var cat = Trailer.GetOptionalValue<PdfDictionary>(PdfName.Root);
        if (cat == null ||
            (cat.GetOptionalValue<PdfName>(PdfName.TypeName) != PdfName.Catalog && !cat.ContainsKey(PdfName.Pages)))
        {

            var matched = StructuralRepairs.RepairFindLastMatching(ctx, MainDocSource.GetStream(ctx, 0), PdfTokenType.DictionaryStart, x =>
            {
                if (x.Type != PdfObjectType.DictionaryObj)
                {
                    return false;
                }
                var dict = x.GetValue<PdfDictionary>();
                if (dict.GetOptionalValue<PdfName>(PdfName.TypeName)?.Value == PdfName.Catalog.Value)
                {
                    return true;
                }
                return false;
            })?.GetValue<PdfDictionary>();
            if (matched != null && cat == null)
            {
                cat = matched;
            }
            else if (matched != null && matched.ContainsKey(PdfName.Pages))
            {
                cat = matched;
            }
        }
        Catalog = cat ?? new PdfDictionary();

        var v = ctx.GetHeaderVersion(MainDocSource);
        if (v >= 1.4m && cat != null)
        {
            var ver = cat.Get<PdfName>(PdfName.Version);
            if (ver != null)
            {
                if (decimal.TryParse(ver.Value, out var cv))
                {
                    v = cv;
                }
            }
        }

        var pagesRef = cat?.GetOptionalValue<PdfDictionary>(PdfName.Pages);
        Pages = new();
        if (ctx.Options.LoadPageTree && pagesRef != null)
        {
            foreach (var pg in CommonUtil.EnumeratePageTree(pagesRef))
            {
                pg.SourceVersion = v;
                Pages.Add(pg);
            }
        }
        PdfVersion = v ?? 1.5m;
    }
}
