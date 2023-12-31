using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PdfLexer.Powershell;

public class PdfCmdlet : PSCmdlet
{
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
}
