# Accessibility Gaps Round 2 — Phase 1 & 2 Implementation Review

Review of the working-tree changes that close findings 1–5 of
[`accessibility_gaps_2.md`](accessibility_gaps_2.md). Scope is the Phase 1 (silent corruption) and Phase 2
(conformance blockers) work only; Phases 3–5 remain open by design and are not assessed here.

Files reviewed:

- `src/PdfLexer/PdfStrings.cs`, `src/PdfLexer/Writing/McidAllocator.cs` (new)
- `src/PdfLexer/Writing/PageWriter.cs`, `FormWriter.cs`, `StructuralSerializer.cs`
- `src/PdfLexer/DOM/Annotations.cs`, `StructuralBuilder.cs`, `StructureNode.cs`
- `src/PdfLexer/PdfDocument.Accessibility.cs`, `ParsingContext.cs`, `ParsingOptions.cs`
- `src/PdfLexer/Remediation/RemediationSession.cs`
- `test/PdfLexer.Tests/AccessibilityCriticalRegressionTests.cs` (new),
  `AccessibilityAuthoringPhase2Tests.cs` (new), `AccessibilityFixtureGenerator.cs`

Everything below marked **Reproduced** was executed against the working tree with temporary probe tests
(since removed). Findings marked **Code read** were not executed; they are flagged as such.

---

## Verdict

The five findings are genuinely addressed and the approach is right in both phases.

- **Finding 1** (text strings) is correct and complete. `PdfString.CreateTextString` widens the guard to
  non-ASCII rather than `c > 255`, which is the right call — the old test would have emitted Latin-1 bytes
  through the `Iso88591` path in `StringSerializer` for the 0x80–0xFF range, exactly the em-dash case the gaps
  doc called out. Every accessibility write site is routed through it.
- **Finding 2** (MCIDs) is well designed. Keying `McidAllocator` on `page.NativeObject` / `form.NativeObject`
  via `ConditionalWeakTable` correctly shares state across `PdfPage` wrappers, `Replace` resets while
  `Append`/`Pre` seed from existing content, and `RemediationSession` now shares the same namespace. The
  duplicate-registration guards in `BuildParentTree` are a good backstop and are exercised by tests.
- **Findings 3–5** are implemented as specified, including the Matterhorn nesting check, the `PrinterMark`
  artifact rule, the full set of field-type factories, and the fillable-form fixture covering all of them.
  `ParsingContext.Warning` is a sensible addition for the non-strict path.

Six defects below are worth fixing before this is called done; three of them are uncaught exceptions or
wrong-on-the-wire output rather than missing polish. Nothing requires rework of the design.

### Priority

