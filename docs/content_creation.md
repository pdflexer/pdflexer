# Content Creation

Note: Work in progress

General api has been adapted from Pdfkit (js).

### Creating a New Page

```csharp
using var doc = PdfDocument.Create();

var pg = doc.AddPage(); // defaults to letter page size
// or specify a page size ->  doc.AddPage(PageSize.LEGAL);
// or page size can be modified by accessing /modifying the page media box:
pg.MediaBox.LLx = 0;
pg.MediaBox.LLy = 0;
pg.MediaBox.URx = 500;
pg.MediaBox.URy = 200;

{ // scope for writer, saved on dispose
      using var writer = pg.GetWiter();
      writer.SetStrokingRGB(255, 0, 0);
            .LineWidth(0.05m)
            .Rect(10m, 10m, 10m, 10m)
            .Stroke();
            .SetStrokingRGB(0, 0, 0);
            .Circle((15m, 15m, 5m)
            .Stroke();
}
doc.SaveTo("newFilePath.pdf")
```

### Modifying An Existing Page

```csharp
using var doc = PdfDocument.Open("filePath.pdf");
var pg = doc.Pages.First();
{ // scope for writer, saved on dispose
      using var writer = pg.GetWiter(PageWriterMode.Append);
      writer.Font(ContentWriter.Base14.Courier, 10) // font / size
            .TexdtMove(100f, 200f)
            .Text("Hello world");
}
doc.SaveTo("newFilePath.pdf")
```
