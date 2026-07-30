using System;
using System.IO;
using System.Linq;
using System.Text;
using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Remediation;
using Xunit;

namespace PdfLexer.Tests;

public class RemediationM4Tests
{
    [Fact]
    public void ExistingLinkAnnotationCanBeAdoptedIntoTaggedLinkText()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(72, 700).Text("Open account").EndText();
        }
        var annotation = new PdfDictionary
        {
            [PdfName.Subtype] = PdfName.Link,
            [PdfName.Rect] = new PdfArray { 70, 695, 160, 715 },
            [PdfName.Contents] = PdfString.CreateTextString("Open account"),
            [PdfName.Border] = new PdfArray { 0, 0, 0 }
        };
        page.NativeObject[PdfName.Annots] = new PdfArray { annotation };

        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false,
            LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact
        });
        var report = session.Commit(
            new Rule("link-text", RemediationActions.Tag("Link"),
                Predicates.Text.Contains("Open account"), CandidateSelector.Text(Granularity.Line)),
            new Rule("adopt-link", RemediationActions.AdoptAnnotation(ClaimPredicates.FromRule("link-text")),
                Predicates.Annotation.Subtype("Link"), CandidateSelector.Annotations()));

        Assert.True(report.Committed);
        Assert.True(annotation.ContainsKey(PdfName.StructParent));
        var link = Assert.Single(doc.Structure.GetRoot().Children, x => x.Type == "Link");
        Assert.Single(link.ObjectReferences);
        var inventory = Assert.Single(report.AnnotationInventory);
        Assert.Equal(RemediationAnnotationDisposition.Applied, inventory.Disposition);
        Assert.Equal("adopt-link", inventory.RuleId);
    }

    [Fact]
    public void DryRunPlansAnnotationWithoutMutatingDictionary()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var annotation = new PdfDictionary
        {
            [PdfName.Subtype] = (PdfName)"Stamp",
            [PdfName.Rect] = new PdfArray { 10, 10, 30, 30 },
            [PdfName.Contents] = PdfString.CreateTextString("Approved")
        };
        page.NativeObject[PdfName.Annots] = new PdfArray { annotation };
        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = true,
            LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact
        });

        var report = session.DryRun(new Rule("stamp", RemediationActions.AdoptAnnotation(),
            Predicates.Annotation.Subtype("Stamp"), CandidateSelector.Annotations()));

        Assert.DoesNotContain(report.Diagnostics, x => x.Contains("UnmodeledAnnotation", StringComparison.Ordinal));
        Assert.False(annotation.ContainsKey(PdfName.StructParent));
        Assert.Equal(RemediationAnnotationDisposition.Planned, Assert.Single(report.AnnotationInventory).Disposition);
    }

    [Fact]
    public void CompleteArtifactPropertiesAreEmitted()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12)
                .TextMove(72, 740).Text("Header").EndText();
        }
        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false,
            LeftoverPolicy = RemediationLeftoverPolicy.FailFast
        });
        var report = session.Commit(new Rule("header", RemediationActions.HeaderArtifact(),
            Predicates.Text.Equals("Header"), CandidateSelector.Text(Granularity.Line)));

        Assert.True(report.Committed);
        var artifact = Assert.Single(page.GetContentNodes<double>().OfType<MarkedContentGroup<double>>(),
            x => x.Tag.Name == PdfName.Artifact);
        var properties = artifact.Tag.InlineProps!;
        Assert.Equal(PdfName.Pagination, properties.Get<PdfName>(PdfName.TYPE));
        Assert.Equal(PdfName.Header, properties.Get<PdfName>(PdfName.Subtype));
        Assert.NotNull(properties.Get<PdfArray>(PdfName.BBox));
        Assert.Equal("Top", Assert.IsType<PdfName>(Assert.Single(
            properties.Get<PdfArray>((PdfName)"Attached")!).Resolve()).Value);
    }

    [Fact]
    public void JsonParsesAnnotationAndCanonicalArtifactProperties()
    {
        const string json = """
        { "schema":"pdflexer.remediation.ruleset.v1", "rules":[
          { "id":"a", "candidates":{"kind":"annotation"},
            "predicate":{"kind":"annotationSubtype","value":"Link"},
            "action":{"kind":"adoptAnnotation","accessibleDescription":"Account"} },
          { "id":"h", "candidates":{"kind":"text","granularity":"line"},
            "action":{"kind":"artifact","type":"Pagination","semanticSubtype":"Header",
                      "includeBoundingBox":true,"attached":["Top"]} }
        ] }
        """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var rules = SerializedRemediationRules.Load(stream).RuleSet.Rules;
        Assert.IsType<AdoptAnnotationRemediationAction>(rules[0].Action);
        var artifact = Assert.IsType<ArtifactRemediationAction>(rules[1].Action);
        Assert.Equal(ArtifactSemanticSubtype.Header, artifact.SemanticSubtype);
        Assert.True(artifact.IncludeBoundingBox);
        Assert.Equal(ArtifactAttachmentEdge.Top, Assert.Single(artifact.Attached!));
    }

    [Fact]
    public void DuplicateAnnotationConsumersBlockCommitWithoutMutation()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var annotation = new PdfDictionary
        {
            [PdfName.Subtype] = (PdfName)"Stamp",
            [PdfName.Rect] = new PdfArray { 10, 10, 30, 30 },
            [PdfName.Contents] = PdfString.CreateTextString("Approved")
        };
        page.NativeObject[PdfName.Annots] = new PdfArray { annotation };
        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = true,
            LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact
        });

        var error = Assert.Throws<InvalidOperationException>(() => session.Commit(
            new Rule("first", RemediationActions.AdoptAnnotation(), candidates: CandidateSelector.Annotations()),
            new Rule("second", RemediationActions.AdoptAnnotation(), candidates: CandidateSelector.Annotations())));

        Assert.Contains("AnnotationAlreadyConsumed", error.Message, StringComparison.Ordinal);
        Assert.False(annotation.ContainsKey(PdfName.StructParent));
    }

    [Fact]
    public void MissingIntoTargetAndBlankLinkDescriptionAreCommitBlockers()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);
        var link = new PdfDictionary
        {
            [PdfName.Subtype] = PdfName.Link,
            [PdfName.Rect] = new PdfArray { 10, 10, 30, 30 },
            [PdfName.Border] = new PdfArray { 0, 0, 0 }
        };
        page.NativeObject[PdfName.Annots] = new PdfArray { link };
        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = true,
            LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact
        });

        var error = Assert.Throws<InvalidOperationException>(() => session.Commit(
            new Rule("link", RemediationActions.AdoptAnnotation(ClaimPredicates.FromRule("text")),
                candidates: CandidateSelector.Annotations()),
            new Rule("text", RemediationActions.Tag("Link"), RemediationPredicate.Never,
                CandidateSelector.Text(Granularity.Line))));

        Assert.Contains("AnnotationAdoptionTargetMissing", error.Message, StringComparison.Ordinal);
        Assert.Contains("non-empty /Contents", error.Message, StringComparison.Ordinal);
        Assert.False(link.ContainsKey(PdfName.StructParent));
    }

    [Fact]
    public void AnnotationReferencesToGroupRulesAreRejectedAtDeclarationTime()
    {
        var rules = new RuleSet("invalid-annotation-reference", new[]
        {
            new Rule("text", RemediationActions.Tag("P"), candidates: CandidateSelector.Text(Granularity.Line)),
            new Rule("group", RemediationActions.Group("Sect", ClaimPredicates.FromRule("text")), stage: Stage.Group),
            new Rule("annotation", RemediationActions.AdoptAnnotation(ClaimPredicates.FromRule("group")),
                candidates: CandidateSelector.Annotations())
        });

        using var doc = PdfDocument.Create();
        doc.AddPage(PageSize.LETTER);
        using var session = doc.BeginRemediation(new RemediationSessionConfiguration
        {
            StrictConformance = false,
            LeftoverPolicy = RemediationLeftoverPolicy.AutoArtifact
        });

        var validation = session.Validate(rules);

        Assert.Contains(validation.Errors,
            x => x.Contains("Classify claims only", StringComparison.Ordinal));
    }

    [Fact]
    public void ConflictingLegacyAndCanonicalArtifactTypesAreRejected()
    {
        const string json = """
        { "schema":"pdflexer.remediation.ruleset.v1", "rules":[
          { "id":"a", "candidates":{"kind":"text","granularity":"line"},
            "action":{"kind":"artifact","type":"Pagination","subtype":"Layout"} }
        ] }
        """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var error = Assert.Throws<InvalidDataException>(() => SerializedRemediationRules.Load(stream));
        Assert.Contains("conflicts with legacy", error.Message, StringComparison.Ordinal);
    }

}
