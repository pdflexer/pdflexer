using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Images;
using PdfLexer.Writing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PdfLexer.Tests;

internal static class AccessibilityFixtureGenerator
{
    private static readonly object GenerateLock = new();
    private static readonly AccessibilityFixtureBlueprint[] Blueprints =
    {
        new(
            "Accessible report",
            AccessibleReportFixtureBaseName,
            AccessibilityFixtureKind.Anchor,
            AccessibilityFixtureCoverage.DocumentSetup |
            AccessibilityFixtureCoverage.Headings |
            AccessibilityFixtureCoverage.Lists |
            AccessibilityFixtureCoverage.Tables |
            AccessibilityFixtureCoverage.Links |
            AccessibilityFixtureCoverage.Figures |
            AccessibilityFixtureCoverage.Navigation |
            AccessibilityFixtureCoverage.UnicodeText |
            AccessibilityFixtureCoverage.Artifacts |
            AccessibilityFixtureCoverage.MultiPage,
            CreateAccessibleReportFixture),
        new(
            "Retagged navigation",
            RetaggedNavigationFixtureBaseName,
            AccessibilityFixtureKind.Anchor,
            AccessibilityFixtureCoverage.DocumentSetup |
            AccessibilityFixtureCoverage.Headings |
            AccessibilityFixtureCoverage.Links |
            AccessibilityFixtureCoverage.Retagging |
            AccessibilityFixtureCoverage.Navigation |
            AccessibilityFixtureCoverage.MultiPage,
            CreateRetaggedNavigationFixture),
        new(
            "Fillable form",
            FillableFormFixtureBaseName,
            AccessibilityFixtureKind.Anchor,
            AccessibilityFixtureCoverage.DocumentSetup |
            AccessibilityFixtureCoverage.Headings |
            AccessibilityFixtureCoverage.Forms,
            CreateFillableFormFixture),
        new(
            "Reused image",
            ReusedImageFixtureBaseName,
            AccessibilityFixtureKind.Focused,
            AccessibilityFixtureCoverage.DocumentSetup |
            AccessibilityFixtureCoverage.Figures |
            AccessibilityFixtureCoverage.XObjects |
            AccessibilityFixtureCoverage.MultiPage,
            CreateReusedImageFixture),
        new(
            "Tagged form XObject",
            TaggedFormXObjectFixtureBaseName,
            AccessibilityFixtureKind.Focused,
            AccessibilityFixtureCoverage.DocumentSetup |
            AccessibilityFixtureCoverage.XObjects,
            CreateTaggedFormXObjectFixture)
    };

    public const string AccessibleReportFixtureBaseName = "accessible-report";
    public const string RetaggedNavigationFixtureBaseName = "retagged-navigation";
    public const string FillableFormFixtureBaseName = "fillable-form";
    public const string ReusedImageFixtureBaseName = "reused-image";
    public const string TaggedFormXObjectFixtureBaseName = "tagged-form-xobject";

    public const string AccessibleReportFixtureFileName = AccessibleReportFixtureBaseName + "-ua2.pdf";
    public const string RetaggedNavigationFixtureFileName = RetaggedNavigationFixtureBaseName + "-ua2.pdf";
    public const string FillableFormFixtureFileName = FillableFormFixtureBaseName + "-ua2.pdf";
    public const string ReusedImageFixtureFileName = ReusedImageFixtureBaseName + "-ua2.pdf";
    public const string TaggedFormXObjectFixtureFileName = TaggedFormXObjectFixtureBaseName + "-ua2.pdf";

    public const string NewDocumentFixtureFileName = AccessibleReportFixtureFileName;
    public const string RetaggedDocumentFixtureFileName = RetaggedNavigationFixtureFileName;
    public const string TaggedWidgetFixtureFileName = FillableFormFixtureFileName;

    public static string FixtureRootPath
    {
        get
        {
            var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
            return Path.Combine(tp, "results", "accessibility-fixtures");
        }
    }

