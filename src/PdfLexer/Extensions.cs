using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer;

public static class Extensions
{
    /// <summary>
    /// Fully loads a PDF object so that it is safe to dispose of the source 
    /// pdf.
    /// </summary>
    /// <param name="obj"></param>
    public static IPdfObject FullyLoad(this IPdfObject obj)
    {
        CommonUtil.RecursiveLoad(obj);
        return obj;
    }
}
