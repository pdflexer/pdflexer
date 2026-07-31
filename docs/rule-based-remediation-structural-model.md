# Structural Model for Rule-Based Remediation

**Status:** Design proposal — 2026-07-29
**Related:** [Rule-Based Remediation](rule-based-remediation.md) ·
[Gap Tracker](rule-based-remediation-gaps.md) · [Delivery Plan](rule-based-remediation-plan.md) ·
[Validation Corpus](rule-based-remediation-corpus.md)

## 1. Purpose

Today a rule set is a flat list of rules. The structure a document is *supposed* to have exists only
in the rule author's head, distributed across rule ids, predicates, and cardinality constraints.
Nothing states it, so nothing can check it, and every downstream problem inherits that: applicability
cannot be tested, output correctness cannot be asserted beyond hand-written counts, and a reviewer
cannot tell what the rule set is trying to produce without reading every rule.

This proposes making the expected structure an explicit, authored artifact — a **structural
template** written in PDF structure vocabulary — and reorganizing authoring around it:

1. Declare the structure the document should have.
2. Declare the labels and anchors that discriminate its parts.
3. Write rules that classify and group content into it.

The template is written in the same vocabulary as the deliverable. There is no intermediate
"logical section" model and no mapping layer between one vocabulary and another. The thing the
author declares is the thing the output is graded against.

## 2. What changes and what does not

**Unchanged.** Candidates, claims as the intermediate representation, exclusive text and content
ownership, the leftover policy, anchors, toleranced zones, flow regions, predicates, actions,
confidence, and the commit pipeline. The template is additive. A rule set with no template behaves
exactly as it does today.

**Changed.** Rule sets gain four declarations — a structural template, an artifact inventory, and two
label namespaces — and gain a validation phase that compares the produced structure tree against the
template. In a later phase, the template also drives materialization.

**Superseded.** The "logical sections" concept discussed earlier is dropped. It would have required
its own vocabulary plus a mapping to PDF structure; the template uses the target vocabulary directly.

## 3. The authored artifacts

A rule set becomes five things rather than one:

| Artifact | Answers | Axis |
| --- | --- | --- |
| Structural template | What structure should this document have? | Document |
| Artifact inventory | What page furniture is expected, and where? | Page |
| Candidate labels | What named conditions describe raw content? | Candidate |
| Claim labels | What named conditions describe classified content? | Claim |
| Anchors, zones, flow regions | Where is each part, geometrically? | Page / flow |
| Rules | How does content become claims and structure? | — |

Anchors, zones, and flow regions keep their current meaning and become the *boundary mechanism* the
template's slots are discriminated by, rather than free-standing concepts an author invents ad hoc.

## 4. Authoring workflow

The ordering is a real dependency, not a style preference.

**Step 1 — declare the structure.** Write the expected tree. This is reviewable by someone who knows
PDF/UA but not this rule language, which is a different and larger population than the set of people
who can review a rule list. It is also the artifact a model produces most reliably: "the expected
PDF/UA structure of an invoice" is a far better-posed question than "seven interdependent rules with
geometric tolerances."

**Step 2 — declare labels and anchors.** Anchors are chosen *to discriminate the slots the template
declares*. Authoring them first means guessing what they need to tell apart. Labels name the
conditions that recur across rules.

**Step 3 — write rules.** Classify content into leaves, group into parents, refine attributes, as
today. Each rule may declare which template slot it fills.

**Step 4 — dry-run and diff.** The produced tree is compared to the template and the difference is
reported positionally. This replaces "read the report and decide whether it looks right."

## 5. The structural template

### 5.1 Vocabulary

Node types are PDF standard structure types. No new type vocabulary is introduced. The engine
already carries most of what is needed to validate a template in isolation: the standard type list
and the parent-child legality rules used by strict conformance validation.

A template is checked for legality **before any document is processed**. "Your expected structure is
not valid PDF/UA" is an authoring error available at declaration time, not a commit failure
discovered after a rule set has been written.

### 5.2 Occurrence operators

The template is a content model — a regular tree grammar. The formalism is deliberately borrowed
rather than invented; the constraints that keep XSD and RELAX NG content models tractable apply here
for the same reasons.

Phase one ships three operators:

| Operator | Meaning |
| --- | --- |
| *(sequence)* | Children occur in the declared order |
| `?` | Optional — zero or one |
| `*` / `+` | Repetition — zero-or-more, one-or-more |

Deliberately deferred:

- **`\|` alternation** — a slot that is either a table or a paragraph list. Real, but not needed by
  the initial corpus, and it interacts with the determinism requirement below.
- **Recursion** — nested lists of arbitrary depth. Covered in phase one by explicit depth
  (`L > LI > L > LI`), which handles the observed cases without opening the general case.
