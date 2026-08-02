using PdfLexer.Content;
using PdfLexer.DOM;

namespace PdfLexer.Remediation;

public sealed record RemediationUnaccountedContent(
    int PageIndex,
    RemediationCandidateKind CandidateKind,
    string CandidateId,
    StructuredSourceRef SourceReference,
    PdfRect<double> BoundingBox,
    PdfRect<double> RelativeBoundingBox,
    string? RawText = null,
    string? NormalizedText = null,
    string? ResourceIdentity = null,
    string? ResourceName = null,
    int ResourceUseCount = 0);
