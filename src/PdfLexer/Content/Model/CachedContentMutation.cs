﻿using Microsoft.VisualBasic;
using PdfLexer.DOM;
using PdfLexer.Writing;
using System;
using System.Numerics;
using System.Resources;
using System.Runtime.CompilerServices;

namespace PdfLexer.Content.Model;

public class CachedContentMutation : CachedContentMutation<double>
{
    public CachedContentMutation(Func<IContentGroup<double>, IEnumerable<IContentGroup<double>>> mutation) : base(mutation)
    {

    }

    public CachedContentMutation(Func<IContentGroup<double>, IContentGroup<double>> mutation) : base(mutation) { }
}

/// <summary>
/// Helper class for recursively applying mutations to content groups across nested forms.
/// 
/// Includes internal caching to increase performance in cases where form are re-used 
/// across multiple pages.
/// </summary>
/// <typeparam name="T"></typeparam>
public class CachedContentMutation<T> where T : struct, IFloatingPoint<T>
{
    private Func<IContentGroup<T>, IEnumerable<IContentGroup<T>>> _mutation;
    private ConditionalWeakTable<PdfStream, Dictionary<GfxMatrix<T>, XObjForm?>> _cache =
        new ConditionalWeakTable<PdfStream, Dictionary<GfxMatrix<T>, XObjForm?>>();

    /// <summary>
    /// Create mutation helper with func that applies mutation to content group returning enumerable result.
    /// </summary>
    /// <param name="mutation"></param>
    public CachedContentMutation(Func<IContentGroup<T>, IEnumerable<IContentGroup<T>>> mutation)
    {
        _mutation = mutation;
    }

    /// <summary>
    /// Create mutation helper with func that applies mutation to content group returning single (or none/null) content group result.
    /// </summary>
    /// <param name="mutation"></param>
    public CachedContentMutation(Func<IContentGroup<T>, IContentGroup<T>?> mutation)
    {
        _mutation = Wrap(mutation);
    }

    /// <summary>
    /// Applies the mutation to a list of content groups.
    /// 
    /// If any content groups are forms these are parsed into content groups themselves and
    /// mutation applied recursively.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public List<IContentGroup<T>> Apply(List<IContentGroup<T>> content)
    {
        var newContents = new List<IContentGroup<T>>();
        foreach (var item in content)
        {
            if (item is FormContent<T> nested)
            {
                var frm = RecursivelyApply(nested);
                if (frm != null)
                {
                    newContents.Add(frm);
                }
                
            }
            else
            {
                newContents.AddRange(_mutation(item).Where(x=> x != null));
            }

        }
        return newContents;
    }

    /// <summary>
    /// Applies the mutation to all content of a page including nested forms.
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public PdfPage Apply(PdfPage page)
    {
        var model = page.GetContentModel<T>(false);
        var mutated = Apply(model);

        PdfPage newPage = page.NativeObject.CloneShallow();
        var res = new PdfDictionary();
        newPage.Resources = res;
        using var writer = newPage.GetWriter<T>(PageWriteMode.Replace);
        writer.AddContent(mutated);
        return newPage;
    }

    private FormContent<T>? RecursivelyApply(FormContent<T> form)
    {
        if (_cache.TryGetValue(form.Stream, out var result) && result.TryGetValue(form.GraphicsState.CTM, out var match) && false)
        {
            if (match == null) { return null; }
            return new FormContent<T>
            {
                GraphicsState = form.GraphicsState,
                Stream = match.NativeObject
            };
        }

        var parser = new ContentModelParser<T>(ParsingContext.Current, form.ParentPage ?? new PdfDictionary(), form.Stream, new GfxState<T> { CTM = form.GraphicsState.CTM });
        var content = parser.Parse();

        // clip items to form bbox to match existing clipping from form bbox
        // -> we need to adjust bbox for moved items so can't use that
        var bbox = form.Stream.Dictionary.Get<PdfArray>(PdfName.BBox);
        if (bbox == null) { bbox = PageSizeHelpers.GetMediaBox(PageSize.LETTER); }
        var formBox = ((PdfRectangle)bbox).ToContentModel<T>();
        formBox = form.GraphicsState.CTM.GetTransformedBoundingBox(formBox);
        foreach (var item in content)
        {
            if (!(item is FormContent<T>))
            {
                item.ClipFrom(form.GraphicsState);
                item.ClipExcept(formBox);
            }
        }

        var newContents = Apply(content);

        if (newContents.Count == 0)
        {
            AddToCache(null);
            return null;
        }

        // these were in a form, need to "undo" the applied CTM
        if (!form.GraphicsState.CTM.IsIdentity)
        {
            if (!form.GraphicsState.CTM.Invert(out var inv))
            {
                throw new PdfLexerException("Unable to invert matrix for form.");
            }

            foreach (var item in newContents)
            {
                item.TransformInitial(inv);
            }
        }

        PdfRect<T>? bb = default;

        foreach (var item in newContents)
        {
            var cbb = item.GetBoundingBox();
            if (bb == null)
            {
                bb = cbb;
            }
            else
            {
                bb = new PdfRect<T>
                {
                    LLx = T.Min(bb.LLx, cbb.LLx),
                    URx = T.Max(bb.URx, cbb.URx),
                    LLy = T.Min(bb.LLy, cbb.LLy),
                    URy = T.Max(bb.URy, cbb.URy),
                };
            }
        }

        var xObj = new XObjForm();
        var resources = new PdfDictionary();

        // TODO revisit this, do we need to expand?
        xObj.BBox = PdfRectangle.FromContentModel(bb! with {
            LLx = bb.LLx - FPC<T>.V100, 
            LLy = bb.LLy - FPC<T>.V100, 
            URx = bb.URx + FPC<T>.V100, 
            URy = bb.URy + FPC<T>.V100
        });
        xObj.NativeObject.Dictionary[PdfName.Resources] = resources;
        var writer = new ContentWriter<T>(resources);
        writer.AddContent(newContents);
        xObj.NativeObject.Contents = writer.Complete();

        AddToCache(xObj);

        return new FormContent<T>
        {
            GraphicsState = form.GraphicsState with { Clipping = null },
            Stream = xObj.NativeObject
        };

        void AddToCache(XObjForm? value)
        {
            if (result == null)
            {
                result = new Dictionary<GfxMatrix<T>, XObjForm?>();
                result[form.GraphicsState.CTM] = value;
                _cache.Add(form.Stream, result);
            }
            else
            {
                result[form.GraphicsState.CTM] = value;
            }
        }
    }

    private static Func<IContentGroup<T>, IEnumerable<IContentGroup<T>>> Wrap(Func<IContentGroup<T>, IContentGroup<T>?> single)
    {
        IEnumerable<IContentGroup<T>> Wrapped(IContentGroup<T> input)
        {
            var r = single(input);
            if (r != null)
            {
                yield return r;
            }
        }
        return Wrapped;
    }
}
