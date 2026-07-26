using System;
using System.Collections.Generic;
using System.Linq;
using PdfLexer.DOM;
using Xunit;

namespace PdfLexer.Tests;

[Collection(VeraPdfTestCollection.Name)]
public class AccessibilityVeraPdfTests
{
    private static readonly Lazy<IReadOnlyList<GeneratedAccessibilityFixture>> Ua1Fixtures =
        new(() => AccessibilityFixtureGenerator.GenerateAll(PdfUaProfile.PdfUa1));
    private static readonly Lazy<IReadOnlyList<GeneratedAccessibilityFixture>> Ua2Fixtures =
        new(() => AccessibilityFixtureGenerator.GenerateAll(PdfUaProfile.PdfUa2));

    [VeraPdfFact]
    public void StructuredReportPassesPdfUa1()
    {
        ValidateFixture(AccessibilityFixtureGenerator.AccessibleReportFixtureBaseName, PdfUaProfile.PdfUa1);
    }

    [VeraPdfFact]
    public void FillableFormPassesPdfUa1()
    {
        ValidateFixture(AccessibilityFixtureGenerator.FillableFormFixtureBaseName, PdfUaProfile.PdfUa1);
    }

    [VeraPdfFact]
    public void MultiPassPageWritingPassesPdfUa1()
    {
        ValidateFixture(AccessibilityFixtureGenerator.MultiPassWritingFixtureBaseName, PdfUaProfile.PdfUa1);
    }

    [VeraPdfFact]
    public void RetaggedNavigationPassesPdfUa1()
    {
        ValidateFixture(AccessibilityFixtureGenerator.RetaggedNavigationFixtureBaseName, PdfUaProfile.PdfUa1);
    }

    [VeraPdfFact]
    public void ReusedImagePassesPdfUa1()
    {
        ValidateFixture(AccessibilityFixtureGenerator.ReusedImageFixtureBaseName, PdfUaProfile.PdfUa1);
    }

    [VeraPdfFact]
    public void TaggedFormXObjectPassesPdfUa1()
    {
        ValidateFixture(AccessibilityFixtureGenerator.TaggedFormXObjectFixtureBaseName, PdfUaProfile.PdfUa1);
    }

    [VeraPdfFact]
    public void TaggedAnnotationPassesPdfUa1()
    {
        ValidateFixture(AccessibilityFixtureGenerator.TaggedAnnotationFixtureBaseName, PdfUaProfile.PdfUa1);
    }

    [VeraPdfFact]
    public void StructuredReportPassesPdfUa2()
    {
        ValidateFixture(AccessibilityFixtureGenerator.AccessibleReportFixtureBaseName, PdfUaProfile.PdfUa2);
    }

    [VeraPdfFact]
    public void FillableFormPassesPdfUa2()
    {
        ValidateFixture(AccessibilityFixtureGenerator.FillableFormFixtureBaseName, PdfUaProfile.PdfUa2);
    }

    [VeraPdfFact]
    public void MultiPassPageWritingPassesPdfUa2()
    {
        ValidateFixture(AccessibilityFixtureGenerator.MultiPassWritingFixtureBaseName, PdfUaProfile.PdfUa2);
    }

    [VeraPdfFact]
    public void RetaggedNavigationPassesPdfUa2()
    {
        ValidateFixture(AccessibilityFixtureGenerator.RetaggedNavigationFixtureBaseName, PdfUaProfile.PdfUa2);
    }

    [VeraPdfFact]
    public void ReusedImagePassesPdfUa2()
    {
        ValidateFixture(AccessibilityFixtureGenerator.ReusedImageFixtureBaseName, PdfUaProfile.PdfUa2);
    }

    [VeraPdfFact]
    public void TaggedFormXObjectPassesPdfUa2()
    {
        ValidateFixture(AccessibilityFixtureGenerator.TaggedFormXObjectFixtureBaseName, PdfUaProfile.PdfUa2);
    }

    [VeraPdfFact]
    public void TaggedAnnotationPassesPdfUa2()
    {
        ValidateFixture(AccessibilityFixtureGenerator.TaggedAnnotationFixtureBaseName, PdfUaProfile.PdfUa2);
    }

    private static void ValidateFixture(string baseName, PdfUaProfile profile)
    {
        var expectedFileName =
            AccessibilityFixtureGenerator.GetFixtureFileName(baseName, profile);
        var fixtures = profile == PdfUaProfile.PdfUa1 ? Ua1Fixtures.Value : Ua2Fixtures.Value;
        var fixture = fixtures.Single(x => x.FileName == expectedFileName);

        var result = VeraPdfValidation.Validate(fixture.Bytes, profile);

        Assert.False(result.ProcessingFailed, result.StandardError);
        Assert.True(result.ValidationPerformed, result.Report);
        Assert.True(result.PassedRules > 0, result.Report);
        Assert.True(result.PassedChecks > 0, result.Report);
        Assert.Equal(0, result.FailedRules);
        Assert.Equal(0, result.FailedChecks);
        Assert.True(result.IsCompliant, result.Report);
    }
}
