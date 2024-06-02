using DotNext.Collections.Generic;
using PdfLexer.Content;
using PdfLexer.Filters;

namespace PdfLexer.DOM;

public class XObjForm
{
    public PdfStream NativeObject { get; }
    public XObjForm(PdfStream dict)
    {
        NativeObject = dict;
    }

    internal XObjForm()
    {
        NativeObject = new PdfStream();
        NativeObject.Dictionary[PdfName.Subtype] = PdfName.Form;
    }

    public XObjForm(PageSize size)
    {
        NativeObject = new PdfStream();
        NativeObject.Dictionary[PdfName.Subtype] = PdfName.Form;
        NativeObject.Dictionary[PdfName.BBox] = PageSizeHelpers.GetMediaBox(size);
    }

    public XObjForm(PdfRect<double> bbox)
    {
        NativeObject = new PdfStream();
        NativeObject.Dictionary[PdfName.Subtype] = PdfName.Form;
        NativeObject.Dictionary[PdfName.BBox] = PdfRectangle.FromContentModel(bbox).NativeObject;
    }

    public XObjForm(double width, double height)
    {
        NativeObject = new PdfStream();
        NativeObject.Dictionary[PdfName.Subtype] = PdfName.Form;
        NativeObject.Dictionary[PdfName.BBox] = new PdfArray {
            PdfCommonNumbers.Zero, PdfCommonNumbers.Zero,
            new PdfDoubleNumber(width), new PdfDoubleNumber(height)
        };
    }

    public static implicit operator XObjForm(PdfStream str) => new XObjForm(str);
    public static implicit operator PdfStream(XObjForm form) => form.NativeObject;

    public PdfRectangle? BBox
    {
        get => NativeObject.Dictionary.Get<PdfArray>(PdfName.BBox);
        set => NativeObject.Dictionary.Set(PdfName.BBox, value?.NativeObject);
    }

    public PdfDictionary? Resources
    {
        get => NativeObject.Dictionary.Get<PdfDictionary>(PdfName.Resources);
        set => NativeObject.Dictionary.Set(PdfName.Resources, value);
    }

    public PdfStreamContents? Contents
    {
        get => NativeObject.Contents;
        set => NativeObject.Contents = value == null ? PdfStreamContents.Empty : value;
    }

    // Type -> XObject (optional)
    // Subtype => Form
    // FormType => 1 (optional)
    // BBox req => PdfRectangle clipping box
    // Matrix opt => transformation matrix
    // Resources => same as page
    // Group opt
    // Ref opt
    // Metadata opt
    // PieceInfo opt
    // LastModified opt
    // StructParent req if structural
    // StructParents req if structural
    // OPT opt
    // OC opt
    // Name req in pdf v1 -> not recommended
    // + stream items

