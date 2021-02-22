using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Parsers.Nested
{
    internal enum ParseState
    {
        None,
        ReadDictKey,
        ReadDictValue,
        ReadArray,
        SkipDict,
        SkipArray
    }

    internal struct ObjParseState
    {
        public ParseState State { get; set; }
        public PdfName CurrentKey { get; set; }
        public PdfDictionary Dict { get; set; }
        public PdfArray Array { get; set; }
        public List<IPdfObject> Bag { get; set; }

        public bool IsParsing()
        {
            return Dict != null || Array != null;
        }

        public PdfArray GetArrayFromBag()
        {
            for (var i=0;i<Bag.Count;i++)
            {
                var item = Bag[i];
                if (item is PdfNumber num 
                    && i + 2 < Bag.Count
                    && Bag[i+1] is PdfIntNumber num2
                    && Bag[i+2] is IndirectRefToken)
                {
                    Array.Add(new PdfIndirectRef((long)num, num2.Value));
                    i+=2;
                } else
                {
                    Array.Add(item);
                }
            }
            return Array;
        }

        public PdfDictionary GetDictionaryFromBag()
        {
            bool key = true;
            PdfName name = null;
            for (var i=0;i<Bag.Count;i++)
            {
                var item = Bag[i];
                if (key)
                {
                    if (item is PdfName nm)
                    {
                        name = nm;
                    } else
                    {
                        throw new ApplicationException("");
                    }
                } else
                {
                    if (item is PdfNumber num 
                        && i + 2 < Bag.Count
                        && Bag[i+1] is PdfIntNumber num2
                        && Bag[i+2] is IndirectRefToken)
                    {
                        Dict[name] = new PdfIndirectRef((long)num, num2.Value);
                        i+=2;
                    } else
                    {
                        Dict[name] = item;
                    }
                    
                }
                key = !key;
            }
            Dict.IsModified = false;
            return Dict;
        }
    }
}