| # | Item | Severity | Effort | Evidence |
| --- | --- | --- | --- | --- |
| [R1](#r1-keynotfoundexception-escapes-savepdf-when-ap-has-no-n) | `KeyNotFoundException` escapes save when `/AP` has no `/N` | **High** | XS | Reproduced |
| [R2](#r2-keynotfoundexception-from-addannotbindannotation-when-the-annotation-has-no-p) | `KeyNotFoundException` from `AddAnnot` when annotation has no `/P` | **High** | XS | Reproduced |
| [R3](#r3-unselected-choice-fields-render-a-value-they-do-not-have) | Unselected choice fields render a value they do not have | **High** | XS | Reproduced |
| [R4](#r4-addformfieldwidget-title-discards-title-every-radio-button-gets-the-group-tooltip) | `AddFormField(widget, title)` discards `title`; radio buttons share one label | **High** | S | Reproduced |
| [R5](#r5-needappearances-and-acroform-da-are-last-write-wins-across-fields) | `NeedAppearances` and AcroForm `/DA` are last-write-wins | **Medium** | S | Reproduced |
| [R6](#r6-idtree-names-loses-byte-ordering-once-any-id-is-non-ascii) | `IDTree` `/Names` loses byte ordering once any ID is non-ASCII | **Medium** | S | Reproduced |
| [R7](#r7-only-o-suppression-was-applied-to-the-table-attribute-dictionary-only) | Only-`/O` suppression applied to the Table attribute dict only | **Medium** | XS | Reproduced |
| [R8](#r8-the-annotation-sweep-is-strict-only-while-reference-validation-warns) | Annotation sweep is strict-only while reference validation warns | **Medium** | S | Reproduced |
| [R9](#r9-the-new-phase-2-widget-test-does-not-run-in-a-normal-checkout) | New Phase-2 widget test does not run in a normal checkout | **Medium** | XS | Reproduced |
| [R10](#r10-print-false-widgets-can-never-be-saved-in-strict-mode) | `print: false` widgets can never be saved in strict mode | Low | XS | Reproduced |
| [R11](#r11-generated-appearance-streams-omit-tx-bmc--emc-and-ignore-mk) | Appearances omit `/Tx BMC … EMC` and ignore `/MK` | Low | S | Reproduced |
| [R12](#r12-replace-after-a-tagged-append-pass-fails-with-a-misleading-message) | `Replace` after a tagged `Append` fails with a misleading message | Low | XS | Reproduced |
| [R13](#r13-every-pagewriter-re-parses-the-page-content-stream) | Every `PageWriter` re-parses the page content stream | Low | XS | Reproduced |
| [R14](#r14-smaller-notes) | Smaller notes (dead branch, role-mapped tags, shared forms) | Low | XS | Code read |

Effort key matches the source document: XS ≈ under an hour · S ≈ half a day · M ≈ 1–3 days.

---

## High

### R1. `KeyNotFoundException` escapes `Save`/`PDF` when `/AP` has no `/N`

`PdfDocument.Accessibility.cs:379` and `:384`:

```csharp
private static bool HasNormalAppearance(PdfDictionary annotation)
{
    return annotation.Get<PdfDictionary>((PdfName)"AP")?[PdfName.N].Resolve() is PdfStream or PdfDictionary;
}
```

`PdfDictionary.this[key]` (`PdfDictionary.cs:299`) is `_dictionary[key]` — it **throws** on a missing key
rather than returning null. An annotation carrying only a down or rollover appearance (`/AP << /D … >>`) is
perfectly legal input and hits this. `NormalAppearances` at `:384` has the identical shape.

Reproduced — strict save of a `Square` annotation whose `/AP` has only `/D`:

```
KeyNotFoundException :: The given key 'N' was not present in the dictionary.
```

The intended outcome is `PdfAccessibilityConformanceException: Visible Square annotations require a normal
appearance stream.` Instead the caller gets an exception type that is not part of the documented save contract
and that no caller can reasonably catch.

**Fix:** use the non-throwing accessor in both methods.

```csharp
annotation.Get<PdfDictionary>((PdfName)"AP")?.Get(PdfName.N)?.Resolve() is PdfStream or PdfDictionary
```

Worth a grep for the same pattern elsewhere in the new code — `structRoot[PdfName.ParentTree]` at
`PdfDocument.Accessibility.cs:278` has it too, though a serialized structure tree always has a `/ParentTree`,
so it is not reachable today.

### R2. `KeyNotFoundException` from `AddAnnot`/`BindAnnotation` when the annotation has no `/P`

`StructuralBuilder.cs:411`:

```csharp
var owner = annotation[PdfName.P].Resolve();
if (owner != null && !ReferenceEquals(owner, page.NativeObject)) { ... }
```

The `owner != null` guard shows the intent was a nullable read, but the throwing indexer means the null branch
is unreachable and a missing `/P` throws instead. `page.AddAnnotation` backfills `/P`, so the
`addWhenMissing: true` path is usually safe — but only when the annotation was *not* already in `/Annots`. An
annotation the caller appended to `/Annots` directly reaches `EnsureAnnotationOnPage` with `present == true`,
skips `AddAnnotation`, and throws.

Reproduced — annotation pushed onto `/Annots` by hand, then `AddAnnot`:

```
KeyNotFoundException :: The given key 'P' was not present in the dictionary.
```

This is the exact scenario finding 3 exists to support: binding an externally created annotation.

**Fix:** `var owner = annotation.Get(PdfName.P)?.Resolve();` — the rest of the method is already written for it.

### R3. Unselected choice fields render a value they do not have

`Annotations.cs:271`:

```csharp
[PdfName.N] = CreateTextAppearance(rect, appearance, selectedValue ?? values[0]).NativeObject.Indirect()
```

When `selectedValue` is null the field correctly gets no `/V`, but the appearance stream is generated for
`values[0]`.

Reproduced — `CreateChoiceWidget(..., ChoiceFieldKind.List, appearance, selectedValue: null, ...)`:

```
field has /V: False
appearance stream:
  ...
  BT
  1 0 0 1 3 5 Tm
  [(Alpha)] TJ
  ET
```

A sighted user sees "Alpha" selected; assistive technology reads the field value and finds nothing selected.
That divergence is the specific failure mode this whole workstream exists to prevent, and it is in the
generated output rather than in author-supplied content.

**Fix:** pass `selectedValue ?? string.Empty` — the empty appearance is the correct rendering of an unselected
choice field. Same call site is fine for combo fields.

### R4. `AddFormField(widget, title)` discards `title`; every radio button gets the group tooltip

`StructuralBuilder.cs:158` (and the identical `StructuralContext` copy at `:753`):

```csharp
ApplyAnnotationDescription(widget.NativeObject, widget.Field,
    widget.Field.Get<PdfString>((PdfName)"TU")?.Value ?? title);
```

The existing field `/TU` wins over the caller's `title`, so `title` is silently ignored for any widget produced
by a factory that already set a tooltip — which is all of them. The sibling API takes the opposite precedence:
`AddLabeledFormField` at `:182` uses `tooltip ?? title`, so there `title` wins.

This lands hardest on radio groups, where the tooltip is necessarily a property of the *group* and the label is
necessarily a property of the *button*. The library's own fixture demonstrates it —
`AccessibilityFixtureGenerator.cs:448-449`:

```csharp
var radioOne = checkboxField.Back().AddFormField(radio.Widgets[0], "Email contact");
var radioTwo = radioOne.Back().AddFormField(radio.Widgets[1], "Phone contact");
```

Reproduced — both widgets after binding:

```
widget TU=Choose one Contents=Choose one AS=A
widget TU=Choose one Contents=Choose one AS=Off
```

The fixture author clearly intended "Email contact" and "Phone contact". A screen reader announces the same
string for both buttons, which is a PDF/UA failure in a fixture that is meant to be the veraPDF-clean reference.

**Fix, two parts:**

1. Flip the precedence to `title ?? widget.Field.Get<PdfString>("TU")?.Value` so the two `Add*FormField`
   overloads agree, and only write `field[/TU]` when the description actually came from the caller — a
   per-widget title should not overwrite the shared field tooltip.
2. Add a per-option label to `RadioButtonOption` (`Annotations.cs:60`) and have `CreateRadioGroup` write it to
   each widget's `/TU` and `/Contents`, leaving the group tooltip on the field. Then update the fixture.

---

## Medium

### R5. `NeedAppearances` and AcroForm `/DA` are last-write-wins across fields

`Annotations.cs:322-324` runs on *every* field creation:

```csharp
acroForm[(PdfName)"DA"] = new PdfString(da);
if (appearance.NeedAppearances) acroForm[(PdfName)"NeedAppearances"] = PdfBoolean.True;
else acroForm.Remove((PdfName)"NeedAppearances");
```

Both are document-level settings being driven from a per-field options object. Reproduced — field 1 created
with `NeedAppearances = true, FontSize = 18`, then field 2 with the defaults at `FontSize = 6`:

```
after f1: NeedAppearances=True  DA=/F1 18 Tf 0 g
after f2: NeedAppearances=False DA=/F2 6 Tf 0 g
```

Creating a second field silently revoked an explicit opt-in from the first. The `/DA` overwrite is milder —
each field carries its own `/DA` — but the document default now describes the last field created, which is what
a viewer falls back to for any field added later by a user or another tool.

**Fix:** never clear `NeedAppearances` from a per-field call (set-only), and write the AcroForm `/DA` only when
absent. Better still, move both onto an explicit document-level API (`doc.ConfigureAcroForm(font, size,
needAppearances)`), leaving `FormFieldAppearanceOptions` to describe just the field. The existing assertion at
`AccessibilityAuthoringPhase2Tests.cs:128` passes either way, so it does not catch this.

### R6. `IDTree` `/Names` loses byte ordering once any ID is non-ASCII

`StructuralSerializer.cs:309` sorts the keys with `StringComparer.Ordinal`, then `:316` writes each through
`PdfString.CreateTextString`, which emits ASCII as single bytes and everything else as UTF-16BE with an
`FE FF` BOM (`StringSerializer.cs:136-142`). Ordinal ordering of .NET strings and byte ordering of the
serialized keys are no longer the same relation.

Reproduced — IDs `aÄ` and `zzz`:

```
key[0] value='aÄ' encoding=UTF16BE
key[2] value='zzz' encoding=PdfDocument
```

By serialized bytes `zzz` (`7A 7A 7A`) sorts before `aÄ` (`FE FF 00 61 00 C4`), so the array is out of order.
ISO 32000-1 §7.9.6 requires name-tree keys to be sorted by byte value; a consumer that binary-searches the tree
(rather than scanning it) will fail to resolve `/Ref` and `/Headers` targets.

This is a side effect of the finding-1 fix rather than a pre-existing bug — before the change every key was
single-byte, so ordinal ordering was close enough.

**Fix:** sort on the serialized bytes. The cheapest correct version is to project each key to its written form
once and sort the pairs on that:

```csharp
static byte[] KeyBytes(string id) => id.Any(c => c > 0x7F)
    ? new byte[] { 0xFE, 0xFF }.Concat(Encoding.BigEndianUnicode.GetBytes(id)).ToArray()
    : Encoding.Latin1.GetBytes(id);
```

then `OrderBy(x => KeyBytes(x.Key), ByteArrayComparer)`. A regression test with one ASCII and one non-ASCII ID
would have caught this — the existing Unicode round-trip test uses a single ID, so ordering is never exercised.

### R7. Only-`/O` suppression was applied to the Table attribute dictionary only

Finding 5's third task is "suppress emission of attribute dictionaries that end up carrying only `/O`". The
guard landed on the Table dict (`StructuralSerializer.cs:235`) but not on the Layout dict built in
`StructuralBuilder.cs:224` / `:819`, which adds `/O /Layout` unconditionally and then conditionally adds
`TextAlign`/`Width`/`Height`.

Reproduced — `AddParagraph("p").AddLayoutAttributes()` with all arguments null:

```
/A = O
```

A `/A << /O /Layout >>` with no properties is the same meaningless artifact the finding describes for tables.

**Fix:** in `AddLayoutAttributes`, only add the dictionary to `Attributes` when it gained at least one key
beyond `/O`. The same one-line check is worth applying wherever attribute dicts are assembled.

### R8. The annotation sweep is strict-only while reference validation warns

`ValidateAccessibilityAuthoringAfterSerialization` returns immediately when `StrictConformance != true`
(`PdfDocument.Accessibility.cs:266`), so the new `/Annots` sweep never runs outside strict mode. Meanwhile
finding 5's `ValidateStructureReferences` was deliberately moved *above* the strict check
(`:171-177`) so it warns in non-strict mode.

Reproduced — non-strict document, tagged paragraph, plus an untagged `Square` annotation:

```
has /StructParent: False
warnings: 0
```

The two Phase-2 findings therefore ship with opposite policies for the same class of author mistake, and the
non-strict case is the one most likely to be a remediation run over a real document.

**Fix:** pick one. Extracting the per-annotation checks into a routine that takes a
`throwOnViolation` flag and calling it in both modes (throw when strict, `Context.Warning` otherwise) matches
what finding 5 already does and costs very little.

### R9. The new Phase-2 widget test does not run in a normal checkout

`AccessibilityAuthoringPhase2Tests.cs:151-153`:

```csharp
var path = File.Exists("/workspace/test/Roboto-Regular.ttf")
    ? "/workspace/test/Roboto-Regular.ttf"
    : "../../../../test/Roboto-Regular.ttf";
```

Four `..` from `test/PdfLexer.Tests/bin/Debug/net10.0/` lands on `<repo>/test`, so the relative branch resolves
to `<repo>/test/test/Roboto-Regular.ttf` — one level too deep. Reproduced on this checkout:

```
System.IO.DirectoryNotFoundException :
  Could not find a part of the path '<repo>/test/test/Roboto-Regular.ttf'
```

`WidgetFactories_CreateAppearancesStatesAndSingleLogicalFields` is the only test covering the Phase-2 widget
factories, and it currently fails rather than runs. The pattern was copied from
`FontEmbeddingAccessibilityTests.cs:41-45`, which has the same bug; `Phase2AccessibilityTests.cs:23` too. Three
tests fail out of the box for this reason. The repo already has the right helper —
`PathUtil.GetPathFromSegmentOfCurrent("test")`, used correctly in `AccessibilityTextOutputTests.cs:38`.

Substituting it, the six `AccessibilityAuthoringPhase2Tests` pass, so this is only a harness bug — but it means
the Phase-2 field-type work currently has no *executing* coverage.

**Fix:** use `Path.Combine(PathUtil.GetPathFromSegmentOfCurrent("test"), "Roboto-Regular.ttf")` in the new test,
and fix the two pre-existing copies while there.

---

## Low

### R10. `print: false` widgets can never be saved in strict mode

`ValidateWidgetAnnotation` (`PdfDocument.Accessibility.cs:362-364`) requires the Print flag on every visible
widget, but every widget factory exposes a `print` parameter that defaults to `true`. Reproduced with
`print: false` plus `AddFormField`:

```
PdfAccessibilityConformanceException :: Widget annotations require the Print flag.
```

The validation itself is defensible; the problem is the same shape as recorded finding 12 (`AddNote`) — an API
surface that cannot be used in the default configuration, and the failure arrives at save rather than at the
call that caused it.

**Fix:** reject `print: false` at creation time in strict mode (as `ValidateLegacyWidgetFactory` already does
for the legacy overload) so the error points at the right line, or drop the parameter from the new factories.

### R11. Generated appearance streams omit `/Tx BMC … EMC` and ignore `/MK`

Reproduced — the `/AP /N` stream for a text widget:

```
1 1 1 rg  0 0 50 20 re  f*
0 0 0 RG  1 w  0.5 0.5 49 19 re  S
0 0 0 rg  /F1 10 Tf  10 TL
BT 1 0 0 1 3 5 Tm [(This\312value\312is\312far\312too\312long\312for\312the\312box)] TJ ET
```

Two things:

- ISO 32000-1 §12.7.3.3 says a variable-text appearance should be enclosed in `/Tx BMC … EMC`, and the whole
  stream should be bracketed by `q … Q`. Acrobat uses the marked-content wrapper when it regenerates the
  appearance; without it, regeneration behaviour is viewer-dependent.
- The white fill and 1 pt black border are hardcoded and `/MK` is never consulted, so every generated field
  looks the same and there is no way to author a borderless or coloured field.

Overflow is not itself a bug — the form XObject `/BBox` clips it — but an explicit `re W n` clip inside the
`BMC` block is the conventional form.

Also worth documenting rather than fixing: when the appearance font is a subsetted TrueType font, `/DR` carries
the subset, so a viewer regenerating the appearance after the user types has only the glyphs used at authoring
time.

### R12. `Replace` after a tagged `Append` pass fails with a misleading message

`McidAllocator.Prepare` resets `state.Next = 0` for `PageWriteMode.Replace`. That is right in isolation, but if
an earlier pass already bound structure elements to MCIDs on that page, those bindings survive in the structure
tree while their content does not. Reproduced:

```
p1 mcid=0 p2 mcid=0
save: PdfAccessibilityConformanceException ::
  Page content MCID 0 is registered to more than one structure element.
```

Failing loudly is the correct outcome and the backstop did its job — but the message describes a symptom in the
serializer, not the cause, which is three call sites earlier. Either have `Prepare` detect that MCIDs were
already allocated for the page and throw something like *"Replace discards content already bound to structure
elements on this page"*, or mention `PageWriteMode.Replace` in the serializer message.

### R13. Every `PageWriter` re-parses the page content stream

`McidAllocator.Prepare` (`McidAllocator.cs:28`) calls `FindHighestMcid(page.GetContentNodes())` unconditionally,
including when `state.Initialized` is already true — `Refresh` only uses the result to raise `state.Next`.
Measured on a page holding 200 marked-content sequences: 50 further writers cost 34 ms, ~0.7 ms per writer,
growing with page size.

Not alarming for typical authoring, and it does not throw on malformed content (verified — a garbage content
stream still allocates MCID 0 cleanly). But the re-parse buys nothing after the first call.

**Fix:** skip the scan when `state.Initialized` is true, since anything written since then went through the
allocator. `Replace` still resets unconditionally.

### R14. Smaller notes

Code read, not reproduced:

- **Dead branch.** `StructuralSerializer.cs:486`, `if (xObjectRef.XObject.Resolve() is PdfDictionary
  xObjectDict)`, can never match: `XObjForm.NativeObject` is a `PdfStream`, and `PdfStream` derives from
  `PdfObject`, not `PdfDictionary`. Harmless today because `BindFormXObject` sets `form.StructParents` eagerly
  (confirmed: a whole-bound form does get `/StructParents 0` in the saved file), but it reads as live code.
  Either delete it or make it `is PdfStream s` and write to `s.Dictionary`.
- **Role-mapped tags.** `ValidatePageAnnotations` compares `structureElement.Get<PdfName>(PdfName.S)` against
  the literal `Link`/`Form`/`Annot`, so a custom type that `MapRole`s onto one of those is rejected. Resolving
  through `RoleMap` before comparing would match the rest of the validator's behaviour.
- **Shared form bindings.** `BindFormXObject` dedupes per node, so two different structure elements binding the
  same `XObjForm` allocate two `StructParents` indexes while the form's `/StructParents` keeps only the second.
  The first ParentTree entry is orphaned, and the new duplicate detection does not catch it because the indexes
  differ. Pre-existing, and probably worth an explicit throw.

---

## Suggested sequencing

1. **R1, R2, R3** — one-line fixes each, all three are wrong output or wrong exception type. Add the
   corresponding cases to `AccessibilityAuthoringPhase2Tests`.
2. **R9** — until this is fixed the widget factories have no executing coverage, so it gates confidence in
   everything else in Phase 2.
3. **R4, R5** — the two form-authoring correctness defects; both need a small API decision (title precedence,
   where document-level AcroForm settings live), so worth doing together.
4. **R6, R7, R8** — spec and consistency cleanups.
5. **R10–R14** — opportunistic.

None of this changes the assessment in the source document that findings 3–5 are "implemented, validation
pending". R1–R6 should land before the pinned veraPDF run, since R3, R4 and R6 would show up in it and R1/R2
would abort it.

## One gap in the checklist itself

`accessibility_gaps_2.md` marks finding 5's third task — "Suppress emission of attribute dictionaries that end
up carrying only `/O`" — as complete. Per [R7](#r7-only-o-suppression-was-applied-to-the-table-attribute-dictionary-only)
it is complete for tables only. The other Phase 1 and Phase 2 checkboxes hold up against the code.
