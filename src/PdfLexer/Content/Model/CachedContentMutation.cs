using PdfLexer.DOM;
using PdfLexer.Writing;
using System.Numerics;
using System.Runtime.CompilerServices;
using PdfLexer.Fonts;

namespace PdfLexer.Content.Model;

public class CachedContentMutation : CachedContentMutation<double>
{
    public CachedContentMutation(Func<IContentGroup<double>, IEnumerable<IContentGroup<double>>> mutation) : base(mutation)
    {
    }

    public CachedContentMutation(Func<IContentNode<double>, IEnumerable<IContentNode<double>>> mutation) : base(mutation)
    {

    }

    public CachedContentMutation(Func<IContentGroup<double>, IContentGroup<double>> mutation) : base(mutation) { }

    public CachedContentMutation(Func<IContentNode<double>, IContentNode<double>> mutation) : base(mutation) { }
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
    private Func<IContentNode<T>, IEnumerable<IContentNode<T>>> _mutation;
    private readonly ConditionalWeakTable<PdfStream, Dictionary<FormCacheKey, List<CachedFormEntry>>> _cache =
        new();

    /// <summary>
    /// Create mutation helper with func that applies mutation to content node returning enumerable result.
    /// </summary>
    /// <param name="mutation"></param>
    public CachedContentMutation(Func<IContentNode<T>, IEnumerable<IContentNode<T>>> mutation)
    {
        _mutation = mutation;
    }

    public CachedContentMutation(Func<IContentGroup<T>, IEnumerable<IContentGroup<T>>> mutation)
    {
        _mutation = WrapCompat(mutation);
    }

    /// <summary>
    /// Create mutation helper with func that applies mutation to content node returning single (or none/null) content node result.
    /// </summary>
    /// <param name="mutation"></param>
    public CachedContentMutation(Func<IContentNode<T>, IContentNode<T>?> mutation)
    {
        _mutation = Wrap(mutation);
    }

    public CachedContentMutation(Func<IContentGroup<T>, IContentGroup<T>?> mutation)
    {
        _mutation = WrapCompat(mutation);
    }

