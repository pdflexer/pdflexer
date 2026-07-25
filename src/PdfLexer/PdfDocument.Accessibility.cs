using System.Text;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Remediation;

namespace PdfLexer;

public sealed partial class PdfDocument
{
    internal const string Pdf17StructureNamespaceUri = "http://iso.org/pdf/ssn";
    internal const string Pdf20StructureNamespaceUri = "http://iso.org/pdf2/ssn";

    private AccessibilityConfiguration? _accessibilityConfiguration;
    internal bool IsStrictAccessibility => _accessibilityConfiguration?.StrictConformance == true;

    internal void ValidateLegacyWidgetFactory()
    {
        const string message = "The legacy text-widget overload has no embedded appearance font. Use FormFieldAppearanceOptions.";
        if (_accessibilityConfiguration?.StrictConformance == true)
            throw new PdfAccessibilityConformanceException(message);
        Context.Warning(message);
    }

    internal void ValidateAppearanceFont(IWritableFont font)
    {
        if (_accessibilityConfiguration?.StrictConformance == true && !IsPdfFontDictionaryEmbedded(font.GetPdfFont()))
            throw new PdfAccessibilityConformanceException("Strict accessibility requires an embedded font for form-field appearances.");
    }

    private static bool IsPdfFontDictionaryEmbedded(PdfDictionary dict)
    {
        var subtype = dict.Get<PdfName>(PdfName.Subtype);
        PdfDictionary? descriptor = subtype == PdfName.Type0
            ? dict.Get<PdfArray>(PdfName.DescendantFonts)?.FirstOrDefault()?.Resolve().GetAs<PdfDictionary>()?.Get<PdfDictionary>(PdfName.FontDescriptor)
            : dict.Get<PdfDictionary>(PdfName.FontDescriptor);
        return descriptor != null && (descriptor.ContainsKey(PdfName.FontFile) ||
            descriptor.ContainsKey(PdfName.FontFile2) || descriptor.ContainsKey(PdfName.FontFile3));
    }

    /// <summary>
    /// Applies the document-level metadata and viewer defaults required for the library's PDF/UA-oriented authoring flow.
    /// </summary>
    /// <remarks>
    /// This helper only supports new documents and existing documents that are currently untagged. It does not remediate
    /// or edit an existing <c>StructTreeRoot</c>. See <c>docs/accessibility-authoring.md</c> for the supported workflow.
    /// </remarks>
    public void ApplyAccessibilitySetup(
        string language,
        string title,
        PdfUaProfile profile = PdfUaProfile.PdfUa1,
        bool strictConformance = true)
    {
        ThrowIfAccessibilityAuthoringIsUnsupported(nameof(ApplyAccessibilitySetup));

        _accessibilityConfiguration = AccessibilityConfiguration.Create(profile, strictConformance);

        Catalog[PdfName.Lang] = PdfString.CreateTextString(language);

        var info = Trailer.GetOrCreateValue<PdfDictionary>(PdfName.Info);
        Trailer[PdfName.Info] = info.Indirect();
        info[PdfName.Title] = PdfString.CreateTextString(title);

        var viewerPreferences = Catalog.GetOrCreateValue<PdfDictionary>(PdfName.ViewerPreferences);
        viewerPreferences[PdfName.DisplayDocTitle] = PdfBoolean.True;

        var markInfo = Catalog.GetOrCreateValue<PdfDictionary>(PdfName.MarkInfo);
        markInfo[PdfName.Marked] = PdfBoolean.True;
        markInfo[PdfName.Suspects] = PdfBoolean.False;

        if (Pages != null)
        {
            foreach (var page in Pages)
            {
                var tabs = page.NativeObject.Get<PdfName>(PdfName.Tabs);
                if (tabs == null || tabs == PdfName.S)
                {
                    page.NativeObject[PdfName.Tabs] = PdfName.S;
                }
            }
        }

        if (_accessibilityConfiguration.MinimumPdfVersion > PdfVersion)
        {
            PdfVersion = _accessibilityConfiguration.MinimumPdfVersion;
        }

        Catalog[PdfName.Metadata] = CreateAccessibilityMetadata(language, title, _accessibilityConfiguration).Indirect();
        ApplyAccessibilityStructureDefaults();
    }

