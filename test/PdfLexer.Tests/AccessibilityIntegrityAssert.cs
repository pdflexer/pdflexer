using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using Xunit;

namespace PdfLexer.Tests;

internal static class AccessibilityIntegrityAssert
{
    private const string Pdf20NamespaceUri = "http://iso.org/pdf2/ssn";

    public static void HasDocumentSetup(PdfDocument document, PdfUaProfile? expectedProfile = null)
    {
        Assert.NotNull(document.Catalog.Get<PdfString>(PdfName.Lang));

        var viewerPreferences = document.Catalog.Get<PdfDictionary>(PdfName.ViewerPreferences);
        Assert.NotNull(viewerPreferences);
        Assert.Equal(PdfBoolean.True, viewerPreferences![PdfName.DisplayDocTitle]);

        var markInfo = document.Catalog.Get<PdfDictionary>(PdfName.MarkInfo);
        Assert.NotNull(markInfo);
        Assert.Equal(PdfBoolean.True, markInfo![PdfName.Marked]);
        Assert.Equal(PdfBoolean.False, markInfo[PdfName.Suspects]);

        foreach (var page in document.Pages)
        {
            Assert.NotNull(page.NativeObject.Get<PdfName>(PdfName.Tabs));
        }

        var xml = GetMetadataXml(document);
        Assert.Contains("pdfuaid:part", xml);

        if (expectedProfile.HasValue)
        {
            HasPdfUaMetadata(document, expectedProfile.Value);
        }
    }

    public static string GetMetadataXml(PdfDocument document)
    {
        var metadata = document.Catalog[PdfName.Metadata].Resolve().GetAs<PdfStream>();
        return Encoding.UTF8.GetString(metadata.Contents.GetDecodedData());
    }

    public static void HasPdfUaMetadata(PdfDocument document, PdfUaProfile expectedProfile)
    {
        var xml = GetMetadataXml(document);
        Assert.Contains($"<pdfuaid:part>{expectedProfile switch { PdfUaProfile.PdfUa1 => 1, _ => 2 }}</pdfuaid:part>", xml);

        if (expectedProfile == PdfUaProfile.PdfUa2)
        {
            Assert.Contains("<pdfuaid:rev>2024</pdfuaid:rev>", xml);
        }
        else
        {
            Assert.DoesNotContain("<pdfuaid:rev>", xml);
        }
    }

    public static void HasBasicStructureIntegrity(PdfDocument document)
    {
        var structRoot = GetStructTreeRoot(document);
        Assert.NotNull(structRoot[PdfName.ParentTree]);

        var rootKids = structRoot[PdfName.K].Resolve();
        AssertStructParents(rootKids, structRoot);
    }

    public static void HasPdf20RootNamespace(PdfDocument document)
    {
        var documentElement = GetRootDocumentElement(document);
        var ns = documentElement.Get<PdfDictionary>(PdfName.NS);
        Assert.NotNull(ns);
        Assert.Equal(Pdf20NamespaceUri, ns!.Get<PdfString>(PdfName.NS)?.Value);
    }

    public static void HasValidTableAndListHierarchy(PdfDocument document)
    {
        foreach (var element in GetStructureElements(document))
        {
            var type = element.Get<PdfName>(PdfName.S)?.Value;
            if (type == null)
            {
                continue;
            }

            switch (type)
            {
                case "Table":
                    Assert.All(GetChildTypes(element), x => Assert.Contains(x, new[] { "TR", "THead", "TBody", "TFoot", "Caption" }));
                    break;
                case "TR":
                    Assert.All(GetChildTypes(element), x => Assert.Contains(x, new[] { "TH", "TD" }));
                    break;
                case "TH":
                case "TD":
                    Assert.Equal("TR", GetParentType(element));
                    break;
                case "L":
                    Assert.All(GetChildTypes(element), x => Assert.Equal("LI", x));
                    break;
                case "LI":
                    Assert.Equal("L", GetParentType(element));
                    Assert.All(GetChildTypes(element), x => Assert.Contains(x, new[] { "Lbl", "LBody" }));
                    break;
                case "Lbl":
                case "LBody":
                    Assert.Equal("LI", GetParentType(element));
                    break;
            }
        }
    }

    public static void HasAccessibleLinkDescriptions(PdfDocument document)
    {
        Assert.All(GetAnnotations(document, PdfName.Link), x =>
        {
            var contents = x.Get<PdfString>(PdfName.Contents)?.Value;
            Assert.False(string.IsNullOrWhiteSpace(contents));
        });
    }

    public static void HasStructureDestinationLinks(PdfDocument document)
    {
        Assert.Contains(GetAnnotations(document, PdfName.Link), annotation =>
        {
            if (!annotation.TryGetValue(PdfName.Dest, out var dest) || dest == null)
            {
                return false;
            }

            return dest.Resolve().Type == PdfObjectType.ArrayObj &&
                IsStructureDestination(dest.Resolve().GetAs<PdfArray>());
        });
    }

