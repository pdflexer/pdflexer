using System.Numerics;
using PdfLexer.Writing;

namespace PdfLexer.Content.Model;

public class MarkedContentGroup<T> : IContentGroup<T>, IContentContainer<T> where T : struct, IFloatingPoint<T>
{
    public MarkedContentGroup(MarkedContent tag)
    {
        Tag = tag;
        Children = new List<IContentNode<T>>();
    }

    public MarkedContent Tag { get; }
    public List<IContentNode<T>> Children { get; }
    IEnumerable<IContentNode<T>> IContentContainer<T>.Children => Children;
    public ParsedContentId? ParsedItemId => ContentIdentityHelpers.Merge(Children);
    public StructuredSourceRef? SourceReference => ContentIdentityHelpers.MergeSourceRefs(Children);
    public required GfxState<T> GraphicsState { get; set; }
    public ContentType Type => ContentType.MarkedContent;
    public bool CompatibilitySection { get; set; }

    public void Write(ContentWriter<T> writer)
    {
        writer.MarkedContent(Tag);
        ContentModelWriter<T>.WriteGroups(writer, Children);
        writer.EndMarkedContent();
    }

    public PdfRect<T> GetBoundingBox()
    {
        if (Children.Count == 0) return new PdfRect<T> { LLx = T.Zero, LLy = T.Zero, URx = T.Zero, URy = T.Zero };
        
        var rect = Children[0].GetBoundingBox();
        for (int i = 1; i < Children.Count; i++)
        {
            var childRect = Children[i].GetBoundingBox();
            rect = new PdfRect<T>
            {
               LLx = T.Min(rect.LLx, childRect.LLx),
               LLy = T.Min(rect.LLy, childRect.LLy),
               URx = T.Max(rect.URx, childRect.URx),
               URy = T.Max(rect.URy, childRect.URy)
            };
        }
        return rect;
    }

    // Geometry operations for internal use or migration
    public IContentNode<T>? CopyArea(PdfRect<T> rect)
    {
        var newChildren = new List<IContentNode<T>>();
        foreach (var child in Children)
        {
            if (child is IContentItem<T> item)
            {
                var copied = item.CopyArea(rect);
                if (copied != null)
                {
                    newChildren.Add(copied);
                }
            }
            else if (child is MarkedContentGroup<T> mcg)
            {
                var copied = mcg.CopyArea(rect);
                if (copied != null)
                {
                    newChildren.Add(copied);
                }
            }
        }
        
        if (newChildren.Count == 0) return null;

        var group = new MarkedContentGroup<T>(Tag)
        {
            GraphicsState = GraphicsState,
            CompatibilitySection = CompatibilitySection
        };
        group.Children.AddRange(newChildren);
        return group;
    }

    public (IContentNode<T>? Inside, IContentNode<T>? Outside) Split(PdfRect<T> rect)
    {
         var inside = new List<IContentNode<T>>();
         var outside = new List<IContentNode<T>>();

         foreach (var child in Children)
         {
             if (child is IContentItem<T> item)
             {
                 var (i, o) = item.Split(rect);
                 if (i != null) inside.Add(i);
                 if (o != null) outside.Add(o);
             }
             else if (child is MarkedContentGroup<T> mcg)
             {
                 var (i, o) = mcg.Split(rect);
                 if (i != null) inside.Add(i);
                 if (o != null) outside.Add(o);
             }
         }

         MarkedContentGroup<T>? inGroup = null;
         if (inside.Count > 0)
         {
             inGroup = new MarkedContentGroup<T>(Tag)
             {
                 GraphicsState = GraphicsState,
                 CompatibilitySection = CompatibilitySection
             };
             inGroup.Children.AddRange(inside);
         }

         MarkedContentGroup<T>? outGroup = null;
         if (outside.Count > 0)
         {
             outGroup = new MarkedContentGroup<T>(Tag)
             {
                 GraphicsState = GraphicsState,
                 CompatibilitySection = CompatibilitySection
             };
             outGroup.Children.AddRange(outside);
         }

         return (inGroup, outGroup);
    }

    public MarkedContentGroup<T> Wrap(IEnumerable<IContentItem<T>> leaves, MarkedContent tag)
    {
        return Children.Wrap(leaves, tag);
    }

    public void Transform(GfxMatrix<T> transformation)
    {
        GraphicsState = GraphicsState with
        {
            CTM = transformation * GraphicsState.CTM
        };
        foreach (var child in Children)
        {
            if (child is IContentItem<T> item)
            {
                item.Transform(transformation);
            }
        }
    }

    public void TransformInitial(GfxMatrix<T> transformation)
    {
        GraphicsState = GraphicsState with
        {
            CTM = GraphicsState.CTM * transformation
        };
        foreach (var child in Children)
        {
            if (child is IContentItem<T> item)
            {
                item.TransformInitial(transformation);
            }
        }
    }

    public void ClipExcept(PdfRect<T> rect)
    {
        foreach (var child in Children)
        {
            if (child is IContentItem<T> item)
            {
                item.ClipExcept(rect);
            }
        }
    }

    public void ClipFrom(GfxState<T> other)
    {
        foreach (var child in Children)
        {
            if (child is IContentItem<T> item)
            {
                item.ClipFrom(other);
            }
        }
    }
}
