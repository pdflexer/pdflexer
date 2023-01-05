using PdfLexer.DOM;
using System.Management.Automation;

namespace PdfLexer.Powershell;

[Cmdlet(
        "Get",
        "PdfPages"),
     OutputType(typeof(PdfPage))
   ]
public class GetPdfPages : PathCmdlet
{

    [Parameter(
        Mandatory = true,
        ValueFromPipeline = true,
        ParameterSetName = "document",
        HelpMessage = "Pdf document to write")]
    [ValidateNotNullOrEmpty]
    public PdfDocument? Document { get; set; } = null!;

    [Parameter(
       Mandatory = false,
        ValueFromPipelineByPropertyName = true,
        HelpMessage = "Pages numbers to load (1 indexed), if not provided all pages will be loaded.")]
    public int[]? Number { get; set; } = null!;

    protected override void ProcessRecord()
    {
        if (HasPaths())
        {
            foreach (var path in GetPaths())
            {
                using var pdf = PdfDocument.Open(path);
                if (Number != null)
                {
                    HandlePages(pdf, Number);
                }
                else
                {
                    foreach (var pg in pdf.Pages)
                    {
                        pg.NativeObject.FullyLoad();
                        WriteObject(pg);
                    }
                }

                foreach (var err in pdf.Context.ParsingErrors)
                {
                    WriteWarning(err);
                }
            }
        }
        else if (Document != null)
        {
            if (Number != null)
            {
                HandlePages(Document, Number);
            } else
            {
                foreach (var pg in Document.Pages)
                {
                    pg.NativeObject.FullyLoad();
                    WriteObject(pg);
                }
            }

            foreach (var err in Document.Context.ParsingErrors)
            {
                WriteWarning(err);
            }
        }

        void HandlePages(PdfDocument pdf, int[] pagNums)
        {
            foreach (var i in pagNums)
            {
                if (i < 0)
                {
                    var ip = Math.Abs(i);
                    if (ip > pdf.Pages.Count)
                    {
                        throw new Exception($"Index {i} was greater than total pages {pdf.Pages.Count}");
                    }
                    var pg = pdf.Pages[^ip];
                    pg.NativeObject.FullyLoad();
                    WriteObject(pg);
                }
                else
                {
                    if (i > pdf.Pages.Count)
                    {
                        throw new Exception($"Index {i} was greater than total pages {pdf.Pages.Count}");
                    }
                    var pg = pdf.Pages[i - 1];
                    pg.NativeObject.FullyLoad();
                    WriteObject(pg);
                }
            }
        }
    }
}
