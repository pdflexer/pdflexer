# Content Redaction

Note: Work in progress

```csharp
using var doc = PdfDocument.Open("filepath.pdf");

for (var i = 0; i < doc.Pages.Count; i++)
{
      var redactor = new Redactor(doc.Context, doc.Pages[i]);
      var pg = redact.RedactContent(c => c.Char == 'e');
      doc.Pages[i] = result;
}
doc.SaveTo("redacted.pdf");

```
