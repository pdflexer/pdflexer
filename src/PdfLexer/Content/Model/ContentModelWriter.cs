using PdfLexer.Writing;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Xml.Schema;
using static DotNext.Threading.Tasks.DynamicTaskAwaitable;

namespace PdfLexer.Content.Model;

internal class ContentModelWriter<T> where T : struct, IFloatingPoint<T>
{
    public static PdfStreamContents CreateContent(PdfDictionary resources, List<IContentGroup<T>> groups, PdfDictionary? catalog=null)
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

        if (catalog != null)
        {
            HandleOCGs(catalog, writer);
        }

        // writer will restore if needed
        return writer.Complete();
    }

    public static void WriteContent(ContentWriter<T> writer, List<IContentGroup<T>> groups, PdfDictionary? catalog = null)
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
        writer.ReconcileCompatibility(false);
        while (writer.GraphicsStackSize > orig)
        {
            writer.Restore();
        }


        if (catalog != null)
        {
            HandleOCGs(catalog, writer);
        }

    }

    private static void HandleOCGs(PdfDictionary catalog, ContentWriter<T> writer)
    {
        if (writer.OCDefaults.Count > 0)
        {
            var ocp = catalog.GetOrCreateValue<PdfDictionary>("OCProperties");
            var ocgs = ocp.GetOrCreateValue<PdfArray>("OCGs");
            var ocd = ocp.GetOrCreateValue<PdfDictionary>(PdfName.D);
            var on = ocd.GetOrCreateValue<PdfArray>(PdfName.ON);
            var off = ocd.GetOrCreateValue<PdfArray>(PdfName.OFF);
            foreach (var (k, v) in writer.OCDefaults)
            {
                if (v)
                {
                    if (!on.Contains(k))
                    {
                        on.Add(k);
                    }
                }
                else
                {
                    if (!off.Contains(k))
                    {
                        off.Add(k);
                    }
                }
                ocgs.Add(k);
            }
        }
    }
}
