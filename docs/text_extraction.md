# Text Extraction

Note: Beta quality

For most accurate text extraction the PdfLexer.CMaps nuget / project should be referenced and `CMaps.AddKnownPdfCMaps();` called before attempting extraction.

```csharp
// get all chars
CMaps.AddKnownPdfCMaps();
using var doc = PdfDocument.Open(data);
var page = doc.Pages.First();
var reader = page.GetTextScanner();
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
var reader = page.GetTextScanner();
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

### Character Extraction Performance

Initial benchmarking results with some random PDFs from tests set iterating over all characters in PDF. ~2-5x faster and ~10-20x less allocations with the exception of issue2128r.pdf (libraries getting different characters, need to determine which is correct and probably remove from set if PdfPig is wrong). Not exactly a 1-to-1 comparison as PdfPig / Pdfium are doing extra work but if you really just want text extracted with positions the results are realistic.
Note: PdfiumCore is c++ wrapper so GC / memory stats not meaningful.

```
|            Method |             Pdf |           Mean |        Error |       StdDev | Ratio | RatioSD |        Gen0 |       Gen1 |      Gen2 |   Allocated | Alloc Ratio |
|------------------ |---------------- |---------------:|-------------:|-------------:|------:|--------:|------------:|-----------:|----------:|------------:|------------:|
|   ReadTxtPdfLexer |  __bpl13210.pdf |   127,117.2 us |  1,622.82 us |  2,166.42 us |  1.00 |    0.00 |   2000.0000 |  1000.0000 |         - |  13371163 B |       1.000 |
|     ReadTxtPdfPig |  __bpl13210.pdf |   441,769.4 us |  6,425.38 us |  7,890.94 us |  3.48 |    0.09 |  64000.0000 |  9000.0000 |         - | 309797832 B |      23.169 |
| ReadTxtPdfiumCore |  __bpl13210.pdf |   938,071.7 us |  9,385.09 us | 11,172.28 us |  7.41 |    0.14 |           - |          - |         - |     24520 B |       0.002 |
|                   |                 |                |              |              |       |         |             |            |           |             |             |
|   ReadTxtPdfLexer |   __ecma262.pdf |   279,521.7 us |  1,806.69 us |  2,349.20 us |  1.00 |    0.00 |   4000.0000 |  1500.0000 |         - |  28179192 B |       1.000 |
|     ReadTxtPdfPig |   __ecma262.pdf |   890,434.5 us |  7,165.72 us |  9,062.34 us |  3.19 |    0.04 | 100000.0000 | 40000.0000 | 1000.0000 | 572496224 B |      20.316 |
| ReadTxtPdfiumCore |   __ecma262.pdf | 1,922,444.8 us | 29,846.60 us | 36,654.31 us |  6.89 |    0.15 |           - |          - |         - |     17352 B |       0.001 |
|                   |                 |                |              |              |       |         |             |            |           |             |             |
|   ReadTxtPdfLexer |    __gesamt.pdf |   213,240.5 us |  3,372.72 us |  4,142.00 us |  1.00 |    0.00 |   1666.6667 |   666.6667 |         - |   9439448 B |       1.000 |
|     ReadTxtPdfPig |    __gesamt.pdf |   823,523.2 us | 47,649.19 us | 58,517.49 us |  3.87 |    0.31 | 112000.0000 | 35000.0000 |         - | 582032984 B |      61.660 |
| ReadTxtPdfiumCore |    __gesamt.pdf |   815,046.2 us | 22,493.10 us | 29,247.38 us |  3.82 |    0.16 |           - |          - |         - |     26952 B |       0.003 |
|                   |                 |                |              |              |       |         |             |            |           |             |             |
|   ReadTxtPdfLexer | __issue1133.pdf |    48,675.6 us |  2,447.92 us |  3,267.90 us |  1.00 |    0.00 |    800.0000 |   200.0000 |         - |   5313042 B |       1.000 |
|     ReadTxtPdfPig | __issue1133.pdf |   183,939.5 us |  4,432.71 us |  5,917.54 us |  3.80 |    0.35 |  19000.0000 |  3500.0000 |  500.0000 | 113377216 B |      21.339 |
| ReadTxtPdfiumCore | __issue1133.pdf |   202,531.0 us |  7,366.32 us |  9,316.03 us |  4.13 |    0.21 |           - |          - |         - |      3432 B |       0.001 |
|                   |                 |                |              |              |       |         |             |            |           |             |             |
|   ReadTxtPdfLexer |      bug1669099 |     4,250.9 us |     97.71 us |    130.44 us |  1.00 |    0.00 |    335.9375 |   195.3125 |   70.3125 |   1790403 B |       1.000 |
|     ReadTxtPdfPig |      bug1669099 |    16,540.8 us |    721.93 us |    963.76 us |  3.90 |    0.27 |   1687.5000 |  1312.5000 |  687.5000 |   8644116 B |       4.828 |
| ReadTxtPdfiumCore |      bug1669099 |     7,951.6 us |     99.82 us |    126.24 us |  1.87 |    0.07 |           - |          - |         - |       195 B |       0.000 |
|                   |                 |                |              |              |       |         |             |            |           |             |             |
|   ReadTxtPdfLexer |       issue1905 |     3,091.4 us |     52.91 us |     68.80 us |  1.00 |    0.00 |    148.4375 |    50.7813 |         - |    683350 B |       1.000 |
|     ReadTxtPdfPig |       issue1905 |    11,956.6 us |    228.61 us |    289.12 us |  3.87 |    0.16 |   1156.2500 |   578.1250 |         - |   6945942 B |      10.165 |
| ReadTxtPdfiumCore |       issue1905 |     9,477.4 us |    716.87 us |    932.14 us |  3.07 |    0.33 |           - |          - |         - |       195 B |       0.000 |
|                   |                 |                |              |              |       |         |             |            |           |             |             |
|   ReadTxtPdfLexer |      issue2128r |     1,539.0 us |     85.35 us |    113.94 us |  1.00 |    0.00 |    179.6875 |   125.0000 |  105.4688 |   1459202 B |       1.000 |
|     ReadTxtPdfPig |      issue2128r |       844.4 us |     10.10 us |     11.63 us |  0.54 |    0.04 |     26.3672 |          - |         - |    113574 B |       0.078 |
| ReadTxtPdfiumCore |      issue2128r |    18,045.9 us |    259.17 us |    308.53 us | 11.59 |    0.82 |           - |          - |         - |       206 B |       0.000 |
```

In PDFs with heavy vector drawing usage, PdfLexer's performance advantage increases due to ignoring all non-text operations when extracting text. Below is an example of reading text from a CAD drawing with ~5X perf advantage to PdfPig and ~2X to Pdfium. In some edge cases performance differences are in the 20-50x range to PdfPig.

```
|          Method |      Mean |    Error |   StdDev | Ratio | RatioSD |      Gen0 |      Gen1 |      Gen2 |  Allocated | Alloc Ratio |
|---------------- |----------:|---------:|---------:|------:|--------:|----------:|----------:|----------:|-----------:|------------:|
|   ReadTxtPdfPig | 104.24 ms | 5.424 ms | 7.241 ms |  1.00 |    0.00 | 5800.0000 | 3200.0000 | 1400.0000 | 36897246 B |       1.000 |
| ReadTxtPdfLexer |  18.19 ms | 0.243 ms | 0.307 ms |  0.18 |    0.01 |  531.2500 |         - |         - |  2349022 B |       0.064 |
|   ReadTxtPdfium |  37.00 ms | 0.449 ms | 0.600 ms |  0.36 |    0.03 |         - |         - |         - |      235 B |       0.000 |
```

### Word Extraction Performance

Word extraction performance increases vs just character extraction relative to alteratives and is ~3-5X faster. Once again not a direct comparison as PdfLexer is using a simpler algorithm to determine words but is effective in most cases (issue2128r still odd outlier).

```
|            Method |         testPdf |           Mean |        Error |       StdDev | Ratio | RatioSD |        Gen0 |       Gen1 |      Gen2 |   Allocated | Alloc Ratio |
|------------------ |---------------- |---------------:|-------------:|-------------:|------:|--------:|------------:|-----------:|----------:|------------:|------------:|
|   ReadTxtPdfLexer |  __bpl13210.pdf |   126,325.7 us |  2,477.57 us |  3,307.49 us |  1.00 |    0.00 |   1750.0000 |  1000.0000 |         - |  13327716 B |       1.000 |
|     ReadTxtPdfPig |  __bpl13210.pdf |   437,932.3 us |  8,083.43 us | 10,222.95 us |  3.46 |    0.13 |  64000.0000 |  9000.0000 |         - | 309797832 B |      23.245 |
| ReadTxtPdfiumCore |  __bpl13210.pdf |   977,775.0 us | 27,647.57 us | 36,908.71 us |  7.75 |    0.36 |           - |          - |         - |     24520 B |       0.002 |
|                   |                 |                |              |              |       |         |             |            |           |             |             |
|   ReadTxtPdfLexer |   __ecma262.pdf |   291,551.4 us |  3,278.49 us |  4,376.69 us |  1.00 |    0.00 |   4000.0000 |  1500.0000 |         - |  28179192 B |       1.000 |
|     ReadTxtPdfPig |   __ecma262.pdf |   894,364.4 us | 24,131.78 us | 28,727.16 us |  3.07 |    0.10 |  99000.0000 | 43000.0000 | 1000.0000 | 572495360 B |      20.316 |
| ReadTxtPdfiumCore |   __ecma262.pdf | 1,998,108.9 us | 47,743.33 us | 60,380.02 us |  6.85 |    0.25 |           - |          - |         - |     17352 B |       0.001 |
|                   |                 |                |              |              |       |         |             |            |           |             |             |
|   ReadTxtPdfLexer |    __gesamt.pdf |   226,715.3 us |  4,542.59 us |  5,744.92 us |  1.00 |    0.00 |   1666.6667 |   666.6667 |         - |   9267096 B |       1.000 |
|     ReadTxtPdfPig |    __gesamt.pdf |   790,364.9 us |  7,849.22 us |  8,724.38 us |  3.48 |    0.10 | 112000.0000 | 36000.0000 |         - | 582032984 B |      62.806 |
| ReadTxtPdfiumCore |    __gesamt.pdf |   853,628.8 us | 47,969.97 us | 60,666.66 us |  3.77 |    0.29 |           - |          - |         - |     26952 B |       0.003 |
|                   |                 |                |              |              |       |         |             |            |           |             |             |
|   ReadTxtPdfLexer | __issue1133.pdf |    47,323.5 us |    422.67 us |    519.08 us |  1.00 |    0.00 |    818.1818 |   272.7273 |         - |   5312940 B |       1.000 |
|     ReadTxtPdfPig | __issue1133.pdf |   202,789.1 us | 10,386.24 us | 13,865.34 us |  4.33 |    0.28 |  19000.0000 |  4000.0000 |  500.0000 | 113377224 B |      21.340 |
| ReadTxtPdfiumCore | __issue1133.pdf |   197,541.8 us |  8,856.55 us | 10,543.09 us |  4.18 |    0.24 |           - |          - |         - |      3432 B |       0.001 |
|                   |                 |                |              |              |       |         |             |            |           |             |             |
|   ReadTxtPdfLexer |      bug1669099 |     4,209.5 us |     72.36 us |     94.09 us |  1.00 |    0.00 |    343.7500 |   187.5000 |   70.3125 |   1790509 B |       1.000 |
|     ReadTxtPdfPig |      bug1669099 |    17,044.0 us |    783.18 us |  1,045.52 us |  4.03 |    0.26 |   1687.5000 |  1343.7500 |  687.5000 |   8642666 B |       4.827 |
| ReadTxtPdfiumCore |      bug1669099 |     8,495.4 us |    123.98 us |    147.58 us |  2.01 |    0.05 |           - |          - |         - |       195 B |       0.000 |
|                   |                 |                |              |              |       |         |             |            |           |             |             |
|   ReadTxtPdfLexer |       issue1905 |     3,124.9 us |     25.65 us |     30.53 us |  1.00 |    0.00 |    148.4375 |    50.7813 |         - |    683350 B |       1.000 |
|     ReadTxtPdfPig |       issue1905 |    13,718.8 us |    844.23 us |  1,127.02 us |  4.36 |    0.38 |   1156.2500 |   562.5000 |         - |   6945953 B |      10.165 |
| ReadTxtPdfiumCore |       issue1905 |     8,278.6 us |    218.07 us |    275.79 us |  2.65 |    0.10 |           - |          - |         - |       195 B |       0.000 |
|                   |                 |                |              |              |       |         |             |            |           |             |             |
|   ReadTxtPdfLexer |      issue2128r |     1,649.2 us |     66.39 us |     88.63 us |  1.00 |    0.00 |    214.8438 |   157.2266 |  139.6484 |   1459318 B |       1.000 |
|     ReadTxtPdfPig |      issue2128r |       846.5 us |      9.03 us |     11.74 us |  0.52 |    0.03 |     26.3672 |          - |         - |    113574 B |       0.078 |
| ReadTxtPdfiumCore |      issue2128r |    18,003.6 us |    371.54 us |    483.10 us | 10.97 |    0.53 |           - |          - |         - |       206 B |       0.000 |
```
