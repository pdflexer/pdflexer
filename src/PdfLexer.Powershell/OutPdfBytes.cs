using PdfLexer.DOM;
using System.Management.Automation;

namespace PdfLexer.Powershell;

[Cmdlet(
        "Out",
        "PdfBytes"    
    ),
    OutputType(typeof(byte[]))
   ]
public class OutPdfBytes : Cmdlet, IDisposable
{
    private PdfDocument? _doc = PdfDocument.Create();

    [Parameter(
        Mandatory = false,
        HelpMessage = "If xobjects should be deduplicated when writing.")]
    [ValidateNotNullOrEmpty]
    public SwitchParameter DedupObjects { get; set; }

    [Parameter(
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Pdf page to write.")]
    public PdfPage[]? PdfPage { get; set; } = null!;

    [Parameter(
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Pdf document to write")]
    public PdfDocument[]? Document { get; set; } = null!;


    protected override void BeginProcessing()
    {
        base.BeginProcessing();
    }
    protected override void ProcessRecord()
    {
        foreach (var pg in GetInputPages())
        {
            if (_doc == null) { _doc = PdfDocument.Create(); }
            _doc.Pages.Add(pg);
        }
        base.ProcessRecord();
    }

    protected override void StopProcessing()
    {
        WriteVerbose("Stop processing triggered.");
        base.StopProcessing();
    }

    protected override void EndProcessing()
    {
        if (_doc != null)
        {
            var bytes = _doc.Save();
            _doc.Dispose();
            _doc = null;
            WriteObject(bytes, false);
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
        }
        base.EndProcessing();
    }

    public void Dispose()
    {
    }

    protected IEnumerable<PdfPage> GetInputPages()
    {
        if (PdfPage != null)
        {
            WriteVerbose("Using PdfPages for input.");
            foreach (var pg in PdfPage)
            {
                yield return pg;
            }
        }
        else if (Document != null)
        {
            WriteVerbose("Using PdfDocuments for input.");
            foreach (var doc in Document)
            {
                foreach (var pg in doc.Pages)
                {
                    yield return pg;
                }
            }
        }
    }
}
