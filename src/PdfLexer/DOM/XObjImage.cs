using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM
{
    public class XObjImage
    {
        public PdfDictionary NativeObject { get => Stream.Dictionary; }
        public PdfStreamContents Contents { get => Stream.Contents; set => Stream.Contents = value; }
        public PdfStream Stream { get; }


        public XObjImage()
        {
            Stream = new PdfStream();
            NativeObject[PdfName.TypeName] = PdfName.XObject;
            NativeObject[PdfName.Subtype] = PdfName.Image;
        }

        public XObjImage(PdfStream str)
        {
            Stream = str;
        }

        public static implicit operator XObjImage(PdfStream str) => new XObjImage(str);
        public static implicit operator PdfStream(XObjImage img) => img.Stream;

        public PdfNumber? Width { 
            get => NativeObject.GetOrSetValue<PdfNumber>(PdfName.Width, PdfCommonNumbers.Zero); 
            set => NativeObject.Set(PdfName.Width, value);
        }

        public PdfNumber? Height
        {
            get => NativeObject.GetOrSetValue<PdfNumber>(PdfName.Height, PdfCommonNumbers.Zero);
            set => NativeObject.Set(PdfName.Height, value);
        }

        public PdfNumber? BitsPerComponent
        {
            get => NativeObject.Get<PdfNumber>(PdfName.BitsPerComponent);
            set => NativeObject.Set(PdfName.BitsPerComponent, value);
        }

        public PdfBoolean? ImageMask
        {
            get => NativeObject.GetOrSetValue<PdfBoolean>(PdfName.ImageMask, PdfBoolean.False);
            set => NativeObject.Set(PdfName.ImageMask, value);
        }

        public PdfArray? Decode
        {
            get => NativeObject.Get<PdfArray>(PdfName.Decode);
            set => NativeObject.Set(PdfName.Decode, value);
        }

        public IPdfObject? ColorSpace
        {
            get => NativeObject.Get(PdfName.ColorSpace);
            set => NativeObject.Set(PdfName.ColorSpace, value);
        }

        public IPdfObject? Mask
        {
            get => NativeObject.Get(PdfName.Mask);
            set {
                if (value == null) { return; }
                if (value.Type == PdfObjectType.StreamObj)
                {
                    NativeObject[PdfName.Mask] = value.Indirect();
                } else
                {
                    NativeObject[PdfName.Mask] = value;
                }
            }
        }

        public IPdfObject? SMask
        {
            get => NativeObject.Get(PdfName.SMask);
            set
            {
                if (value == null) { return; }
                if (value.Type == PdfObjectType.StreamObj)
                {
                    NativeObject[PdfName.SMask] = value.Indirect();
                }
                else
                {
                    NativeObject[PdfName.SMask] = value;
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