    public RemediationSession BeginRemediation(RemediationSessionConfiguration? configuration = null)
    {
        ThrowIfAccessibilityAuthoringIsUnsupported(nameof(BeginRemediation));
        return new RemediationSession(this, configuration ?? new RemediationSessionConfiguration());
    }

    private static PdfStream CreateAccessibilityMetadata(string language, string title, AccessibilityConfiguration config)
    {
        var revisionLine = string.IsNullOrEmpty(config.Revision)
            ? string.Empty
            : $"      <pdfuaid:rev>{config.Revision}</pdfuaid:rev>{Environment.NewLine}";

        var xml =
$"""
<?xpacket begin="" id="W5M0MpCehiHzreSzNTczkc9d"?>
<x:xmpmeta xmlns:x="adobe:ns:meta/" x:xmptk="PdfLexer">
  <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
    <rdf:Description rdf:about=""
      xmlns:dc="http://purl.org/dc/elements/1.1/"
      xmlns:pdf="http://ns.adobe.com/pdf/1.3/"
      xmlns:pdfuaid="http://www.aiim.org/pdfua/ns/id/">
      <dc:title>
        <rdf:Alt>
          <rdf:li xml:lang="x-default">{EscapeXml(title)}</rdf:li>
        </rdf:Alt>
      </dc:title>
      <dc:language>
        <rdf:Bag>
          <rdf:li>{EscapeXml(language)}</rdf:li>
        </rdf:Bag>
      </dc:language>
      <pdf:Title>{EscapeXml(title)}</pdf:Title>
      <pdfuaid:part>{config.Part}</pdfuaid:part>
{revisionLine}    </rdf:Description>
  </rdf:RDF>
</x:xmpmeta>
<?xpacket end="w"?>
""";

        var stream = new PdfStream(new PdfByteArrayStreamContents(Encoding.UTF8.GetBytes(xml)));
        stream.Dictionary[PdfName.TypeName] = PdfName.Metadata;
        stream.Dictionary[PdfName.Subtype] = PdfName.XML;
        return stream;
    }

