using System.Numerics;

namespace PdfLexer.Content.Model;

public static class ContentExtensions
{
    /// <summary>
    /// Flattens the hierarchical content structure into a linear list of leaf nodes (Text, Path, Image, Form).
    /// Ignores the Marked Content structure and other containers.
    /// </summary>
    public static IEnumerable<IContentNode<T>> Flatten<T>(this IEnumerable<IContentNode<T>> content) where T : struct, IFloatingPoint<T>
    {
       foreach (var item in content)
       {
           if (item is IContentContainer<T> group)
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
