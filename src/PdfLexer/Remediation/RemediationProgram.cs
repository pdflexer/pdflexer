using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace PdfLexer.Remediation;

/// <summary>Complete prescriptive remediation definition selected for one document family.</summary>
public sealed record RemediationProgram
{
    public RemediationProgram(
        string id,
        RemediationTemplate template,
        IEnumerable<BindingRule> bindings,
        IEnumerable<RemediationAnchor>? anchors = null,
        IEnumerable<ArtifactDeclaration>? artifacts = null,
        IEnumerable<SlotCountAssertion>? assertions = null,
        TextNormalizationOptions? textNormalization = null,
        IEnumerable<OccurrenceBoundaryDeclaration>? boundaries = null,
        IEnumerable<RemediationFragment>? fragments = null,
        IEnumerable<RegionDeclaration>? regions = null,
        IEnumerable<RegionArtifactAccounting>? regionAccounting = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Program id is required.", nameof(id));
        Id = id;
        Template = template ?? throw new ArgumentNullException(nameof(template));
        Bindings = CopyRequired(bindings, nameof(bindings));
        Anchors = CopyOptional(anchors, nameof(anchors));
        Artifacts = CopyOptional(artifacts, nameof(artifacts));
        Assertions = CopyOptional(assertions, nameof(assertions));
        Boundaries = CopyOptional(boundaries, nameof(boundaries));
        Fragments = CopyOptional(fragments, nameof(fragments));
        Regions = CopyOptional(regions, nameof(regions));
        RegionAccounting = CopyOptional(regionAccounting, nameof(regionAccounting));
        TextNormalization = textNormalization ?? TextNormalizationOptions.Default;

        static ReadOnlyCollection<T> CopyRequired<T>(IEnumerable<T>? values, string parameter) where T : class =>
            CopyOptional(values ?? throw new ArgumentNullException(parameter), parameter);
        static ReadOnlyCollection<T> CopyOptional<T>(IEnumerable<T>? values, string parameter) where T : class
        {
            var copy = (values ?? Array.Empty<T>()).ToList();
            if (copy.Any(x => x == null)) throw new ArgumentException("Collections cannot contain null entries.", parameter);
            return new ReadOnlyCollection<T>(copy);
        }
    }

    public string Id { get; }
    public RemediationTemplate Template { get; init; }
    public IReadOnlyList<BindingRule> Bindings { get; }
    public IReadOnlyList<RemediationAnchor> Anchors { get; }
    public IReadOnlyList<ArtifactDeclaration> Artifacts { get; }
    public IReadOnlyList<SlotCountAssertion> Assertions { get; }
    public IReadOnlyList<OccurrenceBoundaryDeclaration> Boundaries { get; }
    public IReadOnlyList<RemediationFragment> Fragments { get; }
    public TextNormalizationOptions TextNormalization { get; }
    public IReadOnlyList<RegionDeclaration> Regions { get; }
    public IReadOnlyList<RegionArtifactAccounting> RegionAccounting { get; }
}

/// <summary>Closed prescriptive structure contract for a document family.</summary>
public sealed record RemediationTemplate
{
    public RemediationTemplate(string id, string version, PdfUaProfile profile, RemediationTemplateNode document)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Template id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException("Template version is required.", nameof(version));
        Id = id;
        Version = version;
        Profile = profile;
        Document = document ?? throw new ArgumentNullException(nameof(document));
    }

    public string Id { get; }
    public string Version { get; }
    public PdfUaProfile Profile { get; init; }
    public RemediationTemplateNode Document { get; }
}

/// <summary>Prescriptive template node. Non-root nodes have a local name used to form a slot path.</summary>
public abstract record RemediationTemplateParticle;

public sealed record RemediationFragmentMount : RemediationTemplateParticle
{
    public RemediationFragmentMount(
        string fragmentId,
        string alias,
        TemplateOccurrence occurrence = TemplateOccurrence.ExactlyOne,
        OccurrenceBoundary? occurrenceBoundary = null)
    {
        if (string.IsNullOrWhiteSpace(fragmentId)) throw new ArgumentException("Fragment id is required.", nameof(fragmentId));
        if (!SlotRef.IsValidSegment(alias)) throw new ArgumentException($"Invalid fragment alias '{alias}'.", nameof(alias));
        if (!Enum.IsDefined(occurrence)) throw new ArgumentOutOfRangeException(nameof(occurrence));
        FragmentId = fragmentId;
        Alias = alias;
        Occurrence = occurrence;
        OccurrenceBoundary = occurrenceBoundary;
    }

    public string FragmentId { get; }
    public string Alias { get; }
    public TemplateOccurrence Occurrence { get; }
    public OccurrenceBoundary? OccurrenceBoundary { get; }
}