    private static string EscapeXml(string value)
    {
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    internal void ApplyAccessibilityStructureDefaults()
    {
        if (_structure == null || _accessibilityConfiguration?.RequiredDocumentNamespaceUri == null)
        {
            return;
        }

        var documentNode = _structure.GetRoot();
        var expectedUri = _accessibilityConfiguration.RequiredDocumentNamespaceUri;
        if (documentNode.Namespace == null)
        {
            documentNode.Namespace = _structure.DeclareNamespace(expectedUri);
            return;
        }

        if (_accessibilityConfiguration.StrictConformance &&
            !string.Equals(documentNode.Namespace.Uri, expectedUri, StringComparison.Ordinal))
        {
            throw new PdfAccessibilityConformanceException(
                $"PDF/UA-2 requires the root Document structure element to use the PDF 2.0 namespace '{expectedUri}'.");
        }
    }

    internal void ValidateAccessibilityAuthoringBeforeSave()
    {
        if (_structure == null)
        {
            return;
        }

        ValidateStructureReferences(_structure.GetRoot());

        if (_accessibilityConfiguration?.StrictConformance != true)
        {
            return;
        }

        ApplyAccessibilityStructureDefaults();

        var root = _structure.GetRoot();
        if (!string.Equals(root.Type, "Document", StringComparison.Ordinal))
        {
            throw new PdfAccessibilityConformanceException(
                "Strict accessibility authoring requires the structure tree root element to be 'Document'.");
        }

        if (_accessibilityConfiguration.RequiredDocumentNamespaceUri != null)
        {
            if (root.Namespace == null ||
                !string.Equals(root.Namespace.Uri, _accessibilityConfiguration.RequiredDocumentNamespaceUri, StringComparison.Ordinal))
            {
                throw new PdfAccessibilityConformanceException(
                    $"PDF/UA-2 requires the root Document structure element to use the PDF 2.0 namespace '{_accessibilityConfiguration.RequiredDocumentNamespaceUri}'.");
            }
        }

        ValidateRoleMap(_structure.GetStructureRoot());
        ValidateStructureNode(root);
        ValidateNavigation(root);
    }

    private void ValidateStructureReferences(StructureNode node)
    {
        ValidateIds(node, node.References, "Ref");
        ValidateIds(node, node.Headers, "Headers");
        foreach (var child in node.Children)
        {
            ValidateStructureReferences(child);
        }
    }

    private void ValidateIds(StructureNode source, IEnumerable<string> ids, string relationship)
    {
        foreach (var id in ids.Where(x => !_structure!.GetStructureRoot().IdMap.ContainsKey(x)).Distinct(StringComparer.Ordinal))
        {
            var sourceName = string.IsNullOrEmpty(source.ID)
                ? source.Type
                : $"{source.Type} (ID '{source.ID}')";
            var message = $"Structure element {sourceName} has a /{relationship} reference to missing ID '{id}'.";
            if (_accessibilityConfiguration?.StrictConformance == true)
            {
                throw new PdfAccessibilityConformanceException(message);
            }
            Context.Warning(message);
        }
    }

    private void ValidateNavigation(StructureNode root)
    {
        if (HasType(root, "TOC") && Pages.Count > 1 && _pageLabels.Count == 0)
        {
            throw new PdfAccessibilityConformanceException(
                "Strict accessibility mode requires PageLabels to be defined when a Table of Contents (TOC) is present in a multi-page document.");
        }
    }

    private static bool HasType(StructureNode node, string type)
    {
        if (node.Type == type) return true;
        foreach (var child in node.Children)
        {
            if (HasType(child, type)) return true;
        }
        return false;
    }

    private static void ValidateRoleMap(StructureRoot root)
    {
        foreach (var (custom, _) in root.RoleMap)
        {
            if (StandardStructureTypes.Contains(custom))
            {
                throw new PdfAccessibilityConformanceException(
                    $"Strict accessibility mode rejects mapping from a standard structure type to another type in the same namespace. Custom type '{custom}' is already a standard structure type.");
            }
        }
    }

    internal void ValidateAccessibilityAuthoringAfterSerialization(IReadOnlyList<PdfIndirectRef> pageRefs, PdfDictionary catalog)
    {
        // Only documents that opted into the accessible-authoring flow are validated; a document
        // that merely used the structure builder never asked for PDF/UA conformance feedback.
        if (_accessibilityConfiguration == null)
        {
            return;
        }

        var isStrict = _accessibilityConfiguration.StrictConformance;
        var structRoot = catalog.Get<PdfDictionary>(PdfName.StructTreeRoot);
        if (structRoot == null && isStrict)
        {
            throw new PdfAccessibilityConformanceException(
                "Strict accessibility authoring requires a structure tree to be serialized.");
        }

        var parentTreeObj = structRoot?.Get(PdfName.ParentTree)?.Resolve();
        var parentTree = parentTreeObj is PdfDictionary ptDict ? ptDict : (parentTreeObj is PdfIndirectRef ir && ir.GetObject() is PdfDictionary irDict ? irDict : null);
        if (parentTree == null && isStrict)
        {
            throw new PdfAccessibilityConformanceException(
                "Strict accessibility authoring requires a ParentTree in the structure tree root.");
        }
        var parentTreeEntries = parentTree != null ? BuildParentTreeLookup(parentTree) : new Dictionary<int, IPdfObject>();

        foreach (var pageRef in pageRefs)
        {
            if (pageRef.GetObject() is not PdfDictionary pageDict)
            {
                continue;
            }

            var page = new PdfPage(pageDict);

            if (pageDict.ContainsKey(PdfName.Annots))
            {
                var tabs = pageDict.Get<PdfName>(PdfName.Tabs);
                if (tabs != PdfName.S)
                {
                    ReportViolation("Strict accessibility mode requires annotated pages to have /Tabs set to /S (Structure order).", isStrict);
                }

                ValidatePageAnnotations(page, parentTreeEntries, isStrict);
            }

            // Deliberately strict-only: sweeping every page's content model is far more expensive
            // than the /Annots walk above, which runs in both modes.
            if (isStrict)
            {
                ValidateRenderedContent(page.GetContentNodes(), pageDict.Get<PdfNumber>(PdfName.StructParents), parentTreeEntries);
            }
        }
    }

    private void ReportViolation(string message, bool isStrict)
    {
        if (isStrict)
        {
            throw new PdfAccessibilityConformanceException(message);
        }
        Context.Warning(message);
    }

    private void ValidatePageAnnotations(PdfPage page, IReadOnlyDictionary<int, IPdfObject> parentTreeEntries, bool isStrict)
    {
        var annots = page.NativeObject.Get<PdfArray>(PdfName.Annots);
        if (annots == null) return;
        foreach (var item in annots)
        {
            if (item.Resolve() is not PdfDictionary annotation) continue;
            var subtype = annotation.Get<PdfName>(PdfName.Subtype);
            if (subtype == null || subtype == (PdfName)"Popup" || IsHiddenAnnotation(annotation) || IsWhollyOffPage(page, annotation))
                continue;

            if (subtype == (PdfName)"PrinterMark")
            {
                if (!HasNormalAppearance(annotation))
                    ReportViolation("Visible PrinterMark annotations require a normal appearance stream.", isStrict);
                if (!NormalAppearances(annotation).All(IsArtifactedAppearance))
                    ReportViolation("PrinterMark annotation appearance content must be marked as an Artifact.", isStrict);
                continue;
            }

            var index = annotation.Get<PdfNumber>(PdfName.StructParent);
            if (index == null || !parentTreeEntries.TryGetValue((int)index, out var owner) ||
                owner.Resolve() is not PdfDictionary structureElement)
            {
                ReportViolation($"Visible {subtype.Value} annotations require a valid /StructParent entry backed by the ParentTree.", isStrict);
                continue;
            }

            var rawTag = structureElement.Get<PdfName>(PdfName.S)?.Value;
            var actualTag = rawTag;
            if (rawTag != null && _structure != null && _structure.GetStructureRoot().RoleMap.TryGetValue(rawTag, out var mapped))
            {
                actualTag = mapped;
            }
            var expectedTag = subtype == PdfName.Link ? "Link" : subtype == PdfName.Widget ? "Form" : "Annot";
            if (!string.Equals(actualTag, expectedTag, StringComparison.Ordinal))
            {
                ReportViolation($"{subtype.Value} annotations must be nested in a {expectedTag} structure element, not {actualTag ?? "an untyped element"}.", isStrict);
            }

            if (subtype == PdfName.Widget)
            {
                ValidateWidgetAnnotation(annotation, isStrict);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(annotation.Get<PdfString>(PdfName.Contents)?.Value))
                    ReportViolation($"Visible {subtype.Value} annotations require a non-empty /Contents description.", isStrict);
                if (subtype != PdfName.Link && !HasNormalAppearance(annotation))
                    ReportViolation($"Visible {subtype.Value} annotations require a normal appearance stream.", isStrict);
            }
        }
    }

