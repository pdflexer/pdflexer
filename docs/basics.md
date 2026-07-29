# Basic PdfLexer Usage

## Opening and Creating Documents

PDFs can be opened from a file path, `byte[]`, or `Stream` using `PdfDocument.Open()`.

```csharp
using PdfLexer;

using var doc = PdfDocument.Open("input.pdf");
Console.WriteLine($"I have {doc.Pages.Count} pages");

using var created = PdfDocument.Create();
Console.WriteLine($"I have {created.Pages.Count} pages"); // 0
```

PDFs can be created using `PdfDocument.Create()`. Any objects or pages can be copied from existing PDFs to new documents.

Important:

- When copying pages and objects from another document, the source document must not be disposed before the destination document is saved or disposed.
- If you need to dispose the source document early, either:
  - use `StreamingWriter`, or
  - call `FullyLoad()` on the objects you want to keep

```csharp
using var src = PdfDocument.Open("input.pdf");
using var dest = PdfDocument.Create();

// zero-copy page reuse
dest.Pages.AddRange(src.Pages);
```

## Parsing Options and Errors

`ParsingOptions` controls how PDFs are parsed. `DocumentOptions` is used for document-open concerns such as passwords.

```csharp
using var doc = PdfDocument.Open("protected.pdf", new DocumentOptions
{
    UserPass = "pass"
});
```

## Preferred Access Model

Preferred usage is:

- use `PdfDocument` and `PdfPage` wrappers for common access
- use `page.NativeObject` when you need raw PDF dictionary traversal

Examples of common high-level access:

```csharp
using var doc = PdfDocument.Open("input.pdf");
var page = doc.Pages[0];

var mediaBox = page.MediaBox;
var streams = page.Contents.ToList();
```

Examples of low-level/raw access:

```csharp
using var doc = PdfDocument.Open("input.pdf");
var page = doc.Pages[0];

var resources = page.NativeObject.Get<PdfDictionary>(PdfName.Resources);
var rawContents = page.NativeObject.GetRequired<IPdfObject>(PdfName.Contents);
```

Use raw access when:

- the wrapper does not expose the feature you need
- you are traversing arbitrary PDF objects
- you need to inspect the exact underlying object shape

## Traversing Raw PDF Objects

`PdfDictionary` provides five preferred lookup shapes:

| Method | Missing | PDF null | Wrong type | Indirect reference |
| --- | --- | --- | --- | --- |
| `Get(key)` | `null` | `PdfNull` | n/a | resolved |
| `Get<T>(key)` | `null` | `null` | `null` | resolved |
| `GetOptional<T>(key)` | `null` | `null` | throws | resolved |
| `GetRequired<T>(key)` | throws | throws | throws | resolved |
| `TryGet<T>(key, out value)` | `false` | `false` | `false` | resolved |

The typed PDF-null behavior above applies when `T` is a concrete expected type. A request for `IPdfObject`
accepts and returns `PdfNull`, preserving the ability to inspect arbitrary resolved objects.

The indexer and `TryGetValue(key, out value)` expose the stored object without resolving indirect references.
The indexer follows `IDictionary` semantics and throws `KeyNotFoundException` for an absent key; use `Get(key)`
for a nullable resolved read.

Legacy `Get<T>`, `GetOptionalValue<T>`, `GetRequiredValue<T>`, and generic `TryGetValue<T>` remain supported but
are hidden from IntelliSense. In particular, legacy generic `TryGetValue<T>` defaults to throwing on a type
mismatch; new code should use `TryGet<T>` for conventional non-throwing behavior.

For object-level casting after you already have an `IPdfObject`, prefer:

- `obj.GetAs<T>()`: required typed access on the object itself
- `obj.GetAsOrNull<T>()`: optional typed access on the object itself

These are distinct from dictionary entry access and are preferred over the older `obj.GetValue<T>()` and `obj.GetValueOrNull<T>()` aliases.

```csharp
using var doc = PdfDocument.Open("input.pdf");
var page = doc.Pages[0];
var pageDict = page.NativeObject;

// optional typed access
PdfDictionary? resources = pageDict.Get<PdfDictionary>(PdfName.Resources);

// required typed access
PdfArray mediaBox = pageDict.GetRequired<PdfArray>(PdfName.MediaBox);
```

When the object type is not known up front, request the resolved `IPdfObject`:

```csharp
var val = page.NativeObject.GetRequired<IPdfObject>(PdfName.Contents);

switch (val)
{
    case PdfArray arr:
        foreach (var item in arr)
        {
            var stream = item.GetAs<PdfStream>();
            Console.WriteLine(stream.Dictionary.GetRequired<PdfNumber>(PdfName.Length));
        }
        break;
    case PdfStream single:
        Console.WriteLine(single.Dictionary.GetRequired<PdfNumber>(PdfName.Length));
        break;
    default:
        throw new ApplicationException("Invalid Contents value on page");
}
```

`IPdfObject.GetPdfObjType()` is useful when you want the direct object type without switching manually on indirect references first.

```csharp
var val = page.NativeObject.GetRequired<IPdfObject>(PdfName.Contents);

switch (val.GetPdfObjType())
{
    case PdfObjectType.ArrayObj:
        var arr = val.GetAs<PdfArray>();
        break;
    case PdfObjectType.StreamObj:
        var stream = val.GetAs<PdfStream>();
        break;
}
```

## Page Access

`doc.Pages` is a `List<PdfPage>`.

Common page properties and operations:

```csharp
var page = doc.Pages[0];

var mediaBox = page.MediaBox;
var resources = page.Resources;

using var writer = page.GetWriter();
```

For text/image/content scanning, PdfLexer provides scanner APIs on top of page access:

```csharp
using var doc = PdfDocument.Open("input.pdf");
var page = doc.Pages[0];

var textScanner = page.GetTextScanner();
var wordScanner = page.GetWordScanner();
```

## Modifying Documents

The `PdfDocument` class includes `Save()` and `SaveTo(...)` methods.

```csharp
using var doc = PdfDocument.Open(File.ReadAllBytes("input.pdf"));
doc.Trailer[PdfName.Info] = new PdfDictionary
{
    [PdfName.Title] = new PdfString("Updated")
};

using var fs = File.Create("output.pdf");
doc.SaveTo(fs);
```

Objects can also be copied between documents:

```csharp
using var doc = PdfDocument.Open(File.ReadAllBytes("input.pdf"));
using var doc2 = PdfDocument.Open(File.ReadAllBytes("input2.pdf"));

doc2.Trailer[PdfName.Info] = doc.Trailer[PdfName.Info];
using var fs = File.Create("output.pdf");
doc2.SaveTo(fs);
```

When modifying objects, remember that shared objects stay shared until you clone them. Use `CloneShallow()` when you want to fork a dictionary or array before editing.