    /// <summary>
    /// Applies the mutation to a list of content nodes.
    /// 
    /// If any content nodes are forms these are parsed into content nodes themselves and
    /// mutation applied recursively.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public List<IContentNode<T>> Apply(List<IContentNode<T>> content)
    {
        var newContents = new List<IContentNode<T>>();
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
            else if (item is MarkedContentGroup<T> mcg)
            {
                var children = Apply(mcg.Children);
                if (children.Count > 0)
                {
                    var newMcg = new MarkedContentGroup<T>(mcg.Tag)
                    {
                        GraphicsState = mcg.GraphicsState,
                        CompatibilitySection = mcg.CompatibilitySection
                    };
                    newMcg.Children.AddRange(children);
                    newContents.Add(newMcg);
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
    /// Applies the mutation to a list of legacy content groups.
    /// </summary>
    public List<IContentGroup<T>> Apply(List<IContentGroup<T>> content)
    {
        return Apply(content.Cast<IContentNode<T>>().ToList()).Cast<IContentGroup<T>>().ToList();
    }

    /// <summary>
    /// Applies the mutation to all content of a page including nested forms.
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public PdfPage Apply(PdfPage page)
    {
        var model = page.GetContentNodes<T>(false);
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
        var cacheKey = new FormCacheKey(form.GraphicsState.CTM, GetClippingHash(form.GraphicsState.Clipping));
        if (_cache.TryGetValue(form.Stream, out var result)
            && result.TryGetValue(cacheKey, out var matches))
        {
            foreach (var cached in matches)
            {
                if (!ClippingEqual(cached.Clipping, form.GraphicsState.Clipping))
                {
                    continue;
                }

                if (cached.Form == null) { return null; }
                return new FormContent<T>
                {
                    GraphicsState = form.GraphicsState,
                    Stream = cached.Form.NativeObject
                };
            }
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
            if (item is IContentItem<T> leaf && item is not FormContent<T>)
            {
                leaf.ClipFrom(form.GraphicsState);
                leaf.ClipExcept(formBox);
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
                if (item is IContentItem<T> leaf)
                {
                    leaf.TransformInitial(inv);
                }
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
                result = new Dictionary<FormCacheKey, List<CachedFormEntry>>();
                _cache.Add(form.Stream, result);
            }

            if (!result.TryGetValue(cacheKey, out var entries))
            {
                entries = new List<CachedFormEntry>();
                result[cacheKey] = entries;
            }

            entries.Add(new CachedFormEntry(CloneClipping(form.GraphicsState.Clipping), value));
        }
    }

    private static int GetClippingHash(List<IClippingSection<T>>? clipping)
    {
        if (clipping == null)
        {
            return 0;
        }

        var hash = new HashCode();
        hash.Add(clipping.Count);
        foreach (var clip in clipping)
        {
            hash.Add(clip.GetType());
            if (clip is ClippingInfo<T> pathClip)
            {
                hash.Add(pathClip.TM);
                hash.Add(pathClip.EvenOdd);
                hash.Add(pathClip.Path.Count);
                foreach (var path in pathClip.Path)
                {
                    hash.Add(path);
                }
            }
            else if (clip is TextClippingInfo<T> textClip)
            {
                hash.Add(textClip.TM);
                hash.Add(textClip.LineMatrix);
                hash.Add(textClip.NewLine);
                hash.Add(textClip.Glyphs.Count);
                foreach (var glyph in textClip.Glyphs)
                {
                    hash.Add(glyph.Shift);
                    hash.Add(glyph.Bytes);
                    AddGlyphHash(ref hash, glyph.Glyph);
                }
            }
            else
            {
                hash.Add(clip.TM);
            }
        }

        return hash.ToHashCode();
    }

    private static void AddGlyphHash(ref HashCode hash, Glyph? glyph)
    {
        if (glyph == null)
        {
            hash.Add(0);
            return;
        }

        hash.Add(glyph.Char);
        hash.Add(glyph.MultiChar);
        hash.Add(glyph.w0);
        hash.Add(glyph.w1);
        hash.Add(glyph.IsWordSpace);
        hash.Add(glyph.CodePoint);
        hash.Add(glyph.CID);
        hash.Add(glyph.Undefined);
        hash.Add(glyph.Name);
        hash.Add(glyph.GuessedUnicode);
        if (glyph.BBox == null)
        {
            hash.Add(0);
            return;
        }

        hash.Add(glyph.BBox.Length);
        foreach (var value in glyph.BBox)
        {
            hash.Add(value);
        }
    }

    private static List<IClippingSection<T>>? CloneClipping(List<IClippingSection<T>>? clipping)
    {
        return clipping?.Select(x => x.ShallowClone()).ToList();
    }

    private static bool ClippingEqual(List<IClippingSection<T>>? a, List<IClippingSection<T>>? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        if (a.Count != b.Count) return false;

        for (var i = 0; i < a.Count; i++)
        {
            var c1 = a[i];
            var c2 = b[i];
            if (c1.GetType() != c2.GetType())
            {
                return false;
            }

            if (c1 is ClippingInfo<T> ci1 && c2 is ClippingInfo<T> ci2)
            {
                if (ci1.TM != ci2.TM) { return false; }
                if (ci1.EvenOdd != ci2.EvenOdd) { return false; }
                if (ci1.Path.Count != ci2.Path.Count) { return false; }
                if (!ci1.Path.SequenceEqual(ci2.Path)) { return false; }
                continue;
            }

            if (c1 is TextClippingInfo<T> tc1 && c2 is TextClippingInfo<T> tc2)
            {
                if (tc1.TM != tc2.TM) { return false; }
                if (tc1.LineMatrix != tc2.LineMatrix) { return false; }
                if (tc1.NewLine != tc2.NewLine) { return false; }
                if (tc1.Glyphs.Count != tc2.Glyphs.Count) { return false; }
                for (var g = 0; g < tc1.Glyphs.Count; g++)
                {
                    if (!GlyphEqual(tc1.Glyphs[g], tc2.Glyphs[g])) { return false; }
                }
                continue;
            }

            if (c1.TM != c2.TM)
            {
                return false;
            }
        }

        return true;
    }

    private static bool GlyphEqual(GlyphOrShift<T> a, GlyphOrShift<T> b)
    {
        if (a.Shift != b.Shift || a.Bytes != b.Bytes)
        {
            return false;
        }

        return GlyphEqual(a.Glyph, b.Glyph);
    }

    private static bool GlyphEqual(Glyph? a, Glyph? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a == null || b == null) return false;
        if (a.Char != b.Char) return false;
        if (a.MultiChar != b.MultiChar) return false;
        if (a.w0 != b.w0 || a.w1 != b.w1) return false;
        if (a.IsWordSpace != b.IsWordSpace) return false;
        if (a.CodePoint != b.CodePoint || a.CID != b.CID) return false;
        if (a.Undefined != b.Undefined) return false;
        if (a.Name != b.Name) return false;
        if (a.GuessedUnicode != b.GuessedUnicode) return false;

        if (a.BBox == null && b.BBox == null) return true;
        if (a.BBox == null || b.BBox == null) return false;
        if (a.BBox.Length != b.BBox.Length) return false;
        for (var i = 0; i < a.BBox.Length; i++)
        {
            if (a.BBox[i] != b.BBox[i]) return false;
        }
        return true;
    }

    private static Func<IContentNode<T>, IEnumerable<IContentNode<T>>> Wrap(Func<IContentNode<T>, IContentNode<T>?> single)
    {
        IEnumerable<IContentNode<T>> Wrapped(IContentNode<T> input)
        {
            var r = single(input);
            if (r != null)
            {
                yield return r;
            }
        }
        return Wrapped;
    }

    private static Func<IContentNode<T>, IEnumerable<IContentNode<T>>> WrapCompat(Func<IContentGroup<T>, IEnumerable<IContentGroup<T>>> mutation)
    {
        IEnumerable<IContentNode<T>> Wrapped(IContentNode<T> input)
        {
            if (input is not IContentGroup<T> group)
            {
                throw new InvalidOperationException($"Legacy content-group mutation cannot handle node type {input.GetType().Name}.");
            }
            foreach (var item in mutation(group))
            {
                yield return item;
            }
        }
        return Wrapped;
    }

    private static Func<IContentNode<T>, IEnumerable<IContentNode<T>>> WrapCompat(Func<IContentGroup<T>, IContentGroup<T>?> single)
    {
        IEnumerable<IContentNode<T>> Wrapped(IContentNode<T> input)
        {
            if (input is not IContentGroup<T> group)
            {
                throw new InvalidOperationException($"Legacy content-group mutation cannot handle node type {input.GetType().Name}.");
            }
            var result = single(group);
            if (result != null)
            {
                yield return result;
            }
        }
        return Wrapped;
    }

    private readonly record struct FormCacheKey(GfxMatrix<T> CTM, int ClippingHash);

    private sealed record CachedFormEntry(List<IClippingSection<T>>? Clipping, XObjForm? Form);
}
