# Test Coverage Phase 2

This document captures the next set of test additions that would materially improve confidence in `pdflexer` after the recent content-model and mutation test expansion.

It is intentionally narrower than a full coverage inventory. The goal is to prioritize scenarios that are:

- public and user-visible
- easy to regress silently
- under-specified by the current tests
- implied by the implementation, docs, or TODO notes

## Test Construction Guidance

Default test style for this phase:

- prefer creating a `PdfPage` and writing content through `ContentWriter<T>` or `FormWriter`
- prefer parsing that written content back through the public APIs before asserting behavior
- prefer writer-built fixtures over manually constructing `TextContent<T>`, `FormContent<T>`, `MarkedContentGroup<T>`, `GfxState<T>`, or clipping structures directly

Why:

- writer-built fixtures exercise more of the real library surface
- they avoid tests that accidentally depend on internal state shape instead of public behavior
- they produce more realistic graphics state, resource, and parsing conditions

Exceptions are still reasonable for narrow unit tests where direct construction is materially clearer or the writer path cannot express the setup cleanly, especially for:

- explicit failure-path tests
- unsupported-operation tests
- synthetic glyph or clipping edge cases
- tiny pure-helper tests where round-tripping through a page would add noise without adding confidence

## Priority 1: Finish the content-model gaps that are still not actually closed

The recent test additions improved coverage, but several items marked complete in `missing_tests.md` are still only partially covered or not covered directly.

### 1. `TextContent<T>` fidelity helpers [COMPLETED]

Why this still matters:

- text helpers feed geometry, mutation, and extraction
- current tests cover some happy paths, but not the full helper surface

Still needed:

- [x] direct tests for `GetGlyphBoundingBoxes()` on multi-glyph and multi-segment content
- [x] `TransformInitial()` behavior, not just `Transform()`
- [x] explicit `ClipExcept()` and `ClipFrom()` propagation assertions
- [x] partial-intersection cases where `CopyArea()` / `Split()` preserve spacing and clipping state, not just text value
- [ ] vertical-writing or rotated multi-segment cases if supported by the current model

Suggested tests:

- [x] `TextContent_GetGlyphBoundingBoxes_TracksAllGlyphs`
- [x] `TextContent_TransformInitial_UpdatesCharacterPositions`
- [x] `TextContent_ClipExcept_UpdatesAllSegments`
- [x] `TextContent_ClipFrom_AppendsClippingToAllSegments`
- [x] `TextContent_Split_PreservesSpacingAcrossRemovedGlyphs`

### 2. `FormContent<T>` public helper behavior [COMPLETED]

Why this still matters:

- forms are central to recursive mutation, writing, and flattening
- current additions only lock down part of the surface

Still needed:

- [x] `Parse()` using `ParentPage` resource fallback when the form itself lacks required resources
- [x] `Transform()` and `TransformInitial()` updating clipping transforms, not only CTM
- [x] direct coverage for form parse behavior when nested inside another form or page with inherited resources

Suggested tests:

- [x] `FormContent_Parse_UsesParentPageResources`
- [x] `FormContent_Transform_UpdatesClippingTransforms`
- [x] `FormContent_TransformInitial_UpdatesClippingTransforms`

### 3. `MarkedContentGroup<T>` container operations [COMPLETED]

Why this still matters:

- the container helpers recurse across mixed node types
- current tests mostly check existence, not exact shape

Still needed:

- [x] `Split()` assertions for both inside and outside groups
- [x] `CopyArea()` with mixed descendants where some intersect and some do not
- [x] `ClipFrom()` propagation checks, not only `ClipExcept()`
- [x] nested-group transform and clipping propagation

Suggested tests:

- [x] `MarkedContentGroup_Split_ReturnsInsideAndOutsideGroups`
- [x] `MarkedContentGroup_CopyArea_DropsNonIntersectingDescendants`
- [x] `MarkedContentGroup_ClipFrom_PropagatesToDescendants`
- [x] `MarkedContentGroup_Transform_PropagatesThroughNestedContainers`

### 4. Legacy/new content API parity [COMPLETED]

Why this still matters:

- `GetContentModel()` is now a compatibility bridge over `GetContentNodes()`
- the current parity tests are shallow

Still needed:

- [x] parity on mixed pages with marked content, forms, paths, and text together
- [x] parity for `flattenForms` on more than one form and with top-level siblings
- [x] a real test of `Apply(List<IContentGroup<T>>)` rather than casting to nodes first

Suggested tests:

