using PdfLexer.DOM;
using PdfLexer.Serializers;
using System.Management.Automation;

namespace PdfLexer.Powershell;

[Cmdlet(
        "Out",
        "Pdf"),
   ]
public class OutPdf : PathCmdlet, IDisposable
{
    private FileStream? _fo;
    private StreamingWriter? _sw;

    [Parameter(
        Mandatory = true,
        ValueFromPipeline = true,
        ParameterSetName = "page",
        HelpMessage = "Pdf page to write.")]
    [ValidateNotNullOrEmpty]
    public PdfPage? PdfPage { get; set; } = null!;

    [Parameter(
        Mandatory = true,
        ValueFromPipeline = true,
        ParameterSetName = "document",
        HelpMessage = "Pdf document to write")]
    [ValidateNotNullOrEmpty]
    public PdfDocument? Document { get; set; } = null!;

    [Parameter(
        Mandatory = true,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Path to output pdf document")]
    [ValidateNotNullOrEmpty]
    public string OutputPath { get; set; } = null!;

    [Parameter(
        Mandatory = false,
        HelpMessage = "If xobjects should be deduplicated when writing.")]
    [ValidateNotNullOrEmpty]
    public bool DedupObjects { get; set; }

    [Parameter(
        Mandatory = false,
        HelpMessage = "If pages should be appended to existing document.")]
    [ValidateNotNullOrEmpty]
    public bool Append { get; set; }

    protected override void BeginProcessing()
    {
        var path = GetCorrectPath(OutputPath);
        if (Append && File.Exists(path))
        {
            var tmp = path + ".tmp";
            File.Move(path, tmp);
            _fo = File.Create(path);
            _sw = new StreamingWriter(_fo, DedupObjects, true, true);
            using var pv = PdfDocument.Open(tmp);
            foreach (var pg in pv.Pages)
            {
                _sw.AddPage(pg);
            }
        }
        else
        {
            _fo = File.Create(path);
            _sw = new StreamingWriter(_fo, DedupObjects, true, true);
        }
    }
    protected override void ProcessRecord()
    {
        if (HasPaths())
        {
            foreach (var path in GetPaths())
            {
                using var pv = PdfDocument.Open(path);
                foreach (var pg in pv.Pages)
                {
                    _sw!.AddPage(pg);
                }
            }
        } else if (PdfPage != null)
        {
            _sw!.AddPage(PdfPage);
        }
        else if (Document != null)
        {
            foreach (var pg in Document.Pages)
            {
                _sw!.AddPage(pg);
            }
        }
        GC.Collect();
    }

    protected override void EndProcessing()
    {
        _sw?.Complete(new PdfDictionary());
        _sw?.Dispose();
        _fo?.Dispose();
        _sw = null;
        _fo = null;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
    }

    public void Dispose()
    {
        _sw?.Dispose();
        _fo?.Dispose();
    }
}
