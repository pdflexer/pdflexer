using PdfLexer.Content;
using PdfLexer.Content.Model;

namespace PdfLexer.Remediation;

public sealed partial class RemediationSession
{
    private IReadOnlyList<RemediationRegionAbsorptionOutcome> ApplyProgramRegionAccounting(
        CompiledRemediationProgram compiled,
        IReadOnlyList<PageRemediationState> pages,
        DocumentRegionIndex regions,
        List<RemediationClaim> claims,
        List<string> diagnostics)
    {
        if (compiled.Program.RegionAccounting.Count == 0) return Array.Empty<RemediationRegionAbsorptionOutcome>();
        var artifacts = compiled.Program.Artifacts.ToDictionary(x => x.Id, StringComparer.Ordinal);
        var anchors = compiled.Program.Anchors.ToDictionary(x => x.Id, StringComparer.Ordinal);
        var outcomes = new List<RemediationRegionAbsorptionOutcome>();

        foreach (var page in pages)
        {
            var context = CreateProgramEvaluationContext(page, claims, anchors, regions,
                diagnostics, compiled.Program.TextNormalization);

            foreach (var structural in claims.Where(x => x.PageIndex == page.PageIndex &&
                         x.Status == ClaimStatus.Applied && x.Action is BindTemplateSlotRemediationAction))
            foreach (var candidate in structural.Candidates)
            {
                var matching = MatchingAccounting(compiled, regions, context, candidate).ToArray();
                if (matching.Length == 0) continue;
                var accounting = matching.OrderByDescending(x => x.Priority).ThenBy(x => x.Id, StringComparer.Ordinal).First();
                ReportDiagnostic(DiagnosticCode.ProgramRegionAccountingConflict,
                    $"Program:{compiled.Program.Id}:RegionAccounting:{accounting.Id}",
                    $"Region accounting '{accounting.Id}' matches structurally owned candidate '{candidate.CandidateId}'.",
                    diagnostics, structural.ProgramSlot, structural.BindingId, new[] { candidate.CandidateId },
                    new Dictionary<string, object?>
                    {
                        ["regionId"] = accounting.RegionId,
                        ["accountingId"] = accounting.Id,
                        ["owner"] = structural.BindingId ?? structural.RuleId,
                        ["priority"] = accounting.Priority
                    });
            }

            foreach (var candidate in CollectUnownedPaintingCandidates(page, compiled.Program.TextNormalization))
            {
                var matches = MatchingAccounting(compiled, regions, context, candidate)
                    .OrderByDescending(x => x.Priority).ThenBy(x => x.Id, StringComparer.Ordinal).ToArray();
                if (matches.Length == 0) continue;
                var top = matches.Where(x => x.Priority == matches[0].Priority).ToArray();
                if (top.Length != 1)
                {
                    ReportDiagnostic(DiagnosticCode.ProgramRegionAccountingAmbiguous,
                        $"Program:{compiled.Program.Id}:RegionAccounting",
                        $"Candidate '{candidate.CandidateId}' matches {top.Length} region accounting declarations at priority {top[0].Priority}.",
                        diagnostics, candidateIds: new[] { candidate.CandidateId },
                        evidence: new Dictionary<string, object?>
                        {
                            ["accountingIds"] = top.Select(x => x.Id).ToArray(),
                            ["regionIds"] = top.Select(x => x.RegionId).ToArray(),
                            ["priority"] = top[0].Priority
                        });
                    continue;
                }

                var accounting = top[0];
                var artifact = artifacts[accounting.ArtifactId];
                var segment = regions.FindContaining(accounting.RegionId, candidate)!;
                var action = new ArtifactRemediationAction(artifact.Subtype, artifact.SemanticSubtype,
                    artifact.IncludeBoundingBox, artifact.Attached);
                var claim = new RemediationClaim($"region-accounting:{accounting.Id}", new[] { candidate }, "Artifact", segment.Confidence)
                {
                    BindingId = accounting.Id,
                    DefinitionId = accounting.Id,
                    ArtifactId = accounting.ArtifactId,
                    PageIndex = page.PageIndex,
                    Status = ClaimStatus.Applied,
                    SelectorDebugString = $"Region({accounting.RegionId})",
                    Action = action,
                    ProgramId = compiled.Program.Id,
                    TextNormalization = compiled.Program.TextNormalization
                };
                claims.Add(claim);
                page.TextOwnership.Add(claim, GetTargetSpans(candidate));
                if (candidate is ContentRemediationCandidate content) page.ContentOwnership[content.Item] = claim;
                outcomes.Add(new RemediationRegionAbsorptionOutcome(
                    candidate.CandidateId, candidate.Kind,
                    candidate is TextRemediationCandidate text ? compiled.Program.TextNormalization.Normalize(text.Text) : null,
                    accounting.RegionId, accounting.Id, accounting.ArtifactId,
                    artifact.Subtype, artifact.SemanticSubtype, artifact.IncludeBoundingBox, artifact.Attached,
                    page.PageIndex, candidate.RelativeBoundingBox, segment.Confidence,
                    segment.Activation?.ToString()));
            }
        }
        return outcomes;
    }

