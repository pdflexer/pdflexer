using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Serializers;
using PdfLexer.Writing;
using System.IO;
using System.Management.Automation;

namespace PdfLexer.Powershell;

[Cmdlet(
        "Invoke",
        "PdfRead"),
    OutputType(typeof(PdfPage))]

public class InvokePdfRead : PdfCmdlet
{
    [Parameter(
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = false,
         ParameterSetName = "pages",
     HelpMessage = "Pdf page to write.")]
    public PdfPage[]? Page { get; set; } = null!;

    [Parameter(
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = false,
        ParameterSetName = "documents",
        HelpMessage = "Pdf document to write")]
    public PdfDocument[]? Document { get; set; } = null!;

    [Parameter(
    Mandatory = false,
    HelpMessage = "Width of pdf page in points.")]
    public double Width { get; set; }

    [Parameter(
    Mandatory = false,
    HelpMessage = "Height of pdf page in points.")]
    public double Height { get; set; }

    protected override void ProcessRecord()
    {
       
        // WriteObject(pg);
    }

    protected IEnumerable<PdfPage> GetInputPages()
    {
        if (Page != null)
        {
            foreach (var page in Page)
            {
                yield return page;
            }
        }
        if (Document != null)
        {
            foreach (var doc in Document)
            {
                foreach (var page in doc.Pages)
                {
                    yield return page;
                }
            }
        }
    }
}