    public static string GetFixturePath(string fileName) => Path.Combine(FixtureRootPath, fileName);

    public static string GetFixtureFileName(string baseName, PdfUaProfile profile)
        => $"{baseName}-{(profile == PdfUaProfile.PdfUa1 ? "ua1" : "ua2")}.pdf";

    public static IReadOnlyList<GeneratedAccessibilityFixture> GenerateAll()
    {
        lock (GenerateLock)
        {
            Directory.CreateDirectory(FixtureRootPath);
            var results = new List<GeneratedAccessibilityFixture>(Blueprints.Length * 2);
            foreach (var bp in Blueprints)
            {
                results.Add(SaveFixture(bp, PdfUaProfile.PdfUa1));
                results.Add(SaveFixture(bp, PdfUaProfile.PdfUa2));
            }
            return results;
        }
    }

    public static IReadOnlyList<GeneratedAccessibilityFixture> GenerateAll(PdfUaProfile profile)
    {
        lock (GenerateLock)
        {
            Directory.CreateDirectory(FixtureRootPath);
            return Blueprints.Select(bp => SaveFixture(bp, profile)).ToArray();
        }
    }

    private static GeneratedAccessibilityFixture SaveFixture(AccessibilityFixtureBlueprint blueprint, PdfUaProfile profile)
    {
        using (var document = blueprint.CreateDocument(profile))
        {
            var fileName = GetFixtureFileName(blueprint.BaseName, profile);
            var path = GetFixturePath(fileName);
            var bytes = document.Save();
            File.WriteAllBytes(path, bytes);
            return new GeneratedAccessibilityFixture(
                blueprint.Name,
                fileName,
                path,
                blueprint.Kind,
                profile,
                blueprint.Coverage,
                bytes);
        }
    }

    private static PdfDocument CreateAccessibleReportFixture(PdfUaProfile profile)
    {
        using var image = new Image<Rgba32>(48, 48);
        image.Mutate(x => x.BackgroundColor(Color.Black));
        var reportImage = image.CreatePdfImage();
        var unicodeFont = LoadUnicodeFont();

        var doc = PdfDocument.Create();
        var page1 = doc.AddPage();
        var page2 = doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "Fixture: Accessible Report", profile, strictConformance: true);

        var report = doc.Structure.AddPart("Accessibility Annual Report");
        var overview = report.AddSection("Overview");
        var title = overview.AddHeader(1, "Accessibility Annual Report");
        var intro = title.Back()
            .AddParagraph("Executive summary")
            .ActualText("Executive summary for the annual accessibility report");
        var findings = intro.Back()
            .AddList("Key findings")
            .ListNumbering(StructureListNumbering.Decimal);

        var firstFinding = findings.AddListItem("Finding one");
        var firstLabel = firstFinding.AddLabel("1.");
        var firstBody = firstLabel.Back().AddListBody("Keyboard support improved");

        var secondFinding = findings.AddListItem("Finding two");
        var secondLabel = secondFinding.AddLabel("2.");
        var secondBody = secondLabel.Back().AddListBody("Alt text coverage is now complete");

        var figure = findings.Back().AddFigure("Audit progress chart", "Bar chart showing accessibility audit progress");
        figure.BindImage(reportImage, page1);

        var regional = figure.Back().AddSection("Regional Results");
        var regionalHeading = regional.AddHeader(2, "Regional Results");
        var regionalParagraph = regionalHeading.Back()
            .AddParagraph("Résumé et café pour Montréal")
            .Lang("fr-CA");
        var table = regionalParagraph.Back()
            .AddTable("Regional accessibility status")
            .TableSummary("Accessibility readiness by region and audit status.");

        var headerGroup = table.AddTableHead();
        var headerRow = headerGroup.AddRow();
        var regionHeader = headerRow.AddHeaderCell()
            .ElementId("report-region")
            .TableScope(StructureScope.Column);
        var statusHeader = regionHeader.Back().AddHeaderCell()
            .ElementId("report-status")
            .TableScope(StructureScope.Column);