    public static void HasOnlyTaggedOrArtifactContent(PdfDocument document)
    {
        var parentTree = GetStructTreeRoot(document)[PdfName.ParentTree].Resolve().GetAs<PdfDictionary>();
        var parentTreeEntries = BuildParentTreeLookup(parentTree);

        foreach (var page in document.Pages)
        {
            ValidateRenderedContent(page.GetContentNodes(), page.NativeObject.Get<PdfNumber>(PdfName.StructParents), parentTreeEntries);
        }
    }

    public static IReadOnlyList<PdfDictionary> GetStructureElements(PdfDocument document)
    {
        return EnumerateStructElements(GetStructTreeRoot(document)[PdfName.K].Resolve()).ToList();
    }

    public static PdfDictionary GetStructTreeRoot(PdfDocument document)
    {
        var structRoot = document.Catalog.Get<PdfDictionary>(PdfName.StructTreeRoot);
        Assert.NotNull(structRoot);
        return structRoot!;
    }

    public static PdfDictionary GetRootDocumentElement(PdfDocument document)
    {
        var root = GetStructTreeRoot(document)[PdfName.K].Resolve().GetAs<PdfDictionary>();
        Assert.Equal(PdfName.StructElem, root.Get<PdfName>(PdfName.TYPE));
        Assert.Equal(PdfName.Document, root.Get<PdfName>(PdfName.S));
        return root;
    }

    public static IReadOnlyList<PdfDictionary> GetAnnotations(PdfDocument document, PdfName subtype)
    {
        var annotations = new List<PdfDictionary>();
        foreach (var page in document.Pages)
        {
            var annots = page.NativeObject.Get<PdfArray>(PdfName.Annots);
            if (annots == null)
            {
                continue;
            }

            foreach (var annot in annots)
            {
                var dict = annot.Resolve().GetAs<PdfDictionary>();
                if (dict.Get<PdfName>(PdfName.Subtype) == subtype)
                {
                    annotations.Add(dict);
                }
            }
        }

        return annotations;
    }

    public static PdfStream GetFirstXObject(PdfPage page, PdfName subtype)
    {
        return page.Resources.Get<PdfDictionary>(PdfName.XObject)!
            .Select(x => x.Value.Resolve().GetAsOrNull<PdfStream>())
            .First(x => x?.Dictionary.Get<PdfName>(PdfName.Subtype) == subtype)!;
    }

    public static string ExtractTextWithPdfPig(byte[] bytes)
    {
        using var doc = UglyToad.PdfPig.PdfDocument.Open(bytes);
        return string.Join("\n", doc.GetPages().Select(x => x.Text));
    }

    private static void AssertStructParents(IPdfObject item, PdfDictionary structRoot)
    {
        if (item.Type == PdfObjectType.ArrayObj)
        {
            foreach (var child in item.GetAs<PdfArray>())
            {
                AssertStructParents(child.Resolve(), structRoot);
            }
            return;
        }

        if (item.Type != PdfObjectType.DictionaryObj)
        {
            return;
        }

        var dict = item.GetAs<PdfDictionary>();
        if (dict.Get<PdfName>(PdfName.TYPE) != PdfName.StructElem)
        {
            return;
        }

        var parent = dict[PdfName.P].Resolve().GetAs<PdfDictionary>();
        Assert.NotNull(parent);

        if (parent == structRoot)
        {
            Assert.Same(structRoot, parent);
        }

        if (dict.TryGetValue(PdfName.K, out var kids) && kids != null)
        {
            AssertStructParents(kids.Resolve(), structRoot);
        }
    }

    private static IEnumerable<PdfDictionary> EnumerateStructElements(IPdfObject item)
    {
        if (item.Type == PdfObjectType.ArrayObj)
        {
            foreach (var child in item.GetAs<PdfArray>())
            {
                foreach (var nested in EnumerateStructElements(child.Resolve()))
                {
                    yield return nested;
                }
            }

            yield break;
        }

        if (item.Type != PdfObjectType.DictionaryObj)
        {
            yield break;
        }

        var dict = item.GetAs<PdfDictionary>();
        if (dict.Get<PdfName>(PdfName.TYPE) == PdfName.StructElem)
        {
            yield return dict;
        }

        if (dict.TryGetValue(PdfName.K, out var kids) && kids != null)
        {
            foreach (var nested in EnumerateStructElements(kids.Resolve()))
            {
                yield return nested;
            }
        }
    }