    private static IEnumerable<RegionArtifactAccounting> MatchingAccounting(
        CompiledRemediationProgram compiled,
        DocumentRegionIndex regions,
        RemediationEvaluationContext context,
        RemediationCandidate candidate)
    {
        foreach (var accounting in compiled.Program.RegionAccounting)
        {
            if (regions.FindContaining(accounting.RegionId, candidate) == null) continue;
            if (candidate.Kind == RemediationCandidateKind.Text)
            {
                if (!accounting.AllowText) continue;
                if (accounting.TextPredicate != null && !accounting.TextPredicate.Evaluate(context, candidate).IsMatch) continue;
            }
            else if (!accounting.CandidateKinds.Contains(candidate.Kind)) continue;
            yield return accounting;
        }
    }

    private IEnumerable<RemediationCandidate> CollectUnownedPaintingCandidates(
        PageRemediationState page,
        TextNormalizationOptions normalization)
    {
        foreach (var item in EnumerateItems(page.WorkingContent).OfType<TextContent<double>>()
                     .Where(x => x.SourceReference is { }))
        {
            var source = new SourceTextSpan(item.SourceReference!.Value, item.SourceCharacterOffset, item.Text.Length);
            foreach (var span in page.TextOwnership.GetUnowned(source))
            {
                var localStart = span.StartCharacterIndex - item.SourceCharacterOffset;
                var text = localStart >= 0 && localStart + span.CharacterCount <= item.Text.Length
                    ? item.Text.Substring(localStart, span.CharacterCount) : item.Text;
                if (string.IsNullOrWhiteSpace(normalization.Normalize(text))) continue;
                var template = page.StructuredText.GetCandidates(Granularity.Paragraph)
                    .Concat(page.StructuredText.GetCandidates(Granularity.Line))
                    .FirstOrDefault(x => x.SourceReferences.Contains(span.SourceReference));
                var range = new RemediationTextRange(span.SourceReference, span.StartCharacterIndex, span.CharacterCount, text);
                var characters = page.StructuredText.Characters.Where(x => x.SourceReference == span.SourceReference &&
                    x.SourceCharacterIndex >= span.StartCharacterIndex && x.SourceCharacterIndex < span.EndCharacterIndex).ToArray();
                var textTemplate = template as TextRemediationCandidate;
                RemediationCandidate candidate = new TextRemediationCandidate(
                    textTemplate?.Granularity ?? Granularity.Character, text, item.GetBoundingBox(),
                    new StructuredPageSpace(page.Page).Normalize(item.GetBoundingBox()), characters,
                    new[] { span.SourceReference }, textTemplate?.ContentOrderIndex ?? int.MaxValue,
                    characters.Length == 0 ? textTemplate?.FontSize ?? 0 : characters.Average(x => x.FontSize),
                    textTemplate?.FontName, textTemplate?.FontWeight, textTemplate?.Italic, textTemplate?.IsGrayish,
                    new[] { range }) { PageIndex = page.PageIndex };
                yield return candidate;
            }
        }
        foreach (var item in EnumerateItems(page.WorkingContent).Where(IsPaintingItem)
                     .Where(x => x is not TextContent<double>)
                     .Where(x => !page.ContentOwnership.ContainsKey(x)))
            yield return CreateContentCandidate(page, item, int.MaxValue);
    }
}
