using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Writing;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PdfLexer.Remediation;

/// <summary>
/// Coordinates rule-driven remediation for a currently untagged PDF document.
/// </summary>
public sealed partial class RemediationSession : IDisposable
{
    private readonly PdfDocument _document;
    private readonly HashSet<PdfPage> _pagesWithAllocatedMcids = new();
    private readonly Dictionary<PdfPage, int> _pageStructParents = new();
    private CompiledRemediationProgram? _program;
    private readonly List<DiagnosticSuppression> _suppressions = new();
    private PdfUaProfile _effectiveProfile;
    private static readonly HashSet<DiagnosticCode> NonSuppressibleDiagnosticCodes = new()
    {
        DiagnosticCode.GroupCompositionAmbiguous,
        DiagnosticCode.GroupCompositionCycle,
        DiagnosticCode.AnnotationAdoptionTargetMissing,
        DiagnosticCode.AnnotationAdoptionAmbiguous,
        DiagnosticCode.AnnotationAlreadyConsumed,
        DiagnosticCode.PrescriptiveUnaccountedContent,
        DiagnosticCode.TemplateIdentityCollision
    };
    private RemediationTraceRequest? _traceRequest;
    private List<RemediationPredicateTrace>? _predicateTraces;
    private List<RemediationRuntimeDiagnostic>? _programRuntimeDiagnostics;
    private bool _committed;
    private bool _disposed;

    internal RemediationSession(PdfDocument document, RemediationSessionConfiguration configuration)
    {
        _document = document;
        Configuration = configuration;
        _effectiveProfile = configuration.Profile;
        Structure = new StructuralBuilder();
    }

    /// <summary>Session configuration and commit-time accessibility setup options.</summary>
    public RemediationSessionConfiguration Configuration { get; }

    /// <summary>Structure builder owned by the remediation session.</summary>
    public StructuralBuilder Structure { get; }