- [x] `GetContentModel_And_GetContentNodes_AgreeOnMixedPageShape`
- [x] `GetContentModel_FlattenForms_AgreesOnMixedPageShape`
- [x] `CachedContentMutation_CompatApply_ListOfGroups_PreservesBehavior`

## Priority 2: Mutation, writing, and save semantics

These are the highest-risk behavioral areas outside the immediate content-model helpers.

### 5. `CachedContentMutation` cache-key and recursion edge cases [COMPLETED]

Why this matters:

- the implementation now depends on clipping structure, CTM identity, and nested form reuse
- subtle mistakes usually produce valid PDFs with incorrect content

Still needed:

- [x] multi-section clipping equivalence with more than one clipping segment
- [x] cache reuse boundaries where the same form is referenced under different parent clipping stacks
- [x] behavior when the mutation expands a single leaf into multiple leaves inside a form
- [x] mixed marked-content-plus-form rewrites that preserve container ordering

Suggested tests:

- [x] `CacheKey_Treats_MultiSectionTextClipping_AsEquivalent`
- [x] `Cache_DoesNotReuse_When_ParentClippingStacksDiffer`
- [x] `Mutation_CanExpandLeafInsideForm_AndPreserveOrder`
- [x] `Mutation_PreservesMarkedContentAndFormSiblingOrder`

### 6. Content writer overloads and round-tripping [COMPLETED]

Why this matters:

- `ContentWriter.Model.cs` is now part of the public migration path
- write/read mismatches are expensive for users to debug

Still needed:

- [x] writing a node list with forms and marked content in the same call and re-parsing the exact shape
- [x] writing nested marked content with top-level siblings
- [x] round-trip tests that compare content order and not just type presence
- [x] failure-path tests for invalid state transitions during writing

Suggested tests:

- [x] `ContentWriter_AddContent_NodeList_WithFormsAndMarkedContent_RoundTrips`
- [x] `ContentWriter_AddContent_PreservesOrderAcrossNestedAndTopLevelContent`
- [x] `ContentWriter_SetGS_ReconcilesClippingAndCTMWithoutCorruption`

### 7. Save-path tradeoffs and document rewrite behavior [COMPLETED]

Why this matters:

- save intentionally discards or rebuilds some structures
- those are major compatibility decisions

Still needed:

- [x] explicit `/StructTreeRoot` removal test when no in-memory structure is rebuilt
- [x] page-tree rebuild tests for insertions and deletions, not just reordering
- [x] object-stream/compressed-object rewrite boundaries where the save path falls back or changes behavior
- [x] copy/save behavior for encrypted streams and unsupported copy cases

Suggested tests:

- [x] `Save_Removes_StructTreeRoot_WhenNotRebuilt`
- [x] `Save_Rebuilds_PageTree_AfterInsertAndDelete`
- [x] `Save_Rewrites_ObjectStreamBackedInputs_Correctly`
- [x] `EncryptedStream_Copy_ThrowsExpectedNotSupportedException`

## Priority 3: Access API and context semantics

These areas affect almost every user of the DOM surface.

### 8. Wrapper read behavior versus raw/effective access [COMPLETED]

Why this matters:

- current wrapper properties materialize state during reads
- the access API consistency work depends on tests that state current behavior clearly

Still needed:

- [x] explicit coverage for `CropBox`, `BleedBox`, `TrimBox`, and `ArtBox`
- [x] tests that compare direct/raw object state before and after wrapper reads
- [x] inherited-value scenarios where the page dictionary does not contain the direct key initially

Suggested tests:

- [x] `PdfPage_CropAndBleedAccess_DocumentsMaterializationBehavior`
- [x] `PdfPage_DirectVsWrapperReads_ShowExpectedDifferences`
- [x] `PdfPage_InheritedBoxRead_MaterializesOrAvoidsMaterialization_AsDocumented`

### 9. Ambient `ParsingContext` semantics [COMPLETED]

Why this matters:

- `PdfDocument.Context` currently reflects ambient state, not document-owned state
- subtle cross-contamination bugs are likely in nested or concurrent usage

Still needed:

- [x] nested context usage across two documents in the same logical flow
- [x] concurrent access scenarios if the library expects the ambient model to be thread-safe
- [x] scanner and content-model operations run under different active contexts during the same test

Suggested tests:

- [x] `PdfDocument_Context_NestedScopes_DoNotCrossContaminateUnexpectedly`
- [x] `PdfDocument_Context_ConcurrentReads_RespectCurrentScope`
- [x] `ScannerAndContentModel_UseActiveParsingContextConsistently`

## Priority 4: Scanner, extraction, and resource-boundary behavior

These areas define whether the library’s read surfaces agree.

