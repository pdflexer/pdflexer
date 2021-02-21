using System;
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

        public static PdfName Prev => new PdfName("/Prev");
    }
}