    private void ValidateWidgetAnnotation(PdfDictionary annotation, bool isStrict)
    {
        var field = annotation.Get<PdfDictionary>(PdfName.Parent) ?? annotation;
        var tooltip = field.Get<PdfString>((PdfName)"TU")?.Value ??
            annotation.Get<PdfString>((PdfName)"TU")?.Value;
        if (string.IsNullOrWhiteSpace(tooltip))
            ReportViolation("Widget annotations require a non-empty /TU tooltip.", isStrict);
        var flags = (int?)annotation.Get<PdfNumber>(PdfName.F) ?? 0;
        if ((flags & 4) == 0)
            ReportViolation("Widget annotations require the Print flag.", isStrict);
        if (!HasNormalAppearance(annotation))
            ReportViolation("Widget annotations require a normal appearance stream.", isStrict);

        if (field.Get<PdfName>(PdfName.FT) != PdfName.Btn) return;
        var normal = annotation.Get<PdfDictionary>((PdfName)"AP")?.Get<PdfDictionary>(PdfName.N);
        if (normal == null) return; // pushbuttons use a single normal appearance stream
        var state = annotation.Get<PdfName>((PdfName)"AS");
        var value = field.Get<PdfName>(PdfName.V);
        if (state == null || !normal.ContainsKey(state) ||
            (value != null && state != (PdfName)"Off" && state != value))
            ReportViolation("Button widget /AP /N states, /AS, and field /V must be synchronized.", isStrict);
    }