        var bodyGroup = statusHeader.Back().Back().Back().AddTableBody();
        var northRow = bodyGroup.AddRow();
        var northRegion = northRow.AddDataCell().TableHeaders("report-region");
        var northStatus = northRegion.Back().AddDataCell().TableHeaders("report-status");
        var montrealRow = northStatus.Back().Back().AddRow();
        var montrealRegion = montrealRow.AddDataCell().TableHeaders("report-region");
        var montrealStatus = montrealRegion.Back().AddDataCell().TableHeaders("report-status");

        var jumpLink = regional.Back().AddLink(
            page1,
            new PdfRect<double>(40, 528, 220, 540),
            regionalHeading.GetNode(),
            "Jump to regional results",
            "Jump to regional results",
            "Jump to regional results");

        var externalLink = jumpLink.Back().AddLinkAction(
            page1,
            new PdfRect<double>(40, 498, 260, 510),
            new PdfDictionary
            {
                [PdfName.S] = PdfName.URI,
                [(PdfName)"URI"] = new PdfString("https://example.com/accessibility")
            },
            "Read the full audit methodology",
            "Read the full audit methodology");

        using (var writer = page1.GetWriter())
        {
            writer.BeginMarkedContent(title.GetNode());
            writer.Font(unicodeFont, 16).TextMove(40, 760).Text("Accessibility Annual Report");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(intro.GetNode());
            writer.Font(unicodeFont, 12).TextMove(40, 734).Text("Executive summary");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(firstLabel.GetNode());
            writer.Font(unicodeFont, 12).TextMove(40, 704).Text("1.");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(firstBody.GetNode());
            writer.Font(unicodeFont, 12).TextMove(60, 704).Text("Keyboard support improved");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(secondLabel.GetNode());
            writer.Font(unicodeFont, 12).TextMove(40, 682).Text("2.");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(secondBody.GetNode());
            writer.Font(unicodeFont, 12).TextMove(60, 682).Text("Alt text coverage is now complete");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(jumpLink.GetNode());
            writer.Font(unicodeFont, 12).TextMove(40, 528).Text("Jump to regional results");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(externalLink.GetNode());
            writer.Font(unicodeFont, 12).TextMove(40, 498).Text("Read the full audit methodology");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(figure.GetNode());
            writer.Image(reportImage, 40, 560, 64, 64);
            writer.EndMarkedContent();

            writer.BeginArtifact(PdfName.Pagination);
            writer.Font(unicodeFont, 10).TextMove(520, 20).Text("Page 1");
            writer.EndMarkedContent();
        }

        using (var writer = page2.GetWriter())
        {
            writer.BeginMarkedContent(regionalHeading.GetNode());
            writer.Font(unicodeFont, 16).TextMove(40, 760).Text("Regional Results");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(regionalParagraph.GetNode());
            writer.Font(unicodeFont, 12).TextMove(40, 736).Text("Résumé et café pour Montréal");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(regionHeader.GetNode());
            writer.Font(unicodeFont, 12).TextMove(40, 702).Text("Region");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(statusHeader.GetNode());
            writer.Font(unicodeFont, 12).TextMove(180, 702).Text("Status");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(northRegion.GetNode());
            writer.Font(unicodeFont, 12).TextMove(40, 680).Text("North");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(northStatus.GetNode());
            writer.Font(unicodeFont, 12).TextMove(180, 680).Text("Complete");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(montrealRegion.GetNode());
            writer.Font(unicodeFont, 12).TextMove(40, 658).Text("Montreal");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(montrealStatus.GetNode());
            writer.Font(unicodeFont, 12).TextMove(180, 658).Text("In review");
            writer.EndMarkedContent();

            writer.BeginArtifact(PdfName.Pagination);
            writer.Font(unicodeFont, 10).TextMove(520, 20).Text("Page 2");
            writer.EndMarkedContent();
        }

