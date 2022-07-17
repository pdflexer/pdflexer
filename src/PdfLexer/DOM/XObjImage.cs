using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM
{
    public class XObjImage
    {
        public PdfDictionary Dictionary { get => Stream.Dictionary; }
        public PdfStreamContents Contents { get => Stream.Contents; }
        public PdfStream Stream { get; }


        public XObjImage()
        {
            Stream = new PdfStream();
            Dictionary[PdfName.TypeName] = PdfName.XObject;
            Dictionary[PdfName.Subtype] = PdfName.Image;
        }

        public XObjImage(PdfStream str)
        {
            Stream = str;
        }

        public static implicit operator XObjImage(PdfStream str) => new XObjImage(str);
        public static implicit operator PdfStream(XObjImage img) => img.Stream;

        public PdfNumber Width { 
            get => Dictionary.GetOrSetValue<PdfNumber>(PdfName.Width, PdfCommonNumbers.Zero); 
            set => Dictionary[PdfName.Width] = value; 
        }

        public PdfNumber Height
        {
            get => Dictionary.GetOrSetValue<PdfNumber>(PdfName.Height, PdfCommonNumbers.Zero);
            set => Dictionary[PdfName.Height] = value;
        }

        public PdfNumber BitsPerComponent
        {
            get => Dictionary.Get<PdfNumber>(PdfName.BitsPerComponent);
            set => Dictionary[PdfName.BitsPerComponent] = value;
        }

        public PdfBoolean ImageMask
        {
            get => Dictionary.GetOrSetValue<PdfBoolean>(PdfName.ImageMask, PdfBoolean.False);
            set => Dictionary[PdfName.ImageMask] = value;
        }

        public PdfArray Decode
        {
            get => Dictionary.Get<PdfArray>(PdfName.Decode);
            set => Dictionary[PdfName.Decode] = value;
        }

        public IPdfObject ColorSpace
        {
            get => Dictionary.Get(PdfName.ColorSpace);
            set => Dictionary[PdfName.ColorSpace] = value;
        }

        public IPdfObject Mask
        {
            get => Dictionary.Get(PdfName.Mask);
            set {
                if (value.Type == PdfObjectType.StreamObj)
                {
                    Dictionary[PdfName.Mask] = value.Indirect();
                } else
                {
                    Dictionary[PdfName.Mask] = value;
                }
            }
        }



        // Width int
        // Height int
        // ColorSpace name / array
        // BitsPerComponent int
        // Intent name
        // ImageMask bool
        // Mask image or array
        // Decode array
        // Interpolate
        // Alternates array (of alt image dicts)
        // SMask image
        // SMaskInData int (for JPXDecode)
        // Name name (obsolete)
        // StructParent int
        // ID byte string
        // OPI dict, ignored if ImageMask true
        // Metadata stream
        // OC dict optional content
        //
    }
}
