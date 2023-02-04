using PdfLexer.DOM;
using PdfLexer.Serializers;
using System.Management.Automation;

namespace PdfLexer.Powershell;

[Cmdlet(
        "Out",
        "Pdf"),
   ]
public class OutPdf : InputOutputPathCmdlet, IDisposable
{
    private FileStream? _fo;
    private StreamingWriter? _sw;

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
        var path = GetOutputPath();
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
            using var pv = PsHelpers.OpenDocument(tmp);
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
        if (_sw == null) { return; }
        foreach (var pg in GetInputPages())
        {
            _sw.AddPage(pg);
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
#if NET7_0_OR_GREATER
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
#else
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
#endif
    }

    public void Dispose()
    {
        _sw?.Dispose();
        _fo?.Dispose();
    }
}
