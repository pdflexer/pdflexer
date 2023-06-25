using Microsoft.VisualBasic;
using PdfLexer.DOM;
using PdfLexer.Writing;
using System;
using System.Numerics;
using System.Resources;
using System.Runtime.CompilerServices;

namespace PdfLexer.Content.Model;

public class CachedContentMutation : CachedContentMutation<double>
{
    public CachedContentMutation(Func<IContentGroup<double>, IContentGroup<double>> mutation) : base(mutation)
    {

    }
}

public class CachedContentMutation<T> where T : struct, IFloatingPoint<T>
{
    private Func<IContentGroup<T>, IContentGroup<T>> _mutation;
    private ConditionalWeakTable<PdfStream, Dictionary<GfxMatrix<T>, XObjForm>> _cache =
        new ConditionalWeakTable<PdfStream, Dictionary<GfxMatrix<T>, XObjForm>>();

    public CachedContentMutation(Func<IContentGroup<T>, IContentGroup<T>> mutation)
    {
        _mutation = mutation;
    }

    public List<IContentGroup<T>> Mutate(PdfDictionary page, List<IContentGroup<T>> content)
    {
        var newContents = new List<IContentGroup<T>>();
        foreach (var item in content)
        {
            IContentGroup<T> mutated;
            if (item is FormContent<T> nested)
            {
                mutated = RecursivelyApply(page, nested);
            }
            else
            {
                mutated = _mutation(item);
            }

            newContents.Add(mutated);
        }
        return newContents;
    }

    public PdfPage Mutate(PdfPage page)
    {
        var model = page.GetContentModel<T>(false);
        var mutated = Mutate(page.NativeObject, model);

        PdfPage newPage = page.NativeObject.CloneShallow();
        var res = new PdfDictionary();
        newPage.Resources = res;
        using var writer = newPage.GetWriter<T>(PageWriteMode.Replace);
        writer.AddContent(mutated);
        return newPage;
    }

    private FormContent<T> RecursivelyApply(PdfDictionary page, FormContent<T> form)
    {
        if (_cache.TryGetValue(form.Stream, out var result) && result.TryGetValue(form.GraphicsState.CTM, out var match))
        {
            return new FormContent<T>
            {
                GraphicsState = form.GraphicsState,
                Stream = match.NativeObject
            };
        }

        var parser = new ContentModelParser<T>(ParsingContext.Current, page, form.Stream, new GfxState<T> { CTM = form.GraphicsState.CTM });
        var content = parser.Parse();
        var newContents = Mutate(page, content);
        // these were in a form, need to "undo" the applied CTM
        if (!form.GraphicsState.CTM.IsIdentity)
        {
            if (!form.GraphicsState.CTM.Invert(out var inv))
            {
                throw new PdfLexerException("Unable to invert matrix for form.");
            }

            foreach (var item in newContents)
            {
                // if (item is FormContent<T>)
                // {
                //     continue;
                // }
                item.Transform(inv);
            }
        }


        var xObj = new XObjForm();
        var resources = new PdfDictionary();
        xObj.NativeObject.Dictionary[PdfName.Resources] = resources;
        var writer = new ContentWriter<T>(resources);
        writer.AddContent(newContents);
        xObj.NativeObject.Contents = writer.Complete();
        if (result == null)
        {
            result = new Dictionary<GfxMatrix<T>, XObjForm>();
            result[form.GraphicsState.CTM] = xObj;
            _cache.Add(form.Stream, result);
        }
        else
        {
            result[form.GraphicsState.CTM] = xObj;
        }

        return new FormContent<T>
        {
            GraphicsState = form.GraphicsState,
            Stream = xObj.NativeObject
        };
    }
}
