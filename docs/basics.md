# Basic PdfLexer Usage

### Opening and Creating Documents

PDFs can currently be opened from a `byte[]` or `Stream` using the `PdfDocument.Open()` method. PDFs can be created using `PdfDocument.Create()`. `PdfDocument` data can be accessed by using the `Trailer`, `Catalog` and `Pages` properties on the document.

```csharp
using var doc = PdfDocument.Open(File.ReadAllBytes("input.pdf"));
// doc.Trailer -> pdf dictionary
// doc.Catalog -> pdf dictionary
Console.WriteLine($"I have {doc.Pages.Count} pages");
using var doc2 = PdfDocument.Create();
Console.WriteLine($"I have {doc2.Pages.Count} pages"); // 0
```

### Stream Based Access

Note: Beta quality

As mentioned above documents may be open using streams and reduce memory requirements for opening documents. A special `PdfDocument.OpenLowMemory()` method is available that tweaks logic to reduce memory requirements further and use local disk for temp storage.

```csharp
using var fs = File.Open("pdf.path")
using var doc = PdfDocument.Open(fs);
// -- or --
using var doc = PdfDocument.OpenLowMemory(fs);
...
```

### Traversing Document Structure

Documents can be inspected by either accessing the Trailer, Catalog, or individual page dictionaries. Below is an example of grabbing the resource entry on a page using different helper methods for getting values if the Pdf object type is known.

```csharp
using var doc = PdfDocument.Open(File.ReadAllBytes("input.pdf"));
var page = doc.Pages.First();

// return null if key does not exist or is not a dictionary
_ = page.Get<PdfDictionary>(PdfName.Resources);

// throws PdfLexerException if key does not exist or is not a dictionary
_ = page.GetRequiredvalue<PdfDictionary>(PdfName.Resources);

// throws PdfLexerException if key exists and is not PdfDictionary
// returns existing PdfDictionary if exists
// creates new empty PdfDictionary if does not exist
_ = page.GetOrCreateValue<PdfDictionary>(PdfName.Resources);
```

In some cases the type of pdf object is not known (eg. content stream can be array or single value). In these cases we can use the non-generic `GetRequiredValue()` that returns an `IPdfObject`. Below is example of calculating total length of content streams on a page. When using non-generic methods that return an `IPdfObject` the returned object may be a PDF indirect reference object. If the direct object is desired `IPdfObject.Resolve()` should be called first.

```csharp
// note the .Resolve() call which will turn indirect refs into direct objects
// this is done automatically when using the generic based method shown above
var val = page.GetRequiredValue(PdfName.Contents).Resolve();
// since we have direct object after calling resolve, can do type matching in c# for different options
long total = 0;
switch (val)
{
    case PdfArray arr:
        foreach (var str in arr)
        {
            // note no .Resolve() here since the Get*<T>() automatically resolve
            // indirect references
            var stream = str.GetValue<PdfStream>();
            total += stream.Dictionary.GetRequiredValue<PdfNumber>(PdfName.Length);
        }
        break;
    case PdfStream single:
        total += single.Dictionary.GetRequiredValue<PdfNumber>(PdfName.Length);
        break;
    default:
        throw new ApplicationException("Invalid Contents value in page:" + val.Type);
}
```

Alternatively `IPdfObject.GetPdfObjType()` will return the type of the direct object. The `Resolve()` approach is particularly useful when using c# pattern matching to cast to the appropriate object type but the below example accomplishes the same without `Resolve()` or pattern matching instead relying on `GetObject<T>()`.

```csharp
var val = page.GetRequiredValue(PdfName.Contents);
long total = 0;
// GetPdfObjType returns the type on the direct object even if val is an indirect object
switch (val.GetPdfObjType())
{
    case PdfObjectType.ArrayObj:
        var arr = val.GetValue<PdfArray>();
        foreach (var str in arr)
        {
            var stream = str.GetValue<PdfStream>();
            total += stream.Dictionary.GetRequiredValue<PdfNumber>(PdfName.Length);
        }
        break;
    case PdfObjectType.StreamObj:
        var single = val.GetValue<PdfStream>();
        total += single.Dictionary.GetRequiredValue<PdfNumber>(PdfName.Length);
        break;
    default:
        throw new ApplicationException("Invalid Contents value in page");
}
```

### Modifying documents

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

When modifying objects it will affect all pages / objects referencing the modified object in the document (eg. font referenced by multiple pages, updating on one page will affect all). PdfLexer provides helper methods on PdfDictionary and PdfArray objetcs `CloneShallow()` to create a shallow clone of the object so that modifications to the clone will only affect new references created to the object.
