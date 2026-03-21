using PdfLexer.Content;
using PdfLexer.Content.Model;
using System;
using System.Collections.Generic;
using System.Numerics;
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

    public static List<IContentGroup<T>> Shift<T>(this List<IContentGroup<T>> content, T dx, T dy) where T : struct, IFloatingPoint<T>
    {
        Shift(content.Cast<IContentNode<T>>().ToList(), dx, dy);
        return content;
    }

    public static List<IContentNode<T>> Shift<T>(this List<IContentNode<T>> content, T dx, T dy) where T : struct, IFloatingPoint<T>
    {
        var mtx = GfxMatrix<T>.Identity with { E = dx, F = dy };
        foreach (var item in content.Flatten())
        {
            if (item is IContentItem<T> leaf)
            {
                leaf.TransformInitial(mtx);
            }
        }
        return content;
    }
}
