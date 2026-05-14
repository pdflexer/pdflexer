using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PdfLexer.DOM;
using Xunit;

namespace PdfLexer.Tests;

public class AccessibilityComplianceTests
{
    private const AccessibilityFixtureCoverage RequiredCoverage =
        AccessibilityFixtureCoverage.DocumentSetup |
        AccessibilityFixtureCoverage.Headings |
        AccessibilityFixtureCoverage.Lists |
        AccessibilityFixtureCoverage.Tables |
        AccessibilityFixtureCoverage.Links |
        AccessibilityFixtureCoverage.Figures |
        AccessibilityFixtureCoverage.Forms |
        AccessibilityFixtureCoverage.Retagging |
        AccessibilityFixtureCoverage.Navigation |
        AccessibilityFixtureCoverage.UnicodeText |
        AccessibilityFixtureCoverage.XObjects |
        AccessibilityFixtureCoverage.MultiPage |
        AccessibilityFixtureCoverage.Artifacts;

    [Fact]
    public void Accessibility_Fixture_Generator_Writes_All_Scenarios_To_Known_Path()
    {
        var fixtures = AccessibilityFixtureGenerator.GenerateAll();

        var expectedBaseNames = new[]
        {
            AccessibilityFixtureGenerator.AccessibleReportFixtureBaseName,
            AccessibilityFixtureGenerator.RetaggedNavigationFixtureBaseName,
            AccessibilityFixtureGenerator.FillableFormFixtureBaseName,
            AccessibilityFixtureGenerator.ReusedImageFixtureBaseName,
            AccessibilityFixtureGenerator.TaggedFormXObjectFixtureBaseName
        };

        var expectedFileNames = expectedBaseNames
            .SelectMany(b => new[]
            {
                AccessibilityFixtureGenerator.GetFixtureFileName(b, PdfUaProfile.PdfUa1),
                AccessibilityFixtureGenerator.GetFixtureFileName(b, PdfUaProfile.PdfUa2)
            });

        Assert.Equal(expectedFileNames.OrderBy(x => x), fixtures.Select(x => x.FileName).OrderBy(x => x));

        Assert.Equal(6, fixtures.Count(x => x.Kind == AccessibilityFixtureKind.Anchor));
        Assert.Equal(4, fixtures.Count(x => x.Kind == AccessibilityFixtureKind.Focused));
        Assert.Equal(5, fixtures.Count(x => x.Profile == PdfUaProfile.PdfUa1));
        Assert.Equal(5, fixtures.Count(x => x.Profile == PdfUaProfile.PdfUa2));

        var aggregateCoverage = fixtures.Aggregate(
            AccessibilityFixtureCoverage.None,
            (current, fixture) => current | fixture.Coverage);
        Assert.True(
            (aggregateCoverage & RequiredCoverage) == RequiredCoverage,
            $"Fixture corpus is missing coverage for: {RequiredCoverage & ~aggregateCoverage}");

        foreach (var fixture in fixtures)
        {
            Assert.True(File.Exists(fixture.Path), fixture.Path);

            using var document = PdfDocument.Open(fixture.Bytes);

            var sb = new StringBuilder();
            foreach (var pg in document.Pages)
            {
                sb.AppendLine("Page start");
                sb.AppendLine();
                sb.Append(pg.DumpDecodedContents());
                sb.AppendLine();
            }
            File.WriteAllText(fixture.Path + ".txt", sb.ToString());
            AccessibilityIntegrityAssert.HasDocumentSetup(document, fixture.Profile);
            AccessibilityIntegrityAssert.HasBasicStructureIntegrity(document);
            AccessibilityIntegrityAssert.HasValidTableAndListHierarchy(document);
            AccessibilityIntegrityAssert.HasOnlyTaggedOrArtifactContent(document);
            AssertFixtureContract(fixture, document, fixture.Bytes);
        }
    }

