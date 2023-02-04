using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Powershell;

internal class PsHelpers
{
    public static PdfDocument OpenDocument(string path, ParsingOptions? options=null)
    {
        var ds = Environment.GetEnvironmentVariable("PDFLEXER_DISABLE_STREAMING");
        var fm = Environment.GetEnvironmentVariable("PDFLEXER_FORCE_IN_MEMORY");
        if (ds == null && fm == null)
        {
            return PdfDocument.Open(path, options);
        }
        else
        {
            return PdfDocument.Open(File.ReadAllBytes(path), options);
        }
    }
}
