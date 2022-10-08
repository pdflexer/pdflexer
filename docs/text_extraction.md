# Text Extraction

Note: Beta quality

```csharp
// get all chars
using var doc = PdfDocument.Open(data);
var page = doc.Pages.First();
var reader = new TextScanner(doc.Context, page);
while (reader.Advance())
{
    sb.Append(reader.Glyph.Char);
}
var str = sb.ToString();
```

```csharp
// get filtered chars
var doc = PdfDocument.Open(data);
using var page = doc.Pages.First();
var reader = new TextScanner(doc.Context, page);
while (reader.Advance())
{
    if (reader.Position.LLy > miny && reader.Position.LLx > minx) {
        sb.Append(reader.Glyph.Char);
    }
}
var str = sb.ToString();
```

```csharp
// get words
var doc = PdfDocument.Open(data);
using var page = doc.Pages.First();
var reader = new SimpleWordReader(doc.Context, page);
while (reader.Advance())
{
    sb.AppendLine(reader.CurrentWord);
    var (llx ,lly, urx, ury) = reader.GetWordBoundingBox();
}
var str2 = sb.ToString();
```

Initial benchmarking results with some random PDFs from tests set iterating over all characters in PDF. ~4x faster and ~10x less allocations with the exception of issue2128r.pdf (libraries getting different characters, need to determine which is correct and probably remove from set if PdfPig is wrong). Not exactly a 1-to-1 comparison as PdfPig is doing some extra work and tracking color etc.

```
|          Method |         testPdf |         Mean |        Error |       StdDev | Ratio | RatioSD |        Gen0 |       Gen1 |      Gen2 |    Allocated | Alloc Ratio |
|---------------- |---------------- |-------------:|-------------:|-------------:|------:|--------:|------------:|-----------:|----------:|-------------:|------------:|
|   ReadTxtPdfPig |  __bpl13210.pdf | 487,008.8 us |  7,958.25 us |  9,773.45 us |  1.00 |    0.00 |  63000.0000 | 19000.0000 |         - | 302536.42 KB |        1.00 |
| ReadTxtPdfLexer |  __bpl13210.pdf | 137,776.8 us |  5,171.67 us |  6,156.51 us |  0.28 |    0.02 |   1750.0000 |  1000.0000 |         - |  13015.35 KB |        0.04 |
|                 |                 |              |              |              |       |         |             |            |           |              |             |
|   ReadTxtPdfPig |   __ecma262.pdf | 973,119.6 us | 18,345.77 us | 22,530.25 us |  1.00 |    0.00 | 100000.0000 | 40000.0000 | 1000.0000 | 558951.75 KB |        1.00 |
| ReadTxtPdfLexer |   __ecma262.pdf | 300,097.0 us |  3,841.41 us |  5,128.18 us |  0.31 |    0.01 |   4000.0000 |  1500.0000 |         - |  27518.74 KB |        0.05 |
|                 |                 |              |              |              |       |         |             |            |           |              |             |
|   ReadTxtPdfPig |    __gesamt.pdf | 849,221.3 us | 26,122.46 us | 33,036.55 us |  1.00 |    0.00 | 111000.0000 | 34000.0000 |         - | 568520.13 KB |        1.00 |
| ReadTxtPdfLexer |    __gesamt.pdf | 234,854.4 us |  4,125.68 us |  5,066.71 us |  0.28 |    0.01 |   1500.0000 |   500.0000 |         - |   9302.48 KB |        0.02 |
|                 |                 |              |              |              |       |         |             |            |           |              |             |
|   ReadTxtPdfPig | __issue1133.pdf | 248,139.3 us | 52,741.98 us | 66,701.72 us |  1.00 |    0.00 |  18500.0000 |  4000.0000 |  500.0000 | 110848.86 KB |        1.00 |
| ReadTxtPdfLexer | __issue1133.pdf |  61,400.4 us |  6,567.24 us |  8,305.46 us |  0.26 |    0.07 |    800.0000 |   200.0000 |         - |   5393.27 KB |        0.05 |
|                 |                 |              |              |              |       |         |             |            |           |              |             |
|   ReadTxtPdfPig |      bug1669099 |  18,103.9 us |    323.04 us |    420.04 us |  1.00 |    0.00 |   1687.5000 |  1312.5000 |  687.5000 |   8528.11 KB |        1.00 |
| ReadTxtPdfLexer |      bug1669099 |   5,002.0 us |    129.97 us |    164.37 us |  0.28 |    0.01 |    382.8125 |   250.0000 |  117.1875 |   1878.07 KB |        0.22 |
|                 |                 |              |              |              |       |         |             |            |           |              |             |
|   ReadTxtPdfPig |       issue1905 |  15,190.6 us |  1,691.24 us |  2,257.76 us |  1.00 |    0.00 |   1156.2500 |   562.5000 |         - |    6787.2 KB |        1.00 |
| ReadTxtPdfLexer |       issue1905 |   3,153.6 us |     44.66 us |     59.62 us |  0.21 |    0.03 |    144.5313 |    46.8750 |         - |    671.11 KB |        0.10 |
|                 |                 |              |              |              |       |         |             |            |           |              |             |
|   ReadTxtPdfPig |      issue2128r |     888.6 us |     13.59 us |     16.68 us |  1.00 |    0.00 |     25.3906 |          - |         - |    110.91 KB |        1.00 |
| ReadTxtPdfLexer |      issue2128r |   1,708.6 us |     84.45 us |    112.74 us |  1.91 |    0.13 |    210.9375 |   146.4844 |  136.7188 |    1425.3 KB |       12.85 |
```
