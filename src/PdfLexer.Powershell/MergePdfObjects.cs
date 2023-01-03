using PdfLexer.Content;
using PdfLexer.Serializers;
using System.Management.Automation;

namespace PdfLexer.Powershell;

[Cmdlet(
        "Merge",
        "PdfObjects")
    ]

public class MergePdfObjects : Cmdlet
{
    [Parameter(
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "Path to pdf document")]
    [ValidateNotNullOrEmpty]
    [Alias("FullName")]
    public string FilePath { get; set; } = null!;

    [Parameter(
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "Path to save deduplicated pdf document to")]
    [ValidateNotNullOrEmpty]
    public string OutputPath { get; set; } = null!;

    [Parameter(
        Mandatory = false,
        HelpMessage = "If inline images should be converted to xobjs to improve deduplication")]
    [ValidateNotNullOrEmpty]
    public bool InlineImages { get; set; }

    protected override void BeginProcessing()
    {
        using var doc = PdfDocument.Open(FilePath);
        using var fo = File.Create(OutputPath);
        using var writer = new StreamingWriter(fo, true, true);
        foreach (var pg in doc.Pages)
        {
            var pgr = pg;
            if (InlineImages)
            {
                pgr = ContentUtil.ConvertInlineImagesToXObjs(doc.Context, pgr);
            }
            writer.AddPage(pgr);
        }
        writer.Complete(doc.Trailer, doc.Catalog);
    }
}
