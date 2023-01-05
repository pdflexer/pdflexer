using Microsoft.PowerShell.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PdfLexer.Powershell;

/// <summary>
/// Adapted from https://stackoverflow.com/questions/8505294/how-do-i-deal-with-paths-when-writing-a-powershell-cmdlet
/// </summary>
public class PathCmdlet : PSCmdlet
{
    private string[]? _paths;
    private bool _shouldExpandWildcards;
    [Parameter(
        Mandatory = true,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = true,
        ParameterSetName = "literal")
    ]
    [Alias("PSPath")]
    [ValidateNotNullOrEmpty]
    public string[]? LiteralPath
    {
        get { return _paths; }
        set { _paths = value; }
    }
    [Parameter(
        Position = 0,
        Mandatory = true,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        ParameterSetName = "path")
    ]
    [ValidateNotNullOrEmpty]
    public string[]? Path
    {
        get { return _paths; }
        set
        {
            _shouldExpandWildcards = true;
            _paths = value;
        }
    }

    public bool HasPaths() => _paths != null;

    public IEnumerable<string> GetPaths()
    {
        if (_paths == null) { yield break; }
        foreach (string path in _paths)
        {
            ProviderInfo provider;
            PSDriveInfo drive;
            List<string> filePaths = new List<string>();
            if (_shouldExpandWildcards)
            {
                // Turn *.txt into foo.txt,foo2.txt etc.
                // if path is just "foo.txt," it will return unchanged.
                filePaths.AddRange(this.GetResolvedProviderPathFromPSPath(path, out provider));
            }
            else
            {
                // no wildcards, so don't try to expand any * or ? symbols.                    
                filePaths.Add(this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(
                    path, out provider, out drive));
            }
            // ensure that this path (or set of paths after wildcard expansion)
            // is on the filesystem. A wildcard can never expand to span multiple
            // providers.
            if (IsFileSystemPath(provider, path) == false)
            {
                continue;
            }
            // at this point, we have a list of paths on the filesystem.
            foreach (string filePath in filePaths)
            {
                yield return filePath;
            }
        }
    }

    public string GetCorrectPath(string path)
    {
        return this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(
                    path, out var provider, out var drive);
    }
  
    private bool IsFileSystemPath(ProviderInfo provider, string path)
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
            this.WriteError(error);
            // tell our caller that the item was not on the filesystem
            isFileSystem = false;
        }
        return isFileSystem;
    }
}
