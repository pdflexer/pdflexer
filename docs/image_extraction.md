# Image Extraction

Note: Alpha quality

Image extraction is largely completed excluding several specialized colorspaces. Long term goal is to allow easy image extraction and modification scenarios (replace image, downsize image, etc).

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
    // from PdfLexer.Images project
    using var imageSharpImage = pdfImg.GetImage(doc.Context);
    imageSharpImage.SaveAsPng("/tmp/path.png");
}
```