    private static bool HasNormalAppearance(PdfDictionary annotation)
    {
        return annotation.Get<PdfDictionary>((PdfName)"AP")?.Get(PdfName.N)?.Resolve() is PdfStream or PdfDictionary;
    }

    private static IEnumerable<PdfStream> NormalAppearances(PdfDictionary annotation)
    {
        var normal = annotation.Get<PdfDictionary>((PdfName)"AP")?.Get(PdfName.N)?.Resolve();
        if (normal is PdfStream stream)
        {
            yield return stream;
        }
        else if (normal is PdfDictionary states)
        {
            foreach (var value in states.Values)
            {
                if (value.Resolve() is PdfStream state) yield return state;
            }
        }
    }

    private static bool IsArtifactedAppearance(PdfStream appearance)
    {
        var content = Encoding.ASCII.GetString(appearance.Contents.GetDecodedData());
        return content.Contains("/Artifact", StringComparison.Ordinal) &&
               (content.Contains(" BMC", StringComparison.Ordinal) ||
                content.Contains(" BDC", StringComparison.Ordinal));
    }

    private static bool IsHiddenAnnotation(PdfDictionary annotation)
    {
        var flags = (int?)annotation.Get<PdfNumber>(PdfName.F) ?? 0;
        return (flags & 2) != 0 || (flags & 32) != 0;
    }

    private static bool IsWhollyOffPage(PdfPage page, PdfDictionary annotation)
    {
        var rectArray = annotation.Get<PdfArray>(PdfName.Rect);
        if (rectArray == null) return false;
        var rect = new PdfRectangle(rectArray);
        var box = page.CropBox;
        return (decimal)rect.URx <= (decimal)box.LLx || (decimal)rect.LLx >= (decimal)box.URx ||
               (decimal)rect.URy <= (decimal)box.LLy || (decimal)rect.LLy >= (decimal)box.URy;
    }

    internal void ValidateAccessibilityAuthoringSnapshot()
    {
        ValidateAccessibilityAuthoringBeforeSave();

        var catalog = Catalog.CloneShallow();
        catalog.Remove(PdfName.StructTreeRoot);

        if (Pages == null)
        {
            return;
        }

        var (pagesRef, refs, annotationMap) = BuildPageTree();
        catalog[PdfName.Pages] = pagesRef;

        if (_structure != null)
        {
            var pageMap = new Dictionary<PdfPage, PdfIndirectRef>();
            if (Pages.Count == refs.Count)
            {
                for (var i = 0; i < Pages.Count; i++)
                {
                    pageMap[Pages[i]] = refs[i];
                }
            }

            var serializer = new Writing.StructuralSerializer(pageMap, annotationMap);
            var result = serializer.ConvertToPdf(_structure.GetRoot());
            catalog[PdfName.StructTreeRoot] = PdfIndirectRef.Create(result.Root);

            var markInfo = catalog.GetOrCreateValue<PdfDictionary>(PdfName.MarkInfo);
            markInfo[PdfName.Marked] = PdfBoolean.True;
        }

        ValidateAccessibilityAuthoringAfterSerialization(refs, catalog);
    }

