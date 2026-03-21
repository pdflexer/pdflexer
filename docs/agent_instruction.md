# PdfLexer AI Usage Guide

**Summary**: PdfLexer is a high-performance .NET PDF library with two complementary access layers:

- a high-level wrapper layer built around `PdfDocument` and `PdfPage`
- a low-level native PDF object layer built around `PdfDictionary`, `PdfArray`, `IPdfObject`, and `PdfIndirectRef`

Preferred guidance:

- use wrappers for common tasks
- use `NativeObject` as the raw escape hatch

## 1. Setup and NuGets

- `PdfLexer`: core library
- `PdfLexer.CMaps`: recommended for accurate text extraction
- `PdfLexer.ImageSharpExts`: ImageSharp conversions for extracted PDF images

```csharp
PdfLexer.CMaps.CMaps.AddKnownPdfCMaps(); // once at startup if doing text extraction
```

## 2. Opening and Creating

```csharp
using PdfLexer;

using var doc = PdfDocument.Open("path.pdf");
using var created = PdfDocument.Create();
using var protectedDoc = PdfDocument.Open("protected.pdf", new DocumentOptions
{
    UserPass = "pass"
});
```

## 3. Preferred Access Style

For common operations, start with wrappers:

```csharp
var page = doc.Pages[0];
var mediaBox = page.MediaBox;
var contents = page.Contents.ToList();
```

For unsupported or low-level features, drop down to the raw PDF dictionary:

```csharp
var pageDict = page.NativeObject;
var resources = pageDict.Get<PdfDictionary>(PdfName.Resources);
var rawContents = pageDict.GetRequiredValue(PdfName.Contents);
```

## 4. Raw DOM and Object Traversal

The main low-level types are:

- `PdfDictionary`
- `PdfArray`
- `PdfName`
- `PdfString`
- `PdfStream`
- `PdfIndirectRef`
- `PdfNumber`

Typed dictionary access auto-resolves indirect references:

```csharp
var pageDict = page.NativeObject;

var resources = pageDict.Get<PdfDictionary>(PdfName.Resources);
var mediaBox = pageDict.GetRequiredValue<PdfArray>(PdfName.MediaBox);
```

Other available low-level helpers:

- `pageDict.TryGetValue(PdfName.Resources, out var rawValue)` for optional raw access
- `pageDict.TryGetValue<PdfDictionary>(PdfName.Resources, out var typedValue, errorOnMismatch: false)` for optional typed access

`Get*`/`GetRequiredValue*` remain the preferred accessors for most examples. Note that `TryGetValue<T>` defaults `errorOnMismatch` to `true`, so it can throw if the key exists but resolves to a different type. Use `errorOnMismatch: false` if you want it to simply return `false` on mismatch.

Once you already have an `IPdfObject`, prefer `GetAs<T>()` and `GetAsOrNull<T>()` for object-level typed access rather than the older `GetValue<T>()` aliases.

When you do not know the underlying type, use raw access and resolve explicitly:

```csharp
var obj = page.NativeObject.GetRequiredValue(PdfName.Contents).Resolve();

if (obj is PdfArray arr)
{
    foreach (var item in arr)
    {
        var stream = item.GetAs<PdfStream>();
    }
}
```

## 5. Page Operations

`doc.Pages` is a `List<PdfPage>`.

```csharp
using var src = PdfDocument.Open("a.pdf");
using var dest = PdfDocument.Create();
dest.Pages.AddRange(src.Pages);
```

Useful `PdfPage` members:

- `page.MediaBox`
- `page.Resources`
- `page.Contents`
- `page.GetWriter()`
- `page.GetTextScanner()`
- `page.GetWordScanner()`
- `page.GetContentModel<T>()`

## 6. Writing Content

Use `page.GetWriter()` for page content generation or modification.

```csharp
using PdfLexer.Writing;

var page = doc.AddPage();

using var writer = page.GetWriter();
writer
    .SetStrokingRGB(255, 0, 0)
    .Rect(10, 10, 100, 50)
    .Stroke()
    .BeginText()
    .Font(Base14.Helvetica, 12)
    .TextMove(50, 500)
    .Text("Simple text")
    .EndText();
```

## 7. Text Extraction

Use the page scanning helpers:

```csharp
PdfLexer.CMaps.CMaps.AddKnownPdfCMaps();

using var doc = PdfDocument.Open("input.pdf");
var page = doc.Pages[0];

var scanner = page.GetWordScanner();
while (scanner.Advance())
{
    var word = scanner.CurrentWord;
    var bbox = scanner.GetWordBoundingBox();
}
```

## 8. Structure and Tagged PDF Writing

```csharp
using PdfLexer.DOM;

var page = doc.AddPage();
using var writer = page.GetWriter();

var builder = new StructuralBuilder();
builder.AddSection("Main")
       .AddParagraph("Intro")
       .WriteContent(writer, w =>
       {
           w.BeginText()
            .Font(Base14.Helvetica, 12)
            .TextMove(50, 700)
            .Text("Hello")
            .EndText();
       });

doc.Structure = builder;
```

## 9. Image Extraction

```csharp
using var doc = PdfDocument.Open("input.pdf");
var page = doc.Pages[0];

var scanner = new ImageScanner(doc.Context, page);
while (scanner.Advance())
{
    if (scanner.TryGetImage(out var pdfImg))
    {
        // Requires PdfLexer.ImageSharpExts
        using var img = pdfImg.GetImageSharp(doc.Context);
    }
}
```

## 10. Advanced Content Access

For advanced editing or inspection, the content model is the higher-level API over raw content operators:

```csharp
var model = page.GetContentModel<double>();

foreach (var item in model)
{
    Console.WriteLine(item.Type);
}
```

If you need exact PDF content stream structure, use `PageContentScanner` or raw page dictionaries instead.

## Tips

- Dispose `PdfDocument` promptly, especially when opening by file path.
- Use wrappers first; drop to `NativeObject` only when needed.
- Use `CloneShallow()` before editing shared dictionaries/arrays.
