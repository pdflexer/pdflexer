using System.Numerics;

namespace PdfLexer.Content.Model;

public static class ContentExtensions
{
    private static IEnumerable<IContentGroup<T>> Flatten<T>(List<IContentGroup<T>> content) where T : struct, IFloatingPoint<T>
    {
        foreach (var item in content)
        {
            if (item is MarkedContentGroup<T> group)
            {
                foreach (var child in Flatten(group.Children))
                {
                    yield return child;
                }
            }
            else
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Flattens the hierarchical content structure into a linear list of leaf nodes (Text, Path, Image, Form).
    /// Ignores the Marked Content structure.
    /// </summary>
    public static IEnumerable<IContentGroup<T>> Flatten<T>(this IEnumerable<IContentGroup<T>> content) where T : struct, IFloatingPoint<T>
    {
       foreach (var item in content)
       {
           if (item is MarkedContentGroup<T> group)
           {
               foreach (var child in Flatten(group.Children))
               {
                   yield return child;
               }
           }
           else
           {
               yield return item;
           }
       }
    }
}
