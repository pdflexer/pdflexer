using PdfLexer.Serializers;
using System.Management.Automation;

namespace PdfLexer.Powershell;

[Cmdlet(
        "New",
        "PdfWriter"),
    OutputType(typeof(StreamingWriter))]

public class NewPdfWriter : PathCmdlet
{

    [Parameter(
    Mandatory = false,
    HelpMessage = "If xobjects should be deduplicated when writing.")]
    [ValidateNotNullOrEmpty]
    public SwitchParameter DedupObjects { get; set; }

    protected override void ProcessRecord()
    {
        foreach (var path in GetPaths())
        {
            var so = File.Create(path);

            var sw = new StreamingWriter(so, DedupObjects, true, true);
            WriteObject(sw);
        }
    }
}
