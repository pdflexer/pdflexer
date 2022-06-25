# pdflexer
pdflexer is a PDF parsing library. It is focused on efficient parsing and modification of PDF files and is mainly targeted for users familiar with the pdf spec. The parsing logic was implemented from scratch but some higher level functionality (eg. filters) has been ported from the [pdf.js](https://github.com/mozilla/pdf.js) project.

pdflexer differs from existing .net libraries in that it:
* Is primarly designed for PDF modification (not just reading). Any object / page read from a PDF can be modified and written to others PDFs.
* Has lazy parsing features which allow objects to be parsed on demand increasing performance in many cases.
* Designed for direct access to the native PDF objects types. Any higher level objects are simple wrappers areound the native pdf object types (eg `PdfPage` is a wrapper around a `PdfDictionary`. The `PdfDictionary` can be directly modified for features not implemented on `PdfPage`)
* Attempts to be performant / efficient. Not a ton of effort has been put in here but it is a goal to keep this in mind.

### State of library
| Feature         | WIP | Alpha | Beta | Release |
| ---------       | ----| ----- | ---- | ----    |
| Document access |  |     |      |  :heavy_check_mark:  |
|  Stream based access |  |     |  :heavy_check_mark:  |    |
| General modification <br> (non page content) |  |     |  :heavy_check_mark:  |    |
| Merging / splitting |  | |:heavy_check_mark: |  |
| [Streaming writer](docs/streaming_writer.md#section)  | |  |:heavy_check_mark: |  |
| [Resource dedup](docs/streaming_writer.md#resource-deduplication) | | :heavy_check_mark:  | |  |
| [Page content access](docs/page_content.md) | :heavy_check_mark: |   | |  |
| Text extraction | |  | |  |
| Image extraction | |  | |  |
| Page content modification | |  | |  |

### Major Gaps
- [ ] PDF encryption support
- [ ] Filter support (deflate, ascii85, asciihex completed)
- [ ] Content stream access (partially complete)
- [ ] Higher level utility functions (eg. image / text extraction), currently all APIs are through the raw PdfObjects requiring extensive knowledge of the PDF spec to use pdflexer for most tasks.
- [ ] Public API cleanup / documentation. Lots of classes / properties exposed that will likely be internalized.
- [ ] Documentation / examples

### Work In Progress
- *Low memory mode / Stream support*: Goal is to support modification / manipulation of large PDFs in resource contrained environments (eg. Lambda, RPI, etc). Basic stream support implemented but lacking some repair features. Ceratin areas of library need unification between Span<byte> based processing and Stream processing. `PdfDocument.OpenLowMemory(Stream stream)` added which results in buffering to disk of some content to reduce memory storage but some parts of library still rely on in memory storage (eg. content streams). 
- *StreamingWriter*: Flushes pages as they are written to reduce memory consumption.
- *Pdf resource deduplication*: Optionally removes exact duplicates of images / fonts.

## Basic Usage
PDFs can currently be opened from a `byte[]` or `Stream` using the `PdfDocument.Open()` method. `Streams` lack some capabilities to repair corrupt PDFs that are implemented for `byte[]`. PDFs can be created using `PdfDocument.Create()`.  `PdfDocument` data can be accessed by using the `Trailer`, `Catalog` and `Pages` properties on the document.

```csharp
using var doc = PdfDocument.Open(File.ReadAllBytes("input.pdf"));
// doc.Trailer -> pdf dictionary
// doc.Catalog -> pdf dictionary
Console.WriteLine($"I have {doc.Pages.Count} pages");
using var doc2 = PdfDocument.Create();
Console.WriteLine($"I have {doc2.Pages.Count} pages"); // 0
```


All PDF objects implement the `IPdfObject` interface shown below.
```csharp
/// <summary>
/// Interface implemented by all PdfObjects parsed from Pdfs.
/// </summary>
public interface IPdfObject
{
    /// <summary>
    /// The underlying type of this Pdf Object.
    /// Note: this may be <see cref="PdfObjectType.IndirectRefObj"/>.
    /// Use IPdfObject.GetPdfObjType() to always return the
    /// direct object type.
    /// </summary>
    public PdfObjectType Type { get; }
    ...
}
```

Library users will most often be inerested in the extension methods below that simplify access to `IPdfObjects`:

```csharp
/// <summary>
/// Gets the pdf object type of the direct object. This will
/// return the type of the referenced object if <see cref="item"/>
/// is an <see cref="PdfObjectType.IndirectRefObj"/>.
/// </summary>
/// <param name="item"></param>
/// <returns></returns>
public static PdfObjectType GetPdfObjType(this IPdfObject item){}
/// <summary>
/// Returns the underlying PdfObject type.
/// If <see cref="item"/> is an indirect reference
/// the direct object is returned.
/// </summary>
/// <typeparam name="T">Type of PdfObject</typeparam>
/// <param name="item"></param>
/// <returns></returns>
/// <exception cref="PdfTokenMismatchException">Excetion if <see cref="item"/> is not of type <see cref="T"/></exception>
public static T GetValue<T>(this IPdfObject item) where T : IPdfObject
```

Some of the PdfObject types also provide helpers such as the `PdfDictionary` type which has `TryGetValue<T>()` and `GetRequiredValue<T>` methods to return dictionary contents and resolve any indirect / lazy references.

### Document Modification
The `PdfDocument` class includes a `SaveTo(Stream stream)` method. Pdf documents can be opened and objects modified / replaced and then saved:

```csharp
using var doc = PdfDocument.Open(File.ReadAllBytes("input.pdf"));
doc.Trailer["/NewValue"] = new PdfString("This is a new value added to existing pdf.");
using var fs = File.Create("output.pdf");
doc.SaveTo(fs);
```

Objects can also be copied between PDF documents. The below example copies document metadata from one PDF to another.

```csharp
using var doc = PdfDocument.Open(File.ReadAllBytes("input.pdf"));
using var doc2 = PdfDocument.Open(File.ReadAllBytes("input2.pdf"));
doc2.Trailer[PdfName.Info] = doc.Trailer[PdfName.Info]
doc2.SaveTo(fs);
```

### Splitting / Merging
Splitting and merging of PDFs can be accomplished by copying pages from one PDF to another.
```csharp
// splitting pdf
using var doc = PdfDocument.Open(File.ReadAllBytes("input.pdf"));
using var doc2 = PdfDocument.Create();
using var doc3 = PdfDocument.Create();
doc2.Pages.AddRange(doc.Pages.Take(10));
doc3.Pages.AddRange(doc.Pages.Skip(10));
doc2.SaveTo(fsOne);
doc2.SaveTo(fsTwo);
```

#### Merging benchmarks
note: may be unrealistic, need to do more varied scenarios
```
|        Method |     Mean |     Error |    StdDev | Ratio | RatioSD |    Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|-------------- |---------:|----------:|----------:|------:|--------:|---------:|---------:|---------:|----------:|
| MergePdfSharp | 2.142 ms | 0.0814 ms | 0.1029 ms |  1.00 |    0.00 | 554.6875 | 328.1250 | 109.3750 |      3 MB |
|   MergePdfPig | 2.529 ms | 0.0462 ms | 0.0550 ms |  1.19 |    0.05 | 496.0938 | 164.0625 | 164.0625 |      2 MB |
| MergePdfLexer | 1.050 ms | 0.1757 ms | 0.2158 ms |  0.49 |    0.09 | 392.5781 | 392.5781 | 148.4375 |      1 MB |
```
PdfPig seems too slow here... need to research. Previous experience shows it's faster than PdfSharp.
Note: PdfLexer perf advantage degrades some with large pdfs due to the way indirect references are tracked in dictionaries

#### Splitting benchmarks
note: may be unrealistic, need to do more varied scenarios

```
|        Method |     Mean |     Error |    StdDev | Ratio | RatioSD |     Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|-------------- |---------:|----------:|----------:|------:|--------:|----------:|---------:|---------:|----------:|
| SplitPdfSharp | 6.845 ms | 0.5337 ms | 0.6750 ms |  1.00 |    0.00 | 1593.7500 | 562.5000 | 234.3750 |      9 MB |
|   SplitPdfPig | 8.546 ms | 0.3733 ms | 0.4721 ms |  1.26 |    0.12 | 1609.3750 | 765.6250 | 484.3750 |      8 MB |
| SplitPdfLexer | 2.513 ms | 0.0585 ms | 0.0718 ms |  0.37 |    0.04 |  707.0313 | 535.1563 | 269.5313 |      4 MB |
```

