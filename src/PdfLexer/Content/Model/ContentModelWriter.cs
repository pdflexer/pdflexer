using PdfLexer.Writing;
using System.Numerics;

namespace PdfLexer.Content.Model;

internal class ContentModelWriter<T> where T : struct, IFloatingPoint<T>
{
    public static PdfStreamContents CreateContent(PdfDictionary resources, List<IContentNode<T>> groups, PdfDictionary? catalog=null)
    {
        var writer = new ContentWriter<T>(resources);
        writer.Save();
        WriteGroups(writer, groups);

        if (catalog != null)
        {
            HandleOCGs(catalog, writer);
        }

        // writer will restore if needed
        return writer.Complete();
    }

    public static void WriteContent(ContentWriter<T> writer, List<IContentNode<T>> groups, PdfDictionary? catalog = null)
    {
        var orig = writer.GraphicsStackSize;
        writer.Save();
        WriteGroups(writer, groups);
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

    internal static void WriteGroups(ContentWriter<T> writer, List<IContentNode<T>> groups)
    {
        foreach (var group in groups)
        {
            if (group is IContentItem<T> item)
            {
                writer.ReconcileCompatibility(item.CompatibilitySection);
                writer.SetGS(item.GraphicsState);
            }

            if (group is IContentWritable<T> writable)
            {
                writable.Write(writer);
                continue;
            }

            throw new InvalidOperationException($"Content node of type {group.GetType().Name} cannot be written because it does not implement {nameof(IContentWritable<T>)}.");
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
