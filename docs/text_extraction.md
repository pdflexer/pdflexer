# Text Extraction

Note: Work in progress / non-functional

Lots of work TODO regarding fonts / glyphs.

```csharp
// get all chars
var doc = PdfDocument.Open(data);
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
var page = doc.Pages.First();
var reader = new TextScanner(doc.Context, page);
while (reader.Advance())
{
    if (reader.Position.LLy > miny && reader.Position.LLx > minx) {
        sb.Append(reader.Glyph.Char);
    }
}
var str = sb.ToString();
```
