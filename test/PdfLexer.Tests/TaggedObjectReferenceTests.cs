using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Images;
using PdfLexer.Writing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Linq;
using Xunit;

namespace PdfLexer.Tests;

public class TaggedObjectReferenceTests
{
    [Fact]
    public void Tagged_Link_RoundTrips_With_OBJR_And_StructParent()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        var dest = new PdfArray
        {
            page.NativeObject.Indirect(),
            PdfName.XYZ,
            new PdfIntNumber(0),
            new PdfIntNumber(0),
            PdfNull.Value
        };

        var link = doc.Structure.AddLink(page, new PdfRect<double>(10, 10, 60, 20), dest, "Jump", "Jump alt");
        using (var writer = page.GetWriter())
        {
            writer.BeginMarkedContent(link.GetNode());
            writer.Font(Base14.Helvetica, 12).Text("Jump");
            writer.EndMarkedContent();
        }

        using var saved = PdfDocument.Open(doc.Save());
        var page2 = saved.Pages[0];
        var annot = page2.NativeObject.Get<PdfArray>(PdfName.Annots)![0].Resolve().GetAs<PdfDictionary>();

        Assert.Equal(PdfName.Link, annot.Get<PdfName>(PdfName.Subtype));
        Assert.True(page2.NativeObject.ContainsKey(PdfName.StructParents));
        Assert.True(annot.ContainsKey(PdfName.StructParent));

        var structRoot = saved.Catalog.Get<PdfDictionary>(PdfName.StructTreeRoot)!;
        var parentTree = structRoot[PdfName.ParentTree].Resolve().GetAs<PdfDictionary>();
        var linkElem = GetParentTreeValue(parentTree, (int)annot.Get<PdfNumber>(PdfName.StructParent)!);
        Assert.Equal(PdfName.Link, linkElem.Get<PdfName>(PdfName.S));

