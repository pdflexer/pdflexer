# Remediation — Form XObject Handling

Last updated: 2026-07-26

**Status:** Proposed. Not implemented, not scheduled.

How the rule-based remediation engine should treat form XObjects: when to flatten them into page
content, when to leave them intact, and what to do with the cases where neither is safe.

Related: [RRM-002](rule-based-remediation-gaps.md#rrm-002-declarative-rules-cannot-select-or-artifact-non-text-content)
(non-text content selection), [rule-based-m0.md](rule-based-m0.md) (the survey that decides how much
of this is needed), [rule-based-remediation-plan.md](rule-based-remediation-plan.md) (M4 — content
accounting).

---

## The problem

`RemediationSession.BuildPageStates` (`RemediationSession.cs:736`) parses page content with
`page.GetContentNodes<double>()` — `flattenForms` defaults to `false`. `ContentWrapExtensions.Flatten`
descends only into `MarkedContentGroup<T>`, never into `FormContent<T>`.

So a form XObject is a single opaque leaf to the rule engine. Its interior is unreachable: no
candidate enumeration, no predicate matching, no tagging.

For a document whose body text sits inside a form — common with template-driven and report-generator
output — **rules see nothing**. The form surfaces as one untagged item, and depending on the leftover
policy it either fails the commit or gets swept into an artifact. Under `AutoArtifact` that means the
entire document body becomes hidden content while passing PDF/UA validation. That interaction is the
RRM-016 hazard in its worst form.

The capability exists — `GetContentNodes<T>(flattenForms: true)` — but remediation does not use it,
and turning it on unconditionally is wrong for reasons below.

---

## Why unconditional flattening is wrong

A form XObject carrying `/Group << /S /Transparency >>` is a **compositing barrier**. Its contents
render into a separate buffer; that buffer composites onto the page once, using the alpha, blend
mode, and soft mask in effect at the `Do`. Inlined operators composite individually, directly onto
the page. The two differ whenever:

| Condition | Why inlining diverges |
| --- | --- |
| Group alpha < 1 | The alpha applies to the group *as a whole*. Inlined, it applies per object, so overlapping objects inside the group double-composite. |
| Blend mode ≠ Normal | Objects blend against the group's initial backdrop, determined by `/I` (isolated). Inlined, they blend against whatever is already on the page. |
| Knockout (`/K true`) | Each object composites against the group's *initial* backdrop, discarding earlier objects in the group. Not expressible as an inline operator sequence at any level of effort. |
| Soft mask at `Do` time | The `/SMask` in the ExtGState masks the group as a unit. |

The knockout row is the important one: this is not a flattener that needs to get smarter. Some
transparency groups have no inline equivalent, so the only correct behaviour is to detect them and
decline.

Nothing reads `/Group` today. `PdfName.Group` and `PdfName.Transparency` exist as constants and are
unused anywhere in the content path, which is why flattening is unconditional and why transparency
groups have already produced visible failures in the `ModelRebuild` regression runs.

### What flattening already gets right

Worth recording so it is not re-litigated:

- **Geometry.** `PageContentScanner` composes the form's `/Matrix` into the CTM and synthesizes an
  explicit `re W n` clip from `/BBox` (its `ClipForm` state). Flattening is geometrically faithful.
- **Resource names.** `ContentWriter.AddResource` (`ContentWriter.cs:214`) keys by object identity and
  mints fresh names, so a form's `/F1` cannot clobber the page's `/F1` on rewrite. The lenient
  `MergeResources` in the scanner is a scan-time convenience and does not leak into output.

---

## The flatten-safety predicate

Cheap and fully static. Evaluate per `Do`:

1. **No `/Group`, or `/S` is not `/Transparency`** → flatten. This is the large majority of real
   forms; most are plain template placement.
2. **Transparency group, but inert at the `Do`** → flatten. Inert means all of:
   - non-stroking alpha `ca == 1` (ISO 32000-1 §11.6.6 — the group composites with the current alpha
     constant),
   - `CA == 1`,
   - `BM` is `/Normal` or `/Compatible`,
   - `SMask` is `/None`,
   - the group is not knockout (`/K` absent or `false`).
3. **Otherwise** → do not flatten.

A conservative refinement worth adopting: even when the `Do`-time state is inert, scan the form's own
`/ExtGState` resources for any `ca`/`CA` < 1, non-Normal `BM`, or non-`None` `SMask`. A knockout or
non-isolated group whose *interior* uses transparency still diverges when inlined. Rejecting those
costs little — they are rare — and removes the subtlest failure class.

Isolation (`/I`) only affects the result when the blend mode is non-Normal or a backdrop interaction
exists, both already excluded by the inert test, so it needs no separate check.

---

## Why the disposition is per form, not a session flag

There is a second approach: leave the form intact and tag inside it. PDF supports this natively — a
form XObject carries its own `/StructParents`, MCIDs scope to that stream, and the parent tree
resolves them. The machinery already exists: `StructuralBuilder.BindFormXObject` allocates form
`/StructParents` (and now guards against binding one form under two structure elements), and
`McidAllocator` is owner-scoped through `ConditionalWeakTable<IPdfObject, AllocationState>`.

In-place tagging has one fatal flaw: **a form drawn twice produces the same MCID twice on the page**,
and the structure tree cannot distinguish the instances. That is a limitation of the PDF model, not
of this implementation.

The two approaches fail on disjoint inputs:

| | Non-inert transparency group | Form drawn more than once |
| --- | --- | --- |
| **Flatten** | ❌ compositing diverges | ✅ duplicates the content, which is what tagging wants |
| **Tag in place** | ✅ stream untouched, renders identically | ❌ ambiguous MCIDs |

Neither wins globally, so the choice belongs per form:

| Form | Disposition |
| --- | --- |
| No transparency group, or an inert one | **Flatten** into page content |
| Non-inert group, drawn once | **Tag in place** with form-scoped `/StructParents` |
| Non-inert group, drawn more than once | **Clone the stream per `Do`**, then tag each copy in place |

The clone-per-use option dissolves the reuse problem completely, and it is available *because this is
individual-document remediation rather than bulk*. Duplicating a form stream so each instance owns its
`/StructParents` costs file size and nothing else. In a bulk pipeline where forms exist to save space
that trade would be wrong; here it is free.

---

## Recommended scope

Do not build all three tiers. Build this:

### In scope

1. **The inertness check.** One predicate over the form dictionary and the `Do`-time ExtGState, per
   the rules above. This is the piece standing between the current behaviour and correctness.
2. **Flatten inert forms** in the remediation path — switch `BuildPageStates` to a flattening parse
   for forms that pass, so their interiors become ordinary candidates.
3. **Treat non-inert groups as atomic leaves.** Do not descend. Let a rule tag the form as a `Figure`
   with `/Alt`, or artifact it — and **emit a diagnostic** naming the form and the specific reason it
   was not flattened.

Step 3 is the pragmatic bet: non-inert transparency groups are usually graphic effects — drop
shadows, watermarks, charts with alpha — not body text. Treating them as atomic figures is often the
semantically correct answer regardless, and it means form-scoped tagging need not be built at all
for the MVP.

The diagnostic is not optional. A form that was silently artifacted because it could not be flattened
is exactly the failure RRM-016 exists to prevent; it must appear in the run output, not in the
rendering.

### Deferred

- **Tier 2, in-place tagging with form-scoped `/StructParents`.** Build only if the corpus shows body
  text inside non-inert groups. The infrastructure is already present when it is needed.
- **Tier 3, clone-per-use.** Build only if tier 2 is built *and* such a form is reused.

---

## Implementation notes

| Site | Change |
| --- | --- |
| `RemediationSession.cs:736` (`BuildPageStates`) | Currently `page.GetContentNodes<double>()`. Needs a parse that flattens selectively rather than an all-or-nothing `flattenForms` bool. |
| `ContentModelParser` / `PageContentScanner` | The flatten decision lives here. Today `flattenForms` is a constructor bool threaded to the scanner; it needs to become a per-form predicate callback, or the parser needs to consult the predicate as it encounters each `Do`. |
| `FormContent<T>` | Gains the group metadata needed to explain a decline — whether it has a transparency group and which inertness condition failed — so the diagnostic can name a reason rather than say "not flattened". |
| `ContentWrapExtensions.Flatten` | Unchanged. Once forms are flattened at parse time their contents are ordinary leaves; unflattened forms stay opaque leaves, which is the intended tier-3 behaviour. |

`FormContent<T>` throws `NotSupportedException` for `CopyArea`, `Split`, `ClipExcept`, and `ClipFrom`.
That stays true and is fine — under this design a form is either flattened (so those operations act on
its interior items) or deliberately atomic.

One caveat carried over from the surrounding architecture: any page touched by remediation has its
`/Contents` fully regenerated from the model (`RemediationSession.cs:2200`). Flattening therefore
inherits whatever rewrite-fidelity risk the content model carries generally. That is a separate
concern from this document, but it is why the inertness check must be conservative — a wrong flatten
decision is a silent visual regression, not an error.

---

## What the M0 survey must answer

Three numbers per document decide how much of this gets built:

1. How many form XObjects are drawn?
2. How many carry a `/Group` with `/S /Transparency` that is **not** inert at the `Do`?
3. Does any of those contain text?

If (2) is near zero across the corpus, ship the recommended scope and stop. If (3) is ever non-zero,
tier 2 becomes necessary and the milestone placement changes — because in that case rules cannot see
the document body at all, which is a first-contact blocker rather than a content-accounting
refinement.

These should be first-class survey outputs, not folded into a generic form-hazard count.

---

## Escalation path if rewrite fidelity proves to be a problem

The alternative that is strictly better on fidelity: **surgical injection** — write `BDC`/`EMC` into
the original content stream at byte offsets without regenerating it. Forms stay untouched, both
failure modes above disappear, and "no visual change" becomes true by construction rather than by
measurement.

It is a larger architectural change and is not proposed for the MVP. It is recorded here as the exit
ramp if content-model rewrite fidelity turns out to be a real problem rather than the pixel noise and
malformed-input divergence the existing `ModelRebuild` regression runs appear to be showing.

---

## Open questions

- Should the flatten predicate be caller-overridable — a session configuration option to force or
  forbid flattening for a known-good document family — or is the static check sufficient on its own?
- When a non-inert group is treated as an atomic leaf, is the default disposition `Figure` (requires
  `/Alt` from the rule author) or `Artifact` (requires the author to assert it is decorative)?
  Defaulting to neither, and failing until a rule claims it, is the safest option and the most
  annoying one.
- Does the clone-per-use option need `/BBox` or resource adjustment per copy, or is a shallow stream
  clone with a fresh `/StructParents` sufficient?

---

## Milestone placement

Implementation belongs in **M4** (complete content accounting) alongside RRM-002, since tagging a form
as a `Figure` is precisely the RRM-002 capability. The inertness check and the survey questions belong
in **M0**.

The exception: if the M0 survey answers yes to question 3 above, this moves to **M2**
(first-contact hardening). A document whose body text is invisible to every rule is not a
content-accounting gap — it is a document the engine cannot process at all.
