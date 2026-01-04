# Content Model Review Notes

- `ContentModelParser`: SCN pattern in forms overwrites shifted pattern; patterns in forms render with wrong transform.
- `ImageContent`, `ShadingContent`, `FormContent` (no BBox): `GetBoundingBox` ignores shear/rotation; must transform unit square by full CTM per spec.
- `TextContent.EnumerateCharacters`: returns positions from stale `GraphicsState` instead of evolving `gfx`; positions wrong after text moves/newlines.
- `TextContent.SplitInternal` (outside path): clipping uses `bbx1`/`bby1` only (1×1 box); outside text effectively clipped away.
- `CachedContentMutation`: form cache disabled via `&& false`; nested form mutations rebuilt every time.
