﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLexer
{
    public class PdfName : PdfObject
    {
        internal bool? NeedsEscaping { get;} 
        public PdfName(string value)
        {
            Value = value;
            NeedsEscaping = null;
        }

        internal PdfName(string value, bool needsEscaping)
        {
            Value = value;
            NeedsEscaping = needsEscaping;
        }

        public string Value { get; }
        public override PdfObjectType Type => PdfObjectType.NameObj;

        public override bool Equals(object obj)
        {
            if (!(obj is PdfName name))
            {
                return false;
            }

            return Value.Equals(name?.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static PdfName Prev => new PdfName("/Prev", false);
        public static PdfName Length => new PdfName("/Length", false);
        public static PdfName Root => new PdfName("/Root", false);
        public static PdfName Pages => new PdfName("/Pages", false);
        public static PdfName TypeName => new PdfName("/Type", false);
        public static PdfName Count => new PdfName("/Count", false);
        public static PdfName Kids => new PdfName("/Kids", false);
        public static PdfName Contents => new PdfName("/Contents", false);
        public static PdfName Resources => new PdfName("/Resources", false);
        public static PdfName MediaBox => new PdfName("/MediaBox", false);
        public static PdfName CropBox => new PdfName("/CropBox", false);
        public static PdfName Rotate => new PdfName("/Rotate", false);
        public static PdfName Catalog => new PdfName("/Catalog", false);

        public static implicit operator PdfName(string name)
        {
            // TODO common lookups for above?
            if (name[0] == '/')
            {
                return new PdfName(name);
            } else
            {
                return new PdfName("/"+name);
            }
            
        }
        
    }
}