    private static void AssertFixtureContract(GeneratedAccessibilityFixture fixture, PdfDocument document, byte[] bytes)
    {
        if (fixture.Profile == PdfUaProfile.PdfUa2)
        {
            AccessibilityIntegrityAssert.HasPdf20RootNamespace(document);
            Assert.StartsWith("%PDF-2.0", System.Text.Encoding.ASCII.GetString(bytes.Take(8).ToArray()));
        }

        if (fixture.Coverage.HasFlag(AccessibilityFixtureCoverage.MultiPage))
        {
            Assert.True(document.Pages.Count > 1, $"{fixture.FileName} should span multiple pages.");
        }

        if (fixture.Coverage.HasFlag(AccessibilityFixtureCoverage.Headings))
        {
            Assert.Contains(
                AccessibilityIntegrityAssert.GetStructureElements(document),
                x => IsHeading(x.Get<PdfName>(PdfName.S)));
        }

        if (fixture.Coverage.HasFlag(AccessibilityFixtureCoverage.Lists))
        {
            var listElements = AccessibilityIntegrityAssert.GetStructureElements(document)
                .Where(x => x.Get<PdfName>(PdfName.S) == PdfName.L)
                .ToList();
            Assert.NotEmpty(listElements);
            Assert.Contains(
                listElements.SelectMany(GetAttributes),
                x => x.Get<PdfName>(PdfName.O) == PdfName.List && x.ContainsKey(PdfName.ListNumbering));
        }

        if (fixture.Coverage.HasFlag(AccessibilityFixtureCoverage.Tables))
        {
            var tableElements = AccessibilityIntegrityAssert.GetStructureElements(document)
                .Where(x =>
                    x.Get<PdfName>(PdfName.S) == PdfName.Table ||
                    x.Get<PdfName>(PdfName.S) == (PdfName)"TH" ||
                    x.Get<PdfName>(PdfName.S) == (PdfName)"TD")
                .ToList();
            Assert.NotEmpty(tableElements);
            var tableAttributes = tableElements.SelectMany(GetAttributes).Where(x => x.Get<PdfName>(PdfName.O) == PdfName.Table).ToList();
            Assert.Contains(tableAttributes, x => x.ContainsKey(PdfName.Summary));
            Assert.Contains(tableAttributes, x => x.ContainsKey(PdfName.Scope) || x.ContainsKey(PdfName.Headers));
        }

        if (fixture.Coverage.HasFlag(AccessibilityFixtureCoverage.Links))
        {
            Assert.Contains(
                AccessibilityIntegrityAssert.GetStructureElements(document),
                x => x.Get<PdfName>(PdfName.S) == PdfName.Link);
            Assert.Contains(AccessibilityIntegrityAssert.GetAnnotations(document, PdfName.Link), x => x.ContainsKey(PdfName.StructParent));
            AccessibilityIntegrityAssert.HasAccessibleLinkDescriptions(document);
            if (fixture.Profile == PdfUaProfile.PdfUa2)
            {
                AccessibilityIntegrityAssert.HasStructureDestinationLinks(document);
            }
        }

        if (fixture.Coverage.HasFlag(AccessibilityFixtureCoverage.Figures))
        {
            Assert.Contains(
                AccessibilityIntegrityAssert.GetStructureElements(document),
                x => x.Get<PdfName>(PdfName.S) == PdfName.Figure && !string.IsNullOrWhiteSpace(x.Get<PdfString>(PdfName.Alt)?.Value));
        }

        if (fixture.Coverage.HasFlag(AccessibilityFixtureCoverage.Forms))
        {
            Assert.Contains(
                AccessibilityIntegrityAssert.GetStructureElements(document),
                x => x.Get<PdfName>(PdfName.S) == PdfName.Form);
            Assert.Contains(
                AccessibilityIntegrityAssert.GetAnnotations(document, PdfName.Widget),
                x =>
                    x.ContainsKey(PdfName.StructParent) &&
                    x[PdfName.Parent].Resolve().GetAs<PdfDictionary>().Get<PdfString>(PdfName.TU) != null);
        }

        if (fixture.Coverage.HasFlag(AccessibilityFixtureCoverage.Navigation))
        {
            Assert.Contains(
                AccessibilityIntegrityAssert.GetAnnotations(document, PdfName.Link),
                x =>
                    x.ContainsKey(PdfName.Dest) ||
                    (x.Get<PdfDictionary>(PdfName.A)?.ContainsKey(PdfName.D) ?? false));
        }

        if (fixture.Coverage.HasFlag(AccessibilityFixtureCoverage.UnicodeText))
        {
            var extracted = AccessibilityIntegrityAssert.ExtractTextWithPdfPig(bytes);
            Assert.True(
                extracted.Any(x => x > 127),
                $"{fixture.FileName} should expose non-ASCII extracted text. Extracted text was: {extracted}");
            Assert.True(HasFontWithToUnicode(document), $"{fixture.FileName} should expose a ToUnicode-backed font resource.");
        }

        if (fixture.Coverage.HasFlag(AccessibilityFixtureCoverage.XObjects))
        {
            Assert.True(HasStructuredXObject(document), $"{fixture.FileName} should include an XObject with structure linkage.");
        }

        if (fixture.Coverage.HasFlag(AccessibilityFixtureCoverage.Retagging))
        {
            var names = document.Catalog.Get<PdfDictionary>(PdfName.Names);
            Assert.NotNull(names);
            var dests = names![PdfName.Dests].Resolve().GetAs<PdfDictionary>();
            var nameEntries = dests.Get<PdfArray>(PdfName.Names);
            Assert.NotNull(nameEntries);
            Assert.Contains(nameEntries!, x => x.GetAsOrNull<PdfString>()?.Value == "chapter-1");
        }

        if (fixture.Coverage.HasFlag(AccessibilityFixtureCoverage.Artifacts))
        {
            Assert.True(
                document.Pages.Any(HasArtifactContent),
                $"{fixture.FileName} should include explicit Artifact marked-content.");
        }
    }