    /// <summary>
    /// Adds the prescriptive program selected for this document. The program is compiled before it
    /// is selected as the session's native execution plan; only one program may be selected.
    /// </summary>
    public RemediationSession Use(RemediationProgram program)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(program);
        return Use(RemediationProgramCompiler.Compile(program));
    }

    /// <summary>Adds an already compiled prescriptive program without compiling it again.</summary>
    public RemediationSession Use(CompiledRemediationProgram program)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(program);
        if (_program != null)
            throw new InvalidOperationException("A remediation session accepts exactly one prescriptive program.");
        if (!program.IsValid)
            throw new ArgumentException(string.Join(Environment.NewLine, program.Errors), nameof(program));
        _program = program;
        _effectiveProfile = program.Program.Template.Profile;
        return this;
    }

    /// <summary>Compiles a prescriptive program without parsing pages or mutating the document.</summary>
    public ValidationReport Validate(RemediationProgram program)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(program);
        var compiled = RemediationProgramCompiler.Compile(program);
        return new ValidationReport(compiled.Errors);
    }

    /// <summary>Adds a justified diagnostic suppression.</summary>
    public RemediationSession Suppress(DiagnosticCode code, string scope, string reason)
    {
        ThrowIfDisposed();
        _suppressions.Add(new DiagnosticSuppression(code, scope, reason));
        return this;
    }

    /// <summary>Evaluates the selected program without mutating the document.</summary>
    public RemediationReport DryRun()
    {
        ThrowIfDisposed();
        if (_program == null)
            throw new InvalidOperationException("Select a remediation program before evaluating the session.");
        return EvaluateProgram(apply: false);
    }

    /// <summary>Evaluates configured rule sets and retains selected predicate rejection traces.</summary>
    public RemediationReport DryRun(RemediationTraceRequest traceRequest)
    {
        ArgumentNullException.ThrowIfNull(traceRequest);
        ThrowIfDisposed();
        _traceRequest = traceRequest;
        _predicateTraces = new List<RemediationPredicateTrace>();
        try
        {
            if (_program == null)
                throw new InvalidOperationException("Select a remediation program before evaluating the session.");
            return EvaluateProgram(apply: false);
        }
        finally
        {
            _traceRequest = null;
            _predicateTraces = null;
        }
    }

    /// <summary>Applies the selected program and accessibility setup to the document.</summary>
    public RemediationReport Commit()
    {
        ThrowIfDisposed();
        if (_committed)
        {
            throw new InvalidOperationException("Remediation session has already been committed.");
        }
        if (Configuration.RunMode == RemediationRunMode.Authoring)
        {
            throw new InvalidOperationException("Authoring remediation sessions are dry-run only and cannot commit.");
        }

        if (_program == null)
            throw new InvalidOperationException("Select a remediation program before committing the session.");
        return CommitProgram();
    }

    private static PdfRect<double> UnionBounds(IEnumerable<PdfRect<double>> rects)
    {
        using var enumerator = rects.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return new PdfRect<double>(0, 0, 0, 0);
        }

        var result = enumerator.Current;
        while (enumerator.MoveNext())
        {
            var rect = enumerator.Current;
            result = new PdfRect<double>(
                Math.Min(result.LLx, rect.LLx),
                Math.Min(result.LLy, rect.LLy),
                Math.Max(result.URx, rect.URx),
                Math.Max(result.URy, rect.URy));
        }

        return result;
    }
    private static string TemplateDifferenceKey(RemediationTemplateDifference difference) =>
        string.Join("\0",
            difference.Kind,
            difference.ProgramId,
            difference.SlotId,
            difference.ExpectedPath,
            difference.ActualPath,
            difference.ExpectedValue,
            difference.ActualValue,
            string.Join(",", difference.PageIndexes),
            difference.RuleId);

    private RemediationTemplateDifference ReportTemplateDifference(
        RemediationTemplateDifference difference,
        List<string> diagnostics)
    {
        var path = difference.ExpectedPath ?? difference.ActualPath ?? "Document";
        var scope = $"Program:{difference.ProgramId}:Template:{path}";
        var suppressed = Configuration.DiagnosticStrictness != RemediationDiagnosticStrictness.Strict &&
            _suppressions.Any(x => x.Code == difference.DiagnosticCode &&
                (x.Scope == "*" || x.Scope == scope));
        ReportDiagnostic(
            difference.DiagnosticCode,
            scope,
            $"Template mismatch at '{path}': expected {difference.ExpectedValue ?? difference.ExpectedPath ?? "<none>"}, " +
            $"actual {difference.ActualValue ?? difference.ActualPath ?? "<none>"}.",
            diagnostics);
        return difference with { Suppressed = suppressed };
    }

    private static string SemanticShape(RemediationSemanticTree tree)
    {
        var builder = new StringBuilder("Document");
        void Append(RemediationSemanticNode node)
        {
            builder.Append('/').Append(node.Tag);
            if (node.SlotId != null) builder.Append('#').Append(node.SlotId);
            builder.Append('(');
            foreach (var child in node.Children) Append(child);
            builder.Append(')');
        }
        foreach (var root in tree.Roots) Append(root);
        return builder.ToString();
    }
    private static bool ClaimAppearsOnPage(RemediationClaim claim, int pageIndex) =>
        ClaimAppearsOnPage(claim, pageIndex, new HashSet<ClaimId>());

    private static bool ClaimAppearsOnPage(
        RemediationClaim claim,
        int pageIndex,
        HashSet<ClaimId> visited)
    {
        if (!visited.Add(claim.ClaimId))
        {
            return false;
        }
        return claim.PageIndex == pageIndex ||
            claim.RelatedClaims.Any(x => ClaimAppearsOnPage(x, pageIndex, visited));
    }
    private IReadOnlyList<PageRemediationState> BuildPageStates()
    {
        var states = new List<PageRemediationState>();
        for (var i = 0; i < _document.Pages.Count; i++)
        {
            var page = _document.Pages[i];
            var structured = page.GetStructuredText();
            var content = page.GetContentNodes<double>();
            states.Add(new PageRemediationState(page, i, structured, content, content.ToList()));
        }

        BuildContentCandidateIndexes(states);
        return states;
    }

    private static IReadOnlyList<RemediationCandidate> SelectCandidates(
        PageRemediationState pageState,
        CandidateSelector selector)
    {
        if (selector is CandidateSelector.TextSelector text)
        {
            return pageState.TextCandidates[text.Granularity];
        }
        if (selector is CandidateSelector.AnnotationSelector)
        {
            return pageState.AnnotationCandidates;
        }

        var kinds = ((CandidateSelector.ContentSelector)selector).Kinds;
        return pageState.ContentCandidates.Where(x => kinds.Contains(x.Kind)).Cast<RemediationCandidate>().ToArray();
    }

    private static void BuildContentCandidateIndexes(IReadOnlyList<PageRemediationState> states)
    {
        var allItems = states
            .SelectMany(state => EnumerateItems(state.WorkingContent)
                .Where(IsPaintingItem)
                .Where(x => x is not TextContent<double>)
                .Select(item => (State: state, Item: item)))
            .ToList();
        var resourceCounts = allItems
            .Select(x => GetResourceObject(x.Item))
            .Where(x => x != null)
            .GroupBy(x => x!, ReferenceEqualityComparer.Instance)
            .ToDictionary(x => x.Key, x => x.Count(), ReferenceEqualityComparer.Instance);

        foreach (var state in states)
        {
            var pageSpace = new StructuredPageSpace(state.Page);
            state.TextCandidates = Enum.GetValues<Granularity>()
                .ToDictionary(
                    granularity => granularity,
                    granularity => (IReadOnlyList<TextRemediationCandidate>)state.StructuredText
                        .GetCandidates(granularity)
                        .Select(candidate => candidate.WithContentOrderIndex(
                            candidate.SourceReferences.Count == 0
                                ? candidate.SequenceIndex
                                : candidate.SourceReferences.Min(source => source.OperatorStart)) with
                        {
                            PageIndex = state.PageIndex
                        })
                        .ToArray());
            state.AnnotationCandidates = BuildAnnotationCandidates(state, pageSpace);
            state.ContentCandidates = allItems
                .Where(x => ReferenceEquals(x.State, state))
                .Select((x, index) =>
                {
                    var resource = GetResourceObject(x.Item);
                    return new ContentRemediationCandidate(
                        GetCandidateKind(x.Item)!.Value,
                        x.Item,
                        x.Item.GetBoundingBox(),
                        pageSpace.Normalize(x.Item.GetBoundingBox()),
                        x.Item.SourceReference?.OperatorStart ?? index,
                        resource != null && resourceCounts.TryGetValue(resource, out var count) ? count : 1,
                        GetStableResourceIdentity(resource),
                        FindResourceName(state.Page, resource))
                    {
                        PageIndex = state.PageIndex
                    };
                })
                .ToArray();
        }
    }

    private static IReadOnlyList<AnnotationRemediationCandidate> BuildAnnotationCandidates(
        PageRemediationState state,
        StructuredPageSpace pageSpace)
    {
        var annotations = state.Page.NativeObject.Get<PdfArray>(PdfName.Annots);
        if (annotations == null) return Array.Empty<AnnotationRemediationCandidate>();
        var result = new List<AnnotationRemediationCandidate>();
        for (var index = 0; index < annotations.Count; index++)
        {
            if (annotations[index].Resolve() is not PdfDictionary annotation) continue;
            var subtype = annotation.Get<PdfName>(PdfName.Subtype)?.Value ?? "Unknown";
            var flags = (int?)annotation.Get<PdfNumber>(PdfName.F) ?? 0;
            var hidden = (flags & 2) != 0 || (flags & 32) != 0;
            PdfRect<double>? bounds = null;
            PdfRect<double>? relative = null;
            var offPage = false;
            if (annotation.Get<PdfArray>(PdfName.Rect) is { } rectArray && rectArray.Count >= 4)
            {
                var rect = new PdfRectangle(rectArray);
                bounds = new PdfRect<double>((double)rect.LLx, (double)rect.LLy, (double)rect.URx, (double)rect.URy);
                relative = pageSpace.Normalize(bounds);
                var box = state.Page.CropBox;
                offPage = rect.URx <= box.LLx || rect.LLx >= box.URx ||
                    rect.URy <= box.LLy || rect.LLy >= box.URy;
            }
            var (destinationKind, destinationValue) = DescribeAnnotationDestination(annotation);
            result.Add(new AnnotationRemediationCandidate(
                annotation, index, subtype, bounds, relative, flags, hidden, offPage,
                annotation.ContainsKey(PdfName.StructParent),
                annotation.Get<PdfString>(PdfName.Contents)?.Value,
                destinationKind, destinationValue, int.MaxValue / 2 + index)
            {
                PageIndex = state.PageIndex
            });
        }
        return result;
    }

    private static (AnnotationDestinationKind Kind, string? Value) DescribeAnnotationDestination(PdfDictionary annotation)
    {
        if (annotation.TryGetValue(PdfName.Dest, out var direct) && direct != null)
        {
            return (AnnotationDestinationKind.Internal, DescribeDestinationValue(direct.Resolve()));
        }
        var action = annotation.Get<PdfDictionary>(PdfName.A);
        if (action == null) return (AnnotationDestinationKind.None, null);
        var kind = action.Get<PdfName>(PdfName.S)?.Value;
        if (string.Equals(kind, "URI", StringComparison.Ordinal))
        {
            return (AnnotationDestinationKind.Uri, action.Get<PdfString>((PdfName)"URI")?.Value);
        }
        if (string.Equals(kind, "GoTo", StringComparison.Ordinal))
        {
            return (AnnotationDestinationKind.Internal, DescribeDestinationValue(action.Get((PdfName)"D")?.Resolve()));
        }
        if (string.Equals(kind, "GoToR", StringComparison.Ordinal))
        {
            return (AnnotationDestinationKind.Remote, DescribeDestinationValue(action.Get((PdfName)"D")?.Resolve()));
        }
        return (AnnotationDestinationKind.Other, kind);
    }

    private static string? DescribeDestinationValue(IPdfObject? destination) => destination switch
    {
        PdfString text => text.Value,
        PdfName name => name.Value,
        PdfArray array when array.Count > 1 => array[1].Resolve() is PdfName mode ? mode.Value : "array",
        null => null,
        _ => destination.ToString()
    };

    private static ContentRemediationCandidate CreateContentCandidate(
        PageRemediationState pageState,
        IContentItem<double> item,
        int fallbackSequenceIndex) =>
        pageState.ContentCandidates.FirstOrDefault(x => ReferenceEquals(x.Item, item)) ??
        new ContentRemediationCandidate(
            GetCandidateKind(item)!.Value,
            item,
            item.GetBoundingBox(),
            new StructuredPageSpace(pageState.Page).Normalize(item.GetBoundingBox()),
            item.SourceReference?.OperatorStart ?? fallbackSequenceIndex,
            1,
            GetStableResourceIdentity(GetResourceObject(item)),
            FindResourceName(pageState.Page, GetResourceObject(item)))
        {
            PageIndex = pageState.PageIndex
        };

    private static string? FindResourceName(PdfPage page, object? resource)
    {
        if (resource == null)
        {
            return null;
        }

        foreach (var category in new[] { PdfName.XObject, PdfName.Shading })
        {
            if (!page.Resources.TryGet<PdfDictionary>(category, out var resources) || resources == null)
            {
                continue;
            }

            foreach (var entry in resources)
            {
                if (ReferenceEquals(entry.Value.Resolve(), resource))
                {
                    return entry.Key.Value;
                }
            }
        }

        return null;
    }

    private static string? GetStableResourceIdentity(object? resource)
    {
        byte[] bytes;
        if (resource is PdfStream stream)
        {
            bytes = stream.Contents.GetDecodedData();
        }
        else if (resource is IPdfObject pdfObject)
        {
            bytes = Encoding.UTF8.GetBytes(
                DescribeResourceObject(pdfObject.Resolve(), new HashSet<IPdfObject>(ReferenceEqualityComparer.Instance)));
        }
        else
        {
            return null;
        }

        var hash = SHA256.HashData(bytes);
        return "sha256:" + Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string DescribeResourceObject(IPdfObject value, HashSet<IPdfObject> visited)
    {
        value = value.Resolve();
        if (!visited.Add(value))
        {
            return "<cycle>";
        }

        return value switch
        {
            PdfName name => "/" + name.Value,
            PdfString text => "(" + text.Value + ")",
            PdfNumber number => number.ToString() ?? "0",
            PdfBoolean boolean => boolean.Value ? "true" : "false",
            PdfArray array => "[" + string.Join(",", array.Select(x => DescribeResourceObject(x, visited))) + "]",
            PdfDictionary dictionary => "<<" + string.Join(
                ",",
                dictionary.OrderBy(x => x.Key.Value, StringComparer.Ordinal)
                    .Select(x => x.Key.Value + ":" + DescribeResourceObject(x.Value, visited))) + ">>",
            _ => value.ToString() ?? value.Type.ToString()
        };
    }

    private static RemediationCandidateKind? GetCandidateKind(IContentItem<double> item) => item switch
    {
        ImageContent<double> => RemediationCandidateKind.Image,
        PathSequence<double> => RemediationCandidateKind.Path,
        FormContent<double> => RemediationCandidateKind.Form,
        ShadingContent<double> => RemediationCandidateKind.Shading,
        _ => null
    };

    private static object? GetResourceObject(IContentItem<double> item) => item switch
    {
        ImageContent<double> image => image.Stream,
        FormContent<double> form => form.Stream,
        ShadingContent<double> shading => shading.Shading,
        _ => null
    };

    private static bool IsPaintingItem(IContentItem<double> item) => item switch
    {
        PathSequence<double> path => path.Closing != null && path.Closing is not n_Op<double>,
        ImageContent<double> or FormContent<double> or ShadingContent<double> or TextContent<double> => true,
        _ => false
    };

    /// <summary>Declared page furniture for the selected program.</summary>
    private IReadOnlyList<ArtifactDeclaration> ArtifactInventory =>
        _artifactInventory ??= _program != null
            ? _program.Program.Artifacts
            : Array.Empty<ArtifactDeclaration>();

    private RemediationTemplateDifference ReportArtifactDifference(
        RemediationTemplateDifference difference,
        List<string> diagnostics)
    {
        var page = difference.PageIndexes.Count > 0 ? $"Page{difference.PageIndexes[0] + 1}" : "*";
        var scope = $"Artifact:{difference.SlotId ?? "Undeclared"}:{page}";
        var suppressed = Configuration.DiagnosticStrictness != RemediationDiagnosticStrictness.Strict &&
            _suppressions.Any(x => x.Code == difference.DiagnosticCode &&
                (x.Scope == "*" || x.Scope == scope));
        var message = difference.Kind == RemediationTemplateDifferenceKind.UndeclaredArtifact
            ? $"Artifact at '{difference.ActualPath}' ({difference.ActualValue}) matches no declared artifact inventory item."
            : $"Artifact '{difference.SlotId}' expected {difference.ExpectedValue} on {page.ToLowerInvariant()}, found {difference.ActualValue}.";
        ReportDiagnostic(difference.DiagnosticCode, scope, message, diagnostics);
        return difference with { Suppressed = suppressed };
    }

    private IReadOnlyList<ArtifactDeclaration>? _artifactInventory;

    private RemediationEvaluationContext CreateDocumentEvaluationContext(
        PageRemediationState pageState,
        IReadOnlyList<RemediationClaim> claims,
        IReadOnlyDictionary<string, RemediationAnchor> anchors,
        IReadOnlyDictionary<string, TolerancedZone> tolerancedZones,
        IReadOnlyDictionary<string, FlowRegion> flowRegions,
        DocumentFlowIndex documentFlows,
        List<string> diagnostics)
    {
        return new RemediationEvaluationContext(
            claims,
            pageBox: pageState.StructuredText.RelativePageBox,
            pageIndex: pageState.PageIndex,
            pageCount: _document.Pages.Count,
            configuration: Configuration,
            anchors: anchors,
            tolerancedZones: tolerancedZones,
            flowRegions: flowRegions,
            resolvedFlowRegions: documentFlows.ForPage(pageState.PageIndex),
            structuredText: pageState.StructuredText,
            diagnostics: diagnostics,
            flowsArePreResolved: true)
        {
            DocumentFlows = documentFlows,
            DocumentCandidates = documentFlows.Candidates
        };
    }
    private static bool ContainsNearestPredicate(RemediationPredicate predicate) =>
        predicate switch
        {
            AnchorRelativeRemediationPredicate { Kind: AnchorRelativePredicateKind.NearestTo } => true,
            CompositeRemediationPredicate composite => ContainsNearestPredicate(composite.Left) || ContainsNearestPredicate(composite.Right),
            NotRemediationPredicate not => ContainsNearestPredicate(not.Inner),
            _ => false
        };
    private void ApplyLeftoverPolicy(
        PageRemediationState pageState,
        TextOwnershipIndex ownedTargets,
        List<string> diagnostics,
        List<RemediationUnaccountedContent> unaccountedContent,
        TextNormalizationOptions? normalization = null)
    {
        var textNormalization = normalization ?? TextNormalizationOptions.Default;
        var leftovers = EnumerateItems(pageState.WorkingContent)
            .OfType<TextContent<double>>()
            .Where(x => x.SourceReference is { })
            .SelectMany(item =>
            {
                var source = new SourceTextSpan(
                    item.SourceReference!.Value,
                    item.SourceCharacterOffset,
                    item.Text.Length);
                return ownedTargets.GetUnowned(source).Select(span => (Item: item, Span: span));
            })
            .ToList();
        // Program ownership is character-exact, so word bindings can leave only operator
        // whitespace behind. Normalization-empty spans are not rendered semantic content.
        if (normalization != null)
        {
            leftovers = leftovers
                .Where(x => !string.IsNullOrWhiteSpace(textNormalization.Normalize(GetText(x.Item, x.Span))))
                .ToList();
        }
        var graphicalLeftovers = EnumerateItems(pageState.WorkingContent)
            .Where(IsPaintingItem)
            .Where(x => x is not TextContent<double>)
            .Where(x => !pageState.ContentOwnership.ContainsKey(x))
            .ToList();
        if (leftovers.Count == 0 && graphicalLeftovers.Count == 0)
        {
            return;
        }

        {
            foreach (var (item, span) in leftovers)
            {
                var localStart = span.StartCharacterIndex - item.SourceCharacterOffset;
                var text = localStart >= 0 && localStart + span.CharacterCount <= item.Text.Length
                    ? item.Text.Substring(localStart, span.CharacterCount)
                    : item.Text;
                var templateCandidate = pageState.StructuredText.GetCandidates(Granularity.Paragraph)
                        .FirstOrDefault(x => x.SourceReferences.Contains(span.SourceReference)) ??
                    pageState.StructuredText.GetCandidates(Granularity.Line)
                        .FirstOrDefault(x => x.SourceReferences.Contains(span.SourceReference));
                var range = new RemediationTextRange(
                        span.SourceReference,
                        span.StartCharacterIndex,
                        span.CharacterCount,
                        text);
                var characters = pageState.StructuredText.Characters
                        .Where(x => x.SourceReference == span.SourceReference &&
                                    x.SourceCharacterIndex >= span.StartCharacterIndex &&
                                    x.SourceCharacterIndex < span.EndCharacterIndex)
                        .ToArray();
                var candidate = templateCandidate == null
                    ? null
                    : RemediationCandidate.CreateExactRange(templateCandidate, range, characters);
                var bounds = candidate?.BoundingBox ?? item.GetBoundingBox();
                var relativeBounds = candidate?.RelativeBoundingBox ??
                    new StructuredPageSpace(pageState.Page).Normalize(bounds);
                var candidateId = candidate?.CandidateId ??
                    $"RawText:{pageState.PageIndex}:{span.SourceReference.StreamId}:" +
                    $"{span.SourceReference.OperatorStart}:{span.StartCharacterIndex}:{span.CharacterCount}";
                unaccountedContent.Add(new RemediationUnaccountedContent(
                    pageState.PageIndex,
                    RemediationCandidateKind.Text,
                    candidateId,
                    span.SourceReference,
                    bounds,
                    relativeBounds,
                    text,
                    textNormalization.Normalize(text)));
            }

            foreach (var item in graphicalLeftovers)
            {
                var candidate = CreateContentCandidate(pageState, item, int.MaxValue);
                unaccountedContent.Add(new RemediationUnaccountedContent(
                    pageState.PageIndex,
                    candidate.Kind,
                    candidate.CandidateId,
                    item.SourceReference ?? default,
                    candidate.BoundingBox,
                    candidate.RelativeBoundingBox,
                    ResourceIdentity: candidate.ResourceIdentity,
                    ResourceName: candidate.ResourceName,
                    ResourceUseCount: candidate.ResourceUseCount));
            }

            var typeCounts = unaccountedContent
                .Where(x => x.PageIndex == pageState.PageIndex)
                .GroupBy(x => x.CandidateKind)
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key}={x.Count()}");
            ReportDiagnostic(
                DiagnosticCode.PrescriptiveUnaccountedContent,
                $"Page{pageState.PageIndex + 1}",
                $"Prescriptive template left painting content on page {pageState.PageIndex + 1} without a slot or declared artifact inventory item: " +
                string.Join(", ", typeCounts) + ".",
                diagnostics);
            return;
        }

        static string GetText(TextContent<double> item, SourceTextSpan span)
        {
            var localStart = span.StartCharacterIndex - item.SourceCharacterOffset;
            return localStart >= 0 && localStart + span.CharacterCount <= item.Text.Length
                ? item.Text.Substring(localStart, span.CharacterCount)
                : item.Text;
        }
    }

    private void ApplyClassifyClaim(
        PageRemediationState pageState,
        RemediationClaim claim,
        List<string> diagnostics)
    {
        if (claim.Action is not (ArtifactRemediationAction or BindTemplateSlotRemediationAction))
        {
            return;
        }

        foreach (var candidate in claim.Candidates)
        {
            IReadOnlyList<IContentItem<double>> leaves;
            try
            {
                leaves = candidate.MaterializeLeaves(pageState.WorkingContent);
            }
            catch (Exception ex)
            {
                diagnostics.Add($"Rule '{claim.RuleId}' failed to materialize targets: {ex.Message}");
                continue;
            }

            if (leaves.Count == 0)
            {
                diagnostics.Add($"Rule '{claim.RuleId}' matched content but resolved no leaves.");
                continue;
            }

            if (claim.Action is BindTemplateSlotRemediationAction)
            {
                var tagName = (PdfName)claim.Tag;
                var mcid = AllocateMcid(pageState.Page);
                var marked = new MarkedContent(tagName)
                {
                    InlineProps = new PdfDictionary { [PdfName.MCID] = new PdfIntNumber(mcid) }
                };
                var wrapper = pageState.WorkingContent.Wrap(leaves, marked);
                var node = Structure.AddElement(tagName.Value).GetNode();
                if (Configuration.DebugWrite)
                {
                    node.Title = claim.RuleId;
                }

                BindMarkedContent(node, pageState.Page, mcid);
                claim.AddAppliedBinding(new RemediationAppliedBinding(
                    tagName.Value,
                    new[] { mcid },
                    node,
                    wrapper,
                    node.Parent,
                    candidate.SourceReferences,
                    candidate.BoundingBox));
                pageState.MarkDirty();
            }
            else if (claim.Action is ArtifactRemediationAction artifact)
            {
                var wrapper = pageState.WorkingContent.Wrap(
                    leaves,
                    new MarkedContent(PdfName.Artifact)
                    {
                        InlineProps = BuildArtifactProperties(artifact, candidate.BoundingBox)
                    });
                claim.AddAppliedBinding(new RemediationAppliedBinding(
                    PdfName.Artifact.Value,
                    Array.Empty<int>(),
                    null,
                    wrapper,
                    null,
                    candidate.SourceReferences,
                    candidate.BoundingBox));
                pageState.MarkDirty();
            }
        }
    }

    private static void ValidateClaimTargets(
        PageRemediationState pageState,
        RemediationClaim claim,
        List<string> diagnostics)
    {
        if (claim.Action is not (ArtifactRemediationAction or BindTemplateSlotRemediationAction))
        {
            return;
        }

        var allTargets = new List<RemediationClaimTarget<double>>();
        foreach (var candidate in claim.Candidates)
        {
            foreach (var sourceReference in candidate.SourceReferences)
            {
                if (!ContentModelBridge.TryResolveParsedItemId(pageState.WorkingContent, sourceReference, out _))
                {
                    diagnostics.Add($"Rule '{claim.RuleId}' on page {pageState.PageIndex + 1} references content source {sourceReference} that no longer resolves.");
                }
            }

            var targets = candidate.FindTargets(pageState.WorkingContent);
            if (targets.Count == 0)
            {
                diagnostics.Add($"Rule '{claim.RuleId}' on page {pageState.PageIndex + 1} matched content but resolved no leaves.");
                continue;
            }

            ValidateTextRangeTargets(pageState, claim, targets, diagnostics);
            allTargets.AddRange(targets);
        }

        if (allTargets.Count > 0)
        {
            ValidateContiguousTargets(pageState, claim, allTargets.Select(x => x.Item), diagnostics);
        }
    }

    private static void ValidateTextRangeTargets(
        PageRemediationState pageState,
        RemediationClaim claim,
        IReadOnlyList<RemediationClaimTarget<double>> targets,
        List<string> diagnostics)
    {
        foreach (var target in targets)
        {
            if (target.TextRange == null)
            {
                continue;
            }

            if (target.Item is not TextContent<double> textContent)
            {
                diagnostics.Add($"Rule '{claim.RuleId}' on page {pageState.PageIndex + 1} has a text range target that is not text content.");
                continue;
            }

            if (!textContent.TrySplitByCharacterRange(
                target.TextRange.StartCharacterIndex - textContent.SourceCharacterOffset,
                target.TextRange.CharacterCount,
                out _,
                out _,
                out _,
                out var error))
            {
                diagnostics.Add($"Rule '{claim.RuleId}' on page {pageState.PageIndex + 1} cannot materialize text range {target.TextRange.StartCharacterIndex}+{target.TextRange.CharacterCount}: {error}");
            }
        }
    }

    private static void ValidateContiguousTargets(
        PageRemediationState pageState,
        RemediationClaim claim,
        IEnumerable<IContentItem<double>> targetItems,
        List<string> diagnostics)
    {
        var selectedItems = targetItems.Distinct().ToList();
        if (selectedItems.Count <= 1)
        {
            return;
        }

        var itemIndexes = FlattenItems(pageState.WorkingContent)
            .Select((item, index) => (item, index))
            .ToDictionary(x => x.item, x => x.index);
        var indexes = new List<int>();
        foreach (var item in selectedItems)
        {
            if (!itemIndexes.TryGetValue(item, out var index))
            {
                diagnostics.Add($"Rule '{claim.RuleId}' on page {pageState.PageIndex + 1} selected a leaf that does not belong to the page tree.");
                return;
            }

            indexes.Add(index);
        }

        indexes.Sort();
        if (indexes[^1] - indexes[0] + 1 != indexes.Count)
        {
            diagnostics.Add($"Rule '{claim.RuleId}' on page {pageState.PageIndex + 1} selected non-contiguous leaves.");
        }
    }

    private void ApplyPrescriptiveTemplateAssembly(
        PrescriptiveTemplateAssemblyPlan plan,
        IReadOnlyList<PageRemediationState> pageStates,
        List<string> diagnostics,
        IReadOnlyList<RemediationClaim>? executionClaims = null)
    {
        var claims = (executionClaims ?? Array.Empty<RemediationClaim>())
            .Where(x => x.Status == ClaimStatus.Applied)
            .DistinctBy(x => x.ClaimId)
            .ToArray();
        var claimsById = claims.ToDictionary(x => x.ClaimId);
        var syntheticBySlot = claims
            .Where(x => x.RuleId.StartsWith("__template__:", StringComparison.Ordinal) && x.SlotId != null)
            .GroupBy(x => x.SlotId!, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => new Queue<RemediationClaim>(x), StringComparer.Ordinal);
        var existingIds = new Dictionary<string, StructureNode>(StringComparer.Ordinal);

        void IndexExisting(StructureNode node)
        {
            if (node.ID != null)
            {
                if (node.ID.StartsWith("template:", StringComparison.Ordinal))
                {
                    ReportDiagnostic(
                        DiagnosticCode.TemplateIdentityCollision,
                        $"TemplateIdentity:{node.ID}",
                        $"Pre-existing structure /ID '{node.ID}' uses the reserved prescriptive-template namespace.",
                        diagnostics);
                }
                else if (!existingIds.TryAdd(node.ID, node))
                {
                    ReportDiagnostic(
                        DiagnosticCode.TemplateIdentityCollision,
                        $"TemplateIdentity:{node.ID}",
                        $"Structure /ID '{node.ID}' is already duplicated before template assembly.",
                        diagnostics);
                }
            }
            foreach (var child in node.Children) IndexExisting(child);
        }

        var documentRoot = Structure.GetRoot();
        var documentProperties = plan.Document.ProgramTemplate?.Properties;
        documentRoot.Language = documentProperties?.Language ?? documentRoot.Language;
        documentRoot.Alt = documentProperties?.AlternateText ?? documentRoot.Alt;
        documentRoot.ActualText = documentProperties?.ActualText ?? documentRoot.ActualText;
        documentRoot.Expansion = documentProperties?.Expansion ?? documentRoot.Expansion;
        IndexExisting(documentRoot);
        if (HasUnsuppressedDiagnostics(diagnostics)) return;

        IReadOnlyList<StructureNode> MaterializeChildren(
            IReadOnlyList<PrescriptiveTemplateAssemblyNode> planned,
            StructureNode parent)
        {
            var ordered = new List<StructureNode>();
            foreach (var occurrence in planned)
            {
                if (occurrence.ProgramTemplate == null) continue;
                var occurrenceTag = occurrence.ProgramTemplate!.Tag;
                var occurrenceProperties = occurrence.ProgramTemplate!.Properties;
                var occurrenceSlotId = occurrence.SlotReference?.Path[1..];

                RemediationClaim? claim = null;
                if (occurrence.ClaimId is { } claimId) claimsById.TryGetValue(claimId, out claim);
                if (claim == null) claimsById.TryGetValue(new ClaimId(occurrence.Identity), out claim);
                if (claim == null && syntheticBySlot.TryGetValue(occurrenceSlotId!, out var queue) && queue.Count > 0)
                {
                    claim = queue.Dequeue();
                }

                var bindings = claim?.AppliedBindings.Where(x =>
                        x.StructureNode != null &&
                        string.Equals(x.ProducedTag, occurrenceTag, StringComparison.Ordinal))
                    .ToArray() ?? Array.Empty<RemediationAppliedBinding>();
                if (bindings.Length > 1)
                {
                    diagnostics.Add($"Template occurrence '{occurrence.Identity}' has {bindings.Length} structure bindings; exactly one is required.");
                    return ordered;
                }

                StructureNode node;
                if (bindings.Length == 1)
                {
                    node = bindings[0].StructureNode!;
                }
                else if (occurrence.Source == null)
                {
                    node = Structure.AddElement(occurrenceTag).GetNode();
                    claim?.AddAppliedBinding(new RemediationAppliedBinding(
                        occurrenceTag,
                        Array.Empty<int>(),
                        node,
                        null,
                        parent,
                        Array.Empty<StructuredSourceRef>(),
                        null));
                }
                else
                {
                    diagnostics.Add($"Bound template occurrence '{occurrence.Identity}' has no materialized structure node.");
                    return ordered;
                }

                if (node.ID != null && !string.Equals(node.ID, occurrence.Identity, StringComparison.Ordinal))
                {
                    ReportDiagnostic(
                        DiagnosticCode.TemplateIdentityCollision,
                        $"TemplateIdentity:{occurrence.Identity}",
                        $"Template occurrence '{occurrence.Identity}' would overwrite existing structure /ID '{node.ID}'.",
                        diagnostics);
                    return ordered;
                }
                if (existingIds.TryGetValue(occurrence.Identity, out var owner) && !ReferenceEquals(owner, node))
                {
                    ReportDiagnostic(
                        DiagnosticCode.TemplateIdentityCollision,
                        $"TemplateIdentity:{occurrence.Identity}",
                        $"Template occurrence identity '{occurrence.Identity}' is already owned by another structure node.",
                        diagnostics);
                    return ordered;
                }
                node.ID = occurrence.Identity;
                existingIds[occurrence.Identity] = node;
                node.Language = occurrenceProperties.Language ?? node.Language;
                node.Alt = occurrenceProperties.AlternateText ?? node.Alt;
                node.ActualText = occurrenceProperties.ActualText ?? node.ActualText;
                node.Expansion = occurrenceProperties.Expansion ?? node.Expansion;
                if (!ReferenceEquals(node.Parent, parent)) Structure.ReparentStructureNode(node, parent);

                if (!occurrence.OpaqueInterior)
                {
                    var childOrder = MaterializeChildren(occurrence.Children, node);
                    Reorder(node, childOrder);
                }
                ordered.Add(node);
            }
            return ordered;
        }

        static void Reorder(StructureNode parent, IReadOnlyList<StructureNode> desired)
        {
            if (desired.Count == 0) return;
            var desiredSet = desired.ToHashSet(ReferenceEqualityComparer.Instance);
            var stableExtras = parent.Children.Where(x => !desiredSet.Contains(x)).ToArray();
            parent.Children.Clear();
            parent.Children.AddRange(desired);
            parent.Children.AddRange(stableExtras);
        }

        var rootOrder = MaterializeChildren(plan.Document.Children, documentRoot);
        Reorder(documentRoot, rootOrder);
    }

    private void CommitDocumentChanges(Action commit)
    {
        var checkpoint = PdfMutationCheckpoint.Capture(_document);
        var originalStructure = _document.ExistingStructure;
        try
        {
            commit();
        }
        catch
        {
            checkpoint.Restore();
            _document.Structure = originalStructure!;
            throw;
        }
    }

    private sealed class PdfMutationCheckpoint
    {
        private readonly List<(PdfDictionary Target, PdfDictionary Snapshot)> _dictionaries;
        private readonly List<(PdfArray Target, PdfArray Snapshot)> _arrays;

        private PdfMutationCheckpoint(
            List<(PdfDictionary Target, PdfDictionary Snapshot)> dictionaries,
            List<(PdfArray Target, PdfArray Snapshot)> arrays)
        {
            _dictionaries = dictionaries;
            _arrays = arrays;
        }

        public static PdfMutationCheckpoint Capture(PdfDocument document)
        {
            var dictionaries = new List<(PdfDictionary, PdfDictionary)>();
            var arrays = new List<(PdfArray, PdfArray)>();
            var seenDictionaries = new HashSet<PdfDictionary>(ReferenceEqualityComparer.Instance);
            var seenArrays = new HashSet<PdfArray>(ReferenceEqualityComparer.Instance);

            void Visit(IPdfObject? value)
            {
                if (value == null) return;
                var resolved = value.Resolve();
                switch (resolved)
                {
                    case PdfDictionary dictionary when seenDictionaries.Add(dictionary):
                        var dictionarySnapshot = dictionary.CloneShallow();
                        dictionaries.Add((dictionary, dictionarySnapshot));
                        foreach (var item in dictionary) Visit(item.Value);
                        break;
                    case PdfArray array when seenArrays.Add(array):
                        arrays.Add((array, array.CloneShallow()));
                        foreach (var item in array) Visit(item);
                        break;
                    case PdfStream stream:
                        Visit(stream.Dictionary);
                        break;
                }
            }

            Visit(document.Catalog);
            foreach (var page in document.Pages) Visit(page.NativeObject);
            return new PdfMutationCheckpoint(dictionaries, arrays);
        }

        public void Restore()
        {
            for (var i = _arrays.Count - 1; i >= 0; i--)
            {
                var (target, snapshot) = _arrays[i];
                target.Clear();
                foreach (var item in snapshot) target.Add(item);
            }
            for (var i = _dictionaries.Count - 1; i >= 0; i--)
            {
                RestoreDictionary(_dictionaries[i].Target, _dictionaries[i].Snapshot);
            }
        }
    }

    private static void RestoreDictionary(PdfDictionary target, PdfDictionary snapshot)
    {
        target.Clear();
        foreach (var item in snapshot)
        {
            target[item.Key] = item.Value;
        }
    }

    private static PdfDictionary BuildArtifactProperties(
        ArtifactRemediationAction artifact,
        PdfRect<double> bounds)
    {
        var properties = new PdfDictionary
        {
            [PdfName.TYPE] = (PdfName)artifact.Subtype.ToString()
        };
        if (artifact.SemanticSubtype != null)
        {
            properties[PdfName.Subtype] = (PdfName)artifact.SemanticSubtype.Value.ToString();
        }
        if (artifact.IncludeBoundingBox)
        {
            properties[PdfName.BBox] = PdfRectangle.FromContentModel(bounds).NativeObject;
        }
        if (artifact.Attached is { Count: > 0 })
        {
            properties[(PdfName)"Attached"] = new PdfArray(
                artifact.Attached.Select(x => (IPdfObject)(PdfName)x.ToString()).ToList());
        }
        return properties;
    }

    private void RunDiagnostics(IReadOnlyList<PageRemediationState> pageStates, List<string> diagnostics)
    {
        foreach (var pageState in pageStates)
        {
            var pageScope = $"Page{pageState.PageIndex + 1}";
            CheckUntaggedContent(pageState, pageScope, diagnostics);
            CheckMcidIntegrity(pageState, pageScope, diagnostics);
        }

        CheckStructParents(pageStates, diagnostics);
    }

    private void CheckUntaggedContent(PageRemediationState pageState, string scope, List<string> diagnostics)
    {
        var untagged = new List<IContentItem<double>>();
        CheckUntaggedRecursive(pageState.WorkingContent, false, untagged);

        var painting = untagged.Where(IsPaintingItem).ToList();
        if (painting.Count > 0)
        {
            var counts = painting
                .GroupBy(x => GetCandidateKind(x)?.ToString() ?? x.Type.ToString())
                .OrderBy(x => x.Key, StringComparer.Ordinal)
                .Select(x => $"{x.Key}={x.Count()}");
            var msg =
                $"Page {pageState.PageIndex + 1} has painting content outside marked content: " +
                string.Join(", ", counts) + ".";
            ReportDiagnostic(DiagnosticCode.UntaggedContent, scope, msg, diagnostics);
        }
    }

    private void CheckUntaggedRecursive(IEnumerable<IContentNode<double>> nodes, bool inBdc, List<IContentItem<double>> untagged)
    {
        foreach (var node in nodes)
        {
            if (node is MarkedContentGroup<double> marked)
            {
                CheckUntaggedRecursive(marked.Children, true, untagged);
            }
            else if (node is IContentItem<double> item)
            {
                if (!inBdc)
                {
                    // Ignore items that don't paint anything, but typically we want everything inside
                    // For now just add all
                    untagged.Add(item);
                }
            }
        }
    }

    private void CheckMcidIntegrity(PageRemediationState pageState, string scope, List<string> diagnostics)
    {
        // 13.2 "every MCID referenced by exactly one structure element" check.
        // Get all MCIDs from WorkingContent
        var mcidsInContent = new HashSet<int>();
        var duplicateMcidsInContent = new HashSet<int>();

        void CollectMcids(IEnumerable<IContentNode<double>> nodes)
        {
            foreach (var node in nodes)
            {
                if (node is MarkedContentGroup<double> marked)
                {
                    if (marked.Tag.InlineProps?.TryGetValue(PdfName.MCID, out var mcidObj) == true && mcidObj is PdfIntNumber mcidNum)
                    {
                        var mcid = mcidNum.Value;
                        if (!mcidsInContent.Add(mcid))
                        {
                            duplicateMcidsInContent.Add(mcid);
                        }
                    }
                    CollectMcids(marked.Children);
                }
            }
        }

        CollectMcids(pageState.WorkingContent);

        // Get all MCIDs referenced by structure elements on this page
        var referencedMcids = new HashSet<int>();
        var duplicateReferences = new HashSet<int>();

        void CollectReferences(StructureNode node)
        {
            foreach (var contentItem in node.ContentItems)
            {
                if (contentItem.Page == pageState.Page)
                {
                    if (!referencedMcids.Add(contentItem.MCID))
                    {
                        duplicateReferences.Add(contentItem.MCID);
                    }
                }
            }
            foreach (var child in node.Children)
            {
                CollectReferences(child);
            }
        }

        CollectReferences(Structure.GetRoot());

        foreach (var dup in duplicateMcidsInContent)
        {
            ReportDiagnostic(DiagnosticCode.DuplicatedMcid, scope, $"MCID {dup} appears multiple times in the content stream on page {pageState.PageIndex + 1}.", diagnostics);
        }

        foreach (var dup in duplicateReferences)
        {
            ReportDiagnostic(DiagnosticCode.DuplicatedMcid, scope, $"MCID {dup} on page {pageState.PageIndex + 1} is referenced by multiple structure elements.", diagnostics);
        }

        foreach (var mcid in mcidsInContent)
        {
            if (!referencedMcids.Contains(mcid))
            {
                ReportDiagnostic(DiagnosticCode.OrphanedMcid, scope, $"MCID {mcid} on page {pageState.PageIndex + 1} is not referenced by any structure element.", diagnostics);
            }
        }

        foreach (var mcid in referencedMcids)
        {
            if (!mcidsInContent.Contains(mcid))
            {
                ReportDiagnostic(DiagnosticCode.OrphanedMcid, scope, $"Structure element references MCID {mcid} on page {pageState.PageIndex + 1}, but it is missing from the content stream.", diagnostics);
            }
        }
    }

    private void CheckStructParents(IReadOnlyList<PageRemediationState> pageStates, List<string> diagnostics)
    {
        // 13.3 Implement "`/StructParents` set for pages with claims" check.
        // Actually _pageStructParents tracks this
        foreach (var pageState in pageStates)
        {
            if (_pagesWithAllocatedMcids.Contains(pageState.Page))
            {
                if (!_pageStructParents.ContainsKey(pageState.Page))
                {
                    ReportDiagnostic(DiagnosticCode.MissingStructParents, $"Page{pageState.PageIndex + 1}", $"Page {pageState.PageIndex + 1} has MCIDs but no /StructParents entry.", diagnostics);
                }
            }
        }
    }

    private void ReportDiagnostic(
        DiagnosticCode code,
        string scope,
        string message,
        List<string> diagnostics,
        SlotRef? programSlot = null,
        string? bindingId = null,
        IReadOnlyList<string>? candidateIds = null,
        IReadOnlyDictionary<string, object?>? evidence = null)
    {
        var strict = Configuration.DiagnosticStrictness == RemediationDiagnosticStrictness.Strict;
        var suppression = NonSuppressibleDiagnosticCodes.Contains(code)
            ? null
            : _suppressions.FirstOrDefault(x => x.Code == code && (x.Scope == "*" || x.Scope == scope));

        if (_programRuntimeDiagnostics != null)
        {
            if (suppression != null && !strict)
            {
                _programRuntimeDiagnostics.Add(new RemediationRuntimeDiagnostic(
                    code,
                    RemediationDiagnosticDisposition.Acknowledged,
                    scope,
                    message,
                    programSlot,
                    bindingId,
                    candidateIds,
                    evidence,
                    Suppression: suppression));
                diagnostics.Add($"[SUPPRESSED] {code}: {message} (Reason: {suppression.Reason})");
                return;
            }

            _programRuntimeDiagnostics.Add(new RemediationRuntimeDiagnostic(
                code,
                Configuration.RunMode == RemediationRunMode.Authoring && ProgramWorkItemCodes.Contains(code)
                    ? RemediationDiagnosticDisposition.WorkItem
                    : RemediationDiagnosticDisposition.Error,
                scope,
                message,
                programSlot,
                bindingId,
                candidateIds,
                evidence,
                Suppression: suppression));

            if (suppression != null && strict)
                diagnostics.Add($"[IGNORED-SUPPRESSION] {code}: {message}");
            diagnostics.Add($"{code}: {message}");
            return;
        }
        
        if (suppression != null && !strict)
        {
            diagnostics.Add($"[SUPPRESSED] {code}: {message} (Reason: {suppression.Reason})");
            return;
        }

        if (suppression != null && strict)
        {
            diagnostics.Add($"[IGNORED-SUPPRESSION] {code}: {message}");
        }

        diagnostics.Add($"{code}: {message}");
    }

    private static bool HasUnsuppressedDiagnostics(IEnumerable<string> diagnostics) =>
        diagnostics.Any(x => !x.StartsWith("[SUPPRESSED]", StringComparison.Ordinal));

    private static IReadOnlyList<SourceTextSpan> GetTargetSpans(RemediationCandidate candidate)
    {
        if (candidate is not TextRemediationCandidate textCandidate || textCandidate.TextRanges.Count == 0)
        {
            return Array.Empty<SourceTextSpan>();
        }

        if (textCandidate.RequiresExactMaterialization)
        {
            return textCandidate.TextRanges
                .Select(x => new SourceTextSpan(x.SourceReference, x.StartCharacterIndex, x.CharacterCount))
                .ToArray();
        }

        return textCandidate.TextRanges
            .GroupBy(x => x.SourceReference)
            .Select(group =>
            {
                var start = group.Min(x => x.StartCharacterIndex);
                var end = group.Max(x => x.StartCharacterIndex + x.CharacterCount);
                return new SourceTextSpan(group.Key, start, end - start);
            })
            .ToArray();
    }

    private static IReadOnlyList<RemediationClaim> CreateResidualClaims(
        RemediationClaim previous,
        IReadOnlyList<SourceTextSpan> replacing)
    {
        var residuals = new List<RemediationClaim>();
        foreach (var candidate in previous.Candidates)
        {
            foreach (var span in GetTargetSpans(candidate))
            {
                foreach (var residualSpan in Subtract(span, replacing))
                {
                    var characters = ((TextRemediationCandidate)candidate).Characters
                        .Where(x => x.SourceReference == residualSpan.SourceReference &&
                                    x.SourceCharacterIndex >= residualSpan.StartCharacterIndex &&
                                    x.SourceCharacterIndex < residualSpan.EndCharacterIndex)
                        .OrderBy(x => x.SourceCharacterIndex)
                        .ToList();
                    if (characters.Count == 0)
                    {
                        continue;
                    }

                    var range = new RemediationTextRange(
                        residualSpan.SourceReference,
                        residualSpan.StartCharacterIndex,
                        residualSpan.CharacterCount,
                        new string(characters.Select(x => x.Char).ToArray()));
                    var residualCandidate = RemediationCandidate.CreateExactRange(candidate, range, characters);
                    residuals.Add(new RemediationClaim(
                        previous.RuleId,
                        new[] { residualCandidate },
                        previous.Tag,
                        previous.Confidence)
                    {
                        PageIndex = previous.PageIndex,
                        Status = ClaimStatus.Applied,
                        SelectorDebugString = previous.SelectorDebugString,
                        Action = previous.Action,
                        ProgramId = previous.ProgramId,
                        SlotId = previous.SlotId,
                        TextNormalization = previous.TextNormalization
                    });
                }
            }
        }

        return residuals;
    }

    private static IReadOnlyList<SourceTextSpan> Subtract(
        SourceTextSpan source,
        IReadOnlyList<SourceTextSpan> replacing)
    {
        var residuals = new List<SourceTextSpan> { source };
        foreach (var replacement in replacing.Where(source.Overlaps))
        {
            var next = new List<SourceTextSpan>();
            foreach (var residual in residuals)
            {
                if (!residual.Overlaps(replacement))
                {
                    next.Add(residual);
                    continue;
                }

                if (replacement.StartCharacterIndex > residual.StartCharacterIndex)
                {
                    next.Add(new SourceTextSpan(
                        residual.SourceReference,
                        residual.StartCharacterIndex,
                        replacement.StartCharacterIndex - residual.StartCharacterIndex));
                }

                if (replacement.EndCharacterIndex < residual.EndCharacterIndex)
                {
                    next.Add(new SourceTextSpan(
                        residual.SourceReference,
                        replacement.EndCharacterIndex,
                        residual.EndCharacterIndex - replacement.EndCharacterIndex));
                }
            }

            residuals = next;
        }

        return residuals;
    }

    internal static IComparer<RemediationClaim> ReadingOrderComparer { get; } =
        Comparer<RemediationClaim>.Create(CompareClaimsInReadingOrder);

    internal static int CompareClaimsInReadingOrder(RemediationClaim left, RemediationClaim right)
    {
        var page = left.PageIndex.CompareTo(right.PageIndex);
        if (page != 0)
        {
            return page;
        }

        var leftBounds = left.BoundingBox;
        var rightBounds = right.BoundingBox;
        if (leftBounds != null && rightBounds != null)
        {
            var leftCenterY = (leftBounds.LLy + leftBounds.URy) / 2d;
            var rightCenterY = (rightBounds.LLy + rightBounds.URy) / 2d;
            if (Math.Abs(leftCenterY - rightCenterY) > 6d)
            {
                return rightCenterY.CompareTo(leftCenterY);
            }

            var column = leftBounds.LLx.CompareTo(rightBounds.LLx);
            if (column != 0)
            {
                return column;
            }
        }

        var sequence = left.FirstSequenceIndex.CompareTo(right.FirstSequenceIndex);
        if (sequence != 0)
        {
            return sequence;
        }

        var leftSpan = left.Candidates.SelectMany(GetTargetSpans)
            .OrderBy(x => x.SourceReference.StreamId.ToString(), StringComparer.Ordinal)
            .ThenBy(x => x.SourceReference.OperatorStart)
            .ThenBy(x => x.StartCharacterIndex)
            .FirstOrDefault();
        var rightSpan = right.Candidates.SelectMany(GetTargetSpans)
            .OrderBy(x => x.SourceReference.StreamId.ToString(), StringComparer.Ordinal)
            .ThenBy(x => x.SourceReference.OperatorStart)
            .ThenBy(x => x.StartCharacterIndex)
            .FirstOrDefault();

        var stream = string.Compare(
            leftSpan.SourceReference.StreamId.ToString(),
            rightSpan.SourceReference.StreamId.ToString(),
            StringComparison.Ordinal);
        if (stream != 0)
        {
            return stream;
        }

        var source = leftSpan.SourceReference.OperatorStart.CompareTo(rightSpan.SourceReference.OperatorStart);
        if (source != 0)
        {
            return source;
        }

        var character = leftSpan.StartCharacterIndex.CompareTo(rightSpan.StartCharacterIndex);
        if (character != 0)
        {
            return character;
        }

        var leftCandidate = left.Candidates.Select(x => x.CandidateId)
            .OrderBy(x => x, StringComparer.Ordinal)
            .FirstOrDefault() ?? string.Empty;
        var rightCandidate = right.Candidates.Select(x => x.CandidateId)
            .OrderBy(x => x, StringComparer.Ordinal)
            .FirstOrDefault() ?? string.Empty;
        var candidate = string.Compare(leftCandidate, rightCandidate, StringComparison.Ordinal);
        if (candidate != 0)
        {
            return candidate;
        }

        var rule = string.Compare(left.RuleId, right.RuleId, StringComparison.Ordinal);
        return rule != 0
            ? rule
            : string.Compare(left.ProducedTag, right.ProducedTag, StringComparison.Ordinal);
    }

    private static IEnumerable<IContentItem<double>> EnumerateItems(IEnumerable<IContentNode<double>> nodes)
    {
        foreach (var node in nodes)
        {
            if (node is MarkedContentGroup<double> marked)
            {
                foreach (var child in EnumerateItems(marked.Children))
                {
                    yield return child;
                }

                continue;
            }

            if (node is IContentItem<double> item)
            {
                yield return item;
            }
        }
    }

    private static IEnumerable<IContentItem<double>> FlattenItems(IEnumerable<IContentNode<double>> nodes)
    {
        foreach (var node in nodes)
        {
            if (node is MarkedContentGroup<double> marked)
            {
                foreach (var child in FlattenItems(marked.Children))
                {
                    yield return child;
                }

                continue;
            }

            if (node is IContentItem<double> item)
            {
                yield return item;
            }
        }
    }

    internal int AllocateMcid(PdfPage page)
    {
        ThrowIfDisposed();
        _pagesWithAllocatedMcids.Add(page);
        return McidAllocator.Allocate(page);
    }

    internal int GetOrCreateStructParentsIndex(PdfPage page)
    {
        ThrowIfDisposed();

        if (_pageStructParents.TryGetValue(page, out var existing))
        {
            return existing;
        }

        var index = Structure.GetStructureRoot().AllocateStructParentIndex();
        _pageStructParents[page] = index;
        return index;
    }

    internal void BindMarkedContent(StructureNode node, PdfPage page, int mcid)
    {
        ThrowIfDisposed();
        node.ContentItems.Add((page, mcid));
        GetOrCreateStructParentsIndex(page);
    }

    internal void BindAnnotation(
        StructureNode node,
        PdfPage page,
        PdfDictionary annotation,
        string? accessibleDescription = null,
        StructureNode? destinationTarget = null)
    {
        ThrowIfDisposed();

        var index = Structure.GetStructureRoot().AllocateStructParentIndex();
        annotation[PdfName.StructParent] = new PdfIntNumber(index);
        if (!string.IsNullOrWhiteSpace(accessibleDescription))
        {
            annotation[PdfName.Contents] = PdfString.CreateTextString(accessibleDescription);
        }
        node.ObjectReferences.Add(new StructureObjectReference(annotation, index, page)
        {
            AnnotationContents = accessibleDescription,
            StructureDestinationTarget = destinationTarget
        });
        GetOrCreateStructParentsIndex(page);
    }

    internal void BindImage(StructureNode node, XObjImage image, params PdfPage[] pages)
    {
        ThrowIfDisposed();
        Structure.BindImage(node, image, pages);
        foreach (var page in pages)
        {
            GetOrCreateStructParentsIndex(page);
        }
    }

    internal void BindFormXObject(StructureNode node, XObjForm form, params PdfPage[] pages)
    {
        ThrowIfDisposed();
        Structure.BindFormXObject(node, form, pages);
        foreach (var page in pages)
        {
            GetOrCreateStructParentsIndex(page);
        }
    }

    /// <summary>Releases the remediation session.</summary>
    public void Dispose()
    {
        _pagesWithAllocatedMcids.Clear();
        _pageStructParents.Clear();
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(RemediationSession));
        }
    }
}
