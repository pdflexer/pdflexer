using PdfLexer.Content;
using PdfLexer.Parsers;
using PdfLexer.Serializers;
using System.Management.Automation;

namespace PdfLexer.Powershell;

[Cmdlet(
        "Merge",
        "PdfResources")
    ]

internal class MergePdfResources : PdfCmdlet
{
    [Parameter(
        Mandatory = true,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = false,
        ParameterSetName = "no-output")
    ]
    public string[]? Path { get; set; }

    [Parameter(
        Mandatory = false,
        HelpMessage = "If inline images should be converted to xobjs to improve deduplication")]
    [ValidateNotNullOrEmpty]
    public SwitchParameter InlineImages { get; set; }

    protected override void ProcessRecord()
    {
        if (Path != null)
        {
            foreach (var path in Path)
            {
                var resolved = OutputPathCmdlet.GetOutputPathFromString(this, path, false);
                if (resolved == null)
                {
                    throw new ApplicationException($"{path} was not a valid path.");
                }

                var outputPath = resolved + ".tmp";
                {
                    using var doc = PsHelpers.OpenDocument(resolved);
                    using var fo = File.Create(outputPath);
                    using var sw = new StreamingWriter(fo);
                    if (InlineImages)
                    {
                        foreach (var page in doc.Pages)
                        {
                            ContentUtil.ConvertInlineImagesToXObjs(doc.Context, page);
                        }
                    }
                    doc.DeduplicateResources();
                    foreach (var page in doc.Pages)
                    {
                        sw.AddPage(page);
                    }
                    sw.Complete(doc.Trailer);
                }
                File.Delete(resolved);
                File.Move(outputPath, resolved);
            }
        }
    }
}
