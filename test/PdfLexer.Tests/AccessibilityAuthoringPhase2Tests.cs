using System;
using System.IO;
using System.Linq;
using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using Xunit;

namespace PdfLexer.Tests;

public class AccessibilityAuthoringPhase2Tests
{
    [Fact]
    public void TypedReferences_AssignStableIds_Deduplicate_AndRejectCrossTreeTargets()
    {
        using var doc = PdfDocument.Create();
        var target = doc.Structure.AddHeaderCell();
        var source = target.Back().AddDataCell().References(target, target).TableHeaders(target);

        Assert.Equal("struct-1", target.GetNode().ID);
        Assert.Single(source.GetNode().References);
        Assert.Single(source.GetNode().Headers);

        using var other = PdfDocument.Create();
        var foreign = other.Structure.AddParagraph();
        Assert.Throws<ArgumentException>(() => source.References(foreign));
        Assert.Throws<ArgumentNullException>(() => source.References((IStructureContext)null!));
    }

    [Fact]
    public void DanglingStringReference_ThrowsStrict_AndWarnsNonStrict()
    {
        using (var strict = PdfDocument.Create())
        {
            strict.AddPage();
            strict.ApplyAccessibilitySetup("en-US", "strict refs", strictConformance: true);
            strict.Structure.AddParagraph().References("missing");
            Assert.Throws<PdfAccessibilityConformanceException>(() => strict.SaveTo(new MemoryStream()));
        }

        var context = ParsingContext.Reset();
        using var permissive = PdfDocument.Create();
        permissive.AddPage();
        permissive.ApplyAccessibilitySetup("en-US", "permissive refs", strictConformance: false);
        permissive.Structure.AddParagraph("source").References("missing");
        permissive.SaveTo(new MemoryStream());
        Assert.Equal(1, context.WarningCount);
        Assert.Contains("missing ID 'missing'", context.ParsingWarnings[0]);
    }

