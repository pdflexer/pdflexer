# StreamingWriter

The `StreamingWriter` (in the `PdfLexer.Serializers` namespace) provides the ability to create large documents while not holding the contents in memory. As each page is added using `AddPage()` the `StreamingWriter` writes the required contents to the provided `Stream`. A basic example is shown below:

```csharp
var path = "pdfpath.pdf"
using var pdfStr  = File.OpenRead(path);
using var pdf = PdfDocument.Open(pdfStr);
using var fso = File.Create("output.pdf");
using var writer = new StreamingWriter(fso);
foreach (var page in pdf.Pages)
{
    writer.AddPage(page); // data flushed as each page added
}
writer.Complete(new PdfDictionary()); // trailer
```

## Resource Deduplication

### Note this is no longer the suggested method for resource deduplication due to poor performance on large documents

If using `StreamingWriter` it better to save the document without deduplication and then re-open (as a memory mapped file) and deduplicate using `PdfDocument.DeduplicateResources()` and resaving.

An alternative constructor `StreamingWriter(Stream stream, bool dedupXobj, bool inMemoryDedup)` is provided that allows resource deduplication to be enabled while writing. This tracks all `XObject` and `Font` resources as they are written and will deduplicate resources that are exactly the same. Object deduplication is complicated and reduces performance significantly but may be useful under certain circumstances (eg. combining a large number of like documents).

```csharp
var path = "pdfpath.pdf"
using var pdfStr  = File.OpenRead(path);
using var pdf = PdfDocument.Open(pdfStr);
using var fso = File.Create("output.pdf");
using var writer = new StreamingWriter(fso, true, false);
foreach (var page in pdf.Pages)
{
    // data flushed as each page added
    // duplicate fonts / xobjects are deduped
    writer.AddPage(page);
}
writer.Complete(new PdfDictionary()); // trailer
```