    private void ValidateStructureNode(StructureNode node)
    {
        ValidateStructureChildren(node);
        ValidateStructureObjectReferences(node);
        ValidateStructureNodeAttributes(node);

        foreach (var child in node.Children)
        {
            ValidateStructureNode(child);
        }
    }

    private static void ValidateStructureNodeAttributes(StructureNode node)
    {
        switch (node.Type)
        {
            case "Figure":
            case "Formula":
                if (string.IsNullOrWhiteSpace(node.Alt) && string.IsNullOrWhiteSpace(node.ActualText))
                {
                    throw new PdfAccessibilityConformanceException(
                        $"Strict accessibility requires Figure and Formula elements to have either an Alt or ActualText attribute. Element of type '{node.Type}' is missing both.");
                }
                break;
            case "Note":
                throw new PdfAccessibilityConformanceException(
                    "Strict accessibility mode rejects the 'Note' structure type. Use 'FENote' for footnote or endnote semantics, or another supported structure type for other notes.");
        }
    }

    private static void ValidateStructureChildren(StructureNode node)
    {
        switch (node.Type)
        {
            case "Table":
                ValidateChildTypes(
                    node,
                    "Table",
                    "TR",
                    "THead",
                    "TBody",
                    "TFoot",
                    "Caption");
                break;
            case "TR":
                ValidateChildTypes(node, "TR", "TH", "TD");
                break;
            case "TH":
            case "TD":
                if (!string.Equals(node.Parent?.Type, "TR", StringComparison.Ordinal))
                {
                    throw new PdfAccessibilityConformanceException(
                        $"{node.Type} elements must be children of TR elements in strict accessibility mode.");
                }
                break;
            case "L":
                ValidateChildTypes(node, "L", "LI");
                break;
            case "LI":
                if (!string.Equals(node.Parent?.Type, "L", StringComparison.Ordinal))
                {
                    throw new PdfAccessibilityConformanceException(
                        "LI elements must be children of L elements in strict accessibility mode.");
                }

                ValidateChildTypes(node, "LI", "Lbl", "LBody");
                break;
            case "Lbl":
            case "LBody":
                if (!string.Equals(node.Parent?.Type, "LI", StringComparison.Ordinal))
                {
                    throw new PdfAccessibilityConformanceException(
                        $"{node.Type} elements must be children of LI elements in strict accessibility mode.");
                }
                break;
        }
    }

    private void ValidateStructureObjectReferences(StructureNode node)
    {
        if (!string.Equals(node.Type, "Link", StringComparison.Ordinal))
        {
            return;
        }

        foreach (var objRef in node.ObjectReferences)
        {
            if (objRef.Object.Resolve() is not PdfDictionary annot ||
                annot.Get<PdfName>(PdfName.Subtype) != PdfName.Link)
            {
                continue;
            }

            var contents = objRef.AnnotationContents ?? annot.Get<PdfString>(PdfName.Contents)?.Value;
            if (string.IsNullOrWhiteSpace(contents))
            {
                throw new PdfAccessibilityConformanceException(
                    "Accessible link annotations require a non-empty /Contents description.");
            }

            if (_accessibilityConfiguration?.Profile == PdfUaProfile.PdfUa2 &&
                IsInternalCurrentDocumentDestination(annot) &&
                objRef.StructureDestinationTarget == null)
            {
                throw new PdfAccessibilityConformanceException(
                    "PDF/UA-2 accessible links must target a StructureNode destination. Use the StructureNode-targeted AddLink overload.");
            }
        }
    }

