using PdfLexer.DOM;
using PdfLexer.Parsers;
using System.Management.Automation;

namespace PdfLexer.Powershell;

/// <summary>
/// Adapted from https://stackoverflow.com/questions/8505294/how-do-i-deal-with-paths-when-writing-a-powershell-cmdlet
/// </summary>
public class InputOutputPathCmdlet : OutputPathCmdlet
{
    [Parameter(
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Pdf page to write.")]
    public PdfPage? PdfPage { get; set; } = null!;

    [Parameter(
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "Pdf document to write")]
    public PdfDocument? Document { get; set; } = null!;

    // need to determine why PdfPages are binding to InputPath
    // [Parameter(
    // ValueFromPipeline = true,
    // ValueFromPipelineByPropertyName = false,
    // HelpMessage = "Path to pdf to write.")]
    public string[]? InputPath { get; set; } = null!;

    protected ParsingContext _ctx = null!;
    protected override void BeginProcessing()
    {
        _ctx = ParsingContext.Reset();
        base.BeginProcessing();
    }
    protected override void EndProcessing()
    {
        foreach (var err in _ctx.ParsingErrors)
        {
            WriteWarning(err);
        }
        base.EndProcessing();
    }

    protected bool HasInputPaths() => InputPath != null;

    protected bool HasInputPages() => PdfPage != null;

    protected bool HasInputDocument() => Document != null;

    protected IEnumerable<string> GetInputPaths()
    {
        if (InputPath == null ) { yield break; }
        var paths = new List<string>();
        foreach (var path in InputPath)
        {
            ProviderInfo provider;
            var r = this.GetResolvedProviderPathFromPSPath(path, out provider);
            foreach (var p in r)
            {
                if (IsFileSystemPath(this, provider, p))
                {
                    paths.Add(p);
                }
            }
        }
        foreach (var path in paths)
        {
            yield return path;
        }
    }
    protected IEnumerable<PdfPage> GetInputPages()
    {
        if (InputPath != null)
        {
            WriteVerbose("Using string based paths for input.");
            foreach (var path in GetInputPaths())
            {
                using var ctx = new ParsingContext(new ParsingOptions { ThrowOnErrors = false });
                using var doc = PsHelpers.OpenDocument(path);
                foreach (var pg in doc.Pages)
                {
                    yield return pg;
                }
                foreach (var err in ctx.ParsingErrors)
                {
                    WriteWarning(err);
                }
            }
        }
        else if (PdfPage != null)
        {
            WriteVerbose("Using PdfPages for input.");
            yield return PdfPage;
            // foreach (var pg in PdfPage)
            // {
            //     yield return pg;
            // }
        }
        else if (Document != null)
        {
            WriteVerbose("Using PdfDocuments for input.");
            foreach (var pg in Document.Pages)
            {
                yield return pg;
            }
            // foreach (var doc in Document) {
            // 
            // }

        }
    }
}
