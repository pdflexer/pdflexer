using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLexer.Tests
{
    public class Util
    {
        public static decimal GetDocumentHashCode(string path)
        {
            return GetDocumentHashCode(File.ReadAllBytes(path));
        }
        public static decimal GetDocumentHashCode(byte[] data)
        {
            decimal hc = 0;
            using var doc = UglyToad.PdfPig.PdfDocument.Open(data);
            foreach (var page in doc.GetPages())
            {
                hc = unchecked(hc + GetPageHashCode(page));
            }
            return hc;
        }

        public static int CountResources(byte[] pdfDoc)
        {
            var found = new HashSet<ulong>();
            using var doc = PdfDocument.Open(pdfDoc);
            foreach (var page in doc.Pages)
            {
                if (page.NativeObject.TryGetValue<PdfDictionary>(PdfName.Resources, out var res))
                {
                    foreach (var kvp in res.Where(x=>x.Key == PdfName.Font || x.Key == PdfName.XObject))
                    {
                        var inner = kvp.Value.GetValue<PdfDictionary>();
                        foreach (var kvpRes in inner)
                        {
                            var ir = kvpRes.Value as PdfIndirectRef;
                            found.Add(ir.Reference.GetId());
                        }
                    }
                }
            }
            return found.Count;
        }

            public static decimal GetPageHashCode(UglyToad.PdfPig.Content.Page page)
        {
            decimal hc = 0;
            foreach (var letter in page.Letters)
            {
                hc = unchecked(hc + (decimal)letter.Location.X);
                hc = unchecked(hc + (decimal)letter.Location.Y);
                var (r, b, g) = letter.Color.ToRGBValues();
                hc = unchecked(hc + r);
                hc = unchecked(hc + b);
                hc = unchecked(hc + g);
                hc = unchecked(hc + (decimal)letter.Width);
                hc = unchecked(hc + (decimal)letter.PointSize);
                hc = unchecked(hc + (decimal)letter.Font.Name.GetHashCode());
            }
            foreach (var img in page.GetImages())
            {
                hc = unchecked(hc + (decimal)img.Bounds.BottomLeft.X);
                hc = unchecked(hc + (decimal)img.Bounds.BottomLeft.Y);
                hc = unchecked(hc + (decimal)img.Bounds.TopRight.X);
                hc = unchecked(hc + (decimal)img.Bounds.TopRight.Y);
            }

            hc = unchecked(hc + (decimal)(int)page.MediaBox.Bounds.BottomLeft.X);
            hc = unchecked(hc + (decimal)(int)page.MediaBox.Bounds.BottomLeft.Y);
            hc = unchecked(hc + (decimal)(int)page.MediaBox.Bounds.TopRight.X);
            hc = unchecked(hc + (decimal)(int)page.MediaBox.Bounds.TopRight.Y);
            // pdf pig has weird handling for media box, will convert to int if on page but not if inherited
            // hc = unchecked(hc + (decimal)page.MediaBox.Bounds.BottomLeft.X);
            // hc = unchecked(hc + (decimal)page.MediaBox.Bounds.BottomLeft.Y);
            // hc = unchecked(hc + (decimal)page.MediaBox.Bounds.TopRight.X);
            // hc = unchecked(hc + (decimal)page.MediaBox.Bounds.TopRight.Y);
            return hc;
        }
    }
}