public sealed record RemediationTemplateNode : RemediationTemplateParticle
{
    public RemediationTemplateNode(
        string tag,
        string? name = null,
        IEnumerable<RemediationTemplateNode>? children = null,
        TemplateOccurrence occurrence = TemplateOccurrence.ExactlyOne,
        RemediationNodeProperties? properties = null,
        TemplateOrderPolicy orderPolicy = TemplateOrderPolicy.RequireSourceAgreement,
        OccurrenceBoundary? occurrenceBoundary = null,
        IEnumerable<RemediationTemplateParticle>? particles = null)
    {
        if (string.IsNullOrWhiteSpace(tag)) throw new ArgumentException("Template node tag is required.", nameof(tag));
        if (name != null && !SlotRef.IsValidSegment(name)) throw new ArgumentException($"Invalid template node name '{name}'.", nameof(name));
        if (!Enum.IsDefined(occurrence)) throw new ArgumentOutOfRangeException(nameof(occurrence));
        if (!Enum.IsDefined(orderPolicy)) throw new ArgumentOutOfRangeException(nameof(orderPolicy));
        Tag = tag;
        Name = name;
        if (children != null && particles != null)
            throw new ArgumentException("Specify either direct children or ordered particles, not both.");
        Particles = new ReadOnlyCollection<RemediationTemplateParticle>(
            (particles ?? children?.Cast<RemediationTemplateParticle>() ??
                Array.Empty<RemediationTemplateParticle>()).ToList());
        if (Particles.Any(x => x == null))
            throw new ArgumentException("Template particles cannot contain null entries.", nameof(particles));
        Children = new ReadOnlyCollection<RemediationTemplateNode>(Particles.OfType<RemediationTemplateNode>().ToList());
        Occurrence = occurrence;
        Properties = properties ?? new RemediationNodeProperties();
        OrderPolicy = orderPolicy;
        OccurrenceBoundary = occurrenceBoundary;
    }

    public string Tag { get; }
    public string? Name { get; }
    public IReadOnlyList<RemediationTemplateNode> Children { get; }
    public IReadOnlyList<RemediationTemplateParticle> Particles { get; }
    public TemplateOccurrence Occurrence { get; }
    public RemediationNodeProperties Properties { get; }
    public TemplateOrderPolicy OrderPolicy { get; }
    public OccurrenceBoundary? OccurrenceBoundary { get; }
    public bool IsLeaf => Children.Count == 0;
}

/// <summary>A reusable, mountable prescriptive template and its local declarations.</summary>
public sealed record RemediationFragment
{
    public RemediationFragment(
        string id,
        RemediationTemplateNode root,
        IEnumerable<BindingRule>? bindings = null,
        IEnumerable<RemediationAnchor>? anchors = null,
        IEnumerable<ArtifactDeclaration>? artifacts = null,
        IEnumerable<SlotCountAssertion>? assertions = null,
        IEnumerable<OccurrenceBoundaryDeclaration>? boundaries = null,
        IEnumerable<RegionDeclaration>? regions = null,
        IEnumerable<RegionArtifactAccounting>? regionAccounting = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Fragment id is required.", nameof(id));
        Id = id;
        Root = root ?? throw new ArgumentNullException(nameof(root));
        Bindings = Copy(bindings);
        Anchors = Copy(anchors);
        Artifacts = Copy(artifacts);
        Assertions = Copy(assertions);
        Boundaries = Copy(boundaries);

        Regions = Copy(regions);
        RegionAccounting = Copy(regionAccounting);

        static IReadOnlyList<T> Copy<T>(IEnumerable<T>? values) where T : class =>
            Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
    }

    public string Id { get; }
    public RemediationTemplateNode Root { get; }
    public IReadOnlyList<BindingRule> Bindings { get; }
    public IReadOnlyList<RemediationAnchor> Anchors { get; }
    public IReadOnlyList<ArtifactDeclaration> Artifacts { get; }
    public IReadOnlyList<SlotCountAssertion> Assertions { get; }
    public IReadOnlyList<OccurrenceBoundaryDeclaration> Boundaries { get; }
    public IReadOnlyList<RegionDeclaration> Regions { get; }
    public IReadOnlyList<RegionArtifactAccounting> RegionAccounting { get; }
}
/// <summary>Content-independent properties written to a materialized structure element.</summary>
public sealed record RemediationNodeProperties(
    string? Language = null,
    string? AlternateText = null,
    string? ActualText = null,
    string? Expansion = null);

public enum TemplateOrderPolicy
{
    RequireSourceAgreement,
    AllowDeclaredReorder
}

/// <summary>Occurrence constraint owned by the prescriptive program model.</summary>
public enum TemplateOccurrence
{
    ExactlyOne,
    Optional,
    ZeroOrMore,
    OneOrMore
}