### 10. Scanner versus content-model agreement [COMPLETED]

Why this matters:

- the project exposes both scanners and content-model APIs as complementary entry points
- disagreement here creates user-visible confusion

Still needed:

- [x] agreement on ordering across mixed text, forms, and marked content
- [x] agreement on flattening behavior for nested forms
- [x] simple mutation-then-scan tests for paths and text together

Suggested tests:

- [x] `ScannerAndContentModel_AgreeOnMixedContentOrder`
- [x] `ScannerAndContentModel_AgreeOnMultiLevelFormTraversal`
- [x] `RewriteThenScan_PreservesExpectedVisibleOrder`

### 11. `RemoveUnusedResources()` and deduplication edge cases [COMPLETED]

Why this matters:

- resource cleanup is destructive when wrong
- deduplication can create subtle aliasing bugs

Still needed:

- [x] nested-form resources with fonts, extgstate, and xobjects together
- [x] equivalent dictionaries with different key orders and indirect-reference shapes
- [x] cases where a resource is referenced from content that is only reachable through a form

Suggested tests:

- [x] `RemoveUnusedResources_PreservesExtGStateAndFontUsedInNestedForm`
- [x] `DeduplicateResources_HandlesEquivalentDictionariesWithDifferentObjectLayout`
- [x] `DeduplicateResources_DoesNotMergeSemanticallyDifferentNestedForms`

## Priority 5: Failure paths and unsupported behavior

The project has many explicit `NotSupportedException` and TODO branches that would benefit from being pinned down.

### 12. Explicitly unsupported or partially supported workflows [COMPLETED]

Why this matters:

- unsupported behavior should fail consistently and predictably
- these are often the first places regressions appear after refactors

Still needed:

- [x] scanner transition-state failures in `PageContentScanner`
- [x] copy/read attempts against encrypted raw data paths
- [x] text-writing precondition failures such as writing text before setting a font
- [x] unsupported predictor/filter branches where failure mode matters

Suggested tests:

- [x] `PageContentScanner_GetCurrentData_ThrowsInUnsupportedFormFlatteningState`
- [x] `EncryptedRawCopy_ThrowsExpectedNotSupportedException`
- [x] `ContentWriter_TextWithoutFont_ThrowsExpectedMessage`
- [x] `FlatePredictor_UnsupportedValue_ThrowsNotSupportedException`

## Priority 6: Broader integration surfaces

These are lower priority than the items above, but still valuable because they cover distinctive library features.

### 13. Page-to-form and cross-document object movement [COMPLETED]

Why this matters:

- page-to-form conversion and cross-document reuse are core workflows
- resource inheritance and shared-object behavior are easy to misunderstand

Still needed:

- [x] repeated nested forms during uninherit-resource processing
- [x] already-present nested resources not being duplicated
- [x] cross-document page reuse with later mutation before save

Suggested tests:

- [x] `PageToForm_UninheritResources_DoesNotDuplicateAlreadyPresentResources`
- [x] `PageToForm_UninheritResources_PreservesNestedExtGState`
- [x] `CrossDocumentPageReuse_DivergesAsExpectedAfterMutationBeforeSave`

### 14. Tagged PDF and structural round-tripping [COMPLETED]

Why this matters:

- the repository has meaningful structural and tagged-PDF support
- save/rewrite behavior around structure is more fragile than basic page-content workflows

Still needed:

- [x] round-trip tests for structure-preserving saves
- [x] interaction between content rewriting and structure references
- [x] explicit tests for structure removal versus preservation during save

Suggested tests:

- [x] `TaggedPdf_Save_RoundTripsStructureWhenBuilderPresent`
- [x] `ContentRewrite_PreservesOrDropsStructure_AsDocumented`
- [x] `StructuralSerialization_Rewrite_PreservesExpectedNodeLinks`

## Recommended execution order

If phase two is implemented incrementally, the best order is:

1. finish the still-open content-model helper tests
2. add targeted save and mutation edge-case coverage
3. lock down wrapper-read and ambient-context semantics
4. strengthen scanner/resource-boundary integration tests
5. add unsupported/failure-path tests
6. expand page-to-form and tagged-PDF integration coverage

## Exit criteria for phase two

Phase two should be considered complete when:

- every item still implicitly open from `missing_tests.md` is either tested directly or moved out of scope explicitly
- save-path tradeoffs are documented by targeted tests, not only functional smoke tests
- wrapper-read and `ParsingContext` semantics are pinned down by tests
- scanner/content-model agreement is covered for representative mixed-content pages
- unsupported behavior that users can reasonably hit has explicit failure-path tests