    /// <summary>
    /// Creates an xobject form from a page
    /// </summary>
    /// <param name="page"></param>
    /// <param name="uninheritResources">
    /// If page resources should be copied to all child forms
    /// in case these were not included in the form resource
    /// dictionaries.
    /// </param>
    /// <returns></returns>
    public static XObjForm FromPage(PdfPage page, bool uninheritResources = true)
    {
        // handle non zero, zero crop box
        var form = new XObjForm();
        var contents = page.Contents.ToList();
        if (contents.Count == 1)
        {
            form.Contents = contents[0].Contents;
        }
        else
        {
            var flate = new ZLibLexerStream();
            foreach (var stream in contents)
            {
                using var str = stream.Contents.GetDecodedStream();
                str.CopyTo(flate.Stream);
            }
            form.Contents = flate.Complete();
        }

        form.Resources = page.Resources.CloneShallow();

        // handle rotation
        var cb = page.CropBox;
        var rotate = 0m;
        if (page.Rotate != null)
        {
            rotate = (decimal)page.Rotate;
        }

        if (rotate % 360 != 0 && rotate % 90 == 0)
        {
            var r = rotate % 360;
            switch (r)
            {
                case 90:
                case -270:
                    form.BBox = PdfRectangle.Create(0, 0, cb.Height, cb.Width);
                    form.NativeObject.Dictionary[PdfName.Matrix] = new GfxMatrix<double>(0, -1, 1, 0, -cb.LLx, cb.LLy + (double)cb.Width).AsPdfArray();
                    break;
                case -90:
                case 270:
                    form.BBox = PdfRectangle.Create(0, 0, cb.Height, cb.Width);
                    form.NativeObject.Dictionary[PdfName.Matrix] = new GfxMatrix<double>(0, 1, -1, 0, cb.LLx + (double)cb.Height, -cb.LLy).AsPdfArray();
                    break;
                case 180:
                case -180:
                    form.BBox = PdfRectangle.Create(0, 0, cb.Width, cb.Height);
                    form.NativeObject.Dictionary[PdfName.Matrix] = new GfxMatrix<double>(-1, 0, 0, -1, cb.LLx + (double)cb.Width, cb.LLy + (double)cb.Height).AsPdfArray();
                    break;
            }
        }
        else
        {
            form.BBox = PdfRectangle.Create(0, 0, cb.Width, cb.Height);
            form.NativeObject.Dictionary[PdfName.Matrix] = new GfxMatrix<double>(1, 0, 0, 1, -(double)cb.LLx, -(double)cb.LLy).AsPdfArray();
        }

        if (uninheritResources)
        {
            form.Resources.TryGetValue<PdfDictionary>(PdfName.ExtGState, out var gs, false);
            form.Resources.TryGetValue<PdfDictionary>(PdfName.ColorSpace, out var cs, false);
            form.Resources.TryGetValue<PdfDictionary>(PdfName.Pattern, out var patterns, false);
            form.Resources.TryGetValue<PdfDictionary>(PdfName.Shading, out var shading, false);
            form.Resources.TryGetValue<PdfDictionary>(PdfName.XObject, out var xobj, false);
            form.Resources.TryGetValue<PdfDictionary>(PdfName.Font, out var fonts, false);

            EnumerateForms(form.NativeObject.Dictionary, x =>
            {
                var copy = x.CloneShallow();
                MergeDicts(copy, PdfName.ExtGState, gs);
                MergeDicts(copy, PdfName.ColorSpace, cs);
                MergeDicts(copy, PdfName.Pattern, patterns);
                MergeDicts(copy, PdfName.Shading, shading);
                MergeDicts(copy, PdfName.XObject, xobj);
                MergeDicts(copy, PdfName.Font, fonts);

                return copy;
            }, new HashSet<PdfDictionary>());
        }

        return form;
    }
    private static void MergeDicts(PdfDictionary parent, PdfName type, PdfDictionary? source)
    {
        if (source == null)
        {
            return; // no resources
        }

        var dest = parent.GetOptionalValue<PdfDictionary>(type);
        if (dest != null)
        {
            dest = dest.CloneShallow();
        }
        else
        {
            dest = new PdfDictionary();
        }
        foreach (var (key, value) in source)
        {
            if (!dest.ContainsKey(key))
            {
                dest[key] = value.Indirect();
            }
        }
        parent[type] = dest;
    }

    private static void EnumerateForms(PdfDictionary pg, Func<PdfDictionary, PdfDictionary> resourceMutation, HashSet<PdfDictionary> seen)
    {
        if (pg.TryGetValue<PdfDictionary>(PdfName.Resources, out var res, false))
        {
            res = res.CloneShallow();
            pg[PdfName.Resources] = res;
            if (res.TryGetValue<PdfDictionary>(PdfName.XObject, out var xobj, false))
            {
                xobj = xobj.CloneShallow();
                res[PdfName.XObject] = xobj;
                foreach (var (key, value) in xobj)
                {
                    if (value.Resolve() is PdfStream str && str.Dictionary.Get<PdfName>(PdfName.Subtype) == PdfName.Form)
                    {
                        if (seen.Contains(str.Dictionary))
                        {
                            continue;
                        }
                        seen.Add(str.Dictionary);
                        str = str.CloneShallow();
                        seen.Add(str.Dictionary);

                        xobj[key] = str.Indirect();
                        EnumerateForms(str.Dictionary, resourceMutation, seen);
                    }
                }
            }
            pg[PdfName.Resources] = resourceMutation(res);
        }
    }
}
