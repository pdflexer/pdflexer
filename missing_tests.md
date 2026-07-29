# Missing Test Coverage Review

This document captures notable test gaps in the current content-model and cached-mutation implementation.

It is based on a review of:

- current implementation under `src/PdfLexer/Content/Model/`, `src/PdfLexer/DOM/PdfPage.cs`, and `src/PdfLexer/Extensions.cs`
- current tests in `test/PdfLexer.Tests/`

This is not a line-by-line coverage report. It is a practical list of meaningful missing scenarios.

The first half focuses on the current content-model and mutation work.

The second half captures broader library gaps that are visible from the current implementation and test suite.

## Highest-Priority Missing Coverage

## 1. Legacy `CachedContentMutation` compatibility surface is mostly untested [COMPLETED]

Relevant implementation:

- [CachedContentMutation.cs](/workspace/src/PdfLexer/Content/Model/CachedContentMutation.cs)

Current tests focus on the new node-based path and page-level form recursion, but do not really lock down the legacy `IContentGroup<T>` compatibility APIs that were reintroduced.

Missing tests:

- [x] `Apply(List<IContentGroup<T>>)` preserves behavior for a simple list mutation
- [x] `CachedContentMutation(Func<IContentGroup<T>, IEnumerable<IContentGroup<T>>>)` still works for leaf edits
- [x] `CachedContentMutation(Func<IContentGroup<T>, IContentGroup<T>?>)` still works for removal
- [x] compatibility constructors behave correctly when the page contains marked-content containers and forms

Why this matters:

- this is a compatibility surface added specifically to avoid breaking existing users
- regressions here are likely to be source-compatible but behavior-breaking

Suggested tests:

- `CachedContentMutation_CompatApply_WorksForLeafMutation`
- `CachedContentMutation_CompatApply_CanRemoveLeaf`
- `CachedContentMutation_CompatApply_RecursesThroughMarkedContentAndForms`

## 2. Empty-container behavior after mutation is not locked down [COMPLETED]

Relevant implementation:

