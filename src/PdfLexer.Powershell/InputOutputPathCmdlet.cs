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
        HelpMessage = "Path to pdf to write.")]
    [ValidateNotNullOrEmpty]
    public string[]? InputPath { get; set; } = null!;

    [Parameter(
        ValueFromPipeline = true,
        HelpMessage = "Pdf page to write.")]
    [ValidateNotNullOrEmpty]
    public PdfPage[]? PdfPage { get; set; } = null!;

    [Parameter(
        ValueFromPipeline = true,
        ParameterSetName = "document",
        HelpMessage = "Pdf document to write")]
    [ValidateNotNullOrEmpty]
    public PdfDocument[]? Document { get; set; } = null!;


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
                if (IsFileSystemPath(provider, p))
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
            foreach (var pg in PdfPage)
            {
                yield return pg;
            }
        }
        else if (Document != null)
        {
            foreach (var doc in Document) {
                foreach (var pg in doc.Pages)
                {
                    yield return pg;
                }
            }

        }
    }
}
