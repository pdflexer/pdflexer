using System;
using PdfLexer.Content;
using PdfLexer.Remediation;
using Xunit;

namespace PdfLexer.Tests;

public class RemediationNormalizationAndTraceTests
{
    [Fact]
    public void DefaultNormalization_FoldsCompatibilityWhitespaceHyphensQuotesAndSoftHyphens()
    {
        var value = "\uFB01\u00AD\u00A0A\u202FB\u2011C\u2018D\u2019\u201CE\u201D";

        Assert.Equal("fi A B-C'D'\"E\"", TextNormalizationOptions.Default.Normalize(value));
    }

    [Fact]
    public void TextLiteral_NormalizesBothOperands_ButRegexPatternIsNotRewritten()
    {
        var candidate = Candidate("INV\u2011123");
        Assert.True(Predicates.Text.Equals("INV-123").Evaluate(candidate).IsMatch);
        Assert.True(Predicates.Text.Matches("^INV-\\d+$").Evaluate(candidate).IsMatch);
        Assert.False(Predicates.Text.Matches("^INV\u2011\\d+$").Evaluate(candidate).IsMatch);
    }

    [Fact]
    public void PredicateOverride_CanDisableNormalization()
    {
        var predicate = new TextRemediationPredicate(
            TextPredicateKind.Equals, "A B", normalization: TextNormalizationOptions.None);

        Assert.False(predicate.Evaluate(Candidate("A\u00A0B")).IsMatch);
    }

    [Fact]
    public void Trace_PreservesAndShortCircuitAndRejectingOperand()
    {
        var predicate = Predicates.Text.StartsWith("X").And(Predicates.Text.Contains("never"));
        var context = new RemediationEvaluationContext().WithPredicateTracing(true);

        var result = predicate.Evaluate(context, Candidate("ABC"));

        Assert.False(result.IsMatch);
        Assert.Equal(1, result.Trace!.RejectingAndOperand);
        Assert.False(result.Trace.Children![1].Evaluated);
        Assert.Equal("ABC", result.Trace.Children[0].EvaluatedText);
    }

    [Fact]
    public void Trace_MixedPredicateChainNeverContainsNullChildren()
    {
        var predicate = Predicates.Font.Size(NumericOperator.GreaterThan, 999)
            .And(Predicates.Text.Contains("A"));
        var context = new RemediationEvaluationContext().WithPredicateTracing(true);

        var result = predicate.Evaluate(context, Candidate("ABC"));

        Assert.False(result.IsMatch);
        Assert.Equal(1, result.Trace!.RejectingAndOperand);
        Assert.All(result.Trace.Children!, Assert.NotNull);
        Walk(result.Trace);

        static void Walk(PredicateTraceNode node)
        {
            foreach (var child in node.Children ?? Array.Empty<PredicateTraceNode>())
            {
                Assert.NotNull(child);
                Walk(child);
            }
        }
    }

    [Fact]
    public void CandidateSelector_RejectsTextAsContentKind()
    {
        Assert.Throws<ArgumentException>(() =>
            CandidateSelector.Content(RemediationCandidateKind.Text));
    }

    [Fact]
    public void RuleSet_PreservesExplicitPerRuleNormalizationOverride()
    {
        var rule = new Rule("raw", RemediationActions.Tag("P"))
        {
            TextNormalization = TextNormalizationOptions.None
        };

        var ruleSet = new RuleSet("set", new[] { rule }, TextNormalizationOptions.Default);

        Assert.Same(TextNormalizationOptions.None, Assert.Single(ruleSet.Rules).TextNormalization);
    }

    [Fact]
    public void RuleValidation_RejectsDivergentGranularityAndSelector()
    {
        var rule = new Rule("text", RemediationActions.Tag("P"), granularity: Granularity.Line)
        {
            Granularity = Granularity.Word
        };

        Assert.Contains(rule.ValidateShape(), x => x.Contains("conflicts", StringComparison.Ordinal));
    }

    private static RemediationCandidate Candidate(string text) =>
        new TextRemediationCandidate(
            Granularity.Line,
            text,
            new PdfRect<double>(0, 0, 10, 10),
            new PdfRect<double>(0, 0, 10, 10),
            Array.Empty<StructuredCharacter>(),
            Array.Empty<StructuredSourceRef>(),
            0,
            12);
}
