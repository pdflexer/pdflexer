# Page Content

Note: Alpha quality

Higher level page content access methods have not been completed (eg. read text, read images, etc). The `PageContentScanner` allows access to the raw content stream operations. When the `flattenForms` argument is true the scanner will read operations recursively from any referenced forms as well, simplifying use cases where all page content is desired (note: the form calling eg. `/F1 Do` operation is not returned by the scanner when `flattenForms` is true).

```csharp

using var doc = PdfDocument.Open(data);
var scanner = new PageContentScanner(doc.Context, doc.Pages.First(), flattenForms:true);
while (scanner.Peek() != PdfOperatorType.EOC)
{
    var op = scanner.GetCurrentOperation();
    // handle operation
    scanner.SkipCurrent();
}
```

When using form flattening the `PageContentScanner.FormStack` can be viewed to determine current location of parsing. The `ReadStack` list will be empty when on the main page content. When in form(s) the `FormName` property will contain the name of the current form. If multiple forms are read recursiving the info can be viewed in the `ReadStack` (eg. Page->Form1->Form2 in ReadStack, Form3 in FormName). This will be simplified in the future.
