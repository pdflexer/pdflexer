using System;
using System.IO;
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

    private static FormFieldAppearanceOptions EmbeddedAppearance()
    {
        var path = File.Exists("/workspace/test/Roboto-Regular.ttf")
            ? "/workspace/test/Roboto-Regular.ttf"
            : "../../../../test/Roboto-Regular.ttf";
        return new FormFieldAppearanceOptions
        {
            Font = TrueTypeFont.CreateWritableFont(File.ReadAllBytes(path)),
            FontSize = 10
        };
    }
}
