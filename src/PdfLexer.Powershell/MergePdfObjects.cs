using PdfLexer.Content;
using PdfLexer.Serializers;
using System.Management.Automation;

namespace PdfLexer.Powershell;

[Cmdlet(
        "Merge",
        "PdfObjects")
    ]

internal class MergePdfObjects : InputOutputPathCmdlet
{
    [Parameter(
        Mandatory = false,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        ParameterSetName = "no-output")
    ]
    public string? __ { get; set; }

    [Parameter(
        Mandatory = false,
        HelpMessage = "If inline images should be converted to xobjs to improve deduplication")]
    [ValidateNotNullOrEmpty]
    public bool InlineImages { get; set; }

    protected override void ProcessRecord()
    {
        throw new NotImplementedException("TODO rework this");
        if (HasInputPaths())
        {
            foreach (var path in GetInputPaths())
            {
                var filePath = path;
                var output = filePath;
                var bak = filePath + ".tmp";
                File.Move(filePath, bak);
                filePath = bak;
                using var doc = PdfDocument.Open(filePath, new ParsingOptions { ThrowOnErrors = false });
                using var fo = File.Create(output);
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
                foreach (var err in doc.Context.ParsingErrors)
                {
                    WriteWarning(err);
                }
                try
                {
                    File.Delete(bak);
                }
                catch (Exception ex)
                {
                    WriteWarning("Failed to delete tmp file:" + ex.Message);
                }
            }
        } else if (PdfPage != null)
        {
            foreach (var pg in PdfPage)
            {
                
            }
        }
    }
}