- [CachedContentMutation.cs:88](/workspace/src/PdfLexer/Content/Model/CachedContentMutation.cs#L88)

The helper currently rebuilds marked-content containers only if mutated descendants remain. There is no explicit test for what happens when mutation removes all descendants from:

- a single marked-content container
- nested marked-content containers
- a form that becomes empty

Missing tests:

- [x] removing the only child of a marked-content group removes the group from the result
- [x] removing all descendants from nested groups removes all now-empty ancestors
- [x] mutating all descendants of a form to `null` returns no form reference
- [x] repeated empty-form rewrites reuse the cached null result correctly

Why this matters:

- container dropping is part of the current behavior
- empty-result caching is a special branch that is currently untested

Suggested tests:

- `Mutation_Removes_EmptyMarkedContentContainers`
- `Mutation_Removes_TransitivelyEmptyMarkedContentAncestors`
- `Cache_Reuses_Null_Result_For_EmptyFormRewrite`

## 3. Cache edge cases are not fully covered [COMPLETED]

Relevant implementation:

- [CachedContentMutation.cs:137](/workspace/src/PdfLexer/Content/Model/CachedContentMutation.cs#L137)

Current tests cover:

- same form + same transform -> reuse
- same form + different transform -> no reuse
- same form + same transform + different clipping -> no reuse

Still missing:

- [x] identical text clipping objects created independently still reuse
- [x] path clipping and text clipping with same transform stay isolated
- [x] repeated null/empty rewrites are cached correctly
- [x] nested-form recursion with reused inner forms behaves consistently

Why this matters:

- the cache key now depends on clipping structure
- clipping-specific branches are easy to regress without direct tests

Suggested tests:

- `Cache_Reuses_Rewritten_Forms_For_StructurallyEqual_TextClipping`
- `Cache_DoesNotCrossReuse_Between_PathAndTextClipping`
- `Cache_Works_For_Reused_InnerForms_In_NestedForms`

## 4. Non-invertible form transform failure path is untested [COMPLETED]

Relevant implementation:

- [CachedContentMutation.cs:186](/workspace/src/PdfLexer/Content/Model/CachedContentMutation.cs#L186)

The form rewrite path throws if the effective form CTM cannot be inverted.

Missing test:

- [x] page/form content with a non-invertible CTM causes `PdfLexerException`

Why this matters:

- it is an explicit correctness guard
- currently untested exception paths tend to drift

Suggested test:

- `CachedContentMutation_Throws_For_NonInvertibleFormTransform`

## Important Missing Coverage

## 5. `MarkedContentGroup<T>` geometry and transform helpers are effectively untested [COMPLETED]

Relevant implementation:

- [MarkedContentGroup.cs](/workspace/src/PdfLexer/Content/Model/MarkedContentGroup.cs)

Current tests mostly verify parsing/writing shape and mutation descent. They do not lock down the newly added container operations.

Missing tests:

- [x] `GetBoundingBox()` unions multiple children correctly
- [x] `CopyArea()` preserves tag/state and copies only intersecting descendants
- [x] `Split()` returns inside/outside groups correctly
- [x] `Transform()` updates both group state and descendant state
- [x] `TransformInitial()` updates both group state and descendant state
- [x] `ClipExcept()` propagates to descendants
- [x] `ClipFrom()` propagates to descendants

Why this matters:

- this is newly added behavior on a semantically important container type
- these methods are easy to break because they recurse across mixed node types

Suggested tests:

- `MarkedContentGroup_BoundingBox_UnionsChildren`
- `MarkedContentGroup_CopyArea_PreservesContainerMetadata`
- `MarkedContentGroup_Split_ReturnsInsideAndOutsideGroups`
- `MarkedContentGroup_Transform_PropagatesToDescendants`

## 6. `Shift(...)` extensions are untested [COMPLETED]

Relevant implementation:

- [Extensions.cs](/workspace/src/PdfLexer/Extensions.cs)

The extension now has both `List<IContentGroup<T>>` and `List<IContentNode<T>>` overloads and relies on `Flatten()`.

Missing tests:

- [x] shifting a `List<IContentGroup<T>>` mutates descendant leaves
- [x] shifting a `List<IContentNode<T>>` mutates descendant leaves
- [x] shifting nested marked-content content works
- [x] shifting content with forms only shifts the form placement, not parsed descendants

Why this matters:

- this is user-facing convenience API
- the overload split was changed recently and is not covered

Suggested tests:

- `Shift_GroupList_ShiftsDescendantLeaves`
- `Shift_NodeList_ShiftsDescendantLeaves`
- `Shift_Preserves_FormBoundaryBehavior`

## 7. `Flatten()` behavior is only lightly covered [COMPLETED]

Relevant implementation:

- [ContentExtensions.cs](/workspace/src/PdfLexer/Content/Model/ContentExtensions.cs)

Current coverage checks flattening through marked content. It does not lock down the generalized `IContentContainer<T>` behavior or form boundary behavior.

Missing tests:

- [x] flatten recurses through nested marked-content containers of multiple levels
- [x] flatten treats forms as leaf/reference nodes rather than expanding them
- [x] flatten preserves order across mixed containers and leaves

Why this matters:

- flatten semantics affect traversal helpers and convenience APIs

Suggested tests:

- `Flatten_PreservesOrder_AcrossNestedContainers`
- `Flatten_DoesNotExpand_Forms`

## 8. `GetContentNodes()` vs `GetContentModel()` parity is not tested [COMPLETED]

Relevant implementation:

- [PdfPage.cs:161](/workspace/src/PdfLexer/DOM/PdfPage.cs#L161)

The current compatibility implementation routes `GetContentModel()` through `GetContentNodes()` and casts back to `IContentGroup<T>`.

Missing tests:

- [x] both methods return equivalent tree shape for ordinary parsed content
- [x] `GetContentModel()` still works for marked-content/group-heavy pages
- [x] `flattenForms` behavior is consistent between the two APIs

Why this matters:

- this is the compatibility bridge between the old and new public surfaces

Suggested tests:

- `GetContentModel_And_GetContentNodes_ReturnEquivalentTrees`
- `GetContentModel_RespectsFlattenFormsCompatibility`

## Medium-Priority Missing Coverage

## 9. `TextContent<T>` still lacks coverage for several fidelity-sensitive helpers [COMPLETED]

Relevant implementation:

- [TextContent.cs](/workspace/src/PdfLexer/Content/Model/TextContent.cs)

Current tests cover:

- `Create(...)`
- round-trip write/read
- `EnumerateCharacters()` for segmented, newline, and rotated text

Still missing:

- [x] `GetGlyphBoundingBoxes()`
- [x] `GetBoundingBox()` for multi-segment text
- [x] `Transform()` and `TransformInitial()` behavior
- [x] clipping propagation through `ClipExcept()` / `ClipFrom()`
- [x] `CopyArea()` / `Split()` on text
- [x] multi-char glyph enumeration behavior

Why this matters:

- text is the most user-visible content type
- many of these helpers feed mutation and geometry workflows

Suggested tests:

- `TextContent_GetGlyphBoundingBoxes_TracksAllGlyphs`
- `TextContent_Transform_UpdatesCharacterPositions`
- `TextContent_CopyArea_And_Split_WorkForPartialIntersection`
- `TextContent_EnumerateCharacters_ExpandsMultiCharGlyphs`

## 10. `FormContent<T>` helper behavior is only partially covered [COMPLETED]

Relevant implementation:

- [FormContent.cs](/workspace/src/PdfLexer/Content/Model/FormContent.cs)

Current tests cover unsupported geometry operations.

Still missing:

- [x] `Parse()` with `ParentPage` resource fallback
- [x] `GetBoundingBox()` when `/BBox` exists
- [x] `GetBoundingBox()` fallback path when `/BBox` is missing
- [x] `Transform()` and `TransformInitial()` updating clipping transforms

Why this matters:

- forms are central to recursive mutation behavior
- these are direct public methods on a key type

Suggested tests:

- `FormContent_Parse_UsesParentPageResources`
- `FormContent_GetBoundingBox_UsesBBoxAndPlacement`
- `FormContent_GetBoundingBox_FallsBackWithoutBBox`

## 11. Writer overload coverage is incomplete for node-based APIs [COMPLETED]

Relevant implementation:

- [ContentWriter.Model.cs](/workspace/src/PdfLexer/Writing/ContentWriter.Model.cs)

Current tests cover a mixed-node write case and a non-writable-node failure.

Missing tests:

- [x] `AddContent(IContentNode<T>)`
- [x] `AddContent(List<IContentNode<T>>)`
- [x] writing nested marked-content plus top-level siblings in one call
- [x] writing a node list that includes forms and marked content together


Why this matters:

- these overloads are part of the new node-based public shape

Suggested tests:

- `ContentWriter_AddContent_SingleNode_WritesCorrectly`
- `ContentWriter_AddContent_NodeList_WritesMixedTreeCorrectly`

## Lower-Priority but Valuable Coverage

## 12. `ClippingEqual`/clipping-hash equivalence paths are only indirectly tested [COMPLETED]

Relevant implementation:

- [CachedContentMutation.cs:264](/workspace/src/PdfLexer/Content/Model/CachedContentMutation.cs#L264)

Current tests prove some reuse/non-reuse scenarios, but not the deeper equivalence logic for:

- [x] glyph-based clipping
- [x] structurally equal but separately allocated clipping objects
- [x] multiple clipping sections in sequence

Suggested tests:

- `CacheKey_Treats_StructurallyEqual_MultiSectionClipping_AsEquivalent`

## 13. Round-trip compatibility of newer node/container APIs is not broadly covered [COMPLETED]

Relevant implementation:

- node/container split across `IContentNode<T>`, `IContentItem<T>`, and `IContentContainer<T>`

Missing tests:

- [x] parse -> `GetContentNodes()` -> write -> parse for mixed pages containing:
  - text
  - path
  - form
  - marked content
- [x] generic traversal helpers over these mixed pages

Suggested tests:

- `ContentNodes_RoundTrip_MixedPage_PreservesShape`

## Recommended Next Additions

If only a small number of tests are added next, the best value is likely:

1. Legacy `CachedContentMutation` compatibility tests
2. Empty-container and empty-form mutation tests
3. `MarkedContentGroup<T>` geometry/transform tests
4. `GetContentNodes()` / `GetContentModel()` parity tests
5. `Shift(...)` extension tests

Those five areas would close the biggest gaps created by the recent content-model and mutation changes.

## Broader Library Coverage Gaps

## 14. `PdfDocument.Context` semantics are not directly locked down [COMPLETED]

Relevant implementation:

- [PdfDocument.cs:19](/workspace/src/PdfLexer/PdfDocument.cs#L19)
- [PdfDocument.Context.cs](/workspace/src/PdfLexer/PdfDocument.Context.cs)

There are many tests that use `doc.Context`, but I did not find direct tests proving the current ambient-context semantics or protecting against accidental changes.

Missing tests:

- [x] two documents opened under different parsing contexts behave as expected
- [x] `PdfDocument.Context` reflects `ParsingContext.Current` rather than document-owned state
- [x] concurrent or nested parsing-context usage does not cross-contaminate document operations in obvious scenarios

Why this matters:

- this is a known API-design sharp edge
- behavior here is subtle and easy to regress silently

Suggested tests:

- `PdfDocument_Context_TracksAmbientParsingContext`
- `PdfDocument_Context_DoesNotImplyDocumentOwnedState`

## 15. Read-only wrapper access is not tested for hidden mutation [COMPLETED]

Relevant implementation:

- [PdfPage.cs](/workspace/src/PdfLexer/DOM/PdfPage.cs)

`PdfPage.Resources`, `MediaBox`, and several other accessors materialize defaults through `GetOrCreateValue(...)` or similar patterns.

I did not find targeted tests proving which reads mutate the DOM and which should not.

Missing tests:

- [x] reading `Resources` on a page without resources mutates or does not mutate exactly as intended
- [x] reading `Rotate`, `CropBox`, `BleedBox`, `TrimBox`, and `ArtBox` has explicitly verified behavior
- [x] wrapper inspection on a page/dictionary can be compared before and after reads

Why this matters:

- read-on-access mutation is one of the bigger DX and correctness risks in the broader library
- this should be intentional and tested, even before it is redesigned

Suggested tests:

- `PdfPage_ReadAccess_DocumentsCurrentMaterializationBehavior`
- `PdfPage_Inspection_DoesNotUnexpectedlyDirtyPage` or, if current behavior is intentional, a test that locks the current mutation behavior down until it is redesigned

## 16. Save-path tradeoff behavior is mostly indirect, not targeted [COMPLETED]

Relevant implementation:

- [PdfDocument.Saving.cs](/workspace/src/PdfLexer/PdfDocument.Saving.cs)

There are many functional save/rewrite tests, but I did not find focused tests for the explicit tradeoffs in the save path.

Missing tests:

- [x] `/Names` removal on save is explicitly verified
- [x] `/StructTreeRoot` removal on save when no in-memory structure is supplied is explicitly verified
- [x] `/Encrypt` removal on save is explicitly verified
- [x] page-tree rebuild behavior is explicitly verified for reordered or newly added pages

Why this matters:

- these are major documented library behaviors
- current tests may pass without telling users what is intentionally lost

Suggested tests:

- `Save_Removes_Names_WhenNotRebuilt`
- `Save_Removes_Encrypt_FromOutput`
- `Save_Rebuilds_PageTree_For_CurrentPagesList`

## 17. Outline/document-level read-model behavior is lightly covered [COMPLETED]

Relevant implementation:

- [PdfDocument.cs](/workspace/src/PdfLexer/PdfDocument.cs)

There are outline and structural tests, but I did not see strong coverage for the lazy read/cache behavior of:

- `Outlines`
- `Structure`

Missing tests:

- [x] `Outlines` parses once and caches
- [x] setting `Outlines` overrides lazy parse behavior
- [x] `Structure` lazy-creates a builder consistently

Why this matters:

- these are public document-level convenience surfaces
- lazy initialization/caching bugs can be subtle and user-visible

Suggested tests:

- `PdfDocument_Outlines_IsLazyAndCached`
- `PdfDocument_Structure_LazyCreatesBuilder`

## 18. Resource-deduplication behavior appears under-tested at edge cases [COMPLETED]

Relevant implementation:

- `DeduplicateResources()` and `Content/Deduplication.cs`

There are some functional resource-count checks, but I did not find narrow tests for correctness in tricky cases such as:

- [x] shared forms with nested resources
- [x] pages that reference equivalent but differently ordered resource dictionaries
- [x] deduplication preserving output semantics across nested XObjects

Why this matters:

- resource deduplication can silently corrupt output if identity/hash assumptions are wrong

Suggested tests:

- `DeduplicateResources_PreservesNestedFormSemantics`
- `DeduplicateResources_HandlesEquivalentResourceDictionaries`

## 19. Page-to-form conversion coverage is focused on cropbox/rotation, not resource fallback edge cases [COMPLETED]

Relevant implementation:

- `XObjForm.FromPage(...)`
- `PageToFormTests.cs`

Current tests cover several geometry cases well.

Still missing:

- [x] inherited page resources copied into child forms when `uninheritResources` is enabled
- [x] repeated nested forms do not get duplicated incorrectly during resource uninheritance
- [x] forms with extgstate/font/xobject inheritance edge cases

Why this matters:

- form conversion is one of the library’s more distinctive workflows
- resource propagation bugs can be hard to spot visually

Suggested tests:

- `PageToForm_UninheritResources_CopiesInheritedResourcesIntoNestedForms`
- `PageToForm_UninheritResources_DoesNotDuplicateAlreadyPresentResources`

## 20. `RemoveUnusedResources()` lacks direct behavioral tests [COMPLETED]

Relevant implementation:

- `PdfPage.RemoveUnusedResources()`
- `Content/ResourceCleaner.cs`

I did not find a focused unit/integration test for the public wrapper API itself.

Missing tests:

- [x] unused fonts are removed
- [x] unused XObjects are removed
- [x] resources referenced from nested forms are preserved

Why this matters:

- resource cleanup is public and potentially destructive

Suggested tests:

- `RemoveUnusedResources_RemovesUnusedTopLevelFontAndXObject`
- `RemoveUnusedResources_PreservesResourcesNeededByNestedForms`

## 21. Encrypted open/save scenarios could use more targeted assertions [COMPLETED]

Relevant implementation:

- `EncryptionTests.cs`
- [PdfDocument.Saving.cs:71](/workspace/src/PdfLexer/PdfDocument.Saving.cs#L71)

There is existing encrypted-document coverage, but the explicit save semantics still seem lightly targeted.

Missing tests:

- [x] saving an encrypted input produces unencrypted output with no `/Encrypt`
- [x] output remains readable after decrypt-on-save behavior
- [x] forcing serialization on encrypted docs behaves as expected

Why this matters:

- encryption behavior is a significant user-facing compatibility decision

Suggested tests:

- `EncryptedInput_Save_RemovesEncryptAndProducesReadableOutput`

## 22. Wrapper/document cloning and shared-object behavior is only partially covered [COMPLETED]

Relevant implementation:

- widespread use of `CloneShallow()`

There are functional tests and utility usage, but not many explicit tests documenting what edits remain shared versus isolated.

Missing tests:

- [x] shallow-cloned page dictionaries diverge where expected after mutation
- [x] copied pages shared across docs behave as expected until save
- [x] resource dictionary cloning boundaries are explicitly verified

Why this matters:

- shared-object behavior is one of the most important mental-model issues in the library

Suggested tests:

- `CloneShallow_PageMutation_DoesNotBackPropagateUnexpectedly`
- `CrossDocumentPageReuse_RemainValidUntilSaveBoundary`

## 23. Scanner/content access boundary behavior is not strongly documented by tests [COMPLETED]

Relevant implementation:

- scanners and COM share some underlying resource/context semantics

There are many scanner tests and many COM tests, but fewer targeted tests that pin down expected agreement on:

- [x] text results between scanners and COM-backed rewriting for simple cases
- [x] form/resource traversal consistency
- [x] content-order expectations

Why this matters:

- the library now explicitly positions scanners and COM as complementary surfaces
- their overlap should be tested where results are expected to agree

Suggested tests:

- `ScannerAndContentModel_AgreeOnSimplePageTextAfterRewrite`
- `ScannerAndContentModel_AgreeOnNestedFormTraversalForSimpleCases`

## Recommended Broader Additions

If the test plan expands beyond the recent content-model work, the best broader additions are likely:

1. `PdfDocument.Context` behavior tests
2. explicit save-tradeoff tests for `/Names`, `/StructTreeRoot`, and `/Encrypt`
3. read-access materialization tests on `PdfPage`
4. `RemoveUnusedResources()` behavior tests
5. page-to-form resource propagation tests

Those would close several user-visible gaps that are currently described in docs/roadmaps but not strongly enforced by targeted tests.
