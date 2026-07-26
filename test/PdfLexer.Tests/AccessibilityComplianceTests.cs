using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PdfLexer.Content.Model;
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
        AccessibilityFixtureCoverage.Artifacts |
        AccessibilityFixtureCoverage.UnicodeMetadata |
        AccessibilityFixtureCoverage.MultiPassWriting |
        AccessibilityFixtureCoverage.Annotations;

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
            AccessibilityFixtureGenerator.TaggedFormXObjectFixtureBaseName,
            AccessibilityFixtureGenerator.UnicodeMetadataFixtureBaseName,
            AccessibilityFixtureGenerator.MultiPassWritingFixtureBaseName,
            AccessibilityFixtureGenerator.TaggedAnnotationFixtureBaseName
        };

        var expectedFileNames = expectedBaseNames
            .SelectMany(b => new[]
            {
                AccessibilityFixtureGenerator.GetFixtureFileName(b, PdfUaProfile.PdfUa1),
                AccessibilityFixtureGenerator.GetFixtureFileName(b, PdfUaProfile.PdfUa2)
            });

        Assert.Equal(expectedFileNames.OrderBy(x => x), fixtures.Select(x => x.FileName).OrderBy(x => x));

        Assert.Equal(6, fixtures.Count(x => x.Kind == AccessibilityFixtureKind.Anchor));
        Assert.Equal(10, fixtures.Count(x => x.Kind == AccessibilityFixtureKind.Focused));
        Assert.Equal(8, fixtures.Count(x => x.Profile == PdfUaProfile.PdfUa1));
        Assert.Equal(8, fixtures.Count(x => x.Profile == PdfUaProfile.PdfUa2));

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
            AssertPdf6CompatibleTable(document);
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
            var figures = AccessibilityIntegrityAssert.GetStructureElements(document)
                .Where(x => x.Get<PdfName>(PdfName.S) == PdfName.Figure)
                .ToList();
            Assert.Contains(figures, x => !string.IsNullOrWhiteSpace(x.Get<PdfString>(PdfName.Alt)?.Value));

            foreach (var figure in figures.Where(x => x.ContainsKey(PdfName.Pg)))
            {
                var layout = Assert.Single(
                    GetAttributes(figure),
                    x => x.Get<PdfName>(PdfName.O) == PdfName.Layout);
                var bbox = layout.Get<PdfArray>(PdfName.BBox);
                Assert.NotNull(bbox);
                Assert.Equal(4, bbox!.Count);
                Assert.All(bbox, x => Assert.IsAssignableFrom<PdfNumber>(x.Resolve()));
                Assert.True((decimal)(PdfNumber)bbox[0] < (decimal)(PdfNumber)bbox[2]);
                Assert.True((decimal)(PdfNumber)bbox[1] < (decimal)(PdfNumber)bbox[3]);
            }
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
            AssertCoherentAcroFormGraph(document);
            AssertRadioOptionMapping(document);
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

        if (fixture.Coverage.HasFlag(AccessibilityFixtureCoverage.UnicodeMetadata))
        {
            Assert.Contains(
                AccessibilityIntegrityAssert.GetStructureElements(document),
                x =>
                    x.Get<PdfName>(PdfName.S) == PdfName.Figure &&
                    x.Get<PdfString>(PdfName.Alt)?.Value == AccessibilityFixtureGenerator.UnicodeMetadataValue &&
                    x.Get<PdfString>(PdfName.ActualText)?.Value == AccessibilityFixtureGenerator.UnicodeMetadataValue &&
                    x.Get<PdfString>(PdfName.E)?.Value == AccessibilityFixtureGenerator.UnicodeMetadataValue);
        }

        if (fixture.Coverage.HasFlag(AccessibilityFixtureCoverage.MultiPassWriting))
        {
            var mcids = document.Pages
                .SelectMany(page => GetMarkedContentIds(page.GetContentNodes()))
                .OrderBy(x => x)
                .ToArray();
            Assert.Equal(new[] { 0, 1 }, mcids);
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

    private static void AssertPdf6CompatibleTable(PdfDocument document)
    {
        var table = Assert.Single(
            AccessibilityIntegrityAssert.GetStructureElements(document),
            x => x.Get<PdfName>(PdfName.S) == PdfName.Table);
        var tableAttributes = GetAttributes(table)
            .Where(x => x.Get<PdfName>(PdfName.O) == PdfName.Table)
            .ToList();
        Assert.Contains(tableAttributes, x => x.ContainsKey(PdfName.Summary));

        var rows = GetStructureChildren(table);
        Assert.Equal(3, rows.Count);
        Assert.All(rows, x => Assert.Equal((PdfName)"TR", x.Get<PdfName>(PdfName.S)));

        var headerCells = GetStructureChildren(rows[0]);
        Assert.Equal(2, headerCells.Count);
        Assert.All(headerCells, x => Assert.Equal((PdfName)"TH", x.Get<PdfName>(PdfName.S)));
        Assert.Equal("report-region", headerCells[0].Get<PdfString>(PdfName.ID)?.Value);
        Assert.Equal("report-status", headerCells[1].Get<PdfString>(PdfName.ID)?.Value);
        Assert.All(headerCells, cell =>
        {
            var tableAttribute = Assert.Single(
                GetAttributes(cell),
                x => x.Get<PdfName>(PdfName.O) == PdfName.Table);
            Assert.Equal(PdfName.Column, tableAttribute.Get<PdfName>(PdfName.Scope));
        });

        var expectedHeaders = new[] { "report-region", "report-status" };
        foreach (var row in rows.Skip(1))
        {
            var cells = GetStructureChildren(row);
            Assert.Equal(2, cells.Count);
            Assert.All(cells, x => Assert.Equal((PdfName)"TD", x.Get<PdfName>(PdfName.S)));

            for (var index = 0; index < cells.Count; index++)
            {
                var tableAttribute = Assert.Single(
                    GetAttributes(cells[index]),
                    x => x.Get<PdfName>(PdfName.O) == PdfName.Table);
                var headers = tableAttribute.Get<PdfArray>(PdfName.Headers);
                var header = Assert.Single(headers!);
                Assert.Equal(PdfObjectType.StringObj, header.Resolve().Type);
                Assert.Equal(expectedHeaders[index], header.Resolve().GetAs<PdfString>().Value);
            }
        }
    }

    private static IReadOnlyList<PdfDictionary> GetStructureChildren(PdfDictionary element)
    {
        if (!element.TryGetValue(PdfName.K, out var kids) || kids == null)
        {
            return Array.Empty<PdfDictionary>();
        }

        var resolved = kids.Resolve();
        if (resolved.Type == PdfObjectType.DictionaryObj)
        {
            var child = resolved.GetAs<PdfDictionary>();
            return child.Get<PdfName>(PdfName.TYPE) == PdfName.StructElem
                ? new[] { child }
                : Array.Empty<PdfDictionary>();
        }

        if (resolved.Type != PdfObjectType.ArrayObj)
        {
            return Array.Empty<PdfDictionary>();
        }

        return resolved.GetAs<PdfArray>()
            .Select(x => x.Resolve().GetAsOrNull<PdfDictionary>())
            .Where(x => x?.Get<PdfName>(PdfName.TYPE) == PdfName.StructElem)
            .Select(x => x!)
            .ToArray();
    }

    private static void AssertCoherentAcroFormGraph(PdfDocument document)
    {
        var activeWidgets = new Dictionary<PdfDictionary, PdfPage>();
        foreach (var page in document.Pages)
        {
            var annotations = page.NativeObject.Get<PdfArray>(PdfName.Annots);
            if (annotations == null)
            {
                continue;
            }

            foreach (var annotationObject in annotations)
            {
                var annotation = annotationObject.Resolve().GetAsOrNull<PdfDictionary>();
                if (annotation?.Get<PdfName>(PdfName.Subtype) == PdfName.Widget)
                {
                    activeWidgets[annotation] = page;
                }
            }
        }

        var acroForm = document.Catalog.Get<PdfDictionary>((PdfName)"AcroForm");
        var fields = acroForm?.Get<PdfArray>(PdfName.Fields);
        Assert.NotNull(fields);

        var visitedWidgets = new HashSet<PdfDictionary>();
        foreach (var fieldObject in fields!)
        {
            AssertField(fieldObject.Resolve().GetAs<PdfDictionary>(), null);
        }

        Assert.Equal(activeWidgets.Count, visitedWidgets.Count);

        void AssertField(PdfDictionary field, PdfDictionary expectedParent)
        {
            if (expectedParent != null)
            {
                Assert.Same(expectedParent, field[PdfName.Parent].Resolve());
            }

            var kids = field.Get<PdfArray>(PdfName.Kids);
            if (kids == null)
            {
                Assert.Equal(PdfName.Widget, field.Get<PdfName>(PdfName.Subtype));
                AssertWidget(field, expectedParent);
                return;
            }

            foreach (var kidObject in kids)
            {
                var kid = kidObject.Resolve().GetAs<PdfDictionary>();
                if (kid.Get<PdfName>(PdfName.Subtype) == PdfName.Widget)
                {
                    AssertWidget(kid, field);
                }
                else
                {
                    AssertField(kid, field);
                }
            }
        }

        void AssertWidget(PdfDictionary widget, PdfDictionary expectedField)
        {
            Assert.True(activeWidgets.TryGetValue(widget, out var page), "Field /Kids references a widget outside the active page tree.");
            Assert.Same(page!.NativeObject, widget[PdfName.P].Resolve());
            if (expectedField != null)
            {
                Assert.Same(expectedField, widget[PdfName.Parent].Resolve());
            }
            Assert.True(visitedWidgets.Add(widget), "A widget is referenced by more than one field.");
            Assert.Contains(
                AccessibilityIntegrityAssert.GetStructureElements(document),
                element => ContainsObjectReference(element, widget));
        }
    }

    private static void AssertRadioOptionMapping(PdfDocument document)
    {
        var acroForm = document.Catalog.Get<PdfDictionary>((PdfName)"AcroForm");
        var fields = acroForm?.Get<PdfArray>(PdfName.Fields);
        Assert.NotNull(fields);

        var radio = Assert.Single(
            fields!.Select(x => x.Resolve().GetAs<PdfDictionary>()),
            x => x.Get<PdfString>(PdfName.T)?.Value == "contact_method");
        var options = radio.Get<PdfArray>((PdfName)"Opt");
        var kids = radio.Get<PdfArray>(PdfName.Kids);
        Assert.NotNull(options);
        Assert.NotNull(kids);
        Assert.Equal(kids!.Count, options!.Count);
        Assert.Equal(
            new[] { "Email", "Phone" },
            options.Select(x => x.Resolve().GetAs<PdfString>().Value));
    }

    private static bool ContainsObjectReference(PdfDictionary structureElement, PdfDictionary widget)
    {
        if (!structureElement.TryGetValue(PdfName.K, out var kids) || kids == null)
        {
            return false;
        }

        var resolved = kids.Resolve();
        if (resolved.Type == PdfObjectType.DictionaryObj)
        {
            var kid = resolved.GetAs<PdfDictionary>();
            return kid.Get<PdfName>(PdfName.TYPE) == PdfName.OBJR &&
                   ReferenceEquals(kid[PdfName.Obj].Resolve(), widget);
        }

        return resolved.Type == PdfObjectType.ArrayObj &&
               resolved.GetAs<PdfArray>().Any(x =>
                   x.Resolve().GetAsOrNull<PdfDictionary>() is { } kid &&
                   kid.Get<PdfName>(PdfName.TYPE) == PdfName.OBJR &&
                   ReferenceEquals(kid[PdfName.Obj].Resolve(), widget));
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

    private static IEnumerable<int> GetMarkedContentIds(IEnumerable<IContentNode<double>> nodes)
    {
        foreach (var node in nodes)
        {
            if (node is not MarkedContentGroup<double> marked)
            {
                continue;
            }

            var value = marked.Tag.InlineProps?.Get<PdfNumber>(PdfName.MCID) ??
                        marked.Tag.PropList?.Get<PdfNumber>(PdfName.MCID);
            if (value != null)
            {
                yield return (int)value;
            }

            foreach (var child in GetMarkedContentIds(marked.Children))
            {
                yield return child;
            }
        }
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