    private static void ValidateChildTypes(StructureNode node, string parentType, params string[] allowedChildren)
    {
        foreach (var child in node.Children)
        {
            if (!allowedChildren.Contains(child.Type, StringComparer.Ordinal))
            {
                throw new PdfAccessibilityConformanceException(
                    $"{parentType} elements may only contain {string.Join(", ", allowedChildren)} children in strict accessibility mode.");
            }
        }
    }

    private static Dictionary<int, IPdfObject> BuildParentTreeLookup(PdfDictionary parentTree)
    {
        var lookup = new Dictionary<int, IPdfObject>();
        var nums = parentTree.Get<PdfArray>(PdfName.Nums);
        if (nums == null)
        {
            return lookup;
        }

        for (var i = 0; i + 1 < nums.Count; i += 2)
        {
            lookup[(PdfIntNumber)nums[i]] = nums[i + 1].Resolve();
        }

        return lookup;
    }

    private void ValidateRenderedContent(
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
                    if (parentTreeIndex == null)
                    {
                        throw new PdfAccessibilityConformanceException(
                            "Tagged content requires a page or form StructParents entry.");
                    }

                    if (!parentTreeEntries.TryGetValue((int)parentTreeIndex, out var entry) ||
                        entry.Type != PdfObjectType.ArrayObj)
                    {
                        throw new PdfAccessibilityConformanceException(
                            "Tagged content is missing a ParentTree array entry for its StructParents index.");
                    }

                    var refs = entry.GetAs<PdfArray>();
                    if (mcid >= refs.Count || refs[mcid].Resolve().Type == PdfObjectType.NullObj)
                    {
                        throw new PdfAccessibilityConformanceException(
                            $"Tagged content MCID {mcid} is missing from the ParentTree.");
                    }

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

                if (artifactContext ||
                    taggedContext ||
                    formContent.Stream.Dictionary.ContainsKey(PdfName.StructParent))
                {
                    continue;
                }

                throw new PdfAccessibilityConformanceException(
                    "Real form content must be tagged, marked as Artifact, or bound with StructParent/StructParents.");
            }

            if (node is ImageContent<double> imageContent)
            {
                if (artifactContext ||
                    taggedContext ||
                    imageContent.Stream.Dictionary.ContainsKey(PdfName.StructParent) ||
                    imageContent.Stream.Dictionary.ContainsKey(PdfName.StructParents))
                {
                    continue;
                }

                throw new PdfAccessibilityConformanceException(
                    "Real image content must be tagged, marked as Artifact, or bound with StructParent/StructParents.");
            }

            if (node is TextContent<double> textContent)
            {
                if (artifactContext)
                {
                    continue;
                }

                if (!taggedContext)
                {
                    throw new PdfAccessibilityConformanceException(
                        "Real page content must be tagged with BeginMarkedContent(...) or wrapped in BeginArtifact(...).");
                }

                foreach (var segment in textContent.Segments)
                {
                    var font = segment.GraphicsState.Font;
                    if (font == null || !IsFontEmbedded(font))
                    {
                        var fontName = font?.Name ?? "Unknown";
                        throw new PdfAccessibilityConformanceException(
                            $"Strict accessibility requires all rendered fonts for tagged content to be embedded. Font '{fontName}' is not embedded.");
                    }
                }
                continue;
            }

