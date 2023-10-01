using Microsoft.PowerShell.Commands;
using System.Management.Automation;

namespace PdfLexer.Powershell;

/// <summary>
/// Adapted from https://stackoverflow.com/questions/8505294/how-do-i-deal-with-paths-when-writing-a-powershell-cmdlet
/// </summary>
public class OutputPathCmdlet : PSCmdlet
{
    private string? _path;
    private bool _shouldExpandWildcards;
    [Parameter(
        Mandatory = true,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        ParameterSetName = "output-literal")
    ]
    [Alias("PSPath")]
    [ValidateNotNullOrEmpty]
    public string? LiteralPath
    {
        get { return _path; }
        set { _path = value; }
    }
    [Parameter(
        Position = 0,
        Mandatory = true,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false,
        ParameterSetName = "output-path")
    ]
    [ValidateNotNullOrEmpty]
    public string? Path
    {
        get { return _path; }
        set
        {
            _shouldExpandWildcards = true;
            _path = value;
        }
    }

    public bool HasPaths() => _path != null;

    internal static string? GetOutputPathFromString(PSCmdlet cmd, string path)
    {
        ProviderInfo provider;
        PSDriveInfo drive;
        List<string> filePaths = new List<string>();
        if (false)
        {
            // Turn *.txt into foo.txt,foo2.txt etc.
            // if path is just "foo.txt," it will return unchanged.
            filePaths.AddRange(cmd.GetResolvedProviderPathFromPSPath(path, out provider));
        }
        else
        {
            // no wildcards, so don't try to expand any * or ? symbols.                    
            filePaths.Add(cmd.SessionState.Path.GetUnresolvedProviderPathFromPSPath(
                path, out provider, out drive));
        }
        // ensure that this path (or set of paths after wildcard expansion)
        // is on the filesystem. A wildcard can never expand to span multiple
        // providers.
        if (IsFileSystemPath(cmd, provider, path) == false)
        {
            return null;
        }

        foreach (var p in filePaths)
        {
            cmd.WriteVerbose("Resolved output path: " + p);
        }

        if (filePaths.Count > 1)
        {
            // create a .NET exception wrapping our error text
            var ex = new ArgumentException(path +
                " did not resolve to single location.");
            cmd.WriteError(new ErrorRecord(ex, "InvalidPath", ErrorCategory.InvalidArgument, path));
        }
        return filePaths[0];
    }
    public string? GetOutputPath()
    {
        if (_path == null) { return null; }
        return GetOutputPathFromString(this, _path);
    }

    public string GetCorrectPath(string path)
    {
        return this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(
                    path, out var provider, out var drive);
    }
  
    protected static bool IsFileSystemPath(PSCmdlet cmd, ProviderInfo provider, string path)
    {
        bool isFileSystem = true;
        // check that this provider is the filesystem
        if (provider.ImplementingType != typeof(FileSystemProvider))
        {
            // create a .NET exception wrapping our error text
            ArgumentException ex = new ArgumentException(path +
                " does not resolve to a path on the FileSystem provider.");
            // wrap this in a powershell errorrecord
            ErrorRecord error = new ErrorRecord(ex, "InvalidProvider",
                ErrorCategory.InvalidArgument, path);
            // write a non-terminating error to pipeline
            cmd.WriteError(error);
            // tell our caller that the item was not on the filesystem
            isFileSystem = false;
        }
        return isFileSystem;
    }
}