/// <summary>How a repeating composite starts a new materialized occurrence.</summary>
public abstract record OccurrenceBoundary
{
    private OccurrenceBoundary() { }

    /// <summary>Derive the boundary from the first required, singular child.</summary>
    public sealed record Derived : OccurrenceBoundary;

    /// <summary>Use a descendant slot as the logical opener.</summary>
    public sealed record StartsOnSlot : OccurrenceBoundary
    {
        public StartsOnSlot(SlotRef reference) =>
            Reference = reference ?? throw new ArgumentNullException(nameof(reference));
        public SlotRef Reference { get; }
    }

    /// <summary>Use a named producer-specific boundary declaration.</summary>
    public sealed record StartsOnBoundary : OccurrenceBoundary
    {
        public StartsOnBoundary(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Boundary id is required.", nameof(id));
            Id = id;
        }
        public string Id { get; }
    }
}

/// <summary>Producer-specific evidence used by an explicit repeating boundary.</summary>
public sealed record OccurrenceBoundaryDeclaration
{
    public OccurrenceBoundaryDeclaration(
        string id,
        CandidateSelector candidates,
        RemediationPredicate? predicate = null,
        PageSelector? pages = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Boundary id is required.", nameof(id));
        Id = id;
        Candidates = candidates ?? throw new ArgumentNullException(nameof(candidates));
        Predicate = predicate ?? RemediationPredicate.Always;
        Pages = pages ?? PageSelector.Every;
    }

    public string Id { get; }
    public CandidateSelector Candidates { get; }
    public RemediationPredicate Predicate { get; }
    public PageSelector Pages { get; }
}

/// <summary>How a reference selects occurrences of a repeating slot.</summary>
public enum OccurrenceSelector
{
    Only,
    SameOccurrence,
    Nth,
    NearestPrevious,
    All
}

/// <summary>Typed slot reference with an explicit occurrence-selection policy.</summary>
public sealed record SlotOccurrenceRef
{
    public SlotOccurrenceRef(SlotRef slot, OccurrenceSelector selector = OccurrenceSelector.Only, int? occurrenceNumber = null)
    {
        Slot = slot ?? throw new ArgumentNullException(nameof(slot));
        if (!Enum.IsDefined(selector)) throw new ArgumentOutOfRangeException(nameof(selector));
        if (selector == OccurrenceSelector.Nth && occurrenceNumber is not > 0)
            throw new ArgumentOutOfRangeException(nameof(occurrenceNumber), "Occurrence numbers are one-based.");
        if (selector != OccurrenceSelector.Nth && occurrenceNumber != null)
            throw new ArgumentException("Only Nth may specify an occurrence number.", nameof(occurrenceNumber));
        Selector = selector;
        OccurrenceNumber = occurrenceNumber;
    }

    public SlotRef Slot { get; }
    public OccurrenceSelector Selector { get; }
    public int? OccurrenceNumber { get; }
    public static SlotOccurrenceRef Only(SlotRef slot) => new(slot);
    public static SlotOccurrenceRef SameOccurrence(SlotRef slot) => new(slot, OccurrenceSelector.SameOccurrence);
    public static SlotOccurrenceRef Nth(SlotRef slot, int occurrenceNumber) => new(slot, OccurrenceSelector.Nth, occurrenceNumber);
    public static SlotOccurrenceRef NearestPrevious(SlotRef slot) => new(slot, OccurrenceSelector.NearestPrevious);
    public static SlotOccurrenceRef All(SlotRef slot) => new(slot, OccurrenceSelector.All);
}

/// <summary>Canonical root-relative reference to a template slot.</summary>
public sealed record SlotRef
{
    private static readonly Regex Segment = new("^[A-Za-z][A-Za-z0-9_-]*$", RegexOptions.CultureInvariant);

    public SlotRef(string path)
    {
        Path = Normalize(path);
    }

    public string Path { get; }
    public bool IsRoot => Path == "/";
    public bool IsAbsolute => Path.StartsWith("/", StringComparison.Ordinal);
    public bool IsRelative => Path.StartsWith("./", StringComparison.Ordinal);

    public static SlotRef Absolute(string path)
    {
        var slot = new SlotRef(path);
        if (!slot.IsAbsolute) throw new ArgumentException("An absolute slot path must begin with '/'.", nameof(path));
        return slot;
    }
    public static SlotRef Relative(string path)
    {
        var slot = new SlotRef(path);
        if (!slot.IsRelative) throw new ArgumentException("A relative slot path must begin with './'.", nameof(path));
        return slot;
    }
    public static SlotRef Parse(string path) => new(path);
    public static bool TryParse(string? path, out SlotRef slot)
    {
        try
        {
            slot = new SlotRef(path ?? string.Empty);
            return true;
        }
        catch (ArgumentException)
        {
            slot = null!;
            return false;
        }
    }
    public override string ToString() => Path ?? string.Empty;