    private static IEnumerable<PdfDictionary> GetAttributes(PdfDictionary structElement)
    {
        if (!structElement.TryGetValue(PdfName.A, out var attributes) || attributes == null)
        {
            yield break;
        }

        var resolved = attributes.Resolve();
        if (resolved.Type == PdfObjectType.DictionaryObj)
        {
            yield return resolved.GetAs<PdfDictionary>();
            yield break;
        }

        if (resolved.Type != PdfObjectType.ArrayObj)
        {
            yield break;
        }

        foreach (var item in resolved.GetAs<PdfArray>())
        {
            if (item.Resolve().Type == PdfObjectType.DictionaryObj)
            {
                yield return item.Resolve().GetAs<PdfDictionary>();
            }
        }
    }

    private static bool HasFontWithToUnicode(PdfDocument document)
    {
        foreach (var page in document.Pages)
        {
            var fonts = page.Resources.Get<PdfDictionary>(PdfName.Font);
            if (fonts == null)
            {
                continue;
            }

            if (fonts.Values
                .Select(x => x.Resolve().GetAs<PdfDictionary>())
                .Any(x => x.Get<PdfStream>(PdfName.ToUnicode) != null))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasStructuredXObject(PdfDocument document)
    {
        foreach (var page in document.Pages)
        {
            var xObjects = page.Resources.Get<PdfDictionary>(PdfName.XObject);
            if (xObjects == null)
            {
                continue;
            }

            if (xObjects.Values
                .Select(x => x.Resolve().GetAsOrNull<PdfStream>())
                .Any(x => x != null && (x.Dictionary.ContainsKey(PdfName.StructParent) || x.Dictionary.ContainsKey(PdfName.StructParents))))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasArtifactContent(PdfPage page)
    {
        return page.GetContentNodes()
            .OfType<PdfLexer.Content.Model.MarkedContentGroup<double>>()
            .Any(ContainsArtifactGroup);
    }

    private static bool ContainsArtifactGroup(PdfLexer.Content.Model.MarkedContentGroup<double> group)
    {
        if (group.Tag.Name == PdfName.Artifact)
        {
            return true;
        }

        return group.Children
            .OfType<PdfLexer.Content.Model.MarkedContentGroup<double>>()
            .Any(ContainsArtifactGroup);
    }

    private static bool IsHeading(PdfName structureType)
    {
        if (structureType == null)
        {
            return false;
        }

        var value = structureType.Value;
        return value.Length == 2 && value[0] == 'H' && char.IsDigit(value[1]);
    }
}