            if (!artifactContext && !taggedContext)
            {
                throw new PdfAccessibilityConformanceException(
                    "Real page content must be tagged with BeginMarkedContent(...) or wrapped in BeginArtifact(...).");
            }
        }
    }

    private static bool IsFontEmbedded(IReadableFont font)
    {
        var dict = font.NativeObject;
        var subtype = dict.Get<PdfName>(PdfName.Subtype);

        if (subtype == PdfName.Type3)
        {
            // Type 3 fonts are always embedded in the sense that they are defined in the PDF.
            return true;
        }

        PdfDictionary? descriptor = null;
        if (subtype == PdfName.Type0)
        {
            var descendantFonts = dict.Get<PdfArray>(PdfName.DescendantFonts);
            if (descendantFonts != null && descendantFonts.Count > 0)
            {
                var descendant = descendantFonts[0].Resolve().GetAs<PdfDictionary>();
                descriptor = descendant.Get<PdfDictionary>(PdfName.FontDescriptor);
            }
        }
        else
        {
            descriptor = dict.Get<PdfDictionary>(PdfName.FontDescriptor);
        }

        if (descriptor == null)
        {
            return false;
        }

        return descriptor.ContainsKey(PdfName.FontFile) ||
               descriptor.ContainsKey(PdfName.FontFile2) ||
               descriptor.ContainsKey(PdfName.FontFile3);
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

    private static bool IsInternalCurrentDocumentDestination(PdfDictionary annotation)
    {
        if (annotation.TryGetValue(PdfName.Dest, out var dest) && dest != null)
        {
            return IsInternalDestinationObject(dest.Resolve());
        }

        var action = annotation.Get<PdfDictionary>(PdfName.A);
        if (action == null || action.Get<PdfName>(PdfName.S) != PdfName.GoTo)
        {
            return false;
        }

        return action.TryGetValue((PdfName)"D", out var goToDest) &&
            goToDest != null &&
            IsInternalDestinationObject(goToDest.Resolve());
    }

    private static bool IsInternalDestinationObject(IPdfObject destination)
    {
        if (destination.Type == PdfObjectType.NameObj || destination.Type == PdfObjectType.StringObj)
        {
            return true;
        }

        if (destination.Type != PdfObjectType.ArrayObj)
        {
            return false;
        }

        var array = destination.GetAs<PdfArray>();
        if (array.Count == 0)
        {
            return false;
        }

        return !IsStructureDestination(array);
    }

    internal static bool IsStructureDestination(PdfArray destination)
    {
        if (destination.Count == 0)
        {
            return false;
        }

        var target = destination[0].Resolve();
        return target.Type == PdfObjectType.DictionaryObj &&
            target.GetAs<PdfDictionary>().Get<PdfName>(PdfName.TYPE) == PdfName.StructElem;
    }

    private void ThrowIfAccessibilityAuthoringIsUnsupported(string operation)
    {
        if (Catalog.ContainsKey(PdfName.StructTreeRoot))
        {
            throw new PdfAccessibilitySetupException(
                $"{operation} only supports new or currently untagged documents. Editing a document with an existing StructTreeRoot is not supported.");
        }
    }

    private static readonly HashSet<string> StandardStructureTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "Document", "Part", "Art", "Sect", "Div", "BlockQuote", "Caption", "TOC", "TOCI", "Index", "NonStruct", "Private",
        "P", "H1", "H2", "H3", "H4", "H5", "H6", "H",
        "L", "LI", "Lbl", "LBody",
        "Table", "TR", "TH", "TD", "THead", "TBody", "TFoot",
        "Span", "Quote", "Note", "Reference", "Code", "Link", "Annot", "Ruby", "RB", "RT", "RP", "Warichu", "WT", "WP",
        "Figure", "Formula", "Form", "FENote", "Sub", "Em", "Strong"
    };

    internal sealed record AccessibilityConfiguration(
        PdfUaProfile Profile,
        bool StrictConformance,
        int Part,
        string? Revision,
        decimal MinimumPdfVersion,
        string? RequiredDocumentNamespaceUri)
    {
        public static AccessibilityConfiguration Create(PdfUaProfile profile, bool strictConformance)
        {
            return profile switch
            {
                PdfUaProfile.PdfUa1 => new AccessibilityConfiguration(
                    profile,
                    strictConformance,
                    1,
                    null,
                    1.7m,
                    null),
                PdfUaProfile.PdfUa2 => new AccessibilityConfiguration(
                    profile,
                    strictConformance,
                    2,
                    "2024",
                    2.0m,
                    Pdf20StructureNamespaceUri),
                _ => throw new PdfLexerException($"Unsupported PDF/UA profile: {profile}")
            };
        }
    }
}

public enum PdfUaProfile
{
    PdfUa1,
    PdfUa2
}