    [Fact]
    public void AddAnnot_AddsAndBindsSquareAnnotation_AndBindRequiresOwnership()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "annotation", strictConformance: true);
        var appearance = new XObjForm(20, 20);
        var square = new PdfDictionary
        {
            [PdfName.TypeName] = PdfName.Annot,
            [PdfName.Subtype] = (PdfName)"Square",
            [PdfName.Rect] = new PdfArray { 10, 10, 30, 30 },
            [(PdfName)"AP"] = new PdfDictionary { [PdfName.N] = appearance.NativeObject.Indirect() }
        };

        doc.Structure.AddAnnot(page, square, "Review box", "Review box");
        using var output = new MemoryStream();
        doc.SaveTo(output);
        output.Position = 0;
        using var saved = PdfDocument.Open(output);
        var savedSquare = saved.Pages[0].NativeObject.Get<PdfArray>(PdfName.Annots)![0].Resolve().GetAs<PdfDictionary>();

        Assert.NotNull(savedSquare.Get<PdfNumber>(PdfName.StructParent));
        Assert.Single(page.NativeObject.Get<PdfArray>(PdfName.Annots)!);

        var orphan = new PdfDictionary { [PdfName.Subtype] = (PdfName)"Square" };
        Assert.Throws<ArgumentException>(() => doc.Structure.BindAnnotation(orphan, page));
    }

    [Fact]
    public void StrictVisibleAnnotation_RequiresDescriptionAndAppearance()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "annotation", strictConformance: true);
        var square = new PdfDictionary
        {
            [PdfName.TypeName] = PdfName.Annot,
            [PdfName.Subtype] = (PdfName)"Square",
            [PdfName.Rect] = new PdfArray { 10, 10, 30, 30 }
        };
        doc.Structure.AddAnnot(page, square);
        Assert.Throws<PdfAccessibilityConformanceException>(() => doc.SaveTo(new MemoryStream()));
    }

    [Fact]
    public void WidgetFactories_CreateAppearancesStatesAndSingleLogicalFields()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        var appearance = EmbeddedAppearance();

        var text = AnnotationFactory.CreateTextWidget(
            doc, page, new PdfRect<double>(10, 10, 110, 30), "name", appearance,
            "Ada", "Full name");
        var check = AnnotationFactory.CreateCheckboxWidget(
            doc, page, new PdfRect<double>(10, 40, 30, 60), "yes", appearance,
            true, tooltip: "Agree");
        var combo = AnnotationFactory.CreateChoiceWidget(
            doc, page, new PdfRect<double>(10, 70, 110, 90), "choice",
            new[] { "One", "Two" }, ChoiceFieldKind.Combo, appearance, "Two", "Choice");
        var button = AnnotationFactory.CreatePushButtonWidget(
            doc, page, new PdfRect<double>(10, 100, 110, 125), "submit", "Submit", appearance, "Submit");
        var radio = AnnotationFactory.CreateRadioGroup(
            doc, "radio",
            new[]
            {
                new RadioButtonOption(page, new PdfRect<double>(10, 140, 30, 160), "A", true),
                new RadioButtonOption(page, new PdfRect<double>(40, 140, 60, 160), "B")
            },
            appearance, "Radio");

        Assert.NotNull(text.NativeObject.Get<PdfDictionary>((PdfName)"AP")?.Get<PdfStream>(PdfName.N));
        Assert.Equal((PdfName)"Yes", check.NativeObject.Get<PdfName>((PdfName)"AS"));
        Assert.Equal((PdfName)"Two", combo.Field.Get<PdfString>(PdfName.V)?.Value is string value ? (PdfName)value : null);
        Assert.Equal(1 << 16, (int)button.Field.Get<PdfNumber>((PdfName)"Ff")!);
        Assert.Equal(2, radio.Widgets.Count);
        Assert.Equal(5, doc.Catalog.Get<PdfDictionary>((PdfName)"AcroForm")!.Get<PdfArray>(PdfName.Fields)!.Count);
        Assert.False(doc.Catalog.Get<PdfDictionary>((PdfName)"AcroForm")!.ContainsKey((PdfName)"NeedAppearances"));
    }

    [Fact]
    public void LegacyWidget_WarnsNonStrict_AndRejectsStrict()
    {
        var context = ParsingContext.Reset();
        using (var permissive = PdfDocument.Create())
        {
            var page = permissive.AddPage();
            AnnotationFactory.CreateTextWidget(permissive, page, new PdfRect<double>(0, 0, 20, 20), "legacy");
            Assert.Equal(1, context.WarningCount);
        }

        using var strict = PdfDocument.Create();
        var strictPage = strict.AddPage();
        strict.ApplyAccessibilitySetup("en-US", "strict", strictConformance: true);
        Assert.Throws<PdfAccessibilityConformanceException>(() =>
            AnnotationFactory.CreateTextWidget(strict, strictPage, new PdfRect<double>(0, 0, 20, 20), "legacy"));
    }

    [Fact]
    public void IDTree_MaintainsByteOrderingWithNonAsciiKeys()
    {
        using var doc = PdfDocument.Create();
        doc.ApplyAccessibilitySetup("en-US", "IDTree test", PdfUaProfile.PdfUa1, strictConformance: true);
        var p1 = doc.Structure.AddParagraph("p1");
        p1.GetNode().ID = "aÄ";
        var p2 = doc.Structure.AddParagraph("p2");
        p2.GetNode().ID = "zzz";

        using var ms = new MemoryStream();
        doc.SaveTo(ms);
        ms.Position = 0;
        using var saved = PdfDocument.Open(ms);
        var structRoot = saved.Catalog.Get<PdfDictionary>(PdfName.StructTreeRoot)!;
        var idTree = structRoot.Get<PdfDictionary>(PdfName.IDTree)!;
        var names = idTree.Get<PdfArray>(PdfName.Names)!;

        // "zzz" (7A 7A 7A) comes before "aÄ" (FE FF 00 61 00 C4) by byte order
        var firstKey = names[0].GetAs<PdfString>().Value;
        var secondKey = names[2].GetAs<PdfString>().Value;
        Assert.Equal("zzz", firstKey);
        Assert.Equal("aÄ", secondKey);
    }

    [Fact]
    public void AddLayoutAttributes_OnlyAddsWhenMultipleEntries()
    {
        using var doc = PdfDocument.Create();
        var p = doc.Structure.AddParagraph("test");
        p.AddLayoutAttributes(null, null, null);
        Assert.Empty(p.GetNode().Attributes);

        p.AddLayoutAttributes(textAlign: "Left");
        Assert.Single(p.GetNode().Attributes);
    }

    [Fact]
    public void ValidatePageAnnotations_WarnsInNonStrictMode()
    {
        var context = ParsingContext.Reset();
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "NonStrict test", PdfUaProfile.PdfUa1, strictConformance: false);

        var annot = new PdfDictionary
        {
            [PdfName.TypeName] = PdfName.Annot,
            [PdfName.Subtype] = (PdfName)"Square",
            [PdfName.Rect] = new PdfArray { new PdfIntNumber(0), new PdfIntNumber(0), new PdfIntNumber(10), new PdfIntNumber(10) }
        };
        page.AddAnnotation(annot);

        using var ms = new MemoryStream();
        doc.SaveTo(ms);
        Assert.True(context.WarningCount > 0);
    }

    [Fact]
    public void RadioGroupTooltip_SurvivesPerWidgetStructureTitles()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        var radio = AnnotationFactory.CreateRadioGroup(
            doc, "contact",
            new[]
            {
                new RadioButtonOption(page, new PdfRect<double>(10, 10, 30, 30), "Email", true, "Email contact"),
                new RadioButtonOption(page, new PdfRect<double>(40, 10, 60, 30), "Phone", false, "Phone contact")
            },
            EmbeddedAppearance(), "Preferred contact method");

        doc.Structure.AddFormField(radio.Widgets[0], "Email contact");
        doc.Structure.AddFormField(radio.Widgets[1], "Phone contact");

        // The field tooltip is shared by both kids and must remain the group description.
        Assert.Equal("Preferred contact method", radio.Field.Get<PdfString>((PdfName)"TU")!.Value);
        Assert.Equal("Email contact", radio.Widgets[0].NativeObject.Get<PdfString>((PdfName)"TU")!.Value);
        Assert.Equal("Email contact", radio.Widgets[0].NativeObject.Get<PdfString>(PdfName.Contents)!.Value);
        Assert.Equal("Phone contact", radio.Widgets[1].NativeObject.Get<PdfString>((PdfName)"TU")!.Value);
        Assert.Equal("Phone contact", radio.Widgets[1].NativeObject.Get<PdfString>(PdfName.Contents)!.Value);
        var options = radio.Field.Get<PdfArray>((PdfName)"Opt");
        Assert.NotNull(options);
        Assert.Equal(
            new[] { "Email", "Phone" },
            options!.Select(x => x.Resolve().GetAs<PdfString>().Value));
        Assert.Equal(radio.Widgets.Count, options.Count);
    }

    [Fact]
    public void AddFormField_StillSuppliesFieldTooltipWhenFactoryLeftItEmpty()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        var widget = AnnotationFactory.CreateTextWidget(
            doc, page, new PdfRect<double>(10, 10, 110, 30), "name", EmbeddedAppearance());

        doc.Structure.AddFormField(widget, "Full name");

        Assert.Equal("Full name", widget.Field.Get<PdfString>((PdfName)"TU")!.Value);
        Assert.Equal("Full name", widget.NativeObject.Get<PdfString>((PdfName)"TU")!.Value);
    }

    [Fact]
    public void PostSerializationValidation_IsSkippedWithoutAccessibilitySetup()
    {
        var context = ParsingContext.Reset();
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        doc.Structure.AddParagraph("untagged content");
        page.AddAnnotation(new PdfDictionary
        {
            [PdfName.TypeName] = PdfName.Annot,
            [PdfName.Subtype] = (PdfName)"Square",
            [PdfName.Rect] = new PdfArray { new PdfIntNumber(0), new PdfIntNumber(0), new PdfIntNumber(10), new PdfIntNumber(10) }
        });

        doc.SaveTo(new MemoryStream());

        Assert.Equal(0, context.WarningCount);
    }

    [Fact]
    public void TextAppearance_WrapsVariableTextInClippedTxMarkedContent()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        var widget = AnnotationFactory.CreateTextWidget(
            doc, page, new PdfRect<double>(10, 10, 60, 30), "f", EmbeddedAppearance(), "value", "tip");

        var appearance = widget.NativeObject.Get<PdfDictionary>((PdfName)"AP")!.Get<PdfStream>(PdfName.N)!;
        var content = System.Text.Encoding.ASCII.GetString(appearance.Contents.GetDecodedData());

        var order = new[] { "q", "/Tx BMC", "W", "n", "ET", "Q", "EMC" };
        var position = -1;
        foreach (var token in order)
        {
            var next = content.IndexOf(token, position + 1, StringComparison.Ordinal);
            Assert.True(next > position, $"'{token}' missing or out of order in:\n{content}");
            position = next;
        }
    }

    [Fact]
    public void BindFormXObject_RejectsBindingOneFormToTwoElements()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        var formWriter = new PdfLexer.Writing.FormWriter(50, 50);
        formWriter.SetFillRGB(0, 0, 0).Rect(0, 0, 10, 10).Fill();
        var form = formWriter.Complete();

        doc.Structure.AddFigure("first", "first figure").BindFormXObject(form, page);
        var second = doc.Structure.AddFigure("second", "second figure");

        Assert.Throws<PdfAccessibilityConformanceException>(() => second.BindFormXObject(form, page));
    }

    private static FormFieldAppearanceOptions EmbeddedAppearance()
    {
        var testDir = PathUtil.GetPathFromSegmentOfCurrent("test");
        var path = Path.Combine(testDir, "Roboto-Regular.ttf");
        return new FormFieldAppearanceOptions
        {
            Font = TrueTypeFont.CreateWritableFont(File.ReadAllBytes(path)),
            FontSize = 10
        };
    }
}
