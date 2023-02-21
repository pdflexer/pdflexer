using PdfLexer.Content;
using PdfLexer.Writing;

namespace PdfLexer.DOM;

public sealed class PdfPage
{
    public PdfDictionary NativeObject { get; }
    internal ExistingIndirectRef? SourceRef { get; }
    internal decimal? SourceVersion { get; set; }

    internal PdfPage(PdfDictionary page, ExistingIndirectRef ir)
    {
        NativeObject = page;
        SourceRef = ir;
    }

    public PdfPage(PdfDictionary page)
    {
        NativeObject = page;
        page[PdfName.TypeName] = PdfName.Page;
    }

    public PdfPage(PageSize size)
    {
        NativeObject = new PdfDictionary();
        NativeObject[PdfName.TypeName] = PdfName.Page;
        MediaBox = PageSizeHelpers.GetMediaBox(size);
    }

    public PdfPage() : this(new PdfDictionary())
    {
    }

    [return: NotNullIfNotNull("dict")]
    public static implicit operator PdfPage?(PdfDictionary? dict) => dict == null ? null :new PdfPage(dict);
    [return: NotNullIfNotNull("page")]
    public static implicit operator PdfDictionary?(PdfPage? page) => page?.NativeObject;

    public PdfDictionary Resources { 
        get => NativeObject.GetOrCreateValue<PdfDictionary>(PdfName.Resources);
        set => NativeObject[PdfName.Resources] = value; }
    public PdfRectangle MediaBox { get => NativeObject.GetOrCreateValue<PdfArray>(PdfName.MediaBox); set => NativeObject[PdfName.MediaBox] = value.NativeObject; }
    public PdfRectangle CropBox { get => GetWithDefault(PdfName.CropBox, PdfName.MediaBox); set => NativeObject[PdfName.CropBox] = value.NativeObject; }
    public PdfRectangle BleedBox { get => GetWithDefault(PdfName.BleedBox, PdfName.CropBox); set => NativeObject[PdfName.BleedBox] = value.NativeObject; }
    public PdfRectangle TrimBox { get => GetWithDefault(PdfName.TrimBox, PdfName.CropBox); set => NativeObject[PdfName.TrimBox] = value.NativeObject; }
    public PdfRectangle ArtBox { get => GetWithDefault(PdfName.ArtBox, PdfName.CropBox); set => NativeObject[PdfName.ArtBox] = value.NativeObject; }
    public PdfNumber? Rotate
    {
        get
        {
            var r = NativeObject.Get<PdfNumber>(PdfName.Rotate);
            if (r != null)
            {
                return r;
            }
            NativeObject[PdfName.Rotate] = PdfCommonNumbers.Zero;
            return PdfCommonNumbers.Zero;
        }
        set => NativeObject.Set(PdfName.Rotate, value);
    }


    public PageTreeNode? Parent
    {
        get => NativeObject.Get<PdfDictionary>(PdfName.Parent);
        set => NativeObject.Set(PdfName.Parent, value?.Dictionary.Indirect());
    }


    public IEnumerable<PdfStream> Contents { get
        {
            
            var cnt = NativeObject?.Get(PdfName.Contents)?.Resolve();
            if (cnt == null) { yield break; }
            if (cnt.Type == PdfObjectType.ArrayObj)
            {
                var arr = (PdfArray)cnt;
                foreach (var item in arr)
                {
                    yield return item.GetAs<PdfStream>();
                }
            } else
            {
                yield return cnt.GetAs<PdfStream>();
            }
                
        }
    }

    public void AddXObj(PdfName nm, IPdfObject xobj)
    {
        NativeObject.GetOrCreateValue<PdfDictionary>(PdfName.Resources).GetOrCreateValue<PdfDictionary>(PdfName.XObject)[nm] = xobj.Indirect();
    }


    private PdfArray GetWithDefault(PdfName primary, PdfName secondary)
    {
        var p = NativeObject.Get<PdfArray>(primary);
        if (p != null) { return p; }
        var s = NativeObject.GetOrCreateValue<PdfArray>(secondary).CloneShallow();
        NativeObject[primary] = s;
        return s;
    }

    public PageWriter GetWriter(PageWriteMode mode=PageWriteMode.Append)
    {
        return new PageWriter(this, mode);
    }

    public TextScanner GetTextScanner(ParsingContext ctx)
    {
        return new TextScanner(ctx, this);
    }

    public TextScanner GetTextScanner()
    {
        return new TextScanner(ParsingContext.Current, this);
    }
}


// inheritable
// Resources required (dictionary)
// MediaBox required (rectangle)
// CropBox => default to MediaBox (rectangle)
// Rotate (integer)

// LastModified date required if PieceInfo present
// Group dictionary
// Thumb stream
// B array
// Dur number
// Trans dictionary
// Annots array
// AA dictionary
// Metadata stream
// PieceInfo dictionary
// StructParents integer
// ID byte string
// PZ number
// SeparationInfo dictionary
// Tabs name
// TemplateInstantieted name
// PressSteps dictionary
// UserUnit number
// VP dictionary

