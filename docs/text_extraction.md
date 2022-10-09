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

Initial benchmarking results with some random PDFs from tests set iterating over all characters in PDF. ~4x faster and ~10x less allocations with the exception of issue2128r.pdf (libraries getting different characters, need to determine which is correct and probably remove from set if PdfPig is wrong). Not exactly a 1-to-1 comparison as PdfPig / Pdfium are doing extra work but if you really just want text extracted with positions the results are realistic.
Note: PdfiumCore is c++ wrapper so GC / memory stats not meaningful.

```
|            Method |         testPdf |           Mean |         Error |        StdDev | Ratio | RatioSD |        Gen0 |       Gen1 |      Gen2 |   Allocated | Alloc Ratio |
|------------------ |---------------- |---------------:|--------------:|--------------:|------:|--------:|------------:|-----------:|----------:|------------:|------------:|
|     ReadTxtPdfPig |  __bpl13210.pdf |   509,484.1 us |   8,884.91 us |  11,861.10 us |  1.00 |    0.00 |  64000.0000 |  9000.0000 |         - | 309797832 B |       1.000 |
|   ReadTxtPdfLexer |  __bpl13210.pdf |   147,434.6 us |   2,670.07 us |   3,564.47 us |  0.29 |    0.01 |   1666.6667 |   666.6667 |         - |  13197616 B |       0.043 |
| ReadTxtPdfiumCore |  __bpl13210.pdf | 1,060,009.4 us |  18,396.87 us |  23,266.15 us |  2.08 |    0.07 |           - |          - |         - |     24520 B |       0.000 |
|                   |                 |                |               |               |       |         |             |            |           |             |             |
|     ReadTxtPdfPig |   __ecma262.pdf | 1,015,502.2 us |  12,847.72 us |  16,248.25 us |  1.00 |    0.00 | 100000.0000 | 41000.0000 | 1000.0000 | 572231264 B |       1.000 |
|   ReadTxtPdfLexer |   __ecma262.pdf |   321,015.2 us |   9,527.21 us |  12,388.07 us |  0.31 |    0.01 |   4500.0000 |  1000.0000 |         - |  27132032 B |       0.047 |
| ReadTxtPdfiumCore |   __ecma262.pdf | 2,302,162.3 us | 177,576.93 us | 230,900.17 us |  2.26 |    0.22 |           - |          - |         - |     17352 B |       0.000 |
|                   |                 |                |               |               |       |         |             |            |           |             |             |
|     ReadTxtPdfPig |    __gesamt.pdf |   908,264.4 us |  56,108.65 us |  74,903.43 us |  1.00 |    0.00 | 111000.0000 | 37000.0000 |         - | 582032984 B |       1.000 |
|   ReadTxtPdfLexer |    __gesamt.pdf |   251,910.0 us |  12,458.91 us |  16,200.10 us |  0.28 |    0.03 |   1500.0000 |   500.0000 |         - |   9267216 B |       0.016 |
| ReadTxtPdfiumCore |    __gesamt.pdf |   919,024.3 us |  21,756.58 us |  26,719.03 us |  1.01 |    0.08 |           - |          - |         - |     26952 B |       0.000 |
|                   |                 |                |               |               |       |         |             |            |           |             |             |
|     ReadTxtPdfPig | __issue1133.pdf |   210,948.1 us |   7,061.49 us |   9,426.89 us |  1.00 |    0.00 |  19000.0000 |  4000.0000 |  500.0000 | 113377620 B |       1.000 |
|   ReadTxtPdfLexer | __issue1133.pdf |    52,222.7 us |   1,256.24 us |   1,633.46 us |  0.25 |    0.01 |    800.0000 |   200.0000 |         - |   5313042 B |       0.047 |
| ReadTxtPdfiumCore | __issue1133.pdf |   221,222.8 us |   4,062.85 us |   4,989.55 us |  1.05 |    0.06 |           - |          - |         - |      3432 B |       0.000 |
|                   |                 |                |               |               |       |         |             |            |           |             |             |
|     ReadTxtPdfPig |      bug1669099 |    18,518.0 us |     371.93 us |     496.52 us |  1.00 |    0.00 |   1687.5000 |  1343.7500 |  687.5000 |   8641014 B |       1.000 |
|   ReadTxtPdfLexer |      bug1669099 |     4,570.1 us |      66.28 us |      88.48 us |  0.25 |    0.01 |    328.1250 |   179.6875 |   62.5000 |   1790457 B |       0.207 |
| ReadTxtPdfiumCore |      bug1669099 |     9,053.9 us |     115.72 us |     150.47 us |  0.49 |    0.01 |           - |          - |         - |       195 B |       0.000 |
|                   |                 |                |               |               |       |         |             |            |           |             |             |
|     ReadTxtPdfPig |       issue1905 |    13,253.5 us |      89.13 us |     109.46 us |  1.00 |    0.00 |   1156.2500 |   578.1250 |         - |   6945942 B |       1.000 |
|   ReadTxtPdfLexer |       issue1905 |     3,160.8 us |      97.22 us |     129.78 us |  0.24 |    0.01 |    148.4375 |    50.7813 |         - |    683350 B |       0.098 |
| ReadTxtPdfiumCore |       issue1905 |    11,646.0 us |   1,381.62 us |   1,844.42 us |  0.90 |    0.14 |           - |          - |         - |       195 B |       0.000 |
|                   |                 |                |               |               |       |         |             |            |           |             |             |
|     ReadTxtPdfPig |      issue2128r |       914.8 us |       7.01 us |       9.12 us |  1.00 |    0.00 |     25.3906 |          - |         - |    113575 B |       1.000 |
|   ReadTxtPdfLexer |      issue2128r |     1,730.9 us |      97.03 us |     129.54 us |  1.89 |    0.14 |    226.5625 |   152.3438 |  150.3906 |   1459370 B |      12.849 |
| ReadTxtPdfiumCore |      issue2128r |    18,292.6 us |     215.00 us |     247.60 us | 20.01 |    0.37 |           - |          - |         - |       206 B |       0.002 |
```

In PDFs with heavy vector drawing usage, PdfLexer's performance advantage increases due to ignoring all non-text operations when extracting text. Below is an example of reading text from a CAD drawing with ~5X perf advantage to PdfPig and ~2X to Pdfium. In some edge cases performance differences are in the 20-50x range to PdfPig.

```
|          Method |      Mean |    Error |   StdDev | Ratio | RatioSD |      Gen0 |      Gen1 |      Gen2 |  Allocated | Alloc Ratio |
|---------------- |----------:|---------:|---------:|------:|--------:|----------:|----------:|----------:|-----------:|------------:|
|   ReadTxtPdfPig | 104.24 ms | 5.424 ms | 7.241 ms |  1.00 |    0.00 | 5800.0000 | 3200.0000 | 1400.0000 | 36897246 B |       1.000 |
| ReadTxtPdfLexer |  18.19 ms | 0.243 ms | 0.307 ms |  0.18 |    0.01 |  531.2500 |         - |         - |  2349022 B |       0.064 |
|   ReadTxtPdfium |  37.00 ms | 0.449 ms | 0.600 ms |  0.36 |    0.03 |         - |         - |         - |      235 B |       0.000 |
```