    internal static bool IsValidSegment(string value) => !string.IsNullOrWhiteSpace(value) && Segment.IsMatch(value);

    private static string Normalize(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !(path.StartsWith('/') || path.StartsWith("./", StringComparison.Ordinal)))
            throw new ArgumentException("Slot paths must be absolute ('/title') or reserved relative syntax ('./title').", nameof(path));
        if (path == "/") return path;
        if (path.EndsWith('/') || path.Contains("//", StringComparison.Ordinal))
            throw new ArgumentException($"Slot path '{path}' is not canonical.", nameof(path));
        var prefixLength = path.StartsWith("./", StringComparison.Ordinal) ? 2 : 1;
        var parts = path[prefixLength..].Split('/');
        if (parts.Any(x => !IsValidSegment(x)))
            throw new ArgumentException($"Slot path '{path}' contains an invalid segment.", nameof(path));
        return path.StartsWith("./", StringComparison.Ordinal) ? "./" + string.Join('/', parts) : "/" + string.Join('/', parts);
    }
}

public abstract record BindingTarget
{
    private BindingTarget() { }
    public sealed record Slot : BindingTarget
    {
        public Slot(SlotRef reference) => Reference = reference ?? throw new ArgumentNullException(nameof(reference));
        public SlotRef Reference { get; }
    }
    public sealed record Artifact(string Id) : BindingTarget
    {
        public string Id { get; } = string.IsNullOrWhiteSpace(Id)
            ? throw new ArgumentException("Artifact id is required.", nameof(Id))
            : Id;
    }

    public static BindingTarget ToSlot(SlotRef slot) => new Slot(slot);
    public static BindingTarget ToArtifact(string id) => new Artifact(id);
}

/// <summary>One candidate-to-slot or candidate-to-artifact mapping.</summary>
public sealed record BindingRule
{
    public BindingRule(
        string id,
        BindingTarget target,
        CandidateSelector candidates,
        RemediationPredicate? predicate = null,
        PageSelector? pages = null,
        double? minConfidence = null,
        BindingCardinality? cardinality = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Binding id is required.", nameof(id));
        if (minConfidence is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(minConfidence));
        Id = id;
        Target = target ?? throw new ArgumentNullException(nameof(target));
        Candidates = candidates ?? throw new ArgumentNullException(nameof(candidates));
        Predicate = predicate ?? RemediationPredicate.Always;
        Pages = pages ?? PageSelector.Every;
        MinConfidence = minConfidence;
        Cardinality = cardinality;
    }

    public string Id { get; }
    public BindingTarget Target { get; }
    public CandidateSelector Candidates { get; }
    public RemediationPredicate Predicate { get; }
    public PageSelector Pages { get; }
    public double? MinConfidence { get; }
    public BindingCardinality? Cardinality { get; }
}

/// <summary>References a singular template slot as a positional anchor.</summary>
public sealed record SlotAnchor : RemediationAnchor
{
    public SlotAnchor(string id, SlotRef slot)
        : this(id, SlotOccurrenceRef.Only(slot))
    {
    }

    public SlotAnchor(string id, SlotOccurrenceRef reference)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Anchor id is required.", nameof(id));
        Id = id;
        Reference = reference ?? throw new ArgumentNullException(nameof(reference));
    }
    public override string Id { get; init; }
    public SlotOccurrenceRef Reference { get; }
    public SlotRef Slot => Reference.Slot;
    public OccurrenceSelector OccurrenceSelector => Reference.Selector;
    public int? OccurrenceNumber => Reference.OccurrenceNumber;
    public override string DebugString => $"SlotAnchor({Id}, {Reference.Slot}, {Reference.Selector})";
}

public sealed record SlotCountAssertion
{
    public SlotCountAssertion(string id, SlotRef slot, AssertionCount expected,
        SemanticAssertionScope scope = SemanticAssertionScope.Document, PageSelector? pages = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Assertion id is required.", nameof(id));
        if (!Enum.IsDefined(scope)) throw new ArgumentOutOfRangeException(nameof(scope));
        Id = id;
        Slot = slot ?? throw new ArgumentNullException(nameof(slot));
        Expected = expected ?? throw new ArgumentNullException(nameof(expected));
        Scope = scope;
        Pages = pages ?? PageSelector.Every;
    }

    public string Id { get; }
    public SlotRef Slot { get; }
    public AssertionCount Expected { get; }
    public SemanticAssertionScope Scope { get; }
    public PageSelector Pages { get; }
}

/// <summary>Execution policy for authoring dry-runs versus enforced commits.</summary>
public enum RemediationRunMode
{
    Authoring,
    Enforced
}
