# Content Creation

Note: Work in progress

General api has been adapted from Pdfkit (js).

```csharp
// get all chars
using var doc = PdfDocument.Create();

var pg = doc.AddPage();
pg.MediaBox.URx = 500;
pg.MediaBox.URy = 200;

using var writer = pg.GetWiter();
writer.SetStrokingRGB(255, 0, 0);
      .LineWidth(0.05m)
      .Rect(10m, 10m, 10m, 10m)
      .Stroke();
      .SetStrokingRGB(0, 0, 0);
      .Circle((15m, 15m, 5m)
      .Stroke();

```
