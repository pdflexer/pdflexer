# Rule-Based Remediation — Architecture and Direction

Last updated: 2026-07-31

What the model *is*, where it is going, and what is missing. The reference doc says how to use the
language as shipped; this says why it is shaped that way and which parts are not settled yet.

| Document | Role |
| --- | --- |
| [rule-based-remediation.md](rule-based-remediation.md) | The rule language and API as shipped |
| **This document** | The model, its invariants, and open architectural gaps |
| [rule-based-remediation-gaps.md](rule-based-remediation-gaps.md) | Gap register — one section per RRM ID |
| [rule-based-remediation-structural-model.md](rule-based-remediation-structural-model.md) | Design spec for the template and artifact inventory (RRM-039 – RRM-041) |
| [rule-based-remediation-plan.md](rule-based-remediation-plan.md) | Milestones, gates, sequencing |
| [rule-based-remediation-corpus.md](rule-based-remediation-corpus.md) | The input corpus gaps are validated against |

---

## The model

Remediating a known document type is three separate problems, and the language keeps them separate.

**1. Declare the structure.** A closed, ordered `RemediationStructuralTemplate` states the tags,
hierarchy, occurrence, and sibling order the family is supposed to have. Every non-`Document` node
carries a slot id. This is authored from the document by a human — *"a heading, then a paragraph,
then a line-item table"* — and it is the easy half.

**2. Name the places.** Anchors, toleranced zones, flow regions, and named layout zones give stable
names to regions and reference points on the page. This layer exists so that positional logic can be
written once, named, and reused rather than restated inline in every rule.

**3. Map content into the structure.** Rules select candidates with predicates — text, font, colour,
geometry, anchor-relative position — and bind what they find to a slot, or to a declared artifact
inventory item. Group passes compose repeated and derived occurrences bottom-up. Refine adds
attributes. This is the hard half, and it is where iteration happens.

The split matters because the three change at different rates. The structure of an invoice is stable
for years. The producer's layout changes with every template revision upstream. Keeping the *target*
in one artifact and the *matching* in another means a layout change touches rules and leaves the
contract alone.

---

## Decision: prescriptive is the model (2026-07-30)

The template owns the structure tree. Rules produce **bindings**, never structure.

- Prescriptive mode is the supported way to author a document family. Descriptive mode is retained
  only as a migration aid and is scheduled for removal once the corpus is migrated; new rule sets
  should not use it.
- `Tag`, `Group`, and `MergeTo` cannot produce structure in prescriptive mode, and structural
  `Link(...)` creation is rejected. Content reaches the tree through `Bind`, `BindOver`, or a
  slot-bound specialized action (`TableOver`, `AdoptAnnotation`).
- Content matching no slot and no declared artifact is a non-suppressible commit blocker. There is
  no silent absorption path.

### The invariant

> In prescriptive mode, no node exists in committed output that the template did not declare.

This is one testable statement, and it is the thing that keeps the two hierarchy mechanisms — the
declared template and the derived Group passes — from drifting apart. Group passes may only produce
bindings for declared slots; they may not invent a parent. Every future structural action must be
checked against this invariant rather than against a prose rule about which mechanism owns what.

The one deliberate exception is a **bound `Table` slot's interior**. Its `TR`/`TH`/`TD` grid is
derived from the document, not declared, so it is opaque below the slot root and validated by table
hierarchy checks. Any future action producing document-derived interior structure should follow the
same pattern: declare the root, validate the interior structurally, do not pretend derived nodes are
declared slot occurrences.

### The invariant puts the Group stage in question

Group passes were designed to answer *how do rules build nested structure bottom-up when nothing
declares it* — RRM-004's problem. The invariant above abolishes the premise: if all hierarchy is
declared, there is no undeclared hierarchy left for Group to construct. Group's original
justification does not survive prescriptive-only.

What remains in the stage is narrow. `Tag`, `Group`, and `MergeTo` are already rejected in
prescriptive mode, so it contains exactly two things:

- `BindOver` — partition claims into occurrences of a repeating composite slot;
- `TableOver` / `TableOverFlattenedCells` — derive a grid interior.

Neither is a rule in the sense the rest of the language means. A rule *selects content by predicate,
then acts*; both of these operate on content already selected and bound, and make a grouping decision
over it. That mismatch is observable rather than theoretical: `BindOver`'s "predicate" is a
run-continuation test rather than a membership filter, which is why `FromSlot("line")` yields one
occurrence containing every line instead of one occurrence per line — verified against C-30's rule
set with both lines on a single page, where it also reports `TemplateWrongOrder` naming the child
slot rather than the missing partition.

