# Merging and /or Splitting PDFs

Splitting and merging of PDFs can be accomplished by copying pages from one PDF to another. The lazy parsing features of PDF lexer make splitting and merging PDFs very efficient as a lot of the parsing can be skipping and bytes simply copied.

```csharp
// splitting pdf
using var doc = PdfDocument.Open("input.pdf");
using var doc2 = PdfDocument.Create();
using var doc3 = PdfDocument.Create();
doc2.Pages.AddRange(doc.Pages.Take(10));
doc3.Pages.AddRange(doc.Pages.Skip(10));
doc2.SaveTo(fsOne);
doc2.SaveTo(fsTwo);

// merging
using var docA = PdfDocument.Open("inputA.pdf");
using var docB = PdfDocument.Open("inputB.pdf");
using var combined = PdfDocument.Create();
combined.Pages.AddRange(docA.Pages);
combined.Pages.AddRange(docb.Pages);
using var fsOut = File.Create("combined.pdf");
combined.SaveTo(fsOut);
```

#### Merging benchmarks

note: need to do more varied scenarios

```
|        Method |     Mean |     Error |    StdDev | Ratio | RatioSD |    Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|-------------- |---------:|----------:|----------:|------:|--------:|---------:|---------:|---------:|----------:|
| MergePdfSharp | 2.142 ms | 0.0814 ms | 0.1029 ms |  1.00 |    0.00 | 554.6875 | 328.1250 | 109.3750 |      3 MB |
|   MergePdfPig | 2.529 ms | 0.0462 ms | 0.0550 ms |  1.19 |    0.05 | 496.0938 | 164.0625 | 164.0625 |      2 MB |
| MergePdfLexer | 1.050 ms | 0.1757 ms | 0.2158 ms |  0.49 |    0.09 | 392.5781 | 392.5781 | 148.4375 |      1 MB |
```

#### Splitting benchmarks

note: need to do more varied scenarios

```
|        Method |     Mean |     Error |    StdDev | Ratio | RatioSD |     Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|-------------- |---------:|----------:|----------:|------:|--------:|----------:|---------:|---------:|----------:|
| SplitPdfSharp | 6.845 ms | 0.5337 ms | 0.6750 ms |  1.00 |    0.00 | 1593.7500 | 562.5000 | 234.3750 |      9 MB |
|   SplitPdfPig | 8.546 ms | 0.3733 ms | 0.4721 ms |  1.26 |    0.12 | 1609.3750 | 765.6250 | 484.3750 |      8 MB |
| SplitPdfLexer | 2.513 ms | 0.0585 ms | 0.0718 ms |  0.37 |    0.04 |  707.0313 | 535.1563 | 269.5313 |      4 MB |
```
