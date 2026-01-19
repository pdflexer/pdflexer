using System.Numerics;
using PdfLexer.Writing;

namespace PdfLexer.Content.Model;

public class MarkedContentGroup<T> : IContentGroup<T> where T : struct, IFloatingPoint<T>
{
    public MarkedContentGroup(MarkedContent tag)
    {
        Tag = tag;
        Children = new List<IContentGroup<T>>();
    }

    public MarkedContent Tag { get; }
    public List<IContentGroup<T>> Children { get; }
    
    // MarkedContentGroup itself doesn't have a specific GS, it uses the GS of its children/context.
    // However, the interface requires it. We might need to rethink this, 
    // but for now, we can perhaps return the GS of the first child or a default?
    // Actually, looking at the plan/analysis, we decided GS is baked into leaf nodes.
    // IContentGroup interface forces us to have a GraphicsState property. 
    // This is a bit awkward for a container. 
    // Let's look at IContentGroup definition again.
    
    // The current IContentGroup has GfxState property.
    // If IContentGroup is the base for EVERYTHING, then MarkedContentGroup must have it.
    // But a Group doesn't really have *a* state, it has children with states.
    // Ideally, we'd split IContentGroup into IContentItem and ILeafContentItem, but that's a bigger refactor.
    // For now, I will implement it but maybe throw or return root state? 
    // Actually, traditionally in this model, every object has a resolved state at its start. 
    // So the Group acts as a point in the stream. It effectively has the state *at the point BDC is called*.
    
    public required GfxState<T> GraphicsState { get; set; }

    public ContentType Type => ContentType.MarkedContent;

    public bool CompatibilitySection { get; set; }

    public void ClipExcept(PdfRect<T> rect)
    {
        foreach (var child in Children)
        {
            child.ClipExcept(rect);
        }
    }

    public void ClipFrom(GfxState<T> other)
    {
        foreach (var child in Children)
        {
            child.ClipFrom(other);
        }
    }

    public IContentGroup<T>? CopyArea(PdfRect<T> rect)
    {
        var newChildren = new List<IContentGroup<T>>();
        foreach (var child in Children)
        {
            var copied = child.CopyArea(rect);
            if (copied != null)
            {
                newChildren.Add(copied);
            }
        }
        
        if (newChildren.Count == 0) return null;

        var group = new MarkedContentGroup<T>(Tag)
        {
            GraphicsState = GraphicsState, // Copy state?
            CompatibilitySection = CompatibilitySection
        };
        group.Children.AddRange(newChildren);
        return group;
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


    public (IContentGroup<T>? Inside, IContentGroup<T>? Outside) Split(PdfRect<T> rect)
    {
         var inside = new List<IContentGroup<T>>();
         var outside = new List<IContentGroup<T>>();

         foreach (var child in Children)
         {
             var (i, o) = child.Split(rect);
             if (i != null) inside.Add(i);
             if (o != null) outside.Add(o);
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

    public void Transform(GfxMatrix<T> transformation)
    {
        foreach (var child in Children)
        {
            child.Transform(transformation);
        }
    }

    public void TransformInitial(GfxMatrix<T> transformation)
    {
        foreach (var child in Children)
        {
           child.TransformInitial(transformation);
        }
    }

    public void Write(ContentWriter<T> writer)
    {
        writer.MarkedContent(Tag);
        ContentModelWriter<T>.WriteGroups(writer, Children);
        writer.EndMarkedContent();
    }

}