**The likely end state is that partitioning becomes a property of a composite slot**, not a stage.
That is specified in
[Occurrence boundaries are declared on the slot](#1-occurrence-boundaries-are-declared-on-the-slot).
The `Table` case follows the same shape: grid-derived for a `Table` slot, with columns and header
policy declared on the slot, which is where they belong once the interior is opaque.

`groupPass` goes with it. Sparse pass numbers are the author manually encoding nesting depth that the
template already states; assembly can order composite slots by template depth without being told.
`TemplateSlotContentMode.FlattenLeafClaims` is likewise already a slot property that happens to be
reachable only through a Group-stage action.

**Removing the stage is not scheduled, deliberately.** The argument above is constructed from the
code, not demonstrated against documents, and the entire prescriptive corpus is one fixture.
Multi-column layouts and irregular tables are where a counterexample would appear. Restructuring on
inference ahead of evidence is the mistake this project has already made once.

Note that declaring the boundary on the slot is *not* the same decision and does not wait on it. It
is additive and correct under either outcome: if the stage survives, `BindOver` consumes declared
boundaries instead of inventing runs; if it does not, the declaration is already where it needs to
be. What follows applies to removing the stage:

1. Stop investing in Group as a concept — no new Group-stage actions, no richer claim-predicate
   vocabulary for run-breaking. Reaching for either is the signal that a partition belongs on the
   slot.
2. Instrument the decision. When the first real families are authored, count the `BindOver` rules and
   ask of each whether the declared child shape could have implied the partition. A consistent *yes*
   collapses the stage.
3. Treat the Group stage as transitional in the reference doc so nothing is built assuming
   permanence.

Carrying it meanwhile costs a second hierarchy mechanism to keep aligned with the first — D1 through
D4 all lived on that boundary. Removing it prematurely costs discovering, several families in, that
some partition genuinely needed a rule.

---

## The authoring process

Template first. Rules iterated against it. Template adjusted for outliers as they surface.

1. **Write the template** from a handful of representative documents. It is a description of intent
   and takes minutes, not days.
2. **Name the places** — zones for regions, anchors for labels and repeated landmarks, flow regions
   for content that continues across pages.
3. **Bind slots**, easiest first. Titles, footers, and fixed labels usually fall out immediately.
4. **Iterate on the hard slots** — relative positioning, ambiguous anchors, variable presence —
   running against samples and reading per-slot diagnostics.
5. **Adjust the template** when samples disagree with it. A slot absent from a third of the corpus is
   `Optional`, not a rule bug.
6. **Freeze.** Every declared slot is bound across every sample; unfilled becomes an error rather
   than a worklist item.

Two properties follow from this order and are worth stating because neither is available if rules
are written first.

**The template is independent of the rules by construction.** Because it is authored before any rule
exists, it cannot encode what the rules happen to do. It stays a statement of intent rather than a
snapshot of behavior. This is why the template must never be regenerated from output in a loop — see
*Migration* below for the one narrow exception.

**The template is a denominator.** Rules-first development has no way to know what has not been done
yet. Template-first does: `TemplateSlotUnfilled` across the declared slots is a burn-down list during
authoring and a commit blocker at freeze. Same diagnostic, two lifecycle phases.

---

## Direction: identity and reference

Four changes to settle at the concept level. Each is cheap now and breaking later, because each
concerns *identity* — the part of a language that cannot be refactored once rule sets exist.

### 1. The slot namespace should be path-scoped, not flat

Occurrence identity is already hierarchical: a materialized node stores
`template:Document/items[2]/body[1]` in `/ID`. The *declaration* namespace is not — slot ids are
globally unique strings enforced by a duplicate check in
`RemediationStructuralTemplateValidator`.

That gap blocks two things the model will need:

- **Reusable structural fragments.** An address block, a signature block, a header/footer group
  shared across families cannot be declared once and referenced twice, because both instantiations
  would need the same slot ids.
- **Deep nested repetition.** A repeating section containing a repeating subsection is expressible
  today, but the slot names have to be manually disambiguated by the author.

Direction: a slot is identified by its declared path; a `ref`/`define` particle for fragments becomes
additive once it is. The matcher already computes paths (`ExpectedPath`), so most of the machinery
exists.

### 2. Slots, not rule ids, are the durable reference

In template-first authoring the template is stable and rules churn — a positional rule may be
rewritten five times while iterating. But every durable reference in the language currently keys on
rule id:

| Construct | Keys on | Should key on |
| --- | --- | --- |
| `ClaimPredicates.FromRule` | rule id | slot (`FromSlot` exists — finish the migration) |
| `BeforeClaim` / `AfterClaim` | rule id | slot |
| `PriorClaimAnchor` | rule id | slot |
| `RuleOutputCountAssertion` | rule id | slot |

Renaming or splitting a rule during iteration breaks anchors, predicates, and assertions that had
nothing to do with the change. Rule ids should remain for provenance, diagnostics, and reports; they
should not be how one part of a rule set points at another.

The current authoring guidance — *"keep rule ids stable because reports and downstream tests depend
on them"* — is a symptom of this and should stop being necessary.

### 3. Anchors should be definable relative to slots

The hard rules are positional and relative. What an author wants to say is structural: *"the block
below whatever filled `bill-to-header`"*, *"the value right of `invoice-number-label`"*.
`PriorClaimAnchor` already proves the mechanism; it points at a rule id rather than a slot.

Pointing it at slots closes the loop between layers 1 and 2: the template's names become the names
the anchor layer uses, and a rule refactor cannot break an anchor.

### 4. The template declares shape and order; assertions declare everything else

`RemediationStructuralTemplateNode` already carries `Pages` and `SpansPages` — page-axis conditions
living on a structural declaration. The pressure to add more is constant: column counts, text
length, confidence floors, continuation-page conditions, "this section only appears when there is a
discount line."

#### The test for where something belongs

Three different things get conflated when reaching for "predicates on the template":

| Kind | Example | Belongs in | Why |
| --- | --- | --- | --- |
| **Mapping** | "the `H1` in the header zone is the title" | Rules | Churns every iteration |
| **Shape** | "exactly one of `paid-stamp` \| `due-notice`" | Template | Assembly must know it to place nodes |
| **Output** | "`/Lang` on this slot is `fr-CA`" | Template | Content-independent: it is part of the node being declared, not a condition on it |
| **Constraint** | "if `discount-line` is bound, `discount-total` is required" | Assertions | The template already permits both; this constrains the combination |

> **Does it change what nodes exist or where they are placed? It is grammar. Is it a property of a
> declared node that does not depend on content? It is part of the declaration. Does it constrain a
> shape that is already determined? It is an assertion. Everything else is mapping and stays in
> rules.**

Alternation passes the first test — `AssembleChildren` must know which branch to place. A fixed
`/Lang` or a `TH`'s `/Scope` passes the second: it is knowable from the declaration alone, so
requiring a rule to re-select already-bound content in order to set it is pure ceremony — see
[content-independent attributes](#3-content-independent-attributes-are-declared-not-refined).
Conditional requirement fails both: the slots are declared and placeable, and the constraint is on
the result.

#### Why mapping predicates do not go on the tree

Two reasons. The structural one is the lifetime argument that makes the whole authoring loop work:
the template is independent of the rules by construction, which is what lets unfilled slots be a
burn-down list and stops the contract becoming a snapshot of rule behavior. Put predicates on the
node and every iteration on the hard mapping problem edits the contract.

The practical one is that mapping is not a predicate. It is anchors, zones, flow regions, confidence
floors, page selectors, normalization, Group passes, and cardinality. An inline predicate handles the
trivial cases and forces a second syntax the moment a slot needs anchor-relative selection — and it
does not fit the shapes that actually occur, such as several rules feeding one repeating slot, or a
rule binding by position relative to *another slot*.

#### What assertions need before this holds

`RemediationSemanticAssertion` is the right home but currently keys on the wrong axis. Assertions
key on rule id or tag, and scope only to `Document` or `PerPage`, so there is no way to say "this
constraint applies to slot X." That is *why* `Pages`/`SpansPages` leaked onto the template node —
there was nowhere else for a per-slot condition to go. Three additions close it:

- **slot-keyed** — count, presence, or content of whatever bound to slot X, instead of keying on a
  churning rule id or an ambiguous tag when five slots are `P`;
- **slot-scoped** — `Scope = Slot("items")`, so "every row has four cells" applies within a subtree;
- **conditional** — "if slot X is bound then slot Y is required," the cross-slot case.

The third is also the pressure valve for the grammar limitation: a loose template plus conditional
assertions expresses a variable family better than encoding every variant into one closed tree. Once
these exist, `Pages`/`SpansPages` migrate to slot-scoped assertions rather than being joined by more
fields.

---

## Direction: partitioning and accounting move onto declarations

Once the template exists, an author is doing exactly three things:

1. **Labeling** — which content is this slot;
2. **Partitioning** — where one occurrence of a repeating slot ends and the next begins;
3. **Accounting** — what everything else is.

Labeling is irreducibly hard and belongs in rules. Partitioning and accounting are currently done
through rules as well, and neither should be — and they are exactly the two that a *dynamic* document
family makes hard, because dynamic means repetition and unbounded incidental content.

### Shape versus count

The template declares **shape**: which tags exist, how they nest, what order they emit in, and which
nodes repeat. That is authored and fixed.

It cannot declare **count**. No template knows an invoice has seven line items; that is a fact about
the document. Something must therefore read the document to decide how many occurrences of a
repeating slot exist and which bound claims land in which one. That decision is unavoidable. The only
open questions are **what evidence it reads** and **who declares that evidence**.

Today the evidence is a `BindOver` claim run: consecutive claims matching a `ClaimPredicate`, broken
only at a page boundary with no shared flow instance
(`RemediationSession.EvaluateDocumentClaimRunRule`). That is the wrong place for it on three counts —
it is procedural rather than declared, it lives in a rule rather than on the slot it partitions, and
its break condition is a property of the engine rather than of the document family.

### 1. Occurrence boundaries are declared on the slot

The vocabulary already exists and is attached to the wrong concept. `FlowBoundary`
(`FlowRegion.cs:65-74`) is precisely "what starts a new instance":

```
FlowBoundary.Anchor("line-start") | .Zone("items") | .Matching(predicate) | .PageBoundary
```

It is reachable only through `FlowRegion`, and clipped there: `ProbeBoundary` returns one boundary
per page and `byPage[pageIndex][regionId]` holds one resolution per region per page
(`DocumentFlowRegionResolver.cs:90`, `:138`), so a region cannot activate twice on a page even though
the boundary concept supports it.

The direction is to make the boundary a property of the repeating slot, defaulted from the declared
child shape:

> A repeating composite slot's **opening child** is its first declared child, if that child is
> required and non-repeating. Claims bound to the composite's descendant slots are walked in document
> order; a claim bound to the opening child slot closes the current occurrence and opens the next.

`Sect#item > [H2#title, P#body*]` — each `title` opens an item. `Sect#item > P#line` — each `line`
opens an item. Neither needs a partition declaration, because the template already contains the
information: the composite repeats, and each occurrence has exactly one opener. Deriving the boundary
reads what is declared; it does not infer anything from the document.

**This removes rules rather than adding syntax.** `RemediationStructuralTemplateTests.cs:504`
declares `Sect#item` (`OneOrMore`) over `P#line` (`ExactlyOne`) and needs a second rule whose only
job is the partition:

```csharp
new Rule("line", Bind(), candidates: Text(Paragraph), slot: "line"),
new Rule("item", BindOver(ClaimPredicates.FromSlot("line")),
         stage: Stage.Group, groupPass: 10, slot: "item")   // ← partition declaration
```

That rule is also wrong: it passes with the two lines on separate pages and produces **one** `item`
containing both lines when they share a page, with a `TemplateWrongOrder` naming the child slot
rather than the missing partition. Under the derivation the second rule does not exist and the
single-page case is correct.

#### When it is not derivable

The derivation is **not a fallback guess**. If the opening child is not a required singleton, the
template is rejected at declaration time — "slot `item` repeats but its occurrence boundary cannot be
derived; declare `startsOn`" — before any document is processed.

| Declared shape | Why not derivable | Author declares |
| --- | --- | --- |
| `Sect#item > P#line*` | Repeating first child — no opener | `startsOn: FlowBoundary.Matching(...)` |
| `Sect#item > [H2#title?, P#body]` | Optional first child — a missing opener is ambiguous | `startsOn` naming the slot explicitly, or a predicate |
| Opener bound by a rule yielding several claims per occurrence (a two-line heading) | Every claim would open an occurrence | `startsOn` with coalescing, or bind the heading as one claim |
| Row boundary is geometric, not semantic | No slot corresponds to the boundary | `startsOn: FlowBoundary.Matching(...)`, or a region |

The first row is one word away from the working case (`ExactlyOne` → `ZeroOrMore`), and that is the
intended behavior: the template says less, so the author says more. The failure mode is a declaration
error naming the slot, not a silent collapse discovered in output.

#### Nesting

A repeating composite inside a repeating composite partitions bottom-up by template depth, and an
outer boundary dominates — opening an outer occurrence closes any open inner ones. That depth is in
the template, which is what `groupPass: 10` currently makes the author restate by hand.

#### What it buys beyond deleting rules

Occurrence identity keys on a boundary activation rather than a run position, so
`template:Document/item[2]/line[1]` is stable when an unrelated rule changes what else is on the page.
That is the same instance concept as `FlowRegionInstanceId`, which is why lifting the
one-activation-per-page cap is the first piece of work here: it is the shared substrate whether the
boundary ends up on the slot or stays in `BindOver`.

### 2. One region concept, not four

`NamedLayoutZone`, `TolerancedZone`, `FlowRegion`, and anchors are four declarations of one idea — a
named place — differing only in whether it is fuzzy and whether it continues. The author picks one up
front and then cannot have the properties of another: there is no fuzzy header band that continues
across pages, because tolerance lives on one type and continuation on a different one.

Direction: one `Region` declaration carrying `Tolerance`, `Start`/`End` boundaries, and
`Continuation` as optional properties, with the named layout zones as presets. Anchors stay distinct
— they are content-derived *points*, not places — but should *yield* regions (`RightOf(anchor,
width)`), which is what makes anchor-relative rules readable and closes the loop with
[slot-relative anchors](#3-anchors-should-be-definable-relative-to-slots).

### 3. Content-independent attributes are declared, not refined

Most attributes in a known family are constant per slot: `/Lang`, `/Scope` on header cells,
`/ListNumbering`, `/Placement`, alt text for a fixed logo. Each currently costs a Refine rule whose
predicate re-finds content *already bound to the slot that needs the attribute* — a rule per
attribute doing nothing but re-selection.

Content-independent attributes belong on the template node. Refine then exists only for attributes
derived from content: alt text from a caption, `/ColSpan` from geometry. In a typical family this
removes a large fraction of the rule list.

This extends the invariant rather than eroding the shape/assertion split: the template declares the
nodes **and their content-independent properties**. See the fourth row of
[the placement test](#the-test-for-where-something-belongs). `Pages`/`SpansPages` are still
misplaced, because they are conditions on placement rather than properties of the node.

### 4. Accounting is declared by region

This is what makes prescriptive strictness survivable on a dynamic family. Leftover painting content
is a non-suppressible commit blocker (`RemediationSession.cs:2280`), and the only way to clear it is
an `Artifact` rule per kind of incidental content — page numbers, rules, shading, watermarks,
continuation notices. Every new sample yields new blockers, so the accounting backlog never closes.

`RemediationArtifactInventoryItem.ZoneId` does not address this: it constrains *where a declared
artifact may appear*, not what a region's unbound content is. The inventory needs region-scoped
absorption — *"unbound painting content in the footer region is `Pagination`"* — so accounting is a
declaration to review rather than an open-ended rule backlog, and still a blocker everywhere outside
a declared catch region.

One related hole belongs with it: the prescriptive branch of `ApplyLeftoverPolicy` reports a single
per-page diagnostic and returns before populating `unaccountedContent`, so the strictest mode gives
the *least* information about what actually failed.

### What this removes

| Removed | Because |
| --- | --- |
| `groupPass` | Nesting depth is in the template |
| Stage selection by the author | Slot-keyed binding plus slot-declared partitioning means nobody chooses `Classify` vs `Group` |
| `BindOver` in the common case | The declared child shape implies the partition |
| One Refine rule per static attribute | The attribute is declared on the slot |
| One Artifact rule per kind of page furniture | The region declares what its leftovers are |

### The guardrail

Everything above moves authority onto the template, and declared order is currently unverified
against document order at three layers: assembly reorders into declaration order, the matcher
validates the reordered tree, and `CheckReadingOrder` returns early under a prescriptive template
(`RemediationSession.cs:5063`). Reading order is most of what PDF/UA is for. A per-slot opt-in for
intentional reordering, with a diagnostic otherwise, should land before more authority moves onto the
template.

---

## Architecture review additions (2026-07-31)

These notes constrain the direction above; they do not reopen the decision that prescriptive
templates own structure. They make explicit several contracts that must be settled before the new
surface is treated as stable or the existing corpus is broadly migrated.

### Removing author-selected stages does not make evaluation phaseless

Slot-relative anchors and predicates introduce data dependencies. If the rule binding
`invoice-number-value` locates content relative to `invoice-number-label`, every producer of the
label slot must finish before the dependent rule runs. The dependency is more important than the
order in which rules happen to be declared.

The rule set should therefore compile to an internal execution graph even if `Stage` disappears from
the authoring language:

1. Resolve template, fragment, slot, region, and artifact references.
2. Build edges for every slot-relative anchor, predicate, partition, and content-derived refinement.
3. Reject unresolved references, cycles, and references whose occurrence selection is ambiguous.
4. Evaluate candidate-to-slot bindings in deterministic topological order. A dependent operation
   waits for all producers of the referenced slot, not merely the first producer listed.
5. Partition repeating composites bottom-up, assemble the declared tree, apply static declaration
   properties and content-derived refinements, then evaluate assertions and accounting.

These are engine phases, not choices exposed to the author. Rules with no dependency may execute in
parallel, but their committed result and report order must remain deterministic. A future
candidate predicate that depends on a slot is valid only through this graph; it must not introduce a
hidden fixed-point evaluation loop.

### Slot identity has three distinct levels

Path scoping needs a reference model, not only a change from one string format to another. The model
must distinguish:

- a **slot definition** inside a template or reusable fragment;
- a **mounted slot path** inside a particular family template, such as `billing/address/line`;
- a **materialized occurrence identity**, such as
  `template:Document/billing[1]/address[2]/line[1]`.

Public APIs and serialized rules should use a typed `SlotRef` representation with defined absolute
and relative forms. Rules shipped with a fragment need relative references; family-level rules need
an instance-qualified path when the same fragment is mounted more than once. Fragment mounts
therefore require stable aliases, and recursive fragment expansion must be rejected.

A reference to a repeating slot also needs an occurrence-selection contract. `All`,
`SameOccurrence`, `NearestPrevious`, or another explicit selector may be appropriate in different
contexts; silently choosing the first or nearest occurrence is not. Slot-relative anchors and
conditional assertions should normally default to `SameOccurrence` when evaluated inside a repeated
ancestor and fail as ambiguous when no common occurrence scope exists.

Occurrence paths containing ordinals are deterministic for the same input and rule set, but they are
not persistent across insertion or removal of an earlier occurrence. Documentation and APIs must not
promise cross-revision durability from an identity such as `item[2]`. If persistent identity across
document revisions becomes a requirement, it needs a semantic key or source-object identity rather
than a positional path.

Rule ids remain stable provenance and diagnostic identities. They are not references through which
one rule discovers semantic output produced by another.

### Keep layout evidence out of the structural contract

The inferred opener rule is structural: a repeating composite with one required singleton opening
child can declare that each occurrence starts when that child is bound. Explicit boundary evidence
can instead be producer-specific. An inline
`startsOn: FlowBoundary.Matching(layoutPredicate)` on a structural node would make the template
change when the producer layout changes, contradicting the separation at the start of this document.

For a boundary that cannot be derived from shape, the template should name the required partition
strategy or boundary, while the layout artifact defines how that name resolves for a producer
version. A slot may safely say `startsOn: Slot("title")` or refer to a stable named boundary; raw
coordinates, fonts, text matching, and geometric predicates stay in regions and binding rules.

The same split applies to specialized slots. A `Table` declaration owns semantic policy such as its
role, header behavior, and constant `/Scope` attributes. Producer-dependent column coordinates,
row-boundary geometry, and cell recognition remain mapping configuration keyed to the table slot.
Moving `TableOver` off the Group stage must not move layout coordinates into the structural
template.

Content-independent does not necessarily mean globally constant. `/Lang`, fixed-logo alt text, and
similar properties may vary by locale or family version. Reusable fragments need typed parameters or
family-level declaration values rather than layout rules or duplicated fragment definitions.

### Partitioning requires a complete failure contract

Opening-child derivation is the default for the common case, but it is not sufficient as the complete
partition model. Before it replaces `BindOver`, the implementation and report must define:

- what happens to descendant claims before the first opener;
- how missing, duplicate, and multi-claim openers are diagnosed;
- how a logical opener may be explicitly coalesced from several claims;
- which order is used in multi-column and rotated layouts;
- how cross-page continuation and a new boundary on a continuation page interact;
- how nested boundaries close open inner and outer occurrences;
- how explicit geometric, keyed, or joined partitions are represented; and
- how every decision appears in the dry-run assembly report.

Unexpected claims, ambiguous ownership, and incomplete occurrences are commit blockers. Partition
behavior must not fall back to one giant occurrence when evidence is missing.

Slot-declared boundaries should land additively while Group remains available. Group removal is a
decision made from real family evidence, including multi-column and irregular-table inputs, not only
from synthetic fixtures.

### Reading-order disagreement is a prerequisite gate

The proposed per-slot guardrail must land before broad migration or additional authority moves onto
the template. Assembly cannot silently use declaration order and then validate the tree it just
constructed.

Source painting order is not automatically the intended reading order either, so the check is not a
universal requirement to preserve source order. The report should compare source/content-stream
order, geometric reading evidence, and declared output order, and identify every inversion with its
slot and candidates. The default is a diagnostic requiring review. Intentional reordering requires
an explicit per-slot policy or acknowledgement and remains visible in dry-run and commit reports.

The result still undergoes the ordinary logical-order and MCID integrity checks. An acknowledgement
permits an intentional move; it does not waive output validation.

### Region accounting is guarded absorption, not a semantic catch-all

Region-scoped artifact accounting is useful, but an unrestricted declaration such as "all unbound
footer content is pagination" can hide a failed semantic binding or a newly added disclaimer. A catch
region must declare and validate:

- the painting candidate kinds it may absorb;
- whether text is allowed, defaulting to false;
- optional text signatures, predicates, counts, or other limits for expected furniture;
- precedence and ambiguity behavior where accounting regions overlap; and
- the artifact type, subtype, bounds, and attachment metadata to emit.

Annotations, widgets, and other interactive objects are never absorbed as painting artifacts. An
item selected both by a structural binding and by region accounting is a conflict, not an accounting
success. Every absorbed item must appear individually in the report with its candidate identity,
region, declaration, and emitted artifact properties. Corpus runs should highlight newly absorbed or
disappeared content even when the declaration technically permits it.

Explicit artifact bindings remain available for furniture needing narrower recognition. Region
absorption runs only after structural bindings and explicit artifacts have established ownership,
and content outside an approved catch region remains a non-suppressible blocker.

### Semantic assertions and mapping checks remain separate

Slot-keyed assertions validate the completed semantic contract: for example, exactly one `title`
slot is filled, or every `item` occurrence containing `discount` also contains `discount-total`.
Conditional and child-count assertions over repeating structures are evaluated per occurrence unless
they explicitly request document-wide aggregation.

Rule cardinality validates mapping behavior: a particular matcher was expected to find one input,
even if another rule could also fill the same slot. Replacing that check with only a slot count would
allow a broken or overly broad matcher to be hidden by another producer. `Rule.Cardinality` and
equivalent rule-local diagnostics therefore remain supported. What is removed is the use of rule ids
as durable dependencies between independent language constructs.

### Opaque derived interiors are a realization strategy

`Table` should be the first use of a general slot-realization contract rather than a permanent
one-off exception. At minimum, a slot compiles to one of these strategies:

- **Direct** — bind content directly to the declared leaf node;
- **Composite** — assemble declared descendant occurrences from their bindings; or
- **OpaqueDerived** — declare the slot root while a specialized materializer derives and owns a
  validated interior.

An opaque strategy declares its allowed root tag, supported profiles, interior grammar, ownership
rules, identity behavior, validator, and reporting. No generic binding or refinement may reach into
an opaque interior unless that strategy explicitly exposes a typed staging surface. This keeps
future tables, forms, annotations, or other derived constructs inside the invariant without adding
unreviewed exceptions.

### `Region` should be compositional, not a bag of optional fields

Unifying the authoring vocabulary does not require erasing meaningful lifecycle differences. A
fixed zone, an anchor-derived area, and a repeated flow instance resolve differently. Model `Region`
as a discriminated expression or small algebra of fixed, anchored, and flow forms, with tolerance as
a composable decoration and continuation available only where it is meaningful. Presets may hide the
details for common zones.

Declaration validation must reject invalid combinations before page parsing. Resolution must state
the coordinate system, page rotation behavior, tolerance application, activation count, and overlap
rules. Anchors remain points derived from evidence and may yield region expressions; they are not
regions themselves.

### Authoring lifecycle and template applicability are explicit

The worklist-to-freeze transition needs an execution contract. `Authoring` policy may report unfilled
slots and other incompleteness while dry-running a corpus. `Enforced` policy treats the same
conditions as commit blockers. Production entry points and CI accept only `Enforced`; authoring mode
must not be embedded in a distributable rule set as a way to weaken compliance.

Each family template also has an identity, version, target PDF/UA profile, and applicability check.
Applicability is evaluated before binding and fails closed when no template or more than one template
matches. Locale, producer version, and business-unit variants should compose shared fragments rather
than accumulate optional branches in one universal template.

Template compilation should include a feasibility pass against the available binding and realization
vocabulary. It cannot prove that document content will match, but it can reject a declared shape that
the engine has no supported way to realize for the selected profile.

### Required sequencing before broad migration

1. Fix declaration-only ambiguity validation so a prescriptive template validates without rules.
2. Settle typed slot references, fragment mount identity, occurrence scoping, and dependency-graph
   compilation before publishing the replacement serialized language.
3. Add detailed unaccounted-content inventory and the reading-order disagreement policy.
4. Add inferred and explicit slot occurrence boundaries without removing Group.
5. Exercise the authoring loop against at least two or three real internal template families and run
   them as corpora, not one document at a time.
6. Use that evidence to decide whether Group disappears and how much of the existing region surface
   should migrate behind the compositional `Region` API.

Do not migrate the forty-one legacy generated rule sets merely to validate the new surface. A small
real vertical slice should settle the breaking identity and execution contracts first; broad
migration follows once those contracts survive producer variation.

---

## Current gaps

Split by where the fix has to happen. **Breaking** means deferring it makes the eventual change a
migration for existing rule sets; **additive** means it can be added later without disturbing them.

### Architecture

| Gap | Impact | Deferral cost |
| --- | --- | --- |
| Slot declaration namespace is flat | No reusable template fragments; manual disambiguation for nested repetition | **Breaking** |
| Durable references key on rule id | Rule refactors break anchors, predicates, and assertions | **Breaking** |
| Anchors cannot reference slots | Positional logic is bound to rule names, the least stable identity in the system | **Breaking** |
| `Pages`/`SpansPages` live on template nodes | Structure/assertion factoring erodes as constraints accumulate | **Breaking** |
| Group stage may be vestigial | Its premise — undeclared hierarchy — was abolished by the invariant. What remains (`BindOver`, `TableOver`) is partitioning wearing a rule's clothes, and it is a second hierarchy mechanism to keep aligned with the first | Unscheduled by design; see [the invariant](#the-invariant-puts-the-group-stage-in-question) |
| Four declarations for one idea — a named place | `NamedLayoutZone`, `TolerancedZone`, `FlowRegion`, and anchors do not compose: tolerance lives on one type and continuation on another, so a fuzzy band that continues across pages is inexpressible | **Breaking** for the declaration surface — RRM-043 |
| Declared order is never checked against document order | Assembly reorders, the matcher validates the reordered tree, `CheckReadingOrder` early-returns (`RemediationSession.cs:5063`). Reading order is most of what PDF/UA is for, and there is no diagnostic when the two disagree | Additive, but the risk grows as more authority moves onto the template — RRM-046 |
| Descriptive mode still present | Two materialization paths to maintain and test | Additive (removal) |
| One template per composed rule set | Families cannot share declared structure even where it is genuinely identical | Additive, after fragments |
| Artifact accounting is a parallel declaration system | "Content matched nothing declared" answers to two vocabularies (`PrescriptiveUnaccountedContent`, `ArtifactUndeclared`) | Additive |

### Language

| Gap | Impact | Deferral cost | Tracker |
| --- | --- | --- | --- |
| No alternation or unordered groups | Variable families are forced to over-use `Optional`, which weakens the contract toward nothing | Additive — see the note below; cheaper than it looks | — |
| Declaration ambiguity check is coupled to binding state | A prescriptive template cannot be declaration-validated before its rules exist, which is step 1 of the authoring process | Defect, not a design gap | — |
| Occurrence boundaries are not declarable | `BindOver` partitions by run-continuation, and no claim predicate says "start a new occurrence here." The page break is the only working boundary, so a within-page repeating composite silently collapses into one occurrence | Do not extend the predicate vocabulary; declare the boundary on the slot instead | RRM-042 |
| Flow-region instances are page-granular and unused within a page | `FlowRegionInstanceId(regionId, activationIndex)` and `FindSharedInstance` already express "did new content start a new occurrence," but both resolver paths activate at most once per page (`DocumentFlowRegionResolver.cs:90`, `:138`), and `BindOver` only consults the instance when claims cross a page | Additive, and correct under either outcome of the Group question — both designs consume the same instances | RRM-042 |
| Content-independent attributes require a rule | A fixed `/Lang` or `TH` `/Scope` costs a Refine rule that re-selects content already bound to the slot needing the attribute | Additive | RRM-044 |
| Content accounting is per item, not per region | Every kind of page furniture needs its own `Artifact` rule; `ZoneId` constrains where a declared artifact may appear but does not declare a region's leftovers, so the backlog never closes on a dynamic family | Additive | RRM-045 |
| The prescriptive leftover diagnostic identifies nothing | `ApplyLeftoverPolicy` reports one per-page message and returns before populating `unaccountedContent`, so the strictest mode reports the least about what failed | Defect, not a design gap | RRM-045 |
| No split primitive | One content item carrying two roles — `Lbl` + `LBody`, label + value — is unreachable | Additive | RRM-021 |
| Recurring predicate logic cannot be named | Compound positional conditions are restated per slot; traces print expanded boolean trees | Additive | RRM-041 |
| No forward candidate inspection | Authoring a positional rule means guessing at what the engine sees | Additive | RRM-037 |
| Row membership is not declarable; no spans, `/Scope`, `/Summary` | Irregular and complex tables are not expressible | Additive | RRM-005, RRM-025 |
| Heading levels are literal with no ordering model | Optional sections shift heading levels and cannot be expressed relatively | Additive | RRM-023 |
| Geometry is not text-orientation aware | Rotated or vertical content is out of reach | Additive | RRM-008 |
| Assertions cannot key on a slot | They key on rule id (churns) or tag (ambiguous when five slots are `P`), so per-slot constraints have nowhere to live — which is why `Pages`/`SpansPages` leaked onto template nodes | Additive | — |
| Assertions cannot scope to a slot | `Scope` is `Document` or `PerPage` only; "every row has four cells" cannot be confined to a subtree | Additive | — |
| No conditional assertions | "If slot X is bound then slot Y is required" is inexpressible, so variable families are pushed into the grammar instead | Additive | — |
| Candidate and claim predicates are separate languages | `ClaimPredicates.FromSlot` lets a Group or Refine rule condition on what a slot bound, but Classify rules use candidate predicates with no slot awareness — so a leaf bind cannot say "only if slot X was bound" | Additive | — |

### Extending the grammar is cheap in prescriptive mode

This is worth stating precisely, because the obvious reading — "the grammar is a sequence, so
extending it means a real parser" — is wrong once descriptive mode is gone.

`PrescriptiveTemplateAssemblyPlan.AssembleChildren` **does not parse a sequence**. It walks the
template's declared children in order and, for each particle, pulls matching sources out of the
remaining set by slot-id equality. Output order is declaration order by construction. Occurrence is a
count of how many sources carried that slot. Consequences:

- **Unordered groups are already the behavior.** Assembly ignores source order entirely; declaring
  order is what would need adding, not ignoring it.
- **Alternation is small.** A `choice` particle is "which branch has bound sources — exactly one
  assembles, more than one is an error." It is a particle type, a validator rule, and a branch in
  `AssembleChildren`.
- **Conditional structure belongs in assertions**, not the grammar — "if slot X is bound then Y is
  required" is a constraint, not a shape.

The greedy no-backtracking walk lives in `RemediationStructuralTemplateMatcher.MatchChildren`, which
is **descriptive-mode machinery**. In prescriptive mode it validates a tree that was constructed from
the template, so its ordering checks are near-tautological and its useful residue is unbound,
unexpected, and occurrence detection. Extending the grammar therefore does not require teaching the
matcher to backtrack; it requires deciding how much of the matcher survives descriptive removal.

The flip side, and the reason this is cheap: **prescriptive mode does not verify declared order
against the document.** Assembly silently reorders content into declared order, the matcher then
validates that reordered tree, and `CheckReadingOrder` returns early under a prescriptive template
(`RemediationSession.cs:5063`). Three layers, no check that the declared order is the order a reader
needs. That is a deliberate consequence of the template owning order, but it is currently
unqualified: there is no diagnostic when declared order and content order disagree. Reading order is
most of what PDF/UA is for, and this should be scoped — a per-slot opt-in for intentional
reordering — rather than left globally silent.

**The declaration ambiguity defect.** `ValidateNode` rejects two same-tag sibling particles when one
can be empty, using `ParticleKey`, which only includes the slot id when that slot appears in
`boundSlots` — a set derived from the *rules*. With no rules, every particle keys on tag alone.
Verified: a prescriptive template of `H1#title`, `P#body` (Optional), `P#notes` (Optional) fails
declaration validation with *"Template child sequence is ambiguous between 'Document/body' and
'Document/notes'"* when zero rules exist, still fails when partially bound, and passes once all three
slots are bound. That directly blocks step 1 of the authoring process, and the error disappears as
rules are written, which is the most confusing possible signal.

Prescriptive mode requires a unique id on every non-`Document` node, so ambiguity is impossible there
by construction. The fix is to key on the id whenever the template is prescriptive rather than
whenever a rule happens to bind it. It is a one-line condition and should land before any POC
authoring starts.

### Process

| Gap | Impact |
| --- | --- |
| The corpus is entirely synthetic | Fixtures can confirm beliefs about producer output but cannot contradict them. M0's "at least one real input per family from the actual producing system" is still unchecked, and the template is a bet on what the producer emits |
| Gates have been marked complete ahead of evidence | RRM-039 was marked Complete before any prescriptive fixture existed. Each premature Complete removes a reason to look again |
| No feasibility check at template declaration | An author can declare structure the rule vocabulary cannot produce — `Lbl`/`LBody` where label and body share a `Tj`, `/ListNumbering`, row-scoped headers — and only discover it deep into iteration |
| No corpus-level run | Rules are iterated one document at a time, so per-slot failure patterns across a family are assembled by hand |
| Migration debt | 41 of 42 generated rule sets predate prescriptive mode |

---

## Migration

Prescriptive-only means every existing rule set needs a template. One of forty-two is prescriptive
today; six declare a descriptive template; the rest declare none.

The order that works:

1. Rule sets with a descriptive template: flip the mode, add slot ids and `Bind` actions. The tree
   they produce is already validated against the declared shape, so the diff is mechanical.
2. Rule sets with no template: hand-author a first-draft template, then use the existing
   `TemplateDifferences` report to converge. This is the **one** narrow case where deriving a draft
   template from current output is legitimate — the rules already exist and are known-good, and the
   template is being back-filled rather than defined. It is a one-time bootstrap per rule set, never
   a refresh loop, and the draft must be reviewed before it is frozen.
3. Remove descriptive mode once the corpus is migrated.

Do not add a regenerate-template step to CI. A template that tracks its rules automatically asserts
nothing.
