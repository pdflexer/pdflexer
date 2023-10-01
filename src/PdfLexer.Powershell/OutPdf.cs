using PdfLexer.DOM;
using PdfLexer.Serializers;
using System.IO;
using System.Management.Automation;

namespace PdfLexer.Powershell;

[Cmdlet(
        "Out",
        "Pdf"),
    OutputType(typeof(string)
    ),
   ]
public class OutPdf : InputOutputPathCmdlet, IDisposable
{
    private FileStream? _fo;
    private StreamingWriter? _sw;
    private string? path;

    [Parameter(
        Mandatory = false,
        HelpMessage = "If xobjects should be deduplicated when writing.")]
    [ValidateNotNullOrEmpty]
    public SwitchParameter DedupObjects { get; set; }

    [Parameter(
        Mandatory = false,
        HelpMessage = "If pages should be appended to existing document.")]
    [ValidateNotNullOrEmpty]
    public SwitchParameter Append { get; set; }

    [Parameter(
    Mandatory = false,
    HelpMessage = "If documents passed should be closed / disposed after pages written.")]
    public SwitchParameter Close { get; set; }

    [Parameter(
    Mandatory = false,
    HelpMessage = "If file at output location should be replaced. Ignored if -Append used.")]
    public SwitchParameter Force { get; set; }

    protected override void BeginProcessing()
    {
        path = GetOutputPath();
        if (path == null)
        {
            return;
        }
        if (Append && File.Exists(path))
        {
            var tmp = path + ".tmp";
            File.Move(path, tmp);
            _fo = File.Create(path);
            _sw = new StreamingWriter(_fo, DedupObjects, true, true);
            {
                using var pv = PdfDocument.Open(tmp);
                foreach (var pg in pv.Pages)
                {
                    _sw.AddPage(pg);
                }
            }
            File.Delete(tmp);
        }
        else
        {
            if (File.Exists(path) && !Force)
            {
                throw new PdfLexerException($"File exists at {path} and -Force option not used");
            }
            _fo = File.Create(path);
            _sw = new StreamingWriter(_fo, DedupObjects, true, true);
        }
    }
    protected override void ProcessRecord()
    {
        if (_sw == null) { return; }
        foreach (var pg in GetInputPages())
        {
            _sw.AddPage(pg);
        }
        if (Document != null && Close)
        {
            foreach (var doc in Document) 
            {
                doc.Dispose();
            }
        }
        GC.Collect();
    }

    protected override void StopProcessing()
    {
        WriteVerbose("Stop processing triggered.");
    }

    protected override void EndProcessing()
    {
        _sw?.Complete(new PdfDictionary());
        _sw?.Dispose();
        _fo?.Dispose();
        _sw = null;
        _fo = null;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);

        WriteObject(path);
    }

    public void Dispose()
    {
        _sw?.Dispose();
        _fo?.Dispose();
    }
}
