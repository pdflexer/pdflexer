using PdfLexer.Writing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Schema;

namespace PdfLexer.Content.Model;

internal class ContentModelWriter
{
    public static PdfStreamContents CreateContent(PdfDictionary resources, List<IContentGroup> groups)
    {
        var writer = new ContentWriter(resources);
        IContentGroup? prev = null;
        foreach (var group in groups)
        {
            if (prev != null)
            {
                if (prev.CompatibilitySection != group.CompatibilitySection)
                {
                    if (group.CompatibilitySection)
                    {
                        writer.Op(BX_Op.Value);
                    }
                    else
                    {
                        writer.Op(EX_Op.Value);
                    }
                }
                if (prev.Markings != group.Markings)
                {
                    if (prev.Markings == null)
                    {
                        for (var i = 0; i < group.Markings.Count; i++)
                        {
                            writer.MarkedContent(group.Markings[i]);
                        }

                    }
                    else if (group.Markings == null)
                    {
                        for (var i = 0; i < prev.Markings.Count; i++)
                        {
                            writer.EndMarkedContent();
                        }
                    }
                    else
                    {
                        var min = Math.Min(prev.Markings.Count, group.Markings.Count);
                        var i = 0;
                        for (; i < min; i++)
                        {
                            if (prev.Markings[i] == group.Markings[i])
                            {
                                continue;
                            }
                        }
                        if (i < prev.Markings.Count)
                        {
                            for (var x = i; x < prev.Markings.Count; x++)
                            {
                                writer.EndMarkedContent();
                            }
                        }
                        if (i < group.Markings.Count)
                        {
                            for (var x = i; x < group.Markings.Count; x++)
                            {
                                writer.MarkedContent(group.Markings[i]);
                            }
                        }
                    }
                }
            } else
            {
                if (group.CompatibilitySection)
                {
                    writer.Op(BX_Op.Value);
                }
                if (group.Markings != null)
                {
                    for (var i = 0; i < group.Markings.Count; i++)
                    {
                        writer.MarkedContent(group.Markings[i]);
                    }
                }
            }
            group.Write(writer);
            prev = group;
        }
        if (prev != null)
        {
            if (prev.CompatibilitySection)
            {
                writer.Op(EX_Op.Value);
            }
            if (prev.Markings != null)
            {
                for (var i = 0; i < prev.Markings.Count; i++)
                {
                    writer.EndMarkedContent();
                }
            }
        }
        return writer.Complete();
    }
}