        var kids = linkElem[PdfName.K].Resolve().GetAs<PdfArray>();
        var objr = kids.Select(x => x.Resolve().GetAsOrNull<PdfDictionary>())
            .First(x => x?.Get<PdfName>(PdfName.TYPE) == PdfName.OBJR)!;
        Assert.Same(annot, objr[PdfName.Obj].Resolve());
        AccessibilityIntegrityAssert.HasBasicStructureIntegrity(saved);
    }

    [Fact]
    public void Tagged_Widget_RoundTrips_With_Form_StructElem()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();

        doc.Structure.AddFormField(doc, page, new PdfRect<double>(10, 10, 110, 30), "full_name", "Name", "Full name", print: true);

        using var saved = PdfDocument.Open(doc.Save());
        var page2 = saved.Pages[0];
        var annot = page2.NativeObject.Get<PdfArray>(PdfName.Annots)![0].Resolve().GetAs<PdfDictionary>();
        var field = annot[PdfName.Parent].Resolve().GetAs<PdfDictionary>();

        Assert.Equal(PdfName.Widget, annot.Get<PdfName>(PdfName.Subtype));
        Assert.Equal("Full name", field.Get<PdfString>(PdfName.TU)!.Value);
        Assert.Equal(4, (PdfIntNumber)annot[PdfName.F]);
        Assert.True(saved.Catalog.Get<PdfDictionary>((PdfName)"AcroForm")!.Get<PdfArray>(PdfName.Fields)!.Count > 0);

        var structRoot = saved.Catalog.Get<PdfDictionary>(PdfName.StructTreeRoot)!;
        var parentTree = structRoot[PdfName.ParentTree].Resolve().GetAs<PdfDictionary>();
        var formElem = GetParentTreeValue(parentTree, (int)annot.Get<PdfNumber>(PdfName.StructParent)!);
        Assert.Equal(PdfName.Form, formElem.Get<PdfName>(PdfName.S));

        var kid = formElem[PdfName.K].Resolve().GetAs<PdfDictionary>();
        Assert.Equal(PdfName.OBJR, kid.Get<PdfName>(PdfName.TYPE));
        AccessibilityIntegrityAssert.HasBasicStructureIntegrity(saved);
    }

    [Fact]
    public void Tagged_Image_Reused_On_Two_Pages_Maps_To_One_Structure_Node()
    {
        using var image = new Image<Rgba32>(4, 4);
        image.Mutate(x => x.BackgroundColor(Color.Black));
        var xobj = image.CreatePdfImage();

        using var doc = PdfDocument.Create();
        var page1 = doc.AddPage();
        var page2 = doc.AddPage();
        var figure = doc.Structure.AddFigure("Figure", "Alt");
        figure.BindImage(xobj, page1, page2);

        using (var writer = page1.GetWriter())
        {
            writer.Image(xobj, 10, 10, 20, 20);
        }
        using (var writer = page2.GetWriter())
        {
            writer.Image(xobj, 20, 20, 20, 20);
        }

        using var saved = PdfDocument.Open(doc.Save());
        var img1 = GetFirstXObject(saved.Pages[0], PdfName.Image);
        var img2 = GetFirstXObject(saved.Pages[1], PdfName.Image);

        Assert.Equal((PdfIntNumber)img1.Dictionary[PdfName.StructParent], (PdfIntNumber)img2.Dictionary[PdfName.StructParent]);

        var structRoot = saved.Catalog.Get<PdfDictionary>(PdfName.StructTreeRoot)!;
        var parentTree = structRoot[PdfName.ParentTree].Resolve().GetAs<PdfDictionary>();
        var figureElem = GetParentTreeValue(parentTree, (int)img1.Dictionary.Get<PdfNumber>(PdfName.StructParent)!);
        Assert.Equal(PdfName.Figure, figureElem.Get<PdfName>(PdfName.S));
        Assert.True(saved.Pages[0].NativeObject.ContainsKey(PdfName.StructParents));
        Assert.True(saved.Pages[1].NativeObject.ContainsKey(PdfName.StructParents));
        AccessibilityIntegrityAssert.HasBasicStructureIntegrity(saved);
    }

    [Fact]
    public void Tagged_Form_XObject_Internal_MCIDs_Map_Back_To_Structure_Elements()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        var container = doc.Structure.AddFigure("Tagged Form", "Alt");
        var first = container.AddSpan("First");
        first.Back();
        var second = container.AddSpan("Second");
        second.Back();

        var formWriter = new FormWriter(100, 50);
        formWriter.BeginMarkedContent(first.GetNode());
        formWriter.Font(Base14.Helvetica, 12).Text("One");
        formWriter.EndMarkedContent();
        formWriter.BeginMarkedContent(second.GetNode());
        formWriter.TextMove(0, 15).Text("Two");
        formWriter.EndMarkedContent();
        var form = formWriter.Complete();
        container.BindFormXObject(form, page);

        using (var writer = page.GetWriter())
        {
            writer.Form(form, 10, 10);
        }

        using var saved = PdfDocument.Open(doc.Save());
        var formXObject = (XObjForm)GetFirstXObject(saved.Pages[0], PdfName.Form);
        var index = (int)formXObject.StructParents!;

        var structRoot = saved.Catalog.Get<PdfDictionary>(PdfName.StructTreeRoot)!;
        var parentTree = structRoot[PdfName.ParentTree].Resolve().GetAs<PdfDictionary>();
        var refs = GetParentTreeArray(parentTree, index);

        Assert.Equal(2, refs.Count);
        Assert.Equal("First", refs[0].Resolve().GetAs<PdfDictionary>().Get<PdfString>(PdfName.T)!.Value);
        Assert.Equal("Second", refs[1].Resolve().GetAs<PdfDictionary>().Get<PdfString>(PdfName.T)!.Value);
        Assert.True(saved.Pages[0].NativeObject.ContainsKey(PdfName.StructParents));
    }

    private static PdfDictionary GetParentTreeValue(PdfDictionary parentTree, int index)
    {
        var nums = parentTree.Get<PdfArray>(PdfName.Nums)!;
        for (var i = 0; i < nums.Count; i += 2)
        {
            if ((PdfIntNumber)nums[i] == index)
            {
                return nums[i + 1].Resolve().GetAs<PdfDictionary>();
            }
        }

        throw new Xunit.Sdk.XunitException($"ParentTree entry {index} not found.");
    }

    private static PdfArray GetParentTreeArray(PdfDictionary parentTree, int index)
    {
        var nums = parentTree.Get<PdfArray>(PdfName.Nums)!;
        for (var i = 0; i < nums.Count; i += 2)
        {
            if ((PdfIntNumber)nums[i] == index)
            {
                return nums[i + 1].Resolve().GetAs<PdfArray>();
            }
        }

        throw new Xunit.Sdk.XunitException($"ParentTree array {index} not found.");
    }

    private static PdfStream GetFirstXObject(PdfPage page, PdfName subtype)
    {
        return page.Resources.Get<PdfDictionary>(PdfName.XObject)!
            .Select(x => x.Value.Resolve().GetAsOrNull<PdfStream>())
            .First(x => x?.Dictionary.Get<PdfName>(PdfName.Subtype) == subtype)!;
    }
}
