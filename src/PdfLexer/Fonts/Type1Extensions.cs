using PdfLexer.DOM;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Fonts
{
    internal static class Type1Extensions
    {
        public static void AddWidthInfo(this ISimpleUnicode dict, Glyph[] glyphs)
        {
            if (dict.FirstChar == null) { return; }
            int fc = dict.FirstChar;
            int lc = (dict.LastChar ?? dict.Widths?.Count + fc);
            float mw = dict.MissingWidth ?? 0;
            mw = (float)(mw / 1000.0);
            var ws = new float[lc - fc + 1];
            int pos = 0;

            for (var i = pos; i < ws.Length; i++)
            {
                ws[pos] = mw;
            }
            foreach (var val in dict.Widths)
            {
                if (pos < ws.Length)
                {
                    var t = (double)(val.GetAs<PdfNumber>() ?? 0m);
                    ws[pos] = (float)(t / 1000.0);
                }
                pos++;
            }

            var bbox = dict.FontBBox;

            for (var i = 0; i < 256; i++)
            {
                var glyph = glyphs[i];
                if (glyph == null) {
                    glyph = new Glyph
                    {
                        GuessedUnicode = true,
                        Char = (char)i,
                        CodePoint = (uint)i,
                    };
                    glyphs[i] = glyph;
                }
                var lup = i - fc;
                if (lup < ws.Length && lup > -1)
                {
                    glyph.w0 = ws[lup];
                    if (bbox != null)
                    {
                        glyph.BBox = new decimal[] { 0, 0, (decimal)glyph.w0, bbox.URy / 1000.0m }; // todo font matrix vs 1000?
                    }
                }
                else
                {
                    glyph.w0 = mw;
                }

                if (glyph.CodePoint.Value == 32)
                {
                    glyph.IsWordSpace = true;
                }
            }
        }

        public static (bool HadBase, string[] Encoding, Dictionary<int, string> Diffs) GetDiffedEncoding(this FontType1 t1)
        {
            // grab encoding / diffs if exist
            PdfName be = null;
            string[] baseEncoding = null;
            if (t1.Encoding != null && t1.Encoding.GetPdfObjType() == PdfObjectType.NameObj)
            {
                be = t1.Encoding.GetAs<PdfName>();
                if (be == PdfName.MacRomanEncoding || be == PdfName.WinAnsiEncoding)
                {
                    baseEncoding = Encodings.GetEncoding(be);
                }
            }

            Dictionary<int, string> diffs = null;
            if (t1.Encoding != null && t1.Encoding.GetPdfObjType() == PdfObjectType.DictionaryObj)
            {
                var diff = (FontEncoding)(PdfDictionary)t1.Encoding;
                if (diff.BaseEncoding != null)
                {
                    baseEncoding = Encodings.GetEncoding(be);
                }
                diffs = new Dictionary<int, string>();
                foreach (var (code, name) in diff.ReadDifferences())
                {
                    diffs[code] = name.Value.Substring(1);
                }
            }

            return (baseEncoding != null, GetDiffedEncoding(baseEncoding, diffs), diffs);
        }


        private static string[] GetDiffedEncoding(string[] baseEncoding, Dictionary<int, string> diffs)
        {
            var be = new string[256];
            for (var charCode = 0; charCode < 256; charCode++)
            {
                string glyphName = null;
                if (diffs != null && diffs.TryGetValue(charCode, out glyphName))
                {
                    // value set;
                }
                else if (baseEncoding != null && (glyphName = baseEncoding[charCode]) != null)
                {
                    // value set;
                }
                else
                {
                    glyphName = Encodings.StandardEncoding[charCode];
                }
                if (glyphName == null)
                {
                    continue;
                }
                be[charCode] = glyphName;
            }
            return be;
        }
    }
}