        return doc;
    }

    private static PdfDocument CreateRetaggedNavigationFixture(PdfUaProfile profile)
    {
        var unicodeFont = LoadUnicodeFont();
        using var source = PdfDocument.Create();
        var contentsPage = source.AddPage();
        var chapterPage = source.AddPage();
        var destination = new PdfArray { chapterPage.NativeObject.Indirect(), PdfName.XYZ, 0, 760, PdfNull.Value };
        source.Catalog[PdfName.Names] = new PdfDictionary
        {
            [PdfName.Dests] = new PdfDictionary
            {
                [PdfName.Names] = new PdfArray
                {
                    new PdfString("chapter-1"),
                    destination
                }
            }
        };

        var reopened = PdfDocument.Open(source.Save());
        reopened.ApplyAccessibilitySetup("en-US", "Fixture: Retagged Navigation", profile, strictConformance: true);

        var manual = reopened.Structure.AddPart("Retagged manual");
        var contents = manual.AddSection("Contents");
        var contentsHeading = contents.AddHeader(1, "Contents");
        var chapter = contents.Back().AddSection("Chapter 1");
        var chapterHeading = chapter.AddHeader(2, "Chapter 1");
        var chapterParagraph = chapterHeading.Back().AddParagraph("Retagged body content");
        var contentsParagraph = contentsHeading.Back().AddParagraph("Open Chapter 1");
        var chapterLink = contentsParagraph.Back().AddLink(
            reopened.Pages[0],
            new PdfRect<double>(40, 624, 190, 642),
            chapterHeading.GetNode(),
            "Jump to Chapter 1",
            "Jump to Chapter 1",
            "Jump to Chapter 1");

        using (var writer = reopened.Pages[0].GetWriter())
        {
            writer.BeginMarkedContent(contentsHeading.GetNode());
            writer.Font(unicodeFont, 16).TextMove(40, 680).Text("Contents");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(contentsParagraph.GetNode());
            writer.Font(unicodeFont, 12).TextMove(40, 652).Text("Open Chapter 1");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(chapterLink.GetNode());
            writer.Font(unicodeFont, 12).TextMove(40, 624).Text("Jump to Chapter 1");
            writer.EndMarkedContent();
        }

        using (var writer = reopened.Pages[1].GetWriter())
        {
            writer.BeginMarkedContent(chapterHeading.GetNode());
            writer.Font(unicodeFont, 16).TextMove(40, 680).Text("Chapter 1");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(chapterParagraph.GetNode());
            writer.Font(unicodeFont, 12).TextMove(40, 652).Text("Retagged body content");
            writer.EndMarkedContent();
        }

        return reopened;
    }

    private static PdfDocument CreateFillableFormFixture(PdfUaProfile profile)
    {
        var unicodeFont = LoadUnicodeFont();
        var doc = PdfDocument.Create();
        var page = doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "Fixture: Fillable Form", profile, strictConformance: true);

        var formSection = doc.Structure.AddSection("Volunteer Registration");
        var heading = formSection.AddHeader(1, "Volunteer Registration");
        var instructions = heading.Back().AddParagraph("Complete the form to request follow-up.");
        var nameLabel = instructions.Back().AddParagraph("Full name");
        var nameField = nameLabel.Back().AddFormField(
            doc,
            page,
            new PdfRect<double>(140, 695, 320, 718),
            "full_name",
            "Full name",
            "Full name",
            print: true);
        var emailLabel = nameField.Back().AddParagraph("Email address");
        var emailField = emailLabel.Back().AddFormField(
            doc,
            page,
            new PdfRect<double>(140, 645, 320, 668),
            "email",
            "Email address",
            "Email address",
            print: true);
        var confirmation = emailField.Back().AddParagraph("Required fields are announced with their tooltips.");

        using (var writer = page.GetWriter())
        {
            writer.BeginMarkedContent(heading.GetNode());
            writer.Font(unicodeFont, 16).TextMove(40, 760).Text("Volunteer Registration");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(instructions.GetNode());
            writer.Font(unicodeFont, 12).TextMove(40, 734).Text("Complete the form to request follow-up.");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(nameLabel.GetNode());
            writer.Font(unicodeFont, 12).TextMove(40, 704).Text("Full name");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(emailLabel.GetNode());
            writer.Font(unicodeFont, 12).TextMove(40, 654).Text("Email address");
            writer.EndMarkedContent();

            writer.BeginMarkedContent(confirmation.GetNode());
            writer.Font(unicodeFont, 12).TextMove(40, 614).Text("Required fields are announced with their tooltips.");
            writer.EndMarkedContent();
        }

        return doc;
    }

    private static PdfDocument CreateReusedImageFixture(PdfUaProfile profile)
    {
        using var image = new Image<Rgba32>(32, 32);
        image.Mutate(x => x.BackgroundColor(Color.Black));
        var xobj = image.CreatePdfImage();

        var doc = PdfDocument.Create();
        var page1 = doc.AddPage();
        var page2 = doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "Fixture: Reused Image", profile, strictConformance: true);

        var figure = doc.Structure.AddFigure("Shared Figure", "Shared accessibility figure");
        figure.BindImage(xobj, page1, page2);

        using (var writer = page1.GetWriter())
        {
            writer.Image(xobj, 40, 680, 32, 32);
        }

        using (var writer = page2.GetWriter())
        {
            writer.Image(xobj, 40, 680, 32, 32);
        }

        return doc;
    }

    private static PdfDocument CreateTaggedFormXObjectFixture(PdfUaProfile profile)
    {
        var unicodeFont = LoadUnicodeFont();
        var doc = PdfDocument.Create();
        var page = doc.AddPage();
        doc.ApplyAccessibilitySetup("en-US", "Fixture: Form XObject", profile, strictConformance: true);

        var parent = doc.Structure.AddParagraph("Form parent");
        var first = parent.AddSpan("One");
        var second = first.Back().AddSpan("Two");

        var formWriter = new FormWriter(100, 40);
        formWriter.BeginMarkedContent(first.GetNode());
        formWriter.Font(unicodeFont, 12).TextMove(0, 20).Text("One");
        formWriter.EndMarkedContent();
        formWriter.BeginMarkedContent(second.GetNode());
        formWriter.Font(unicodeFont, 12).TextMove(40, 20).Text("Two");
        formWriter.EndMarkedContent();

        var form = formWriter.Complete();
        parent.BindFormXObject(form, page);

        using (var writer = page.GetWriter())
        {
            writer.Form(form, 40, 680);
        }

        return doc;
    }

    private static IWritableFont LoadUnicodeFont()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var fontPath = Path.Combine(tp, "Roboto-Regular.ttf");
        return TrueTypeFont.CreateType0WritableFont(File.ReadAllBytes(fontPath));
    }
}

[Flags]
internal enum AccessibilityFixtureCoverage
{
    None = 0,
    DocumentSetup = 1 << 0,
    Headings = 1 << 1,
    Lists = 1 << 2,
    Tables = 1 << 3,
    Links = 1 << 4,
    Figures = 1 << 5,
    Forms = 1 << 6,
    Retagging = 1 << 7,
    Navigation = 1 << 8,
    UnicodeText = 1 << 9,
    XObjects = 1 << 10,
    MultiPage = 1 << 11,
    Artifacts = 1 << 12
}

internal enum AccessibilityFixtureKind
{
    Anchor,
    Focused
}

internal sealed record GeneratedAccessibilityFixture(
    string Name,
    string FileName,
    string Path,
    AccessibilityFixtureKind Kind,
    PdfUaProfile Profile,
    AccessibilityFixtureCoverage Coverage,
    byte[] Bytes);

internal sealed record AccessibilityFixtureBlueprint(
    string Name,
    string BaseName,
    AccessibilityFixtureKind Kind,
    AccessibilityFixtureCoverage Coverage,
    Func<PdfUaProfile, PdfDocument> CreateDocument);
