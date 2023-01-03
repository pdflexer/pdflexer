using PdfLexer.Serializers;
using System.Management.Automation;

namespace PdfLexer.Powershell;

[Cmdlet(
        "New",
        "PdfWriter"),
    OutputType(typeof(StreamingWriter))]

public class NewPdfWriter : Cmdlet
{
    [Parameter(
    Mandatory = true,
    ValueFromPipelineByPropertyName = true,
    HelpMessage = "Path to output pdf document")]
    [ValidateNotNullOrEmpty]
    public string FilePath { get; set; } = null!;

    [Parameter(
    Mandatory = false,
    HelpMessage = "If xobjects shoudl be deduplicated when writing.")]
    [ValidateNotNullOrEmpty]
    public bool DedupObjects { get; set; }

    protected override void BeginProcessing()
    {
        var so = File.Create(FilePath);

        var sw = new StreamingWriter(so, DedupObjects, true, true);
        WriteObject(sw);
    }
}
