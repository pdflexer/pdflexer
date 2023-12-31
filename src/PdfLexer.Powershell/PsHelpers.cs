using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace PdfLexer.Powershell;

internal class PsHelpers
{
    public static PdfDocument OpenDocument(string path, DocumentOptions? options=null)
    {
        var fm = Environment.GetEnvironmentVariable("PDFLEXER_FORCE_IN_MEMORY");
        if (fm != null)
        {
            var doc = PdfDocument.Open(File.ReadAllBytes(path), options);
            Documents.Add(new WeakReference<PdfDocument>(doc));
            return doc;
        }
        else
        {
            var doc = PdfDocument.Open(path, options);
            Documents.Add(new WeakReference<PdfDocument>(doc));
            return doc;
        }
    }

    public static PdfDocument OpenDocument(byte[] data, DocumentOptions? options = null)
    {
        var doc = PdfDocument.Open(data, options);
        Documents.Add(new WeakReference<PdfDocument>(doc));
        return doc;
    }
    private static List<WeakReference<PdfDocument>> Documents = new List<WeakReference<PdfDocument>>();

    public static void CloseDocuments()
    {
        var docs = Documents;
        Documents = new List<WeakReference<PdfDocument>>();
        foreach (var doc in docs)
        {
            if (doc.TryGetTarget(out var pdf))
            {
                pdf.Dispose();
            }
        }
    }
}