    private static IReadOnlyList<string> GetChildTypes(PdfDictionary element)
    {
        if (!element.TryGetValue(PdfName.K, out var kids) || kids == null)
        {
            return Array.Empty<string>();
        }

        var resolved = kids.Resolve();
        if (resolved.Type == PdfObjectType.DictionaryObj)
        {
            var child = resolved.GetAs<PdfDictionary>();
            return child.Get<PdfName>(PdfName.TYPE) == PdfName.StructElem
                ? new[] { child.Get<PdfName>(PdfName.S)!.Value }
                : Array.Empty<string>();
        }

        if (resolved.Type != PdfObjectType.ArrayObj)
        {
            return Array.Empty<string>();
        }

        return resolved.GetAs<PdfArray>()
            .Select(x => x.Resolve().GetAsOrNull<PdfDictionary>())
            .Where(x => x?.Get<PdfName>(PdfName.TYPE) == PdfName.StructElem)
            .Select(x => x!.Get<PdfName>(PdfName.S)!.Value)
            .ToArray();
    }

    private static string? GetParentType(PdfDictionary element)
    {
        var parent = element.Get<PdfDictionary>(PdfName.P);
        if (parent == null || parent.Get<PdfName>(PdfName.TYPE) != PdfName.StructElem)
        {
            return null;
        }

        return parent.Get<PdfName>(PdfName.S)?.Value;
    }

    private static Dictionary<int, IPdfObject> BuildParentTreeLookup(PdfDictionary parentTree)
    {
        var lookup = new Dictionary<int, IPdfObject>();
        var nums = parentTree.Get<PdfArray>(PdfName.Nums);
        Assert.NotNull(nums);

        for (var i = 0; i + 1 < nums!.Count; i += 2)
        {
            lookup[(PdfIntNumber)nums[i]] = nums[i + 1].Resolve();
        }

        return lookup;
    }

    private static void ValidateRenderedContent(
        IEnumerable<IContentNode<double>> nodes,
        PdfNumber? parentTreeIndex,
        IReadOnlyDictionary<int, IPdfObject> parentTreeEntries,
        bool artifactContext = false,
        bool taggedContext = false)
    {
        foreach (var node in nodes)
        {
            if (node is MarkedContentGroup<double> marked)
            {
                var isArtifact = marked.Tag.Name == PdfName.Artifact;
                var childTaggedContext = taggedContext;
                if (!isArtifact && TryGetMarkedContentId(marked.Tag, out var mcid))
                {
                    Assert.NotNull(parentTreeIndex);
                    Assert.True(parentTreeEntries.TryGetValue((int)parentTreeIndex!, out var entry));
                    Assert.Equal(PdfObjectType.ArrayObj, entry.Type);

                    var refs = entry.GetAs<PdfArray>();
                    Assert.True(mcid < refs.Count, $"MCID {mcid} was not present in the ParentTree.");
                    Assert.NotEqual(PdfObjectType.NullObj, refs[mcid].Resolve().Type);
                    childTaggedContext = true;
                }

                ValidateRenderedContent(
                    marked.Children,
                    parentTreeIndex,
                    parentTreeEntries,
                    artifactContext || isArtifact,
                    childTaggedContext);
                continue;
            }

            if (node is FormContent<double> formContent)
            {
                var formStructParents = formContent.Stream.Dictionary.Get<PdfNumber>(PdfName.StructParents);
                if (formStructParents != null)
                {
                    ValidateRenderedContent(formContent.Parse(), formStructParents, parentTreeEntries, artifactContext, false);
                    continue;
                }

                Assert.True(
                    artifactContext ||
                    taggedContext ||
                    formContent.Stream.Dictionary.ContainsKey(PdfName.StructParent),
                    "Real form content must be tagged, marked as Artifact, or bound with StructParent/StructParents.");
                continue;
            }

            if (node is ImageContent<double> imageContent)
            {
                Assert.True(
                    artifactContext ||
                    taggedContext ||
                    imageContent.Stream.Dictionary.ContainsKey(PdfName.StructParent) ||
                    imageContent.Stream.Dictionary.ContainsKey(PdfName.StructParents),
                    "Real image content must be tagged, marked as Artifact, or bound with StructParent/StructParents.");
                continue;
            }

            Assert.True(
                artifactContext || taggedContext,
                "Real page content must be tagged or wrapped in an Artifact marked-content sequence.");
        }
    }

    private static bool TryGetMarkedContentId(MarkedContent marked, out int mcid)
    {
        var value = marked.InlineProps?.Get<PdfNumber>(PdfName.MCID) ??
                    marked.PropList?.Get<PdfNumber>(PdfName.MCID);
        if (value != null)
        {
            mcid = (int)value;
            return true;
        }

        mcid = default;
        return false;
    }

    private static bool IsStructureDestination(PdfArray destination)
    {
        if (destination.Count == 0)
        {
            return false;
        }

        var target = destination[0].Resolve();
        return target.Type == PdfObjectType.DictionaryObj &&
            target.GetAs<PdfDictionary>().Get<PdfName>(PdfName.TYPE) == PdfName.StructElem;
    }
}
