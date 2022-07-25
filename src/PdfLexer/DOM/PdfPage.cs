using PdfLexer.Content;
using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM
{
    public class PdfPage
    {
        public PdfDictionary Dictionary { get; }
        internal ExistingIndirectRef SourceRef { get; }

        internal PdfPage(PdfDictionary page, ExistingIndirectRef ir)
        {
            Dictionary = page;
            SourceRef = ir;
        }

        public PdfPage(PdfDictionary page)
        {
            Dictionary = page;
            page[PdfName.TypeName] = PdfName.Page;
        }

        public PdfPage() : this(new PdfDictionary())
        {
        }

        public static implicit operator PdfPage(PdfDictionary dict) => new PdfPage(dict);
        public static implicit operator PdfDictionary(PdfPage page) => page.Dictionary;


        public PdfDictionary Resources { 
            get => Dictionary.GetOrCreateValue<PdfDictionary>(PdfName.Resources);
            set => Dictionary[PdfName.Resources] = value; }
        public PdfRectangle MediaBox { get => Dictionary.GetOrCreateValue<PdfArray>(PdfName.MediaBox); set => Dictionary[PdfName.MediaBox] = value.Array; }
        public PdfRectangle CropBox { get => GetWithDefault(PdfName.CropBox, PdfName.MediaBox); set => Dictionary[PdfName.CropBox] = value.Array; }
        public PdfRectangle BleedBox { get => GetWithDefault(PdfName.BleedBox, PdfName.CropBox); set => Dictionary[PdfName.BleedBox] = value.Array; }
        public PdfRectangle TrimBox { get => GetWithDefault(PdfName.TrimBox, PdfName.CropBox); set => Dictionary[PdfName.TrimBox] = value.Array; }
        public PdfRectangle ArtBox { get => GetWithDefault(PdfName.ArtBox, PdfName.CropBox); set => Dictionary[PdfName.ArtBox] = value.Array; }
        public PdfNumber Rotate { get 
            {
                var r = Dictionary.Get<PdfNumber>(PdfName.Rotate);
                if (r != null) {
                    return r;
                }
                Dictionary[PdfName.Rotate] = PdfCommonNumbers.Zero;
                return PdfCommonNumbers.Zero; 
            }
            set => Dictionary[PdfName.Rotate] = value; }


        public PageTreeNode Parent { get => Dictionary.Get<PdfDictionary>(PdfName.Parent); set => Dictionary[PdfName.Parent] = value.Dictionary.Indirect(); }


        public IEnumerable<PdfStream> Contents { get
            {
                
                var cnt = Dictionary?.Get(PdfName.Contents)?.Resolve();
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
            Dictionary.GetOrCreateValue<PdfDictionary>(PdfName.Resources).GetOrCreateValue<PdfDictionary>(PdfName.XObject)[nm] = xobj.Indirect();
        }


        private PdfArray GetWithDefault(PdfName primary, PdfName secondary)
        {
            var p = Dictionary.Get<PdfArray>(primary);
            if (p != null) { return p; }
            var s = Dictionary.GetOrCreateValue<PdfArray>(secondary).CloneShallow();
            Dictionary[primary] = s;
            return s;
        }

        public PageWriter GetWriter(PageWriteMode mode=PageWriteMode.Append)
        {
            return new PageWriter(this, mode);
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

}
