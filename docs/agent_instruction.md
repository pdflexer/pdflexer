# PdfLexer AI Usage Guide

**Summary**: PdfLexer is a high-performance .NET PDF library balancing low-level DOM access with high-level helpers. It supports lazy parsing, efficient low-allocation operations, and PDF 2.0 features.

## 1. Setup & NuGets
*   **PdfLexer**: Core library.
*   **PdfLexer.CMaps**: Required for accurate text extraction. Call `PdfLexer.CMaps.CMaps.AddKnownPdfCMaps()` at startup.
*   **PdfLexer.ImageSharpExts**: Extensions for converting `XObjImage` to `ImageSharp` images.

## 2. Opening & Creating
**Lazy Parsing**: Default behavior. Parses structure only when accessed.
**Creation**:
```csharp
using PdfLexer;

// Create new
using var doc = PdfDocument.Create();

// Open (File path uses MemoryMappedFile - recommended)
using var existing = PdfDocument.Open("path.pdf");
// Open (Stream / Byte[])
using var streamDoc = PdfDocument.Open(stream);

// Configuration
var opts = new ParsingOptions { 
    ThrowOnErrors = true, 
    Eagerness = Eagerness.FullEager // Parse everything upfront
};
using var reliableDoc = PdfDocument.Open("path.pdf", new DocumentOptions { UserPass = "pass" });
```

## 3. DOM & Objects
The PDF DOM is exposed as `IPdfObject`.
*   **Key Types**: `PdfDictionary`, `PdfArray`, `PdfName`, `PdfString`, `PdfStream`, `PdfIndirectRef`, `PdfNumber` (`PdfIntNumber`, `PdfDoubleNumber`).
*   **Resolving**: Objects fetched from dicts/arrays might be `PdfIndirectRef`. usage of `.Resolve()` is critical.
    *   `obj.Resolve()`: Returns the direct object.
    *   `dict.GetRequiredValue<T>(key)`: Auto-resolves.
    *   `dict.Get<T>(key)`: Auto-resolves, returns null if missing/wrong type.

```csharp
var page = doc.Pages[0];
// Low-level DOM access
var resources = page.NativeObject.Get<PdfDictionary>(PdfName.Resources);
// Traverse
if (resources.TryGetValue(PdfName.Font, out var fontsObj)) {
    var fonts = fontsObj.Resolve() as PdfDictionary;
}
```

## 4. Page Operations
**Access**: `doc.Pages` is a `List<PdfPage>`.
**Merge/Split**: Efficiently copy pages between documents.
```csharp
using var src = PdfDocument.Open("a.pdf");
using var dest = PdfDocument.Create();
dest.Pages.AddRange(src.Pages); // Zero-copy, lazy merge
```
**Page Properties**:
*   `page.MediaBox`: `PdfRectangle` (x, y, w, h).
*   `page.Rotate`: Int (0, 90, 180, 270).
*   `page.Resources`: `PdfDictionary`.

## 5. Writing Content (`PageWriter`)
Use `PageWriter<T>` (T: `float` or `double`).
```csharp
using PdfLexer.Writing;

using var writer = new PageWriter<double>(page); // Auto-updates content on Dispose

// Graphics State
writer.Save()      // q
      .Restore();  // Q

// Drawing
writer.SetStrokeColor(1, 0, 0) // RGB Red
      .SetLineWidth(2)
      .DrawRect(10, 10, 100, 50)
      .Stroke();

// Text
writer.Text("Simple text", 50, 500);

// Images
// using PdfLexer.ImageSharpExts;
// var xObj = XObjImage.FromImageSharp(doc, imageSharpImage);
// writer.Image(xObj, x, y, w, h);
```

## 6. Text Extraction
**Requirement**: Load CMaps first.
**Classes**: `TextScanner` (chars), `SimpleWordScanner` (words).
```csharp
PdfLexer.CMaps.CMaps.AddKnownPdfCMaps(); // Once per app domain

var page = doc.Pages[0];
var scanner = page.GetWordScanner();
while (scanner.Advance()) {
    var word = scanner.CurrentWord;
    var bbox = scanner.GetWordBoundingBox(); // (llx, lly, urx, ury)
}
```

## 7. Structure & Accessibility (`StructuralBuilder`)
Create Tagged PDFs (PDF 2.0).
```csharp
using PdfLexer.DOM;

var builder = new StructuralBuilder();
builder.AddSection("Main")
       .AddParagraph("Intro")
           .WriteContent(writer, w => w.Text("Hello")) // Links content to tag
       .Back();
doc.Structure = builder;
```

## 8. Images Extraction
Use `ImageScanner`.
```csharp
var scanner = new ImageScanner(doc.Context, page);
while (scanner.Advance()) {
    if (scanner.TryGetImage(out var pdfImg)) {
        // Convert to ImageSharp (requires PdfLexer.ImageSharpExts)
        using var img = pdfImg.GetImageSharp(doc.Context);
    }
}
```

## 9. Advanced / Content Modification
Parse and modify content streams using `ContentModel`.
```csharp
// Load content model
var model = page.GetContentModel<double>();
// Modify operations (e.g. remove text)
model.RemoveAll(x => x is PdfLexer.Content.Model.TextOp);
// Save back
page.SetContentModel(model);
```

## Tips
*   **Dispose**: `PdfDocument` holds file locks (if mapped). Dispose it.
*   **Cloning**: Use `obj.CloneShallow()` when sharing mutable objects (like Dicts) across pages to avoid unintended side effects.
*   **PdfName**: `PdfName.Create("Key")` caches names.
