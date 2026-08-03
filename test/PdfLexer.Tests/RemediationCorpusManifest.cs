using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using PdfLexer.Remediation;

namespace PdfLexer.Tests;

internal static class RemediationCorpusManifest
{
    public const string CurrentSchema = "pdflexer.remediation.corpus.v1";
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        AllowTrailingCommas = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string AssetRootPath
    {
        get
        {
            var testRoot = PathUtil.GetPathFromSegmentOfCurrent("test");
            return Path.Combine(testRoot, "PdfLexer.Tests", "Fixtures", "Remediation");
        }
    }

    public static RemediationCorpusDefinition Load()
    {
        var path = Path.Combine(AssetRootPath, "corpus.json");
        using var stream = File.OpenRead(path);
        var manifest = JsonSerializer.Deserialize<RemediationCorpusDefinition>(stream, Options)
            ?? throw new InvalidDataException("The remediation corpus manifest is empty.");
        Validate(manifest, path);
        return manifest;
    }

    public static string ResolveAsset(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
            throw new InvalidDataException("Corpus asset paths must be non-empty and relative.");
        var root = Path.GetFullPath(AssetRootPath) + Path.DirectorySeparatorChar;
        var resolved = Path.GetFullPath(Path.Combine(AssetRootPath, relativePath));
        if (!resolved.StartsWith(root, StringComparison.Ordinal))
            throw new InvalidDataException($"Corpus asset path '{relativePath}' escapes the asset root.");
        return resolved;
    }

    public static RemediationProgram LoadProgram(RemediationCorpusCase item, PdfUaProfile profile)
    {
        if (item.Status != RemediationCorpusStatus.Active)
            throw new InvalidOperationException($"Pending corpus case '{item.Id}' has no executable program.");
        var relative = item.Programs.Program ?? (profile == PdfUaProfile.PdfUa1 ? item.Programs.Ua1 : item.Programs.Ua2);
        var program = SerializedRemediationProgram.Load(ResolveAsset(relative!));
        if (item.Programs.Program != null)
        {
            if (program.Template.Profile != profile)
            {
                var template = program.Template with { Profile = profile };
                program = program with { Template = template };
            }
        }
        else if (program.Template.Profile != profile)
        {
            throw new InvalidDataException(
                $"Corpus case '{item.Id}' expected {profile}, but '{relative}' declares {program.Template.Profile}.");
        }
        return program;
    }

    private static void Validate(RemediationCorpusDefinition manifest, string source)
    {
        if (manifest.Schema != CurrentSchema)
            throw new InvalidDataException($"Unsupported corpus schema '{manifest.Schema}' in '{source}'.");
        if (manifest.Cases.Count == 0)
            throw new InvalidDataException("The remediation corpus manifest contains no cases.");
        var duplicate = manifest.Cases.GroupBy(x => x.Id, StringComparer.Ordinal)
            .FirstOrDefault(x => x.Count() > 1);
        if (duplicate != null)
            throw new InvalidDataException($"Corpus case id '{duplicate.Key}' is duplicated.");
        foreach (var item in manifest.Cases)
        {
            if (string.IsNullOrWhiteSpace(item.Id) || string.IsNullOrWhiteSpace(item.Name) ||
                string.IsNullOrWhiteSpace(item.InputFactory) || string.IsNullOrWhiteSpace(item.OutputBase))
                throw new InvalidDataException("Every corpus case requires id, name, inputFactory, and outputBase.");
            if (item.Status == RemediationCorpusStatus.Active)
            {
                var hasProgram = !string.IsNullOrWhiteSpace(item.Programs.Program);
                var hasSplit = !string.IsNullOrWhiteSpace(item.Programs.Ua1) && !string.IsNullOrWhiteSpace(item.Programs.Ua2);
                if (!hasProgram && !hasSplit)
                    throw new InvalidDataException($"Active corpus case '{item.Id}' requires program or (ua1 and ua2) programs.");
                if (hasProgram && (item.Programs.Ua1 != null || item.Programs.Ua2 != null))
                    throw new InvalidDataException($"Active corpus case '{item.Id}' cannot declare both program and ua1/ua2.");
                if (item.Pending != null)
                    throw new InvalidDataException($"Active corpus case '{item.Id}' cannot declare a pending gap.");
                var declaredPaths = hasProgram
                    ? new[] { item.Programs.Program! }
                    : new[] { item.Programs.Ua1!, item.Programs.Ua2! };
                foreach (var relative in declaredPaths)
                    if (!File.Exists(ResolveAsset(relative)))
                        throw new FileNotFoundException($"Program asset for corpus case '{item.Id}' was not found.", relative);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(item.Programs.Program) || !string.IsNullOrWhiteSpace(item.Programs.Ua1) || !string.IsNullOrWhiteSpace(item.Programs.Ua2))
                    throw new InvalidDataException($"Pending corpus case '{item.Id}' must not declare executable programs.");
                if (item.Pending == null || string.IsNullOrWhiteSpace(item.Pending.GapId) ||
                    string.IsNullOrWhiteSpace(item.Pending.Reason))
                    throw new InvalidDataException($"Pending corpus case '{item.Id}' requires a gap id and reason.");
            }
        }
    }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record RemediationCorpusDefinition
{
    public string Schema { get; init; } = string.Empty;
    public IReadOnlyList<RemediationCorpusCase> Cases { get; init; } = Array.Empty<RemediationCorpusCase>();
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record RemediationCorpusCase
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string InputFactory { get; init; } = string.Empty;
    public string OutputBase { get; init; } = string.Empty;
    public RemediationCorpusStatus Status { get; init; }
    public RemediationCorpusOutcome Outcome { get; init; }
    public RemediationCorpusPrograms Programs { get; init; } = new();
    public RemediationCorpusExpectation Expected { get; init; } = new();
    public IReadOnlyList<RemediationCorpusSuppression> Suppressions { get; init; } =
        Array.Empty<RemediationCorpusSuppression>();
    public RemediationCorpusPending? Pending { get; init; }
}

internal enum RemediationCorpusStatus { Active, Pending }
internal enum RemediationCorpusOutcome { Commit, Diagnose, AuthoringDiagnose, CommitWithAcknowledgedAssertion }

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record RemediationCorpusPrograms
{
    public string? Program { get; init; }
    public string? Ua1 { get; init; }
    public string? Ua2 { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record RemediationCorpusExpectation
{
    public IReadOnlyDictionary<string, int> Slots { get; init; } = new Dictionary<string, int>();
    public IReadOnlyDictionary<string, int> Bindings { get; init; } = new Dictionary<string, int>();
    public IReadOnlyDictionary<string, int> Artifacts { get; init; } = new Dictionary<string, int>();
    public IReadOnlyList<RemediationExpectedDiagnostic> Diagnostics { get; init; } = Array.Empty<RemediationExpectedDiagnostic>();
    public bool RasterPreserved { get; init; } = true;
    public bool GlyphGeometryPreserved { get; init; } = true;
    public bool VeraPdfCompliant { get; init; } = true;
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record RemediationExpectedDiagnostic
{
    public DiagnosticCode Code { get; init; }
    public RemediationDiagnosticDisposition Disposition { get; init; }
    public string? Slot { get; init; }
    public string? Binding { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record RemediationCorpusSuppression
{
    public DiagnosticCode Code { get; init; }
    public string Scope { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record RemediationCorpusPending
{
    public string GapId { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}