- **Unordered groups** — PDF reading order is ordered by nature; an unordered content model would be
  asserting something weaker than the document actually guarantees.

> [!NOTE]
> The operators declare **whether** a slot repeats. They do not declare what **delimits** one
> occurrence from the next, and that is a separate fact that has to be read from the document. This
> spec left it implicit; in prescriptive mode it is currently answered by `BindOver`'s claim runs,
> which break only at a page boundary and therefore collapse a within-page repeat into one
> occurrence. The occurrence boundary belongs on the repeating slot, derived from the declared child
> shape where possible — see
> [RRM-042](rule-based-remediation-gaps.md#rrm-042-repeating-slot-occurrence-boundaries-are-not-declarable)
> and the
> [architecture direction](rule-based-remediation-architecture.md#1-occurrence-boundaries-are-declared-on-the-slot).

### 5.3 Determinism

**Content models must be deterministic**: at any point in matching a child sequence, the next element
type must select at most one candidate position in the model. This is the same constraint XSD calls
unique particle attribution.

This is a design requirement, not an implementation detail, because it is what makes the failure
message good. A deterministic model can say *"expected `Table` at `body[2]`, found `P`"*. A
nondeterministic one can only say *"the children of `body` do not match."* Given that the entire
point of this work is a diagnostic an author can act on, ambiguity is not worth its expressiveness.
A template that is nondeterministic is rejected at declaration time with the ambiguous position
named.

### 5.4 Slot identity

Every template node has an address. Two forms:

- **Declared id** — an author-supplied stable name (`"line-items"`). Used by rules to bind. Stable
  across template edits.
- **Derived path** — positional (`Document/Sect[1]/Table`). Used by diagnostics. Always available.

Rules bind by declared id. Diagnostics report both. Repeating slots address instances by index
(`line-items[3]`).

### 5.5 The page axis

The template is document-scoped: one tree for the whole document. A node may own content on several
pages — cross-page structure elements are already supported end to end.

Two optional per-node declarations sit on the page axis:

- `pages` — a page selector constraining where a slot's content may occur (`first`, `last`, a range).
- `spansPages` — an assertion that a node is expected to span a page break, or expected not to. This
  is a check, not a mechanism; continuation itself is governed by flow regions.

### 5.6 Illustrative shape

Illustrative only; the serialized schema is the contract and is specified separately.

```
template:
  Document:
    - Sect id=letterhead pages=first
        - P id=sender
    - Sect id=body
        - P    id=salutation
        - P    id=body-paragraph  repeat="+"
        - P    id=note            optional
        - Table id=line-items
            - TR id=header-row
                - TH repeat="+"
            - TR id=data-row repeat="+"
                - TD repeat="+"
        - P    id=signature
```

## 6. The artifact inventory

Page furniture is **not** part of the structural template, because artifacts are not structure
elements — they live outside the structure tree. Running headers, footers, page numbers, rules, and
watermarks therefore need a parallel declaration on the page axis.

```
artifacts:
  - id=page-footer     subtype=Pagination  pages=every  zone=footer  occurrence=exactly-one
  - id=letterhead-rule subtype=Layout      pages=first  zone=rule    occurrence=0..1
```

This closes a hazard the current model has no answer for. Today `AutoArtifact` sweeps unclaimed
content into artifacts, which is how a rule set can silently convert an invoice total into hidden
content while passing every conformance check available. With a declared inventory, **content
artifacted that matches no declared artifact is an error**, not a silent success. The leftover
policy gains something to check against rather than a policy switch between "fail on anything" and
"absorb everything."

**Delivered semantics** (RRM-040):

- **Closed when declared.** With no inventory, behavior is exactly as before. Once any composed rule
  set declares an item, every produced artifact must match something declared — whether produced by
  an explicit `Artifact(...)` rule or absorbed by the leftover policy. Rule-produced artifacts are
  not exempt; the hazard does not care which path hid the content.
- **Union composition.** Items from every composed rule set merge into one effective inventory and
  duplicate ids are rejected, exactly as anchors, zones, and flow regions already compose. A
  boilerplate rule set can own the shared furniture while a family set adds its own.
- **Occurrence is per page**, expressed as the same inclusive count range assertions use, and
  evaluated for every page the item's selector includes. It is the teeth: an item declared
  `exactly-one` per page that absorbs fourteen items is a violation, which is the "footer zone
  swallowed the table" case that zone matching alone waves through.
- **Zone optional, ambiguity rejected conservatively.** `ArtifactSubtype` has four values, so header
  and footer are both `Pagination`; two items sharing a subtype must both declare a zone. Declaration
  validation cannot know the page count — a one-page document is both first and last — so page
  selectors are ignored for this check. The cost is an occasional unnecessary zone.
- **Untyped absorbed content matches only zone-declared items.** Content swept up by the leftover
  policy carries no authored subtype, so it is discriminated geometrically or not at all. A zoneless
  item never absorbs leftovers. This makes zone-qualification load-bearing rather than advisory: a
  bare "one footer per page" declaration cannot silently absorb a mid-page total.
- **Matched absorbed content adopts the item's subtype** instead of being wrapped as a bare
  `/Artifact`, so the inventory is load-bearing rather than only a check. Output changes only when an
  inventory is declared.

## 7. Labels

A label is a **named predicate**, not a marking pass. `Label("body-text")` expands at the point of
use.

```
candidateLabels:
  body-text: Not(Page.First).And(Not(Flow.InZone("margins")))
  money:     Text.Matches("^\\$?[0-9,]+\\.[0-9]{2}$")

claimLabels:
  heading:   ClaimIs("H1").Or(ClaimIs("H2")).Or(ClaimIs("H3"))
```

Named predicates rather than a labelling stage, for four reasons:

- **No ownership interaction.** Nothing is claimed, so labels overlap freely. A marking pass could
  not: classify ownership is exclusive, so a second rule labelling already-labelled text is rejected
  as a conflict, and `Override` deletes the first claim rather than coexisting with it.
- **No new stage.** No ordering, no dependency graph, no interaction with the pipeline shape.
- **Composable.** `Label("body-text").And(Text.StartsWith("Dear"))` works because it is a predicate.
- **Better traces.** A rejection currently prints the whole expanded boolean tree. A named predicate
  lets the trace print `body-text` and expand on request. The diagnostic payoff here is plausibly
  larger than the authoring payoff.

**Two namespaces, deliberately.** A candidate label describes raw content ("this text is in the
body"); a claim label describes classified content ("this claim became a heading"). They are
evaluated at different times against different inputs. Using one where the other is expected is a
validation error, not a coercion.

**Naming.** These are *labels*, not tags. `Tag` in this model already means the PDF structure tag —
`Tag("P")`, `ClaimIs(tag)`, `ProducedTag`. Calling a label a tag would make `HasTag("body text")`
read as "is a PDF element of type body text," which is the opposite of the intent. The collision
matters more than usual because generated rule sets are a target audience.

Labels may reference other labels. Cycles are rejected at declaration time.

## 8. Rules and binding

### 8.1 Phase one — descriptive

Rules build the tree exactly as they do today. A rule may additionally declare the template slot it
fills:

```
rules:
  - id=salutation  slot=salutation  action=Tag(P)  predicate=Label("body-text").And(Text.StartsWith("Dear"))
  - id=body-line   slot=body-paragraph  action=Tag(P)  predicate=Label("body-text")
```

`slot` is optional. Unbound rules behave exactly as now — utility rules, artifacting, and
refinement generally have no slot.

After the plan is finalized, the produced tree is matched against the template and the difference
reported. Nothing about materialization changes.

**Why descriptive first.** It is cheap, it delivers most of the value immediately, and — most
importantly — it validates the template language itself against real documents *before* the
pipeline depends on it. Committing the materialization path to a content model that has never met a
real corpus is the expensive version of this mistake.

### 8.2 Phase two — prescriptive

Later, the template drives materialization: declared container nodes are created from the template,
and rules bind claims into slots rather than assembling parents bottom-up.

The significant consequence is that **declared hierarchy no longer needs to be assembled**. Today
parents are built by Group rules consuming claims, and a Group rule cannot consume another Group
rule's output — which is why nested lists are unreachable. With a prescriptive template, `L > LI > L
> LI` is something the template states and rules populate. No fixed point, no multi-pass grouping,
no dependency ordering between grouping rules.

This materially changes the open question about the stage model. That decision should be made after
phase one, informed by it, rather than settled first and then revisited.

## 9. Validation and diagnostics

Phase one produces a **positional tree diff** against the template. Reported difference kinds:

| Kind | Example |
| --- | --- |
| Missing required | `body/signature` expected, absent |
| Unexpected node | `body[4]` is `Figure`, not permitted here |
| Wrong order | `signature` precedes `line-items` |
| Occurrence violation | `header-row` expected exactly one, found three |
| Illegal nesting | `TD` outside a `TR` |
| Undeclared artifact | an item artifacted on page 2 matching no declared artifact |
| Missing declared artifact | `page-footer` required on every page, absent on page 3 |
| Artifact occurrence violation | `page-footer` expected exactly one, 14 items absorbed on page 2 |
| Slot bound but unfilled | rule `signature` bound to `body/signature` produced nothing |

Each is a diagnostic with a code, routed through the existing strictness and suppression mechanism,
so a family with a known deviation can suppress one code at one scope with a recorded reason rather
than lowering global strictness.

This is the first diagnostic surface in the system that speaks in the author's own terms. Existing
diagnostics report engine facts — orphaned MCIDs, reading-order drift, per-rule match counts. A tree
diff reports the author's intent against the outcome.

## 10. Interaction with the existing model

**Claims and ownership** — unchanged. Claims remain the IR; ownership remains exclusive; the
template describes the shape claims are expected to produce, not how they are produced.

**Stages** — unchanged in phase one. Reconsidered in phase two, see §8.2.

**Flow regions** — unchanged and complementary. The template says a `Table` is expected; the flow
region says where its content begins and ends and whether it continues across pages. Cross-page
structure is already supported end to end, so a template node spanning pages needs no new mechanism.

**Rule cardinality** — the template's occurrence operator is the cardinality declaration for bound
rules. Per-rule cardinality remains for unbound rules. Declaring both on the same rule is a
validation error rather than a silent precedence rule.

**Semantic assertions** — the template subsumes most hand-written assertions. Rule-output counts,
structure-element counts, and parent-child shapes are all derivable from it. Explicit assertions
remain for statements the template cannot make — value-level or cross-document conditions.

**Applicability** — a rule set whose template's required anchors do not resolve is not applicable to
the document. This is the weak form of a family guard and it comes for free. It is not a complete
substitute for an explicit fingerprint, because it tests the rule set's own preconditions rather
than the document's identity, but it converts the common case from "produce wrong structure" to
"refuse."

**Confidence and tolerances** — unaffected. The template constrains and validates; it does not
match. Anchors, geometric tolerances, and their calibration remain exactly as hard as they were.

## 11. Non-goals

Stating these because a declared expected structure can look like it does more than it does.

- **It is not a matcher.** Deciding which content fills slot 3 versus slot 4 is still rules,
  anchors, labels, and tolerances. The template constrains the answer and detects a wrong one; it
  does not find the right one.
- **It does not reduce the need for geometric calibration.** If anything it raises the value of
  calibration by making its failures visible.
- **It does not repair input-level defects.** Fonts, encodings, and word boundaries live below the
  structure layer and are unreachable from here.
- **It does not replace external validation.** veraPDF proves well-formedness; the template proves
  the output matches what the author intended. Both are required and neither implies the other.
- **It is not a general document schema language.** Scope is bounded to what PDF/UA structure needs.
- **Declaration-time legality checking is shallow.** The nesting check rejects the constructions PDF/UA
  names explicitly — `TD` outside a `TR`, `LI` outside an `L` — and defaults permissive everywhere
  else, so `P > Table` and `Figure > H1` validate clean. "Validated for PDF/UA legality" means
  "no known-illegal nesting," not "proven legal."
- **The artifact inventory grades what remediation produces**, not what the input already contained.
  Marked content that arrived as `/Artifact` is outside its scope.

## 12. Phasing

| Phase | Content | Depends on |
| --- | --- | --- |
| 0 | Labels — named predicates, two namespaces | nothing |
| 1 | Structural template, descriptive; artifact inventory; tree diff | labels are helpful, not required |
| 2 | Composition-model decision, informed by phase 1 | phase 1 |
| 3 | Prescriptive template drives materialization | phase 2 |

Phases 0 and 1 are MVP work. Phase 2 is a decision with a small implementation. Phase 3 is post-MVP
and should not be committed to until phase 1 has met a real corpus.

The first six-family fixture pass established two concrete constraints for phase 2:

- a closed template is owned by a document family, not by any generic rule helper the family happens
  to reuse; distinct report, form, sidebar, and mixed-page shapes require distinct declarations;
- actions that synthesize hierarchy must resolve one planned structural shape and materialize that
  same shape. `TableOver` now shares its resolved `Table > TR > TH/TD` plan between template
  matching and commit rather than presenting consumed `Span` claims during planning.

## 13. Open questions

1. **Slot binding granularity.** May several rules bind to one repeating slot? Probably yes — a
   `data-row` filled by different rules for different row shapes — but it complicates the occurrence
   check, since the count is then across rules. Declaring the occurrence boundary on the slot
   (RRM-042) resolves this: the count comes from boundary activations, so it is independent of how
   many rules contributed claims.
2. **Partial templates.** Should a template be allowed to describe only part of a document, with the
   remainder unconstrained? Useful for incremental adoption; risks becoming the default and
   discarding the guarantee. A per-node `open`/`closed` marker is the likely answer.
3. **Alternation timing.** Deferred in phase one, but a document family with a genuinely variable
   section shape would force it earlier. The corpus should be checked for this before phase one is
   scoped.
4. **Template inheritance.** Families that differ slightly — an invoice with and without a discount
   block — could share a base template. Composition rules interact with the existing open question
   about rule-set composition and precedence.
5. **Where the diff runs.** Dry-run only, or also at commit? Commit-time is stronger; dry-run-only
   keeps the commit path cheaper. Probably both, with the commit-time check gated by strictness.
