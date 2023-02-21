# Image Extraction

Image extraction is largely completed excluding several specialized colorspaces and image filters. Long term goal is to allow easy image extraction and modification scenarios (replace image, downsize image, etc).

Note: Extensions to convert pdf images to ImageSharp format provided by PdfLexer.ImageSharpExts nuget / project.

Gaps:

- Colorspaces:
  - /ICCBased
  - /Separation
- Filters / encodings:
  - /JBIG2Decode
  - /JPXDecode (works by using ImageSharp under some circumstances)
  - /DCTDecode (works by using ImageSharp under some circumstances)

```csharp
// get all chars
using var doc = PdfDocument.Open(data);
var page = doc.Pages.First();
var reader = new ImageScanner(doc.Context, page);
while (reader.Advance())
{
    if (!reader.TryGetImage(out var pdfImg))
    {
        throw new ApplicationException("Image read failure failure");
    }

    // Image location / size:
    // pdfImg.X
    // pdfImg.Y
    // pdfImg.W
    // pdfImg.H

    // ImageSharp conversion supported through
    // from PdfLexer.ImageSharpExts project / nuget
    using var imageSharpImage = pdfImg.GetImageSharp(doc.Context);
    imageSharpImage.SaveAsPng("/tmp/path.png");
}
```

Some bench results with pdfium with both converting PDF image to an ImageSharp image.

```
|     Method |          testPdf |        Mean |       Error |      StdDev | Ratio | RatioSD |      Gen0 |      Gen1 |      Gen2 | Allocated | Alloc Ratio |
|----------- |----------------- |------------:|------------:|------------:|------:|--------:|----------:|----------:|----------:|----------:|------------:|
|   PdfLexer | 1bit_decode_gray |  1,309.9 us |    40.78 us |    51.57 us |  1.00 |    0.00 |    5.8594 |    1.9531 |    1.9531 |  333811 B |       1.000 |
| PdfiumCore | 1bit_decode_gray |  2,097.9 us |    16.15 us |    20.43 us |  1.60 |    0.06 |         - |         - |         - |    3237 B |       0.010 |
|            |                  |             |             |             |       |         |           |           |           |           |             |
|   PdfLexer | 4bit_indexed_rgb | 16,146.1 us |   275.97 us |   349.02 us |  1.00 |    0.00 | 1250.0000 | 1250.0000 | 1250.0000 |   38913 B |        1.00 |
| PdfiumCore | 4bit_indexed_rgb | 41,385.3 us | 1,919.99 us | 2,563.13 us |  2.57 |    0.17 |  142.8571 |  142.8571 |  142.8571 |    1510 B |        0.04 |
|            |                  |             |             |             |       |         |           |           |           |           |             |
|   PdfLexer |          ccitt_1 |    806.1 us |    11.68 us |    15.19 us |  1.00 |    0.00 |   10.7422 |    1.9531 |         - |   45777 B |        1.00 |
| PdfiumCore |          ccitt_1 |    524.8 us |     3.64 us |     4.61 us |  0.65 |    0.01 |         - |         - |         - |    1249 B |        0.03 |
```
