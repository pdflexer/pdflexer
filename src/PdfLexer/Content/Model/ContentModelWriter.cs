using PdfLexer.Writing;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Xml.Schema;

namespace PdfLexer.Content.Model;

internal class ContentModelWriter<T> where T : struct, IFloatingPoint<T>
{
    public static PdfStreamContents CreateContent(PdfDictionary resources, List<IContentGroup<T>> groups)
    {
        var writer = new ContentWriter<T>(resources);
        writer.Save();
        foreach (var group in groups)
        {
            writer.ReconcileCompatibility(group.CompatibilitySection);
            writer.ReconcileMC(group.Markings);
            writer.SetGS(group.GraphicsState);
            group.Write(writer);
        }

        // writer will restore if needed
        return writer.Complete();
    }

    public static void WriteContent(ContentWriter<T> writer, List<IContentGroup<T>> groups)
    {
        var orig = writer.GraphicsStackSize;
        writer.Save();
        foreach (var group in groups)
        {
            writer.ReconcileCompatibility(group.CompatibilitySection);
            writer.ReconcileMC(group.Markings);
            writer.SetGS(group.GraphicsState);
            group.Write(writer);
        }
        while (writer.GraphicsStackSize > orig)
        {
            writer.Restore();
        }
    }
}
