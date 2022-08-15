using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PdfLexer.DOM
{

    public class FontCID : IPdfFont
    {
        public PdfDictionary NativeObject { get; }

        public FontCID(bool trueType)
        {
            NativeObject = new PdfDictionary();
            NativeObject[PdfName.TypeName] = PdfName.Font;
            NativeObject[PdfName.Subtype] = trueType ? PdfName.CIDFontType2 : PdfName.CIDFontType0;
        }

        public FontCID(PdfDictionary dict)
        {
            NativeObject = dict;
        }

        public static implicit operator FontCID?(PdfDictionary? dict) => dict == null ? null : new (dict);
        public static implicit operator PdfDictionary(FontCID page) => page.NativeObject;

        /// <summary>
        /// required
        /// </summary>
        public PdfName? Subtype
        {
            get => NativeObject.Get<PdfName>(PdfName.Subtype);
            set => NativeObject.Set(PdfName.Subtype, value);
        }

        /// <summary>
        /// required
        /// If type 2 cidfont name should be CIDFonts BaseFont name
        /// If type 0 concat CIDFont BaseFont name "-" and the CMap name in encoding entry
        /// </summary>
        public PdfName? BaseFont
        {
            get => NativeObject.Get<PdfName>(PdfName.BaseFont);
            set => NativeObject.Set(PdfName.BaseFont, value);
        }

        /// <summary>
        /// required
        /// </summary>
        public CIDSystemInfo? CIDSystemInfo
        {
            get => NativeObject.Get<PdfDictionary>(PdfName.CIDSystemInfo);
            set => NativeObject.Set(PdfName.CIDSystemInfo, value?.NativeObject);
        }

        /// <summary>
        /// required
        /// </summary>
        public FontDescriptor? FontDescriptor
        {
            get => NativeObject.Get<PdfDictionary>(PdfName.FontDescriptor);
            set => NativeObject.Set(PdfName.FontDescriptor, value?.NativeObject.Indirect());
        }


        /// <summary>
        /// optional
        /// default width for glyphs
        /// default 1000
        /// </summary>
        public PdfNumber? DW
        {
            get => NativeObject.Get<PdfNumber>(PdfName.DW) ?? (PdfNumber)1000;
            set => NativeObject.Set(PdfName.DW, value);
        }

        /// <summary>
        /// optional
        /// use DW if not present
        /// </summary>
        public PdfArray? W
        {
            get => NativeObject.Get<PdfArray>(PdfName.W);
            set => NativeObject.Set(PdfName.W, value);
        }

        /// <summary>
        /// optional
        /// default vertical size for glyphs
        /// default [880 -1000]
        /// </summary>
        public PdfArray? DW2
        {
            get => NativeObject.Get<PdfArray>(PdfName.DW2);
            set => NativeObject.Set(PdfName.DW2, value);
        }

        /// <summary>
        /// optional
        /// use DW2 if not present
        /// </summary>
        public PdfArray? W2
        {
            get => NativeObject.Get<PdfArray>(PdfName.W2);
            set => NativeObject.Set(PdfName.W2, value);
        }

        /// <summary>
        /// optional
        /// type 2 cidfonts only
        /// stream or name
        /// if name should be Identity
        /// may only appear on embedded fonts
        /// </summary>
        public IPdfObject? CIDToGIDMap
        {
            get => NativeObject.Get(PdfName.CIDToGIDMap);
            set => NativeObject.Set(PdfName.CIDToGIDMap, value);
        }

        public IEnumerable<(uint cid, float width)> ReadW() => ReadWidths(W);
        public IEnumerable<(uint cid, float width)> ReadW2() => ReadWidths(W2);

        public Dictionary<uint, uint>? ReadCIDToGid()
        {
            var obj = CIDToGIDMap;
            if (obj == null)
            {
                return null;
            }
            if (obj.Type == PdfObjectType.NameObj)
            {
                return null;
            } else if (obj.Type != PdfObjectType.StreamObj)
            {
                return null;
            }

            var str = obj.GetAs<PdfStream>();
            var lu = new Dictionary<uint, uint>();
            using var buffer = str.Contents.GetDecodedBuffer();
            var data = buffer.GetData();
            var cids = data.Length / 2;
            for (var i=0;i<cids;i++)
            {
                var b0 = data[2 * i];
                var b2 = data[2 * i + 1];
                var gid = b0 << 8 | b2;
                lu[(uint)i] = (uint)gid;
            }
            return lu;

        }
        private static IEnumerable<(uint cid, float width)> ReadWidths(PdfArray? array)
        {
            if (array == null) { yield break; }
            uint? firstCode = null;
            uint? lastCode = null;
            foreach (var rv in array)
            {
                var val = rv.Resolve();
                switch (val)
                {
                    case PdfNumber cnt:
                        if (firstCode == null)
                        {
                            firstCode = (uint)cnt;
                            continue;
                        }
                        if (lastCode == null)
                        {
                            lastCode = (uint)cnt;
                            continue;
                        }
                        for (var i = firstCode.Value; i <= lastCode.Value; i++)
                        {
                            yield return ((uint)(i), (float)cnt);
                        }
                        firstCode = null;
                        lastCode = null;
                        continue;
                    case PdfArray arr:
                        firstCode ??= 0;
                        foreach (var item in arr)
                        {
                            if (item is PdfNumber w)
                            {
                                yield return (firstCode.Value, (float)w);
                            }
                            firstCode += 1;
                        }
                        firstCode = null;
                        lastCode = null;
                        continue;
                }
            }
        }
    }


    public class CIDSystemInfo
    {
        public PdfDictionary NativeObject { get; }

        public CIDSystemInfo()
        { 
            NativeObject = new();
        }

        public CIDSystemInfo(PdfDictionary dict)
        {
            NativeObject = dict;
        }

        public static implicit operator CIDSystemInfo?(PdfDictionary? dict) => dict == null ? null :new CIDSystemInfo(dict);
        public static implicit operator PdfDictionary?(CIDSystemInfo? page) => page?.NativeObject;

     

        /// <summary>
        /// required
        /// A string identifying the issuer of the char collection
        /// </summary>
        public PdfString? Registry
        {
            get => NativeObject.Get<PdfString>(PdfName.Registry);
            set => NativeObject.Set(PdfName.Registry, value);
        }

        /// <summary>
        /// required
        /// A string identifying the char collection from the issuer
        /// </summary>
        public PdfString? Ordering
        {
            get => NativeObject.Get<PdfString>(PdfName.Ordering);
            set => NativeObject.Set(PdfName.Ordering, value);
        }

        /// <summary>
        /// required
        /// Supplement / version number of CID collection
        /// Starts at 0.
        /// </summary>
        public PdfIntNumber? Supplement
        {
            get => NativeObject.Get<PdfIntNumber>(PdfName.Supplement);
            set => NativeObject.Set(PdfName.Supplement, value);
        }
    }
}
