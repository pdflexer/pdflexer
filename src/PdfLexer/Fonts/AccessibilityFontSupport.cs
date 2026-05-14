using System.Text;

namespace PdfLexer.Fonts;

internal interface IAccessibilityAwareWritableFont
{
    void EnableAccessibilityMapping();
    bool CanEncode(char c);
    IWritableFont? GetUnicodeSafeAlternative();
}

internal static class AccessibilityFontSupport
{
    public static PdfStream CreateSingleByteToUnicodeCMap(IEnumerable<(byte Code, string Unicode)> mappings, string name)
    {
        using var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, Encoding.ASCII, 1024, leaveOpen: true))
        {
            writer.WriteLine("/CIDInit /ProcSet findresource begin");
            writer.WriteLine("12 dict begin");
            writer.WriteLine("begincmap");
            writer.WriteLine("/CIDSystemInfo <<");
            writer.WriteLine("/Registry (Adobe)");
            writer.WriteLine("/Ordering (UCS)");
            writer.WriteLine("/Supplement 0");
            writer.WriteLine(">> def");
            writer.WriteLine($"/CMapName /{name} def");
            writer.WriteLine("/CMapType 2 def");
            writer.WriteLine("1 begincodespacerange");
            writer.WriteLine("<00> <FF>");
            writer.WriteLine("endcodespacerange");

            var ordered = mappings
                .GroupBy(x => x.Code)
                .Select(x => x.First())
                .OrderBy(x => x.Code)
                .ToList();

            for (var offset = 0; offset < ordered.Count; offset += 100)
            {
                var batch = ordered.Skip(offset).Take(100).ToList();
                writer.WriteLine($"{batch.Count} beginbfchar");
                foreach (var item in batch)
                {
                    writer.Write($"<{item.Code:X2}> <");
                    foreach (var ch in item.Unicode)
                    {
                        writer.Write($"{(int)ch:X4}");
                    }
                    writer.WriteLine(">");
                }
                writer.WriteLine("endbfchar");
            }

            writer.WriteLine("endcmap");
            writer.WriteLine("CMapName currentdict /CMap defineresource pop");
            writer.WriteLine("end");
            writer.WriteLine("end");
        }

        var pdfStream = new PdfStream(new PdfByteArrayStreamContents(stream.ToArray()));
        pdfStream.Dictionary[PdfName.TYPE] = PdfName.CMap;
        return pdfStream;
    }
}
